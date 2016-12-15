using System;
using System.Web;
using System.Data;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Net;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.IO;

namespace Webot.Common
{

    /// <summary>
    /// 相应功能函数
    /// Author: Ridge Wong
    /// Revision: 1.0.3 2006-3-30
    /// 1.0.4 (Ridge Wong: Checked at 2006-5-17)
    /// 1.1.0 (Ridge Wong: 2006-8-9)
    /// 2.0.1 (Ridge Wong: 2008-1-29 增加反射调用与绑定)
    /// </summary>
    public sealed class Util
    {
        /// <summary>
        /// 私有类,无须实例化
        /// </summary>
        private Util()
        {

        }

        #region 版本控制相关
        /// <summary>
        /// 判断版本是否高于比较的相关版本
        /// </summary>
        /// <param name="sVer">比较对象版本</param>
        /// <param name="tVer">被比较对象版本</param>
        /// <returns>如果高于该版本则返回真,否则返回假.</returns>
        public static bool VersionIsHigherThan(IVersion sVer, IVersion tVer)
        {
            int x = sVer.CurrentVersion.Major;
            int y = tVer.CurrentVersion.Major;
            if (x != y)
            {
                return (x > y) ? true : false;
            }
            else
            {
                x = sVer.CurrentVersion.Minor;
                y = tVer.CurrentVersion.Minor;
                if (x != y)
                {
                    return (x > y) ? true : false;
                }
                else
                {
                    x = sVer.CurrentVersion.Build;
                    y = tVer.CurrentVersion.Build;
                    if (x != y)
                    {
                        return (x > y) ? true : false;
                    }
                    else
                    {
                        x = sVer.CurrentVersion.Revision;
                        y = tVer.CurrentVersion.Revision;
                        return (x > y) ? true : false;
                    }
                }
            }
        }
        #endregion

        #region 服务器判断

        /// <summary>
        /// 本服务器提交
        /// </summary>
        /// <returns>True|False</returns>
        public static bool IsSelfDomainSubmit()
        {
            string sHttp_Referer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"];
            string sServer_Name = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            string ChkStr = string.Empty;
            if (sHttp_Referer != null)
            {
                ChkStr = sHttp_Referer.Substring(7, sServer_Name.Length);
            }
            if (ChkStr != sServer_Name)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断输入字符串是否符合正则表达式的模式
        /// </summary>
        /// <param name="strPattern">匹配模式</param>
        /// <param name="strInput">输入字符串</param>
        /// <returns>是否匹配</returns>
        public static bool IsMatch(string strPattern, string strInput)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(strInput, strPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 判断输入字符串是否符合正则表达式的模式
        /// </summary>
        /// <param name="strPattern">匹配模式</param>
        /// <param name="strInput">输入字符串</param>
        /// <param name="idxGroup">组索引，起始为1。</param>
        /// <param name="iMin">最小值</param>
        /// <param name="iMax">最大值</param>
        /// <param name="strMatchValue">匹配输出值</param>
        /// <returns>是否匹配</returns>
        public static bool IsMatch(string strPattern, string strInput, int idxGroup, int iMin, int iMax, out string strMatchValue)
        {
            bool blnReturn = false;
            strMatchValue = string.Empty;
            Match m = Regex.Match(strInput, strPattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                try
                {
                    strMatchValue = m.Groups[idxGroup].Value;
                    int iValue = Convert.ToInt32(strMatchValue);
                    blnReturn = (iValue <= iMax && iValue >= iMin);
                }
                catch (Exception)
                {
                    blnReturn = false;
                }
            }

            return blnReturn;
        }

        /// <summary>
        /// 判断输入对象是否为数字类型
        /// </summary>
        /// <param name="strInput">输入对象</param>
        /// <returns>是否为不含小数点的数字类型</returns>
        public static bool IsNumerical(object strInput)
        {
            if (strInput == null) return false;
            bool bValue = true;
            string strCheck = strInput.ToString();
            if (strCheck.Length == 0) return false;
            for (int i = 0; i < strCheck.Length; i++)
            {
                if (!char.IsDigit(strCheck, i))
                {
                    bValue = false;
                    break;
                }
            }
            return bValue;
        }

        /// <summary>
        /// 判断输入是否没有内容或为空内容
        /// Added by Ridge : 2006-3-30
        /// </summary>
        /// <param name="strInput">输入对象</param>
        /// <returns>是否为空内容或Null对象</returns>
        public static bool IsEmpty(object strInput)
        {
            if (strInput == null) { return true; }
            return (strInput.ToString().Trim() == string.Empty) ? true : false;
        }

        /// <summary>
        /// 检查是否通过表单Post提交
        /// </summary>
        /// <returns>是否是POST</returns>
        public static bool IsPostMethod()
        {
            return (HttpContext.Current.Request.HttpMethod == "POST");
        }

        /// <summary>
        /// 图片上传服务器端验证
        /// </summary>
        /// <param name="imgFile">上传文件域</param>
        /// <param name="maxWidth">最大宽度，忽视为0。</param>
        /// <param name="maxHeight">最大高度，忽视为0。</param>
        /// <param name="maxFileSize">最大文件大小，忽视为0。</param>
        /// <param name="validExt">有效的文件扩展名，如"gif|jpg|png"，忽视为空字符。</param>
        /// <returns>如果符合要求，则返回的数组长度为0。</returns>
        public static string[] ValidateImage(System.Web.HttpPostedFile imgFile, int maxWidth, int maxHeight, int maxFileSize, string validExt)
        {
            System.Collections.ArrayList errList = new System.Collections.ArrayList(5);
            try
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(imgFile.InputStream, false, false);
                if (maxWidth != 0 && img.Width > maxWidth)
                {
                    errList.Add("图片宽度超过" + maxWidth.ToString() + "像素");
                }
                if (maxHeight != 0 && img.Height > maxHeight)
                {
                    errList.Add("图片高度超过" + maxHeight.ToString() + "像素");
                }
                if (maxFileSize != 0 && imgFile.ContentLength > maxFileSize)
                {
                    errList.Add("图片文件大小超过" + (maxFileSize / 1024).ToString("f1") + "KB");
                }
                if (validExt != string.Empty)
                {
                    Regex regEx = new Regex(@"\.(" + validExt + ")$", RegexOptions.IgnoreCase);
                    if (!regEx.Match(imgFile.FileName).Success)
                    {
                        errList.Add("图片文件类型不是" + validExt.Replace("|", "、") + "格式的文件");
                    }
                }
                //img.Dispose();
            }
            catch (Exception)
            {
                errList.Add("不是一个有效的图片格式");
            }
            errList.TrimToSize();
            return (string[])errList.ToArray(typeof(string));
        }


        /// <summary>
        /// 判断是否向服务器重新发送(POST)数据
        /// </summary>
        public static bool IsRefurbish()
        {
            return (HttpContext.Current.Request.Headers["Accept"] == "*/*");
        }
        #endregion 服务器判断

        /// <summary>
        /// 获取左边定长字符
        /// </summary>
        /// <param name="sourceStr">源字符</param>
        /// <param name="length">长度</param>
        /// <returns>定长字符</returns>
        public static string Left(string sourceStr, int length)
        {
            return (sourceStr.Length > length) ? sourceStr.Substring(0, length) : sourceStr;
        }

        /// <summary>
        /// 获取本地文件MD5哈希值
        /// </summary>
        /// <param name="FilePath">本地文件完整路径</param>
        /// <remarks>
        /// 如果文件不存在则返回字符N/A
        /// </remarks>
        public static string GetMD5Hash(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                return "N/A";
            }
            else
            {
                System.IO.FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                System.Security.Cryptography.MD5 md = new MD5CryptoServiceProvider();
                md.Initialize();
                byte[] b = md.ComputeHash(fs);
                return ByteArrayToHexStr(b, true);
            }
        }

