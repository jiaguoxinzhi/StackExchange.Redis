using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore;


//using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WS_Core.Enums;

    /// <summary>
    /// 执行结果
    /// </summary>
    [Author("Linyee", "2018-06-29")]
    public class ExecuteResult: ExecuteResult<object>
    {

        /// <summary>
        /// 启用状态
        /// </summary>
        public bool Enabled { get; private set; } = true;

        /// <summary>
        /// 禁用
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public ExecuteResult Disable(string msg="已禁用")
        {
            Enabled = false;
            this.Msg = msg;
            return this;
        }

        ///// <summary>
        ///// 设置完成
        ///// </summary>
        ///// <param name="msg"></param>
        ///// <param name="data"></param>
        //public ExecuteResult SetOk(string msg, T data)
        //{
        //    this.IsOk = true;
        //    this.Msg = msg;
        //    this.Data = data;
        //    return this;
        //}

        ///// <summary>
        ///// 设置异常
        ///// </summary>
        ///// <param name="ex"></param>
        //public new ExecuteResult SetException(Exception ex)
        //{
        //    base.SetException(ex);
        //    return this;
        //}

        ///// <summary>
        ///// 从无类型转为强类型
        ///// 同类型则转换Data，不同则为Null
        ///// </summary>
        ///// <param name="res"></param>
        //public void From(ExecuteResult res)
        //{
        //    if (res == null) return;
        //    if(res.Data!=null && res.Data?.GetType()==typeof(T)) this.Data = (T)res.Data;
        //    this.InnerException = res.InnerException;
        //    this.IsOk = res.IsOk;
        //    this.Msg = res.Msg;
        //}


        ///// <summary>
        ///// 设置完成
        ///// </summary>
        ///// <param name="msg"></param>
        //public new ExecuteResult SetOk(string msg = "Ok")
        //{
        //    this.IsOk = true;
        //    this.Msg = msg;
        //    return this;
        //}

        ///// <summary>
        ///// 设置失败
        ///// </summary>
        ///// <param name="msg"></param>
        //public new ExecuteResult SetFail(string msg, int code = -200)
        //{
        //    this.IsOk = false;
        //    this.Msg = msg;
        //    return this;
        //}

        ///// <summary>
        ///// 设置失败
        ///// </summary>
        ///// <param name="msg"></param>
        //public new ExecuteResult SetFail(StatusCodeEnum code = StatusCodeEnum.FAIL)
        //{
        //    base.SetFail(code);
        //    return this;
        //}

    }


    /// <summary>
    /// 执行结果
    /// Linyee 2018-12-19
    /// </summary>
    public class ExecuteResult<T>
    {

    #region "构造"
    /// <summary>
    /// 创建 空 执行结果
    /// </summary>
    public ExecuteResult() {
        }

        /// <summary>
        /// 创建 执行结果
        /// </summary>
        /// <param name="isok"></param>
        public ExecuteResult(bool isok) {
            this.IsOk = isok;
        }

        /// <summary>
        /// 创建 正确 执行结果
        /// </summary>
        /// <param name="msg"></param>
        public ExecuteResult(string msg)
        {
            this.IsOk = true;
            this.Msg = msg;
        }

        /// <summary>
        /// 创建 异常 执行结果
        /// </summary>
        /// <param name="ex"></param>
        public ExecuteResult(Exception ex)
        {
            this.IsOk = false;
            this.Msg = ex.Message;
            this.InnerException = ex;
            LogService.Exception(ex);
        }

    ///// <summary>
    ///// 捕获运行错误
    ///// </summary>
    ///// <param name="p"></param>
    ///// <returns></returns>
    //public static ExecuteResult<T> TryRun(Func<T> p)
    //{
    //    ExecuteResult<T> result = new ExecuteResult<T>();
    //    try
    //    {
    //        var res = p.Invoke();
    //        return result.SetOk("Ok", res);
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        return result.SetException(ex);
    //    }
    //    //catch (DbEntityValidationException ex)
    //    //{
    //    //    return result.SetException(ex);
    //    //}
    //    catch (Exception ex)
    //    {
    //        return result.SetException(ex);
    //    }
    //}

    #endregion

    #region "方法"

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Author("Linyee","2019-03-13")]
    public ExecuteResult<T> SetData(T data)
    {
        this.Data = data;
        return this;
    }

    /// <summary>
    /// 设置
    /// </summary>
    /// <param name="res"></param>
    public ExecuteResult<T> Set(ExecuteResult<T> res)
    {
        this.Code = res.Code;
        this.Msg = res.Msg;
        this.Page = res.Page;
        this.Limit = res.Limit;
        this.Count = res.Count;
        this.Data = res.Data;
        this.InnerException = res.InnerException;

        return this;
    }

    /// <summary>
    /// 设置代码
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public ExecuteResult<T> SetCode(StatusCodeEnum code)
        {
            this.Code = code;
            return this;
        }

        /// <summary>
        /// 设置完成
        /// </summary>
        /// <param name="msg"></param>
        public ExecuteResult<T> SetOk(string msg="Ok")
        {
            this.IsOk = true;
            this.Msg = msg;
            return this;
        }

        /// <summary>
        /// 设置完成
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public ExecuteResult<T> SetOk(string msg,T data)
        {
            this.IsOk = true;
            this.Msg = msg;
            this.Data = data;
            return this;
        }

    /// <summary>
    /// 设置失败
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="code"></param>
    public ExecuteResult<T> SetFail(string msg,int code=-200)
        {
            this.IsOk = false;
            this.Msg = msg;
            this.InnerException = new Exception(msg);
            return this;
        }

    /// <summary>
    /// 设置失败
    /// </summary>
    /// <param name="code"></param>
    public ExecuteResult<T> SetFail(StatusCodeEnum code =  StatusCodeEnum.FAIL)
        {
            this.Code = code;
            this.IsOk = false;
            this.Desc = code.ToString().Replace("_", " ");
            this.Msg = ConstEnum.GetEnumDescription(code); 
            return this;
        }

        /// <summary>
        /// 设置异常
        /// </summary>
        /// <param name="ex"></param>
        public ExecuteResult<T> SetException(Exception ex)
        {
            LogService.Exception(ex);
            this.IsOk = false;
            this.Msg = ex.Message;
            this.InnerException = ex;
            return this;
        }

    /// <summary>
    /// 追加信息
    /// </summary>
    /// <param name="fmt"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public ExecuteResult<T> AppendMsg(string fmt,params string[] args)
    {
        this.Msg+=string.Format(" "+ fmt, args);
        return this;
    }

    ///// <summary>
    ///// 设置异常
    ///// </summary>
    ///// <param name="ex"></param>
    //public ExecuteResult<T> SetException(DbUpdateException ex)
    //{
    //    LogService.Exception(ex);
    //    this.IsOk = false;
    //    this.Msg = ex.Message;
    //    this.InnerException = ex;
    //    return this;
    //}

    ///// <summary>
    ///// 设置异常
    ///// </summary>
    ///// <param name="ex"></param>
    //public ExecuteResult<T> SetException(DbEntityValidationException ex)
    //{            
    //    this.IsOk = false;
    //    this.Msg = LogService.Exception(ex);
    //    this.InnerException = ex;
    //    return this;
    //}

    ///// <summary>
    ///// 设置异常
    ///// </summary>
    ///// <param name="ex"></param>
    //public ExecuteResult<T> SetException(EntitySqlException ex)
    //{            
    //    this.IsOk = false;
    //    this.Msg = LogService.Exception(ex);
    //    this.InnerException = ex;
    //    return this;
    //}

    ///// <summary>
    ///// 设置异常
    ///// </summary>
    ///// <param name="ex"></param>
    //public ExecuteResult<T> SetException(SqlException ex)
    //{
    //    this.IsOk = false;
    //    this.Msg = LogService.Exception(ex);
    //    this.InnerException = ex;
    //    return this;
    //}

    #endregion

    #region "类型转换"

    /// <summary>
    /// 转弱类型
    /// </summary>
    /// <param name="view"></param>
    public static implicit operator ExecuteResult(ExecuteResult<T> view)
        {
            var item= new ExecuteResult()
            {
                 
            };

            if (view == null) return item;
            if (item == null) return item;

            var viewType = view.GetType();
            foreach (var p in item.GetType().GetProperties())
            {
                var sp = viewType.GetProperty(p.Name);
                if (sp == null) continue;
                if (p.PropertyType != sp.PropertyType) continue;
                var val = sp.GetValue(view);
                p.SetValue(item, val);
            }

            return item;
        }

    /// <summary>
    /// 转主数据
    /// </summary>
    /// <param name="view"></param>
    public static implicit operator T(ExecuteResult<T> view)
    {
        return view.Data;
    }

    #endregion

    #region "属性"

    /// <summary>
    /// 子数据
    /// </summary>
    [JsonIgnore]
    public readonly List<ExecuteResult> Subs = new List<ExecuteResult>();

    /// <summary>
    /// 数据
    /// </summary>
    public T Data { get; set; }

        /// <summary>
        /// 消息 通常是中文简要描述
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 描述 通常是英文简要描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        public StatusCodeEnum Code { get; set; } = StatusCodeEnum.Accepted;

        /// <summary>
        /// 记录总数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 当前页数
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 当前页大小
        /// </summary>
        public int Limit { get; set; } = int.MaxValue;


        /// <summary>
        /// 是否正常
        /// </summary>
        public bool IsOk { get { return Code == StatusCodeEnum.OK; } set { if (value) Code = StatusCodeEnum.OK; else if(!value && Code== StatusCodeEnum.OK) Code = StatusCodeEnum.FAIL; } }

        /// <summary>
        /// 内联异常信息
        /// </summary>
        public Exception InnerException { get;protected set; }
    #endregion
}
