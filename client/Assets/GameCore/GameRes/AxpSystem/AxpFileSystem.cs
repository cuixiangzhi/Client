#if UNITY_IPHONE || UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using GameCore;


/**************************************************************
 * 
 * 注意：
 *      (1) Axp在IOS所有目录下的后缀为.axp.在Android 平台下/data/data/com.tencent.tmgp.tstl/lib/
 *          目录下为.so文件。在Android Persistent data path 下为.axp 
 *      (2) 
 * **************************************************************/




namespace AxpTools
{
    public class AxpFileSystem : IAxpSystem
    {
        private AxpMap m_AxpMap;
        public uint m_AxpFileThresholdSize = 0;// Read From AxpMap

        public AxpFileSystem()
        {
        }

        public override bool Initial()
        {

            Games.TLBB.Log.LogSystem.Info("Initial AxpFileSystem..");

            float time = Time.realtimeSinceStartup;
            m_AxpMap = AxpMap.Init();   //初始化AxpMap
#if UNITY_EDITOR
            if (!File.Exists(GameInfo.AxpPath + AxpMap.MAPNAME))
            {
                if (!File.Exists(GameInfo.AxpPath + AxpMap.MAPNAME) && File.Exists(Application.streamingAssetsPath + "/" + AxpMap.MAPNAME))
                    File.Copy(Application.streamingAssetsPath + "/" + AxpMap.MAPNAME, GameInfo.AxpPath + AxpMap.MAPNAME, true);
                else
                    return false;
            }
#else
            if (!File.Exists(GameInfo.AxpPath + AxpMap.MAPNAME))
            {
#if UNITY_ANDROID && !UNITY_EDITOR
            #region findDb = File.Exists(AxpMap.MAPNAME)
                string[] t_fileNameList = AxpFileStream.assetManager.Call<string[]>("list", "");
                bool findDb = false;
                for (int i = 0; i < t_fileNameList.Length; i++)
                {
                    if (t_fileNameList[i] == AxpMap.MAPNAME)
                    {
                        findDb = true;
                        break;
                    }
                }
            #endregion
                if (findDb)
                {
                    AxpFileStream stream = AxpFileStream.OpenFile(AxpMap.MAPNAME, true, true);
                    if (stream == null)
                        return false;
                    byte[] data = stream.ReadBytes(stream.FileLength);
                    if (data.Length <= 0)
                        return false;
                    File.WriteAllBytes(GameInfo.AxpPath + AxpMap.MAPNAME, data);
                    stream.Close();
                }

#elif UNITY_IOS || UNITY_STANDALONE
                if (File.Exists(Application.streamingAssetsPath + "/" + AxpMap.MAPNAME))
                {
                    File.Copy(Application.streamingAssetsPath + "/" + AxpMap.MAPNAME, GameInfo.AxpPath + AxpMap.MAPNAME);
                }
#endif
                else
                {
                    return false;
                }

            }
#endif
            if (!m_AxpMap.Load())
                return false;

#if !UNITY_EDITOR
            if(!m_AxpMap.CompareAxpMapInPersistentAndStreamingAssets())
            {
//                 Games.TLBB.Log.LogSystem.Error("AxpMap版本对比不一致, 不是同一版本，请卸载再重新安装");
//                 compareVersion = false;
//                 MessageBoxProxy.Show("安装失败！请卸载客户端后，再重新安装", (param) =>
//                 {
//                      Games.TLBB.Util.GameUtil.SafeQuitGame();
//                 }, null, (param) =>
//                 {
//                     Games.TLBB.Util.GameUtil.SafeQuitGame();
//                 } , null);
                return false;
            }
            
#endif
            Games.TLBB.Log.LogSystem.Info("AxpMap初始化完毕，实例化时间：" + (Time.realtimeSinceStartup - time));
            m_AxpFileThresholdSize = m_AxpMap.AxpFileMaxSize;
#if UNITY_ANDROID && !UNITY_EDITOR
//             DirectoryInfo t_LibDir = new DirectoryInfo("/data/data/com.tencent.tmgp.tstl/lib/");
// 
//             if (t_LibDir != null)
//             {
//                 FileInfo[] t_libfileinfo = t_LibDir.GetFiles();
// 
//                 for (int index = 0; index < t_libfileinfo.Length; index++)
//                 {
//                     Games.TLBB.Log.LogSystem.Info(t_libfileinfo[index].Name);
// 
//                     if (t_libfileinfo[index].Name.EndsWith(".axp.so"))
//                     {
//                         Games.TLBB.Log.LogSystem.Info("libs directory File :" + t_libfileinfo[index].Name);
// 
//                         AxpFile t_AxpFile = new AxpFile();
//                         t_AxpFile.openPakFile("/data/data/com.tencent.tmgp.tstl/lib/" + t_libfileinfo[index].Name, AxpFilePath.AndroidLibPath);
//                         t_AxpFile.m_strFileName = t_libfileinfo[index].Name;
//                         m_AxpMap.RegisterAxpFile(t_AxpFile.id, t_AxpFile); //注册AXP信息
//                     }
//                 }
//             }
//             else
//             {
//                 Games.TLBB.Log.LogSystem.Info("Can't find lib .so files.");
//             }

            //first load streaming assets directory's axp file.
            if (AxpFileStream.assetManager != null)
            {
                string[] t_fileNameList = AxpFileStream.assetManager.Call<string[]>("list", "");
                if (t_fileNameList.Length == 0)
                {
                    Games.TLBB.Log.LogSystem.Info("lib.so files is null.....");
                    return false;
                }
                Games.TLBB.Log.LogSystem.Info("libs files size : {0}", t_fileNameList.Length);
                for (int index = 0; index < t_fileNameList.Length; index++)
                {
                    if (t_fileNameList[index].EndsWith(".axp.so"))
                    {
                        Games.TLBB.Log.LogSystem.Info("StreamingAssets File :" + t_fileNameList[index]);

                        AxpFile t_AxpFile = new AxpFile();
                        t_AxpFile.openPakFile(t_fileNameList[index], AxpFilePath.StreamingAssetsPath);
                        t_AxpFile.m_strFileName = t_fileNameList[index];
                        m_AxpMap.RegisterAxpFile(t_AxpFile.id, t_AxpFile); //注册AXP信息
                    }
                }
            }
#elif UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR

            DirectoryInfo t_StreamingAssetsDir = new DirectoryInfo(Application.streamingAssetsPath);
            FileInfo[] t_StreamingAssetFolderFiles = t_StreamingAssetsDir.GetFiles();

            for (int index = 0; index < t_StreamingAssetFolderFiles.Length; index++)
            {
                if (t_StreamingAssetFolderFiles[index].Name.EndsWith(".axp.so"))
                {
                    Games.TLBB.Log.LogSystem.Info("StreamingAssets File :" + t_StreamingAssetFolderFiles[index]);

                    AxpFile t_AxpFile = new AxpFile();
                    t_AxpFile.openPakFile(Application.streamingAssetsPath + "/" + t_StreamingAssetFolderFiles[index].Name,AxpFilePath.StreamingAssetsPath);
                    t_AxpFile.m_strFileName = t_StreamingAssetFolderFiles[index].Name;
                    m_AxpMap.RegisterAxpFile(t_AxpFile.id, t_AxpFile); //注册AXP信息
                }
            }

#endif
            Games.TLBB.Log.LogSystem.Info("Collect StreamingAssets axp files success.");
            //second load persistent data path directory's axp file.

            DirectoryInfo t_TempDir = new DirectoryInfo(GameInfo.AxpPath);
            FileInfo[] t_basefileinfo = t_TempDir.GetFiles();

            for (int index = 0; index < t_basefileinfo.Length; index++)
            {

                if (t_basefileinfo[index].Name.Contains(".axp.so"))
                {
                    Games.TLBB.Log.LogSystem.Info("StreamingAssets File :" + t_basefileinfo[index].Name);

                    AxpFile t_AxpFile = new AxpFile();
                    string t_filename = t_basefileinfo[index].Name;

                    Games.TLBB.Log.LogSystem.Info("Collect Persistent data path file name : {0}", t_filename);

                    if (!t_AxpFile.openPakFile(GameInfo.AxpPath + t_filename, AxpFilePath.PersistentDataPath))
                    {
                        Games.TLBB.Log.LogSystem.Info("Open Persistent data path file name: {0} failed.", t_filename);
                    }

                    t_AxpFile.m_strFileName = t_filename;
                    m_AxpMap.RegisterAxpFile(t_AxpFile.id, t_AxpFile);
                }
            }

            Games.TLBB.Log.LogSystem.Info("Collect Persistent data path axp files success.");
            if (m_AxpMap.Check())
                return true;
            else
                return false;

        }
        public override bool Exists(string fileName)
        {
            return m_AxpMap.Exists(fileName);
        }
        public override int MemoryInfo(string fileName)
        {
            return 0;
        }
        public override bool openFile(string strFileName, out string axpFileName, out AxpFilePath axpFilePath, out uint dataSize, out uint offset)
        {
            axpFileName = null;
            axpFilePath = AxpFilePath.PersistentDataPath;
            dataSize = offset = 0;

            float time = Time.realtimeSinceStartup;
            int blockNodeID;
            AxpFile axp = m_AxpMap.GetAxpFileByFileName(strFileName, out blockNodeID);
            if (axp != null)
            {
                if (axp.openFile(blockNodeID, out dataSize, out offset))
                {
                    axpFileName = axp.m_strFileName;
                    axpFilePath = axp.axpFilePath;
                    Games.TLBB.Log.LogSystem.Info("查找文件" + strFileName + "成功，耗时：" + (Time.realtimeSinceStartup - time));
                    return true;
                }
            }
            Games.TLBB.Log.LogSystem.Error("查找文件" + strFileName + "失败，耗时：" + (Time.realtimeSinceStartup - time));
            axpFileName = "";
            return false;
        }
        public override int getFileOffset(string strFileName, out string axpFileName, out string axpFileFullName, out AxpFilePath axpFilePath)
        {
            axpFileName = null;
            axpFileFullName = null;
            axpFilePath = AxpFilePath.PersistentDataPath;
            int blockNodeID;
            AxpFile axp = m_AxpMap.GetAxpFileByFileName(strFileName, out blockNodeID);
            if (axp != null && blockNodeID != -1)
            {
                axpFileName = axp.m_strFileName;
                axpFileFullName = axp.m_strPathFileName;
                axpFilePath = axp.axpFilePath;
                return axp.GetFileOffset(blockNodeID);
            }
            else
            {
                return -1;
            }
        }

