using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace LycheeBB
{
    class Config
    {
        public static int chunkSize = 65536;
    }

    public enum Mode
    {
        FullBuild = 0,
        IncrementalBuild = 1,
    }

    public class BuildArguments
    {
        public string location;
        public Mode mode;
        public string version;
        public string parentVersion;
        public string prefix;
        public BuildTarget buildTarget;
        public float bundleSizeHint;
        public bool compressAssetBundles;
        public bool redundant;
        public string[] scenesInBuild;
    }

    public interface BuildResult
    {
        string[] GetAllBundles();
        string[] GetSceneBundles();

        string GetBundleHash(string bundleName);
        string[] GetBundleDependencies(string bundleName, bool recursive);
        string[] GetBundleAssets(string bundleName);

        string[] GetAllCustomFiles();
        string GetCustomFileHash(string fileName);
    }

    public interface CustomScript
    {
        void Awake();

        bool HasUI();
        void DrawUI();

        bool PreBuildEvent(BuildArguments args);
        void PostBuildEvent(BuildResult result);

        bool AddGroup(out int groupId, out string[] assets);

        bool AddCustomFiles(out string[] files);
    }

    [Serializable]
    class Pair<T1, T2>
    {
        public T1 first;
        public T2 second;

        public Pair(T1 a, T2 b)
        {
            first = a;
            second = b;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int a = first.GetHashCode();
                int b = second.GetHashCode();
                return (a + b) * (a + b + 1) / 2 + b;
            }
        }

        public override bool Equals(object other)
        {
            var o = other as Pair<T1, T2>;

            if (o == null) {
                return false;
            }

            return first.Equals(o.first) && second.Equals(o.second);
        }
    }

    class Utility
    {
        public static Pair<T1, T2> MakePair<T1, T2>(T1 a, T2 b)
        {
            return new Pair<T1, T2>(a, b);
        }

        public static T[] SubArray<T>(T[] array, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, index, result, 0, length);
            return result;
        }

        public static string ToHexString(byte[] buffer)
        {
            StringBuilder s = new StringBuilder(buffer.Length * 2);

            foreach (byte b in buffer)
            {
                s.Append(b.ToString("x2"));
            }

            return s.ToString();
        }

        public static byte[] FromHexString(string str)
        {
            byte[] buffer = new byte[str.Length / 2];

            for (int i = 0; i < str.Length; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }

            return buffer;
        }

        public static byte[] ToBigEndianArray<T>(T a)
        {
            byte[] buf = typeof(BitConverter)
                .GetMethod("GetBytes", new Type[]{typeof(T)})
                .Invoke(null, new object[]{a}) as byte[];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buf);
            }

            return buf;
        }

        public static void WriteToFile(FileStream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    class RollingChecksum
    {
        public void Update(byte[] buffer, int length)
        {
            ushort a = 0;
            ushort b = 0;

            for (int i = 0; i < length; ++i)
            {
                byte ch = buffer[i];
                a = (ushort)(a + ch);
                b = (ushort)(b + (length - i) * ch);
            }

            value_ = ((uint)b << 16) | a;
            length_ = length;
        }

        public void Roll(byte oldByte, byte newByte)
        {
            ushort a = (ushort)(value_ & 0xffff);
            ushort b = (ushort)(value_ >> 16);

            a = (ushort)(a - oldByte + newByte);
            b = (ushort)(b - length_ * oldByte + a);

            value_ = ((uint)b << 16) | a;
        }

        public uint Digest()
        {
            return value_;
        }

        private uint value_ = 0;
        private int length_ = 0;
    }

    [Serializable]
    class BuildDatabase
    {
        public string prefix
        {
            get { return prefix_; }
            set { prefix_ = value; }
        }

        public int currentBundleId
        {
            get { return currentBundleId_; }
            set { currentBundleId_ = value; }
        }

        public void AddSceneAsset(string path)
        {
            sceneAssetSet_.Add(AssetDatabase.AssetPathToGUID(path), 0);
        }

        public bool HasSceneAsset(string path)
        {
            return sceneAssetSet_.ContainsKey(AssetDatabase.AssetPathToGUID(path));
        }

        public void AddGroup(int groupId)
        {
            groupIdSet_.Add(groupId, 0);
        }

        public bool HasGroup(int groupId)
        {
            return groupIdSet_.ContainsKey(groupId);
        }

        public void AddAsset(string path, int bundleId)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            string hash = AssetDatabase.GetAssetDependencyHash(path).ToString();

            assetGuidToHash_.Add(guid, hash);
            assetGuidToBundleId_.Add(guid, bundleId);
        }

        public bool CompareAssetHash(string path)
        {
            return CompareAssetHashByGuid(AssetDatabase.AssetPathToGUID(path));
        }

        public bool CompareAssetHashByGuid(string guid)
        {
            string oldHash;

            if (assetGuidToHash_.TryGetValue(guid, out oldHash))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string hash = AssetDatabase.GetAssetDependencyHash(path).ToString();
                return (hash == oldHash);
            }

            return false;
        }

        public int GetAssetBundleId(string path)
        {
            return GetAssetBundleIdByGuid(AssetDatabase.AssetPathToGUID(path));
        }

        public int GetAssetBundleIdByGuid(string guid)
        {
            int bundleId;

            if (assetGuidToBundleId_.TryGetValue(guid, out bundleId)) {
                return bundleId;
            }

            return 0;
        }

        public void SetLeafBundle(int bundleId)
        {
            leafBundleIdSet_.Add(bundleId, 0);
        }

        public bool IsLeafBundle(int bundleId)
        {
            return leafBundleIdSet_.ContainsKey(bundleId);
        }

        public void AddBundle(string bundle, byte[] hash)
        {
            bundleNameToHash_.Add(bundle, hash);
        }

        public Dictionary<string, byte[]> GetBundles()
        {
            return bundleNameToHash_;
        }

        private string prefix_ = "";
        private int currentBundleId_ = 1;

        private Dictionary<string, int> sceneAssetSet_ = new Dictionary<string, int>();
        private Dictionary<int, int> groupIdSet_ = new Dictionary<int, int>();

        private Dictionary<string, string> assetGuidToHash_ = new Dictionary<string, string>();
        private Dictionary<string, int> assetGuidToBundleId_ = new Dictionary<string, int>();
        private Dictionary<int, int> leafBundleIdSet_ = new Dictionary<int, int>();

        private Dictionary<string, byte[]> bundleNameToHash_ = new Dictionary<string, byte[]>();
    }

    [Serializable]
    class BuildStack
    {
        public int Count
        {
            get { return list_.Count; }
        }

        public BuildDatabase this[int index]
        {
            get { return list_[index]; }
            set { list_[index] = value; }
        }

        public BuildDatabase First
        {
            get { return list_.First(); }
        }

        public BuildDatabase Last
        {
            get { return list_.Last(); }
        }

        public void Add(BuildDatabase bd)
        {
            list_.Add(bd);
        }

        static public void SaveToFile(string filename, BuildStack bs)
        {
            FileStream stream = new FileStream(filename, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, bs);

            stream.Close();
        }

        static public void LoadFromFile(string filename, out BuildStack bs)
        {
            FileStream stream = new FileStream(filename, FileMode.Open);

            BinaryFormatter formatter = new BinaryFormatter();

            bs = formatter.Deserialize(stream) as BuildStack;

            stream.Close();
        }

        private List<BuildDatabase> list_ = new List<BuildDatabase>();
    }

    class BuildResultData : BuildResult
    {
        public BuildResultData()
        {
            bundleNameList_ = new List<string>();
            sceneBundleNameSet_ = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bundleNameToHash_ = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bundleNameToDependencies_ = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            bundleNameToAssets_ = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            customFileNameList_ = new List<string>();
            customFileNameToHash_ = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string[] GetAllBundles()
        {
            return bundleNameList_.ToArray();
        }

        public string[] GetSceneBundles()
        {
            return bundleNameList_.Where(x => sceneBundleNameSet_.Contains(x)).ToArray();
        }

        public string GetBundleHash(string bundleName)
        {
            string hash;
            bundleNameToHash_.TryGetValue(bundleName, out hash);
            return hash;
        }

        public string[] GetBundleDependencies(string bundleName, bool recursive)
        {
            if (recursive)
            {
                List<string> list = new List<string>();
                HashSet<string> set = new HashSet<string>();

                list.Add(bundleName);
                set.Add(bundleName);

                for (int i = 0; i < list.Count; ++i)
                {
                    foreach (string p in GetBundleDependencies(list[i], false))
                    {
                        if (set.Add(p)) {
                            list.Add(p);
                        }
                    }
                }

                list.RemoveAt(0);

                return list.ToArray();
            }
            else
            {
                string[] dependencies;
                bundleNameToDependencies_.TryGetValue(bundleName, out dependencies);
                return dependencies;
            }
        }

        public string[] GetBundleAssets(string bundleName)
        {
            string[] assets;
            bundleNameToAssets_.TryGetValue(bundleName, out assets);
            return assets;
        }

        public string[] GetAllCustomFiles()
        {
            return customFileNameList_.ToArray();
        }

        public string GetCustomFileHash(string fileName)
        {
            string hash;
            customFileNameToHash_.TryGetValue(fileName, out hash);
            return hash;
        }

        public void AddBundle(string bundleName, bool isSceneBundle = false)
        {
            bundleNameList_.Add(bundleName);

            if (isSceneBundle) {
                sceneBundleNameSet_.Add(bundleName);
            }
        }

        public void SetBundleHash(string bundleName, string hash)
        {
            bundleNameToHash_.Add(bundleName, hash);
        }

        public void SetBundleDependencies(string bundleName, string[] dependencies)
        {
            bundleNameToDependencies_.Add(bundleName, dependencies);
        }

        public void SetBundleAssets(string bundleName, string[] assets)
        {
            bundleNameToAssets_.Add(bundleName, assets);
        }

        public void AddCustomFile(string fileName)
        {
            customFileNameList_.Add(fileName);
        }

        public void SetCustomFileHash(string fileName, string hash)
        {
            customFileNameToHash_.Add(fileName, hash);
        }

        private List<string> bundleNameList_;
        private HashSet<string> sceneBundleNameSet_;
        private Dictionary<string, string> bundleNameToHash_;
        private Dictionary<string, string[]> bundleNameToDependencies_;
        private Dictionary<string, string[]> bundleNameToAssets_;

        private List<string> customFileNameList_;
        private Dictionary<string, string> customFileNameToHash_;
    }

    class Builder
    {
        public string location
        {
            get; set;
        }

        public Mode mode
        {
            get; set;
        }

        public string version
        {
            get; set;
        }

        public string parentVersion
        {
            get; set;
        }

        public string prefix
        {
            get; set;
        }

        public BuildTarget buildTarget
        {
            get
            {
                return buildTarget_;
            }

            set
            {
                buildTarget_ = value;
                texPlatform_ = GetTexPlatformString(value);
            }
        }

        public float bundleSizeHint
        {
            get
            {
                return bundleSizeHint_ / 1024.0f / 1024.0f;
            }

            set
            {
                bundleSizeHint_ = (long)(value * 1024.0f * 1024.0f);
            }
        }

        public bool compressAssetBundles
        {
            get; set;
        }

        public bool redundant
        {
            get; set;
        }

        public string[] scenesInBuild
        {
            get; set;
        }

        public CustomScript customScript
        {
            get; set;
        }

        public bool compareWithParentVersion
        {
            get; set;
        }

        public void Build()
        {
            BuildArguments args = null;

            if (customScript != null)
            {
                args = new BuildArguments();
                args.location = location;
                args.version = version;
                args.parentVersion = parentVersion;
                args.mode = mode;
                args.prefix = prefix;
                args.buildTarget = buildTarget;
                args.bundleSizeHint = bundleSizeHint;
                args.compressAssetBundles = compressAssetBundles;
                args.scenesInBuild = scenesInBuild;

                if (!customScript.PreBuildEvent(args)) {
                    return;
                }

                location = args.location;
                version = args.version;
                parentVersion = args.parentVersion;
                mode = args.mode;
                prefix = args.prefix;
                buildTarget = args.buildTarget;
                bundleSizeHint = args.bundleSizeHint;
                compressAssetBundles = args.compressAssetBundles;
                scenesInBuild = args.scenesInBuild;
            }

            if (string.IsNullOrEmpty(location))
            {
                Debug.LogError("Location cannot be left empty");
                return;
            }
            else if (Directory.Exists(location) == false)
            {
                Debug.LogError("Location does not exist");
                return;
            }

            if (string.IsNullOrEmpty(version))
            {
                Debug.LogError("Version cannot be left empty");
                return;
            }
            else if (Directory.Exists(GetVersionPath()))
            {
                if (EditorUtility.DisplayDialog("", "The specified version already exists, do you want to overwrite it ?", "Overwrite", "Cancel"))
                {
                    ClearDirectory(GetVersionPath());
                }
                else
                {
                    return;
                }
            }
            else
            {
                DirectoryInfo di = Directory.CreateDirectory(GetVersionPath());

                if (!di.Exists)
                {
                    Debug.LogError("Failed to create version directory");
                    return;
                }
            }

            if (mode == Mode.FullBuild)
            {
                bs_ = new BuildStack();
                bs_.Add(new BuildDatabase());

                RestrictPrefix();
            }
            else
            {
                if (string.IsNullOrEmpty(parentVersion))
                {
                    Debug.LogError("Parent version cannot be left empty in incremental mode");
                    return;
                }

                if (version == parentVersion)
                {
                    Debug.LogError("Version cannot be the same as the parent version");
                    return;
                }

                BuildStack.LoadFromFile(Path.Combine(GetParentVersionPath(), ".LycheeBundleBuilder"), out bs_);

                if (!CopyBundlesFromParentVersion()) {
                    return;
                }

                prefix = bs_.First.prefix;

                if (redundant)
                {
                    bs_.Add(new BuildDatabase());
                }
            }

            if (customScript != null)
            {
                CollectGroupAssets();
            }

            RemovesDuplicateScenes();

            if (!PrebuildAssetBundles())
            {
                return;
            }

            var scenesInBuildBackup = scenesInBuild;

            BuildResultData buildResult = new BuildResultData();

            for (int i = 0; i < bs_.Count; ++i)
            {
                currentBdIdx_ = i;

                if (i < bs_.Count - 1)
                {
                    Debug.Assert(mode == Mode.IncrementalBuild);

                    scenesInBuild = scenesInBuildBackup;

                    SelectScenesInCurrentBd();
                }
                else
                {
                    scenesInBuild = scenesInBuildBackup;

                    UnselectScenesBeforeCurrentBd();
                }

                InitializeBuildIntermediateVars();

                AddAllScenes();

                MergeSceneDependentBundles();
                MergeScenePrivateBundles();

                if (i == 0)
                {
                    AddAllGroups();

                    MergeGroupBundles();
                }

                if (!BuildAssetBundles(buildResult))
                {
                    return;
                }
            }

            BuildStack.SaveToFile(Path.Combine(GetVersionPath(), ".LycheeBundleBuilder"), bs_);

            AddAllCustomFiles(buildResult);

            WriteListFile(buildResult);
            WriteBuildFile(buildResult);

            if (customScript != null) {
                customScript.PostBuildEvent(buildResult);
            }

            if (mode == Mode.IncrementalBuild && compareWithParentVersion)
            {
                LycheeVS.Comparer c = new LycheeVS.Comparer(GetParentVersionPath(), GetVersionPath());
                LycheeVS.CompareResult result = c.Compare();
                Debug.Log(result.ToString());
            }
        }

        private void AddAllCustomFiles(BuildResultData buildResult)
        {
            if (customScript == null) {
                return;
            }

            try
            {
                int i = 1;
                int guessMax = 1;

                string[] files;

                while (customScript.AddCustomFiles(out files))
                {
                    foreach (string file in files)
                    {
                        string destFileName = Path.GetFileName(file).ToLower();
                        string destFileFullPath = Path.Combine(GetVersionPath(), destFileName);

                        File.Copy(file, destFileFullPath);

                        byte[] buffer = File.ReadAllBytes(destFileFullPath);

                        CreateSignatureFile(destFileFullPath, buffer);

                        byte[] md5hash = MD5.Create().ComputeHash(buffer);

                        buildResult.AddCustomFile(destFileName);
                        buildResult.SetCustomFileHash(destFileName, Utility.ToHexString(md5hash));

                        if (i > guessMax) {
                            guessMax = i + i / 2;
                        }

                        EditorUtility.DisplayProgressBar("Lychee Bundle Builder",
                            "Add custom file " + Path.GetFileName(file),
                            CalculateProgress(0.0f, 1.0f, i++, guessMax));
                    }
                }

                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }
        }

        private void WriteListFile(BuildResult buildResult)
        {
            StreamWriter writer = new StreamWriter(Path.Combine(GetVersionPath(), "_list"), false);

            foreach (string bundle in buildResult.GetAllBundles())
            {
                string hash = buildResult.GetBundleHash(bundle);

                writer.Write(bundle + " " + hash + "\r\n");
            }

            foreach (string file in buildResult.GetAllCustomFiles())
            {
                string hash = buildResult.GetCustomFileHash(file);

                writer.Write(file + " " + hash + "\r\n");
            }

            writer.Close();
        }

        private void WriteBuildFile(BuildResult buildResult)
        {
            StreamWriter writer = new StreamWriter(Path.Combine(GetVersionPath(), "_build"), false);

            foreach (string bundle in buildResult.GetAllBundles())
            {
                writer.Write(bundle);

                string[] dependencies = buildResult.GetBundleDependencies(bundle, false);

                if (dependencies.Length > 0)
                {
                    writer.Write(":");

                    for (int i = 0; i < dependencies.Length; ++i)
                    {
                        if (i > 0) {
                            writer.Write(",");
                        }

                        writer.Write(dependencies[i]);
                    }
                }

                writer.Write("\r\n");

                foreach (string path in buildResult.GetBundleAssets(bundle))
                {
                    writer.Write("    " + path + "\r\n");
                }

                writer.Write("\r\n");
            }

            writer.Close();
        }

        private void RestrictPrefix()
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < prefix.Length; ++i)
                {
                    char c = char.ToLower(prefix[i]);

                    if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                    {
                        sb.Append(c);
                    }
                }

                prefix = sb.ToString();
            }
        }

        private void RemovesDuplicateScenes()
        {
            HashSet<string> set = new HashSet<string>();
            List<string> list = new List<string>();

            foreach (var scene in scenesInBuild)
            {
                if (set.Add(AssetDatabase.AssetPathToGUID(scene)))
                {
                    list.Add(NormalizeAssetPath(scene));
                }
            }

            scenesInBuild = list.ToArray();
        }

        private bool PrebuildAssetBundles()
        {
            Dictionary<string, string> assetBundleMap = new Dictionary<string, string>();

            try
            {
                for (int i = 0; i < scenesInBuild.Length; ++i)
                {
                    string[] list = GetDependencies(scenesInBuild[i], true);

                    for (int k = 0; k < list.Length; ++k)
                    {
                        string path = list[k];
                        string bundleName = AssetDatabase.AssetPathToGUID(path);

                        if (assetBundleMap.ContainsValue(bundleName) || IsExcludedAsset(path) || IsEditorOnlyAsset(path))
                        {
                            continue;
                        }

                        assetBundleMap.Add(path, bundleName);
                    }

                    EditorUtility.DisplayProgressBar("Lychee Bundle Builder", "Prepare to build asset bundles",
                        CalculateProgress(0.0f, 1.0f, i + 1, scenesInBuild.Length));
                }

                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }

            if (customScript != null)
            {
                try
                {
                    int i = 0;
                    int guessCount = 1;

                    foreach (var p in groupAssets_)
                    {
                        List<string> assets = p.second;

                        List<string> dependencies = new List<string>();

                        foreach (string asset in assets)
                        {
                            dependencies.AddRange(GetDependencies(asset, true));
                        }

                        assets.AddRange(dependencies);

                        foreach (string asset in assets)
                        {
                            string path = asset;
                            string bundleName = AssetDatabase.AssetPathToGUID(path);

                            if (assetBundleMap.ContainsValue(bundleName) || IsExcludedAsset(path) || IsEditorOnlyAsset(path))
                            {
                                continue;
                            }

                            assetBundleMap.Add(path, bundleName);
                        }

                        if (i == guessCount)
                        {
                            guessCount = i * 3;
                        }

                        EditorUtility.DisplayProgressBar("Lychee Bundle Builder", "Prepare to build asset bundles",
                            CalculateProgress(0.0f, 1.0f, i + 1, guessCount));

                        i += 1;
                    }

                    EditorUtility.ClearProgressBar();
                }
                catch (Exception)
                {
                    EditorUtility.ClearProgressBar();
                    throw;
                }
            }

            AssetBundleBuild[] builds = MakeBuildingMap(assetBundleMap);

            string outPath = GetAssetSizeHintDirectory(buildTarget_);

            DirectoryInfo di = Directory.CreateDirectory(outPath);

            if (!di.Exists)
            {
                Debug.LogError("Failed to create directory");
                return false;
            }

            BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle;

            if (compressAssetBundles) {
                options |= BuildAssetBundleOptions.ChunkBasedCompression;
            }
            else {
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outPath, builds, options, buildTarget_);

            if (manifest == null)
            {
                return false;
            }

            HashSet<string> validAssetBundles = new HashSet<string>();

            foreach (string bundleName in manifest.GetAllAssetBundles())
            {
                validAssetBundles.Add(bundleName.ToLower());
            }

            assetSizeHint_ = new Dictionary<string, long>();

            foreach (KeyValuePair<string, string> p in assetBundleMap)
            {
                if (validAssetBundles.Contains(p.Value))
                {
                    FileInfo fileInfo = new FileInfo(Path.GetFullPath(Path.Combine(outPath, p.Value)));

                    if (!fileInfo.Exists)
                    {
                        Debug.LogError("File error");
                        return false;
                    }

                    assetSizeHint_.Add(p.Key, fileInfo.Length - 4096);
                }
                else
                {
                    assetSizeHint_.Add(p.Key, 0);
                }
            }

            return true;
        }

        private bool CopyBundlesFromParentVersion()
        {
            try
            {
                for (int i = 0; i < bs_.Count; ++i)
                {
                    BuildDatabase bd = bs_[i];

                    Dictionary<string, byte[]> bundles = bd.GetBundles();

                    string[] list = bundles.Keys.ToArray();

                    MD5 md5 = MD5.Create();

                    for (int j = 0; j < list.Length; ++j)
                    {
                        string path = Path.Combine(GetVersionPath(), list[j]);

                        File.Copy(Path.Combine(GetParentVersionPath(), list[j]), path);

                        byte[] hashOld = bundles[list[j]];
                        byte[] hash = md5.ComputeHash(File.ReadAllBytes(path));

                        if (!hash.SequenceEqual(hashOld))
                        {
                            Debug.LogError("Bundle verification failed: " + path);
                            return false;
                        }

                        EditorUtility.DisplayProgressBar("Lychee Bundle Builder", "Copy bundles from parent version",
                            CalculateProgress(0.0f, 1.0f, j + 1, list.Length));
                    }
                }

                EditorUtility.ClearProgressBar();

                return true;
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }
        }

        private void CollectGroupAssets()
        {
            if (customScript == null) {
                return;
            }

            Dictionary<int, HashSet<string>> cache = new Dictionary<int, HashSet<string>>();

            int groupId;
            string[] assets;

            while (customScript.AddGroup(out groupId, out assets))
            {
                HashSet<string> set;

                if (!cache.TryGetValue(groupId, out set))
                {
                    set = new HashSet<string>();
                    cache.Add(groupId, set);
                }

                for (int i = 0; i < assets.Length; ++i)
                {
                    if (AssetExists(assets[i]))
                    {
                        set.Add(NormalizeAssetPath(assets[i]));
                    }
                    else
                    {
                        Debug.LogWarning("Asset does not exist: " + assets[i]);
                    }
                }
            }

            groupAssets_ = new List<Pair<int, List<string>>>();

            foreach (var p in cache)
            {
                groupAssets_.Add(Utility.MakePair(p.Key, p.Value.ToList()));
            }
        }

        private void InitializeBuildIntermediateVars()
        {
            bundleIdToBundleSize_ = new Dictionary<int, long>();
            assetPathToBundleId_ = new Dictionary<string, int>();
            bundleIdToAssetSet_ = new Dictionary<int, HashSet<string>>();
            mergeMap_ = new Dictionary<int, int>();
            relationship_ = new List<Pair<int, int>>();
            groupIdSet_ = new HashSet<int>();
            sceneBundleIdSet_ = new HashSet<int>();
            scenePrivateBundles_ = new Dictionary<int, HashSet<int>>();
            privateBundleIdSet_ = new HashSet<int>();

            currentBundleId_ = bs_[currentBdIdx_].currentBundleId;
        }

        private void SelectScenesInCurrentBd()
        {
            BuildDatabase bd = bs_[currentBdIdx_];

            List<string> list = new List<string>();

            foreach (string scene in scenesInBuild)
            {
                if (bd.HasSceneAsset(scene))
                {
                    list.Add(scene);
                }
            }

            scenesInBuild = list.ToArray();
        }

        private void UnselectScenesBeforeCurrentBd()
        {
            HashSet<string> set = new HashSet<string>();

            for (int i = 0; i < currentBdIdx_; ++i)
            {
                BuildDatabase bd = bs_[i];

                foreach (string scene in scenesInBuild)
                {
                    if (bd.HasSceneAsset(scene))
                    {
                        set.Add(scene);
                    }
                }
            }

            scenesInBuild = scenesInBuild.Except(set).ToArray();
        }

        private void SelectGroupsInCurrentBd()
        {
            if (customScript == null) {
                return;
            }

            BuildDatabase bd = bs_[currentBdIdx_];

            List<Pair<int, List<string>>> list = new List<Pair<int, List<string>>>();

            foreach (var p in groupAssets_)
            {
                if (bd.HasGroup(p.first))
                {
                    list.Add(p);
                }
            }

            groupAssets_ = list;
        }

        private void UnselectGroupsBeforeCurrentBd()
        {
            if (customScript == null) {
                return;
            }

            HashSet<int> set = new HashSet<int>();

            for (int i = 0; i < currentBdIdx_; ++i)
            {
                BuildDatabase bd = bs_[i];

                foreach (var p in groupAssets_)
                {
                    if (bd.HasGroup(p.first))
                    {
                        set.Add(p.first);
                    }
                }
            }

            List<Pair<int, List<string>>> list = new List<Pair<int, List<string>>>();

            foreach (var p in groupAssets_)
            {
                if (!set.Contains(p.first))
                {
                    list.Add(p);
                }
            }

            groupAssets_ = list;
        }

        private void AddAllScenes()
        {
            try
            {
                for (int i = 0; i < scenesInBuild.Length; ++i)
                {
                    AddScene(scenesInBuild[i]);

                    EditorUtility.DisplayProgressBar("Lychee Bundle Builder",
                        "Add scene " + (i + 1) + "/" + scenesInBuild.Length,
                        CalculateProgress(0.0f, 1.0f, i + 1, scenesInBuild.Length));
                }

                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }
        }

        private void AddAllGroups()
        {
            if (customScript == null) {
                return;
            }

            try
            {
                for (int i = 0; i < groupAssets_.Count; ++i)
                {
                    string[] assets = groupAssets_[i].second.ToArray();

                    int bundleId = NewBundleId();

                    SetGroupBundleId(bundleId);

                    AddAssetScrub(assets, bundleId);

                    EditorUtility.DisplayProgressBar("Lychee Bundle Builder", "Add group assets",
                        CalculateProgress(0.0f, 1.0f, i + 1, groupAssets_.Count));
                }

                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }
        }

        private bool BuildAssetBundles(BuildResultData buildResultData)
        {
            BuildDatabase bd = bs_[currentBdIdx_];

            int currentBundleId = bd.currentBundleId;

            Dictionary<int, int> denseBundleIdMap = new Dictionary<int, int>();
            {
                List<int> bundleIdList = bundleIdToAssetSet_.Keys.ToList();

                bundleIdList.Sort();

                for (int i = 0; i < bundleIdList.Count; ++i)
                {
                    if (bundleIdList[i] < bd.currentBundleId)
                    {
                        denseBundleIdMap.Add(bundleIdList[i], bundleIdList[i]);
                    }
                    else
                    {
                        denseBundleIdMap.Add(bundleIdList[i], currentBundleId++);
                    }
                }
            }

            BuildDatabase newBd = new BuildDatabase();

            newBd.prefix = prefix;
            newBd.currentBundleId = currentBundleId;

            for (int i = 0; i < scenesInBuild.Length; ++i)
            {
                newBd.AddSceneAsset(scenesInBuild[i]);
            }

            if (customScript != null)
            {
                for (int i = 0; i < groupAssets_.Count; ++i)
                {
                    newBd.AddGroup(groupAssets_[i].first);
                }
            }

            Dictionary<string, string> assetPathToBundleName = new Dictionary<string, string>();
            HashSet<string> sceneBundleSet = new HashSet<string>();

            try
            {
                int vi = 0;

                foreach (KeyValuePair<string, int> pair in assetPathToBundleId_)
                {
                    int bundleId = denseBundleIdMap[pair.Value];

                    string p = string.IsNullOrEmpty(prefix) ? "" : prefix + "_";
                    string p2 = currentBdIdx_.ToString("x") + "_";
                    string p3 = bundleId.ToString("x");

                    if (IsSceneBundleId(pair.Value))
                    {
                        string bundleName = p + "scene_" + p2 + p3;

                        assetPathToBundleName.Add(pair.Key, bundleName);
                        sceneBundleSet.Add(bundleName);
                    }
                    else
                    {
                        assetPathToBundleName.Add(pair.Key, p + "bundle_" + p2 + p3);
                    }

                    newBd.AddAsset(pair.Key, bundleId);

                    EditorUtility.DisplayProgressBar("Lychee Bundle Builder", "Prepare to build asset bundles",
                        CalculateProgress(0.0f, 1.0f, vi + 1, assetPathToBundleId_.Count));

                    ++vi;
                }

                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }

            AssetBundleBuild[] builds = MakeBuildingMap(assetPathToBundleName);

            BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle;

            if (compressAssetBundles) {
                options |= BuildAssetBundleOptions.ChunkBasedCompression;
            }
            else {
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                GetVersionPath(), builds, options, buildTarget_);

            if (manifest == null)
            {
                Debug.LogError("Failed to build asset bundles");
                return false;
            }

            foreach (string bundle in manifest.GetAllAssetBundles())
            {
                string path = Path.Combine(GetVersionPath(), bundle);

                byte[] buffer = File.ReadAllBytes(path);

                CreateSignatureFile(path, buffer);

                byte[] md5hash = MD5.Create().ComputeHash(buffer);

                buildResultData.AddBundle(bundle, sceneBundleSet.Contains(bundle));
                buildResultData.SetBundleHash(bundle, Utility.ToHexString(md5hash));

                newBd.AddBundle(bundle, md5hash);
            }

            TidyRelationship();

            foreach (int bundleId in GetLeafBundles())
            {
                newBd.SetLeafBundle(bundleId);
            }

            string[] oldList = bd.GetBundles().Keys.ToArray();

            foreach (string bundle in oldList)
            {
                if (!newBd.GetBundles().ContainsKey(bundle))
                {
                    File.Delete(Path.Combine(GetVersionPath(), bundle));
                }
            }

            foreach (AssetBundleBuild build in builds)
            {
                buildResultData.SetBundleDependencies(build.assetBundleName,
                    manifest.GetDirectDependencies(build.assetBundleName));

                buildResultData.SetBundleAssets(build.assetBundleName, build.assetNames);
            }

            bs_[currentBdIdx_] = newBd;

            return true;
        }

        private void CreateSignatureFile(string sourceFile, byte[] sourceFileData)
        {
            FileStream stream = new FileStream(sourceFile + ".signature", FileMode.CreateNew);

            Utility.WriteToFile(stream, Utility.ToBigEndianArray(sourceFileData.Length));

            RollingChecksum rc = new RollingChecksum();
            MD5 md5 = MD5.Create();

            for (int i = 0; i < sourceFileData.Length; i += Config.chunkSize)
            {
                int length = Math.Min(sourceFileData.Length - i, Config.chunkSize);

                rc.Update(Utility.SubArray(sourceFileData, i, length), length);

                Utility.WriteToFile(stream, Utility.ToBigEndianArray(rc.Digest()));
                Utility.WriteToFile(stream, md5.ComputeHash(sourceFileData, i, length));
            }

            stream.Close();
        }

        private void AddScene(string sceneAsset)
        {
            int sceneBundleId = CreateBundleForSceneAsset(sceneAsset);

            SetSceneBundleId(sceneBundleId);

            string[] toplevelAssets = GetDependencies(sceneAsset);

            for (int i = 0; i < toplevelAssets.Length; ++i)
            {
                if (IsLightingDataAsset(toplevelAssets[i])) {
                    AddLightingDataAsset(toplevelAssets[i], sceneBundleId);
                }
                else {
                    AddAssetScrub(new string[]{toplevelAssets[i]}, sceneBundleId);
                }
            }
        }

        private void AddAssetScrub(string[] assets, int rootBundleId = 0)
        {
            HashSet<int> nonleafBundles = new HashSet<int>();
            HashSet<int> leafBundles = new HashSet<int>();

            List<string> list = assets.ToList();
            HashSet<string> set = new HashSet<string>(assets);

            for (int i = 0; i < list.Count; ++i)
            {
                string path = list[i];

                Debug.Assert(!IsSceneAsset(path));

                int bundleId = CreateBundleForAsset(path);

                if (i < assets.Length && rootBundleId > 0)
                {
                    relationship_.Add(Utility.MakePair(rootBundleId, bundleId));
                }

                string[] deps = GetDependencies(path);

                if (deps.Length > 0)
                {
                    nonleafBundles.Add(bundleId);

                    foreach (string p in deps)
                    {
                        if (set.Add(p))
                        {
                            int bundleId2 = CreateBundleForAsset(p);

                            relationship_.Add(Utility.MakePair(bundleId, bundleId2));

                            list.Add(p);
                        }
                    }
                }
                else
                {
                    leafBundles.Add(bundleId);
                }
            }

            TryMergeBundles(leafBundles.ToList());
            TryMergeBundles(nonleafBundles.ToList());
        }

        private void AddLightingDataAsset(string rootAsset, int sceneBundleId)
        {
            foreach (string p in GetDependencies(rootAsset, true))
            {
                SetPrivateBundleId(CreateBundleForAsset(p), sceneBundleId);
            }
        }

        private int CreateBundleForSceneAsset(string path)
        {
            int bundleId = GetBundleFromAsset(path);

            if (bundleId == 0)
            {
                BuildDatabase bd = bs_[currentBdIdx_];

                if ((bundleId = bd.GetAssetBundleId(path)) > 0)
                {
                    MoveAssetToBundle(path, bundleId);
                }
            }

            if (bundleId == 0)
            {
                bundleId = NewBundleId();
                MoveAssetToBundle(path, bundleId);
            }

            return bundleId;
        }

        private int CreateBundleForAsset(string path)
        {
            int bundleId = GetBundleFromAsset(path);

            if (bundleId == 0)
            {
                BuildDatabase bd = bs_[currentBdIdx_];

                if ((bundleId = bd.GetAssetBundleId(path)) > 0)
                {
                    if (bd.CompareAssetHash(path))
                    {
                        MoveAssetToBundle(path, bundleId);
                    }
                    else
                    {
                        string[] dependencies = GetDependencies(path);

                        bool isLeafAsset = (dependencies.Length == 0);
                        bool isLeafBundle = bd.IsLeafBundle(bundleId);

                        if (isLeafAsset == isLeafBundle)
                        {
                            MoveAssetToBundle(path, bundleId);
                        }
                        else
                        {
                            bundleId = 0;
                        }
                    }
                }
            }

            if (bundleId == 0)
            {
                bundleId = NewBundleId();
                MoveAssetToBundle(path, bundleId);
            }

            return bundleId;
        }

        private void TidyRelationship()
        {
            HashSet<Pair<int, int>> relationship = new HashSet<Pair<int, int>>();

            for (int i = 0; i < relationship_.Count; ++i)
            {
                int first = FindMergedBundleId(relationship_[i].first);
                int second = FindMergedBundleId(relationship_[i].second);

                if (first != second) {
                    relationship.Add(Utility.MakePair(first, second));
                }
            }

            relationship_.Clear();

            foreach (Pair<int, int> p in relationship.Distinct())
            {
                relationship_.Add(Utility.MakePair(p.first, p.second));   
            }
        }

        private Pair<int, int> GetRelationships(int bundleId, List<Pair<int, int>> relationship)
        {
            int lower = 0;
            int upper = relationship.Count;
            int index = -1;

            while (lower < upper)
            {
                int mid = lower + (upper - lower) / 2;

                if (relationship[mid].first == bundleId) {
                    index = mid;
                    break;
                }

                if (relationship[mid].first < bundleId) {
                    lower = mid + 1;
                }
                else {
                    upper = mid;
                }
            }

            if (index >= 0)
            {
                lower = index;
                upper = index + 1;

                while (lower > 0 && relationship[lower - 1].first == bundleId) {
                    --lower;
                }

                while (upper < relationship.Count && relationship[upper].first == bundleId) {
                    ++upper;
                }

                return Utility.MakePair(lower, upper);
            }

            return Utility.MakePair(-1, -1);
        }

        private void MergeGroupBundles()
        {
            try
            {
                int[] groups = groupIdSet_.ToArray();

                for (int i = 0; i < groups.Length; ++i)
                {
                    TidyRelationship();

                    List<Pair<int, int>> relationship = relationship_.OrderBy(p => p.first).ToList();

                    HashSet<int> bundles = new HashSet<int>();
                    List<int> list = new List<int>();

                    list.Add(groups[i]);

                    for (int j = 0; j < list.Count; ++j)
                    {
                        Pair<int, int> p = GetRelationships(list[j], relationship);

                        for (int k = p.first; k < p.second; ++k)
                        {
                            int bundleId = relationship[k].second;

                            if (IsSceneBundleId(bundleId) || IsPrivateBundleId(bundleId))
                            {
                                continue;
                            }

                            if (bundles.Contains(bundleId))
                            {
                                continue;
                            }

                            bundles.Add(bundleId);
                            list.Add(bundleId);
                        }
                    }

                    list.RemoveAt(0);

                    HashSet<int> leafBundleSet = GetLeafBundles();

                    List<int> leafBundles = new List<int>();
                    List<int> nonleafBundles = new List<int>();

                    foreach (int bundleId in list)
                    {
                        if (leafBundleSet.Contains(bundleId))
                        {
                            leafBundles.Add(FindMergedBundleId(bundleId));
                        }
                        else
                        {
                            nonleafBundles.Add(FindMergedBundleId(bundleId));
                        }
                    }

                    TryMergeBundles(leafBundles);
                    TryMergeBundles(nonleafBundles);

                    float progress = CalculateProgress(0.0f, 1.0f, i + 1, groups.Length);

                    EditorUtility.DisplayProgressBar("Lychee Bundle Builder",
                        "Merge bundles", progress);
                }

                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }
        }

        private void MergeSceneDependentBundles()
        {
            try
            {
                List<Pair<int, List<int>>> sceneDependencies = new List<Pair<int, List<int>>>();

                foreach (int sceneBundle in sceneBundleIdSet_)
                {
                    sceneDependencies.Add(Utility.MakePair(sceneBundle, new List<int>()));
                }

                int count = sceneDependencies.Count;

                while (sceneDependencies.Count > 0)
                {
                    TidyRelationship();

                    List<Pair<int, int>> relationship = relationship_.OrderBy(p => p.first).ToList();

                    for (int i = 0; i < sceneDependencies.Count; ++i)
                    {
                        HashSet<int> bundles = new HashSet<int>();
                        List<int> list = new List<int>();

                        list.Add(sceneDependencies[i].first);

                        for (int j = 0; j < list.Count; ++j)
                        {
                            Pair<int, int> p = GetRelationships(list[j], relationship);

                            for (int k = p.first; k < p.second; ++k)
                            {
                                int bundleId = relationship[k].second;

                                if (IsSceneBundleId(bundleId) || IsPrivateBundleId(bundleId))
                                {
                                    continue;
                                }

                                if (bundles.Contains(bundleId))
                                {
                                    continue;
                                }

                                bundles.Add(bundleId);
                                list.Add(bundleId);
                            }
                        }

                        sceneDependencies[i].second = list.GetRange(1, list.Count - 1);
                    }

                    sceneDependencies = sceneDependencies.OrderByDescending(p => p.second.Count).ToList();

                    HashSet<int> leafBundleSet = GetLeafBundles();

                    List<int> leafBundles = new List<int>();
                    List<int> nonleafBundles = new List<int>();

                    foreach (int bundleId in sceneDependencies[0].second)
                    {
                        if (leafBundleSet.Contains(bundleId))
                        {
                            leafBundles.Add(FindMergedBundleId(bundleId));
                        }
                        else
                        {
                            nonleafBundles.Add(FindMergedBundleId(bundleId));
                        }
                    }

                    TryMergeBundles(leafBundles);
                    TryMergeBundles(nonleafBundles);

                    sceneDependencies.RemoveAt(0);

                    float progress = CalculateProgress(0.0f, 1.0f, count - sceneDependencies.Count, count);

                    EditorUtility.DisplayProgressBar("Lychee Bundle Builder",
                        "Merge bundles", progress);
                }

                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }
        }

        private HashSet<int> GetLeafBundles()
        {
            List<Pair<int, int>> relationship = new List<Pair<int, int>>();

            for (int i = 0; i < relationship_.Count; ++i)
            {
                Pair<int, int> p = relationship_[i];

                if (IsSceneBundleId(p.first) || IsSceneBundleId(p.second)) {
                    continue;
                }

                if (IsPrivateBundleId(p.first) || IsPrivateBundleId(p.second)) {
                    continue;
                }

                relationship.Add(p);
            }

            HashSet<int> bundles = new HashSet<int>();

            for (int i = 0; i < relationship.Count; ++i) {
                bundles.Add(relationship[i].second);
            }

            for (int i = 0; i < relationship.Count; ++i) {
                bundles.Remove(relationship[i].first);
            }

            return bundles;
        }

        private void MergeScenePrivateBundles()
        {
            int vi = 0;

            foreach (KeyValuePair<int, HashSet<int>> p in scenePrivateBundles_)
            {
                TryMergeBundles(p.Value.ToList());

                float progress = CalculateProgress(0.0f, 1.0f, vi + 1, scenePrivateBundles_.Count);

                EditorUtility.DisplayProgressBar("Lychee Bundle Builder",
                    "Merge bundles", progress);

                ++vi;
            }

            EditorUtility.ClearProgressBar();
        }

        private int FindMergedBundleId(int bundleId)
        {
            int newBundleId;

            while (mergeMap_.TryGetValue(bundleId, out newBundleId))
            {
                bundleId = newBundleId;
            }

            return bundleId;
        }

        private void TryMergeBundles(List<int> bundles)
        {
            BuildDatabase bd = bs_[currentBdIdx_];

            List<Pair<int, long>> newBundleList = new List<Pair<int, long>>();

            foreach (int newBundleId in bundles.FindAll(p => p >= bd.currentBundleId).Distinct())
            {
                newBundleList.Add(Utility.MakePair(newBundleId, GetCachedBundleSize(newBundleId)));
            }

            if (newBundleList.Count == 0) {
                return;
            }

            newBundleList = newBundleList.OrderBy(p => p.second).ToList();

            if (true)
            {
                List<Pair<int, long>> oldBundleList = new List<Pair<int, long>>();

                foreach (int oldBundleId in bundles.FindAll(p => p < bd.currentBundleId).Distinct())
                {
                    oldBundleList.Add(Utility.MakePair(oldBundleId, GetCachedBundleSize(oldBundleId)));
                }

                oldBundleList = oldBundleList.OrderBy(p => p.second).ToList();

                while (newBundleList.Count > 0 && oldBundleList.Count > 0)
                {
                    if (newBundleList[0].second + oldBundleList[0].second <= bundleSizeHint_)
                    {
                        MergeBundle(newBundleList[0].first, oldBundleList[0].first);
                        oldBundleList[0].second = GetCachedBundleSize(oldBundleList[0].first);

                        newBundleList.RemoveAt(0);
                    }
                    else
                    {
                        oldBundleList.RemoveAt(0);
                    }
                }
            }

            while (newBundleList.Count > 1)
            {
                if (newBundleList[0].second + newBundleList[1].second <= bundleSizeHint_)
                {
                    MergeBundle(newBundleList[0].first, newBundleList[1].first);
                    newBundleList[1].second = GetCachedBundleSize(newBundleList[1].first);
                }

                newBundleList.RemoveAt(0);
            }
        }

        private void MergeBundle(int sourceBundleId, int targetBundleId)
        {
            if (sourceBundleId == targetBundleId) {
                return;
            }

            HashSet<string> assets;

            if (bundleIdToAssetSet_.TryGetValue(sourceBundleId, out assets))
            {
                foreach (string path in assets.ToList()) {
                    MoveAssetToBundle(path, targetBundleId);
                }

                mergeMap_.Add(sourceBundleId, targetBundleId);

                bundleIdToAssetSet_.Remove(sourceBundleId);
            }
        }

        private void MoveAssetToBundle(string path, int bundleId)
        {
            int oldBundleId = GetBundleFromAsset(path);

            if (oldBundleId > 0)
            {
                bundleIdToAssetSet_[oldBundleId].Remove(path);
                bundleIdToBundleSize_.Remove(oldBundleId);
            }

            HashSet<string> assets;

            if (!bundleIdToAssetSet_.TryGetValue(bundleId, out assets))
            {
                assets = new HashSet<string>();
                bundleIdToAssetSet_.Add(bundleId, assets);
            }

            assets.Add(path);

            assetPathToBundleId_[path] = bundleId;

            bundleIdToBundleSize_.Remove(bundleId);
        }

        private int GetBundleFromAsset(string path)
        {
            int bundleId = 0;

            assetPathToBundleId_.TryGetValue(path, out bundleId);

            return bundleId;
        }

        private long GetCachedBundleSize(int bundleId)
        {
            long cachedSize;

            if (bundleIdToBundleSize_.TryGetValue(bundleId, out cachedSize)) {
                return cachedSize;
            }

            long size = GetBundleSize(bundleId);

            bundleIdToBundleSize_.Add(bundleId, size);

            return size;
        }

        private long GetBundleSize(int bundleId)
        {
            long size = 0;

            HashSet<string> assets;

            if (bundleIdToAssetSet_.TryGetValue(bundleId, out assets))
            {
                foreach (string path in assets)
                {
                    size += GetAssetSize(path);
                }
            }

            return size;
        }

        private void SetGroupBundleId(int groupId)
        {
            groupIdSet_.Add(groupId);
        }

        private void SetSceneBundleId(int bundleId)
        {
            sceneBundleIdSet_.Add(bundleId);
        }

        private bool IsSceneBundleId(int bundleId)
        {
            return sceneBundleIdSet_.Contains(bundleId);
        }

        private bool IsPrivateBundleId(int bundleId)
        {
            return privateBundleIdSet_.Contains(bundleId);
        }

        private void SetPrivateBundleId(int bundleId, int sceneBundleId)
        {
            Debug.Assert(sceneBundleId > 0);

            HashSet<int> bundles;

            if (!scenePrivateBundles_.TryGetValue(sceneBundleId, out bundles))
            {
                bundles = new HashSet<int>();
                scenePrivateBundles_.Add(sceneBundleId, bundles);
            }

            bundles.Add(bundleId);

            privateBundleIdSet_.Add(bundleId);
        }

        private long CalculateTextureAssetSize(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                object[] size = new object[2];

                GetWidthAndHeight_.Invoke(importer, size);

                int width = (int)size[0];
                int height = (int)size[1];

                int maxSize;
                TextureImporterFormat format;

                if (!importer.GetPlatformTextureSettings(texPlatform_, out maxSize, out format))
                {
                    maxSize = importer.maxTextureSize;
                    format = importer.GetAutomaticFormat(texPlatform_);
                }

                long assetSize = CalculateTextureSize(Math.Min(width, maxSize), Math.Min(height, maxSize), importer.mipmapEnabled, format);

                if (assetSize > 0) {
                    return assetSize;
                }
            }

            return GetAssetFileSize(path);
        }

        private long GetAssetSize(string path, bool recursive)
        {
            if (recursive)
            {
                long size = GetAssetSize(path);

                foreach (string p in GetDependencies(path, true))
                {
                    size += GetAssetSize(p);
                }

                return size;
            }
            else
            {
                return GetAssetSize(path);
            }
        }

        private long GetAssetSize(string path)
        {
            long size;

            if (!assetSizeHint_.TryGetValue(path, out size))
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

                if (type.IsSubclassOf(typeof(Texture)))
                {
                    size = CalculateTextureAssetSize(path);
                }
                //else if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) {
                //    size.exclusive = calculateFilmboxAssetSize(path);
                //}
                else
                {
                    size = GetAssetFileSize(path);
                }
            }

            return size;
        }

        int NewBundleId()
        {
            return currentBundleId_++;
        }

        private string GetVersionPath()
        {
            return Path.Combine(Path.Combine(location,
                GetTargetDirectoryName(buildTarget)), version);
        }

        private string GetParentVersionPath()
        {
            return Path.Combine(Path.Combine(location,
                    GetTargetDirectoryName(buildTarget)), parentVersion);
        }

        private static string[] GetDependencies(string path, bool recursive)
        {
            if (recursive)
            {
                List<string> list = new List<string>();
                HashSet<string> set = new HashSet<string>();

                list.Add(path);
                set.Add(AssetDatabase.AssetPathToGUID(path));

                for (int i = 0; i < list.Count; ++i)
                {
                    foreach (string p in GetDependencies(list[i]))
                    {
                        if (set.Add(AssetDatabase.AssetPathToGUID(p))) {
                            list.Add(p);
                        }
                    }
                }

                list.RemoveAt(0);

                return list.ToArray();
            }
            else
            {
                return GetDependencies(path);
            }
        }

        private static string[] GetDependencies(string path)
        {
            List<string> list = new List<string>();
            HashSet<string> set = new HashSet<string>();

            set.Add(AssetDatabase.AssetPathToGUID(path));

            foreach (string p in AssetDatabase.GetDependencies(path, false))
            {
                if (IsSceneAsset(p)) {
                    continue;
                }

                if (set.Add(AssetDatabase.AssetPathToGUID(p))) {
                    list.Add(p);
                }
            }

            return list.ToArray();
        }

        private static bool IsSceneAsset(string path)
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
            return (type == typeof(SceneAsset));
        }

        private static bool IsLightingDataAsset(string path)
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
            return (type == typeof(LightingDataAsset));
        }

        private static bool IsEditorOnlyAsset(string path)
        {
            if (string.Compare(Path.GetFileName(path), "LightingData.asset", true) == 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsExcludedAsset(string path)
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

            if (type == typeof(MonoScript)) {
                return true;
            }

            return false;
        }

        private static long GetAssetFileSize(string path)
        {
            long size = 0;

            FileInfo info = new FileInfo(Path.GetFullPath(path));

            if (info.Exists) {
                size = info.Length;
            }

            return size;
        }

        private static bool AssetExists(string path)
        {
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path));
        }

        private static string NormalizeAssetPath(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        private static float CalculateProgress(float start, float scale, int i, int imax)
        {
            return start + ((float)i / (float)imax * scale);
        }

        private static long CalculateTextureSize(int width, int height, bool mipmapEnabled, TextureImporterFormat format)
        {
            float size = width * height * 4 * (1.0f / 4.0f);

            if (mipmapEnabled)
            {
                size = size * 1.33f;
            }

            return (long)size;
        }

        private static AssetBundleBuild[] MakeBuildingMap(Dictionary<string, string> assetPathToBundleName)
        {
            var bundleNameToAssetSet = new SortedDictionary<string, HashSet<string>>();

            foreach (KeyValuePair<string, string> p in assetPathToBundleName)
            {
                if (IsExcludedAsset(p.Key)) {
                    continue;
                }

                HashSet<string> assets;

                if (bundleNameToAssetSet.TryGetValue(p.Value, out assets))
                {
                    assets.Add(p.Key);
                }
                else
                {
                    assets = new HashSet<string>();
                    assets.Add(p.Key);
                    bundleNameToAssetSet.Add(p.Value, assets);
                }
            }

            AssetBundleBuild[] builds = new AssetBundleBuild[bundleNameToAssetSet.Count];

            int i = 0;

            foreach (KeyValuePair<string, HashSet<string>> p in bundleNameToAssetSet)
            {
                builds[i].assetBundleName = p.Key;
                builds[i].assetNames = new string[p.Value.Count];

                int j = 0;

                foreach (string pp in p.Value)
                {
                    builds[i].assetNames[j] = pp;
                    ++j;
                }

                ++i;
            }

            return builds;
        }

        private static string GetAssetSizeHintDirectory(BuildTarget buildTarget)
        {
            return Path.Combine(Path.Combine("LycheeBundleBuilder", "AssetSizeHint"),
                GetTargetDirectoryName(buildTarget));
        }

        private static string GetTargetDirectoryName(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSX:
                    return "MacOS";
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    return "Linux";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.WSAPlayer:
                    return "WindowsStore";
                case BuildTarget.Tizen:
                    return "Tizen";
                case BuildTarget.PSP2:
                    return "PSP2";
                case BuildTarget.PS4:
                    return "PS4";
                case BuildTarget.XboxOne:
                    return "XboxOne";
                case BuildTarget.SamsungTV:
                    return "SamsungTV";
                case BuildTarget.N3DS:
                    return "Nintendo3DS";
                case BuildTarget.WiiU:
                    return "WiiU";
                case BuildTarget.tvOS:
                    return "tvOS";
                case BuildTarget.Switch:
                    return "Switch";
            }

            throw new Exception("Unknown build target");
        }

        private static string GetTexPlatformString(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSX:
            case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
                return "Standalone";
            case BuildTarget.iOS:
                return "iPhone";
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.WebGL:
                return "WebGL";
            case BuildTarget.WSAPlayer:
                return "Windows Store Apps";
            case BuildTarget.Tizen:
                return "Tizen";
            case BuildTarget.PSP2:
                return "PSP2";
            case BuildTarget.PS4:
                return "PS4";
            case BuildTarget.XboxOne:
                return "XboxOne";
            case BuildTarget.SamsungTV:
                return "Samsung TV";
            case BuildTarget.N3DS:
                return "Nintendo 3DS";
            case BuildTarget.WiiU:
                return "WiiU";
            case BuildTarget.tvOS:
                return "tvOS";
            case BuildTarget.Switch:
                return "Switch";
            }

            return "";
        }

        private static void ClearDirectory(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            foreach (FileSystemInfo info in dirInfo.GetFileSystemInfos())
            {
                if (info is FileInfo)
                {
                    FileInfo fi = info as FileInfo;
                    fi.Delete();
                }

                if (info is DirectoryInfo)
                {
                    DirectoryInfo di = info as DirectoryInfo;
                    di.Delete(true);
                }
            }
        }

        private MethodInfo GetWidthAndHeight_ = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
            BindingFlags.NonPublic|BindingFlags.Instance);

        private BuildTarget buildTarget_ = BuildTarget.StandaloneWindows;
        private string texPlatform_ = GetTexPlatformString(BuildTarget.StandaloneWindows);

        private BuildStack bs_;

        private int currentBdIdx_;

        private long bundleSizeHint_ = 1024 * 1024 * 8;

        private List<Pair<int, List<string>>> groupAssets_;

        private Dictionary<string, long> assetSizeHint_;

        ////////////////////////////////
        private Dictionary<int, long> bundleIdToBundleSize_;

        private Dictionary<string, int> assetPathToBundleId_;
        private Dictionary<int, HashSet<string>> bundleIdToAssetSet_;

        private Dictionary<int, int> mergeMap_;
        private List<Pair<int, int>> relationship_;

        private HashSet<int> groupIdSet_;

        private HashSet<int> sceneBundleIdSet_;

        private Dictionary<int, HashSet<int>> scenePrivateBundles_;
        private HashSet<int> privateBundleIdSet_;

        private int currentBundleId_;
        ////////////////////////////////
    }

    [Serializable]
    class Settings
    {
        public Settings()
        {
            currentLocationIndex = -1;
            locations = new List<string>();

            mode = 0;

            prefix = "";

            buildTarget = BuildTarget.StandaloneWindows;

            bundleSizeHint = 8.0f;

            compressAssetBundles = true;

            redundant = false;

            sceneList = new List<string>();
            checkedSceneList = new List<int>();

            compareWithParentVersion = false;
        }

        public int currentLocationIndex;
        public List<String> locations;

        public Mode mode;

        public string version;
        public string parentVersion;

        public string prefix;

        public BuildTarget buildTarget;

        public float bundleSizeHint;

        public bool compressAssetBundles;

        public bool redundant;

        public string customScript;

        public List<string> sceneList;
        public List<int> checkedSceneList;

        public bool compareWithParentVersion;
    }
}

