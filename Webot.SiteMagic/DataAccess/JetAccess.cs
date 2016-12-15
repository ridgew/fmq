using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.OleDb;
using System.Collections;

namespace Webot.DataAccess
{
    /// <summary>
    /// Access数据库操作辅助
    /// </summary>
    public static class JetAccess
    {
        //Jet OLEDB:Engine Type Jet x.x Format MDB Files 
        //1 JET10 
        //2 JET11 
        //3 JET2X 
        //4 JET3X 
        //5 JET4X 
        
        /// <summary>
        /// 新建带密码的空Access 2000 数据库
        /// </summary>
        /// <param name="mdbFilePath">数据库文件路径</param>
        /// <param name="password">数据库密码</param>
        /// <returns>字符0为操作成功，否则为失败异常消息。</returns>
        public static string CreateMDB(string mdbFilePath, string password)
        {
            try
            {
                string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;";
                if (password == null || password.Trim() == "")
                {
                    connStr += "Data Source=" + mdbFilePath;
                }
                else
                {
                    connStr += "Jet OLEDB:Database Password=" + password + ";Data Source=" + mdbFilePath;
                }
                object objCatalog = Activator.CreateInstance(Type.GetTypeFromProgID("ADOX.Catalog"));
                object[] oParams = new object[] { connStr };
                objCatalog.GetType().InvokeMember("Create", BindingFlags.InvokeMethod, null, objCatalog, oParams);
                Marshal.ReleaseComObject(objCatalog);
                objCatalog = null;
                return "0";
            }
            catch (Exception exp)
            {
                return exp.Message;
            }
        }

        /// <summary>
        /// 新建空的Access数据库
        /// </summary>
        /// <param name="mdbFilePath">数据库文件路径</param>
        /// <returns>字符0为操作成功，否则为失败异常消息。</returns>
        public static string CreateMDB(string mdbFilePath)
        {
            return CreateMDB(mdbFilePath, null);
        }

        /// <summary>
        /// 压缩带密码Access数据库
        /// </summary>
        /// <param name="mdbFilePath">数据库文件路径</param>
        /// <param name="password">数据库密码</param>
        /// <returns>字符0为操作成功，否则为失败异常消息。</returns>
        public static string CompactMDB(string mdbFilePath, string password)
        {
            string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Jet OLEDB:Engine Type=5;";
            string connStrTemp = connStr;
            string tmpPath = mdbFilePath + ".tmp";
            if (password == null || password.Trim() == "")
            {
                connStr += "Data Source=" + mdbFilePath;
                connStrTemp += "Data Source=" + tmpPath;
            }
            else
            {
                connStr += "Jet OLEDB:Database Password=" + password + ";Data Source=" + mdbFilePath;
                connStrTemp += "Jet OLEDB:Database Password=" + password + ";Data Source=" + mdbFilePath + ".tmp";
            }

            string strRet = "";
            try
            {
                object objJRO = Activator.CreateInstance(Type.GetTypeFromProgID("JRO.JetEngine"));
                object[] oParams = new object[] { connStr, connStrTemp };
                objJRO.GetType().InvokeMember("CompactDatabase", BindingFlags.InvokeMethod, null, objJRO, oParams);
                Marshal.ReleaseComObject(objJRO);
                objJRO = null;
            }
            catch (Exception exp)
            {
                strRet = exp.Message;
            }

            try
            {
                System.IO.File.Delete(mdbFilePath);
                System.IO.File.Move(tmpPath, mdbFilePath);
            }
            catch (Exception expio)
            {
                strRet += expio.Message;
            }

            return (strRet == "") ? "0" : strRet;

        }

        /// <summary>
        /// 压缩带密码Access数据库
        /// </summary>
        /// <param name="mdbFilePath">数据库文件路径</param>
        /// <returns>字符0为操作成功，否则为失败异常消息。</returns>
        public static string CompactMDB(string mdbFilePath)
        {
            return CompactMDB(mdbFilePath, null);
        }