        public override byte[] openFileByBuffer(string strFileName, out string axpFileName, out AxpFilePath axpFilePath, out uint dataSize, out uint offset)
        {
            axpFileName = null;
            axpFilePath = AxpFilePath.PersistentDataPath;
            dataSize = offset = 0;

            byte[] retByte = null;
            int blockNodeID;
            AxpFile axp = m_AxpMap.GetAxpFileByFileName(strFileName, out blockNodeID);
            if (axp != null && blockNodeID != -1)
            {
                retByte = axp.openFileByBuffer(blockNodeID, out dataSize, out offset);
                if (retByte != null)
                {
                    axpFileName = axp.m_strFileName;
                    axpFilePath = axp.axpFilePath;
                    return retByte;
                }
            }
            return retByte;
        }

        public override byte[] openFileByBuffer(string strFileName, uint offset, uint dataSize, bool lockFile)
        {
            byte[] retByte = null;
            int blockNodeID;
            AxpFile axp = m_AxpMap.GetAxpFileByFileName(strFileName, out blockNodeID);
            if (axp != null && blockNodeID != -1)
            {
                retByte = axp.openFileByBuffer(blockNodeID, offset, dataSize);
            }
            return retByte;
        }
        public override byte[] openFileByBuffer(string strFileName, bool lockFile)
        {
            uint dataSize,offset;
            byte[] retByte = null;
            int blockNodeID;
            AxpFile axp = m_AxpMap.GetAxpFileByFileName(strFileName, out blockNodeID);
            if (axp != null && blockNodeID != -1)
            {
                retByte = axp.openFileByBuffer(blockNodeID, out dataSize, out offset);
            }
            return retByte;
        }
        public bool InitialForCheckAxp(string datapath)
        {
            Games.TLBB.Log.LogSystem.Info("Initial AxpFileSystem For Check Axp Files...");

            m_AxpMap = AxpMap.Init();   //初始化AxpMap
            if (!File.Exists(datapath + AxpMap.MAPNAME))
            {
                LogSystem.Error(datapath + AxpMap.MAPNAME + " 不存在");
                return false;
            }

            if (!m_AxpMap.LoadForCheckAxp(datapath))
                return false;

            DirectoryInfo t_AssetsDir = new DirectoryInfo(datapath);
            FileInfo[] t_AssetFolderFiles = t_AssetsDir.GetFiles();

            for (int index = 0; index < t_AssetFolderFiles.Length; index++)
            {
                if (t_AssetFolderFiles[index].Name.EndsWith(".axp.so"))
                {
                    Games.TLBB.Log.LogSystem.Info("Axp File :" + t_AssetFolderFiles[index]);

                    AxpFile t_AxpFile = new AxpFile();
                    t_AxpFile.openPakFile(datapath + "/" + t_AssetFolderFiles[index].Name, AxpFilePath.StreamingAssetsPath);
                    t_AxpFile.m_strFileName = t_AssetFolderFiles[index].Name;
                    m_AxpMap.RegisterAxpFile(t_AxpFile.id, t_AxpFile); //注册AXP信息
                }
            }

            Games.TLBB.Log.LogSystem.Info("Collect Persistent data path axp files success.");
            if (m_AxpMap.Check())
                return true;
            else
                return false;

        }

