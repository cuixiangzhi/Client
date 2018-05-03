//#if UNITY_IPHONE || UNITY_EDITOR || UNITY_STANDALONE
//using UnityEngine;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Linq;
//using System.Diagnostics;
//using System.Text;
//using GameCore;
////using Games.TLBB.Log;

//namespace AxpTools
//{


//    public class MultiValueDictionary<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="MultiValueDictionary&lt;TKey, TValue&gt;"/> class.
//        /// </summary>
//        public MultiValueDictionary()
//            : base()
//        {
//        }


//        /// <summary>
//        /// Adds the specified value under the specified key
//        /// </summary>
//        /// <param name="key">The key.</param>
//        /// <param name="value">The value.</param>
//        public void Add(TKey key, TValue value)
//        {
//            //ArgumentVerifier.CantBeNull(key, "key");

//            HashSet<TValue> container = null;
//            if (!this.TryGetValue(key, out container))
//            {
//                container = new HashSet<TValue>();
//                base.Add(key, container);
//            }
//            container.Add(value);
//        }


//        /// <summary>
//        /// Determines whether this dictionary contains the specified value for the specified key 
//        /// </summary>
//        /// <param name="key">The key.</param>
//        /// <param name="value">The value.</param>
//        /// <returns>true if the value is stored for the specified key in this dictionary, false otherwise</returns>
//        public bool ContainsValue(TKey key, TValue value)
//        {
//            //ArgumentVerifier.CantBeNull(key, "key");
//            bool toReturn = false;
//            HashSet<TValue> values = null;
//            if (this.TryGetValue(key, out values))
//            {
//                toReturn = values.Contains(value);
//            }
//            return toReturn;
//        }


//        /// <summary>
//        /// Removes the specified value for the specified key. It will leave the key in the dictionary.
//        /// </summary>
//        /// <param name="key">The key.</param>
//        /// <param name="value">The value.</param>
//        public void Remove(TKey key, TValue value)
//        {
//            //ArgumentVerifier.CantBeNull(key, "key");

//            HashSet<TValue> container = null;
//            if (this.TryGetValue(key, out container))
//            {
//                container.Remove(value);
//                if (container.Count <= 0)
//                {
//                    this.Remove(key);
//                }
//            }
//        }


//        /// <summary>
//        /// Merges the specified multivaluedictionary into this instance.
//        /// </summary>
//        /// <param name="toMergeWith">To merge with.</param>
//        public void Merge(MultiValueDictionary<TKey, TValue> toMergeWith)
//        {
//            if (toMergeWith == null)
//            {
//                return;
//            }

//            foreach (KeyValuePair<TKey, HashSet<TValue>> pair in toMergeWith)
//            {
//                foreach (TValue value in pair.Value)
//                {
//                    this.Add(pair.Key, value);
//                }
//            }
//        }


//        /// <summary>
//        /// Gets the values for the key specified. This method is useful if you want to avoid an exception for key value retrieval and you can't use TryGetValue
//        /// (e.g. in lambdas)
//        /// </summary>
//        /// <param name="key">The key.</param>
//        /// <param name="returnEmptySet">if set to true and the key isn't found, an empty hashset is returned, otherwise, if the key isn't found, null is returned</param>
//        /// <returns>
//        /// This method will return null (or an empty set if returnEmptySet is true) if the key wasn't found, or
//        /// the values if key was found.
//        /// </returns>
//        public HashSet<TValue> GetValues(TKey key, bool returnEmptySet)
//        {
//            HashSet<TValue> toReturn = null;
//            if (!base.TryGetValue(key, out toReturn) && returnEmptySet)
//            {
//                toReturn = new HashSet<TValue>();
//            }
//            return toReturn;
//        }
//    }

//    public enum AXP_CONTENTS
//    {
//        AC_DISK_FILE,               //磁盘文件
//        AC_MEMORY,                  //内存
//    }
//    public class AxpFile
//    {
//        private const uint AXPK_FILE_FLAG = 0x4B505841; //文件头，AXP标示
//        private const int BLOCK_TABLE_MAXSIZE = 384 * 1024;////1024 * 1024;   //Block Table区间最大大小

//        [StructLayout(LayoutKind.Sequential)]
//        struct File_Head
//        {
//            public uint nIdentity;
//            //public uint nVersion;
//            public int gameversion;
//            public int resfirstversion;
//            public int ressecondversion;
//            public int resthirdversion;
//            public int fileID;
//            public int nEditFlag;
//            public int nBlockTable_Offset;
//            public int nBlockTable_Count;
//            public uint nBlockTable_MaxSize;
//            public int nData_Offset;
//            public uint nData_Size;
//            public uint nData_HoleSize;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        struct File_BlockNode
//        {
//            public uint nDataOffset;
//            public uint nBlockSize;
//            public uint nFlags;
//        }

//        //block table
//        List<File_BlockNode> m_blockTable = new List<File_BlockNode>();

//        MultiValueDictionary<uint, uint> m_mapFreeBlock = new MultiValueDictionary<uint, uint>();

//        public int id { get { return m_bOpenState ? m_fileHead.fileID : -1; } }
//        public bool isOpenState { get { return m_bOpenState; } }
//        public bool isReadOnly { get { return m_bConst; } }
//        public uint dataSize { get { return m_fileHead.nData_Size; } }
//        public uint fileCount { get { return m_FileCount; } }
//        public AxpFilePath axpFilePath { get { return m_path; } }
//        public string m_strPathFileName;                        //file & path name
//        public string m_strFileName;                            //only file name.
//        bool m_bOpenState;                               //axp file open state.
//        bool m_bConst;                                //whether or not edit mode.

//        AxpFilePath m_path;

//        File_Head m_fileHead;

//        AxpFileStream m_fileStream;

