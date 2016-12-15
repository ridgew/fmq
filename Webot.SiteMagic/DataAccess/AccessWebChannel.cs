using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Webot.Common;
using Webot.SiteMagic;

namespace Webot.DataAccess
{
    public class AccessWebChannel : IFilledByID, IWebChannel
    {
        /// <summary>
        /// Access数据库存储的对象
        /// </summary>
        public AccessWebChannel() : base() { }

        /// <summary>
        /// Access数据库存储的对象
        /// </summary>
        public AccessWebChannel(int AccessChannelID)
        {
            this.ChannelID = AccessChannelID;
            this.SetDefined("ChannelID", AccessChannelID);
        }

        private int _channelID = 0;
        /// <summary>
        /// 频道编号
        /// </summary>
        public int ChannelID
        {
            get { return _channelID; }
            set { _channelID = value; }
        }

        private int _sort = 500;
        /// <summary>
        /// 同级排序
        /// </summary>
        public int Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }

        private string _channelName;
        /// <summary>
        /// 频道名称
        /// </summary>
        public string ChannelName
        {
            get { return _channelName; }
            set { _channelName = value; }
        }

        private IWebChannel _parentChannel;
        /// <summary>
        /// 上级频道
        /// </summary>
        public IWebChannel ParentChannel
        {
            get { return _parentChannel; }
            set { _parentChannel = value; }
        }

        private bool _isOuterLink;
        /// <summary>
        /// 是否是外部链接
        /// </summary>
        public bool IsOuterLink
        {
            get { return _isOuterLink; }
            set { _isOuterLink = value; }
        }

        private string _outerLinkUrl;
        /// <summary>
        /// 外部链接URL地址
        /// </summary>
        public string OuterLinkUrl
        {
            get { return _outerLinkUrl; }
            set { _outerLinkUrl = value; }
        }

        private int _archiveDays;
        /// <summary>
        /// 归档天数
        /// </summary>
        public int ArchiveDays
        {
            get { return _archiveDays; }
            set { _archiveDays = value; }
        }

        private string staticFileGenDir;
        /// <summary>
        /// 静态物理文件生成基础路径，需要以'/'结尾。
        /// </summary>
        public string StaticFileGenDir
        {
            get { return staticFileGenDir; }
            set { staticFileGenDir = value; }
        }

        private TempletSetting _tptSettings;
        /// <summary>
        /// 模板定义设置
        /// </summary>
        public TempletSetting TempletConfig
        {
            get { return _tptSettings; }
            set { _tptSettings = value; }
        }

        private bool _internalDbStoreReady = true;

        /// <summary>
        /// 频道是否存在
        /// </summary>
        public bool IsExist = false;

