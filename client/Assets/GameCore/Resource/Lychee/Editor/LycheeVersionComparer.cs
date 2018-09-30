using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LycheeVS
{
    class Config
    {
        public static int chunkSize = 65536;
    }

    public class CompareResult
    {
        public long sourceVersionCapacity;
        public long destVersionCapacity;
        public long downloadSize;

        public int reusableChunks;
        public int totalChunks;

        public string[] unchangedFiles;
        public string[] newFiles;
        public string[] changedFiles;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Source version: ");
            sb.Append((sourceVersionCapacity / 1024.0 / 1024.0).ToString("F2"));
            sb.Append("MBytes");
            sb.AppendLine();

            sb.Append("Destination version: ");
            sb.Append((destVersionCapacity / 1024.0 / 1024.0).ToString("F2"));
            sb.Append("MBytes");
            sb.AppendLine();
            sb.AppendLine();

            sb.Append("Download: ");
            sb.Append((downloadSize / 1024.0 / 1024.0).ToString("F2"));
            sb.Append("MBytes (");
            sb.Append(((double)downloadSize / destVersionCapacity * 100.0).ToString("F2"));
            sb.Append("% of dest)");
            sb.AppendLine();
            sb.AppendLine();

            sb.Append("Reusable: ");
            sb.Append(((destVersionCapacity - downloadSize) / 1024.0 / 1024.0).ToString("F2"));
            sb.Append("MBytes (");
            sb.Append(((double)(destVersionCapacity - downloadSize) / sourceVersionCapacity * 100.0).ToString("F2"));
            sb.Append("% of source)");
            sb.AppendLine();
            sb.AppendLine();

            sb.Append("Reusable chunks in changed files: ");
            sb.Append(reusableChunks);
            sb.Append(" (");
            sb.Append(((double)reusableChunks / totalChunks * 100.0).ToString("F2"));
            sb.Append("% of ");
            sb.Append(totalChunks);
            sb.Append(")");
            sb.AppendLine();
            sb.AppendLine();

            sb.Append("Unchanged files (");
            sb.Append(unchangedFiles.Length);
            sb.Append("): ");
            for (int i = 0; i < unchangedFiles.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(unchangedFiles[i]);
            }
            sb.AppendLine();
            sb.AppendLine();

            sb.Append("New files (");
            sb.Append(newFiles.Length);
            sb.Append("): ");
            for (int i = 0; i < newFiles.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(newFiles[i]);
            }
            sb.AppendLine();
            sb.AppendLine();

            sb.Append("Changed files (");
            sb.Append(changedFiles.Length);
            sb.Append("): ");
            for (int i = 0; i < changedFiles.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(changedFiles[i]);
            }
            sb.AppendLine();
            sb.AppendLine();

            return sb.ToString();
        }
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

            if (o == null)
            {
                return false;
            }

            return first.Equals(o.first) && second.Equals(o.second);
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
    struct ChunkHash
    {
        public uint digest;
        public byte[] md5;
    }

    [Serializable]
    class Signature
    {
        public int length;
        public ChunkHash[] chunkHashes;
    }

    class SignatureIndex
    {
        public uint[] table;
        public int[][] mapping;

        static public SignatureIndex Create(Signature sign)
        {
            SignatureIndex index = new SignatureIndex();

            index.table = new uint[sign.chunkHashes.Length];

            for (int i = 0; i < index.table.Length; ++i)
            {
                index.table[i] = sign.chunkHashes[i].digest;
            }

            Array.Sort(index.table);

            index.mapping = new int[index.table.Length][];

            for (int i = 0; i < index.table.Length; ++i)
            {
                List<int> v = new List<int>();

                for (int j = 0; j < sign.chunkHashes.Length; ++j)
                {
                    if (sign.chunkHashes[j].digest == index.table[i])
                    {
                        v.Add(j);
                    }
                }

                index.mapping[i] = v.ToArray();
            }

            return index;
        }
    }

    class Utility
    {
        public static Pair<T1, T2> MakePair<T1, T2>(T1 a, T2 b)
        {
            return new Pair<T1, T2>(a, b);
        }

        public static int ReadFile(Stream stream, byte[] buffer, int count)
        {
            int offset = 0;
            int read;

            while (count > 0 && (read = stream.Read(buffer, offset, count)) != 0)
            {
                offset += read;
                count -= read;
            }

            return offset;
        }

        public static int ReadInt(byte[] buffer, int startIndex)
        {
            return FromBigEndian(BitConverter.ToInt32(buffer, startIndex));
        }

        public static uint ReadUInt(byte[] buffer, int startIndex)
        {
            return FromBigEndian(BitConverter.ToUInt32(buffer, startIndex));
        }

        public static int FromBigEndian(int value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = (int)SwapEndianness((uint)value);
            }

            return value;
        }

        public static uint FromBigEndian(uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = SwapEndianness(value);
            }

            return value;
        }

        public static uint SwapEndianness(uint x)
        {
            return ((x & 0x000000ff) << 24) | ((x & 0x0000ff00) << 8)
                | ((x & 0x00ff0000) >> 8) | ((x & 0xff000000) >> 24);
        }
    }

    public class Comparer
    {
        public Comparer(string sourceVersion, string destVersion)
        {
            sourceVersion_ = sourceVersion;
            destVersion_ = destVersion;
        }

        public CompareResult Compare()
        {
            CompareResult result = new CompareResult();

            try
            {
                int vi = 0;

                EditorUtility.DisplayProgressBar("Lychee Version Comparer", "Comparing", 0.0f);

                List<Pair<string, string>> srcList = LoadList(Path.Combine(sourceVersion_, "_list"));
                List<Pair<string, string>> destList = LoadList(Path.Combine(destVersion_, "_list"));

                List<Pair<string, string>> unchangedFileList = new List<Pair<string, string>>(
                    destList.Where(p1 => srcList.Any(p2 => p2.first == p1.first && p2.second == p1.second)));

                List<Pair<string, string>> newFileList = new List<Pair<string, string>>(
                    destList.Where(p1 => srcList.All(p2 => p2.first != p1.first)));

                List<Pair<string, string>> changedFileList = new List<Pair<string, string>>(
                    destList.Where(p1 => srcList.Any(p2 => p2.first == p1.first && p2.second != p1.second)));

                foreach (var p in srcList)
                {
                    FileInfo fi = new FileInfo(Path.Combine(sourceVersion_, p.first));
                    result.sourceVersionCapacity += fi.Length;
                }

                foreach (var p in destList)
                {
                    FileInfo fi = new FileInfo(Path.Combine(destVersion_, p.first));
                    result.destVersionCapacity += fi.Length;
                }

                List<string> list = new List<string>();

                foreach (var p in unchangedFileList)
                {
                    list.Add(p.first);
                }

                result.unchangedFiles = list.ToArray();
                list.Clear();

                foreach (var p in newFileList)
                {
                    list.Add(p.first);

                    FileInfo fi = new FileInfo(Path.Combine(destVersion_, p.first));
                    result.downloadSize += fi.Length;
                }

                result.newFiles = list.ToArray();
                list.Clear();

                EditorUtility.DisplayProgressBar("Lychee Version Comparer", "Comparing", 0.2f);

                vi = 0;

                foreach (var p in changedFileList)
                {
                    list.Add(p.first);

                    Signature sign = LoadSignature(Path.Combine(destVersion_, p.first + ".signature"));

                    int count = CountReusableChunks(sign, Path.Combine(sourceVersion_, p.first));

                    result.reusableChunks += count;
                    result.totalChunks += sign.chunkHashes.Length;

                    result.downloadSize += (sign.length - (count * Config.chunkSize));

                    EditorUtility.DisplayProgressBar("Lychee Version Comparer",
                        "Comparing",
                        CalculateProgress(0.2f, 0.8f, ++vi, changedFileList.Count));
                }

                result.changedFiles = list.ToArray();

                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }

            return result;
        }

        private int CountReusableChunks(Signature destFileSign, string srcFilePath)
        {
            int count = 0;

            SignatureIndex signIdx = SignatureIndex.Create(destFileSign);

            Stream stream = File.OpenRead(srcFilePath);

            byte[] buffer = new byte[Config.chunkSize];

            RollingChecksum rc = new RollingChecksum();
            MD5 md5 = MD5.Create();

            while (Utility.ReadFile(stream, buffer, Config.chunkSize) == Config.chunkSize)
            {
                rc.Update(buffer, Config.chunkSize);

                uint digest = rc.Digest();

                int indexOfDigest = Array.BinarySearch(signIdx.table, digest);

                int chunkIndex = -1;

                if (indexOfDigest >= 0)
                {
                    if ((chunkIndex = Match(destFileSign, signIdx, indexOfDigest, md5.ComputeHash(buffer))) >= 0)
                    {
                        ++count;
                        continue;
                    }
                }

                int index = 0;
                int b;

                while ((b = stream.ReadByte()) != -1)
                {
                    byte newByte = (byte)b;
                    byte oldByte = buffer[index];

                    buffer[index] = newByte;

                    rc.Roll(oldByte, newByte);

                    digest = rc.Digest();

                    if (++index == Config.chunkSize)
                    {
                        index = 0;
                    }

                    indexOfDigest = Array.BinarySearch(signIdx.table, digest);

                    if (indexOfDigest >= 0)
                    {
                        md5.Initialize();
                        md5.TransformBlock(buffer, index, buffer.Length - index, null, 0);
                        md5.TransformFinalBlock(buffer, 0, index);

                        if ((chunkIndex = Match(destFileSign, signIdx, indexOfDigest, md5.Hash)) >= 0)
                        {
                            ++count;
                            break;
                        }
                    }
                }
            }

            stream.Close();

            return count;
        }

        private int Match(Signature sign, SignatureIndex index, int indexOfDigest, byte[] md5)
        {
            int[] list = index.mapping[indexOfDigest];

            for (int i = 0; i < list.Length; ++i)
            {
                if (md5.SequenceEqual(sign.chunkHashes[list[i]].md5))
                {
                    return list[i];
                }
            }

            return -1;
        }

        private List<Pair<string, string>> LoadList(string path)
        {
            List<Pair<string, string>> result = new List<Pair<string, string>>();

            StreamReader reader = new StreamReader(path);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] v = line.Split(' ');
                result.Add(Utility.MakePair(v[0], v[1]));
            }

            reader.Close();

            return result;
        }

        private Signature LoadSignature(string path)
        {
            Signature sign = new Signature();

            byte[] buffer = File.ReadAllBytes(path);

            int index = 0;

            sign.length = Utility.ReadInt(buffer, index);
            index += 4;

            int count = sign.length / Config.chunkSize;

            if (sign.length % Config.chunkSize != 0)
            {
                count += 1;
            }

            sign.chunkHashes = new ChunkHash[count];

            for (int i = 0; i < count; ++i)
            {
                sign.chunkHashes[i].digest = Utility.ReadUInt(buffer, index);
                index += 4;
                sign.chunkHashes[i].md5 = new byte[16];
                Array.Copy(buffer, index, sign.chunkHashes[i].md5, 0, 16);
                index += 16;
            }

            return sign;
        }

        private static float CalculateProgress(float start, float scale, int i, int imax)
        {
            return start + ((float)i / (float)imax * scale);
        }

        private string sourceVersion_;
        private string destVersion_;
    }
}