//        int m_FileHeadSize = 0;
//        int m_FileBlockNodeSize = 0;
//        uint m_FileCount = 0;
//#if UNITY_ANDROID
//        private AndroidJavaObject activity_;
//        private AndroidJavaObject ActivityJO
//        {
//            get
//            {
//                if (activity_ != null)
//                    return activity_;

//                activity_ = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
//                if (activity_ == null)
//                {
//                    LogMgr.Log("Initial Activity Failed.");
//                }
//                return activity_;
//            }
//        }

//#endif

//        public AxpFile()
//        {
//            m_bOpenState = false;
//        }

//        public bool createNewPakFile(string strPackFileName, int fileID, bool bConst, int gameVersion = 0, int resFirstVersion = 0, int resSecondVersion = 0, int resThirdVersion = 0)
//        {
//            if (string.IsNullOrEmpty(strPackFileName))
//            {
//                return false;
//            }

//            //generate data.
//            m_strPathFileName = strPackFileName;
//            m_bConst = bConst;

//            //
//            //FileStream m_hPakFile = File.Open(m_strPathFileName, FileMode.Create);\
//#if UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR
//            m_fileStream = AxpFileStream.OpenFile(m_strPathFileName, false);
//#elif UNITY_ANDROID
//            m_fileStream = AxpFileStream.OpenFile(m_strPathFileName, false, false);
//#endif

//            if (m_fileStream == null)
//            {
//                LogMgr.Log("AXP创建文件时打开文件失败" + m_strPathFileName);
//                return false;
//            }
//            //File Head
//            m_fileHead.nIdentity = AXPK_FILE_FLAG;
//            //m_fileHead.nVersion = 1 << 16 | 1;
//            m_fileHead.gameversion = gameVersion;
//            m_fileHead.resfirstversion = resFirstVersion;
//            m_fileHead.ressecondversion = resSecondVersion;
//            m_fileHead.resthirdversion = resThirdVersion;
//            m_fileHead.nEditFlag = m_bConst ? 0 : 1;

//            m_fileHead.fileID = fileID;

//            m_FileHeadSize = Marshal.SizeOf(m_fileHead);
//            m_fileHead.nBlockTable_Offset = m_FileHeadSize;

//            File_BlockNode t_blockNode = new File_BlockNode();
//            m_FileBlockNodeSize = Marshal.SizeOf(t_blockNode);


//            m_fileHead.nBlockTable_Count = 0;
//            m_fileHead.nBlockTable_MaxSize = BLOCK_TABLE_MAXSIZE;

//            m_fileHead.nData_Offset = Marshal.SizeOf(m_fileHead) + BLOCK_TABLE_MAXSIZE;
//            m_fileHead.nData_Size = 0;
//            m_fileHead.nData_HoleSize = 0;


//            //uint dwWriteBytes = 0;

//            //write head.
//            byte[] m_fileHeadBytes = new byte[m_FileHeadSize];
//            IntPtr m_fileHeadPtr = Marshal.AllocHGlobal(m_FileHeadSize);

//            Marshal.StructureToPtr(m_fileHead, m_fileHeadPtr, false);
//            Marshal.Copy(m_fileHeadPtr, m_fileHeadBytes, 0, m_FileHeadSize);
//            Marshal.FreeHGlobal(m_fileHeadPtr);

//            //bw.Write(m_fileHeadBytes, 0, t_fileheadSize);
//            m_fileStream.Write(m_fileHeadBytes);
//            //write head end.

//            //write block table.
//            byte[] m_BlockTableBytes = Enumerable.Repeat((byte)0, BLOCK_TABLE_MAXSIZE).ToArray();   //对区块填0
//            m_fileStream.Write(m_BlockTableBytes);

//            //bw.Close();
//            m_FileCount = 0;
//            m_bOpenState = true;
//            return true;
//        }

//        public bool openPakFile(string strPackFileName, AxpFilePath path)
//        {
//            LogMgr.Log("Enter open Pak File Module. {0}", strPackFileName);

//            if (string.IsNullOrEmpty(strPackFileName))
//            {
//                return false;
//            }

//            //generate data
//            m_strPathFileName = strPackFileName;

//            //whether or not file exist.
//#if UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR
//            m_fileStream = AxpFileStream.OpenFile(m_strPathFileName, path != AxpFilePath.PersistentDataPath);
//#elif UNITY_ANDROID
//            m_fileStream = AxpFileStream.OpenFile(strPackFileName, path != AxpFilePath.PersistentDataPath, path == AxpFilePath.StreamingAssetsPath);
//#endif
//            if (m_fileStream == null)
//            {
//                LogMgr.Log("Open Axp Failed.");
//                return false;
//            }

//            /***********************************************************
//             *                  Read FileHead
//             * ********************************************************/

//            m_FileHeadSize = Marshal.SizeOf(m_fileHead);
//            //byte[] t_readfileheadBytes = new byte[m_FileHeadSize];
//            //m_FileReader.Read(t_readfileheadBytes, 0, m_FileHeadSize);
//            byte[] t_readfileheadBytes = m_fileStream.ReadBytes(m_FileHeadSize);

//            //alloc struct memory size.
//            //read file header.
//            IntPtr t_fileHeadPtr = Marshal.AllocHGlobal(m_FileHeadSize);
//            Marshal.Copy(t_readfileheadBytes, 0, t_fileHeadPtr, m_FileHeadSize);
//            m_fileHead = (File_Head)Marshal.PtrToStructure(t_fileHeadPtr, typeof(File_Head));
//            Marshal.FreeHGlobal(t_fileHeadPtr);

//            LogMgr.Log("Axp GameVersion : {0} , ResFirstVersion : {1} , ResSecondVersion : {2},  ResThirdVersion: {3}", m_fileHead.gameversion, m_fileHead.resfirstversion, m_fileHead.ressecondversion, m_fileHead.resthirdversion);
//            /***********************************************************
//             *                  Read BlockTable
//             * ********************************************************/

