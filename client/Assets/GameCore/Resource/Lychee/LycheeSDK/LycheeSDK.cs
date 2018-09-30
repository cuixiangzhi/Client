using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System;
using System.Text;
using UnityEngine;

namespace LycheeSDK
{
    class Config
    {
        public static int chunkSize = 65536;
    }

    public interface HttpRequest
    {
        bool IsError();
        string ErrorMessage();

        IEnumerator Send();

        string Text();
        byte[] Data();
    }

    public interface Http
    {
        HttpRequest Get(string url);
        HttpRequest Get(string url, int offset, int length);
    }

    public enum Location
    {
        Initial = 1,
        Download = 2,
        Update = 3,
    }

    public interface FileSystem
    {
        bool Exists(Location location, string filename);

        void Delete(Location location, string filename);

        void Copy(Location location, string filename, Location location2, string filename2);

        string[] GetFiles(Location location);

        Stream Open(Location location, string filename, FileMode mode, FileAccess access);
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

    [Serializable]
    class OutPara<T>
    {
        public T para;
    }

    class FileLocator
    {
        public FileLocator(Location location, string filename)
        {
            this.location = location;
            this.filename = filename;
        }

        public Location location;
        public string filename;
    }

    class Utility
    {
        public static Pair<T1, T2> MakePair<T1, T2>(T1 a, T2 b)
        {
            return new Pair<T1, T2>(a, b);
        }

        static public Stream Create(FileSystem fs, FileLocator fl)
        {
            return fs.Open(fl.location, fl.filename, FileMode.Create, FileAccess.Write);
        }

        static public Stream OpenWrite(FileSystem fs, FileLocator fl)
        {
            return fs.Open(fl.location, fl.filename, FileMode.OpenOrCreate, FileAccess.Write);
        }

        static public Stream OpenRead(FileSystem fs, FileLocator fl)
        {
            return fs.Open(fl.location, fl.filename, FileMode.Open, FileAccess.Read);
        }

        static public TextReader OpenText(FileSystem fs, FileLocator fl)
        {
            return new StreamReader(OpenRead(fs, fl));
        }

        static public bool Exists(FileSystem fs, FileLocator fl)
        {
            return fs.Exists(fl.location, fl.filename);
        }

        static public void WriteAllText(FileSystem fs, FileLocator fl, string text)
        {
            Stream stream = Create(fs, fl);

            byte[] bytes = Encoding.UTF8.GetBytes(text);

            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            stream.Close();
        }

        static public void SaveObjectToFile(FileSystem fs, FileLocator fl, object obj)
        {
            Stream stream = Create(fs, fl);

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);

            stream.Flush();
            stream.Close();

            SignFile(fs, fl, new FileLocator(fl.location, fl.filename + ".sign"));
        }

        static public object LoadObjectFromFile(FileSystem fs, FileLocator fl)
        {
            object obj = null;

            if (VerifyFile(fs, fl, new FileLocator(fl.location, fl.filename + ".sign")))
            {
                Stream stream = OpenRead(fs, fl);

                BinaryFormatter formatter = new BinaryFormatter();

                obj = formatter.Deserialize(stream);

                stream.Close();
            }

            return obj;
        }

        static public void DeleteObject(FileSystem fs, FileLocator fl)
        {
            fs.Delete(fl.location, fl.filename);
            fs.Delete(fl.location, fl.filename + ".sign");
        }

        public static void SignFile(FileSystem fs, FileLocator fl, FileLocator fl2)
        {
            Stream stream = Create(fs, fl2);

            byte[] sign = SignFile(fs, fl);

            stream.Write(sign, 0, sign.Length);

            stream.Close();
        }

        public static byte[] SignFile(FileSystem fs, FileLocator fl)
        {
            Stream stream = OpenRead(fs, fl);

            byte[] sign = MD5.Create().ComputeHash(stream);

            stream.Close();

            return sign;
        }

        public static bool VerifyFile(FileSystem fs, FileLocator fl, FileLocator fl2)
        {
            bool result = false;

            if (Exists(fs, fl2))
            {
                Stream stream = OpenRead(fs, fl2);

                byte[] sign = new byte[16];

                if (ReadFile(stream, sign, 16) == 16)
                {
                    result = VerifyFile(fs, fl, sign);
                }

                stream.Close();
            }

            return result;
        }