public class LycheeVersionComparer : EditorWindow
{
    [MenuItem("Window/Lychee Version Comparer")]
    public static void ShowWindow()
    {
        EditorWindow w = EditorWindow.GetWindow(typeof(LycheeVersionComparer));
        w.titleContent = new GUIContent("Lychee VS");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    void Update()
    {
    }

    private void OnGUI()
    {
        GUIStyle style;

        scrollPos_ = EditorGUILayout.BeginScrollView(scrollPos_);

        GUILayout.Label("Source version", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(sourceVersion_, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

        if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            string location = BrowseFolder(sourceVersion_);

            if (location != "")
            {
                sourceVersion_ = location;
            }

            Repaint();
        }

        if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(40)))
        {
            if (!string.IsNullOrEmpty(sourceVersion_) && Directory.Exists(sourceVersion_))
            {
                Application.OpenURL("file://" + sourceVersion_);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Destination version", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(destVersion_, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

        if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            string location = BrowseFolder(destVersion_);

            if (location != "")
            {
                destVersion_ = location;
            }

            Repaint();
        }

        if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(40)))
        {
            if (!string.IsNullOrEmpty(destVersion_) && Directory.Exists(destVersion_))
            {
                Application.OpenURL("file://" + destVersion_);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Compare", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        style = new GUIStyle("button");

        style.fontSize = 14;

        if (GUILayout.Button("Compare", style, GUILayout.Height(20), GUILayout.Width(225)))
        {
            LycheeVS.Comparer c = new LycheeVS.Comparer(sourceVersion_, destVersion_);
            result_ = c.Compare().ToString();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(result_))
        {
            GUILayout.Label("Result", EditorStyles.boldLabel);

            result_ = GUILayout.TextArea(result_);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save"))
            {
                string path = EditorUtility.SaveFilePanel("Save result", "", "", "txt");

                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllText(path, result_);
                }
            }

            if (GUILayout.Button("Copy"))
            {
                EditorGUIUtility.systemCopyBuffer = result_;
            }

            if (GUILayout.Button("Clear"))
            {
                result_ = "";
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(9);
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    private static string BrowseFolder(string folder)
    {
        return EditorUtility.OpenFolderPanel("", folder, "").Replace('/', '\\');
    }

    private Vector2 scrollPos_;
    private string sourceVersion_;
    private string destVersion_;
    private string result_;
}

