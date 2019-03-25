using System;
using System.Collections.Generic;
//using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// 日志类
/// Linyee 2018-05-30
/// </summary>
public static class LogService
{
    #region "写入方法"
    private static object ErrorLogLock = new object(), ErrorListLock = new object();
    /// <summary>
    /// 错误日志
    /// </summary>
    /// <param name="ex"></param>
    public static void Error(Exception ex)
    {
        var LogLine = DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + ex.ToString();
        WriteLog("Error.txt", LogLine, ErrorLogLock);
    }
    private static object ExceptionLock = new object(), ExceptionListLock = new object();

    /// <summary>
    /// 异常日志
    /// </summary>
    /// <param name="ex"></param>
    public static void Exception(string ex)
    {
        var LogLine = DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + ex;
        WriteLog("Exception.txt", LogLine, ExceptionLock);
    }


    ///// <summary>
    ///// 异常日志
    ///// </summary>
    ///// <param name="ex"></param>
    ///// <param name="exname"></param>
    //public static string Exception(SqlException ex, string exname = "异常")
    //{
    //    LogService.AnyLog("Runtime", exname + "：" + ex.Message + "（详见Exception记录）");
    //    string errstr = string.Join("\t",
    //        ex.Errors.Cast<SqlError>().Select(e => string.Join(",",
    //        e.Message))
    //        );
    //    Exception(errstr);
    //    Exception(ex.ToString());
    //    return errstr;
    //}


    /// <summary>
    /// 异常日志
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="exname"></param>
    public static string Exception(Exception ex,string exname= "异常")
    {
        string errstr = ex.Message;
        LogService.AnyLog("Runtime", exname +"：" + errstr + "（详见Exception记录）");
        Exception(ex.ToString());
        return errstr;
    }

    private static object RuntimeLogLock = new object(), RuntimeListLock = new object();


    static Dictionary<long, Queue<string>> LogQueDict = new Dictionary<long, Queue<string>>();
    static Dictionary<long, string> LogTypeDict = new Dictionary<long, string>();
    static object tmplogdictlock = new object();
    /// <summary>
    /// 运行日志
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="message"></param>
    public static void Runtime(long tid, params string[] message)
    {
        string msg = DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + string.Join("\t", message);
        lock (tmplogdictlock)
        {
            if (LogQueDict.ContainsKey(tid))
            {
                LogQueDict[tid].Enqueue(msg);
            }
            else
            {
                var que = new Queue<string>();
                que.Enqueue(msg);
                LogQueDict.Add(tid, que);
            }
        }
    }

    /// <summary>
    /// 运行日志
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="message"></param>
    /// <param name="isend"></param>
    public static void Runtime(long tid, string message,bool isend)
    {
        Runtime(tid, message);
        if (isend)
        {
            lock (tmplogdictlock)
            {
                if (LogQueDict.ContainsKey(tid))
                {
                    var que = LogQueDict[tid];
                    var msg = string.Join("\r\n", que);
                    LogQueDict.Remove(tid);
                    var logfile = "Runtime.txt";
                    //判断文件名
                    if (LogTypeDict.ContainsKey(tid)) {
                        logfile = LogTypeDict[tid];
                        LogTypeDict.Remove(tid);
                    }
                    WriteLog(logfile, msg, RuntimeLogLock);
                }
            }
        }
    }

    /// <summary>
    /// 运行日志
    /// </summary>
    /// <param name="message"></param>
    public static string Runtime(params string[] message)
    {
        string msg = string.Join("\t", message);
        var LogLine =DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + msg;
        WriteLog("Runtime.txt", LogLine, RuntimeLogLock);
        return LogLine;
    }


    private static object SignRuntimeLogLock = new object(), SignRuntimeListLock = new object();
    /// <summary>
    /// 签名运行时
    /// </summary>
    /// <param name="messages"></param>
    public static void SignRuntime(params string[] messages)
    {
        string msg = string.Join("\t", messages);
        var LogLine = DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + msg;
        WriteLog("SignRuntime.txt", LogLine, SignRuntimeLogLock);
    }

    /// <summary>
    /// 请求运行时
    /// </summary>
    /// <param name="messages"></param>
    public static void Request(params string[] messages)
    {
        string msg = string.Join("\t", messages);
        var LogLine = DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + msg;
        WriteLog("Request.txt", LogLine, SignRuntimeLogLock);
    }

    /// <summary>
    /// 任意类别日志
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="logtype"></param>
    /// <param name="logstr"></param>
    public static void AnyLog(long tid, string logtype, params string[] logstr)
    {
        Runtime(tid, logstr);
        if (!LogTypeDict.ContainsKey(tid))
        {
            lock (tmplogdictlock)
            {
                LogTypeDict.Add(tid, logtype+".txt");
            }
        }
    }

    /// <summary>
    /// socket10分钟日志
    /// </summary>
    /// <param name="logstr"></param>
    public static void Socket10Minute(params string[] logstr)
    {
        var LogLine = DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + string.Join("\t", logstr);
        WriteLog("Socket"+ DateTime.Now.ToString("HH")+(DateTime.Now.Minute/10).ToString("D2")+ ".txt", LogLine);
    }

    /// <summary>
    /// socket10分钟日志
    /// </summary>
    /// <param name="logstr"></param>
    public static void WebSocket10Minute(params string[] logstr)
    {
        var LogLine = DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + string.Join("\t", logstr);
        WriteLog("WebSocket"+ DateTime.Now.ToString("HH")+(DateTime.Now.Minute/10).ToString("D2")+ ".txt", LogLine);
    }

