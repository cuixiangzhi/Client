using System;
namespace com.cyou.plugin.resource
{  
    /// <summary>
    /// Config基类
    /// </summary>
    public abstract class IResConfig
    {      
        /// <summary>
        /// ResID,Table表中的id
        /// </summary>
        public Int32 ID
        {
            get;
            set;
        }  
    }    
    public class EditorResConfig : IResConfig
    {
        /// <summary>
        ///  资源路径，例如：Assets/UI/Prefab/UI_Main_Task.prefab
        /// </summary>
        public String MainPath
        {
            get;
            set;
        }
       
    }    
    public class BundleResConfig : IResConfig
    {
        /// <summary>
        /// 资源加载路径： 例如：Assets/UI/Prefab/UI_Main_Task.prefab
        /// </summary>
        public String MainName
        {
            get;
            set;
        }
        /// <summary>
        /// Bundle 的Bundle名字数组， [0] 为主包，其他为依赖包
        /// </summary>
        public String[] BundleNameArray
        {
            get;
            set;
        }      
    }

}
