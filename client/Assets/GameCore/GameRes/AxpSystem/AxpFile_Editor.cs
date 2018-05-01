#if UNITY_EDITOR

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;
using System.Text;


namespace AxpTools
{
    public class AxpFile_Editor
    {
        private const uint AXPK_FILE_FLAG = 0x4B505841; //文件头，AXP标示
        private const int BLOCK_TABLE_MAXSIZE = 384 * 1024;////1024 * 1024;   //Block Table区间最大大小

        [StructLayout(LayoutKind.Sequential)]
        struct File_Head
        {
            public uint nIdentity;
            //public uint nVersion;
            public int gameversion;
            public int resfirstversion;
            public int ressecondversion;
            public int resthirdversion;
            public int fileID;
            public int nEditFlag;
            public int nBlockTable_Offset;
            public int nBlockTable_Count;
            public uint nBlockTable_MaxSize;
            public int nData_Offset;
            public uint nData_Size;
            public uint nData_HoleSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct File_BlockNode
        {
            public uint nDataOffset;
            public uint nBlockSize;
            public uint nFlags;
        }

        //block table
        List<File_BlockNode> m_blockTable = new List<File_BlockNode>();

        MultiValueDictionary<uint, uint> m_mapFreeBlock = new MultiValueDictionary<uint, uint>();

        public int id { get { return m_bOpenState ? m_fileHead.fileID : -1; } }
        public bool isOpenState { get { return m_bOpenState; } }
        public bool isReadOnly { get { return m_bConst; } }
        public uint dataSize { get { return m_fileHead.nData_Size; } }
        public uint fileCount { get { return m_FileCount; } }
        public AxpFilePath axpFilePath { get { return m_path; } }
        public string m_strPathFileName;                        //file & path name
        public string m_strFileName;                            //only file name.
        bool m_bOpenState;                               //axp file open state.
        bool m_bConst;                                //whether or not edit mode.

        AxpFilePath m_path;

        File_Head m_fileHead;

        AxpFileStream m_fileStream;

        int m_FileHeadSize = 0;
        int m_FileBlockNodeSize = 0;
        uint m_FileCount = 0;


        public AxpFile_Editor()
        {
            m_bOpenState = false;
        }


