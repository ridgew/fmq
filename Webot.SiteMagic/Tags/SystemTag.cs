using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Webot.Common;
using System.Web;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 系统解析标签
    /// </summary>
    public class SystemTag : TagBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTag"/> class.
        /// </summary>
        public SystemTag()
            : base()
        { 
        
        }

        /// <summary>
        /// 系统标签
        /// </summary>
        /// <param name="tagdef">标签定义语法{#$ ... $#}</param>
        /// <remarks>
        /// {#$ $Hour$==12 ? "正午": "其他时间" $#}
        /// </remarks>
        public SystemTag(string tagdef)
            : base(tagdef)
        {
            string tagName = base.GetRootTagName();
            if (!tagName.StartsWith("$") && !tagdef.EndsWith("$#}"))
            {
                throw new InvalidProgramException("系统标签定义错误，正常格式：{#$ ... $#}。");
            }
        }

        /// <summary>
        /// 设置输出形式
        /// </summary>
        public override string ToString()
        {
            IResourceDependency res = this.GetResourceDependency();
            if (res == null && TagDefinition.IndexOf("(") == -1 && TagDefinition.IndexOf('[') == -1 && TagDefinition.IndexOf('$') == -1)
            {
                object tagValue = base.GetTagValue();
                return (tagValue == null) ? "" : tagValue.ToString();
            }
            else
            {
                #region 如果资源有直接定义
                if (res != null)
                {
                    string resDefineKey = this.TagDefinition;
                    //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " " + res.ToString());
                    //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " " + resDefineKey);
                    if (res.IsDefined(resDefineKey))
                    {
                        //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + resDefineKey + " " + res.ToString());
                        return res.GetDefinition(resDefineKey).ToString();
                    }
                } 
                #endregion

                #region 草稿设计
                //()表示函数 []表示数组
                //{#$["首页","新闻","动态","联系"][2]$#}		= "动态"
                //{#$Now()$#} {#$Now('yyyy-MM-dd HH:mm:ss')$#}	= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

                //函数表达式数据绑定
                //-----------------------------------------------
                //{$Left(源文本,字段长)}
                //->
                //源文本 = {#FieldName},{%Year}
                //字段长 = 12

                //{${%Hour}==12 ? "正午": "其他时间"}
                //{$Contains({#FieldName},"虚数传播") ? "包含" : "不包含"}

                //数据操作符号
                //> < == != ?: 
                #endregion

                //避免SVN/VSS关键字替换
                if (this.TagDefinition == "{#$" + "Date$#}")
                {
                    return DateTime.Now.Date.ToString("yyyy-M-d");
                }
                else if (this.TagDefinition == "{#$WK$#}")
                {
                    return DateTime.Now.DayOfWeek.GetHashCode().ToString();
                }
                else if (this.TagDefinition == "{#$Year$#}")
                {
                    return DateTime.Now.Year.ToString();
                }
                else if (this.TagDefinition == "{#$Month$#}")
                {
                    return DateTime.Now.Month.ToString();
                }
                else if (this.TagDefinition == "{#$Day$#}")
                {
                    return DateTime.Now.Day.ToString();
                }
                else if (this.TagDefinition == "{#$Hour$#}")
                {
                    return DateTime.Now.Hour.ToString();
                }
                else if (this.TagDefinition == "{#$Minute$#}")
                {
                    return DateTime.Now.Minute.ToString();
                }
                else if (this.TagDefinition == "{#$Second$#}")
                {
                    return DateTime.Now.Second.ToString();
                }
                else if (this.TagDefinition == "{#$MS$#}")
                {
                    return DateTime.Now.Millisecond.ToString();
                }
                else if (this.TagDefinition.StartsWith("{#$<") && this.TagDefinition.EndsWith(">$#}"))
                {
                    string urlMatchVal = this.TagDefinition.Substring(4, this.TagDefinition.Length - 8);
                    //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + urlMatchVal);
                    if (HttpContext.Current.Items["UrlMatchGroup"] == null)
                    {
                        //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + this.TagDefinition); 
                        return string.Empty;
                    }
                    else
                    {
                        GroupCollection grpCol = (GroupCollection)HttpContext.Current.Items["UrlMatchGroup"];
                        if (Util.IsNumerical(urlMatchVal))
                        {
                            return grpCol[Convert.ToInt32(urlMatchVal)].Value;
                        }
                        else
                        {
                            return grpCol[urlMatchVal].Value;
                        }
                    }
                }
                else
                {
                    string objResult = this.TagDefinition;
                    try
                    {
                        //if (GetResourceDependency() != null)
                        //{
                        //    OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + GetResourceDependency().ToString());
                        //}
                        using (TagParse tgp = new TagParse(objResult, GetResourceDependency()))
                        {
                            objResult = tgp.GetValue();
                        }
                    }
                    catch (Exception exp)
                    {
                        //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + this.TagDefinition + "\n" + res);

                        throw new InvalidOperationException(string.Format("系统标签解析错误，标签定义：{0}\n提示：{1} \n源跟踪：{2}",
                            objResult,
                            exp.Message,
                            exp.StackTrace));
                    }
                    return objResult;
                }
            }
        }

        #region 系统实用函数封装\转义
        /// <summary>
        /// 系统支持函数名
        /// </summary>
        internal const string SystemFunctionNames = ",Contains,EmptyReplace,Format,GetContextItem,Html2Text,Length,Now,Repeat,Replace,ReplaceX,RequestHasValue,RequestValid,Size,TrimHTML,";

        /// <summary>
        /// 是否系统支持函数
        /// </summary>
        private static bool IsInSystemFunctions(string funName)
        {
            return (SystemFunctionNames.IndexOf(string.Concat(",", funName, ",")) != -1);
        }

        #region 字符处理
        /// <summary>
        /// 空内容替换
        /// </summary>
        /// <param name="strObject">判断为空的内容</param>
        /// <param name="RepStr">替换为的内容</param>
        /// <returns>如果不为空字符则为其本身</returns>
        public static string EmptyReplace(string strObject, string RepStr)
        {
            if (string.IsNullOrEmpty(strObject))
            {
                return RepStr;
            }
            else
            {
                return (strObject.Trim().Length > 0) ? strObject : RepStr;
            }
        }

        /// <summary>
        /// 过滤HTML标签  {#$TrimHTML(%Title%)$#}
        /// </summary>
        public static string TrimHTML(string htmlTxt)
        {
            return Regex.Replace(htmlTxt, "<.[^<]*>", "", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// HTML格式文本转换为普通文本
        /// </summary>
        /// <param name="htmlObj">转换HTML文本对象</param>
        /// <returns>格式化的普通文本</returns>
        public static string Html2Text(object htmlObj)
        {
            return WebUI.Html2Text(htmlObj);
        }

        /// <summary>
        /// 按相关模型重复多次
        /// </summary>
        /// <param name="num">重复次数</param>
        /// <param name="pattern">复制相关模型</param>
        public static string Repeat(int num, string pattern)
        {
            return WebUI.ReplicateObject(num, pattern);
        }

        /// <summary>
        /// 字符替换
        /// </summary>
        public static string Replace(string source, string find, string rep)
        {
            if (source != null)
            {
                return source.Replace(find, rep);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 正则表达式替换
        /// </summary>
        public static string ReplaceX(string source, string pattern, string rep)
        {
            return Util.RegexReplace(source, pattern, rep);
        }

        /// <summary>
        /// 请求键值数据是否有值
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns></returns>
        public bool RequestHasValue(string key)
        {
            return !string.IsNullOrEmpty(HttpContext.Current.Request[key]);
        }

        /// <summary>
        /// 请求数据是否有效，无效则返回null。
        /// </summary>
        /// <param name="keys">所有需要验证的发送键值，多个键值用逗号分隔</param>
        /// <param name="patterns">验证的匹配模式，与键值一一对应</param>
        /// <param name="computeExp">目标计算表达式</param>
        /// <returns>如果是有效数据则返回计算表达式，否则为null。</returns>
        public static string RequestValid(string keys, string patterns, string computeExp)
        {
            string[] tRKeys = keys.Split(',');
            string[] tPatterns = patterns.Split(',');
            for (int i = 0, j = tRKeys.Length; i < j; i++)
            {
                if (HttpContext.Current.Request[tRKeys[i]] == null ||
                    !Regex.IsMatch(HttpContext.Current.Request[tRKeys[i]],
                    tPatterns[i], RegexOptions.IgnoreCase))
                {
                    return null;
                }
            }
            return computeExp;
        }

        /// <summary>
        /// 字符格式化输出
        /// </summary>
        /// <param name="fmt">格式化模型</param>
        /// <param name="objDat">模型填充对象</param>
        public static string Format(string fmt, params object[] objDat)
        {
            return String.Format(fmt, objDat);
        }

        /// <summary>
        /// 获取当前时间的默认格式化形式 2008-01-29 14:25:18
        /// </summary>
        /// <returns></returns>
        public static string Now()
        {
            return Now("");
        }

        /// <summary>
        /// 获取当前时间的格式化形式 {#$Now()$#}
        /// </summary>
        /// <param name="toStrFormat">标准的DateTime格式化字符形式</param>
        public static string Now(string toStrFormat)
        {
            string strRet = "";
            try
            {
                if (string.IsNullOrEmpty(toStrFormat)) toStrFormat = "yyyy-MM-dd HH:mm:ss";
                strRet = DateTime.Now.ToString(toStrFormat);
            }
            catch (Exception)
            {
                strRet = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return strRet;
        }

        /// <summary>
        /// 目标字符中是否包含指定的字符串
        /// </summary>
        public static bool Contains(string source, string find)
        {
            return (source == null) ? false : (source.IndexOf(find) != -1); 
        }

        /// <summary>
        /// 获取应用请求上下中中的变量信息
        /// </summary>
        /// <param name="itemKey">变量的键名</param>
        /// <returns>默认为字符串形式</returns>
        public static string GetContextItem(string itemKey)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items[itemKey] != null)
            {
                return HttpContext.Current.Items[itemKey].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion

        /// <summary>
        /// 获取相关对象的长度
        /// </summary>
        public static int Length(object objT)
        {
            if (objT == null) return 0;
            return objT.ToString().Length;
        }

        /// <summary>
        /// 获取相关文件长度的字符表示形式 从Byte到M
        /// </summary>
        public static string Size(object objT)
        {
            return WebUI.GetSize(objT);
        }

        #endregion
    }
}
