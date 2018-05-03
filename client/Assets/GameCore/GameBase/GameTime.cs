using UnityEngine;
using System.Collections;
using System;

public class GameTime
{
    public static float frame_time = Time.fixedDeltaTime;
    public static int FPS = (int)(1 / frame_time);
    #region 客户端计数，全局时间
    private static int GlobalTickCount = 0;

    public static int GlobalTick
    {
        get { return GlobalTickCount; }
        set { GlobalTickCount = value; }
    }

    public static void UpdateGlobalTick ( )
    {
        if (GlobalTickCount >= int.MaxValue)
        {
            GlobalTickCount = int.MinValue;
        }
        GlobalTickCount++;
    }
    #endregion


    //************************************
    // Method:    TickToRealTime
    // FullName:  TickToRealTime
    // Access:    public 
    // Returns:   float
    // Qualifier:
    // Parameter: int tickCount
    // Tick转化为真实时间
    //************************************
    public static float TickToRealTime ( int tickCount )
    {
        return tickCount * frame_time;
    }

    //************************************
    // Method:    RealTimeToTick
    // FullName:  RealTimeToTick
    // Access:    public 
    // Returns:   int
    // Qualifier:
    // Parameter: float time
    // 真实时间转成Tick
    //************************************
    public static int RealTimeToTick ( float time )
    {
        return (int)(time * FPS);
    }

    //************************************
    //
    //服务器时间换算部分
    //
    //************************************
    static double m_interval = 0d;

    static DateTime Genesis = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
    static int secondsOfDay = 86400;
    /// <summary>
    /// 从DateTime转成timestamp
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static double ToTimestamp ( DateTime dt )
    {
        TimeSpan span = (dt - Genesis.ToLocalTime());
        return (double)span.TotalSeconds;
    }

    /// <summary>
    /// 从timestamp转成DateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime CoverTimestamp ( double timestamp )
    {
        DateTime dt = Genesis.AddSeconds(timestamp);
        return dt.ToLocalTime();
    }

    public static void Init ( double reqTimeStamp, double serverTimeStamp )
    {
        double now = ToTimestamp( DateTime.Now );
        double ping = now - reqTimeStamp;
        ping = Math.Min(ping, 1);
        m_interval = now - serverTimeStamp + ping;
    }

	// 该时间后的第几天
	public static int DayPass( double startTimeStamp)
	{
		DateTime startTime = CoverTimestamp(startTimeStamp);
        DateTime now = ServerTime();
        if (now.Year != startTime.Year || now.Month != startTime.Month)
            return 8;
        return now.Day - startTime.Day + 1;
	}

    /// <summary>
    /// 是否时间已经到了
    /// 参数是服务器给的时间
    /// </summary>
    /// <param name="timestamp">服务器时间</param>
    /// <returns></returns>
    public static bool IsTimePass ( double timestamp )
    {
        double localNow = ToTimestamp( DateTime.Now );
        return localNow - m_interval > timestamp;
    }

    public  static bool IsTimePassStr(String timeStr)
    {
        double timestamp = ToTimestamp(Convert.ToDateTime(timeStr));
        double localNow = ToTimestamp(DateTime.Now);
        return localNow - m_interval > timestamp;
    }

    /// <summary>
    /// 时间过去多久了
    /// 如果是负值，则说明时间未到，返回值为还剩余多少时间到期
    /// </summary>
    /// <param name="timeStamp">服务器时间</param>
    /// <returns></returns>
    public static double TimePass ( double timeStamp )
    {
        double localNow = ToTimestamp( DateTime.Now );
        return localNow - m_interval - timeStamp;
    }

    /// <summary>
    /// 用服务器的时间计算剩余时间
    /// 参数也是服务器的时间
    /// NOTE：返回结果和函数TimePass刚好相反
    /// </summary>
    /// <param name="time">截止时间</param>
    /// <returns></returns>
    public static double TimeRemain ( double timestamp )
    {
        double localNow = ToTimestamp( DateTime.Now );
        return timestamp - (localNow - m_interval);
    }

    public static double TimeRemainRound(double timestamp)
    {
        double localNow = ToTimestamp(DateTime.Now);
        return timestamp - (localNow - m_interval) + 0.5f;
    }

    /// <summary>
    ///  获取当前服务器的时间
    /// </summary>
    /// <returns>服务器NOW的时间戳</returns>
    public static double GetServerTime ( )
    {
        return ToTimestamp( DateTime.Now ) - m_interval;
    }

    public static DateTime ServerTime()
    {
        return CoverTimestamp(GetServerTime());
    }

    /// <summary>
    /// 服务器时间转换为秒数(以最近的周日为起点)
    /// </summary>
    /// <returns></returns>
    public static int ServerTimeToSeconds()
    {
        DateTime time = ServerTime();
        int passTime = (int)time.DayOfWeek * 3600 * 24 + time.Hour * 3600 + time.Minute * 60 + time.Second;
        return passTime;
    }

    /// <summary>
    /// 计算给定的时间戳是否在当前天（与给定的刷新时刻作为一天的开始）
    /// </summary>
    /// <param name="timestamp">服务器时间戳</param>
    /// <param name="cycleHour">每天刷新时刻的小时</param>
    /// <param name="cycleMinute">每天刷新时刻的分钟</param>
    /// <param name="cycleSecond">每天刷新时刻的秒</param>
    /// <returns></returns>
    public static bool InToday ( double timestamp, int cycleHour = 0, int cycleMinute = 0, int cycleSecond = 0 )
    {
        DateTime serverNow = DateTime.Now.AddSeconds(m_interval);
        DateTime today = new DateTime( serverNow.Year, serverNow.Month, serverNow.Day, cycleHour, cycleMinute, cycleSecond, 0 );
        double todayTS = ToTimestamp( today );

        //小于今天刷新时刻的起始时间，则肯定不是在同一天
        //大于今天刷新时刻的起始时间，则计算是否和现在超过24小时
        if (todayTS < timestamp)
        {
            return timestamp - todayTS < secondsOfDay;
        }
        return false;
    }
}