        public bool CloseAllFile()
        {
            return m_AxpMap.CloseAllFile();
        }

        public override bool insertFileInAxp(string[] files)
        {
            if (insertFileInAxp(m_AxpFileThresholdSize, files))
            {
                m_AxpMap.Save();
                return true;
            }
            return false;
        }
        private bool insertFileInAxp(uint nMaxAxpFileSize, string[] files)
        {
            AxpFile lastAxpFile = m_AxpMap.GetLastAxpFile();
            if (lastAxpFile == null || lastAxpFile.isReadOnly)
            {
                lastAxpFile = CreatNewAxp(m_AxpMap.gameVersion, m_AxpMap.resFirstVersion, m_AxpMap.resSecondVersion, m_AxpMap.resThirdVersion);
                if (lastAxpFile == null)
                    return false;
            }
            uint t_FileCount = lastAxpFile.fileCount;
            uint m_CurrentFilesSize = lastAxpFile.dataSize;
            List<string> m_CollectFiles = new List<string>();

            foreach (string item in files)
            {
                int oldBlock;
                string fileName = Path.GetFileName(item);
                AxpFile oldAxp = m_AxpMap.GetAxpFileByFileName(fileName, out oldBlock);
                if (oldAxp != null && !oldAxp.isReadOnly)   //第一种情况，存在于可读写AXP，直接插入
                {
                    if (!oldAxp.insertContents(null, 0, item, ref oldBlock, AXP_CONTENTS.AC_DISK_FILE, true))
                        return false;
                    m_AxpMap.AddFileToMap(fileName, oldAxp.id, oldBlock);
                }
                else
                {   //第二种情况，需要创建并批量写入
                    uint t_nFileSize = AxpFileStream.getDiskFileSize(item);

                    m_CurrentFilesSize += t_nFileSize;
                    if (t_nFileSize > nMaxAxpFileSize)
                    {
                        Games.TLBB.Log.LogSystem.Error("Insert File is too big! Value:{0}", item);
                        return false;
                    }
                    if (m_CurrentFilesSize > nMaxAxpFileSize || t_FileCount >= 32767)
                    {
                        if (!savePakFile(lastAxpFile, m_CollectFiles))
                            return false;
                        //创建新的AXPFile
                        lastAxpFile = CreatNewAxp(m_AxpMap.gameVersion, m_AxpMap.resFirstVersion, m_AxpMap.resSecondVersion, m_AxpMap.resThirdVersion);
                        if (lastAxpFile == null)
                            return false;
                        //AXPFile创建完成
                        m_CurrentFilesSize = 0;
                        t_FileCount = 0;
                        m_CurrentFilesSize += t_nFileSize;
                        m_CollectFiles.Clear();
                    }
                    t_FileCount++;
                    m_CollectFiles.Add(item);
                }
            }
            if (!savePakFile(lastAxpFile, m_CollectFiles))
                return false;
            return true;
        }

