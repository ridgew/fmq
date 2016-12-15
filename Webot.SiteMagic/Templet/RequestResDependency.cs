using System;
using System.Web;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 应用上下文请求依赖
    /// </summary>
    public class RequestResDependency : IResourceDependency
    {
        private static RequestResDependency _requestInstance = null;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static RequestResDependency Instance
        {
            get
            {
                if (_requestInstance == null)
                {
                    _requestInstance = new RequestResDependency();
                }
                return _requestInstance;
            }
        }

        #region IResourceDependency Members

        /// <summary>
        /// 获取特定对象的定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>返回定义的对象</returns>
        public object GetDefinition(string x)
        {
            if (x.StartsWith("{#$") && x.EndsWith("$#}"))
            {
                x = x.Substring(3, x.Length - 6);
            }
            return HttpContext.Current.Request[x];
        }

        /// <summary>
        /// 返回特定对象是否有定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>是否定义过该对象</returns>
        public bool IsDefined(string x)
        {
            //Webot.Common.OleDbHelper.AppendToFile("~/debug.log",
            //    System.Environment.NewLine + string.Concat("x：", x));
            if (x.StartsWith("{#$") && x.EndsWith("$#}"))
            {
                x = x.Substring(3, x.Length - 6);
            }
            return (HttpContext.Current.Request[x] != null);
        }

        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        public string DependencyIdentity
        {
            get { return "HttpRequest"; }
        }

        #endregion
    }
}