        public static bool VerifyFile(FileSystem fs, FileLocator fl, byte[] sign)
        {
            if (Exists(fs, fl))
            {
                Stream stream = OpenRead(fs, fl);

                bool result = MD5.Create().ComputeHash(stream).SequenceEqual(sign);

                stream.Close();

                return result;
            }

            return false;
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

        public static string ToHexString(byte[] buffer)
        {
            StringBuilder s = new StringBuilder(buffer.Length * 2);

            foreach (byte b in buffer)
            {
                s.Append(b.ToString("x2"));
            }

            return s.ToString();
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

        public static float CalculateProgress(float start, float scale, int i, int imax)
        {
            if (imax == 0) {
                return start + scale;
            }

            return start + (((float)i / imax) * scale);
        }
    }

    enum TaskStatus
    {
        Start = 0,
        Finished = 1,
        Failed = 2,
    }

    abstract class Task
    {
        public TaskStatus status = TaskStatus.Start;
        public object result = null;
        public string errorMessage = "";
        public object userData;

        public abstract IEnumerator Run();
    }

    class TaskManager
    {
        public TaskManager(MonoBehaviour parent, int concurrent)
        {
            parent_ = parent;
            concurrent_ = concurrent;
        }

        public int pendingCount
        {
            get
            {
                return pending_.Count;
            }
        }

        public int runningCount
        {
            get
            {
                return running_.Count;
            }
        }

        public int count
        {
            get
            {
                return pending_.Count + running_.Count;
            }
        }

        public void Add(Task task)
        {
            pending_.Add(task);
        }

        public void Update()
        {
            int index = 0;

            while (index < running_.Count)
            {
                Task task = running_[index];

                if (task.status == TaskStatus.Finished)
                {
                    running_.RemoveAt(index);
                    finished_.Add(task);
                    continue;
                }
                else if (task.status == TaskStatus.Failed)
                {
                    running_.RemoveAt(index);
                    failed_.Add(task);
                    continue;
                }

                ++index;
            }

            while (running_.Count < concurrent_ && pending_.Count > 0)
            {
                Task task = pending_.First();

                parent_.StartCoroutine(task.Run());

                running_.Add(task);
                pending_.RemoveAt(0);
            }
        }

        public List<Task> GetFinished()
        {
            List<Task> finished = finished_.ToList();

            finished_.Clear();

            return finished;
        }

        public List<Task> GetFailed()
        {
            List<Task> failed = failed_.ToList();

            failed_.Clear();

            return failed;
        }

        MonoBehaviour parent_;
        int concurrent_;

        List<Task> pending_ = new List<Task>();
        List<Task> running_ = new List<Task>();
        List<Task> finished_ = new List<Task>();
        List<Task> failed_ = new List<Task>();
    }

    enum LoadLocation
    {
        Auto = 0,
        Initial = 1,
        Downloaded = 2,
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

            for (int i = 0; i < index.table.Length; ++i) {
                index.table[i] = sign.chunkHashes[i].digest;
            }

            Array.Sort(index.table);

            index.mapping = new int[index.table.Length][];

            for (int i = 0; i < index.table.Length; ++i)
            {
                List<int> v = new List<int>();

                for (int j = 0; j < sign.chunkHashes.Length; ++j)
                {
                    if (sign.chunkHashes[j].digest == index.table[i]) {
                        v.Add(j);
                    }
                }

                index.mapping[i] = v.ToArray();
            }

            return index;
        }
    }

    [Serializable]
    class Reusable
    {
        public Pair<int, int>[] chunks;
    }

    [Serializable]
    class Inequable
    {
        public int[] chunks;
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
    class UpdateInfo
    {
        public void SetLocalList(List<Pair<string, string>> localList)
        {
            localList_ = localList;
        }

        public bool VerifyLocalList(List<Pair<string, string>> localList)
        {
            return localList_.SequenceEqual(localList);
        }

        public void SetRemoteList(List<Pair<string, string>> remoteList)
        {
            remoteList_ = remoteList;
        }

        public bool VerifyRemoteList(List<Pair<string, string>> remoteList)
        {
            return remoteList_.SequenceEqual(remoteList);
        }

        public string FindInLocalList(string name)
        {
            if (localListMap_ == null)
            {
                localListMap_ = new Dictionary<string, string>();

                foreach (Pair<string, string> p in localList_) {
                    localListMap_.Add(p.first, p.second);
                }
            }

            string result = null;

            localListMap_.TryGetValue(name, out result);

            return result;
        }

        public string FindInRemoteList(string name)
        {
            if (remoteListMap_ == null)
            {
                remoteListMap_ = new Dictionary<string, string>();

                foreach (Pair<string, string> p in remoteList_) {
                    remoteListMap_.Add(p.first, p.second);
                }
            }

            string result = null;

            remoteListMap_.TryGetValue(name, out result);

            return result;
        }

        public int[] GetFileIds()
        {
            return fileNameMap_.Keys.OrderBy(p => p).ToArray();
        }

        public void RegisterFile(string name, int fileId)
        {
            fileIdMap_.Add(name, fileId);
            fileNameMap_.Add(fileId, name);
        }

        public int GetFileId(string name)
        {
            int fileId = -1;

            fileIdMap_.TryGetValue(name, out fileId);

            return fileId;
        }

        public string GetFileName(int fileId)
        {
            string name = null;

            fileNameMap_.TryGetValue(fileId, out name);

            return name;
        }

        public Signature GetSignature(int fileId)
        {
            Signature result = null;
            signMap_.TryGetValue(fileId, out result);
            return result;
        }

        public void RegisterSignature(int fileId, Signature sign)
        {
            signMap_.Add(fileId, sign);
        }

        public Reusable GetReusable(int fileId)
        {
            Reusable result = null;
            reusableMap_.TryGetValue(fileId, out result);
            return result;
        }

        public void RegisterReusable(int fileId, Reusable reusable)
        {
            reusableMap_.Add(fileId, reusable);
        }