        /// <summary>
        /// 设置Access数据库的访问密码
        /// </summary>
        /// <param name="mdbFilePath">数据库文件路径</param>
        /// <param name="oldPwd">旧密码</param>
        /// <param name="newPwd">新密码</param>
        /// <returns>字符0为操作成功，否则为失败异常消息。</returns>
        public static string SetMDBPassword(string mdbFilePath, string oldPwd, string newPwd)
        {
            string connStr = string.Concat("Provider=Microsoft.Jet.OLEDB.4.0;",
                "Mode=Share Deny Read|Share Deny Write;", //独占模式
                "Jet OLEDB:Database Password=" + oldPwd + ";Data Source=" + mdbFilePath);

            using (OleDbConnection conn = new OleDbConnection(connStr))
            {
                try
                {
                    conn.Open();
                    //如果密码为空时，请不要写方括号，只写一个null即可
                    string sqlOldPwd = (oldPwd == null || oldPwd.Trim() == "") ? "null" : "[" + oldPwd + "]";
                    string sqlNewPwd = (newPwd == null || newPwd.Trim() == "") ? "null" : "[" + newPwd + "]";
                    OleDbCommand cmd = new OleDbCommand(string.Concat("ALTER DATABASE PASSWORD ", sqlNewPwd, " ", sqlOldPwd),
                           conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    return "0";
                }
                catch (Exception exp)
                {
                    return exp.Message;
                }
            }
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        private static string CapitalUpperCase(string str)
        {
            if (str == null) return string.Empty;
            string[] objWords = str.Split(' ');
            for (int i = 0; i < objWords.Length; i++)
            {
                if (objWords[i] != string.Empty)
                {
                    objWords[i] = objWords[i].ToLower();
                    objWords[i] = objWords[i].Substring(0, 1).ToUpper() + objWords[i].Substring(1);
                }
            }
            return String.Join(" ", objWords);
        }

        /// <summary>
        /// OleDb数据类型和标记到.NET类型的数据映射
        /// </summary>
        /// <param name="DATA_TYPE">数据类型</param>
        /// <param name="COLUMN_FLAGS">标记</param>
        /// <param name="PrimaryKey">是否为主键</param>
        /// <returns>默认放回System.String字符类型。</returns>
        public static string GetOleDbTypeMapping(int DATA_TYPE, int COLUMN_FLAGS, out bool PrimaryKey)
        {
            string[] NetTypeMapping = new string[] { 
                    "System.String", "System.Int32", "System.Int64",  "System.Single", "System.Double",
                    "System.Boolean", "System.Object", "System.Decimal", "System.DateTime", "System.Byte",
                    "System.Guid", "$Binary", "$Url", "$File"
               };

            // ? 测试
            PrimaryKey = (COLUMN_FLAGS == 90) ? true : false;

            int idx = 0;
            switch (DATA_TYPE)
            {
                case 2:
                    idx = 1; break;  // 整型
                case 3:
                    idx = 2;
                    if (COLUMN_FLAGS == 90 || COLUMN_FLAGS == 16) PrimaryKey = true;
                    break;  // 长整型， OK @ Access2k OLE/SQL2k OLE
                case 4:
                    idx = 3; break;  // 单精度型
                case 5:
                    idx = 4; break;  // 双精度型
                case 6:
                    idx = 4; break;  // 货币
                case 7:
                    idx = 8; break;  // 日期时间
                case 11:
                    idx = 5; break;  // 真/假
                case 17:
                    idx = 9; break;  // 字节
                case 20:
                    idx = 2;
                    if (COLUMN_FLAGS == 16) PrimaryKey = true;
                    break;  // 长整型， OK @ SQL2k OLE
                case 72:
                    idx = 10; break;  // 同步复制ID GUID
                case 128:
                    idx = 11; break;  // OLE二进制对象
                default:
                    idx = 0; break;
            }
            return NetTypeMapping[idx];
        }


        /// <summary>
        /// 获取下的特定表的ORM信息
        /// </summary>
        /// <param name="connString">连接字符串</param>
        /// <returns>ORM Tables</returns>
        /// <remarks>
        /// http://support.microsoft.com/kb/310107/zh-CN
        /// http://support.microsoft.com/default.aspx?scid=kb;zh-cn;309488
        /// </remarks>
        public static string GetOleTablesInfo(string connString)
        {
            return GetOleTablesInfo(connString, null);
        }


        /// <summary>
        /// 获取下的特定视图的ORM信息
        /// </summary>
        public static string GetOleViewsInfo(string connString)
        {
            return GetOleTablesInfo(connString, "VIEW");
        }

        /// <summary>
        /// 获取下的特定表的ORM信息
        /// </summary>
        /// <param name="connString">连接字符串</param>
        /// <param name="TABLE_TYPE">表类型，SYSTEM TABLE|TABLE|VIEW。</param>
        /// <returns>ORM Tables</returns>
        /// <remarks>
        /// http://support.microsoft.com/kb/310107/zh-CN
        /// http://support.microsoft.com/default.aspx?scid=kb;zh-cn;309488
        /// </remarks>
        public static string GetOleTablesInfo(string connString, string TABLE_TYPE)
        {
            OleDbConnection conn = new OleDbConnection(connString);
            conn.Open();

            // TABLE_CATALOG = joyes_game2006
            // TABLE_SCHEMA = dbo
            // TABLE_NAME = sysfiles1
            // TABLE_TYPE = SYSTEM TABLE

            DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new Object[] { null, null, null, TABLE_TYPE });

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string strTemp = string.Empty;

            foreach (DataRow myField in schemaTable.Rows)
            {
                strTemp = myField["TABLE_TYPE"].ToString();
                string containerName = "Table";
                if (strTemp != string.Empty)
                {
                    containerName = CapitalUpperCase(strTemp).Replace(" ", "");
                }
                sb.Append("<" + containerName + " Name=\"" + myField["TABLE_NAME"].ToString() + "\"");
                strTemp = myField["DESCRIPTION"].ToString();
                if (strTemp != string.Empty)
                {
                    sb.Append(" description=\"" + strTemp + "\"");
                }

                sb.AppendFormat(" DateCreated=\"{0}\"", myField["DATE_CREATED"]);
                strTemp = myField["DATE_MODIFIED"].ToString();
                if (strTemp != string.Empty)
                {
                    sb.Append(" DateModified=\"" + strTemp + "\">");
                }
                else
                {
                    sb.Append(">");
                }

                sb.Append(System.Environment.NewLine);

                sb.Append(GetOleColumnsInfo(conn, myField["TABLE_NAME"].ToString()));
                sb.Append("</" + containerName + ">" + System.Environment.NewLine);
            }
            schemaTable.Dispose();
            conn.Close();
            conn.Dispose();
            return sb.ToString();
        }

        /// <summary>
        /// 获取当前链接下的特定表的列信息
        /// </summary>
        /// <param name="conn">Ole数据连接</param>
        /// <param name="tabName">表名</param>
        /// <returns>ORM格式的XML片段</returns>
        public static string GetOleColumnsInfo(OleDbConnection conn, string tabName)
        {
            DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new Object[] { null, null, tabName, null });
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int idx = schemaTable.Rows.Count;
            ArrayList columnList = new ArrayList(idx);
            string strTemp = string.Empty;
            bool primaryKey = false;

            foreach (DataRow myField in schemaTable.Rows)
            {
                idx = Convert.ToInt32(myField["ORDINAL_POSITION"]);
                strTemp = idx.ToString() + "@<Column name=\"" + myField["COLUMN_NAME"].ToString() + "\" ";
                strTemp += "type=\"" + GetOleDbTypeMapping(Convert.ToInt32(myField["DATA_TYPE"]),
                    Convert.ToInt32(myField["COLUMN_FLAGS"]), out primaryKey) + "\" ";

                if (primaryKey == true)
                {
                    strTemp += "PrimaryKey=\"true\" ";
                }

                if (Convert.ToBoolean(myField["COLUMN_HASDEFAULT"]) == true)
                {
                    strTemp += "default=\"" + myField["COLUMN_DEFAULT"].ToString() + "\" ";
                }

                // CHARACTER_MAXIMUM_LENGTH
                if (myField["CHARACTER_MAXIMUM_LENGTH"].ToString() != string.Empty)
                {
                    strTemp += "MaxLength=\"" + myField["CHARACTER_MAXIMUM_LENGTH"].ToString() + "\" ";
                }

                strTemp += "NullAble=\"" + myField["IS_NULLABLE"].ToString() + "\"";
                if (myField["DESCRIPTION"].ToString() != string.Empty)
                {
                    strTemp += "><description><![CDATA[" + myField["DESCRIPTION"].ToString() + "]]></Description></Column>";
                }
                else
                {
                    strTemp += " />";
                }
                columnList.Add(strTemp);
            }
            schemaTable.Dispose();

            columnList.Sort();
            for (int k = 0; k < columnList.Count; k++)
            {
                strTemp = columnList[k].ToString();
                sb.Append("    " + strTemp.Substring(strTemp.IndexOf("@") + 1) + Environment.NewLine);
            }
            columnList = null;
            return sb.ToString();
        }
    }
}
