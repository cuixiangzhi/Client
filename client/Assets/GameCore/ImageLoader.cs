using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace GameBase
{
    public class ImageLoader : MonoBehaviour
    {
        public bool debuggle = true;

        public delegate void OnLoadTexture(string path,Texture2D texture);

        private Dictionary<string, Texture2D> m_cache = new Dictionary<string, Texture2D>();

        private Dictionary<string, ImageLoadTask> m_loading = new Dictionary<string, ImageLoadTask>();

        private static ImageLoader s_instance;

        public static ImageLoader GetInstance()
        {
            if (s_instance == null)
            {
                var go = new GameObject("ImageLoader");
                s_instance = go.AddComponent<ImageLoader>();
                DontDestroyOnLoad(go);

            }
            return s_instance;
        }

        void Awake()
        {
            s_instance = this;
        }

        void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }

        public void Clear()
        {
            StopAllCoroutines();
            m_loading.Clear();
            m_cache.Clear();
        }

        public Texture2D LoadImage(string path)
        {

            var tex = GetFromCache(path);

            if (tex == null)
            {

                var data = File.ReadAllBytes(path);

                tex = new Texture2D(2, 2);

                tex.LoadImage(data);

                tex.Apply(false, true);
            }

            return tex;

        }

        public bool LoadImageAsync(string path, OnLoadTexture callback)
        {
            var tex = GetFromCache(path);

            if (tex == null)
            {
                ImageLoadTask task;
                if (!m_loading.TryGetValue(path, out task))
                {
                    task = new ImageLoadTask(path,callback);
                    m_loading.Add(path, task);
                    var co = StartCoroutine(_AsyncLoadImage(path));
                }
                else
                {
                    task.AddCallback(callback);
                }

            }
            else if (callback != null)
            {
                callback(path,tex);
            }

            return true;

        }

        Texture2D GetFromCache(string path)
        {
            Texture2D tex;
            if (m_cache.TryGetValue(path, out tex))
            {
                return tex;
            }
            return null;
        }

        IEnumerator _AsyncLoadImage(string path)
        {

            if (debuggle)
            {
                Debug.LogFormat("Start LoadImage {0}", path);
            }

            bool isFromWeb = false;
            string url;
            string filePath = path;

            if (!Path.IsPathRooted(path))
            {
                filePath = Application.persistentDataPath + "/" + path;
            }
            url = "file:///" + filePath;

            var request = new WWW(url);
            yield return request;

            if (isFromWeb)
            {
                OnResourceDownLoaded(path, request);
                if (!string.IsNullOrEmpty(filePath))
                {
                    File.WriteAllBytes(filePath, request.bytes);
                }
            }
            else
            {
                OnResourceLoaded(path, request);
            }

        }

        void OnResourceDownLoaded(string path, WWW request)
        {
            ImageLoadTask task;
            if (m_loading.TryGetValue(path, out task))
            {
                m_loading.Remove(path);
                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogErrorFormat("LoadImage {0} error:{1}", path, request.error);
                    task.SetDone(null);
                }
                else
                {
                    m_cache.Add(path, request.texture);
                    task.SetDone(request.texture);
                }
            }
        }

        void OnResourceLoaded(string path, WWW request)
        {
            ImageLoadTask task;
            if (m_loading.TryGetValue(path, out task))
            {
                m_loading.Remove(path);
                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogErrorFormat("LoadImage {0} error:{1}", path, request.error);
                    task.SetDone(null);
                }
                else
                {
                    m_cache.Add(path, request.texture);
                    task.SetDone(request.texture);
                }
            }
        }

        public class ImageLoadTask
        {
            private string mPath;
            private OnLoadTexture mCallBack;

            public ImageLoadTask(string path,OnLoadTexture callBack)
            {
                mPath = path;
                mCallBack = callBack;
            }

            public void SetDone(Texture2D texture)
            {
                if (mCallBack != null)
                {
                    mCallBack(mPath,texture);
                }
            }

            public void AddCallback(OnLoadTexture cb)
            {
                if (cb != null)
                {
                    mCallBack += cb;
                }
            }
        } 
    }
}