        public Inequable GetInequable(int fileId)
        {
            Inequable result = null;
            inequableMap_.TryGetValue(fileId, out result);
            return result;
        }

        public void RegisterInequable(int fileId, Inequable inequable)
        {
            inequableMap_.Add(fileId, inequable);
        }

        public void RegisterRemovedFile(string name)
        {
            removedFileList_.Add(name);
        }

        public List<string> GetRemovedFileList()
        {
            return removedFileList_;
        }

        public string GetBuild()
        {
            return build_;
        }

        public void SetBuild(string build)
        {
            build_ = build;
        }

        Dictionary<string, int> fileIdMap_ = new Dictionary<string, int>();
        Dictionary<int, string> fileNameMap_ = new Dictionary<int, string>();
        List<Pair<string, string>> localList_ = new List<Pair<string, string>>();
        List<Pair<string, string>> remoteList_ = new List<Pair<string, string>>();
        Dictionary<int, Signature> signMap_ = new Dictionary<int, Signature>();
        Dictionary<int, Reusable> reusableMap_ = new Dictionary<int, Reusable>();
        Dictionary<int, Inequable> inequableMap_ = new Dictionary<int, Inequable>();
        List<string> removedFileList_ = new List<string>();
        string build_;

        [NonSerialized]
        Dictionary<string, string> localListMap_;
        [NonSerialized]
        Dictionary<string, string> remoteListMap_;
    }

    class ComputeFileHashTask : Task
    {
        public ComputeFileHashTask(FileSystem fs, FileLocator fl)
        {
            fs_ = fs;
            fl_ = fl;
        }

        public override IEnumerator Run()
        {
            Stream stream = Utility.OpenRead(fs_, fl_);

            byte[] buffer = new byte[65536];
            int length;

            MD5 md5 = MD5.Create();

            int count = 0;

            while ((length = Utility.ReadFile(stream, buffer, buffer.Length)) > 0)
            {
                md5.TransformBlock(buffer, 0, length, buffer, 0);

                if (++count % 10 == 0) {
                    yield return null;
                }
            }

            md5.TransformFinalBlock(buffer, 0, 0);

            stream.Close();

            result = md5.Hash;
            status = TaskStatus.Finished;
        }

        FileSystem fs_;
        FileLocator fl_;
    }

    class HttpRequestTask : Task
    {
        public HttpRequestTask(Http http, string url, int offset, int length)
        {
            http_ = http;
            url_ = url;
            offset_ = offset;
            length_ = length;
        }

        public override IEnumerator Run()
        {
            status = TaskStatus.Start;
            errorMessage = "";

            HttpRequest request = http_.Get(url_, offset_, length_);

            yield return request.Send();

            if (request.IsError())
            {
                errorMessage = request.ErrorMessage();
                status = TaskStatus.Failed;
            }
            else
            {
                result = request;
                status = TaskStatus.Finished;
            }
        }

        Http http_;
        string url_;
        int offset_;
        int length_;
    }

    class FindReusableChunksTask : Task
    {
        public FindReusableChunksTask(FileSystem fs, FileLocator fl, Signature sign)
        {
            fs_ = fs;
            fl_ = fl;
            sign_ = sign;
        }

        public override IEnumerator Run()
        {
            List<Pair<int, int>> list = new List<Pair<int, int>>();

            SignatureIndex signIdx = SignatureIndex.Create(sign_);

            Stream stream = Utility.OpenRead(fs_, fl_);

            byte[] buffer = new byte[Config.chunkSize];

            RollingChecksum rc = new RollingChecksum();
            MD5 md5 = MD5.Create();

            int i = 0;

            while (Utility.ReadFile(stream, buffer, Config.chunkSize) == Config.chunkSize)
            {
                rc.Update(buffer, Config.chunkSize);

                if (++i % 10 == 0) {
                    yield return null;
                }

                uint digest = rc.Digest();

                int indexOfDigest = Array.BinarySearch(signIdx.table, digest);

                int chunkIndex = -1;

                if (indexOfDigest >= 0)
                {
                    if ((chunkIndex = Match(sign_, signIdx, indexOfDigest, md5.ComputeHash(buffer))) >= 0)
                    {
                        list.Add(Utility.MakePair(chunkIndex, (int)(stream.Position - Config.chunkSize)));
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
                        yield return null;
                    }

                    indexOfDigest = Array.BinarySearch(signIdx.table, digest);

                    if (indexOfDigest >= 0)
                    {
                        md5.Initialize();
                        md5.TransformBlock(buffer, index, buffer.Length - index, null, 0);
                        md5.TransformFinalBlock(buffer, 0, index);

                        if ((chunkIndex = Match(sign_, signIdx, indexOfDigest, md5.Hash)) >= 0)
                        {
                            list.Add(Utility.MakePair(chunkIndex, (int)(stream.Position - Config.chunkSize)));
                            break;
                        }
                    }
                }
            }

            stream.Close();

            list.Sort((a, b) => a.first.CompareTo(b.first));

            Reusable reusable = new Reusable();
            reusable.chunks = list.ToArray();

            result = reusable;
            status = TaskStatus.Finished;
        }

