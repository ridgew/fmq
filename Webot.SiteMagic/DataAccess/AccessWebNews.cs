using System;
using System.Data;
using Webot.SiteMagic;
using Webot.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace Webot.DataAccess
{
    public class AccessWebNews : IFilledByID, IWebNews
    {
        /// <summary>
        /// Access数据库存储的新闻对象
        /// </summary>
        public AccessWebNews()
            : base()
        { }

        /// <summary>
        /// Access数据库存储的新闻对象
        /// </summary>
        /// <param name="newsID">新闻编号</param>
        public AccessWebNews(int newsID)
        {
            this.NewsID = newsID;
        }

        /// <summary>
        /// Access数据库存储的新闻对象
        /// </summary>
        public AccessWebNews(int newsID, IWebChannel webChannel)
        {
            this.NewsID = newsID;
            this.Channel = webChannel;
        }

        private int _NewsID = 0;
        /// <summary>
        /// 新闻编号
        /// </summary>
        public int NewsID
        {
            get { return _NewsID; }
            set { _NewsID = value; }
        }

        private IWebChannel _Channel;
        /// <summary>
        /// 新闻频道/栏目
        /// </summary>
        public IWebChannel Channel
        {
            get { return _Channel; }
            set { _Channel = value; }
        }

        private int _sort = 500;
        /// <summary>
        /// 新闻同类排序
        /// </summary>
        public int Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }

        private bool _isPrimary = false;
        /// <summary>
        /// 是否所属频道下的默认新闻
        /// </summary>
        public bool IsPrimary
        {
            get { return _isPrimary; }
            set { _isPrimary = value; }
        }

        private string _Title = "";
        /// <summary>
        /// 新闻标题
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        private string _Author = "";
        /// <summary>
        /// 新闻作者
        /// </summary>
        public string Author
        {
            get { return _Author; }
            set { _Author = value; }
        }

        private string _Summary = "";
        /// <summary>
        /// 新闻摘要
        /// </summary>
        public string Summary
        {
            get { return _Summary; }
            set { _Summary = value; }
        }

        private string _Content = "";
        /// <summary>
        /// 新闻内容
        /// </summary>
        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

        private bool _isPubed = false;
        /// <summary>
        /// 是否已发布的新闻
        /// </summary>
        public bool IsPubed
        {
            get { return _isPubed; }
            set { _isPubed = value; }
        }

        private int _hits = 0;
        /// <summary>
        /// 点击次数
        /// </summary>
        public int Hits
        {
            get { return _hits; }
            set { _hits = value; }
        }


        private DateTime _timeflag = DateTime.Now;
        /// <summary>
        /// 发布修改时间
        /// </summary>
        public DateTime TimeFlag
        {
            get { return _timeflag; }
            set { _timeflag = value; }
        }

        private string _VirtualPath = "";
        /// <summary>
        /// 新闻内容虚拟/物理文件相对地址
        /// </summary>
        public string VirtualPath
        {
            get { return _VirtualPath; }
            set { _VirtualPath = value; }
        }

        private string _innerUserName = "";
        /// <summary>
        /// 登录用户名
        /// </summary>
        public string InnerUserName
        {
            get { return _innerUserName; }
            set { _innerUserName = value; }
        }

        /// <summary>
        /// 从数据行中加载实体对象
        /// </summary>
        public static AccessWebNews LoadFromDataRow(DataRow dRow)
        {
            if (dRow == null) return null;
            
            AccessWebNews aWebNews = new AccessWebNews(Convert.ToInt32(dRow["NewsID"]));
            aWebNews.IsExist = true;
            aWebNews.Channel = new AccessWebChannel(Convert.ToInt32(dRow["TypeID"]));
            aWebNews.Title = dRow["Title"].ToString();
            aWebNews.VirtualPath = dRow["VirtualPath"].ToString();
            aWebNews.Sort = Convert.ToInt32(dRow["Sort"]);
            aWebNews.IsPrimary = Convert.ToBoolean(dRow["IsPrimary"]);
            aWebNews.Author = dRow["Author"].ToString();
            aWebNews.InnerUserName = dRow["InnerUserName"].ToString();
            aWebNews.Summary = dRow["Summary"].ToString();
            aWebNews.Content = dRow["Content"].ToString();
            aWebNews.IsPubed = Convert.ToBoolean(dRow["Pubed"]);
            aWebNews.TimeFlag = Convert.ToDateTime(dRow["PublishDate"]);
            aWebNews.Hits = Convert.ToInt32(dRow["Hits"]);

            aWebNews.IsFilled = true;
            return aWebNews;
        }

        private bool _internalDbStoreReady = true;
        /// <summary>
        /// 数据源是否已准备就绪
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
        /// 新闻是否存在
        /// </summary>
        public bool IsExist = false;
        /// <summary>
        /// 获取当前实体对象
        /// </summary>
        public object GetInstance()
        {
            if (this.NewsID == 0)
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
                            return objIntial.GetInstanceById(this.NewsID);
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        
        #region IFilledByID Members
        /// <summary>
        /// 通过ID初始化实例对象实现
        /// </summary>
        public object GetInstanceById(int InstanceID)
        {
            AccessWebNews aWebNews = new AccessWebNews(InstanceID);
            DataRow dRow = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey)
                .GetDataRow(string.Format("select top 1 * from Tbl_Articles where NewsID={0}", InstanceID));
            if (dRow != null)
            {
                aWebNews = AccessWebNews.LoadFromDataRow(dRow);
                aWebNews.IsExist = true;
            }
            aWebNews.IsFilled = true;
            return aWebNews;
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
        /// 更新虚拟文件路径
        /// </summary>
        public void RefreshVirtualPath()
        {
            FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey)
                .ExecuteNonQuery(string.Format("update Tbl_Articles set VirtualPath='{1}' where NewsID={0}",
                this.NewsID, this.VirtualPath));
        }

        /// <summary>
        /// 存储/更新实体对象是否成功
        /// </summary>
        public bool StoredSuccess(object Instance)
        {
            AccessWebNews aWebNews = Instance as AccessWebNews;
            if (aWebNews == null) return false;

            string strOp = (aWebNews.NewsID == 0) ? "I" : "U@[NewsID=" + aWebNews.NewsID.ToString() + "]";
            string sql = OleDbHelper.BuildSqlIU(OleDbHelper.DbDialect.MsAccess, "Tbl_Articles", strOp,
                new string[] { "TypeID", "VirtualPath", "Sort", "IsPrimary",
                    "Title", "Author", "InnerUserName", "Summary", "Content", "Pubed", "PublishDate", "Hits" },
                new object[] { 
                    aWebNews.Channel.ChannelID,
                    OleDbHelper.EscapeSQL(aWebNews.VirtualPath),
                    aWebNews.Sort,
                    aWebNews.IsPrimary,
                    OleDbHelper.EscapeSQL(aWebNews.Title),
                    OleDbHelper.EscapeSQL(aWebNews.Author),
                    OleDbHelper.EscapeSQL(aWebNews.InnerUserName),
                    OleDbHelper.EscapeSQL(aWebNews.Summary),
                    OleDbHelper.EscapeSQL(aWebNews.Content),
                    aWebNews.IsPubed,
                    "N'Now()", aWebNews.Hits
                });

            strOp = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey).ExecuteNonQuery(sql).ToString();
            return (strOp != "0");
        }

        /// <summary>
        /// 获取相关属性的值
        /// </summary>
        private string GetInstanceValue(string x)
        {
            //Util.Debug(false, x);
            if (this.NewsID > 0)
            {
                AccessWebNews news = (this.IsFilled == false) ? (AccessWebNews)this.GetInstanceById(this.NewsID) : this;
                if (news.IsExist == true)
                {
                    object strRet = null;
                    switch (x.ToLower())
                    {
                        case "newsid": strRet = news.NewsID; break;
                        case "channelid": strRet = news.Channel.ChannelID; break;
                        case "title": strRet = news.Title; break;
                        case "virtualpath": strRet = news.VirtualPath; break;
                        case "sort": strRet = news.Sort; break;
                        case "isprimary": strRet = news.IsPrimary; break;
                        case "author": strRet = news.Author; break;
                        case "innerusername": strRet = news.InnerUserName; break;
                        case "summary": strRet = news.Summary; break;
                        case "content": strRet = news.Content; break;
                        case "publishdate": strRet = news.TimeFlag; break;
                        case "hits": strRet = news.Hits; break;
                        default: strRet = "";  break;
                    }
                    return strRet.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// 获取对象键值定义
        /// </summary>
        public object GetDefinition(string x)
        {
            // 系统标签： {#$Channel.TypeId$#}
            // -- 
            // 新闻频道资料获取
            #region 获取频道相关信息
            if (x.StartsWith("{#$Channel."))
            {
                string key = x.Substring(11).TrimEnd('}', '$', '#');
                //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + key);
                if (Channel.IsDefined(string.Concat("{#$", key, "$#}")))
                {
                    return Channel.GetDefinition(string.Concat("{#$", key, "$#}"));
                }
                else
                {
                    return Channel.GetDefinition(string.Concat("{#%", key, "%#}"));
                }
            } 
            #endregion

            if (x.IndexOf("{#$NewsID$#}") != -1)
            {
                x = x.Replace("{#$NewsID$#}", NewsID.ToString());
            }

            // Show-{#%NewsID%#}
            StringBuilder sb = new StringBuilder(x.Length);
            Regex RE = new Regex(@"\{#%([\w]+)%#\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection mc = RE.Matches(x);
            int idxBegin = 0, idxEnd = 0;
            foreach (Match m in mc)
            {
                idxEnd = m.Index;
                sb.Append(x.Substring(idxBegin, idxEnd - idxBegin));
                //处理标签定义
                sb.Append(GetInstanceValue(m.Groups[1].Value));

                idxBegin = idxEnd + m.Length;
            }
            if (idxBegin < x.Length)
            {
                sb.Append(x.Substring(idxBegin));
            }
            return sb.ToString();
        }

        public bool IsDefined(string x)
        {
            return (this.GetDefinition(x) != null && this.GetDefinition(x).ToString() != x);
        }

        public object GetStoredInstance()
        {
            return GetInstance();
        }

        #region IResourceDependency Members


        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        public string DependencyIdentity
        {
            get { return "WebNews"; }
        }

        #endregion
    }
}
