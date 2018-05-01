using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Games.TLBB.Manager;

namespace AxpTools
{
#if UNITY_EDITOR
    public
#else
#endif
    class AxpMap
    {
        static readonly Int32 VERSIONID = 0x00010001;
        public static readonly string MAPNAME = "AxpMap.db";

        public int AxpFileCount { get { return AxpFileList.Count; } }
        public uint AxpFileMaxSize { get { return maxPakSize; } }
        public int AxpIDCount { get { return nextID; } }
        public int gameVersion { get { return m_gameVersion; } }
        public int resFirstVersion {  get { return m_resFirstVersion; } }
        public int resSecondVersion { get { return m_resSecondVersion; } }
        public int resThirdVersion { get { return m_resThirdVersion; } }
        static AxpMap _instance = null;
        static int nextID = -1;
        static uint maxPakSize = 0;
        static int m_gameVersion = 0;
        static int m_resFirstVersion = 0;
        static int m_resSecondVersion = 0;
        static int m_resThirdVersion = 0;
        Dictionary<string, KeyValuePair<int, int>> mainDictionary;
        Dictionary<int, AxpFile> AxpFileList = new Dictionary<int, AxpFile>(10);
        public static AxpMap Init() //获取单例实例接口
#region 单例模式
        {
            if (_instance == null)
                _instance = new AxpMap();
            return _instance;
        }
        private AxpMap() { }
#endregion
        public bool Check()
        {
            return AxpFileList.Count == nextID;
        }
        /// <summary>
        /// 增加文件到文件映射表
        /// </summary>
        /// <returns>是否发生文件替换</returns>
        public bool AddFileToMap(string fileName, int AxpId, int blockNodeIndex)
        {
            //fileName = fileName.Replace("AXPBundles", "Bundles");
            if (mainDictionary.ContainsKey(fileName))
            {
                mainDictionary[fileName] = new KeyValuePair<int, int>(AxpId, blockNodeIndex);
                return true;
            }
            else
            {
                mainDictionary[fileName] = new KeyValuePair<int, int>(AxpId, blockNodeIndex);
                return false;
            }
        }
        /// <summary>
        /// 删除文件从文件映射表
        /// </summary>
        public void RemoveFileFromMap(string fileName)
        {
            mainDictionary.Remove(fileName);
        }
        /// <summary>
        /// 判断指定名称文件是否存在
        /// </summary>
        public bool Exists(string fileName)
        {
            if (mainDictionary == null)
                return false;
            return mainDictionary.ContainsKey(fileName);
        }
        /// <summary>
        /// 根据文件名获取文件所在AXP
        /// </summary>
        /// <returns></returns>
        public AxpFile GetAxpFileByFileName(string fileName, out int blockNodeIndex)
        {
            if (mainDictionary.ContainsKey(fileName))
            {
                if (AxpFileList.ContainsKey(mainDictionary[fileName].Key))
                {
                    blockNodeIndex = mainDictionary[fileName].Value;
                    return AxpFileList[mainDictionary[fileName].Key];
                }
                Games.TLBB.Log.LogSystem.Info("不包含注册File" + mainDictionary[fileName].Value);
            }
            Games.TLBB.Log.LogSystem.Info("查表失败");
            blockNodeIndex = -1;
            return null;
        }
        /// <summary>
        /// 注册AxpFile，请保证ID正确，ID是文件与AXP文件的唯一关联
        /// </summary>
        public void RegisterAxpFile(int AxpFileID, AxpFile axpFile)
        {
            AxpFileList[AxpFileID] = axpFile;
        }
        public AxpFile GetLastAxpFile()
        {
            if (AxpFileList.ContainsKey(nextID - 1))
                return AxpFileList[nextID - 1];
            else
                return null;
        }
        public int GetNextID()
        {
            int ret = nextID;
            ++nextID;
            return ret;
        }
        /// <summary>
        /// 保存映射表，编辑器模式下保存到AXP目录、运行模式下保存到持久化目录
        /// </summary>
        public void Save()
        {
            SaveToDir(GameInfo.AxpPath + AxpMap.MAPNAME);
        }
        public void SaveToDir(string dir)
        {
            FileStream file = File.Open(dir, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            Serialize(writer);
            writer.Flush();
            writer.Close();
            file.Close();
            file.Dispose();
        }
        /// <summary>
        /// 检查Axp文件版本
        /// </summary>
        public bool CheckVaild()
        {
            byte[] version = new byte[4];
            FileStream file = new FileStream(GameInfo.AxpPath + AxpMap.MAPNAME, FileMode.Open);
            int code = file.Read(version, 0, 4);
            file.Close();
            file.Dispose();
            if (code == 4 && System.BitConverter.ToInt32(version, 0) == VERSIONID)
                return true;
            return false;

        }
        /// <summary>
        /// 创建新的Map表
        /// </summary>
        public void CreatNewMap(uint maxSize, int num = 10, int gameVersion = 0, int resFirstVersion = 0, int resSecondVersion = 0, int resThirdVersion = 0)
        {
            maxPakSize = maxSize;
            nextID = 0;
            m_gameVersion = gameVersion;
            m_resFirstVersion = resFirstVersion;
            m_resSecondVersion = resSecondVersion;
            m_resThirdVersion = resThirdVersion;
            mainDictionary = new Dictionary<string, KeyValuePair<int, int>>(num);
        }
        /// <summary>
        /// 加载映射表
        /// </summary>
        public bool Load()
        {
            FileStream file = File.Open(GameInfo.AxpPath + AxpMap.MAPNAME, FileMode.Open);
            BinaryReader reader = new BinaryReader(file);
            bool ret = Deserialize(reader);
            reader.Close();
            file.Close();
            file.Dispose();
            return ret;
        }


#if UNITY_EDITOR
        /// <summary>
        /// Load Mapping Table
        /// </summary>
        /// <param name="path">file path name</param>
        /// <returns></returns>
        public bool Load(string path)
        {
            FileStream file = File.Open(path + "/" + AxpMap.MAPNAME, FileMode.Open);
            BinaryReader reader = new BinaryReader(file);
            bool ret = Deserialize(reader);
            reader.Close();
            file.Close();
            file.Dispose();
            return ret;
        }
        /// <summary>
        /// get the axp contain file name from axpmap.
        /// </summary>
        /// <param name="AxpId"></param>
        /// <param name="blockNodeIndex"></param>
        /// <returns></returns>
        public string GetFileNameFromMap(int AxpId, int blockNodeIndex)
        {

            string filename = null;
            foreach(KeyValuePair<string, KeyValuePair<int,int>> data in mainDictionary)
            {
                if(data.Value.Key == AxpId && data.Value.Value == blockNodeIndex)
                {
                    filename = data.Key;
                    return filename;
                }
            }

            return filename;
        }

        /// <summary>
        /// clear all data, only used in editor mode.
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            if(mainDictionary != null)
            {
                mainDictionary.Clear();
            }
        }
            
#endif