        public override bool insertFileInAxpByStream(List<string> fileKeys, List<string> files)
        {
            if (insertFile(m_AxpFileThresholdSize, fileKeys, files))
            {
                m_AxpMap.Save();
                return true;
            }
            return false;
        }
        private bool insertFile(uint nMaxAxpFileSize, List<string> fileKeys, List<string> files)
        {
            AxpFile lastAxpFile = m_AxpMap.GetLastAxpFile();
            if (lastAxpFile == null || lastAxpFile.isReadOnly)
            {
                lastAxpFile = CreatNewAxp(m_AxpMap.gameVersion, m_AxpMap.resFirstVersion, m_AxpMap.resSecondVersion, m_AxpMap.resThirdVersion);
                if (lastAxpFile == null)
                    return false;
            }

            uint t_FileCount = lastAxpFile.fileCount;
            uint m_CurrentFilesSize = lastAxpFile.dataSize;
            List<string> m_CollectFileKey = new List<string>();
            List<string> m_CollectFullFile = new List<string>();
            int fileCnt = fileKeys.Count;
            for (int fileIdx = 0; fileIdx < fileCnt; ++fileIdx)
            {
                int oldBlock;
                string fileName = fileKeys[fileIdx];
                string fullName = files[fileIdx];
                LogSystem.Info("插入文件：{0}", fileName);
                AxpFile oldAxp = m_AxpMap.GetAxpFileByFileName(fileName, out oldBlock);
                if (oldAxp != null && !oldAxp.isReadOnly)   //第一种情况，存在于可读写AXP，直接插入
                {
                    if (!oldAxp.insertContents(null, 0, fullName, ref oldBlock, AXP_CONTENTS.AC_DISK_FILE, true))
                        return false;
                    m_AxpMap.AddFileToMap(fileName, oldAxp.id, oldBlock);
                }
                else
                {   //第二种情况，需要创建并批量写入
                    uint t_nFileSize = AxpFileStream.getDiskFileSize(fullName);

                    m_CurrentFilesSize += t_nFileSize;
                    if (t_nFileSize > nMaxAxpFileSize)
                    {
                        Games.TLBB.Log.LogSystem.Error("Insert File is too big! Value:{0}", fileName);
                        return false;
                    }
                    if (m_CurrentFilesSize > nMaxAxpFileSize || t_FileCount >= 32767)
                    {
                        if (!savePakFile(lastAxpFile, m_CollectFileKey, m_CollectFullFile))
                            return false;
                        //创建新的AXPFile
                        lastAxpFile = CreatNewAxp(m_AxpMap.gameVersion, m_AxpMap.resFirstVersion, m_AxpMap.resSecondVersion, m_AxpMap.resThirdVersion);
                        if (lastAxpFile == null)
                            return false;
                        //AXPFile创建完成
                        m_CurrentFilesSize = 0;
                        t_FileCount = 0;
                        m_CurrentFilesSize += t_nFileSize;
                        m_CollectFileKey.Clear();
                        m_CollectFullFile.Clear();
                    }
                    t_FileCount++;
                    m_CollectFileKey.Add(fileName);
                    m_CollectFullFile.Add(fullName);
                }
            }
            if (!savePakFile(lastAxpFile, m_CollectFileKey, m_CollectFullFile))
                return false;
            return true;
        }
        private AxpFile CreatNewAxp(int gameVersion, int resFirstVersion, int resSecondVersion, int resThirdVerison)
        {
            AxpFile axpFile = new AxpFile();
            int id = m_AxpMap.GetNextID();
            string strPakFileName = GameInfo.AxpPath + "Data" + id + ".axp.so";
            if (!axpFile.createNewPakFile(strPakFileName, id, false, gameVersion, resFirstVersion, resSecondVersion, resThirdVerison))
            {
                Games.TLBB.Log.LogSystem.Info("AXP创建文件失败");
                return null;
            };
            axpFile.m_strFileName = strPakFileName;
            m_AxpMap.RegisterAxpFile(axpFile.id, axpFile);
            return axpFile;
        }