//            File_BlockNode t_FileBlockNode = new File_BlockNode();
//            m_FileBlockNodeSize = Marshal.SizeOf(t_FileBlockNode);
//            byte[] t_readblockTableBytes = m_fileStream.ReadBytes(m_FileBlockNodeSize * m_fileHead.nBlockTable_Count);

//            //convert data
//            IntPtr t_BlockTablePtr = Marshal.AllocHGlobal(m_FileBlockNodeSize * m_fileHead.nBlockTable_Count);
//            Marshal.Copy(t_readblockTableBytes, 0, t_BlockTablePtr, m_FileBlockNodeSize * m_fileHead.nBlockTable_Count);
//            File_BlockNode[] t_TempBlockTableArray = GetArrayOfStruct<File_BlockNode>(t_BlockTablePtr, m_fileHead.nBlockTable_Count);
//            Marshal.FreeHGlobal(t_BlockTablePtr);

//            m_blockTable.AddRange(t_TempBlockTableArray);
//            //最好做一个排序型检测
//            for (int index = 0; index < m_blockTable.Count; index++)
//            {
//                File_BlockNode t_blockNode = m_blockTable[index];
//                if (t_blockNode.nDataOffset < m_fileHead.nData_Offset)
//                {
//                    return false;
//                }

//                if (!getBlockNodeUsed(t_blockNode))
//                {
//                    m_mapFreeBlock.Add(upBoundBlockSize(t_blockNode.nBlockSize), (uint)index);
//                }

//            }

//            m_FileCount = (uint)m_blockTable.Count;
//            m_bOpenState = true;
//            m_path = path;
//            m_bConst = (m_fileHead.nEditFlag & 1) == 0;
//            LogMgr.Log("Axp File Load Successed");
//            return true;
//        }

//        public bool closePakFile()
//        {
//            m_fileStream.Close();
//            m_fileStream = null;
//            m_bOpenState = false;
//            return true;
//        }

//        /// <summary>
//        /// 以文件流方式打开包中的一个文件
//        /// </summary>
//        /// <param name="blockNodeIndex"></param>
//        public bool openFile(int blockNodeIndex, out uint dataSize, out uint offSet)
//        {
//            dataSize = 0;
//            offSet = 0;
//            if (blockNodeIndex < 0 || m_blockTable.Count <= blockNodeIndex)
//                return false;

//            //得到block数据
//            dataSize = m_blockTable[blockNodeIndex].nBlockSize;
//            offSet = m_blockTable[blockNodeIndex].nDataOffset;
//            //LogMgr.Log("Open Axp File :{0} dataSize: {1} offset: {2}", m_strPathFileName, dataSize.ToString(), offSet.ToString());
//            return true;
//        }

//        public int GetFileOffset(int blockNodeIndex)
//        {
//            if (blockNodeIndex < 0 || m_blockTable.Count <= blockNodeIndex)
//                return -1;

//            return (int)m_blockTable[blockNodeIndex].nDataOffset;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="strFileName"></param>
//        /// <param name="dataSize"></param>
//        /// <param name="offSet"></param>
//        /// <returns></returns>
//        public byte[] openFileByBuffer(int blockNodeIndex, out uint dataSize, out uint offSet)
//        {
//            dataSize = 0;
//            offSet = 0;
//            if (m_fileStream == null)
//                return null;

//            if (!openFile(blockNodeIndex, out dataSize, out offSet))
//                return null;

//            m_fileStream.Seek((long)offSet);

//            byte[] t_readByte = m_fileStream.ReadBytes((int)dataSize);
//            if (t_readByte == null)
//                return null;
//            if (t_readByte.Length == dataSize)
//                return t_readByte;
//            return null;
//        }

//        public byte[] openFileByBuffer(int blockNodeIndex, uint offSet, uint dataSize)
//        {
//            if (m_fileStream == null)
//                return null;

//            int baseOffset = GetFileOffset(blockNodeIndex);
//            m_fileStream.Seek((long)(baseOffset + offSet));
//            byte[] t_readByte = m_fileStream.ReadBytes((int)dataSize);
//            if (t_readByte == null)
//                return null;
//            if (t_readByte.Length == dataSize)
//                return t_readByte;
//            return null;
//        }
//        public bool openFileByBuffer_ByCPlusPlus(int blockNodeIndex, out uint dataSize, out uint offSet)
//        {
//            dataSize = 0;
//            offSet = 0;

//            if (!openFile(blockNodeIndex, out dataSize, out offSet))
//                return false;

//            return true;
//        }




//        public bool insertContents(byte[] strContents, uint nContentlen, string strFilePath, ref int blockNodeIndex, AXP_CONTENTS sourceType, bool bSaveAtOnce)
//        {
//            if (m_blockTable.Count <= blockNodeIndex
//            #region || m_bConst
//#if !UNITY_EDITOR
//                || m_bConst
//#endif
//            #endregion
//                //此处是为了编辑器模式下能使用插件对不可编辑AXP进行打包而不对AXP只读属性进行检查
//                )
//            {
//                return false;
//            }

//            //if (m_bConst)
//            //{
//            //    return false;
//            //}

//            if (sourceType == AXP_CONTENTS.AC_DISK_FILE && string.IsNullOrEmpty(strFilePath))
//                return false;

//            //check file whether or not exist.
//            //if (sourceType == AXP_CONTENTS.AC_DISK_FILE && !File.Exists(strContents))
//            //{
//            //    return false;
//            //

//            //get file size.
//            uint nFileSize = sourceType == AXP_CONTENTS.AC_DISK_FILE ? AxpFileStream.getDiskFileSize(strFilePath) : nContentlen;

