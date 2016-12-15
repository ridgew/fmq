/***************************
 * $Id: DbHelper.cs 217 2009-07-21 16:42:04Z fanmaquar@staff.vbyte.com $
 * $Author: fanmaquar@staff.vbyte.com $
 * $Rev: 217 $
 * $Date: 2009-07-22 00:42:04 +0800 (星期三, 22 七月 2009) $
 * ***************************/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace Webot.Common
{

    /// <summary>
    /// 当次数据库请求的模块封装
    /// </summary>
    public class FanmaquerOleDbModule : IHttpModule
    {
        private static readonly string clientsCacheKey = "oledbclients-in-httpcontext";

        #region IHttpModule Members

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
            
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
            context.EndRequest += new EventHandler(context_EndRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            context.Items["ProcessBegin"] = System.DateTime.Now.Ticks;

            //if (context.IsDebuggingEnabled)
            //{
            //    Util.Log2File("~/debug.log",
            //        string.Format("开始处理URL地址：{0} {1}\r\nIP:{2}\r\n",
            //        context.Request.RequestType,
            //        context.Request.RawUrl,
            //        context.Request.UserHostAddress
            //     ));
            //}

        }

        /// <summary>
        /// Handles the EndRequest event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void context_EndRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            Dictionary<string, OleDbHelper> clients = context.Items[clientsCacheKey] as Dictionary<string, OleDbHelper>;

            if (clients != null)
            {
                foreach (OleDbHelper client in clients.Values)
                {
                    client.Release();
                }
                clients.Clear();
            }
            context.Items[clientsCacheKey] = null;

            if (context.IsDebuggingEnabled)
            {
                Util.Log2File("~/debug.log",
                    string.Format("处理URL地址：{0} {1}\r\nIP:{2} 数据库打开{3}次，关闭{4}次，运行{5}次，耗时{6}毫秒",
                    context.Request.RequestType,
                    context.Request.RawUrl,
                    context.Request.UserHostAddress,
                    context.Items["OleDbOpen"],
                    context.Items["OleDbClose"],
                    context.Items["OleExecCount"],
                    (System.DateTime.Now - System.DateTime.FromBinary(Convert.ToInt64(context.Items["ProcessBegin"]))).TotalMilliseconds
                 ));
            }

        }

        #endregion


        /// <summary>
        /// 对特定数据对象进行操作
        /// </summary>
        public delegate object OperatorData<TData>(TData dat);

        /// <summary>
        /// Adds the process count.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="contextKey">The context key.</param>
        /// <param name="initialDat">The initial dat.</param>
        /// <param name="operate">The operate.</param>
        public static void AddProcessCount<TData>(string contextKey, TData initialDat, OperatorData<TData> operate)
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
            {
                if (context.Items[contextKey] == null)
                {
                    context.Items[contextKey] = initialDat;
                }
                else
                {
                    context.Items[contextKey] = operate((TData)context.Items[contextKey]);
                }
            }
        }

        /// <summary>
        /// Gets the OLE db instance.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <returns></returns>
        public static OleDbHelper GetOleDbInstance(string connectionName)
        {
            HttpContext context = HttpContext.Current;
            Dictionary<string, OleDbHelper> clients = context.Items[clientsCacheKey] as Dictionary<string, OleDbHelper>;
            if (clients == null)
            {
                //Util.Log2File("~/debug.log", "Null context："
                //    + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")
                //    + Environment.NewLine);

                clients = new Dictionary<string, OleDbHelper>(StringComparer.InvariantCultureIgnoreCase);
                context.Items[clientsCacheKey] = clients;
            }

            if (!clients.ContainsKey(connectionName))
            {
                //Util.Log2File("~/debug.log", "不存在键值："
                //    + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")
                //    + Environment.NewLine);

                if (WebConfigurationManager.ConnectionStrings[connectionName] == null || string.IsNullOrEmpty(WebConfigurationManager.ConnectionStrings[connectionName].ConnectionString))
                {
                    throw new NullReferenceException("配置文件‘web.config'中没有配置键名为’" + connectionName + "‘的数据库连接字符串。");
                }
                else
                {
                    clients.Add(connectionName, 
                        new OleDbHelper(WebConfigurationManager.ConnectionStrings[connectionName].ConnectionString, true));
                }
                
            }
            return (OleDbHelper)clients[connectionName];
        }
    }

    /// <summary>
    /// OleDBHelper 数据。
    /// </summary>
    public class OleDbHelper
    {
        /// <summary>
        /// 数据库方言
        /// </summary>
        public enum DbDialect
        {
            /// <summary>
            /// Access数据库Sql
            /// </summary>
            MsAccess,
            /// <summary>
            /// SqlServer数据库Sql
            /// </summary>
            MsSqlServer,
            /// <summary>
            /// Oracle数据库Sql
            /// </summary>
            Oracle
        }

        /// <summary>
        /// 获取内置缓存中的实例对象
        /// </summary>
        /// <returns>相关的OleDbHelper对象实例</returns>
        public static OleDbHelper GetInstance(string appKey)
        {
            return FanmaquerOleDbModule.GetOleDbInstance(appKey);
        }

        #region 配置辅助
        /// <summary>
        /// 基于web.config模型的AppSettings设置
        /// </summary>
        /// <param name="configPath">配置文件路径，相对或完整路径。</param>
        /// <param name="key">健</param>
        /// <param name="Value">健的值</param>
        /// <returns>成功则为0，失败则返回异常信息。</returns>
        public static string SetAppSettings(string configPath, string key, string Value)
        {
            string configFile = Util.ParseAppPath(configPath);
            try
            {
                System.Configuration.ConfigXmlDocument xmlConfig = new System.Configuration.ConfigXmlDocument();
                xmlConfig.Load(configFile);

                System.Xml.XmlNode node = xmlConfig.SelectSingleNode("configuration/appSettings/add[@key='" + key + "']");
                if (node != null)
                {
                    node.Attributes["value"].Value = Value;
                }
                else
                {
                    XmlElement element = xmlConfig.CreateElement("add");
                    element.SetAttribute("key", key);
                    element.SetAttribute("value", Value);
                    node = xmlConfig.SelectSingleNode("configuration/appSettings");
                    node.AppendChild(element);
                }
                xmlConfig.Save(configFile);
                return "0";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取基于web.config模型的AppSettings键值设置
        /// </summary>
        /// <param name="configPath">配置文件路径，相对或完整路径。</param>
        /// <param name="key">健</param>
        /// <returns>存在则返回相应值，异常出错则为空字符。</returns>
        public static string GetAppSettings(string configPath, string key)
        {
            string configFile = Util.ParseAppPath(configPath);
            string strRet = "";
            try
            {
                System.Configuration.ConfigXmlDocument xmlConfig = new System.Configuration.ConfigXmlDocument();
                xmlConfig.Load(configFile);
                System.Xml.XmlNode node = xmlConfig.SelectSingleNode("configuration/appSettings/add[@key='" + key + "']");
                if (node != null)
                {
                    strRet = node.Attributes["value"].Value;
                }
            }
            catch (Exception) { }
            return strRet;
        }

        /// <summary>
        /// 设置文本文件的内容
        /// </summary>
        /// <param name="filePath">文件的相对路径，如"/index.html"等。</param>
        /// <param name="charset">文件编码，如"gb2312","utf-8"等</param>
        /// <param name="TxtContent">文本文件的内容</param>
        /// <returns>成功则返回字符"0",错误则放回失败的原因。</returns>
        public static string SetTextFileContent(string filePath, string charset, string TxtContent)
        {
            string strResult = "0";
            try
            {
                string LocalFilePath = Util.ParseAppPath(filePath);
                string fileDir = Path.GetDirectoryName(LocalFilePath);
                if (!Directory.Exists(fileDir)) Directory.CreateDirectory(fileDir);

                using (StreamWriter sw = new StreamWriter(LocalFilePath, false, System.Text.Encoding.GetEncoding(charset)))
                {
                    sw.Write(TxtContent);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception exp)
            {
                strResult = exp.Message;
            }
            return strResult;
        }

        /// <summary>
        /// 附加文本内容到文本文件
        /// </summary>
        public static void AppendToFile(string filePath, string TxtContent)
        {
            lock (filePath)
            {
                StreamWriter sw = new StreamWriter(Util.ParseAppPath(filePath), true, System.Text.Encoding.Default);
                sw.Write(TxtContent);
                sw.Flush();
                sw.Close();
            }
        }

        /// <summary>
        /// 附加文本内容到文本文件
        /// </summary>
        public static void AppendToFile(bool IsAppend, string filePath, string TxtContent)
        {
            lock (filePath)
            {
                StreamWriter sw = new StreamWriter(Util.ParseAppPath(filePath), IsAppend, System.Text.Encoding.Default);
                sw.Write(TxtContent);
                sw.Flush();
                sw.Close();
            }
        }

        /// <summary>
        /// 从文本文件模板加载文本内容
        /// </summary>
        /// <param name="strFilePath">文件保存路径，相对路径或物理路径。</param>
        /// <param name="charset">文件编码</param>
        /// <returns>文本文件中的内容</returns>
        public static string GetTextFileContent(string strFilePath, string charset)
        {
            String TempletPath = Util.ParseAppPath(strFilePath);
            if (!File.Exists(TempletPath)) { return "指定文件不存在！";  }
            FileStream fs = new FileStream(TempletPath, FileMode.Open, FileAccess.Read);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            Encoding Charset = Encoding.GetEncoding(charset);
            return Charset.GetString(bytes);
        }

        #endregion

        #region 数据库操作辅助

        /// <summary>
        /// 创建常用的Insert和Update操作的SQL语句
        /// </summary>
        /// <param name="dialect">Sql方言</param>
        /// <param name="TableName">表名、对象名</param>
        /// <param name="paramOperation">操作描述，Insert一般参数为"I"，Update需紧跟匹配参数形如：U@[ID=8888]。</param>
        /// <param name="Columns">列集合</param>
        /// <param name="objColVal">列值集合</param>
        /// <returns>相关SQL语句</returns>
        /// <example>
        /// string sqlInsert = BuildSqlIU("user","I",new string[]{"username","password"},new object[]{"test","123go"});
        /// RETURNS: sqlInsert = "insert into [user](username,password) values('test','123go')";
        /// 
        /// string sqlUpdate = BuildSqlIU("user","U@[ID=2]",new string[]{"username","password"},new object[]{"test","123go"});
        /// RETURNS: sqlUpdate = "update [user] set username='test',password='123go' where ID=2";
        /// </example>
        public static string BuildSqlIU(DbDialect dialect, string TableName, string paramOperation, string[] Columns, object[] objColVal)
        {
            if (objColVal.Length != Columns.Length)
            {
                throw new Exception("列参数集合和列赋值长度不一致！");
            }
            string tptInsert = "insert into [{0}]({1}) values({2})";
            string tptUpdate = "update [{0}] set {1} {2}";
            string uPattern = "^(U|UPDATE)@\\[([^\\]]*)\\]$";
            string[] objSqlVal = GetSqlItemValue(dialect, objColVal);

            string sqlResult = string.Empty;
            Match m = Regex.Match(paramOperation, uPattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                // update
                string strWhere = m.Groups[2].Value;
                // the sql Where Condition
                if (strWhere.Length > 0) { strWhere = "where " + strWhere; }
                // Fields Values
                for (int i = 0; i < Columns.Length; i++)
                {
                    Columns[i] = String.Concat(Columns[i], "=", objSqlVal[i]);
                }
                sqlResult = String.Format(tptUpdate, TableName,
                    String.Join(", ", Columns), strWhere);
            }
            else
            {
                // insert
                sqlResult = String.Format(tptInsert, TableName,
                    String.Join(", ", Columns), String.Join(", ", objSqlVal));
            }
            return sqlResult;
        }

        /// <summary>
        /// 获取特定值在Sql语句中的文本表现形式
        /// </summary>
        /// <param name="dialect">Sql方言</param>
        /// <param name="objColVal">相关值集合</param>
        /// <returns>相关SQL列值的集合</returns>
        public static string[] GetSqlItemValue(DbDialect dialect, object[] objColVal)
        {
            string[] sqlVal = new string[objColVal.Length];
            string colValue = string.Empty;

            for (int i = 0; i < sqlVal.Length; i++)
            {
                if (objColVal[i].GetType() == typeof(System.String))
                {
                    colValue = objColVal[i].ToString();
                    // 字符
                    // 自定义函数、查询，则直接赋值。 [N'getdate(),N'(select @@IDENTIDY)]
                    if (colValue.StartsWith("N'"))
                    {
                        sqlVal[i] = colValue.Substring(2);
                    }
                    else
                    {
                        sqlVal[i] = String.Concat("'", colValue.Replace("'", "''"), "'");
                    }
                }
                else if (objColVal[i].GetType() == typeof(System.Guid))
                {
                    sqlVal[i] = String.Concat("'", objColVal[i], "'");
                }
                else if (objColVal[i].GetType() == typeof(System.Boolean))
                {
                    // 布尔值
                    if (dialect == DbDialect.MsSqlServer)
                    {
                        sqlVal[i] = Convert.ToBoolean(objColVal[i]) ? "1" : "0";
                    }
                    else if (dialect == DbDialect.MsAccess)
                    {
                        sqlVal[i] = Convert.ToBoolean(objColVal[i]) ? "True" : "False";
                    }
                    else
                    {
                        sqlVal[i] = objColVal[i].ToString();
                    }
                }
                else if (objColVal[i].GetType() == typeof(System.DateTime))
                {
                    // 时间格式
                    if (dialect == DbDialect.MsSqlServer)
                    {
                        sqlVal[i] = String.Concat("'", objColVal[i], "'");
                    }
                    else if (dialect == DbDialect.MsAccess)
                    {
                        sqlVal[i] = String.Concat("#", objColVal[i], "#");
                    }
                    else
                    {
                        sqlVal[i] = objColVal[i].ToString();
                    }
                }
                else
                {
                    sqlVal[i] = objColVal[i].ToString();
                }
            }
            return sqlVal;
        }

        /// <summary>
        /// 创建常用的Insert和Update操作的SQL语句
        /// </summary>
        /// <param name="dialect">Sql方言</param>
        /// <param name="TableName">表名、对象名</param>
        /// <param name="paramOperation">操作描述，Insert一般参数为"I"，Update需紧跟匹配参数形如：U@[ID=8888]。</param>
        /// <param name="Columns">列集合</param>
        /// <param name="dRow">填充数据列的相应数据行</param>
        /// <returns>相关SQL语句</returns>
        public static string BuildSqlIU(DbDialect dialect, string TableName, string paramOperation, string[] Columns, DataRow dRow)
        {
            return BuildSqlIU(dialect, TableName, paramOperation, Columns, dRow.ItemArray);
        }

        /// <summary>
        /// 获取数据表格的第1列数据集合
        /// </summary>
        /// <param name="dTab">数据表格</param>
        /// <param name="colIdx">列索引</param>
        /// <param name="strSperator">列分隔符号</param>
        /// <returns>返回以<c>strSperator</c>分隔的第1列所有值的文本形式</returns>
        public static string GetColumnCollectionString(DataTable dTab, int colIdx, string strSperator)
        {
            if (dTab != null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dTab.Rows.Count; i++)
                {
                    sb.Append(strSperator + dTab.Rows[i][colIdx].ToString());
                }
                return sb.ToString().Substring(strSperator.Length);
            }
            else
            {
                return "NULL";
            }
        }

        /// <summary>
        /// 按顺序建立查询条件、查询参数
        /// </summary>
        /// <param name="strJoin">有效参数之间的连接字符</param>
        /// <param name="objQuery">获取的查询参数</param>
        /// <param name="objQueryFormat">参数和欲获取值结合的格式化字符串</param>
        /// <param name="validatePattern">参数合法性验证匹配模式</param>
        /// <returns>查询语句条件</returns>
        /// <example>
        /// <code>
        ///	string strJoinQuery = BuildJoinQuery(" and ",new object[]{Request.QueryString["tid"],Request.QueryString["pid"], Request.QueryString["mid"], Request.QueryString["BrandID"]}, 
        ///   new string[]{ "gtype={0}", "uploader={0}", "charindex(',{0},',','+gmobile+',',0)>0", "charindex(',{0},',','+gcompany+',',0)>0"},
        ///   new string[] { "^\\d+$", "^\\d+$", "^\\d+$", "^\\d+$"});
        /// </code></example>
        public static string BuildJoinQuery(string strJoin, object[] objQuery, string[] objQueryFormat, string[] validatePattern)
        {
            string strJoinQuery = string.Empty;
            bool hasQuery = false, blnDoValidate = false;
            int QueryCount = objQuery.Length;

            // 数组长度一致
            if (objQuery.Length != objQueryFormat.Length) { return string.Empty; }
            // 检查验证
            if (validatePattern != null && validatePattern.Length == objQuery.Length)
            {
                blnDoValidate = true;
            }

            string[] keyArray = new string[objQuery.Length];
            for (int i = 0; i < keyArray.Length; i++)
            {
                keyArray[i] = (i != 0) ? "2=2" : "1=1";
                if (objQuery[i] != null)
                {
                    hasQuery = true;
                    if (validatePattern != null)
                    {
                        if (blnDoValidate && System.Text.RegularExpressions.Regex.IsMatch(objQuery[i].ToString(), validatePattern[i], System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            keyArray[i] = String.Format(objQueryFormat[i], objQuery[i].ToString());
                        }
                    }
                    else
                    {
                        keyArray[i] = String.Format(objQueryFormat[i], objQuery[i].ToString());
                    }
                }
            }
            if (hasQuery == true)
            {
                strJoinQuery = String.Join(strJoin, keyArray).Replace("1=1" + strJoin, "");
                strJoinQuery = strJoinQuery.Replace(strJoin + "2=2", "");
                strJoinQuery = strJoinQuery.Replace("2=2" + strJoin, "");
            }
            return strJoinQuery;
        }

        /// <summary>
        /// 按顺序建立查询条件，配合存储过程sp_CacheCount使用。
        /// </summary>
        /// <param name="objQuery">获取的查询参数</param>
        /// <param name="objQueryFormat">参数和数据库结合的格式化字符串</param>
        /// <param name="validatePattern">参数合法性验证匹配模式</param>
        /// <returns>查询语句条件</returns>
        public static string BuildSqlWhere(object[] objQuery, string[] objQueryFormat, string[] validatePattern)
        {
            return BuildJoinQuery(" and ", objQuery, objQueryFormat, validatePattern);
        }

        /// <summary>
        /// 按顺序建立查询条件，配合存储过程sp_CacheCount使用。
        /// </summary>
        /// <param name="objQuery">获取的查询参数</param>
        /// <param name="objQueryFormat">参数和数据库结合的格式化字符串</param>
        /// <returns>查询语句条件</returns>
        public static string BuildSqlWhere(object[] objQuery, string[] objQueryFormat)
        {
            return BuildJoinQuery(" and ", objQuery, objQueryFormat, null);
        }

        /// <summary>
        /// 创建方便Sql语句查询的Sql查询片段
        /// </summary>
        /// <param name="repStrs">字符内的分隔字符</param>
        /// <param name="strReplaceObject">待处理的字符</param>
        /// <param name="strPrefix">组合片段前缀</param>
        /// <param name="strAppend">组合片段后缀</param>
        /// <param name="strJoin">组合片段中间连接字符</param>
        /// <returns>相应条件Sql查询片段</returns>
        /// <example>
        /// String Keywords = "坏企鹅|极地运动家";
        /// BuildSqlReplace(new string[]{"|",","},Keywords,"'%","%'"," or content like ") 将返回为以下字符：&#13;&#10;
        /// [Return]: '%坏企鹅%' or content like '%极地运动家%' 
        /// </example>
        public static string BuildSqlReplace(string[] repStrs, string strReplaceObject, string strPrefix, string strAppend, string strJoin)
        {
            string sqlReturn = string.Empty;
            if (strReplaceObject != null)
            {
                for (int i = 0; i < repStrs.Length; i++)
                {
                    strReplaceObject = strReplaceObject.Replace(repStrs[i], repStrs[0]);
                }
                sqlReturn = strPrefix + strReplaceObject.Replace(repStrs[0], strAppend + strJoin + strPrefix)
                    + strAppend; ;
            }
            return sqlReturn;
        }

        /// <summary>
        /// 转义SQL语句中的字符
        /// </summary>
        /// <param name="sql">sql字符输入</param>
        /// <param name="unsafeChars">可选过滤掉的危险字符集合</param>
        /// <returns>简易替换后的sql输出</returns>
        public static string EscapeSQL(string sql, params string[] unsafeChars)
        {
            string validSql = "";
            if (sql != null)
            {
                //sql = sql.Replace("%20", "&#32");
                //sql = sql.Replace("--", "&#45;&#45;");
                //sql = sql.Replace("'", "''");
                if (unsafeChars != null && unsafeChars.Length > 0)
                {
                    foreach (string unsafeStr in unsafeChars)
                    {
                        sql = sql.Replace(unsafeStr, "&#" + ((int)unsafeStr[0]).ToString() + ";"
                            + (unsafeStr.Length > 1 ? unsafeStr.Substring(1) : ""));
                    }
                }
                validSql = sql;
            }
            return validSql;
        }
        #endregion

        private OleDbConnection conn = null;
        private string message;

        /// <summary>
        /// 数据连结字符串
        /// </summary>
        private string strConn;

        /// <summary>
        /// OleDb数据库连接辅助对象
        /// </summary>
        /// <param name="AccessPath">数据库路径</param>
        public OleDbHelper(string AccessPath)
        {
            strConn = "Provider=Microsoft.Jet.OLEDB.4.0; " +
                "Data Source=" + Util.ParseAppPath(AccessPath) + ";";
            conn = new OleDbConnection(strConn);
        }

        /// <summary>
        /// OleDb数据库连接辅助对象
        /// </summary>
        /// <param name="AccessPath">数据库路径</param>
        /// <param name="Password">数据库密码</param>
        public OleDbHelper(string AccessPath, string Password)
        {
            strConn = "Provider=Microsoft.Jet.OLEDB.4.0; " +
                "Data Source=" + Util.ParseAppPath(AccessPath)
                + ";Jet OLEDB:DataBase Password="
                + Password + ";";
            conn = new OleDbConnection(strConn);
        }

        /// <summary>
        /// OleDb数据库连接辅助对象
        /// </summary>
        /// <param name="OleDbConnectionStr">OleDb数据库连接字符串</param>
        /// <param name="IsSelfConnectionStr">是自定义的数据库连接字符串</param>
        public OleDbHelper(string OleDbConnectionStr, bool IsSelfConnectionStr)
        {
            if (IsSelfConnectionStr == true)
            {
                //strConn = OleDbConnectionStr;
                //asp.net 2.0 内置支持 |DataDirectory| 指定App_Data目录
                strConn = ReplaceDataDirectory(OleDbConnectionStr);
                conn = new OleDbConnection(OleDbConnectionStr);
            }
        }

        /// <summary>
        /// 助手产生的OleDb连接对象
        /// </summary>
        public OleDbConnection Connection
        {
            get { return this.conn; }
        }

        /// <summary>
        /// 执行错误异常
        /// </summary>
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (message != value && message.GetHashCode() != value.GetHashCode())
                {
                    Util.Log2File("../Logs/DbError-" + System.DateTime.Now.ToString("yyyyMMdd") + ".log", message);
                }
                message = value;
            }
        }

        private static string ReplaceDataDirectory(string connStr)
        {
            //|DataDirectory|aspnetdb.mdf
            if (connStr.IndexOf("|DataDirectory|", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                connStr = Regex.Replace(connStr, @"\|DataDirectory\|", HttpContext.Current.Server.MapPath("/App_Data/"),
                    RegexOptions.IgnoreCase);
            }
            return connStr;
        }

        /// <summary>
        /// 读取Web.config的AppSettings键值
        /// </summary>
        /// <param name="key">AppSettings键</param>
        /// <returns>AppSettings键值</returns>
        public static string GetAppConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        ///// <summary>
        ///// 解析文件路径
        ///// </summary>
        ///// <param name="strFilePath">文件路径，可以包含物理路径。</param>
        ///// <returns>相应路径的物理路径</returns>
        //public static string ParseAppPath(string strFilePath)
        //{
        //    if (HttpContext.Current != null)
        //    {
        //        if (strFilePath.StartsWith("~"))
        //        {
        //            strFilePath = HttpContext.Current.Request.PhysicalApplicationPath + strFilePath.Substring(1).Replace("/", "\\");
        //            strFilePath = strFilePath.Replace("\\\\", "\\");
        //        }
        //        else
        //        {
        //            if (strFilePath.IndexOf(":\\") == -1)
        //            {
        //                strFilePath = HttpContext.Current.Server.MapPath(strFilePath);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strFilePath);
        //    }
        //    return strFilePath;
        //}

        #region 获取数据
        /// <summary>
        /// 获取一个分页数据表格
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="CurrentPage">当前页面</param>
        /// <param name="PageSize">页次</param>
        /// <returns>数据表格</returns>
        public DataTable GetDataTable(string sql, int CurrentPage, int PageSize)
        {
            ensureDbOpen();
            DataSet ds = new DataSet();
            try
            {
                using (OleDbDataAdapter adp = new OleDbDataAdapter(sql, conn))
                {
                    adp.Fill(ds, (CurrentPage - 1) * PageSize, PageSize, "default");
                }
                return ds.Tables[0];
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 获取一个内存数据库(对不支持多条语句一起操作的数据库有效)
        /// </summary>
        /// <param name="sqlSelectFirst">至少指定一句查询</param>
        /// <param name="sqlSelectLists">其他Sql提取语句</param>
        /// <returns>内存数据库</returns>
        public DataSet GetDataSet(string sqlSelectFirst, params string[] sqlSelectLists)
        {
            ensureDbOpen();
            DataSet ds = new DataSet(); DataTable dTab = new DataTable();
            try
            {
                using (OleDbDataAdapter adp = new OleDbDataAdapter(sqlSelectFirst, conn))
                {
                    adp.Fill(dTab);
                    ds.Tables.Add(dTab);

                    if (sqlSelectLists != null)
                    {
                        for (int i = 0; i < sqlSelectLists.Length; i++)
                        {
                            adp.SelectCommand = new OleDbCommand(sqlSelectLists[i], conn);
                            dTab = new DataTable();
                            adp.Fill(dTab);
                            ds.Tables.Add(dTab);
                        }
                    }
                }
                return ds;
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 获取一个数据表格
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>数据表格</returns>
        public DataTable GetDataTable(string sql)
        {
            ensureDbOpen();
            DataTable dTab = new DataTable();
            try
            {
                using (OleDbDataAdapter adp = new OleDbDataAdapter(sql, conn))
                {
                    adp.Fill(dTab);
                }
            }
            catch (Exception e)
            {
                this.message = e.Message;
            }
            return dTab;
        }

        /// <summary>
        /// 获取一个表格行，并提示是否关闭数据库
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="closeConnection">是否关闭数据库连接</param>
        /// <returns>表格行</returns>
        public DataRow GetDataRow(string sql, bool closeConnection)
        {
            ensureDbOpen();
            using (DataTable dTab = GetDataTable(sql))
            {
                if (closeConnection) { this.Release(); }
                if (dTab == null)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        return dTab.Rows[0];
                    }
                    catch (Exception e)
                    {
                        this.message = e.Message;
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// 获取一个表格行
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public DataRow GetDataRow(string sql)
        {
            return this.GetDataRow(sql, false);
        }
        #endregion
        
        /// <summary>
        /// 返回执行sql语句后第1行1列
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>第1行1列的数据</returns>
        public object ExecuteScalar(string sql)
        {
            ensureDbOpen();
            try
            {
                using (OleDbCommand Cmd = new OleDbCommand(sql, conn))
                {
                    return Cmd.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return -1;
            }
        }

        /// <summary>
        /// 运行Sql语句之后放回该区域的最新主键值
        /// </summary>
        /// <param name="sql">Sql语句（一般为insert语句）</param>
        /// <param name="getScopeIdentity">是否返回局部主键值</param>
        /// <returns>如果获取局部主键则返回，否则返回受影响的数据条数，类似于<c>ExecuteNonQuery</c>。</returns>
        public object ExecuteScalar(string sql, bool getScopeIdentity)
        {
            ensureDbOpen();
            try
            {
                using (OleDbCommand Cmd = new OleDbCommand(sql, conn))
                {
                    if (getScopeIdentity == true)
                    {
                        Cmd.ExecuteNonQuery();
                        Cmd.CommandText = "SELECT @@IDENTITY ";
                        return Cmd.ExecuteScalar();
                    }
                    else
                    {
                        return Cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return -1;
            }
        }

        /// <summary>
        /// 执行Sql语句，并返回受影响的行数
        /// </summary>
        /// <param name="sql">Sql语句</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteNonQuery(string sql)
        {
            ensureDbOpen();
            try
            {
                using (OleDbCommand Cmd = new OleDbCommand(sql, conn))
                {
                    return Cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return 0;
            }
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="cmdParams">The CMD params.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, params OleDbParameter[] cmdParams)
        {
            ensureDbOpen();
            using (OleDbCommand cmd = new OleDbCommand())
            {
                try
                {
                    PrepareCommand(cmd, conn, null, sql, cmdParams);
                    int rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
                catch (System.Data.OleDb.OleDbException e)
                {
                    this.message = e.Message;
                    return -1;
                }
            }
        }

        /// <summary>
        /// 迅速地指定添加与更新的操作并运行。
        /// </summary>
        /// <param name="TableName">表名、对象名</param>
        /// <param name="paramOperation">操作描述，Insert一般参数为"I"，Update需紧跟匹配参数形如：U@[ID=8888]。</param>
        /// <param name="columnNames">列名集合</param>
        /// <param name="paramValues">OleDb参数的值集合</param>
        /// <returns>返回受影响的数据行数</returns>
        public int ExecuteWithParameters(string TableName, string paramOperation, string[] columnNames, object[] paramValues)
        {
            if (paramValues.Length != columnNames.Length)
            {
                throw new Exception("列参数集合和列赋值长度不一致！");
            }
            string tptInsert = "insert into [{0}]({1}) values({2})";
            string tptUpdate = "update [{0}] set {1} {2}";
            string uPattern = "^(U|UPDATE)@\\[([^\\]]*)\\]$";

            string sqlResult = string.Empty;
            Match m = Regex.Match(paramOperation, uPattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                // update
                string strWhere = m.Groups[2].Value;
                // the sql Where Condition
                if (strWhere.Length > 0) { strWhere = "where " + strWhere; }
                // Fields Values
                for (int i = 0; i < columnNames.Length; i++)
                {
                    columnNames[i] = String.Concat(columnNames[i], " = @", columnNames[i]);
                }
                sqlResult = String.Format(tptUpdate, TableName,
                    String.Join(", ", columnNames), strWhere);
            }
            else
            {
                // insert
                sqlResult = String.Format(tptInsert, TableName,
                    String.Join(", ", columnNames), "@" + String.Join(", @", columnNames));
            }

            OleDbParameter[] dbParams = new OleDbParameter[paramValues.Length];
            for (int i = 0; i < paramValues.Length; i++)
            {
                dbParams[i] = new OleDbParameter("@" + columnNames[i], paramValues[i]);
            }

            ensureDbOpen(); 
            using (OleDbCommand cmd = new OleDbCommand(sqlResult, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddRange(dbParams);

                int n = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return n;
            }
        }

        private void PrepareCommand(OleDbCommand cmd, OleDbConnection conn, OleDbTransaction trans, string cmdText, OleDbParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = CommandType.Text; //cmdType;
            if (cmdParms != null)
            {
                foreach (OleDbParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        /// <summary>
        /// Executes the SQL transaction.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="otherSql">The other SQL.</param>
        public void ExecSqlTransaction(string sql, params string[] otherSql)
        {
            ensureDbOpen();

            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;
            OleDbTransaction tx = conn.BeginTransaction();
            cmd.Transaction = tx;
            try
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                for (int i = 0; i < otherSql.Length; i++ )
                {
                    sql = otherSql[i];
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch (System.Data.OleDb.OleDbException e)
            {
                tx.Rollback();
                throw e;
            }
            finally
            {
                tx.Dispose();
                cmd.Dispose();
            }
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        private void ensureDbOpen()
        {
            try
            {
                bool blnOpend = false;
                if (conn == null)
                {
                    conn = new OleDbConnection(this.strConn);
                    conn.Open();
                    blnOpend = true;

                    //Util.Log2File("~/debug.log", "为空状态，打开数据库连接时间："
                    //    + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")
                    //    + Environment.NewLine);
                }
                else
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.ConnectionString = this.strConn;
                        conn.Open();
                        blnOpend = true;

                        //Util.Log2File("~/debug.log", "关闭状态，打开数据库连接时间："
                        //    + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")
                        //    + Environment.NewLine);
                    }
                }

                //Util.Log2File("~/debug.log", "数据库状态" + conn.State + Environment.NewLine);

                if (blnOpend == true)
                {
                    //添加打开数据库次数
                    FanmaquerOleDbModule.AddProcessCount<int>("OleDbOpen", 1, new FanmaquerOleDbModule.OperatorData<int>(delegate(int count)
                    {
                        return count + 1;
                    }));
                }

                //确保打开数据库次数，操作数
                FanmaquerOleDbModule.AddProcessCount<int>("OleExecCount", 1, new FanmaquerOleDbModule.OperatorData<int>(delegate(int count)
                {
                    return count + 1;
                }));

            }
            catch (Exception e)
            {
                this.message = e.Message;
            }
        }

        /// <summary>
        /// 释放连接对象
        /// </summary>
        public void Release()
        {
            if (conn != null && conn.State == ConnectionState.Open)
            {
                conn.Close();
                conn.Dispose();

                //Util.Log2File("~/debug.log", "销毁数据库时间：" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")
                //    + Environment.NewLine);

                //添加关闭销毁数据库连接对象次数
                FanmaquerOleDbModule.AddProcessCount<int>("OleDbClose", 1, new FanmaquerOleDbModule.OperatorData<int>(delegate(int count)
                {
                    return count + 1;
                }));
            }
        }
    }
}