        int Match(Signature sign, SignatureIndex index, int indexOfDigest, byte[] md5)
        {
            int[] list = index.mapping[indexOfDigest];

            for (int i = 0; i < list.Length; ++i)
            {
                if (md5.SequenceEqual(sign.chunkHashes[list[i]].md5)) {
                    return list[i];
                }
            }

            return -1;
        }

        FileSystem fs_;
        FileLocator fl_;
        Signature sign_;
    }

    public class UpdateManager : MonoBehaviour
    {
        public string errorMessage { get; private set; }

        public bool isDownloading { get; private set; }
        public bool isApplying { get; private set; }

        public float progress { get; private set; }

        public long bytesDownloaded { get; private set; }

        public int numberOfSuccessfulRequests { get; private set; }
        public int numberOfFailedRequests { get; private set; }

        public static UpdateManager Create(GameObject where, FileSystem fileSystem, Http http)
        {
            UpdateManager manager = where.AddComponent<UpdateManager>();

            manager.fileSystem_ = fileSystem;
            manager.http_ = http;

            manager.isDownloading = false;
            manager.isApplying = false;
            manager.progress = 0.0f;

            manager.bytesDownloaded = 0;

            manager.numberOfSuccessfulRequests = 0;
            manager.numberOfFailedRequests = 0;

            return manager;
        }

        public void ClearUpdate()
        {
            Utility.DeleteObject(fileSystem_, infoFileLocator_);
            fileSystem_.Delete(dataFileLocator_.location, dataFileLocator_.filename);
        }

        public void ClearDownload(bool overridedOnly)
        {
            List<string> list = FindFiles(LoadLocation.Downloaded);

            if (overridedOnly)
            {
                List<string> list2 = FindFiles(LoadLocation.Initial);

                list = new List<string>(list.Intersect(list2));
            }

            for (int i = 0; i < list.Count; ++i)
            {
                FileDeleteDownload(list[i]);
            }

            fileSystem_.Delete(buildFileLocator_.location, buildFileLocator_.filename);
        }

        public bool HasUnfinishedApply()
        {
            return Utility.Exists(fileSystem_, applyFileLocator_);
        }

        public void StartDownloadUpdate(string url, int concurrent)
        {
            errorMessage = "";
            progress = 0.0f;
            isDownloading = true;
            bytesDownloaded = 0;
            numberOfSuccessfulRequests = 0;
            numberOfFailedRequests = 0;

            StartCoroutine(DownloadUpdate(url, concurrent));
        }

        public void StartApplyUpdate(bool verify)
        {
            errorMessage = "";
            progress = 0.0f;
            isApplying = true;

            StartCoroutine(ApplyUpdate(verify));
        }

        IEnumerator ApplyUpdate(bool verify)
        {
            UpdateInfo info = Utility.LoadObjectFromFile(fileSystem_, infoFileLocator_) as UpdateInfo;

            if (info == null)
            {
                errorMessage = "bad update info";
                isApplying = false;
                yield break;
            }

            if (verify)
            {
                OutPara<bool> result = new OutPara<bool>();

                yield return VerifyDownloadedDataFile(info, dataFileLocator_, result);

                if (!result.para)
                {
                    errorMessage = "bad update data";
                    isApplying = false;
                    yield break;
                }
            }

            Utility.Create(fileSystem_, applyFileLocator_).Close();

            Stream dstream = Utility.OpenRead(fileSystem_, dataFileLocator_);

            byte[] buffer = new byte[Config.chunkSize];

            int[] fileIdList = info.GetFileIds();

            for (int i = 0; i < fileIdList.Length; ++i)
            {
                int fileId = fileIdList[i];

                Inequable inequable = info.GetInequable(fileId);

                if (inequable == null) {
                    continue;
                }

                Reusable reusable = info.GetReusable(fileId);

                string name = info.GetFileName(fileId);
                string temp = name + ".tmp";

                if (verify)
                {
                    string remoteSign = info.FindInRemoteList(name);

                    OutPara<string> fileHash = new OutPara<string>();

                    yield return GetFileHashString(name, LoadLocation.Downloaded, fileHash);

                    if (fileHash.para != null && fileHash.para == remoteSign)
                    {
                        dstream.Seek(Config.chunkSize * inequable.chunks.Length, SeekOrigin.Current);
                        FileDeleteDownload(temp);
                        continue;
                    }

                    OutPara<string> tempSign = new OutPara<string>();

                    yield return GetFileHashString(temp, LoadLocation.Downloaded, tempSign);

                    if (tempSign.para != null && tempSign.para == remoteSign)
                    {
                        dstream.Seek(Config.chunkSize * inequable.chunks.Length, SeekOrigin.Current);
                        FileDuplicateDownload(temp, LoadLocation.Downloaded, name);
                        FileDeleteDownload(temp);
                        continue;
                    }

                    if (reusable != null)
                    {
                        string localSign = info.FindInLocalList(name);

                        yield return GetFileHashString(name, LoadLocation.Auto, fileHash);

                        if (fileHash.para == null || fileHash.para != localSign)
                        {
                            errorMessage = "unmatched local file";
                            isApplying = false;
                            yield break;
                        }
                    }
                }

                Signature sign = info.GetSignature(fileId);
                Stream stream = FileCreateDownload(temp);

                int j = 0;

                foreach (int chunk in inequable.chunks)
                {
                    dstream.Read(buffer, 0, Config.chunkSize);

                    stream.Seek(chunk * Config.chunkSize, SeekOrigin.Begin);

                    if (chunk == sign.chunkHashes.Length - 1) {
                        stream.Write(buffer, 0, sign.length % Config.chunkSize);
                    }
                    else {
                        stream.Write(buffer, 0, Config.chunkSize);
                    }

                    if (++j % 10 == 0) {
                        yield return null;
                    }
                }

                if (reusable != null)
                {
                    Stream rstream = FileOpenRead(name, LoadLocation.Auto);

                    foreach (Pair<int, int> p in reusable.chunks)
                    {
                        rstream.Seek(p.second, SeekOrigin.Begin);
                        rstream.Read(buffer, 0, Config.chunkSize);
                        stream.Seek(p.first * Config.chunkSize, SeekOrigin.Begin);
                        stream.Write(buffer, 0, Config.chunkSize);

                        if (++j % 10 == 0) {
                            yield return null;
                        }
                    }

                    rstream.Close();
                }

                stream.Close();

                FileDuplicateDownload(temp, LoadLocation.Downloaded, name);
                FileDeleteDownload(temp);

                progress = Utility.CalculateProgress(0.0f, 0.9f, i + 1, fileIdList.Length);
                yield return null;
            }

            dstream.Close();

            foreach (string name in info.GetRemovedFileList())
            {
                FileDeleteDownload(name);
            }

            Utility.WriteAllText(fileSystem_, buildFileLocator_, info.GetBuild());

            fileSystem_.Delete(applyFileLocator_.location, applyFileLocator_.filename);

            progress = 1.0f;
            isApplying = false;

            yield return null;
        }