        /// <summary>
        /// export axp contain file, only use in editor mode.
        /// </summary>
        /// <param name="strPackFileName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ExportAxpFile(string strPackFileName,AxpMap axpfilemap, string exportpath, int fileid, AxpFilePath path)
        {
            Games.TLBB.Log.LogSystem.Info("Enter open Pak File Module. {0}", strPackFileName);

            if (string.IsNullOrEmpty(strPackFileName))
            {
                return false;
            }

            //generate data
            m_strPathFileName = strPackFileName;

            //whether or not file exist.
            m_fileStream = AxpFileStream.OpenFile(m_strPathFileName, path != AxpFilePath.PersistentDataPath);

            if (m_fileStream == null)
            {
                Games.TLBB.Log.LogSystem.Info("Open Axp Failed.");
                return false;
            }

            /***********************************************************
             *                  Read FileHead
             * ********************************************************/

            m_FileHeadSize = Marshal.SizeOf(m_fileHead);
            //byte[] t_readfileheadBytes = new byte[m_FileHeadSize];
            //m_FileReader.Read(t_readfileheadBytes, 0, m_FileHeadSize);
            byte[] t_readfileheadBytes = m_fileStream.ReadBytes(m_FileHeadSize);

            //alloc struct memory size.
            //read file header.
            IntPtr t_fileHeadPtr = Marshal.AllocHGlobal(m_FileHeadSize);
            Marshal.Copy(t_readfileheadBytes, 0, t_fileHeadPtr, m_FileHeadSize);
            m_fileHead = (File_Head)Marshal.PtrToStructure(t_fileHeadPtr, typeof(File_Head));
            Marshal.FreeHGlobal(t_fileHeadPtr);

            Games.TLBB.Log.LogSystem.Info("Axp GameVersion : {0} , ResFirstVersion : {1} , ResSecondVersion : {2},  ResThirdVersion: {3}", m_fileHead.gameversion, m_fileHead.resfirstversion, m_fileHead.ressecondversion, m_fileHead.resthirdversion);
            /***********************************************************
             *                  Read BlockTable
             * ********************************************************/

            File_BlockNode t_FileBlockNode = new File_BlockNode();
            m_FileBlockNodeSize = Marshal.SizeOf(t_FileBlockNode);
            byte[] t_readblockTableBytes = m_fileStream.ReadBytes(m_FileBlockNodeSize * m_fileHead.nBlockTable_Count);

            //convert data
            IntPtr t_BlockTablePtr = Marshal.AllocHGlobal(m_FileBlockNodeSize * m_fileHead.nBlockTable_Count);
            Marshal.Copy(t_readblockTableBytes, 0, t_BlockTablePtr, m_FileBlockNodeSize * m_fileHead.nBlockTable_Count);
            File_BlockNode[] t_TempBlockTableArray = GetArrayOfStruct<File_BlockNode>(t_BlockTablePtr, m_fileHead.nBlockTable_Count);
            Marshal.FreeHGlobal(t_BlockTablePtr);

            m_blockTable.AddRange(t_TempBlockTableArray);

            List<string> t_TableInfo = new List<string>();
            
            if (!Directory.Exists(exportpath))
            {
                Directory.CreateDirectory(exportpath);
            }
            //最好做一个排序型检测
            for (int index = 0; index < m_blockTable.Count; index++)
            {
                string t_StrInfo = null;
                File_BlockNode t_blockNode = m_blockTable[index];
                if (t_blockNode.nDataOffset < m_fileHead.nData_Offset)
                {
                    return false;
                }

                if (!getBlockNodeUsed(t_blockNode))
                {
                    string filename = axpfilemap.GetFileNameFromMap(fileid, index);
                    t_StrInfo += index.ToString() + '\t' + filename;
                    t_StrInfo += '\t' + "BlockSize :" + '\t' + t_blockNode.nBlockSize.ToString() + '\t' + "DataOffset:" + '\t' + t_blockNode.nDataOffset.ToString() + '\t' + "UnUsed.";
                    t_TableInfo.Add(t_StrInfo);

                    m_mapFreeBlock.Add(upBoundBlockSize(t_blockNode.nBlockSize), (uint)index);
                }
                else
                {
                    string filename = axpfilemap.GetFileNameFromMap(fileid, index);

                    byte[] data = openFileByBuffer(index,out t_blockNode.nBlockSize, out t_blockNode.nDataOffset);
                    string t_filepath = exportpath + "/" + filename;
                    string t_filepathname = t_filepath.Substring(0, t_filepath.LastIndexOf('/'));
                    if(!Directory.Exists(t_filepathname))
                    {
                        Directory.CreateDirectory(t_filepathname);
                    }
                    FileStream t_fileStream = new FileStream(t_filepath, FileMode.Create, FileAccess.Write);
                    t_fileStream.Write(data, 0, data.Length);
                    t_fileStream.Close();

                    t_StrInfo += index.ToString()  + '\t' + filename;
                    t_StrInfo += '\t' + "BlockSize :" + '\t' + t_blockNode.nBlockSize.ToString() + '\t' + "DataOffset:" + '\t' + t_blockNode.nDataOffset.ToString() + '\t' + "Used.";
                    t_TableInfo.Add(t_StrInfo);

                }

            }


            StreamWriter sw = File.CreateText(exportpath + "/AxpFileTable" + fileid.ToString() + ".txt");
            foreach (string str in t_TableInfo)
            {
                sw.WriteLine(str);
            }
            sw.Close();
            sw.Dispose();
            t_TableInfo.Clear();

            m_FileCount = (uint)m_blockTable.Count;
            m_bOpenState = true;
            m_path = path;
            m_bConst = (m_fileHead.nEditFlag & 1) == 0;
            Games.TLBB.Log.LogSystem.Info("Axp File Load Successed");
            return true;
        }


        public bool closePakFile()
        {
            m_fileStream.Close();
            m_fileStream = null;
            m_bOpenState = false;
            return true;
        }
        private static bool getBlockNodeUsed(File_BlockNode blocknode)
        {
            return (blocknode.nFlags & 0x80000000) != 0;
        }