//            //if has the same name file.
//            if (blockNodeIndex >= 0)
//            {
//                LogMgr.Log("Insert  unity asset bundle exists!");

//                //如果尺寸接近

//                File_BlockNode t_FileBlockNode = m_blockTable[blockNodeIndex];

//                LogMgr.Log("FileBlockNode is :{0}", t_FileBlockNode.nBlockSize.ToString());
//                LogMgr.Log("File Size :{0}", nFileSize.ToString());

//                if (upBoundBlockSize(t_FileBlockNode.nBlockSize) == upBoundBlockSize(nFileSize))
//                {
//                    //直接替换文件内容
//                    if (sourceType == AXP_CONTENTS.AC_DISK_FILE)
//                    {
//                        if (!writeDiskFile(m_blockTable[blockNodeIndex].nDataOffset, upBoundBlockSize(m_blockTable[blockNodeIndex].nBlockSize), strFilePath))
//                            return false;
//                    }
//                    else
//                    {
//                        if (!writeMemory(m_blockTable[blockNodeIndex].nDataOffset, upBoundBlockSize(m_blockTable[blockNodeIndex].nBlockSize), strContents, nContentlen))
//                            return false;
//                    }


//                    if (m_blockTable[blockNodeIndex].nBlockSize != nFileSize)
//                    {
//                        //文件尺寸有差异，保存BlockNode数据
//                        t_FileBlockNode.nBlockSize = nFileSize;
//                        m_blockTable[blockNodeIndex] = t_FileBlockNode;
//                        if (!writeBlockNode((uint)blockNodeIndex))
//                            return false;
//                    }
//                    LogMgr.Log("Insert the same size....");
//                    //
//                }//尺寸发生变化，需要删除旧空间，分配新空间
//                else
//                {
//                    setBlockNodeUsed(ref t_FileBlockNode, false);
//                    m_blockTable[blockNodeIndex] = t_FileBlockNode;
//                    //将旧块加入空闲块列表
//                    m_mapFreeBlock.Add(upBoundBlockSize(t_FileBlockNode.nBlockSize), (uint)blockNodeIndex);

//                    m_fileHead.nData_HoleSize += upBoundBlockSize(t_FileBlockNode.nBlockSize);
//                    LogMgr.Log("HoleSize :{0}", m_fileHead.nData_HoleSize.ToString());
//                    //重新尝试分配空间
//                    KeyValuePair<int, int> nNewBlockPair = allocFreeBlock(nFileSize);
//                    int nNewBlockIndex = nNewBlockPair.Key;
//                    if (nNewBlockPair.Key < 0)
//                    {
//                        return false;
//                    }

//                    LogMgr.Log("Old Index Is :{0}", blockNodeIndex.ToString());
//                    LogMgr.Log("New Index Is :{0}", nNewBlockIndex.ToString());

//                    /*************************************************
//                     *          磁盘操作开始，保存文件
//                     ************************************************/
//                    if (nNewBlockPair.Value > 0)
//                    {
//                        LogMgr.Log("NewBlockPair Is :{0}", nNewBlockPair.Value.ToString());
//                        //分割大空间时产生的副空间
//                        if (!writeBlockNode((uint)(nNewBlockPair.Value)))
//                            return false;
//                    }

//                    //写入硬盘的文件

//                    if (sourceType == AXP_CONTENTS.AC_DISK_FILE)
//                    {
//                        if (!writeDiskFile(m_blockTable[nNewBlockIndex].nDataOffset, upBoundBlockSize(m_blockTable[nNewBlockIndex].nBlockSize), strFilePath))
//                            return false;

//                        LogMgr.Log("WriteDiskFile : {0}", strContents);
//                    }
//                    else
//                    {
//                        if (!writeMemory(m_blockTable[nNewBlockIndex].nDataOffset, upBoundBlockSize(m_blockTable[nNewBlockIndex].nBlockSize), strContents, nContentlen))
//                            return false;
//                    }



//                    if (!writeBlockNode((uint)nNewBlockIndex))
//                        return false;

//                    //文件所在的新的BlockNode
//                    if (blockNodeIndex != nNewBlockIndex)
//                    {
//                        if (!writeBlockNode((uint)blockNodeIndex))
//                            return false;
//                    }

//                    if (!writeFileHead())
//                        return false;

//                    //确认写入磁盘
//                    if (bSaveAtOnce)
//                    {
//                        m_fileStream.Flush();
//                    }

//                    blockNodeIndex = nNewBlockIndex;
//                    LogMgr.Log("Insert a big block successed.");
//                }
//            }
//            else
//            {

//                LogMgr.Log("Insert  unity asset bundle doesn't exists!");

//                //获取一块合适空间
//                KeyValuePair<int, int> nNewBlockPair = allocFreeBlock(nFileSize);
//                blockNodeIndex = nNewBlockPair.Key;

//                if (blockNodeIndex < 0)
//                {
//                    return false;
//                }

//                /*************************************************
//                 *          磁盘操作开始，保存文件
//                 ************************************************/
//                if (nNewBlockPair.Value > 0)
//                {
//                    //分割大空间时产生的副空间
//                    if (!writeBlockNode((uint)(nNewBlockPair.Value)))
//                        return false;
//                }

//                //写入硬盘的文件

//                if (sourceType == AXP_CONTENTS.AC_DISK_FILE)
//                {
//                    if (!writeDiskFile(m_blockTable[blockNodeIndex].nDataOffset, upBoundBlockSize(m_blockTable[blockNodeIndex].nBlockSize), strFilePath))
//                        return false;
//                }
//                else
//                {
//                    if (!writeMemory(m_blockTable[blockNodeIndex].nDataOffset, upBoundBlockSize(m_blockTable[blockNodeIndex].nBlockSize), strContents, nContentlen))
//                        return false;
//                }




