using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

class HttpRequestSample : LycheeSDK.HttpRequest
{
    public HttpRequestSample(string url)
    {
        web_ = UnityWebRequest.Get(url);
        range_ = false;
    }

    public HttpRequestSample(string url, int offset, int length)
    {
        web_ = UnityWebRequest.Get(url);
        web_.SetRequestHeader("Range", "bytes=" + offset + "-" + (offset + length - 1));
        range_ = true;
    }

    public bool IsError()
    {
        return (web_.isNetworkError || (range_ ? web_.responseCode != 206 : web_.responseCode != 200));
    }

    public string ErrorMessage()
    {
        return (web_.isNetworkError ? web_.error : web_.downloadHandler.text);
    }

    public IEnumerator Send()
    {
        yield return web_.Send();
    }

    public string Text()
    {
        return web_.downloadHandler.text;
    }

    public byte[] Data()
    {
        return web_.downloadHandler.data;
    }

    UnityWebRequest web_;
    bool range_;
}

class HttpSample : LycheeSDK.Http
{
    public LycheeSDK.HttpRequest Get(string url)
    {
        return new HttpRequestSample(url);
    }

    public LycheeSDK.HttpRequest Get(string url, int offset, int length)
    {
        return new HttpRequestSample(url, offset, length);
    }
}

class FileSystemSample : LycheeSDK.FileSystem
{
    public string initialPath
    {
        get
        {
#if UNITY_EDITOR_WIN
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "bundles\\Windows");
#elif UNITY_EDITOR_OSX
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "bundles\\MacOS");
#elif !UNITY_EDITOR && UNITY_ANDROID
            throw new System.InvalidOperationException();
#else
            return Path.Combine(Application.dataPath, "bundles");
#endif
        }
    }

    public string downloadPath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "download");
        }
    }

    public string updatePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "update");
        }
    }

    public FileSystemSample()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        apkFS_ = new LycheeSDK.Android.APKFileSystem("bundles");
#endif

        if (!Directory.Exists(downloadPath))
        {
            Directory.CreateDirectory(downloadPath);
        }

        if (!Directory.Exists(updatePath))
        {
            Directory.CreateDirectory(updatePath);
        }
    }

    public void Copy(LycheeSDK.Location location, string filename, LycheeSDK.Location location2, string filename2)
    {
        Stream stream = Open(location, filename, FileMode.Open, FileAccess.Read);
        Stream stream2 = Open(location2, filename2, FileMode.Create, FileAccess.Write);

        byte[] buffer = new byte[65536];

        int count;

        while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            stream2.Write(buffer, 0, count);
        }

        stream.Close();
        stream2.Close();
    }

    public void Delete(LycheeSDK.Location location, string filename)
    {
        if (location != LycheeSDK.Location.Initial)
        {
            File.Delete(GetPath(location, filename));
        }
    }

    public bool Exists(LycheeSDK.Location location, string filename)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (location == LycheeSDK.Location.Initial)
        {
            return apkFS_.Exists(filename);
        }
#endif

        return File.Exists(GetPath(location, filename));
    }

    public string[] GetFiles(LycheeSDK.Location location)
    {
        List<string> list = new List<string>();

#if !UNITY_EDITOR && UNITY_ANDROID
        if (location == LycheeSDK.Location.Initial)
        {
            foreach (string path in apkFS_.GetFiles())
            {
                if (!path.Contains("/")) {
                    list.Add(path);
                }
            }

            return list.ToArray();
        }
#endif

        foreach (string path in Directory.GetFiles(LocationToPath(location),
            "*.*", SearchOption.TopDirectoryOnly))
        {
            list.Add(Path.GetFileName(path));
        }

        return list.ToArray();
    }

    public Stream Open(LycheeSDK.Location location, string filename, FileMode mode, FileAccess access)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (location == LycheeSDK.Location.Initial) {
            return apkFS_.OpenRead(filename);
        }
#endif

        if (access == FileAccess.Read)
        {
            return File.Open(GetPath(location, filename), mode, access, FileShare.Read);
        }

        return File.Open(GetPath(location, filename), mode, access);
    }

    public string GetPath(LycheeSDK.Location location, string filename)
    {
        return Path.Combine(LocationToPath(location), filename);
    }

    public string LocationToPath(LycheeSDK.Location location)
    {
        switch (location)
        {
            case LycheeSDK.Location.Initial:
                return initialPath;

            case LycheeSDK.Location.Download:
                return downloadPath;

            case LycheeSDK.Location.Update:
                return updatePath;
        }

        throw new DirectoryNotFoundException();
    }

#if !UNITY_EDITOR && UNITY_ANDROID
    LycheeSDK.Android.APKFileSystem apkFS_;
#endif
}

