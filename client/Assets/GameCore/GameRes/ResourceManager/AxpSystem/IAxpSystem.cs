using System.Collections.Generic;
namespace AxpTools
{
    public enum AxpFilePath
    {
        PersistentDataPath,
        StreamingAssetsPath,
        AndroidLibPath,
    }
    public abstract class IAxpSystem
    {
        static IAxpSystem()
        {

        }
        public abstract bool Initial();
        public abstract bool openFile(string strFileName, out string axpFileName, out AxpFilePath axpFilePath, out uint dataSize, out uint offset);
        public abstract byte[] openFileByBuffer(string strFileName, out string axpFileName, out AxpFilePath axpFilePath, out uint dataSize, out uint offset);
        public abstract byte[] openFileByBuffer(string strFileName, uint offset,uint length, bool lockFile);
        public abstract byte[] openFileByBuffer(string strFileName, bool lockFile);
        public abstract int getFileOffset(string strFileName, out string axpFileName, out string axpFileFullName, out AxpFilePath axpFilePath);
        public abstract bool insertFileInAxp(string[] files);
        public abstract bool insertFileInAxpByStream(List<string> fileKeys, List<string> fileStreams);
        public abstract bool Exists(string fileName);
        public abstract int MemoryInfo(string fileName);
        //private abstract bool insertFileInAxp(string strPakFilePathName, string strPakFileName, uint nMaxAxpFileSize);
        //private abstract bool savePakFile(string strPakFilePathName, string strPakFileName, Dictionary<string, string> t_AllFiles);
    }
}