
namespace com.cyou.plugin.resource
{
    public enum ResourcePriority : int
    {   
        ResourcePriority_Min = -2,
        ResourcePriority_Async = -1,
        ResourcePriority_Def = 0,  //默认载入级别  
        ResourcePriority_Self = 1,  //自己的加载
        ResourcePriority_SynText = 2,  //文本加载
        ResourcePriority_Max = 3,
    }
}