        IEnumerator DownloadUpdate(string url, int concurrent)
        {
            if (url.EndsWith("/")) {
                url = url.Remove(url.Length - 1);
            }

            TaskManager taskManager = new TaskManager(this, concurrent);

            OutPara<HttpRequest> req = new OutPara<HttpRequest>();

            yield return HttpGet(http_, url + "/_list", req);

            List<Pair<string, string>> localList = new List<Pair<string, string>>();

            yield return GenerateList(taskManager, localList);

            List<Pair<string, string>> remoteList = ParseListFromText(req.para.Text());

            bool resume = false;

            UpdateInfo info = Utility.LoadObjectFromFile(fileSystem_, infoFileLocator_) as UpdateInfo;

            if (info != null)
            {
                if (!info.VerifyLocalList(localList) || !info.VerifyRemoteList(remoteList)) {
                    info = null;
                }
                else {
                    resume = true;
                }
            }

            if (info == null)
            {
                info = new UpdateInfo();

                info.SetLocalList(localList);
                info.SetRemoteList(remoteList);

                for (int i = 0; i < remoteList.Count; ++i)
                {
                    info.RegisterFile(remoteList[i].first, i);
                }

                foreach (Pair<string, string> pair in remoteList.Except(localList))
                {
                    yield return HttpGet(http_, url + "/" + pair.first + ".signature", req);

                    int fileId = info.GetFileId(pair.first);

                    Signature sign = ReadSignature(req.para.Data());

                    info.RegisterSignature(fileId, sign);

                    FileLocator fileLocator;

                    if (FileLocate(pair.first, LoadLocation.Auto, out fileLocator))
                    {
                        Task task = new FindReusableChunksTask(fileSystem_, fileLocator, sign);

                        task.userData = Utility.MakePair(fileId, sign.chunkHashes.Length);

                        taskManager.Add(task);
                    }
                    else
                    {
                        Inequable inequable = new Inequable();

                        inequable.chunks = new int[sign.chunkHashes.Length];

                        for (int i = 0; i < sign.chunkHashes.Length; ++i) {
                            inequable.chunks[i] = i;
                        }

                        info.RegisterInequable(fileId, inequable);
                    }
                }

                while (taskManager.count > 0)
                {
                    taskManager.Update();
                    yield return null;
                }

                foreach (Task task in taskManager.GetFinished())
                {
                    Pair<int, int> p = (Pair<int, int>)task.userData;

                    Inequable inequable = FindInequableChunks((Reusable)task.result, p.second);

                    info.RegisterReusable(p.first, (Reusable)task.result);
                    info.RegisterInequable(p.first, inequable);
                }

                foreach (Pair<string, string> pair in localList)
                {
                    if (FileExists(pair.first, LoadLocation.Downloaded) &&
                        info.FindInRemoteList(pair.first) == null)
                    {
                        info.RegisterRemovedFile(pair.first);
                    }
                }

                yield return HttpGet(http_, url + "/_build", req);

                info.SetBuild(req.para.Text());

                Utility.SaveObjectToFile(fileSystem_, infoFileLocator_, info);
            }

            List<Pair<int, int>> chunkList = CreateDownloadChunkList(info);

            int imax = chunkList.Count;
            int finished = 0;

            Stream stream = null;

            if (resume && Utility.Exists(fileSystem_, dataFileLocator_))
            {
                stream = fileSystem_.Open(dataFileLocator_.location, dataFileLocator_.filename,
                    FileMode.Open, FileAccess.ReadWrite);

                OutPara<int> count = new OutPara<int>();

                yield return VerifyDownloadedData(info, stream, chunkList, count);

                chunkList.RemoveRange(0, count.para);
                finished += count.para;

                progress = Utility.CalculateProgress(0.0f, 1.0f, imax - chunkList.Count, imax);
                yield return null;
            }
            else
            {
                stream = Utility.Create(fileSystem_, dataFileLocator_);
            }

            int lastFileId = -1;
            string lastFileName = null;
            Signature lastSign = null;

            long position = finished * Config.chunkSize;

            byte[] padding = new byte[Config.chunkSize];

            while (finished < imax)
            {
                if (taskManager.pendingCount < concurrent && chunkList.Count > 0)
                {
                    Pair<int, int> p = chunkList.First();

                    int fileId = p.first;
                    int chunk = p.second;

                    if (fileId != lastFileId)
                    {
                        lastFileName = info.GetFileName(p.first);
                        lastSign = info.GetSignature(fileId);
                        lastFileId = fileId;
                    }

                    int offset = Config.chunkSize * chunk;
                    int length = Config.chunkSize;

                    if (chunk == lastSign.chunkHashes.Length - 1) {
                        length = lastSign.length % Config.chunkSize;
                    }

                    Task task = new HttpRequestTask(http_, url + "/" + lastFileName, offset, length);

                    task.userData = position;

                    taskManager.Add(task);

                    position += Config.chunkSize;

                    chunkList.RemoveAt(0);

                    continue;
                }

                foreach (Task task in taskManager.GetFinished())
                {
                    stream.Seek((long)task.userData, SeekOrigin.Begin);

                    byte[] data = ((HttpRequest)task.result).Data();

                    stream.Write(data, 0, data.Length);

                    if (data.Length < Config.chunkSize) {
                        stream.Write(padding, 0, Config.chunkSize - data.Length);
                    }

                    bytesDownloaded += data.Length;

                    ++numberOfSuccessfulRequests;
                    ++finished;
                }

                foreach (Task task in taskManager.GetFailed())
                {
                    Debug.LogWarning(task.errorMessage);

                    taskManager.Add(task);

                    ++numberOfFailedRequests;
                }

                taskManager.Update();

                progress = Utility.CalculateProgress(0.0f, 1.0f, finished, imax);
                yield return null;
            }

            stream.Flush();
            stream.Close();

            progress = 1.0f;
            isDownloading = false;
        }