        public bool LoadForCheckAxp(string filePath)
        {
            FileStream file = File.Open(filePath + AxpMap.MAPNAME, FileMode.Open);
            BinaryReader reader = new BinaryReader(file);
            bool ret = Deserialize(reader);
            reader.Close();
            file.Close();
            file.Dispose();
            return ret;
        }


        /// <summary>
        /// Only Used in Editor Mode.
        /// </summary>
        /// <returns></returns>
        public bool CloseAllFile()
        {

            foreach (KeyValuePair<int, AxpFile> data in AxpFileList)
            {
                AxpFile t_AxpFile = data.Value;
                if (t_AxpFile != null)
                {
                    t_AxpFile.closePakFile();
                }
                else
                {
                    return false;
                }
            }

            return true;

        }


        /// <summary>
        /// only use in game mode. compare persistent data path and streaming assets axp map version.
        /// if return true, the same version. else different version.
        /// </summary>
        /// <returns></returns>
        public bool CompareAxpMapInPersistentAndStreamingAssets()
        {
            byte[] bytes = ResourceProxyManager.Instance.LoadFileInStreamingAssets(AxpMap.MAPNAME);
            if(bytes == null)
            {
                return false;
            }
           
            int id = BitConverter.ToInt32(bytes, 0);
            if (id != VERSIONID)      //Check VESIONID
            {
                Games.TLBB.Log.LogSystem.Error("Axp系统初始化失败，版本号不匹配:{0}:{1}", id, VERSIONID);
                return false;
            }

            int t_GameVersion = -1;
            int t_ResFirstVersion = -1;
            int t_ResSecondVersion = -1;
            int t_ResThirdVersion = -1;
            t_GameVersion = BitConverter.ToInt32(bytes, 4);
            t_ResFirstVersion = BitConverter.ToInt32(bytes, 8);
            t_ResSecondVersion = BitConverter.ToInt32(bytes, 12);
            t_ResThirdVersion = BitConverter.ToInt32(bytes, 16);

            if(m_gameVersion != t_GameVersion || m_resFirstVersion != t_ResFirstVersion || m_resSecondVersion != t_ResSecondVersion || m_resThirdVersion != t_ResThirdVersion)
            {
                return false;
            }

            return true;
            
        }


