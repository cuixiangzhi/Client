using UnityEngine.SceneManagement;

namespace com.cyou.plugin.res.callback
{
    /// <summary>
    /// 回调委托函数
    /// </summary>
    public class ResourceCallback
    {
        /// <summary>
        /// 资源加载回调
        /// </summary>
        /// <param name="index">异步返回id</param>
        /// <param name="obj">Object 对象</param>
        public delegate void DelegateResourceCallBack(int index, System.Object obj);

        /// <summary>
        /// 数组形式资源回调
        /// </summary>
        /// <param name="index">异步返回id</param>
        /// <param name="objArray">Object 数组</param>
        public delegate void DelegateResourceArrayCallBack(int index, System.Object[] objArray);
        /// <summary>
        /// 场景回调
        /// </summary>
        public delegate void DelegateSceneLoadedCallback(Scene scene, LoadSceneMode loadSceneMode);
    }
}
