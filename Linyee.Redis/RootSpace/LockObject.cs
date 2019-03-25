using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


/// <summary>
/// 锁定对象
/// 注意必须是同步代码块，如果有异步代码，会导致无法正常释放
/// </summary>
[Author("Linyee", "2019-01-16")]
public class LockObject : LockObject<object>
{
    /// <summary>
    /// 创建进程锁 指定超时秒数
    /// </summary>
    /// <param name="waitsec"></param>
    public LockObject(int waitsec = 10) : base(waitsec) { }

}

/// <summary>
/// 锁定对象
/// 注意必须是同步代码块，如果有异步代码，会导致无法正常释放
/// </summary>
[Author("Linyee", "2018-07-05")]
[Modifier("Linyee", "2019-01-16", "改为泛型，独立文件，改为根类")]
public class LockObject<TEntity> : IDisposable
{

    #region "获取指定类别锁定对象"
    private static Dictionary<string, object> dict = new Dictionary<string, object>();

    /// <summary>
    /// 获取指定类别锁定对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [Author("Linyee", "2019-01-16")]
    public static object GetLock<T>()
    {
        var itemType = typeof(T);
        return GetLock(itemType);
    }

    /// <summary>
    /// 获取指定类别锁定对象
    /// </summary>
    /// <returns></returns>
    [Author("Linyee", "2019-01-16")]
    public static object GetLock(Type type)
    {
        var itemType = type;
        var typename = itemType.ToString().ToLower();

        lock (dict)
        {
            if (dict.ContainsKey(typename)) return dict[typename];
            else
            {
                var obj = new object();
                dict.Add(typename, obj);
                return obj;
            }
        }
    }

    /// <summary>
    /// 整个数据库操作锁定
    /// </summary>
    public static object PayShoubeiLock = new object();

    /// <summary>
    /// 
    /// </summary>
    static LockObject()
    {
    }
    #endregion

    #region "多进程锁"
    /// <summary>
    /// 进程锁
    /// </summary>
    private Mutex mutex=null;
    private string LockMapName;

    /// <summary>
    /// 创建进程锁
    /// 同进程时 多线程处理结束 才会跳到另一个进程？
    /// </summary>
    [Author("Linyee", "2018-07-05")]
    [Modifier("Linyee", "2019-01-16", "改为泛型")]
    public LockObject(int waitsec=10)
    {
        var b = false;
        bool isExisted = false;
        var dt = DateTime.Now;
        var dt1 = DateTime.Now;
        string TEntityName = typeof(TEntity).ToString();//区分大小写//.ToLower();
        LockMapName = "Percode_Shoubei_Lock_" + TEntityName;

        while (!b)
        {
            if (!b) {
                mutex = new Mutex(false, LockMapName, out b);//多站点，，会导致拒绝访问
            }
            if (!b)
            {
                mutex = Mutex.OpenExisting(LockMapName);
                b = mutex != null;
                isExisted = b;
                LogService.AnyLog("Lock", LockMapName, "获取锁", "获取已存在");
            }
            else
            {
                isExisted = false;
                LogService.AnyLog("Lock", LockMapName, "获取锁", "创建新锁");
            }
            if (!b)
            {
                dt1 = DateTime.Now;
                if ((dt1 - dt).TotalSeconds >= waitsec)
                {
                    throw new Exception("多线程锁 排队超时");
                }
                Thread.Sleep(300);
            }
        }

        //已存在
        //if (isExisted)
        {
            //多线程模式
            b = mutex.WaitOne(waitsec * 1000, false);//WaitOne ReleaseMutex 不对等错误 由于出现被放弃的 mutex,等待过程结束
            if (!b)
            {
                LogService.AnyLog("Lock", LockMapName, "等待锁", "超时"+ waitsec+"秒");
                throw new Exception("多线程锁 等待超时");
            }
        }
    }

    /// <summary>
    /// 释放锁
    /// </summary>
    [Author("Linyee", "2018-07-05")]
    public void Dispose()
    {
        //只能在同线程同步代码中进行释放
        mutex.ReleaseMutex();//代码块非同线程同步代码，含有awit Async 从不同步的代码块中调用了对象同步方法。

        LogService.AnyLog("Lock", LockMapName, "释放锁");

        ////休停100ms 加了也一样
        //AutoResetEvent resetEvent = new AutoResetEvent(false);
        //var timer = new System.Threading.Timer((obj =>
        //{
        //    resetEvent.Set();
        //}), null, 100, -1);
        //Thread.Sleep(100);
        //resetEvent.WaitOne();

        //mutex.Close();
        //mutex.Dispose();
        //mutex = null;
        //GC.Collect();
    }
    #endregion
}