        private static uint upBoundBlockSize(uint nsize)
        {
            if ((nsize & 0xFF) == 0 && nsize != 0)
                return nsize;
            else
            {
                uint temp = (uint)(nsize & (~0xFF)) + 0x100;
                return temp;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="dataSize"></param>
        /// <param name="offSet"></param>
        /// <returns></returns>
        public byte[] openFileByBuffer(int blockNodeIndex, out uint dataSize, out uint offSet)
        {
            dataSize = 0;
            offSet = 0;
            if (m_fileStream == null)
                return null;

            if (!openFile(blockNodeIndex, out dataSize, out offSet))
                return null;

            m_fileStream.Seek((long)offSet);

            byte[] t_readByte = m_fileStream.ReadBytes((int)dataSize);
            if (t_readByte == null)
                return null;
            if (t_readByte.Length == dataSize)
                return t_readByte;
            return null;
        }

        /// <summary>
        /// 以文件流方式打开包中的一个文件
        /// </summary>
        /// <param name="blockNodeIndex"></param>
        public bool openFile(int blockNodeIndex, out uint dataSize, out uint offSet)
        {
            dataSize = 0;
            offSet = 0;
            if (blockNodeIndex < 0 || m_blockTable.Count <= blockNodeIndex)
                return false;

            //得到block数据
            dataSize = m_blockTable[blockNodeIndex].nBlockSize;
            offSet = m_blockTable[blockNodeIndex].nDataOffset;
            //Games.TLBB.Log.LogSystem.Info("Open Axp File :{0} dataSize: {1} offset: {2}", m_strPathFileName, dataSize.ToString(), offSet.ToString());
            return true;
        }


        private T[] GetArrayOfStruct<T>(IntPtr pointerToStruct, int count)
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T));
            T[] output = new T[count];
            IntPtr p = IntPtr.Zero;
            for (int index = 0; index < count; index++)
            {
                //IntPtr p = new IntPtr((pointerToStruct.ToInt32() + index * sizeInBytes));
#if UNITY_STANDALONE || UNITY_EDITOR
                p = new IntPtr((pointerToStruct.ToInt64() + index * sizeInBytes));
#else //UNITY_IOS
    //            UnityEngine.iOS.DeviceGeneration t_DeviceGeneration = UnityEngine.iOS.Device.generation;
				//if(UnityEngine.iOS.Device.generation < UnityEngine.iOS.DeviceGeneration.iPhone5C)
				//{
				//	p = new IntPtr((pointerToStruct.ToInt32() + index * sizeInBytes));
				//}
				//else
				//{
				//	p = new IntPtr((pointerToStruct.ToInt64() + index * sizeInBytes));
				//}
                if (IntPtr.Size == 8)
                {
                    //Games.TLBB.Log.LogSystem.Info("64 bit.");
                    p = new IntPtr((pointerToStruct.ToInt64() + index * sizeInBytes));
                }
                else if (IntPtr.Size == 4)
                {
                    //Games.TLBB.Log.LogSystem.Info("32 bit.");
                    p = new IntPtr((pointerToStruct.ToInt32() + index * sizeInBytes));
                }
                else
                {
                    Games.TLBB.Log.LogSystem.Error("IntPtr Size {0} Error!!!", IntPtr.Size);
                    Application.Quit();
                }
#endif
                //                 if (IntPtr.Size == 8)
                //                 {
                //                     //Games.TLBB.Log.LogSystem.Info("64 bit.");
                //                     p = new IntPtr((pointerToStruct.ToInt64() + index * sizeInBytes));
                //                 }
                //                 else if (IntPtr.Size == 4)
                //                 {
                //                     //Games.TLBB.Log.LogSystem.Info("32 bit.");
                //                     p = new IntPtr((pointerToStruct.ToInt32() + index * sizeInBytes));
                //                 }
                //                 else
                //                 {
                //                     Games.TLBB.Log.LogSystem.Error("IntPtr Size {0} Error!!!", IntPtr.Size);
                //                     Application.Quit();
                //                 }
                output[index] = (T)Marshal.PtrToStructure(p, typeof(T));
            }
            return output;
        }

    }




 
}



#endif