        IEnumerator HttpGet(Http http, string url, OutPara<HttpRequest> req)
        {
            for (;;)
            {
                req.para = http.Get(url);

                yield return req.para.Send();

                if (req.para.IsError())
                {
                    ++numberOfFailedRequests;
                    Debug.LogWarning(req.para.ErrorMessage());
                    continue;
                }

                ++numberOfSuccessfulRequests;
                yield break;
            }
        }

        IEnumerator GetFileHashString(string name, LoadLocation dir, OutPara<string> hash)
        {
            FileLocator fl = new FileLocator(Location.Initial, name);
            FileLocator fl2 = new FileLocator(Location.Download, name);

            if (dir == LoadLocation.Initial)
            {
                if (Utility.Exists(fileSystem_, fl)) {
                    yield return GetFileHashString(fl, hash);
                }
            }
            else if (dir == LoadLocation.Downloaded)
            {
                if (Utility.Exists(fileSystem_, fl2)) {
                    yield return GetFileHashString(fl2, hash);
                }
            }
            else
            {
                if (Utility.Exists(fileSystem_, fl2)) {
                    yield return GetFileHashString(fl2, hash);
                }
                else if (Utility.Exists(fileSystem_, fl)) {
                    yield return GetFileHashString(fl, hash);
                }
            }
        }

        IEnumerator GetFileHashString(FileLocator fl, OutPara<string> hash)
        {
            Task task = new ComputeFileHashTask(fileSystem_, fl);

            yield return task.Run();

            hash.para = Utility.ToHexString((byte[])task.result);
        }

        IEnumerator VerifyDownloadedDataFile(UpdateInfo info, FileLocator dataFileLocator, OutPara<bool> result)
        {
            if (Utility.Exists(fileSystem_, dataFileLocator))
            {
                Stream stream = Utility.OpenRead(fileSystem_, dataFileLocator);

                List<Pair<int, int>> chunkList = CreateDownloadChunkList(info);

                OutPara<int> count = new OutPara<int>();

                yield return VerifyDownloadedData(info, stream, chunkList, count);

                stream.Close();

                result.para = (count.para == chunkList.Count);
            }
            else
            {
                result.para = false;
            }
        }