        /// <summary>
        /// 二进制流的16进制字符串表达形式
        /// </summary>
        /// <param name="bytHash">二进制流</param>
        /// <param name="IsLowerCase">小写16进制字母</param>
        public static string ByteArrayToHexStr(byte[] bytHash, bool IsLowerCase)
        {
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return (IsLowerCase) ? sTemp.ToLower() : sTemp.ToUpper();
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="strEncrypt">待加密字符串</param>
        /// <returns>md5小写密码</returns>
        public static string Md5(string strEncrypt)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strEncrypt, "md5").ToLower();
        }

        /// <summary>
        /// MD5签名校验
        /// </summary>
        /// <param name="sourceTxt">文本源</param>
        /// <param name="IsLowerCase">是否小写输出</param>
        /// <returns>32Hash码</returns>
        public static string MD5(string sourceTxt, bool IsLowerCase)
        {
            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(sourceTxt);
            return MD5(byteData, IsLowerCase);
        }

        /// <summary>
        /// MD5签名校验
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="IsLowerCase">是否小写输出</param>
        /// <returns>32Hash码</returns>
        public static string MD5(byte[] byteData, bool IsLowerCase)
        {
            if (byteData != null && byteData.LongLength > 0)
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytHash = md5.ComputeHash(byteData);
                md5.Clear();

                string sTemp = "";
                for (int i = 0; i < bytHash.Length; i++)
                {
                    sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                }
                return (IsLowerCase) ? sTemp.ToLower() : sTemp.ToUpper();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取右边定长字符
        /// </summary>
        /// <param name="sourceStr">源字符</param>
        /// <param name="length">长度</param>
        /// <returns>定长字符</returns>
        public static string Right(string sourceStr, int length)
        {
            string strReturn = string.Empty;
            if (sourceStr.Length > length)
            {
                int idxStart = (sourceStr.Length - length);
                strReturn = sourceStr.Substring(idxStart);
            }
            else
            {
                strReturn = sourceStr;
            }
            return strReturn;
        }


        /// <summary>
        /// 获取时间的数值表示方式
        /// </summary>
        /// <param name="oDate">时间日期对象</param>
        /// <returns>类似于20051103115844的长整形数字</returns>
        /// <example>GetDigitDate(System.DateTime.Now)</example>
        public static string GetDigitDate(DateTime oDate)
        {
            return oDate.ToString("yyyyMMddHHmmss");
        }

        /// <summary>
        /// 获取单一匹配项组的值 ，如果有多项则选最后一项。
        /// </summary>
        /// <param name="pattern">匹配模式，支持直接量语法和选项‘/regex/gim’。</param>
        /// <param name="strInput">匹配查找源</param>
        /// <param name="objMatch">捕获匹配项集合</param>
        /// <returns>是否找到匹配</returns>
        public static bool GetSingleMatchValue(string pattern, string strInput, out string[] objMatch)
        {
            RegexOptions ExOption = RegexOptions.IgnoreCase | RegexOptions.Multiline;
            Match m = Regex.Match(pattern, "^/(.+)/(g?i?m?)$", ExOption);
            if (m.Success)
            {
                pattern = m.Groups[1].Value;
                string strOption = m.Groups[2].Value;
                if (strOption != string.Empty)
                {
                    ExOption = (strOption.IndexOf("g") != -1) ? RegexOptions.Multiline : RegexOptions.Singleline;
                    if (strOption.IndexOf("i") != -1)
                    {
                        ExOption |= RegexOptions.IgnoreCase;
                    }
                }
            }

            m = Regex.Match(strInput, pattern, ExOption);
            if (m.Success)
            {
                objMatch = new string[m.Groups.Count];
                for (int i = 0; i < objMatch.Length; i++)
                {
                    objMatch[i] = m.Groups[i].Value;
                }
                return true;
            }
            else
            {
                objMatch = null;
                return false;
            }
        }



        /// <summary>
        /// 获取数值时间的时间字符串表示方式
        /// </summary>
        /// <param name="strDate">类似于20051103115844的长整形数字</param>
        /// <param name="strDateFormat">时间的格式化字符</param>
        /// <returns>按时间格式化字符显示的时间格式</returns>
        /// <example>ShowDigitDate("20051103115844","yyyy-MM-dd HH:mm:ss")</example>
        public static string ShowDigitDate(string strDate, string strDateFormat)
        {
            if (strDate.Length == 14)
            {
                System.DateTime date = new DateTime(int.Parse(strDate.Substring(0, 4)), //年
                    int.Parse(strDate.Substring(4, 2)),//月
                    int.Parse(strDate.Substring(6, 2)),//日
                    int.Parse(strDate.Substring(8, 2)),//时
                    int.Parse(strDate.Substring(10, 2)),//分
                    int.Parse(strDate.Substring(12, 2)) //秒
                    );
                return date.ToString(strDateFormat);
            }
            else
            {
                return strDate;
            }
        }


        /// <summary>
        /// 获取数值时间的时间字符串表示方式
        /// </summary>
        /// <param name="strDate">类似于20051103115844的长整形数字</param>
        /// <returns>格式为2005-11-03 11:58:44的时间</returns>
        public static string ShowDigitDate(string strDate)
        {
            return ShowDigitDate(strDate, "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 创建一个内容数据表格
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="columnNames">列名集合</param>
        /// <param name="firstRowData">第一行的值</param>
        /// <remarks>应用范围：填充GridView的数据源，已显示Grid FooterItem。</remarks>
        public static DataTable CreateDataTableWithSimpleData(string tableName, string[] columnNames, object[] firstRowData)
        {
            DataTable emptyTab = new DataTable(tableName);
            if (columnNames.Length != firstRowData.Length)
            {
                throw new InvalidOperationException("数据和列长度不一致！");
            }
            else
            {
                for (int i = 0, j = columnNames.Length; i < j; i++)
                {
                    emptyTab.Columns.Add(new DataColumn(columnNames[i], firstRowData[i].GetType()));
                }
                DataRow dRow = emptyTab.NewRow();
                dRow.ItemArray = firstRowData;
                emptyTab.Rows.Add(dRow);

                return emptyTab;
            }
        }


        #region Web调试辅助
        /// <summary>
        /// 调试信息Web页面输出
        /// </summary>
        /// <param name="blnEndResponse">终止页面输出</param>
        /// <param name="objPara">要输出的变量</param>
        /// <returns>页面输出信息</returns>
        public static void Debug(bool blnEndResponse, params object[] objPara)
        {
            foreach (object obj in objPara)
            {
                if (obj != null)
                {
                    System.Web.HttpContext.Current.Response.Write(String.Format("<xmp>{0}</xmp>", obj.ToString()));
                }
                else
                {
                    System.Web.HttpContext.Current.Response.Write("<xmp>该对象为Null！</xmp>");
                }
            }
            if (blnEndResponse)
            {
                System.Web.HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 输出文本内容
        /// </summary>
        /// <param name="html">文本内容</param>
        /// <param name="endResponse">是否终止输出</param>
        public static void echo(string html, bool endResponse)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Write(html);
                if (endResponse) HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 调试信息Web页面输出
        /// </summary>
        public static void Debug(params object[] objPara)
        {
            Debug(false, objPara);
        }

        /// <summary>
        /// 输出浏览器请求信息
        /// </summary>
        public static void PrintHttpRequest()
        {
            Debug(false, GetHttpRequestInfo());
        }

        /// <summary>
        /// 获取输出浏览器请求信息
        /// </summary>
        public static string GetHttpRequestInfo()
        {
            StringBuilder sb = new StringBuilder(4000);
            sb.Append("Http ServerVariables:" + System.Environment.NewLine);
            foreach (string item in HttpContext.Current.Request.ServerVariables)
            {
                sb.Append(item + " = " + HttpContext.Current.Request.ServerVariables[item] + System.Environment.NewLine);
            }

            sb.Append("Http Headers:" + System.Environment.NewLine);
            foreach (string item in HttpContext.Current.Request.Headers)
            {
                sb.Append(item + " = " + HttpContext.Current.Request.Headers[item] + System.Environment.NewLine);
            }

            sb.Append(System.Environment.NewLine);
            sb.Append("Http Cookies:" + System.Environment.NewLine);
            foreach (string item in HttpContext.Current.Request.Cookies)
            {
                sb.Append(item + " = " + HttpContext.Current.Request.Cookies[item].Value + System.Environment.NewLine);
            }

            sb.Append(System.Environment.NewLine);
            sb.Append("Http QueryString:" + System.Environment.NewLine);
            foreach (string item in HttpContext.Current.Request.QueryString)
            {
                sb.Append(item + " = " + HttpContext.Current.Request.QueryString[item] + System.Environment.NewLine);
            }

            sb.Append(System.Environment.NewLine);
            sb.Append("Http Form:" + System.Environment.NewLine);
            foreach (string item in HttpContext.Current.Request.Form)
            {
                sb.Append(item + " = " + HttpContext.Current.Request.Form[item] + System.Environment.NewLine);
            }
            return sb.ToString();
        }

        #endregion Web调试辅助


        /// <summary>
        /// 读取Web.config的键值
        /// </summary>
        /// <param name="key">AppSettings键</param>
        /// <returns>AppSettings键值</returns>
        public static string GetAppConfig(string key)
        {
            // return ConfigurationSettings.AppSettings[key];
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 获取URL地址的完整路径
        /// </summary>
        /// <param name="url">相对/对比的Url地址</param>
        /// <param name="baseUrl">相对地址的目录或URL</param>
        public static string GetAbsoluteUrl(string url, string baseUrl)
        {
            string urlLower = url.ToLower();
            if (urlLower.StartsWith("#")
                || urlLower.StartsWith("ftp://")
                || urlLower.StartsWith("http://")
                || urlLower.StartsWith("https://")
                || urlLower.StartsWith("javascript:"))
            {
                return url;
            }
            else
            {
                bool blnNotAddPrefix = (baseUrl.ToLower().StartsWith("http://")
                    || baseUrl.ToLower().StartsWith("ftp://")
                    || baseUrl.ToLower().StartsWith("https://"));
                if (!blnNotAddPrefix) baseUrl = "http://localhost/" + baseUrl.TrimStart('/');
                try
                {
                    return (new Uri(new Uri(baseUrl), url)).OriginalString.Substring((blnNotAddPrefix) ? 0 : 16);
                }
                catch (Exception)
                {
                    return url;
                }
            }
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        public static string GetIP()
        {
            string result = String.Empty;
            result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (null == result || result == String.Empty)
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (null == result || result == String.Empty)
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }
            if (null == result || result == String.Empty || !Regex.IsMatch(result, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$"))
            {
                return "0.0.0.0";
            }
            return result;
        }

        /// <summary>
        /// 获取定长(像素)字符
        /// </summary>
        /// <param name="str">待处理字符</param>
        /// <param name="length">长度(像素)</param>
        /// <returns>相应长度(像素)的字符</returns>
        public static string GetLengthString(string str, int length)
        {
            int cn = 14;
            int enb = 10;
            int ens = 8;
            int tlength = 0;
            StringBuilder sb = new StringBuilder();
            char[] chararr = str.ToCharArray();
            for (int i = 0; i < chararr.Length && tlength < length; i++)
            {
                if ((int)chararr[i] <= 255)
                {
                    if ((int)chararr[i] >= 65 && (int)chararr[i] <= 90)
                    {
                        tlength += enb;
                        if (tlength <= length) sb.Append(chararr[i].ToString());
                    }
                    else
                    {
                        tlength += ens;
                        if (tlength <= length) sb.Append(chararr[i].ToString());
                    }
                }
                else
                {
                    tlength += cn;
                    if (tlength <= length) sb.Append(chararr[i].ToString());
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 转换ArrayList为字符串数组
        /// </summary>
        /// <param name="arr">ArrayList数组</param>
        /// <returns>对等字符串数组</returns>
        public static string[] GetObjString(ArrayList arr)
        {
            if (arr == null) return null;
            arr.TrimToSize();
            return (string[])arr.ToArray(typeof(string));
        }

        /// <summary>
        /// 获取字符长度
        /// </summary>
        /// <param name="str">待处理字符</param>
        /// <returns>获取字符长度(像素)</returns>
        public static int GetStringLength(string str)
        {
            int cn = 14;
            int enb = 10;
            int ens = 8;
            int tlength = 0;
            char[] chararr = str.ToCharArray();
            for (int i = 0; i < chararr.Length; i++)
            {
                if ((int)chararr[i] <= 255)
                {
                    if ((int)chararr[i] >= 65 && (int)chararr[i] <= 90)
                    {
                        tlength += enb;
                    }
                    else
                    {
                        tlength += ens;
                    }
                }
                else
                {
                    tlength += cn;
                }
            }
            return tlength;
        }

        #region 网络辅助
        /// <summary>
        /// 获取有效的文件路径
        /// </summary>
        /// <param name="strFilePath">相对或完整路径文件地址</param>
        /// <returns>完整的文件路径地址</returns>
        public static string ParseAppPath(string strFilePath)
        {
            //程序域相对目录
            if (System.Web.HttpContext.Current == null)
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    strFilePath.TrimStart('~', '/').Replace('/', '\\'));
            }
            else
            {

                //Asp.net App程序相对目录
                if (strFilePath.StartsWith("~"))
                {
                    strFilePath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + strFilePath.Substring(1).Replace("/", @"\");
                    strFilePath = strFilePath.Replace(@"\\", @"\");
                    return strFilePath;
                }
                else
                {
                    //本地文件路径
                    if (strFilePath.IndexOf(@":\") == -1)
                    {
                        strFilePath = System.Web.HttpContext.Current.Server.MapPath(strFilePath);
                    }
                }
                return strFilePath;
            }
        }

        /// <summary>
        /// 远程文件是否存在
        /// </summary>
        public static bool HttpExistFile(string url, out long FileSize)
        {
            bool isExist = true;

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/msword, application/vnd.ms-powerpoint, application/vnd.ms-excel, */*";
            req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322)";
            req.Method = "GET";
            req.AllowAutoRedirect = false;
            req.KeepAlive = false;
            req.Referer = url;

            HttpWebResponse resp = null;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                resp = (HttpWebResponse)ex.Response;
            }

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                isExist = false;
                FileSize = 0;
            }
            else
            {
                isExist = true;
                FileSize = resp.ContentLength; ;
            }
            resp.Close();
            return isExist;
        }


        /// <summary>
        /// 本地文件远程Web上传
        /// </summary>
        /// <param name="url">远程上传提交表单地址</param>
        /// <param name="cookies">上传所需的Cookie</param>
        /// <param name="fileFormName">文件表单名称</param>
        /// <param name="uploadfile">上传本地文件完整路径</param>
        /// <param name="contenttype">上传文件类型</param>
        /// <param name="querystring">URL附加键值对</param>
        /// <param name="formKVPair">表单内容键值对</param>
        /// <returns>上传地址处理返回网页内容</returns>
        public static string UploadFileEx(string urlToPost, CookieContainer cookies,
            string fileFormName,
            string localFilePath,
            string contenttype,
            NameValueCollection querystring,
            NameValueCollection formKVPair)
        {
            if ((fileFormName == null) || (fileFormName.Length == 0))
            {
                fileFormName = "file";
            }

            if ((contenttype == null) || (contenttype.Length == 0))
            {
                contenttype = "application/octet-stream";
            }


            string postdata = "";
            if (querystring != null)
            {
                foreach (string key in querystring.Keys)
                {
                    postdata += key + "=" + querystring.Get(key) + "&";
                }
                postdata = "?" + postdata.TrimEnd('&');
            }

            Uri uri = new Uri(urlToPost + postdata);
            string boundary = "----------7d" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
            webrequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 2.0.50727; Fanmaquar)";
            webrequest.CookieContainer = cookies;
            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";


            // Build up the post message header
            StringBuilder sb = new StringBuilder();

            #region 表单内容键值对
            if (formKVPair != null)
            {
                foreach (string key in formKVPair.AllKeys)
                {
                    sb.Append("--" + boundary + "\r\n");
                    sb.Append("Content-Disposition: form-data; name=\"" + key + "\"");
                    sb.Append("\r\n\r\n");
                    sb.Append(formKVPair[key]);
                    sb.Append("\r\n");
                }
            }
            #endregion

            #region 上传文件节点数据
            sb.Append("--" + boundary + "\r\n");

            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(fileFormName);
            sb.Append("\"; filename=\"");
            sb.Append(Path.GetFileName(localFilePath));
            sb.Append("\"");
            sb.Append("\r\n");

            sb.Append("Content-Type: ");
            sb.Append(contenttype);
            sb.Append("\r\n");
            #endregion

            sb.Append("\r\n");

            string postHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

            // Build the trailing boundary string as a byte array
            // ensuring the boundary appears on a line by itself
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            FileStream fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            long length = postHeaderBytes.Length + fileStream.Length + boundaryBytes.Length;
            webrequest.ContentLength = length;

            Stream requestStream = webrequest.GetRequestStream();

            // Write out our post header
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // Write out the file contents
            byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                requestStream.Write(buffer, 0, bytesRead);

            // Write out the trailing boundary
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            WebResponse responce = webrequest.GetResponse();

            Stream s = responce.GetResponseStream();
            StreamReader sr = new StreamReader(s);

            return sr.ReadToEnd();
        }

        //private static string RootFileNameAndEnsureTargetFolderExists(string fileName)
        //{
        //    string rootedFileName = fileName;
        //    rootedFileName = rootedFileName.Replace("%yyyy%", DateTime.Today.Year.ToString()).Replace("%MM%", DateTime.Today.Month.ToString("00")).Replace("%dd%", DateTime.Today.Day.ToString("00"));
        //    if (!Path.IsPathRooted(rootedFileName))
        //    {
        //        rootedFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootedFileName);
        //    }
        //    string directory = Path.GetDirectoryName(rootedFileName);
        //    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        //    {
        //        Directory.CreateDirectory(directory);
        //    }
        //    return rootedFileName;
        //}

        /// <summary>
        /// 记录异常消息到日志文件
        /// </summary>
        /// <param name="filePath">日志文件路径及文件名</param>
        /// <param name="exp">异常实例</param>
        public static void Log2File(string filePath, Exception exp)
        {
            string expMsg = string.Format("错误消息：{0} " + Environment.NewLine
                 + "导致错误的应用程序或对象的名称：{1}  " + Environment.NewLine
                 + "堆栈内容：{2} " + Environment.NewLine
                 + "引发异常的方法：{3} " + Environment.NewLine,
                exp.Message,
                exp.Source,
                exp.StackTrace,
                exp.TargetSite);
            Log2File(System.Text.Encoding.Default.WebName, filePath, expMsg);
        }

        /// <summary>
        /// 记录对象数据到日志文件
        /// </summary>
        /// <param name="filePath">日志文件路径及文件名</param>
        /// <param name="objData">要需记录的数据</param>
        public static void Log2File(string filePath, object objData)
        {
            Log2File(System.Text.Encoding.Default.WebName, filePath, objData);
        }

        private static object ThreadingLockObject = new object();
        /// <summary>
        /// 记录对象数据到日志文件
        /// </summary>
        /// <param name="charset">记录文件编码字符集</param>
        /// <param name="filePath">日志文件路径及文件名</param>
        /// <param name="objData">要需记录的数据</param>
        public static void Log2File(string charset, string filePath, object objData)
        {
            //lock (ThreadingLockObject)
            //{
            //    filePath = RootFileNameAndEnsureTargetFolderExists(ParseAppPath(filePath));
            //    System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath,
            //       true, System.Text.Encoding.GetEncoding(charset));
            //    writer.Write(System.Environment.NewLine);
            //    writer.Write("----{0} @ThreadId:({1}) {2}",
            //        System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"),
            //        System.Threading.Thread.CurrentThread.ManagedThreadId,
            //        Environment.NewLine);
            //    writer.Write(objData ?? "NULL");
            //    writer.Flush();
            //    writer.Close();
            //}
            //Vbyte.Common.LogHelper.DebugFormat("{0}",objData ?? "NULL");

        }

        #endregion

        #region  客户端模拟/辅助函数

        /// <summary>
        /// 客户端脚本:alert(Msg)
        /// </summary>
        /// <param name="Msg">要显示的消息</param>
        public static void Alert(string Msg)
        {
            WriteScript(false, "alert(\"" + Msg + "\");");
        }

        ///<summary>
        /// 客户端脚本:alert(Msg)
        ///</summary>
        ///<param name="Msg">要显示的消息</param>
        ///<param name="returnURL">返回地址</param>
        public static void Alert(string Msg, string returnURL)
        {
            WriteScript(false, "alert(\"" + Msg + "\");location.href='" + returnURL + "';");
        }

        /// <summary>
        /// 客户端脚本:alert(Msg)，并返回。
        /// </summary>
        /// <param name="Msg">要显示的消息</param>
        /// <param name="goBack">是否返回</param>
        public static void Alert(string Msg, bool goBack)
        {
            if (goBack == true)
            {
                WriteScript(false, "alert(\"" + Msg + "\");history.back();");
            }
            else
            {
                WriteScript(false, "alert(\"" + Msg + "\");");
            }
        }

        /// <summary>
        /// 客户端脚本:confrim(Msg)
        /// </summary>
        /// <param name="Msg">待确认的消息</param>
        /// <param name="url">确认之后转向的地址</param>
        public static void Confirm(string Msg, string url)
        {
            WriteScript(false, "if (confirm('" + Msg + "')) \r\n" +
                " { location.href='" + url + "'; }\r\n");
        }

        /// <summary>
        /// 客户端脚本:confrim(Msg)
        /// </summary>
        /// <param name="Msg">待确认的消息</param>
        /// <param name="cfmurl">确认之后转向的地址</param>
        /// <param name="retrunURL">取消之后转向的地址</param>
        public static void Confirm(string Msg, string cfmurl, string retrunURL)
        {
            WriteScript(false, "if (confirm('" + Msg + "')) \r\n" +
                " { location.href='" + cfmurl + "'; } \r\n" +
                "else { location.href='" + retrunURL + "'; }\r\n");
        }

        /// <summary>
        /// 客户端脚本:confrim(Msg)
        /// </summary>
        /// <param name="Msg">待确认的消息</param>
        /// <param name="objScript">确认之后转向的脚本操作</param>
        public static void Confirm(string Msg, params object[] objScript)
        {
            string strScripts = string.Empty;
            foreach (object obj in objScript)
            {
                strScripts += obj.ToString() + "\r\n";
            }
            WriteScript(false, "if (confirm('" + Msg + "')) \r\n" +
                " { \r\n"
                + strScripts
                + "}\r\n");
        }

        /// <summary>
        /// 客户端脚本:重定向网址
        /// </summary>
        /// <param name="URL">重定向的网址</param>
        /// <param name="CopyHistory">是否记录历史</param>
        public static void Redirect(string URL, bool CopyHistory)
        {
            if (CopyHistory == true)
            {
                WriteScript(true, "top.location.href='" + URL + "';");
            }
            else
            {
                WriteScript(true, "top.location.replace('" + URL + "');");
            }
        }

        /// <summary>
        /// 脚本重新打开当前页面
        /// </summary>
        public static void ReloadURL()
        {
            WriteScript(true, "location.href='" + HttpContext.Current.Request.Url + "';");
        }

        /// <summary>
        /// 在服务器表单内注册一串页面加载完成之后的Javascript脚本
        /// </summary>
        /// <param name="page">当前引用页面</param>
        /// <param name="objScript">脚本数组</param>
        public static void WriteDocCompleteScript(Page page, params object[] objScript)
        {
            string str = string.Empty;
            foreach (object obj2 in objScript)
            {
                str = str + obj2.ToString() + "\r\n";
            }
            str = "var eventComplete = window.onload;var __customFunction = function() {\n" + str
                + "\n};\nif (typeof(window.onload) != 'function')  {window.onload = __customFunction; } \n else {window.onload = function(){ \neventComplete();__customFunction();}}";
            page.ClientScript.RegisterStartupScript(page.GetType(), "_ScriptsLoaded", string.Format("<script language=\"javascript\">{0}</script>\r\n", str), false);
        }


        /// <summary>
        /// Web页面javascript脚本输出
        /// </summary>
        /// <param name="blnEndResponse">终止页面输出</param>
        /// <param name="objPara">要输出的变量</param>
        public static void WriteScript(bool blnEndResponse, params object[] objPara)
        {
            foreach (object obj in objPara)
            {
                System.Web.HttpContext.Current.Response.Write(String.Format("<script language=\"javascript\">{0}</script>\r\n", obj.ToString()));
            }
            if (blnEndResponse)
            {
                System.Web.HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// Web页面javascript脚本输出
        /// </summary>
        /// <param name="objPara">要输出的变量</param>
        public static void WriteScript(params object[] objPara)
        {
            WriteScript(false, objPara);
        }

        /// <summary>
        /// 在服务器表单内注册一串Javascript脚本
        /// </summary>
        /// <param name="page">当前引用页面</param>
        /// <param name="objScript">脚本语句数组</param>
        public static void WriteStartUpScript(Page page, params object[] objScript)
        {
            string str = string.Empty;
            foreach (object obj2 in objScript)
            {
                str = str + obj2.ToString() + "\r\n";
            }
            page.ClientScript.RegisterStartupScript(page.GetType(), "_script", string.Format("<script language=\"javascript\">{0}</script>\r\n", str), false);
        }

        #endregion


        #region 文本编码/解码

        /// <summary>
        /// Base64编码
        /// </summary>
        public static string Base64Encode(string SourceText, string charset)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding(charset).GetBytes(SourceText);
            string strBase64 = SourceText;
            try
            {
                strBase64 = System.Convert.ToBase64String(bytes);
            }
            catch { }
            return strBase64;
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        public static string Base64Encode(string SourceText)
        {
            return Base64Encode(SourceText, Encoding.Default.WebName);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        public static string Base64Decode(string strBase64)
        {
            return Base64Decode(strBase64, Encoding.Default.WebName);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        public static string Base64Decode(string strBase64, string charset)
        {
            string strSourceText = string.Empty;
            try
            {
                byte[] bytes = Convert.FromBase64String(strBase64);
                strSourceText = Encoding.GetEncoding(charset).GetString(bytes);
            }
            catch { }
            return strSourceText;
        }

        #endregion 文本编码/解码


        #region 字符过滤与获取
        /// <summary>
        /// 根据自定义的匹配参数设置和处理函数进行替换操作
        /// </summary>
        /// <param name="txtToProcess">要处理的字符串</param>
        /// <param name="cmp">替换配置实例</param>
        /// <param name="replaceDelegate">替换处理委托</param>
        /// <returns>经过相关处理过的字符</returns>
        /// <example>
        /// <![CDATA[
        /// /// <summary>
        /// ///处理HTML文件中的资源地址为绝对路径形式
        /// </summary>
        /// public static string GetRightHtmlText(string baseUrl, string htmlContent)
        /// {
        ///     return Util.GetCustomMatchReplace(htmlContent,
        ///        new CustomMatchReplace("(src|href|url|background)=(?<jsescape>\\\\?)(('|\")?)([^\\s>]+)\\2",
        ///            (RegexOptions.IgnoreCase | RegexOptions.Compiled),
        ///            baseUrl),
        ///       new CustomMatchReplace.ReplaceMatchedHandler(myReplaceHandler));
        /// }
        /// 
        ///  public static string myReplaceHandler(Match m, CustomMatchReplace cmp)
        ///  {
        ///        return m.Groups[1].Value + "=" + m.Groups["jsescape"].Value + m.Groups[2].Value
        ///                + Util.GetAbsoluteUrl(m.Groups[4].Value, cmp.DataStorage[0].ToString()) + m.Groups[3].Value;
        ///  }
        /// 
        /// ]]>
        /// </example>
        public static string GetCustomMatchReplace(string txtToProcess, CustomMatchReplace cmp, CustomMatchReplace.ReplaceMatchedHandler replaceDelegate)
        {
            StringBuilder sb = new StringBuilder(txtToProcess.Length);
            Regex RE = new Regex(cmp.MatchPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            if (cmp.MatchOptions != RegexOptions.None)
            {
                RE = new Regex(cmp.MatchPattern, cmp.MatchOptions);
            }

            MatchCollection mc = RE.Matches(txtToProcess);
            int idxBegin = 0, idxEnd = 0;
            foreach (Match m in mc)
            {
                idxEnd = m.Index;
                sb.Append(txtToProcess.Substring(idxBegin, idxEnd - idxBegin));
                sb.Append(replaceDelegate(m, cmp));
                idxBegin = idxEnd + m.Length;
            }

            if (idxBegin < txtToProcess.Length)
            {
                sb.Append(txtToProcess.Substring(idxBegin));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取能够在客户端显示的Javascript文本内容
        /// </summary>
        /// <param name="OStr">文本内容</param>
        /// <returns>合格的Javascript文本内容</returns>
        public static string GetJsString(string OStr)
        {
            OStr = OStr.Replace("\\", "\\\\"); // 转义符号
            OStr = OStr.Replace("\'", "\\'"); // 单引号
            OStr = OStr.Replace("\"", "\\" + "\""); // 双引号
            OStr = OStr.Replace("\r\n", "\\" + "r" + "\\" + "n"); // 回车换行
            OStr = OStr.Replace("\r", "\\" + "r"); // 回车
            OStr = OStr.Replace("\n", "\\" + "n"); // 换行
            return OStr;
        }

        /// <summary>
        /// 正则表达式替换
        /// Ridge Wong (2006-3-31)
        /// </summary>
        /// <param name="strInput">替换目标字符</param>
        /// <param name="strPattern">匹配模式，支持直接量语法和选项‘/regex/gim’。</param>
        /// <param name="strReplacement">替换为相应字符</param>
        /// <returns>相关字符替换结果</returns>
        public static string RegexReplace(object strInput, string strPattern, string strReplacement)
        {
            if (strInput == null) return string.Empty;
            RegexOptions ExOption = RegexOptions.IgnoreCase | RegexOptions.Multiline;
            Match m = Regex.Match(strPattern, "^/(.+)/(g?i?m?)$", ExOption);
            if (m.Success)
            {
                //设置模式与选项
                strPattern = m.Groups[1].Value;
                string strOption = m.Groups[2].Value;
                if (strOption != string.Empty)
                {
                    ExOption = RegexOptions.None;
                    if (strOption.IndexOf("g") != -1) ExOption |= RegexOptions.ECMAScript;
                    if (strOption.IndexOf("i") != -1) ExOption |= RegexOptions.IgnoreCase;
                    if (strOption.IndexOf("m") != -1) ExOption |= RegexOptions.Multiline;
                }
            }
            return Regex.Replace(strInput.ToString(), strPattern, strReplacement, ExOption);
        }

        /// <summary>
        /// 字符块过滤(剔除从前置到后置尾的所有块的新字符Source)
        /// </summary>
        /// <param name="Source">原始字符</param>
        /// <param name="snippet">中间代码片断，以“REGEX:”开头则后续字符视为不区分大小写正则匹配模式。</param>
        /// <param name="strBefore">片断前置字符</param>
        /// <param name="strAfter">片断后置字符</param>
        public static void FilterBlock(ref string Source, string snippet, string strBefore, string strAfter)
        {
            int idx = -1, len = snippet.Length;
            if (snippet.StartsWith("REGEX:"))
            {
                string pattern = snippet.Substring(6);
                MatchCollection mc = Regex.Matches(Source, pattern, RegexOptions.IgnoreCase);
                if (mc.Count > 0)
                {
                    idx = mc[0].Index;
                    len = mc[0].Length;
                }
            }
            else
            {
                idx = Source.IndexOf(snippet);
            }

            if (idx != -1)
            {
                int idxBegin = Source.LastIndexOf(strBefore, idx);
                int idxEnd = Source.IndexOf(strAfter, idx + len);
                if (idxBegin != -1 && idxEnd != -1)
                {
                    len = idxEnd + strAfter.Length - idxBegin;
                    Source = Source.Replace(Source.Substring(idxBegin, len), "");
                }
            }
        }



        /// <summary>
        /// 获取特定字符片断字符块
        /// </summary>
        /// <param name="Source">原始字符</param>
        /// <param name="snippet">中间代码片断，以“REGEX:”开头则后续字符视为不区分大小写正则匹配模式。</param>
        /// <param name="strBefore">片断前置字符</param>
        /// <param name="strAfter">片断后置字符</param>
        public static string GetSnippetBlock(ref string Source, string snippet, string strBefore, string strAfter)
        {
            int idx = -1, len = snippet.Length;
            string strReturn = string.Empty;

            if (snippet.StartsWith("REGEX:"))
            {
                string pattern = snippet.Substring(6);
                MatchCollection mc = Regex.Matches(Source, pattern, RegexOptions.IgnoreCase);
                if (mc.Count > 0)
                {
                    idx = mc[0].Index;
                    len = mc[0].Length;
                }
            }
            else
            {
                idx = Source.IndexOf(snippet);
            }

            if (idx != -1)
            {
                int idxBegin = Source.LastIndexOf(strBefore, idx);
                int idxEnd = Source.IndexOf(strAfter, idx + len);
                if (idxBegin != -1 && idxEnd != -1)
                {
                    len = idxEnd + strAfter.Length - idxBegin;
                    strReturn = Source.Substring(idxBegin, len);
                }
            }

            return strReturn;
        }


        /// <summary>
        /// 获取特定字符片断Tag所在的索引
        /// </summary>
        /// <param name="Source">原始字符查找</param>
        /// <param name="snippet">查找片断，以“REGEX:”开头则后续字符视为不区分大小写正则匹配模式。</param>
        /// <param name="strTagFind">临近字符</param>
        /// <param name="findNext">是否向下查找</param>
        /// <returns>如果找到tagFind，则返回tagFind的索引，反之为-1。</returns>
        public static int GetSnippetTagIndex(string Source, string snippet, string strTagFind, bool findNext)
        {
            int idx = -1, len = snippet.Length;
            if (snippet.StartsWith("REGEX:"))
            {
                string pattern = snippet.Substring(6);
                MatchCollection mc = Regex.Matches(Source, pattern, RegexOptions.IgnoreCase);
                if (mc.Count > 0)
                {
                    idx = mc[0].Index;
                    len = mc[0].Length;
                }
            }
            else
            {
                idx = Source.IndexOf(snippet);
            }

            if (idx != -1)
            {
                idx = (findNext) ? Source.IndexOf(strTagFind, idx + len) : Source.LastIndexOf(strTagFind, idx);
            }
            return idx;
        }
        #endregion 字符过滤与获取

        #region 反射/元数据调用
        /// <summary>
        /// 调用.NET配件的指定方法
        /// </summary>
        /// <param name="Path">.NET配件路径</param>
        /// <param name="NameSpaceAndClassName">命名空间和类名，如System.Web</param>
        /// <param name="MethodName">方法名称</param>
        /// <param name="Parameters">调用该方法的参数</param>
        /// <returns>运行时消息</returns>
        public static RuntimeMessage CallAsmMethod(string Path, string NameSpaceAndClassName, string MethodName, object[] Parameters)
        {
            try
            {
                //调入文件(不限于dll,exe亦可,只要是.net)
                Assembly Ass = Assembly.LoadFrom(Path);
                //NameSpaceAndClassName是"名字空间.类名",如"namespace1.Class1"
                Type TP = Ass.GetType(NameSpaceAndClassName);
                //MethodName是要调用的方法名,如"Main"
                MethodInfo MI = TP.GetMethod(MethodName);
                object MeObj = System.Activator.CreateInstance(TP);
                //Parameters是调用目标方法时传入的参数列表
                object objResult = MI.Invoke(MeObj, Parameters);
                return new RuntimeMessage(true, "成功调用", 0, objResult);
            }
            catch (Exception e)
            {
                return new RuntimeMessage(false, "出现异常,消息为:" + e.Message, -1, e);
            }
        }

        /// <summary>
        /// 直接调用内部对象的方法/函数(支持重载调用)
        /// </summary>
        /// <param name="refType">目标数据类型</param>
        /// <param name="funName">函数名称，区分大小写。</param>
        /// <param name="funParams">函数参数信息</param>
        public static object InvokeFunction(Type refType, string funName, params object[] funParams)
        {
            return InvokeMethodOrGetProperty(refType, funName, null, funParams);
        }

        /// <summary>
        /// 直接调用内部对象的方法/函数或获取属性(支持重载调用)
        /// </summary>
        /// <param name="refType">目标数据类型</param>
        /// <param name="funName">函数名称，区分大小写。</param>
        /// <param name="objInitial">如果调用属性，则为相关对象的初始化数据，否则为Null。</param>
        /// <param name="funParams">函数参数信息</param>
        /// <returns>运行结果</returns>
        public static object InvokeMethodOrGetProperty(Type refType, string funName, object[] objInitial, params object[] funParams)
        {
            MemberInfo[] mis = refType.GetMember(funName);
            if (mis.Length < 1)
            {
                throw new InvalidProgramException(string.Concat("函数/方法 [", funName, "] 在指定类型(", refType.ToString(), ")中不存在！"));
            }
            else
            {
                MethodInfo targetMethod = null;
                StringBuilder pb = new StringBuilder();
                foreach (MemberInfo mi in mis)
                {
                    if (mi.MemberType != MemberTypes.Method)
                    {
                        if (mi.MemberType == MemberTypes.Property)
                        {
                            #region 调用属性方法Get
                            PropertyInfo pi = (PropertyInfo)mi;
                            targetMethod = pi.GetGetMethod();
                            break;
                            #endregion
                        }
                        else
                        {
                            throw new InvalidProgramException(string.Concat("[", funName, "] 不是有效的函数/属性方法！"));
                        }
                    }
                    else
                    {
                        #region 检查函数参数和数据类型 绑定正确的函数到目标调用
                        bool validParamsLen = false, validParamsType = false;

                        MethodInfo curMethod = (MethodInfo)mi;
                        ParameterInfo[] pis = curMethod.GetParameters();
                        if (pis.Length == funParams.Length)
                        {
                            validParamsLen = true;

                            pb = new StringBuilder();
                            bool paramFlag = true;
                            int paramIdx = 0;

                            #region 检查数据类型 设置validParamsType是否有效
                            foreach (ParameterInfo pi in pis)
                            {
                                pb.AppendFormat("Parameter {0}: Type={1}, Name={2}\n", paramIdx, pi.ParameterType, pi.Name);

                                //不对Null和接受Object类型的参数检查
                                if (funParams[paramIdx] != null && pi.ParameterType != typeof(object) &&
                                     (pi.ParameterType != funParams[paramIdx].GetType()))
                                {
                                    #region 检查类型是否兼容
                                    try
                                    {
                                        funParams[paramIdx] = Convert.ChangeType(funParams[paramIdx], pi.ParameterType);
                                    }
                                    catch (Exception)
                                    {
                                        paramFlag = false;
                                    }
                                    #endregion
                                    //break;
                                }
                                ++paramIdx;
                            }
                            #endregion

                            if (paramFlag == true)
                            {
                                validParamsType = true;
                            }
                            else
                            {
                                continue;
                            }

                            if (validParamsLen && validParamsType)
                            {
                                targetMethod = curMethod;
                                break;
                            }
                        }
                        #endregion
                    }
                }

                if (targetMethod != null)
                {
                    object objReturn = null;
                    #region 兼顾效率和兼容重载函数调用
                    try
                    {
                        object objInstance = System.Activator.CreateInstance(refType, objInitial);
                        objReturn = targetMethod.Invoke(objInstance, BindingFlags.InvokeMethod, Type.DefaultBinder, funParams,
                            System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        objReturn = refType.InvokeMember(funName, BindingFlags.InvokeMethod, Type.DefaultBinder, null, funParams);
                    }
                    #endregion
                    return objReturn;
                }
                else
                {
                    pb.AppendLine("---------------------------------------------");
                    pb.AppendLine("传递参数信息：");
                    foreach (object fp in funParams)
                    {
                        pb.AppendFormat("Type={0}, value={1}\n", fp.GetType(), fp);
                    }

                    throw new InvalidProgramException(string.Concat("函数/方法 [", refType.ToString(), ".", funName,
                        "(args ...) ] 参数长度和数据类型不正确！\n 引用参数信息参考：\n",
                        pb.ToString()));
                }
            }

        }

        /// <summary>
        /// 获取相关对象实体特定属性的值
        /// </summary>
        /// <param name="obj">对象实体</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static object GetInstanceValue(object obj, string propertyName)
        {
            object value = null;
            if (string.IsNullOrEmpty(propertyName))
            {
                value = obj;
            }
            else
            {
                PropertyDescriptor pd = TypeDescriptor.GetProperties(obj).Find(propertyName, true);
                if (pd != null)
                {
                    value = pd.GetValue(obj);
                }
                else
                {
                    value = obj;
                }
            }
            return value;
        }

        /// <summary>
        /// 根据DataReader的当前数据填充实体数据
        /// </summary>
        /// <param name="model">数据实体模型</param>
        /// <param name="dr">DataReader对象</param>
        public static void FillInstanceValue(object model, IDataReader dr)
        {
            Type type = model.GetType();
            PropertyInfo pi;
            for (int i = 0; i < dr.FieldCount; i++)
            {
                pi = type.GetProperty(dr.GetName(i));
                if (pi != null)
                {
                    pi.SetValue(model, dr[i], null);
                }
            }
        }

        /// <summary>
        /// 根据数据行数据填充实体数据
        /// </summary>
        /// <param name="model">数据实体模型</param>
        /// <param name="dRow">数据行数据</param>
        public static void FillInstanceValue(object model, DataRow dRow)
        {
            Type type = model.GetType();
            PropertyInfo pi;
            for (int i = 0; i < dRow.Table.Columns.Count; i++)
            {
                pi = type.GetProperty(dRow.Table.Columns[i].ColumnName);
                if (pi != null)
                {
                    pi.SetValue(model, dRow[i], null);
                }
            }
        }
        #endregion

    }

    /// <summary>
    /// 自定义匹配替换类
    /// </summary>
    public class CustomMatchReplace
    {
        /// <summary>
        /// 自定义匹配替换类
        /// </summary>
        public CustomMatchReplace() { }

        /// <summary>
        /// 自定义匹配替换类
        /// </summary>
        /// <param name="repPattern">初始化的匹配模式</param>
        public CustomMatchReplace(string repPattern)
        {
            this.MatchPattern = repPattern;
        }

        /// <summary>
        /// 自定义匹配替换类(初始化指定所有数据)
        /// </summary>
        /// <param name="repPattern">初始化的匹配模式</param>
        /// <param name="options">匹配选项</param>
        /// <param name="otherData">其他存储数据，将依次存储在<c>DataStorage</c>中。</param>
        public CustomMatchReplace(string repPattern, RegexOptions options, params object[] otherData)
        {
            this.MatchPattern = repPattern;
            this.MatchOptions = options;
            if (otherData != null)
            {
                for (int i = 0; i < otherData.Length; ++i)
                {
                    DataStorage.Add(otherData[i]);
                }
            }
        }

        private string _matchPattern = "";
        /// <summary>
        /// 匹配模式设置
        /// </summary>
        public string MatchPattern
        {
            get { return _matchPattern; }
            set { _matchPattern = value; }
        }

        private RegexOptions _regOptions = RegexOptions.None;
        /// <summary>
        /// 匹配选项
        /// </summary>
        public RegexOptions MatchOptions
        {
            get { return _regOptions; }
            set { _regOptions = value; }
        }

        /// <summary>
        /// 其他数据存储
        /// </summary>
        public ArrayList DataStorage = new ArrayList();

        /// <summary>
        /// 获取替换的处理委托
        /// </summary>
        /// <param name="m">匹配项</param>
        /// <param name="cmp">当前配置实例</param>
        /// <returns>自定义替换后的结果</returns>
        public delegate string ReplaceMatchedHandler(Match m, CustomMatchReplace cmp);
    }

    /// <summary>
    /// 运行时消息
    /// </summary>
    public class RuntimeMessage
    {
        /// <summary>
        /// 初始化一个运行时消息
        /// </summary>
        public RuntimeMessage()
        {
            this.m_Succeed = false;
            this.m_Message = "";
            this.m_Code = -1000;
            this.m_Data = null;
        }

        /// <summary>
        /// 初始化一个运行时消息
        /// </summary>
        /// <param name="IsSucceed">是否运行成功</param>
        public RuntimeMessage(bool IsSucceed)
        {
            this.m_Succeed = IsSucceed;
        }

        /// <summary>
        /// 初始化一个运行时消息
        /// </summary>
        /// <param name="IsSucceed">是否运行成功</param>
        /// <param name="Message">消息</param>
        public RuntimeMessage(bool IsSucceed, string Message)
        {
            this.m_Succeed = IsSucceed;
            this.m_Message = Message;
        }

        /// <summary>
        /// 初始化一个运行时消息
        /// </summary>
        /// <param name="IsSucceed">是否运行成功</param>
        /// <param name="Message">消息</param>
        /// <param name="Code">编码/编号</param>
        public RuntimeMessage(bool IsSucceed, string Message, int Code)
        {
            this.m_Succeed = IsSucceed;
            this.m_Message = Message;
            this.m_Code = Code;
        }

        /// <summary>
        /// 初始化一个运行时消息
        /// </summary>
        /// <param name="IsSucceed">是否运行成功</param>
        /// <param name="Message">消息</param>
        /// <param name="Code">编码/编号</param>
        /// <param name="Data">关联对象，通常是Exception对象。</param>
        public RuntimeMessage(bool IsSucceed, string Message, int Code, object Data)
        {
            this.m_Succeed = IsSucceed;
            this.m_Message = Message;
            this.m_Code = Code;
            this.m_Data = Data;
        }

        /// <summary>
        /// 初始化一个运行时消息
        /// </summary>
        /// <param name="IsSucceed">是否运行成功</param>
        /// <param name="Message">消息</param>
        /// <param name="Code">编码/编号</param>
        /// <param name="Data">关联对象，通常是Exception对象。</param>
        /// <param name="Datas">自定义对象数据集，备用。</param>
        public RuntimeMessage(bool IsSucceed, string Message, int Code, object Data, object[] Datas)
        {
            this.m_Succeed = IsSucceed;
            this.m_Message = Message;
            this.m_Code = Code;
            this.m_Data = Data;
            this.m_Datas = Datas;
        }
        //
        bool m_Succeed;
        string m_Message;
        int m_Code;
        object m_Data;
        object[] m_Datas;

        /// <summary>
        /// 是否成功调用
        /// </summary>
        public bool Succeed
        {
            get { return m_Succeed; }
            set { m_Succeed = value; }
        }

        /// <summary>
        /// 自定义消息
        /// </summary>
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        /// <summary>
        /// 自定义代码
        /// </summary>
        public int Code
        {
            get { return m_Code; }
            set { m_Code = value; }
        }

        /// <summary>
        /// 自定义单一数据
        /// </summary>
        public object Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        /// <summary>
        /// 自定义数据集
        /// </summary>
        public object[] Datas
        {
            get { return m_Datas; }
            set { m_Datas = value; }
        }
    }
}
