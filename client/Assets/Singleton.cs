

/************************************************************************/
/* 单件抽象类                                                            */
/************************************************************************/
public abstract class Singleton<T> where T : class, new()
{
    private static T mInstance = new T();

    public static T Instance
    {
        get
        {
            return mInstance;
        }
    }
}