//                if (!writeBlockNode((uint)blockNodeIndex))
//                    return false;
//                if (!writeFileHead())
//                    return false;



//            }
//            //确认写入磁盘
//            if (bSaveAtOnce)
//            {
//                m_fileStream.Flush();
//            }
//            //if (isUpdateListDynamic() && strFileInPakName.Contains("(list)"))
//            //{
//            //    insertFileList(strFilePathName,strFileInPakName);
//            //}

//            return true;
//        }
//        public bool insertContents(byte[] strContents, uint nContentlen, FileStream fileStream, ref int blockNodeIndex, AXP_CONTENTS sourceType, bool bSaveAtOnce)
//        {
//            if (m_blockTable.Count <= blockNodeIndex
//            #region || m_bConst
//#if !UNITY_EDITOR
//                || m_bConst
//#endif
//            #endregion
//                //此处是为了编辑器模式下能使用插件对不可编辑AXP进行打包而不对AXP只读属性进行检查
//                )
//            {
//                return false;
//            }

//            //if (m_bConst)
//            //{
//            //    return false;
//            //}

//            if (sourceType == AXP_CONTENTS.AC_DISK_FILE && fileStream == null)
//                return false;

//            //check file whether or not exist.
//            //if (sourceType == AXP_CONTENTS.AC_DISK_FILE && !File.Exists(strContents))
//            //{
//            //    return false;
//            //

//            //get file size.
//            uint nFileSize = sourceType == AXP_CONTENTS.AC_DISK_FILE ? (uint)fileStream.Length : nContentlen;

//            //if has the same name file.
//            if (blockNodeIndex >= 0)
//            {
//                LogMgr.Log("Insert  unity asset bundle exists!");

//                //如果尺寸接近

//                File_BlockNode t_FileBlockNode = m_blockTable[blockNodeIndex];

//                LogMgr.Log("FileBlockNode is :{0}", t_FileBlockNode.nBlockSize.ToString());
//                LogMgr.Log("File Size :{0}", nFileSize.ToString());

//                if (upBoundBlockSize(t_FileBlockNode.nBlockSize) == upBoundBlockSize(nFileSize))
//                {
//                    //直接替换文件内容
//                    if (sourceType == AXP_CONTENTS.AC_DISK_FILE)
//                    {
//                        if (!writeDiskFile(m_blockTable[blockNodeIndex].nDataOffset, upBoundBlockSize(m_blockTable[blockNodeIndex].nBlockSize), fileStream))
//                            return false;
//                    }
//                    else
//                    {
//                        if (!writeMemory(m_blockTable[blockNodeIndex].nDataOffset, upBoundBlockSize(m_blockTable[blockNodeIndex].nBlockSize), strContents, nContentlen))
//                            return false;
//                    }


//                    if (m_blockTable[blockNodeIndex].nBlockSize != nFileSize)
//                    {
//                        //文件尺寸有差异，保存BlockNode数据
//                        t_FileBlockNode.nBlockSize = nFileSize;
//                        m_blockTable[blockNodeIndex] = t_FileBlockNode;
//                        if (!writeBlockNode((uint)blockNodeIndex))
//                            return false;
//                    }
//                    LogMgr.Log("Insert the same size....");
//                    //
//                }//尺寸发生变化，需要删除旧空间，分配新空间
//                else
//                {
//                    setBlockNodeUsed(ref t_FileBlockNode, false);
//                    m_blockTable[blockNodeIndex] = t_FileBlockNode;
//                    //将旧块加入空闲块列表
//                    m_mapFreeBlock.Add(upBoundBlockSize(t_FileBlockNode.nBlockSize), (uint)blockNodeIndex);

//                    m_fileHead.nData_HoleSize += upBoundBlockSize(t_FileBlockNode.nBlockSize);
//                    LogMgr.Log("HoleSize :{0}", m_fileHead.nData_HoleSize.ToString());
//                    //重新尝试分配空间
//                    KeyValuePair<int, int> nNewBlockPair = allocFreeBlock(nFileSize);
//                    int nNewBlockIndex = nNewBlockPair.Key;
//                    if (nNewBlockPair.Key < 0)
//                    {
//                        return false;
//                    }

//                    LogMgr.Log("Old Index Is :{0}", blockNodeIndex.ToString());
//                    LogMgr.Log("New Index Is :{0}", nNewBlockIndex.ToString());

//                    /*************************************************
//                     *          磁盘操作开始，保存文件
//                     ************************************************/
//                    if (nNewBlockPair.Value > 0)
//                    {
//                        LogMgr.Log("NewBlockPair Is :{0}", nNewBlockPair.Value.ToString());
//                        //分割大空间时产生的副空间
//                        if (!writeBlockNode((uint)(nNewBlockPair.Value)))
//                            return false;
//                    }

//                    //写入硬盘的文件

//                    if (sourceType == AXP_CONTENTS.AC_DISK_FILE)
//                    {
//                        if (!writeDiskFile(m_blockTable[nNewBlockIndex].nDataOffset, upBoundBlockSize(m_blockTable[nNewBlockIndex].nBlockSize), fileStream))
//                            return false;

//                        LogMgr.Log("WriteDiskFile : {0}", strContents);
//                    }
//                    else
//                    {
//                        if (!writeMemory(m_blockTable[nNewBlockIndex].nDataOffset, upBoundBlockSize(m_blockTable[nNewBlockIndex].nBlockSize), strContents, nContentlen))
//                            return false;
//                    }



//                    if (!writeBlockNode((uint)nNewBlockIndex))
//                        return false;

//                    //文件所在的新的BlockNode
//                    if (blockNodeIndex != nNewBlockIndex)
//                    {
//                        if (!writeBlockNode((uint)blockNodeIndex))
//                            return false;
//                    }