    /// <summary>
    /// socket10分钟日志
    /// </summary>
    /// <param name="logstr"></param>
    public static void WebSocketClient10Minute(params string[] logstr)
    {
        var LogLine = DateTime.Now.ToString("HH:mm:ss.fffffff") + "\t" + string.Join("\t", logstr);
        WriteLog("WebSocketClient" + DateTime.Now.ToString("HH") + (DateTime.Now.Minute / 10).ToString("D2") + ".txt", LogLine);
    }


    /// <summary>
    /// 任意类别日志
    /// </summary>
    /// <param name="logtype"></param>
    /// <param name="logstr"></param>
    public static void AnyLog(string logtype,params string[] logstr)
    {
        WriteLog(logtype+".txt",DateTime.Now.ToString("HH:mm:ss.fffffff")+"\t"+ string.Join("\t", logstr)+"\r\n");
    }
    #endregion

    #region "写入主体"
    private static object RequestFileLock = new object();
    private static object QueueLock = new object();

    /// <summary>
    /// 多线程日志
    /// </summary>
    /// <param name="state"></param>
    public static void WriteLog(object state)
    {
        var log = state?.ToString();
        if (!string.IsNullOrWhiteSpace(log))
        {
            WriteLog("Log.txt",log);
        }
    }

    /// <summary>
    /// 多线程日志
    /// </summary>
    /// <param name="state"></param>
    public static void WriteLogRequest(object state)
    {
        var log = state?.ToString();
        if (!string.IsNullOrWhiteSpace(log))
        {
            WriteLog("Request.txt", log);
        }
    }

    /// <summary>
    /// 写入日志
    /// </summary>
    /// <param name="logtype"></param>
    /// <param name="logs"></param>
    /// <param name="lockobj"></param>
    private static void WriteLog(string logtype, List<string> logs,object lockobj)
    {
        var logstr = string.Join("\r\n", logs) + "\r\n";
        logs.Clear();
        WriteLog(logtype + ".txt", logstr, lockobj);
    }

    private static object TimerListLock = new object();
    private static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
    private static List<timerInfo> TimerList = new List<timerInfo>();
    private static timerInfo LastTimer = null;
    private class timerInfo:IDisposable
    {
        public timerInfo(DateTime now, Timer timer)
        {
            this.addTime = now;
            this.timer = timer;
        }

        public DateTime addTime { get; set; }
        public Timer timer { get; set; }

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
        }
    }
    /// <summary>
    /// 写入日志
    /// </summary>
    /// <param name="logfile"></param>
    /// <param name="log"></param>
    /// <param name="lockobj"></param>
    private static void WriteLog(string logfile, string log,object lockobj=null)
    {
        var LogPath= AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\Log\"+DateTime.Now.ToString("yyyyMMdd")+@"\";
        if (lockobj == null) lockobj = RequestFileLock;
        if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);

        try
        {
            lock (QueueLock)
            {
                lock (lockobj)
                {
                    //压入缓存的日志
                    GetValue(logs, logfile).Enqueue(log);
                }
            }
            var dt = DateTime.Now;
            if (LastTimer != null) LastTimer.Dispose();
                LastTimer = new timerInfo(dt, new Timer(new TimerCallback(WriteLogTimer), dt, 1000, -1));
        }
        catch (Exception ex)
        {
            var file = LogPath + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.txt";
            lock (ExceptionLock)
            {
                File.AppendAllText(file, ex.ToString());
            }
        }
    }

    /// <summary>
    /// 定时写入日志
    /// </summary>
    /// <param name="state"></param>
    private static void WriteLogTimer(object state)
    {
        var LogPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\Log\" + DateTime.Now.ToString("yyyyMMdd") + @"\";
        if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);

        var dt = (DateTime)state;
        try
        {
            lock (TimerListLock)
            {
                var rlist = TimerList.Where(p => p.addTime <= dt).ToList();
                foreach (var item in rlist)
                {
                    TimerList.Remove(item);
                }
            }
        }
        catch (Exception ex)
        {
            var a = ex;
        }

        try
        {
            LogWriteLock.EnterWriteLock();
            string[] keys=new string[0];
            lock (QueueLock)
            {
                keys = logs.Keys.ToArray();
            }
            foreach (var logfile in keys)
            {
                var qu = GetValue(logs, logfile);
                if (qu.Count < 1) continue;
                string log = "";
                lock (QueueLock)
                {
                    log = string.Join("\r\n", qu.ToList());
                    qu.Clear();
                    File.AppendAllText(LogPath+ DateTime.Now.ToString("yyyyMMdd_HH")+logfile, log + "\r\n");
                }
            }
        }
        catch (Exception ex)
        {
            var file = LogPath + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.txt";
            lock (ExceptionLock)
            {
                File.AppendAllText(file, ex.ToString() + "\r\n");
            }
        }
        finally
        {
            //退出写入模式，释放资源占用
            LogWriteLock.ExitWriteLock();
        }
    }

    private static Dictionary<string, Queue<string>> logs = new Dictionary<string, Queue<string>>();
    /// <summary>
    /// 获取指定日志队列
    /// </summary>
    /// <param name="logs"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static Queue<string> GetValue(this Dictionary<string, Queue<string>> logs,string key)
    {
        var kkey = key.ToLower();
        if (!logs.ContainsKey(kkey))
        {
            logs.Add(kkey,new Queue<string>(100000));
        }

        return logs[kkey];
    }
    #endregion
}