public class LycheeBundleBuilder : EditorWindow
{
    [MenuItem("Window/Lychee Bundle Builder")]
    public static void ShowWindow()
    {
        EditorWindow w = EditorWindow.GetWindow(typeof(LycheeBundleBuilder));
        w.titleContent = new GUIContent("Lychee BB");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {

    }

    private void OnEnable()
    {
        menuItemsOnTopOfLocations_ = new string[]{ "[Browse]", "[Clear]", "[None]" };
        selectedLocationIndex_ = Array.IndexOf(menuItemsOnTopOfLocations_, "[None]");

        buildTargets_ = new LycheeBB.Pair<BuildTarget, string>[]
        {
            LycheeBB.Utility.MakePair(BuildTarget.StandaloneWindows, "Windows x86"),
            LycheeBB.Utility.MakePair(BuildTarget.StandaloneWindows64, "Windows x64"),
            LycheeBB.Utility.MakePair(BuildTarget.StandaloneOSXIntel, "MacOS x86"),
            LycheeBB.Utility.MakePair(BuildTarget.StandaloneOSXIntel64, "MacOS x64"),
            LycheeBB.Utility.MakePair(BuildTarget.StandaloneOSX, "MacOS Universal"),
            LycheeBB.Utility.MakePair(BuildTarget.StandaloneLinux, "Linux x86"),
            LycheeBB.Utility.MakePair(BuildTarget.StandaloneLinux64, "Linux x64"),
            LycheeBB.Utility.MakePair(BuildTarget.StandaloneLinuxUniversal, "Linux Universal"),
            LycheeBB.Utility.MakePair(BuildTarget.iOS, "iOS"),
            LycheeBB.Utility.MakePair(BuildTarget.Android, "Android"),
            LycheeBB.Utility.MakePair(BuildTarget.WebGL, "WebGL"),
            LycheeBB.Utility.MakePair(BuildTarget.WSAPlayer, "Windows Store"),
            LycheeBB.Utility.MakePair(BuildTarget.Tizen, "Tizen"),
            LycheeBB.Utility.MakePair(BuildTarget.PSP2, "PS Vita"),
            LycheeBB.Utility.MakePair(BuildTarget.PS4, "PS4"),
            LycheeBB.Utility.MakePair(BuildTarget.XboxOne, "Xbox One"),
            LycheeBB.Utility.MakePair(BuildTarget.SamsungTV, "Samsung Smart TV"),
            LycheeBB.Utility.MakePair(BuildTarget.N3DS, "Nintendo 3DS"),
            LycheeBB.Utility.MakePair(BuildTarget.WiiU, "Wii U"),
            LycheeBB.Utility.MakePair(BuildTarget.tvOS, "Apple's tvOS"),
            LycheeBB.Utility.MakePair(BuildTarget.Switch, "Nintendo Switch"),
        };

        for (selectedBuildTargetIndex_ = 0; selectedBuildTargetIndex_ < buildTargets_.Length; ++selectedBuildTargetIndex_)
        {
            if (buildTargets_[selectedBuildTargetIndex_].first == BuildTarget.StandaloneWindows) {
                break;
            }
        }

        List<LycheeBB.CustomScript> customScripts = new List<LycheeBB.CustomScript>();
        List<String> customScriptNames = new List<String>();

        customScripts.Add(null);
        customScriptNames.Add("[None]");

        Assembly editorAssembly = Assembly.GetAssembly(this.GetType());

        if (editorAssembly != null)
        {
            foreach (Type type in editorAssembly.GetTypes())
            {
                if (typeof(LycheeBB.CustomScript).IsAssignableFrom(type) && type.IsClass)
                {
                    LycheeBB.CustomScript customScript = Activator.CreateInstance(type) as LycheeBB.CustomScript;

                    customScript.Awake();

                    customScripts.Add(customScript);
                    customScriptNames.Add(type.FullName);
                }
            }
        }

        customScripts_ = customScripts.ToArray();
        customScriptNames_ = customScriptNames.ToArray();
        selectedCustomScriptIndex_ = 0;

        showScenesInBuild_ = new AnimBool(false);
        showScenesInBuild_.valueChanged.AddListener(Repaint);

        filter_ = "";

        settings_ = new LycheeBB.Settings();

        LoadSettings();

        start_ = false;
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    void Update()
    {
        if (start_)
        {
            start_ = false;

            builder_.Build();

            settings_.redundant = false;
            settings_.compareWithParentVersion = false;
        }
    }

    private void OnGUI()
    {
        GUIStyle style;

        scrollPos_ = EditorGUILayout.BeginScrollView(scrollPos_);

        if (start_)
        {
            EditorGUI.BeginDisabledGroup(true);
        }

        Event e = Event.current;

        GUILayout.Label("Location", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        List<string> locations = new List<String>(menuItemsOnTopOfLocations_);

        locations.AddRange(settings_.locations);

        int selectedLocationIndex = EditorGUILayout.Popup(selectedLocationIndex_, locations.ToArray());

        if (selectedLocationIndex == Array.IndexOf(menuItemsOnTopOfLocations_, "[Browse]"))
        {
            string location = EditorUtility.OpenFolderPanel("", "", "").Replace('/', '\\');

            if (location != "")
            {
                int oldIndex = locations.IndexOf(location);

                if (oldIndex >= 0)
                {
                    selectedLocationIndex_ = oldIndex;
                }
                else
                {
                    settings_.locations.Insert(0, location);

                    if (settings_.locations.Count > 20) {
                        locations.RemoveRange(20, locations.Count - 20);
                    }

                    selectedLocationIndex_ = menuItemsOnTopOfLocations_.Length;
                }
            }

            Repaint();
        }
        else if (selectedLocationIndex == Array.IndexOf(menuItemsOnTopOfLocations_, "[Clear]"))
        {
            if (settings_.locations.Count > 0)
            {
                if (EditorUtility.DisplayDialog("", "Are you sure you want to clear location history ?", "Yes", "Cancel"))
                {
                    settings_.locations.Clear();
                    selectedLocationIndex_ = Array.IndexOf(menuItemsOnTopOfLocations_, "[None]");
                }
            }

            Repaint();
        }
        else
        {
            selectedLocationIndex_ = selectedLocationIndex;
            Repaint();
        }

        if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(40)))
        {
            if (selectedLocationIndex_ >= menuItemsOnTopOfLocations_.Length) {
                //EditorUtility.RevealInFinder(locations[selectedLocationIndex_]);
                Application.OpenURL("file://" + locations[selectedLocationIndex_]);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Settings", EditorStyles.boldLabel);

        settings_.mode = (LycheeBB.Mode)EditorGUILayout.EnumPopup("Mode", settings_.mode);

        settings_.version = EditorGUILayout.TextField("Version", settings_.version);

        if (settings_.mode == LycheeBB.Mode.FullBuild)
        {
            settings_.prefix = EditorGUILayout.TextField("Prefix", settings_.prefix);
        }
        else
        {
            settings_.parentVersion = EditorGUILayout.TextField("Parent version", settings_.parentVersion);
        }

        string[] buildTargets = new string[buildTargets_.Length];

        for (int i = 0; i < buildTargets_.Length; ++i)
        {
            buildTargets[i] = buildTargets_[i].second;
        }

        settings_.buildTarget = buildTargets_[(selectedBuildTargetIndex_ = EditorGUILayout.Popup("Build Target", selectedBuildTargetIndex_, buildTargets))].first;

        settings_.bundleSizeHint = EditorGUILayout.FloatField("Bundle size hint (MBytes)", settings_.bundleSizeHint);

        settings_.compressAssetBundles = EditorGUILayout.ToggleLeft("Compress asset bundles", settings_.compressAssetBundles);

        if (settings_.mode == LycheeBB.Mode.IncrementalBuild)
        {
            settings_.redundant = EditorGUILayout.ToggleLeft("Create redundant asset bundles", settings_.redundant);

            GUILayout.Label("Create redundant asset bundles for newly added scenes to avoid the dependence on older asset bundles. This may increase the capacity of current and future incremental build.", EditorStyles.wordWrappedMiniLabel);
        }

        selectedCustomScriptIndex_ = EditorGUILayout.Popup("Custom Script", selectedCustomScriptIndex_, customScriptNames_);

        if (selectedCustomScriptIndex_ > 0)
        {
            LycheeBB.CustomScript customScript = customScripts_[selectedCustomScriptIndex_];

            if (customScript.HasUI()) {
                customScript.DrawUI();
            }
        }

        settings_.customScript = customScriptNames_[selectedCustomScriptIndex_];

        GUILayout.Label("Scenes in build", EditorStyles.boldLabel);

        if (showScenesInBuild_.target)
        {
            if (GUILayout.Button("Hide"))
            {
                showScenesInBuild_.target = false;
            }
        }
        else
        {
            if (GUILayout.Button("Show"))
            {
                showScenesInBuild_.target = true;
            }
        }

        if (EditorGUILayout.BeginFadeGroup(showScenesInBuild_.faded))
        {
            string filter = EditorGUILayout.TextField("Filter", filter_);

            if (filter != filter_)
            {
                if (filter.Length > 0)
                {
                    if (filter.StartsWith("?"))
                    {
                        matchResult_.Clear();

                        if (filter == "?+")
                        {
                            for (int i = 0; i < settings_.sceneList.Count; ++i)
                            {
                                if (IsChecked(i))
                                {
                                    matchResult_.Add(i);
                                }
                            }
                        }
                        else if (filter == "?-")
                        {
                            for (int i = 0; i < settings_.sceneList.Count; ++i)
                            {
                                if (!IsChecked(i))
                                {
                                    matchResult_.Add(i);
                                }
                            }
                        }
                    }
                    else
                    {
                        matchResult_ = Match(filter, sceneNameList_);
                    }
                }
                else
                {
                    matchResult_.Clear();
                }

                filter_ = filter;
            }

            int count = 0;

            EditorGUILayout.BeginVertical(GUILayout.MinHeight(100.0f));

            if (filter.Length > 0)
            {
                foreach (int index in matchResult_)
                {
                    string path = settings_.sceneList[index];

                    SetChecked(index, EditorGUILayout.ToggleLeft(path, IsChecked(index)));

                    ++count;
                }
            }
            else
            {
                for (int i = 0; i < settings_.sceneList.Count; ++i)
                {
                    string path = settings_.sceneList[i];

                    SetChecked(i, EditorGUILayout.ToggleLeft(path, IsChecked(i)));

                    ++count;
                }
            }

            if (filter.Length == 0 && count == 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Drag and drop scene assets/folders here");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndVertical();

            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
                {
                    if (DragAndDrop.paths.Length > 0)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    }
                }

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    AddScenes(DragAndDrop.paths);

                    SaveSettings();
                }
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All"))
            {
                if (filter.Length > 0)
                {
                    foreach (int index in matchResult_)
                    {
                        checkedSceneList_.Add(index);
                    }
                }
                else
                {
                    for (int i = 0; i < settings_.sceneList.Count; ++i)
                    {
                        checkedSceneList_.Add(i);
                    }
                }

                SaveSettings();
            }

            if (GUILayout.Button("Select None"))
            {
                if (filter.Length > 0)
                {
                    foreach (int index in matchResult_)
                    {
                        checkedSceneList_.Remove(index);
                    }
                }
                else
                {
                    for (int i = 0; i < settings_.sceneList.Count; ++i)
                    {
                        checkedSceneList_.Remove(i);
                    }
                }

                SaveSettings();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear"))
            {
                if (settings_.sceneList.Count > 0)
                {
                    if (EditorUtility.DisplayDialog("SceneBuilder", "Are you sure you want to clear the scene list ?", "Yes", "Cancel"))
                    {
                        matchResult_.Clear();
                        settings_.sceneList.Clear();
                        sceneNameList_.Clear();
                        sceneSet_.Clear();
                        checkedSceneList_.Clear();

                        SaveSettings();
                    }
                }
            }

            EditorGUILayout.EndFadeGroup();
        }

        GUILayout.Label("Build", EditorStyles.boldLabel);

        if (settings_.mode == LycheeBB.Mode.IncrementalBuild)
        {
            settings_.compareWithParentVersion = EditorGUILayout.ToggleLeft("Report version comparison after build",
                settings_.compareWithParentVersion);

            GUILayout.Space(9);
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        style = new GUIStyle("button");

        style.fontSize = 14;

        if (GUILayout.Button("Build", style, GUILayout.Height(20), GUILayout.Width(225)))
        {
            SaveSettings();

            builder_ = new LycheeBB.Builder();

            builder_.mode = settings_.mode;
            builder_.location = locations[selectedLocationIndex_];
            builder_.version = settings_.version;
            builder_.prefix = settings_.prefix;
            builder_.parentVersion = settings_.parentVersion;
            builder_.buildTarget = settings_.buildTarget;
            builder_.bundleSizeHint = settings_.bundleSizeHint;
            builder_.compressAssetBundles = settings_.compressAssetBundles;
            builder_.redundant = settings_.redundant;
            builder_.compareWithParentVersion = settings_.compareWithParentVersion;

            if (selectedCustomScriptIndex_ > 0) {
                builder_.customScript = customScripts_[selectedCustomScriptIndex_];
            }

            List<string> scenesInBuild = new List<string>();

            for (int i = 0; i < settings_.sceneList.Count; ++i)
            {
                if (IsChecked(i))
                {
                    scenesInBuild.Add(settings_.sceneList[i]);
                }
            }

            builder_.scenesInBuild = scenesInBuild.ToArray();

            start_ = true;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (start_)
        {
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(9);
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    private void LoadSettings()
    {
        try {
            StreamReader reader = new StreamReader("LycheeBundleBuilder.config");

            BinaryFormatter formatter = new BinaryFormatter();

            var settings = formatter.Deserialize(reader.BaseStream) as LycheeBB.Settings;

            if (settings != null)
            {
                settings_ = settings;

                if (settings_.locations == null)
                {
                    settings_.locations = new List<string>();
                }

                if (settings_.prefix == null)
                {
                    settings_.prefix = "";
                }

                if (settings_.sceneList == null)
                {
                    settings_.sceneList = new List<string>();
                }

                if (settings_.currentLocationIndex >= 0 && settings_.currentLocationIndex < settings_.locations.Count) {
                    selectedLocationIndex_ = menuItemsOnTopOfLocations_.Length + settings_.currentLocationIndex;
                }

                for (selectedBuildTargetIndex_ = 0; selectedBuildTargetIndex_ < buildTargets_.Length; ++selectedBuildTargetIndex_)
                {
                    if (buildTargets_[selectedBuildTargetIndex_].first == settings_.buildTarget) {
                        break;
                    }
                }

                for (int i = 0; i < customScriptNames_.Length; ++i)
                {
                    if (customScriptNames_[i] == settings_.customScript) {
                        selectedCustomScriptIndex_ = i;
                    }
                }

                if (settings_.checkedSceneList != null)
                {
                    checkedSceneList_ = new HashSet<int>(settings_.checkedSceneList);
                }

                settings_.redundant = false;
            }

            reader.Close();
        }
        catch (Exception) {
        }
    }

    private void SaveSettings()
    {
        FileStream stream = new FileStream("LycheeBundleBuilder.config", FileMode.Create);

        BinaryFormatter formatter = new BinaryFormatter();

        if (selectedLocationIndex_ < menuItemsOnTopOfLocations_.Length) {
            settings_.currentLocationIndex = -1;
        }
        else {
            settings_.currentLocationIndex = selectedLocationIndex_ - menuItemsOnTopOfLocations_.Length;
        }

        settings_.checkedSceneList = checkedSceneList_.ToList();

        formatter.Serialize(stream, settings_);

        stream.Close();
    }

    private List<string> GetSceneAssetPaths()
    {
        var scenePaths = AssetDatabase.GetAllAssetPaths().Where(path =>
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

            if (type == typeof(SceneAsset)) {
                return true;
            }

            return false;
        });

        return new List<string>(scenePaths);
    }

    private void AddScenes(string[] paths)
    {
        for (int i = 0; i < paths.Length; ++i)
        {
            string path = paths[i].Replace('\\', '/');

            if (File.Exists(path))
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

                if (type == typeof(SceneAsset))
                {
                    if (sceneSet_.Add(AssetDatabase.AssetPathToGUID(path)))
                    {
                        settings_.sceneList.Add(path);

                        string name = Path.GetFileNameWithoutExtension(path);

                        sceneNameList_.Add(name);
                    }
                }
            }
            else if (Directory.Exists(path))
            {
                AddScenes(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
            }
        }
    }

    private List<int> Match(string searchText, List<string> list)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.Add(i);
            }
        }

        return result;
    }

    private void SetChecked(int index, bool c)
    {
        if (c)
        {
            checkedSceneList_.Add(index);
        }
        else
        {
            checkedSceneList_.Remove(index);
        }
    }

    private bool IsChecked(int index)
    {
        return checkedSceneList_.Contains(index);
    }

    private LycheeBB.Builder builder_;

    private Vector2 scrollPos_;

    private string[] menuItemsOnTopOfLocations_;
    private int selectedLocationIndex_;

    private LycheeBB.Pair<BuildTarget, string>[] buildTargets_;
    private int selectedBuildTargetIndex_;

    private LycheeBB.CustomScript[] customScripts_;
    private string[] customScriptNames_;
    private int selectedCustomScriptIndex_;

    private AnimBool showScenesInBuild_;

    private string filter_;

    HashSet<string> sceneSet_ = new HashSet<string>();
    private List<int> matchResult_ = new List<int>();
    private List<string> sceneNameList_ = new List<string>();
    private HashSet<int> checkedSceneList_ = new HashSet<int>();

    private LycheeBB.Settings settings_;

    private bool start_;
}