//                    if (!writeFileHead())
//                        return false;

//                    //确认写入磁盘
//                    if (bSaveAtOnce)
//                    {
//                        m_fileStream.Flush();
//                    }

//                    blockNodeIndex = nNewBlockIndex;
//                    LogMgr.Log("Insert a big block successed.");
//                }
//            }
//            else
//            {

//                LogMgr.Log("Insert  unity asset bundle doesn't exists!");

//                //获取一块合适空间
//                KeyValuePair<int, int> nNewBlockPair = allocFreeBlock(nFileSize);
//                blockNodeIndex = nNewBlockPair.Key;

//                if (blockNodeIndex < 0)
//                {
//                    return false;
//                }

//                /*************************************************
//                 *          磁盘操作开始，保存文件
//                 ************************************************/
//                if (nNewBlockPair.Value > 0)
//                {
//                    //分割大空间时产生的副空间
//                    if (!writeBlockNode((uint)(nNewBlockPair.Value)))
//                        return false;
//                }

//                //写入硬盘的文件

//                if (sourceType == AXP_CONTENTS.AC_DISK_FILE)
//                {
//                    if (!writeDiskFile(m_blockTable[blockNodeIndex].nDataOffset, upBoundBlockSize(m_blockTable[blockNodeIndex].nBlockSize), fileStream))
//                        return false;
//                }
//                else
//                {
//                    if (!writeMemory(m_blockTable[blockNodeIndex].nDataOffset, upBoundBlockSize(m_blockTable[blockNodeIndex].nBlockSize), strContents, nContentlen))
//                        return false;
//                }




//                if (!writeBlockNode((uint)blockNodeIndex))
//                    return false;
//                if (!writeFileHead())
//                    return false;



//            }
//            //确认写入磁盘
//            if (bSaveAtOnce)
//            {
//                m_fileStream.Flush();
//            }
//            //if (isUpdateListDynamic() && strFileInPakName.Contains("(list)"))
//            //{
//            //    insertFileList(strFilePathName,strFileInPakName);
//            //}

//            return true;
//        }
//        /*************************************************
//         * 
//         *           Block表操作
//         * 
//         ************************************************/
//        private static uint upBoundBlockSize(uint nsize)
//        {
//            if ((nsize & 0xFF) == 0 && nsize != 0)
//                return nsize;
//            else
//            {
//                uint temp = (uint)(nsize & (~0xFF)) + 0x100;
//                return temp;

//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="blocknode"></param>
//        /// <returns></returns>

//        private static bool getBlockNodeUsed(File_BlockNode blocknode)
//        {
//            return (blocknode.nFlags & 0x80000000) != 0;
//        }

//        private static void setBlockNodeUsed(ref File_BlockNode blockNode, bool bUsed)
//        {
//            if (bUsed)
//                blockNode.nFlags = (blockNode.nFlags & 0x7FFFFFFF) + 0x80000000;
//            else
//                blockNode.nFlags = blockNode.nFlags & 0x7FFFFFFF;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="nSize"></param>
//        /// <returns></returns>
//        private KeyValuePair<int, int> allocFreeBlock(uint nSize)
//        {
//            uint nBoundFileSize = upBoundBlockSize(nSize);

//            // find the block that can be used.
//            //m_mapFreeBlock.ContainsKey(nBoundFileSize);

//            //uint[] t_values = m_mapFreeBlock.Values.ToArray();
//            //uint[] t_keyArray = m_mapFreeBlock.ContainsKey


//            //also use linq to select the key?? 
//            uint iter = 0;
//            {
//                uint sizeErr = uint.MaxValue, tempErr;
//                foreach (KeyValuePair<uint, HashSet<uint>> item in m_mapFreeBlock)
//                {
//                    if (item.Key > nBoundFileSize && (tempErr = (item.Key - nBoundFileSize)) < sizeErr) //找最小区间块
//                    {
//                        iter = item.Key;
//                        sizeErr = tempErr;
//                    }


//                }
//            }
//            if (iter != 0)
//            {
//                uint nBoundBlockSize = iter;
//                int nRetIndex = -1;
//                //nRetIndex = (int)m_mapFreeBlock[iter];
//                HashSet<uint> t_Values = m_mapFreeBlock.GetValues(iter, true);
//                nRetIndex = (int)t_Values.ElementAt(0);


//                int nRetSecIndex = -1;

//                //
//                m_mapFreeBlock.Remove(iter, t_Values.ElementAt(0));
//                m_fileHead.nData_HoleSize -= nBoundBlockSize;

//                //
//                uint actualSize = nBoundBlockSize - nBoundFileSize;
//                if (actualSize > 255)
//                {
//                    //需要分割比较大的块
//                    File_BlockNode newBlock = new File_BlockNode();
//                    newBlock.nBlockSize = actualSize - 255; //新块大小 ,初始化时会有256Byte对齐
//                    newBlock.nDataOffset = m_blockTable[nRetIndex].nDataOffset + nBoundFileSize;    //剩余的空间
//                    newBlock.nFlags = 0; //清空标记

//                    setBlockNodeUsed(ref newBlock, false);
//                    m_blockTable.Add(newBlock);

//                    //
//                    m_fileHead.nBlockTable_Count += 1;

//                    //加入空闲块
//                    m_mapFreeBlock.Add(actualSize, (uint)(m_blockTable.Count - 1));

//                    m_fileHead.nData_HoleSize += actualSize;
//                    nRetSecIndex = m_blockTable.Count - 1;
//                }


//                //设置新的数据
//                //m_blockTable[nRetIndex].nBlockSize = nSize;
//                File_BlockNode t_Block = m_blockTable[nRetIndex];
//                t_Block.nBlockSize = nSize;
//                setBlockNodeUsed(ref t_Block, true);
//                m_blockTable[nRetIndex] = t_Block;

