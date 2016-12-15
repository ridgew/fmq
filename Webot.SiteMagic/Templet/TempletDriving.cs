using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Webot.Common;
using System.Text.RegularExpressions;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 模板驱动动态内容显示资源
    /// </summary>
    public class TempletDriving : IResourceDependency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempletDriving"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public TempletDriving(HttpContext context)
        {
            CtxInfo = context;
        }

        private HttpContext CtxInfo = null;

        //$PagerFormat$ -> 分页格式化字符串
        //$TempletID$   -> 模板编号
        //$CurrentPage$  -> 当前页
        private string knownKeys = ",$PagerFormat$,$TempletID$,$CurrentPage$,";

        /// <summary>
        /// URL地址中的参数匹配
        /// </summary>
        private string urlPatternMatch = "\\{#(\\s*)\\$(\\d+)\\$(\\s*)#\\}";

        #region IResourceDependency Members

        /// <summary>
        /// 获取特定对象的定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>返回定义的对象</returns>
        public object GetDefinition(string x)
        {
            //Response.Write(context.Items["VirtualURL"]);
            //^/t(\d+)/(\w+)(\-(\d+))?(\.as(p|h)x)
            string virtualURL = CtxInfo.Items["VirtualURL"].ToString();
            string[] urlMatch = null;
            string urlPattern = "^/t(\\d+)/(\\w+)(\\-(\\d+))?(\\.as(p|h)x)";
            if (Util.GetSingleMatchValue(urlPattern, virtualURL, out urlMatch))
            {
                string mIndx = "0";
                //{#$1$#}
                if (Util.IsMatch(urlPatternMatch, x, 2, 1, urlMatch.Length, out mIndx))
                {
                    return urlMatch[Convert.ToInt32(mIndx)];
                }

                string key = x.Trim('{', '#', '}', '$');
                if (string.Compare(key, "PagerFormat", true) == 0)
                {
                    return Regex.Replace(virtualURL, urlPattern, "/t$1/$2-{0}$5", RegexOptions.IgnoreCase);
                }
                else if (string.Compare(key, "TempletID", true) == 0)
                {
                    return urlMatch[1];
                }
                else if (string.Compare(key, "CurrentPage", true) == 0)
                {
                    if (urlMatch[4] == "") return 1;
                    return urlMatch[4];
                }
                else
                {
                    return CtxInfo.Request[key];
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 返回特定对象是否有定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>是否定义过该对象</returns>
        public bool IsDefined(string x)
        {
            if (CtxInfo.Items["VirtualURL"] == null) return false;

            if (knownKeys.IndexOf("," + x + ",") != -1)
            {
                return true;
            }
            else if (Regex.IsMatch(x, urlPatternMatch, RegexOptions.IgnoreCase))
            {
                return true;
            }
            else
            {
                return CtxInfo.Request[x.Trim('{', '#', '}', '$')] != null;
            }
        }

        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        public string DependencyIdentity
        {
            get { return "TempletDependency"; }
        }

        #endregion
    }
}