        void Serialize(BinaryWriter writer)
        {
            writer.Write(VERSIONID);
            writer.Write(m_gameVersion);
            writer.Write(m_resFirstVersion);
            writer.Write(m_resSecondVersion);
            writer.Write(m_resThirdVersion);
            writer.Write(maxPakSize);
            writer.Write(nextID);
            writer.Write((Int32)mainDictionary.Count);
            foreach (string key in mainDictionary.Keys)
            {
                writer.Write(key);
                writer.Write(mainDictionary[key].Key);
                writer.Write(mainDictionary[key].Value);
            }
        }
        bool Deserialize(BinaryReader reader)
        {
            int id = reader.ReadInt32();
            if (id != VERSIONID)      //Check VESIONID
            {
                Games.TLBB.Log.LogSystem.Error("Axp系统初始化失败，版本号不匹配:{0}:{1}", id, VERSIONID);
                return false;
            }
            int t_GameVersion = -1;
            int t_ResFirstVersion = -1;
            int t_ResSecondVersion = -1;
            int t_ResThirdVersion = -1;
            t_GameVersion = reader.ReadInt32();
            t_ResFirstVersion = reader.ReadInt32();
            t_ResSecondVersion = reader.ReadInt32();
            t_ResThirdVersion = reader.ReadInt32();
            uint maxSize = reader.ReadUInt32();
            int currentID = reader.ReadInt32();
            int num = reader.ReadInt32();
            //@ author woonam.安全监测 覆盖安装时 如果旧版本没有清理掉，会出现反序列化读取错误，
            //  读到随机值 数会很大 后面New内存时直接崩溃，故设置个阈值进行合理安全监测，暂时设置
            //  文件数量是100000个文件。
            if (num < 1 || num > 100000)
                return false;
            Games.TLBB.Log.LogSystem.Info("AxpMap GameVersion : {0}, ResFirstVersion : {1}, ResSecondVersion : {2}, ResThirdVersion : {3}", t_GameVersion, t_ResFirstVersion, t_ResSecondVersion, t_ResThirdVersion);
            CreatNewMap(maxSize, num, t_GameVersion, t_ResFirstVersion, t_ResSecondVersion, t_ResThirdVersion);
            nextID = currentID;
            for (int i = 0; i < num; i++)
                mainDictionary.Add(reader.ReadString(), new KeyValuePair<int, int>(reader.ReadInt32(), reader.ReadInt32()));
            return true;
        }
    }
}