//                return new KeyValuePair<int, int>(nRetIndex, nRetSecIndex);
//            }


//            //没有合适的空闲块，分配新块
//            File_BlockNode t_NewBlock = new File_BlockNode();
//            t_NewBlock.nBlockSize = nSize;
//            t_NewBlock.nDataOffset = (uint)m_fileHead.nData_Offset + m_fileHead.nData_Size;
//            t_NewBlock.nFlags = 0;

//            setBlockNodeUsed(ref t_NewBlock, true);
//            m_blockTable.Add(t_NewBlock);

//            //计数加1
//            m_fileHead.nBlockTable_Count += 1;
//            //数据区扩大
//            m_fileHead.nData_Size += upBoundBlockSize(nSize);

//            return new KeyValuePair<int, int>(m_blockTable.Count - 1, -1);
//        }

//        /*************************************************
//         * 
//         *           磁盘表操作
//         * 
//         ************************************************/
//        private bool writeFileHead()
//        {
//            if (m_fileStream == null)
//                return false;

//            //if (m_bConst)
//            //{
//            //    return false;
//            //}

//            m_fileStream.Seek(0);

//            //写FileHeader数据
//            //File_HashNode t_FileHashNode = m_HashTableList[(int)nHashIndex];


//            byte[] t_FileHeadBytes = new byte[m_FileHeadSize];
//            IntPtr t_FileHeadPtr = Marshal.AllocHGlobal(m_FileHeadSize);
//            Marshal.StructureToPtr(m_fileHead, t_FileHeadPtr, false);
//            Marshal.Copy(t_FileHeadPtr, t_FileHeadBytes, 0, m_FileHeadSize);
//            Marshal.FreeHGlobal(t_FileHeadPtr);

//            m_fileStream.Write(t_FileHeadBytes);

//            return true;
//        }

//        private bool writeBlockNode(uint nBlockIndex)
//        {
//            if (nBlockIndex >= (uint)m_blockTable.Count || m_fileStream == null)
//            {
//                return false;
//            }

//            //if (m_bConst)
//            //{
//            //    return false;
//            //}

//            m_fileStream.Seek(m_fileHead.nBlockTable_Offset + (int)nBlockIndex * m_FileBlockNodeSize);

//            //写Block数据
//            File_BlockNode t_FileBlockNode = m_blockTable[(int)nBlockIndex];


//            byte[] t_FileBlockNodeBytes = new byte[m_FileBlockNodeSize];
//            IntPtr t_FileBlockNodePtr = Marshal.AllocHGlobal(m_FileBlockNodeSize);
//            Marshal.StructureToPtr(t_FileBlockNode, t_FileBlockNodePtr, false);
//            Marshal.Copy(t_FileBlockNodePtr, t_FileBlockNodeBytes, 0, m_FileBlockNodeSize);
//            Marshal.FreeHGlobal(t_FileBlockNodePtr);

//            m_fileStream.Write(t_FileBlockNodeBytes);
//            LogMgr.Log(nBlockIndex.ToString());
//            return true;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <param name="nWriteSize"></param>
//        /// <param name="strDiskFile"></param>
//        /// <returns></returns>
//        private bool writeDiskFile(uint offset, uint nWriteSize, string strDiskFile)
//        {
//            if (string.IsNullOrEmpty(strDiskFile))
//                return false;

//            //if (m_bConst)
//            //{
//            //    return false;
//            //}

//            m_fileStream.Seek((int)offset);

//            FileStream fs = new FileStream(strDiskFile, FileMode.Open);
//            BinaryReader br = new BinaryReader(fs);

//            if (fs == null)
//                return false;

//            //byte[] t_ReadBytes = new byte[nWriteSize];
//            //fs.Read(t_ReadBytes, 0, (int)nWriteSize);


//            const int MAX_BUFFER_SIZE = 4096;

//            byte[] buffer = br.ReadBytes(MAX_BUFFER_SIZE);
//            //byte[] buffer = Enumerable.Repeat((byte)0, MAX_BUFFER_SIZE).ToArray();
//            int nReadSuccessNum = buffer.Length;
//            //int nReadSuccessNum = br.Read(buffer);



//            //实际已经写入的尺寸
//            uint nActWriteSize = 0;

//            do
//            {
//                if (nReadSuccessNum == 0)
//                    break;

//                //超过容量
//                //Unity 把Assert给屏蔽了吗？
//                Trace.Assert(nActWriteSize + nReadSuccessNum <= nWriteSize);
//                if (nActWriteSize + nReadSuccessNum > nWriteSize)
//                {
//                    nReadSuccessNum = (int)(nWriteSize - nActWriteSize);
//                }


//                //write
//                m_fileStream.Write(buffer);


//                nActWriteSize += (uint)nReadSuccessNum;

//                if (nActWriteSize >= nWriteSize)
//                {
//                    break;
//                }

//                buffer = br.ReadBytes(MAX_BUFFER_SIZE);

//                nReadSuccessNum = buffer.Length;



//            } while (true);

//            br.Close();
//            fs.Close();


//            //如果写入的尺寸长度不够，用0补齐
//            while (nActWriteSize < nWriteSize)
//            {
//                int nThisWrite = (int)Math.Min(MAX_BUFFER_SIZE, nWriteSize - nActWriteSize);

//                buffer = Enumerable.Repeat((byte)0, nThisWrite).ToArray();

//                m_fileStream.Write(buffer);

//                nActWriteSize += (uint)nThisWrite;
//            }
//            return true;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <param name="nWriteSize"></param>
//        /// <param name="strDiskFile"></param>
//        /// <returns></returns>
//        private bool writeDiskFile(uint offset, uint nWriteSize, FileStream fs)
//        {
//            if (fs == null)
//                return false;