        IEnumerator VerifyDownloadedData(UpdateInfo info, Stream stream, List<Pair<int, int>> downloadChunkList, OutPara<int> count)
        {
            List<Pair<int, ChunkHash>> hashList = CreateDownloadChunkHashList(info, downloadChunkList);

            RollingChecksum rc = new RollingChecksum();

            byte[] buffer = new byte[Config.chunkSize];

            count.para = 0;

            for (int i = 0; i < downloadChunkList.Count; ++i)
            {
                if (Utility.ReadFile(stream, buffer, Config.chunkSize) != Config.chunkSize)
                {
                    stream.Seek(count.para * Config.chunkSize, SeekOrigin.Begin);
                    yield break;
                }

                int length = hashList[i].first;
                uint digest = hashList[i].second.digest;

                rc.Update(buffer, length);

                if (rc.Digest() != digest)
                {
                    stream.Seek(count.para * Config.chunkSize, SeekOrigin.Begin);
                    yield break;
                }

                ++count.para;

                if (i + 1 % 10 == 0) {
                    yield return null;
                }
            }
        }

        List<Pair<int, int>> CreateDownloadChunkList(UpdateInfo info)
        {
            List<Pair<int, int>> result = new List<Pair<int, int>>();

            foreach (int fileId in info.GetFileIds())
            {
                Inequable inequable = info.GetInequable(fileId);

                if (inequable != null)
                {
                    foreach (int chunk in inequable.chunks)
                    {
                        result.Add(Utility.MakePair(fileId, chunk));
                    }
                }
            }

            return result;
        }

        List<Pair<int, ChunkHash>> CreateDownloadChunkHashList(UpdateInfo info, List<Pair<int, int>> downloadChunkList)
        {
            List<Pair<int, ChunkHash>> result = new List<Pair<int, ChunkHash>>();

            int lastFileId = -1;
            Signature lastSign = null;

            foreach (Pair<int, int> p in downloadChunkList)
            {
                int fileId = p.first;
                int chunk = p.second;

                if (fileId != lastFileId)
                {
                    lastSign = info.GetSignature(fileId);
                    lastFileId = fileId;
                }

                int length = Config.chunkSize;

                if (chunk == lastSign.chunkHashes.Length - 1) {
                    length = lastSign.length % Config.chunkSize;
                }

                result.Add(Utility.MakePair(length, lastSign.chunkHashes[chunk]));
            }

            return result;
        }

        Inequable FindInequableChunks(Reusable reusable, int numOfChunks)
        {
            List<int> list = new List<int>();

            int lastChunk = 0;

            for (int i = 0; i < reusable.chunks.Length; ++i)
            {
                int currentChunk = reusable.chunks[i].first;

                for (int j = lastChunk; j < currentChunk; ++j) {
                    list.Add(j);
                }

                lastChunk = currentChunk + 1;
            }

            for (int j = lastChunk; j < numOfChunks; ++j)
            {
                list.Add(j);
            }

            Inequable result = new Inequable();

            result.chunks = list.ToArray();

            return result;
        }

        Signature ReadSignature(byte[] buffer)
        {
            Signature sign = new Signature();

            int index = 0;

            sign.length = Utility.ReadInt(buffer, index);
            index += 4;

            int count = sign.length / Config.chunkSize;

            if (sign.length % Config.chunkSize != 0) {
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

        IEnumerator GenerateList(TaskManager taskManager, List<Pair<string, string>> result)
        {
            List<string> files = FindFiles(LoadLocation.Auto);

            yield return null;

            for (int i = 0; i < files.Count; ++i)
            {
                FileLocator fl;
                FileLocate(files[i], LoadLocation.Auto, out fl);

                Task task = new ComputeFileHashTask(fileSystem_, fl);

                task.userData = files[i];

                taskManager.Add(task);
            }

            while (taskManager.count > 0)
            {
                taskManager.Update();
                yield return null;
            }

            List<Pair<string, string>> list = new List<Pair<string, string>>();

            foreach (Task task in taskManager.GetFinished())
            {
                list.Add(Utility.MakePair((string)task.userData,
                    Utility.ToHexString((byte[])task.result)));
            }

            foreach (Pair<string, string> p in list.OrderBy(p => p.first))
            {
                result.Add(p);
            }
        }

        bool FileLocate(string name, LoadLocation dir, out FileLocator fl)
        {
            if (dir == LoadLocation.Downloaded)
            {
                if (fileSystem_.Exists(Location.Download, name))
                {
                    fl = new FileLocator(Location.Download, name);
                    return true;
                }
            }
            else if (dir == LoadLocation.Initial)
            {
                if (fileSystem_.Exists(Location.Initial, name))
                {
                    fl = new FileLocator(Location.Initial, name);
                    return true;
                }
            }
            else if (fileSystem_.Exists(Location.Download, name))
            {
                fl = new FileLocator(Location.Download, name);
                return true;
            }
            else if (fileSystem_.Exists(Location.Initial, name))
            {
                fl = new FileLocator(Location.Initial, name);
                return true;
            }

            fl = null;
            return false;
        }

        bool FileExists(string name, LoadLocation dir)
        {
            FileLocator fl;
            return FileLocate(name, dir, out fl);
        }

        Stream FileOpenRead(string name, LoadLocation dir)
        {
            FileLocator fl;

            if (FileLocate(name, dir, out fl))
            {
                return Utility.OpenRead(fileSystem_, fl);
            }

            return null;
        }

        Stream FileCreateDownload(string name)
        {
            return Utility.Create(fileSystem_, new FileLocator(Location.Download, name));
        }

        void FileDuplicateDownload(string name, LoadLocation dir, string name2)
        {
            FileLocator fl;

            if (FileLocate(name, dir, out fl)) {
                fileSystem_.Copy(fl.location, fl.filename, Location.Download, name2);
            }
        }

        void FileDeleteDownload(string name)
        {
            if (fileSystem_.Exists(Location.Download, name)) {
                fileSystem_.Delete(Location.Download, name);
            }
        }

        List<string> FindFiles(LoadLocation dir)
        {
            Location location;

            if (dir == LoadLocation.Initial)
            {
                location = Location.Initial;
            }
            else if (dir == LoadLocation.Downloaded)
            {
                location = Location.Download;
            }
            else
            {
                List<string> list1 = FindFiles(LoadLocation.Initial);
                List<string> list2 = FindFiles(LoadLocation.Downloaded);

                return new List<string>(list1.Union(list2));
            }

            List<string> list = new List<string>();

            foreach (string filename in fileSystem_.GetFiles(location))
            {
                if (filename == "_build") {
                    continue;
                }

                list.Add(filename);
            }

            return list;
        }

        List<Pair<string, string>> ParseListFromFile(string filename)
        {
            return ParseListFromText(File.ReadAllText(filename));
        }

        List<Pair<string, string>> ParseListFromText(string text)
        {
            List<Pair<string, string>> result = new List<Pair<string, string>>();

            StringReader reader = new StringReader(text);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] v = line.Split(' ');
                result.Add(Utility.MakePair(v[0], v[1]));
            }

            return result;
        }