        /// <summary>
        /// 获取该频道的导航菜单
        /// </summary>
        /// <param name="ChannelID">频道编号</param>
        /// <param name="IDAndNameFormat">一个频道的完整链接格式化字符，0参为频道ID，1参为频道名称。</param>
        /// <param name="sperator">频道与上级频道之间的分隔字符</param>
        /// <example>
        ///     AccessWebChanel.DrawHtmlNavPath(58, "<a href=/{0}/list.html>{1}</a>", " &gt;&gt; ")
        /// </example>
        public static string DrawHtmlNavPath(int ChannelID, string IDAndNameFormat, string sperator)
        {
            // "{"及"}"为关键标签字符
            IDAndNameFormat = Regex.Replace(IDAndNameFormat, @"\[(\d+)\]", "{$1}");
            string sql = string.Format("select ParentTypeID,TypeID,TypeName,OuterLinkUrl,DisplayByPath from Tbl_Type where TypeID={0}", ChannelID);
            ArrayList list = new ArrayList();

            DataRow dRow = null;
            int ParentTypeID = 1;

            OleDbHelper hp = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey);
            while (ParentTypeID > 0)
            {
                dRow = hp.GetDataRow(sql);
                if (dRow != null)
                {
                    sql = dRow["OuterLinkUrl"].ToString();
                    if (Convert.ToBoolean(dRow["DisplayByPath"]) == true && sql.Length > 0)
                    {
                        list.Add(string.Format(Regex.Replace(IDAndNameFormat, "(href=)(\\'|\\\"?)([^\\s\\>]+)(\\2)", "$1$2{0}$4", RegexOptions.IgnoreCase),
                            sql, dRow["TypeName"].ToString()));
                    }
                    else
                    {
                        list.Add(string.Format(IDAndNameFormat, dRow["TypeID"].ToString(), dRow["TypeName"].ToString()));
                    }
                    ParentTypeID = Convert.ToInt32(dRow["ParentTypeID"]);
                    if (ParentTypeID > 0)
                    {
                        sql = string.Format("select ParentTypeID,TypeID,TypeName,OuterLinkUrl,DisplayByPath from Tbl_Type where TypeID={0}", ParentTypeID);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (list.Count > 0)
            {
                list.Reverse();
                string[] objChNav = (string[])list.ToArray(typeof(string));
                return string.Join(sperator, objChNav);
            }
            else { return ""; }
        }
        
        /// <summary>
        /// 数据源是否准备就绪
        /// </summary>
        public bool DataSourceIsReady
        {
            get
            {
                return _internalDbStoreReady;
            }
            set
            {
                _internalDbStoreReady = value;
            }
        }
        
        /// <summary>
        /// 获取当前实体对象
        /// </summary>
        public object GetInstance()
        {
            if (this.ChannelID == 0)
            {
                return this;
            }
            else
            {
                if (this is IFilledByID)
                {
                    IFilledByID objIntial = this as IFilledByID;
                    if (objIntial == null)
                    {
                        return null;
                    }
                    else
                    {
                        if (objIntial.IsFilled)
                        {
                            return objIntial;
                        }
                        else
                        {
                            return objIntial.GetInstanceById(this.ChannelID);
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        #region IInitializedByID Members
        /// <summary>
        /// 通过ID初始化实例对象实现
        /// </summary>
        public object GetInstanceById(int InstanceID)
        {
            AccessWebChannel aWebChannel = new AccessWebChannel(InstanceID);
            DataRow dRow = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey).GetDataRow(string.Format("select top 1 * from Tbl_Type where TypeID={0}", InstanceID));
            if (dRow != null)
            {
                aWebChannel.ChannelName = dRow["TypeName"].ToString();
                aWebChannel.Sort = Convert.ToInt32(dRow["Sort"]);
                aWebChannel.IsOuterLink = Convert.ToBoolean(dRow["DisplayByPath"]);
                aWebChannel.OuterLinkUrl = dRow["OuterLinkUrl"].ToString();
                aWebChannel.StaticFileGenDir = dRow["StaticFileDir"].ToString();
                aWebChannel.ArchiveDays = Convert.ToInt32(dRow["ArchiveDays"]);
                aWebChannel.ParentChannel = new AccessWebChannel(Convert.ToInt32(dRow["ParentTypeID"]));

                aWebChannel.SetDefined("GroupID", dRow["GroupID"].ToString());
                #region Templet with file generate rules.
                string xmlTptRule = dRow["GenTempletRule"].ToString();
                if (xmlTptRule.Length > 10)
                {
                    StringReader sR = new StringReader(xmlTptRule);
                    //   OleDbHelper.AppendToFile(true, @"D:\wwwroot\newsign\chinaqmzxco29607\@docs\temp.xml",
                    //"\n\r\n**" + dRow["GenTempletRule"].ToString());
                    XmlSerializer xSer = new XmlSerializer(typeof(TempletSetting));
                    aWebChannel.TempletConfig = (TempletSetting)xSer.Deserialize(sR);
                }
                #endregion

                aWebChannel.IsExist = true;
            }
            aWebChannel.IsFilled = true;
            return aWebChannel;
        }

        private bool _isFilled = false;
        /// <summary>
        /// 是否已填充该对象
        /// </summary>
        public bool IsFilled
        {
            get { return _isFilled; }
            set { _isFilled = value; }
        }
        #endregion

        /// <summary>
        /// 频道默认新闻
        /// </summary>
        public IWebNews GetPrimaryNews()
        {
            DataRow dRow = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey)
                .GetDataRow(string.Format("select top 1 * from Tbl_Articles where IsPrimary=true and Pubed=true and TypeID={0} order by NewsID desc",
                this.ChannelID));
            if (dRow != null)
            {
                AccessWebNews aWebNews = AccessWebNews.LoadFromDataRow(dRow);
                aWebNews.IsExist = true;
                aWebNews.IsFilled = true;
                return aWebNews;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 该频道定义的其他对象字典
        /// </summary>
        private Dictionary<string, object> DefinedDic = new Dictionary<string, object>();

        /// <summary>
        /// 设置定义键值对象
        /// </summary>
        public void SetDefined(string key, object value)
        {
            if (DefinedDic.ContainsKey(key))
            {
                DefinedDic[key] = value;
            }
            else
            {
                DefinedDic.Add(key, value);
            }
        }

        /// <summary>
        /// 获取对象键值定义
        /// </summary>
        public object GetDefinition(string x)
        {
            AccessWebChannel instance = this;
            if (this.ChannelID != 0 && this.IsFilled == false)
            {
                instance = (AccessWebChannel)this.GetInstance();
            }

            //如果不是标签定义
            if (!Regex.IsMatch(x, TagBase.TagDefinitionPattern, RegexOptions.IgnoreCase))
            {
                return DefinedDic[x];
            }

            #region 数据库实体定义标签
            if (Regex.IsMatch(x, @"^\{#%([\w]+)%#\}$", RegexOptions.IgnoreCase | RegexOptions.Compiled))
            {
                object strRet = "";
                x = x.Trim('{', '#', '}', '%');

                switch (x.ToLower())
                {
                    case "typeid": strRet = instance.ChannelID; break;
                    case "typename": strRet = instance.ChannelName; break;
                    case "sort": strRet = instance.Sort; break;
                    case "displaybypath": strRet = instance.IsOuterLink; break;
                    case "staticfiledir": strRet = instance.StaticFileGenDir; break;
                    case "archivedays": strRet = instance.ArchiveDays; break;
                    case "parenttypeid": strRet = instance.ParentChannel.ChannelID; break;
                    default: break;
                }
                return strRet;
            } 
            #endregion

            // List-{#$ChannelID$#}.html
            // 系统标签： {#$this.PrimaryNews["Content"]$#}
            // -- 
            #region 频道默认新闻
            if (x.StartsWith("{#$this.PrimaryNews"))
            {
                //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " by Channel ");
                IWebNews pNews = instance.GetPrimaryNews();
                string[] objStr;
                //Pattern:  \[(\"|\')([\w\-_]+)(\1)\]
                bool isMatched = Util.GetSingleMatchValue("\\[(\\\"|\\')([\\w\\-_]+)(\\1)\\]", x, out objStr);
                return (pNews != null && isMatched) ? pNews.GetDefinition("{#%" + objStr[2] + "%#}") : "";
            } 
            #endregion

            #region 上级频道定义
            if (x.StartsWith("{#$ParentChannel."))
            {
                string key = "";
                if (x.StartsWith("{#$ParentChannel.PrimaryNews"))
                {
                    key = x.Replace("ParentChannel.PrimaryNews", "this.PrimaryNews");
                    return instance.ParentChannel.GetDefinition(key);
                }

                key = x.Substring(17).TrimEnd('}', '$', '#');
                if (instance.ParentChannel != null)
                {
                    //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + key);
                    return instance.ParentChannel.GetDefinition(string.Concat("{#%", key, "%#}"));
                }
                else
                {
                    return null;
                }
            }
            #endregion

            #region 频道自身定义
            if (x.StartsWith("{#$Channel."))
            {
                if (x.StartsWith("{#$Channel.ParentChannel."))
                {
                    return GetDefinition("{#$" + x.Substring(11));
                }
                else
                {
                    return GetDefinition("{#%" + x.Substring(11).TrimEnd('}', '#', '$') + "%#}");
                }
            } 
            #endregion

            #region 词典定义
            StringBuilder sb = new StringBuilder(x.Length);
            Regex RE = new Regex(@"\{#\$([\w]+)\$#\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection mc = RE.Matches(x);
            int idxBegin = 0, idxEnd = 0;
            foreach (Match m in mc)
            {
                idxEnd = m.Index;
                sb.Append(x.Substring(idxBegin, idxEnd - idxBegin));

                //处理标签定义
                string key = m.Groups[1].Value;
                if (DefinedDic.ContainsKey(key))
                {
                    sb.Append(DefinedDic[key].ToString());
                }
                else
                {
                    sb.Append(m.Value);
                }
                idxBegin = idxEnd + m.Length;
            }
            if (idxBegin < x.Length)
            {
                sb.Append(x.Substring(idxBegin));
            }

            //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + "在频道中获取定义：" + x
            //    + System.Environment.NewLine + " = " + sb.ToString());

            return sb.ToString();
            #endregion
        }

        /// <summary>
        /// 存储/更新实体对象是否成功
        /// </summary>
        public bool StoredSuccess(object Instance)
        {
            AccessWebChannel aWebChannel = Instance as AccessWebChannel;
            if (aWebChannel == null) return false;
            string strOp = (aWebChannel.ChannelID == 0) ? "I" : "U@[TypeID=" + aWebChannel.ChannelID.ToString() + "]";
            string sql = OleDbHelper.BuildSqlIU(OleDbHelper.DbDialect.MsAccess, "Tbl_Type", strOp,
                new string[] { "TypeName", "ParentTypeID", "Sort", "GroupID",
                    "StaticFileDir", "ArchiveDays", "DisplayByPath", "OuterLinkUrl", "GenTempletRule" },
                new object[] { 
                    OleDbHelper.EscapeSQL(aWebChannel.ChannelName),
                    aWebChannel.ParentChannel.ChannelID,
                    aWebChannel.Sort,
                    Convert.ToInt32(aWebChannel.GetDefinition("GroupID")),
                    OleDbHelper.EscapeSQL((aWebChannel.StaticFileGenDir + "/").Replace("//","/")),
                    aWebChannel.ArchiveDays,
                    aWebChannel.IsOuterLink,
                    OleDbHelper.EscapeSQL(aWebChannel.OuterLinkUrl),
                    OleDbHelper.EscapeSQL(aWebChannel.GetTempletConfigStoreText(65535))
                });

            strOp = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey).ExecuteNonQuery(sql).ToString();
            return (strOp != "0");
        }

        /// <summary>
        /// 获取模板定义的数据库存储文本
        /// </summary>
        /// <param name="tptConfig">模板设置</param>
        /// <param name="maxLength">存储字段的最大长度限制，如255字节等。</param>
        /// <returns>如果操作获取长度则抛出异常</returns>
        public static string GetTempletSettingStoreXml(TempletSetting tptConfig, int maxLength)
        {
            XmlSerializer xSer = new XmlSerializer(typeof(TempletSetting));
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            xSer.Serialize(writer, tptConfig);
            string result = sb.ToString();
            //OleDbHelper.AppendToFile(false, "~/temp.xml", result);
            writer.Close();

            if (result.Length > maxLength)
            {
                throw new OverflowException("序列化后的对象字符为" + result.Length.ToString() + "，超过最大限定长度。");
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// 获取模板定义的数据库存储文本
        /// </summary>
        /// <param name="maxlength">存储字段的最大长度限制，如255字节等。</param>
        public string GetTempletConfigStoreText(int maxlength)
        {
            return GetTempletSettingStoreXml(this.TempletConfig, maxlength);
        }

        /// <summary>
        /// 返回特定对象是否有定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>是否定义过该对象</returns>
        public bool IsDefined(string x)
        {
            object objTemp = this.GetDefinition(x);
            return (objTemp != null && objTemp.ToString() != x);
        }

        ///// <summary>
        ///// 是否是已存在的外部连接URL
        ///// </summary>
        ///// <param name="ChannelID">频道编号</param>
        ///// <param name="OuterLinkUrl">外部链接地址</param>
        //public static bool IsReservedOuterLinkUrl(int ChannelID, string OuterLinkUrl)
        //{
        //    return false;
        //}


        #region IResourceDependency Members
        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        public string DependencyIdentity
        {
            get { return "WebChannel"; }
        }

        #endregion
    }
}