//            //if (m_bConst)
//            //{
//            //    return false;
//            //}

//            m_fileStream.Seek((int)offset);

//            BinaryReader br = new BinaryReader(fs);

//            //byte[] t_ReadBytes = new byte[nWriteSize];
//            //fs.Read(t_ReadBytes, 0, (int)nWriteSize);


//            const int MAX_BUFFER_SIZE = 4096;

//            byte[] buffer = br.ReadBytes(MAX_BUFFER_SIZE);
//            //byte[] buffer = Enumerable.Repeat((byte)0, MAX_BUFFER_SIZE).ToArray();
//            int nReadSuccessNum = buffer.Length;
//            //int nReadSuccessNum = br.Read(buffer);



//            //实际已经写入的尺寸
//            uint nActWriteSize = 0;

//            do
//            {
//                if (nReadSuccessNum == 0)
//                    break;

//                //超过容量
//                //Unity 把Assert给屏蔽了吗？
//                Trace.Assert(nActWriteSize + nReadSuccessNum <= nWriteSize);
//                if (nActWriteSize + nReadSuccessNum > nWriteSize)
//                {
//                    nReadSuccessNum = (int)(nWriteSize - nActWriteSize);
//                }


//                //write
//                m_fileStream.Write(buffer);


//                nActWriteSize += (uint)nReadSuccessNum;

//                if (nActWriteSize >= nWriteSize)
//                {
//                    break;
//                }

//                buffer = br.ReadBytes(MAX_BUFFER_SIZE);

//                nReadSuccessNum = buffer.Length;



//            } while (true);

//            br.Close();
//            fs.Close();


//            //如果写入的尺寸长度不够，用0补齐
//            while (nActWriteSize < nWriteSize)
//            {
//                int nThisWrite = (int)Math.Min(MAX_BUFFER_SIZE, nWriteSize - nActWriteSize);

//                buffer = Enumerable.Repeat((byte)0, nThisWrite).ToArray();

//                m_fileStream.Write(buffer);

//                nActWriteSize += (uint)nThisWrite;
//            }
//            return true;
//        }
//        /// <summary>
//        /// 将一段内存写入文件。
//        /// </summary>
//        /// <param name="noffset"></param>
//        /// <param name="nWriteSize"></param>
//        /// <param name="strMemory"></param>
//        /// <param name="nMemorySize"></param>
//        /// <returns></returns>
//        private bool writeMemory(uint noffset, uint nWriteSize, byte[] strMemory, uint nMemorySize)
//        {
//            if (m_fileStream == null)
//                return false;

//            if (m_bConst)
//                return false;
//            //
//            m_fileStream.Seek(noffset);
//            //超过容量
//            if (nMemorySize > nWriteSize)
//            {
//                return false;
//            }

//            m_fileStream.Write(strMemory);

//            //如果写入的尺寸长度不够，用0补齐

//            if (nMemorySize < nWriteSize)
//            {
//                int nThisWrite = (int)(nWriteSize - nMemorySize);
//                byte[] buffer = Enumerable.Repeat((byte)0, nThisWrite).ToArray();
//                //@todo whether or not this is write or wrong.......
//                m_fileStream.Write(buffer);
//            }
//            return true;
//        }

//        private T[] GetArrayOfStruct<T>(IntPtr pointerToStruct, int count)
//        {
//            int sizeInBytes = Marshal.SizeOf(typeof(T));
//            T[] output = new T[count];
//            IntPtr p = IntPtr.Zero;
//            for (int index = 0; index < count; index++)
//            {
//                //IntPtr p = new IntPtr((pointerToStruct.ToInt32() + index * sizeInBytes));
//#if UNITY_STANDALONE || UNITY_EDITOR
//                p = new IntPtr((pointerToStruct.ToInt64() + index * sizeInBytes));
//#else //UNITY_IOS
//    //            UnityEngine.iOS.DeviceGeneration t_DeviceGeneration = UnityEngine.iOS.Device.generation;
//				//if(UnityEngine.iOS.Device.generation < UnityEngine.iOS.DeviceGeneration.iPhone5C)
//				//{
//				//	p = new IntPtr((pointerToStruct.ToInt32() + index * sizeInBytes));
//				//}
//				//else
//				//{
//				//	p = new IntPtr((pointerToStruct.ToInt64() + index * sizeInBytes));
//				//}
//                if (IntPtr.Size == 8)
//                {
//                    //LogMgr.Log("64 bit.");
//                    p = new IntPtr((pointerToStruct.ToInt64() + index * sizeInBytes));
//                }
//                else if (IntPtr.Size == 4)
//                {
//                    //LogMgr.Log("32 bit.");
//                    p = new IntPtr((pointerToStruct.ToInt32() + index * sizeInBytes));
//                }
//                else
//                {
//                    Games.TLBB.Log.LogSystem.Error("IntPtr Size {0} Error!!!", IntPtr.Size);
//                    Application.Quit();
//                }
//#endif
//                //                 if (IntPtr.Size == 8)
//                //                 {
//                //                     //LogMgr.Log("64 bit.");
//                //                     p = new IntPtr((pointerToStruct.ToInt64() + index * sizeInBytes));
//                //                 }
//                //                 else if (IntPtr.Size == 4)
//                //                 {
//                //                     //LogMgr.Log("32 bit.");
//                //                     p = new IntPtr((pointerToStruct.ToInt32() + index * sizeInBytes));
//                //                 }
//                //                 else
//                //                 {
//                //                     Games.TLBB.Log.LogSystem.Error("IntPtr Size {0} Error!!!", IntPtr.Size);
//                //                     Application.Quit();
//                //                 }
//                output[index] = (T)Marshal.PtrToStructure(p, typeof(T));
//            }
//            return output;
//        }
//    }
//}
//#endif