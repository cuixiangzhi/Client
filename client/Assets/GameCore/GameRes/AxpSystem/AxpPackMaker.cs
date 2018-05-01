#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace AxpTools
{
    public class AxpPackMaker
    {

        public delegate bool AXP_PAKMAKER_COMPARECALLBACK(string strFileNameInDisk);

        protected struct FileNode
        {
            public bool bExecute;
            public string strFileInPak;
            public string strFileInDisk;
            public string strPakName;
            public uint nFileSize;
        }

        /* files's cache buffer.*/
        //protected Dictionary<string, FileNode> m_AllFiles = new Dictionary<string, FileNode>();
        protected List<string> m_AllFiles = new List<string>();
        protected List<string> m_filterListString = new List<string>();
        protected uint m_CurrentFilesSize = 0;
        protected int m_AxpFileNameCount = 0;
        public Uri rootUri;
        AxpMap map = AxpMap.Init();

        public bool addDiskFolder(string strFoldInDisk, string strFoldInPak, string strExtFilter, bool bRecursive = true)
        {
            string[] t_filterstring = strExtFilter.Split('|');
            m_filterListString.AddRange(t_filterstring);

            if (Directory.Exists(strFoldInDisk))
            {
                DirectoryInfo t_TempDir = new DirectoryInfo(strFoldInDisk);
                DirectoryInfo[] diArray = t_TempDir.GetDirectories();


                if (bRecursive)
                {
                    for (int index = 0; index < diArray.Length; index++)
                    {
                        addDiskFolder(diArray[index].FullName, strFoldInPak, strExtFilter, bRecursive);
                    }
                }



                FileInfo[] basefileinfo = t_TempDir.GetFiles();
                for (int index = 0; index < basefileinfo.Length; index++)
                {
                    //if (basefileinfo[index].Name.Substring(basefileinfo[index].Name.Length - strExtFilter.Length).Equals(strExtFilter))

                    if (m_filterListString.Contains(basefileinfo[index].Extension))
                    //if (basefileinfo[index].Extension.Equals(strExtFilter) || basefileinfo[index].Extension.Equals(""))
                    {
                        addDiskFile(basefileinfo[index].FullName);
                    }
                }
            }
            return true;
        }


        private bool addDiskFile(string strFileInDisk)
        {

            if (string.IsNullOrEmpty(strFileInDisk))
                return false;

            //m_AllFiles[strFileInPak] = newNode;
            m_AllFiles.Add(strFileInDisk);
            return true;
        }
        public bool savePakFileInAxpByFileType(string strPakFilePathName, string strPakFileName, uint nMaxAxpFileSize, int suffixlength, Dictionary<string, string> fileType)
        {
            map.CreatNewMap(nMaxAxpFileSize);
            Dictionary<string, List<string>> fileListWithFileType = new Dictionary<string, List<string>>();
            List<string> fileTypeList = new List<string>(fileType.Keys);
            List<string> normalFileList = new List<string>();   //Normal FileList
            string currentAxpName = strPakFileName.Insert(strPakFileName.Length - suffixlength - 1, m_AxpFileNameCount.ToString()); ;
            foreach (string item in m_AllFiles)
            {
                //Is it in the specific fileType List
                string fileExtension = Path.GetExtension(item);
                if( fileExtension.Equals(".txt"))
                {
                    int ww=10;
                }
                
                int index = fileTypeList.FindIndex((t) =>
                {
                    List<string> temp = new List<string>(t.Split('|'));
                    if (temp.Contains(fileExtension))
                        return true;
                    return false;
                });
                if (index>=0)
                {                
                    if (!fileListWithFileType.ContainsKey(fileTypeList[index]))
                        fileListWithFileType.Add(fileTypeList[index], new List<string>());
                    fileListWithFileType[fileTypeList[index]].Add(item);
                }
                else
                {
                    currentAxpName = strPakFileName.Insert(strPakFileName.Length - suffixlength - 1, m_AxpFileNameCount.ToString());// +m_AxpFileNameCount.ToString();

                    uint fileSize = AxpFileStream.getDiskFileSize(item);
                    m_CurrentFilesSize += fileSize;
                    if (m_CurrentFilesSize > nMaxAxpFileSize)
                    {
                        if (!savePakFile(strPakFilePathName, currentAxpName, normalFileList))
                            return false;
                        m_CurrentFilesSize = 0;
                        m_AxpFileNameCount++;
                        m_CurrentFilesSize += fileSize;
                        normalFileList.Clear();
                    }
                    normalFileList.Add(item);
                }
            }
            if (!savePakFile(strPakFilePathName, currentAxpName, normalFileList))
                return false;
            foreach (string item in fileListWithFileType.Keys)
            {
                normalFileList.Clear();
                m_AxpFileNameCount = 0;
                foreach (string file in fileListWithFileType[item])
                {
                    currentAxpName = strPakFileName.Insert(strPakFileName.Length - suffixlength - 1, fileType[item] + m_AxpFileNameCount.ToString());// +m_AxpFileNameCount.ToString();

                    uint fileSize = AxpFileStream.getDiskFileSize(file);
                    m_CurrentFilesSize += fileSize;
                    if (m_CurrentFilesSize > nMaxAxpFileSize)
                    {
                        if (!savePakFile(strPakFilePathName, currentAxpName, normalFileList))
                            return false;
                        m_CurrentFilesSize = 0;
                        m_AxpFileNameCount++;
                        m_CurrentFilesSize += fileSize;
                        normalFileList.Clear();
                    }
                    normalFileList.Add(file);
                }
                if (!savePakFile(strPakFilePathName, currentAxpName, normalFileList))
                    return false;
            }
            map.SaveToDir(strPakFilePathName + "/" + AxpMap.MAPNAME);
            return true;
        }
        public bool savePakFileInAxp(string strPakFilePathName, string strPakFileName, uint nMaxAxpFileSize, int suffixlength , int gameVersion = 0, int resFirstVersion = 0, int resSecondVersion = 0, int resThirdVersion = 0)
        {
            map.CreatNewMap(nMaxAxpFileSize, 10, gameVersion, resFirstVersion, resSecondVersion, resThirdVersion);
            List<string> t_CollectFiles = new List<string>();
            string t_strPakFileName = null;
            uint t_FileCount = 0;
            foreach (string keyitem in m_AllFiles)
            {
                t_strPakFileName = strPakFileName.Insert(strPakFileName.Length - suffixlength - 1, m_AxpFileNameCount.ToString());// +m_AxpFileNameCount.ToString();

                uint fileSize = AxpFileStream.getDiskFileSize(keyitem);
                m_CurrentFilesSize += fileSize;
                
                if (m_CurrentFilesSize > nMaxAxpFileSize || t_FileCount >= 32767)
                {
                    if (!savePakFile(strPakFilePathName, t_strPakFileName, t_CollectFiles, gameVersion, resFirstVersion, resSecondVersion, resThirdVersion))
                        return false;
                    m_CurrentFilesSize = 0;
                    m_AxpFileNameCount++;
                    m_CurrentFilesSize += fileSize;
                    t_FileCount = 0;
                    t_CollectFiles.Clear();
                }
                t_CollectFiles.Add(keyitem);
                t_FileCount++;
            }
            t_strPakFileName = strPakFileName.Insert(strPakFileName.Length - suffixlength - 1, m_AxpFileNameCount.ToString());
            if (!savePakFile(strPakFilePathName, t_strPakFileName, t_CollectFiles, gameVersion, resFirstVersion, resSecondVersion, resThirdVersion))
                return false;
            map.SaveToDir(strPakFilePathName + "/" + AxpMap.MAPNAME);
            return true;
        }

        private bool savePakFile(string strPakFilePathName, string strPakFileName, List<string> t_AllFiles, int gameVersion = 0, int resFirstVersion = 0, int resSecondVersion = 0, int resThirdVersion = 0)
        {
            if (string.IsNullOrEmpty(strPakFileName))
                return false;

            //Pak文件
            AxpFile pakFile = new AxpFile();
            if (!pakFile.createNewPakFile(strPakFilePathName + '/' + strPakFileName, map.GetNextID(), true, gameVersion, resFirstVersion, resSecondVersion, resThirdVersion))
            {
                return false;
            }

            //最终文件数量
            int nActFileCount = 0;

            //加入文件
            int t_Counts = t_AllFiles.Count;
            int t_Count = 0;
            float t_progress = 0.0f;
            foreach (string keyitem in t_AllFiles)
            {
                t_Count += 1;
                t_progress = (float)t_Count / (float)t_Counts;
                uint fileSize = AxpFileStream.getDiskFileSize(keyitem);
                EditorUtility.DisplayProgressBar("Add Resources in Axp File:" + strPakFileName, keyitem, t_progress);
                int blockNodeIndex = -1;
                if (!pakFile.insertContents(null, 0, keyitem, ref blockNodeIndex, AXP_CONTENTS.AC_DISK_FILE, false))
                {
                    return false;
                }
                nActFileCount++;
                //增加相对目录功能
                Uri fileUri = new Uri(keyitem);
                string relativePath = rootUri.MakeRelativeUri(fileUri).ToString();
                map.AddFileToMap(relativePath, pakFile.id, blockNodeIndex);

            }
            EditorUtility.ClearProgressBar();
            System.GC.Collect();

            //生成(list)文件

            //string strListFile = null;
            //if (!_generateListFile(t_AllFiles, strPakFilePathName, ref strListFile, nActFileCount, nTotalCRC))
            //    return false;

            ////加入(list)文件
            //if (!pakFile.insertContents(strListFile, 0, strPakFilePathName, AxpFile.LIST_FILENAME, AXP_CONTENTS.AC_DISK_FILE, false))
            //    return false;


            //@todo... 删除临时文件？？？？

            pakFile.closePakFile();

            return true;
        }

        /// <summary>
        /// 注意，该函数只提供给编辑器模式下使用
        /// </summary>
        public void ClearPackMakerInfo()
        {
            m_AllFiles.Clear();
            m_filterListString.Clear();
            m_CurrentFilesSize = 0;
            m_AxpFileNameCount = 0;
            rootUri = null;
        }


        public bool CloseAllFile()
        {
            return map.CloseAllFile();
        }

    }

}
#endif