        private bool savePakFile(AxpFile axp, List<string> t_AllFiles)
        {
            if (axp == null)
                return false;

            foreach (string item in t_AllFiles)
            {
                int blockNodeIndex = -1;
                if (!axp.insertContents(null, 0, item, ref blockNodeIndex, AXP_CONTENTS.AC_DISK_FILE, true))
                {
                    return false;
                }
                m_AxpMap.AddFileToMap(Path.GetFileName(item), axp.id, blockNodeIndex);
                Games.TLBB.Log.LogSystem.Info("Already insert axp file, name : {0}", item);
            }
            return true;
        }

        private bool savePakFile(AxpFile axp, List<string> t_AllFiles, List<string> fullFiles)
        {
            if (axp == null)
                return false;
            int fileCnt = t_AllFiles.Count;
            for (int idx = 0; idx < fileCnt; ++idx)
            {
                string item = t_AllFiles[idx];
                string fullFile = fullFiles[idx];
                int blockNodeIndex = -1;
                if (!axp.insertContents(null, 0, fullFile, ref blockNodeIndex, AXP_CONTENTS.AC_DISK_FILE, true))
                {
                    return false;
                }
                m_AxpMap.AddFileToMap(item, axp.id, blockNodeIndex);
                Games.TLBB.Log.LogSystem.Info("Already insert axp file, name : {0}", item);
            }
            return true;
        }

    }
}
#endif