        FileSystem fileSystem_;
        Http http_;

        FileLocator infoFileLocator_ = new FileLocator(Location.Update, "info");
        FileLocator dataFileLocator_ = new FileLocator(Location.Update, "data");
        FileLocator applyFileLocator_ = new FileLocator(Location.Download, ".apply");
        FileLocator buildFileLocator_ = new FileLocator(Location.Download, "_build");
    }

    public class BuildDatabase
    {
        public BuildDatabase(FileSystem fileSystem)
        {
            fileSystem_ = fileSystem;
        }

        public bool LocateBundle(string bundleName, out Location location)
        {
            if (fileSystem_.Exists(Location.Download, bundleName))
            {
                location = Location.Download;
                return true;
            }

            if (fileSystem_.Exists(Location.Initial, bundleName))
            {
                location = Location.Initial;
                return true;
            }

            location = Location.Download;
            return false;
        }

        public string GetBundleName(string assetPath)
        {
            EnsureLoaded();

            string result;

            assetPathToBundleName_.TryGetValue(assetPath.Replace('\\', '/'), out result);

            return result;
        }

        public string[] GetAssetPaths(string bundleName)
        {
            EnsureLoaded();

            string[] result;

            if (bundleNameToAssets_.TryGetValue(bundleName, out result)) {
                return result;
            }
            else {
                return new string[0];
            }
        }

        public string[] GetAllBundleNames()
        {
            EnsureLoaded();

            return bundleNameToDependencies_.Keys.ToArray();
        }

        public string[] GetBundleDependencies(string bundleName, bool recursive)
        {
            EnsureLoaded();

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
                string[] result;

                if (bundleNameToDependencies_.TryGetValue(bundleName, out result)) {
                    return result;
                }
                else {
                    return new string[0];
                }
            }
        }

        void EnsureLoaded()
        {
            if (assetPathToBundleName_ == null) {
                LoadBuild();
            }
        }

        void LoadBuild()
        {
            FileLocator fl1 = new FileLocator(Location.Initial, "_build");
            FileLocator fl2 = new FileLocator(Location.Download, "_build");

            TextReader reader = null;

            if (Utility.Exists(fileSystem_, fl2))
            {
                reader = Utility.OpenText(fileSystem_, fl2);
            }
            else if (Utility.Exists(fileSystem_, fl1))
            {
                reader = Utility.OpenText(fileSystem_, fl1);
            }

            if (reader == null)
            {
                Debug.LogError("Failed to load build data file, please ensure the file exists and the path is correct.");
                return;
            }

            assetPathToBundleName_ = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bundleNameToDependencies_ = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            bundleNameToAssets_ = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            string name = null;
            List<string> assets = new List<string>();

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length == 0)
                {
                    if (name != null)
                    {
                        bundleNameToAssets_.Add(name, assets.ToArray());
                    }

                    name = null;
                    assets.Clear();
                    continue;
                }

                if (line[0] != ' ')
                {
                    string[] result = line.Split(':');

                    if (result.Length == 2)
                    {
                        bundleNameToDependencies_.Add(result[0], result[1].Split(','));
                    }

                    name = result[0];
                }
                else if (name != null)
                {
                    line = line.Replace('\\', '/').Trim();
                    assets.Add(line);
                    assetPathToBundleName_[line] = name;
                }
            }

            reader.Close();
        }

        FileSystem fileSystem_;

        Dictionary<string, string> assetPathToBundleName_;
        Dictionary<string, string[]> bundleNameToDependencies_;
        Dictionary<string, string[]> bundleNameToAssets_;
    }
}

