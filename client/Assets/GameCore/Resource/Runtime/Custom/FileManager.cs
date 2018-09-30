using System;
using System.IO;
using LycheeSDK;
using LycheeSDK.Android;
using UnityEngine;
using System.Collections.Generic;

namespace cyou.ldj.sdk
{
    public class FileManager : FileSystem
    {
        private APKFileSystem mAPKFileSystem;
        private string mInitialRoot;
        private string mUpdateRoot;
        private string mDownloadRoot;

        public FileManager()
        {
#if UNITY_EDITOR
            mInitialRoot = Path.GetFullPath("E:/test/Windows/1.1");
            mUpdateRoot = Path.GetFullPath("E:/test/Windows/1.1");
            mDownloadRoot = Path.GetFullPath("E:/test/Windows/1.1");
#else
            mInitialRoot = Path.GetFullPath(Application.streamingAssetsPath + "/bundles");
            mUpdateRoot = Path.GetFullPath(Application.persistentDataPath + "/update");
            mDownloadRoot = Path.GetFullPath(Application.persistentDataPath + "/download");
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            mAPKFileSystem = new APKFileSystem("bundles");
#endif
            if (!Directory.Exists(mDownloadRoot))
            {
                Directory.CreateDirectory(mDownloadRoot);
            }
            if(!Directory.Exists(mUpdateRoot))
            {
                Directory.CreateDirectory(mUpdateRoot);
            }
        }

        public void Copy(Location location, string filename, Location location2, string filename2)
        {
            Stream stream = Open(location, filename, FileMode.Open, FileAccess.Read);
            Stream stream2 = Open(location2, filename2, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[Config.chunkSize];

            int count;

            while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                stream2.Write(buffer, 0, count);
            }

            stream.Close();
            stream2.Close();
        }

        public void Delete(Location location, string filename)
        {
            if (location != Location.Initial)
            {
                File.Delete(GetPath(location, filename));
            }
        }

        public bool Exists(Location location, string filename)
        {
            if (location == Location.Initial && Application.platform == RuntimePlatform.Android)
            {
                return mAPKFileSystem.Exists(filename);
            }
            else
            {
                return File.Exists(GetPath(location, filename));
            }
        }

        public string[] GetFiles(Location location)
        {
            List<string> list = new List<string>();

            if (location == Location.Initial && Application.platform == RuntimePlatform.Android)
            {
                foreach (string path in mAPKFileSystem.GetFiles())
                {
                    if (!path.Contains("/"))
                    {
                        list.Add(path);
                    }
                }

                return list.ToArray();
            }
            else
            {
                foreach (string path in Directory.GetFiles(LocationToPath(location),"*.*", SearchOption.TopDirectoryOnly))
                {
                    list.Add(Path.GetFileName(path));
                }

                return list.ToArray();
            }
        }

        public Stream Open(Location location, string filename, FileMode mode, FileAccess access)
        {
            if (location == Location.Initial && Application.platform == RuntimePlatform.Android)
            {
                return mAPKFileSystem.OpenRead(filename);
            }
            else
            {
                if (access == FileAccess.Read)
                {
                    return File.Open(GetPath(location, filename), mode, access, FileShare.Read);
                }

                return File.Open(GetPath(location, filename), mode, access);
            }
        }

        public string GetPath(Location location, string filename)
        {
            return Path.Combine(LocationToPath(location), filename);
        }

        public string LocationToPath(Location location)
        {
            switch (location)
            {
                case Location.Initial:
                    return mInitialRoot;

                case Location.Download:
                    return mDownloadRoot;

                case Location.Update:
                    return mUpdateRoot;
            }

            throw new DirectoryNotFoundException();
        }
    }
}
