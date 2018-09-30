
using UnityEngine;

namespace com.cyou.plugin.resource.loader.bundle
{
    public interface IAssetBundlerLoader
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init();
        /// <summary>
        /// 同步加载AssetBundle
        /// </summary>
        /// <param name="bundleName">bundle名字</param>
        /// <returns></returns>
        AssetBundle LoadBundle(string bundleName);
        /// <summary>
        /// 异步加载AssetBundle
        /// </summary>
        /// <param name="bundleName">Bundle名字</param>
        /// <returns></returns>
        UnityEngine.AssetBundleCreateRequest LoadBundleAsync(string bundleName);
       
    }
}
