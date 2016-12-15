using System;
using Webot.Common;
using System.Data;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 模板大体分类
    /// (version:ToBuild)
    /// </summary>
    public enum TempletCategory
    { 
        /// <summary>
        /// 综合单一页面模板
        /// </summary>
        MixedSingle = 0,
        /// <summary>
        /// 以列表为主的页面模板
        /// </summary>
        OnListPage = 1,
        /// <summary>
        /// 以内容展示为主的页面模板
        /// </summary>
        OnDetailPage = 2
    }

    /// <summary>
    /// 页面模板
    /// </summary>
    [Serializable]
    public class PageTemplet : IFilledByID, IStorage
    {
        /// <summary>
        /// 页面模板对象(未初始化)
        /// </summary>
        public PageTemplet() { }

        /// <summary>
        /// 页面模板对象
        /// </summary>
        /// <param name="TptID">系统编号</param>
        public PageTemplet(int TptID) {
            this.TempletID = TptID;
        }

        private int _templetID = 0;
        /// <summary>
        /// 模板系统编号
        /// </summary>
        public int TempletID
        {
            get { return _templetID; }
            set { _templetID = value; }
        }

        
        private FileNameSetting _fileNameConfig;
        /// <summary>
        /// 模板生成文件名配置
        /// </summary>
        public FileNameSetting FileNameConfig
        {
            get { return _fileNameConfig; }
            set { _fileNameConfig = value; }
        }

        [NonSerialized]
        private string _tptRawContent = "";
        /// <summary>
        /// 模板原始内容获取
        /// </summary>
        public string TempletRawContent
        {
            get { return _tptRawContent; }
            set { _tptRawContent = value; }
        }

        /// <summary>
        /// 模板是否存在
        /// </summary>
        public bool IsExist = false;

        private TempletCategory _tptCategory;
        /// <summary>
        /// 模板分类
        /// </summary>
        public TempletCategory TptCategory
        {
            get { return _tptCategory; }
            set { _tptCategory = value; }
        }

        /// <summary>
        /// 通过模板名称获取模板内容
        /// </summary>
        /// <param name="TptName">模板名称</param>
        public object GetInstanceByName(string TptName)
        {
            int tid = 0;
            DataRow dRow = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey)
                .GetDataRow(string.Format("select top 1 TempletID from Tbl_Templets where TempletName='{0}'", TptName));
            if (dRow != null)
            {
                tid = Convert.ToInt32(dRow["TempletID"]);
            }
            return GetInstanceById(tid);
        }

        /// <summary>
        /// 根据副本路径文件刷新模板内容
        /// </summary>
        public void RefreshContentByCopy()
        {
            PageTemplet pageTptSet = this;
            if (this.IsFilled == false)
            {
                pageTptSet = this.GetInstance() as PageTemplet;
            }

            string fTptCopy = Util.ParseAppPath(pageTptSet.FileSaveUrl);
            if (System.IO.File.Exists(fTptCopy))
            {
                pageTptSet.TempletRawContent = OleDbHelper.GetTextFileContent(fTptCopy, "utf-8");
                pageTptSet.StoredSuccess(pageTptSet);
            }
        }

        #region IFilledByID Members
        /// <summary>
        /// 通过ID获取实例
        /// </summary>
        public object GetInstanceById(int InstanceID)
        {
            PageTemplet tpt = new PageTemplet(InstanceID);
            DataRow dRow = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey)
                .GetDataRow(string.Format("select top 1 * from Tbl_Templets where TempletID={0}",
                InstanceID));
            if (dRow != null)
            {
                tpt.IsExist = true;

                tpt.TempletName = dRow["TempletName"].ToString();
                tpt.Property = dRow["Property"].ToString();
                tpt.SiteBaseDir = dRow["SiteBaseDir"].ToString();
                tpt.FileSaveUrl = dRow["FileSaveUrl"].ToString();
                tpt.IsSystemBuild = Convert.ToBoolean(dRow["IsSysBuild"]);
                tpt.IsMixedSyntax = Convert.ToBoolean(dRow["MixedSyntax"]);
                tpt.ModifiedTime = Convert.ToDateTime(dRow["TimeFlag"]);
                tpt.TempletRawContent = dRow["Content"].ToString();
                //tpt.ResDependency = (byte[])dRow["ResDependence"];

                int tptCate = Convert.ToInt32(dRow["Category"]);
                if (tptCate == 1)
                {
                    tpt.TptCategory = TempletCategory.OnListPage;
                }
                else if (tptCate == 2)
                {
                    tpt.TptCategory = TempletCategory.OnDetailPage;
                }
                else
                {
                    tpt.TptCategory = TempletCategory.MixedSingle;
                }
            }
            tpt.IsFilled = true;

            return tpt;
        }

        private bool _isFilled = false;
        /// <summary>
        /// 是否已填充数据
        /// </summary>
        public bool IsFilled
        {
            get
            {
                return this._isFilled;
            }
            set
            {
                this._isFilled = value;
            }
        }

        #endregion

        private string _tptName;
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TempletName
        {
            get { return _tptName; }
            set { _tptName = value; }
        }

        private string _property;
        /// <summary>
        /// 模板属性
        /// </summary>
        public string Property
        {
            get { return _property; }
            set { _property = value; }
        }

        private string _siteBaseDir;
        /// <summary>
        /// 模板资源基于站点的路径目录
        /// </summary>
        public string SiteBaseDir
        {
            get { return _siteBaseDir; }
            set { _siteBaseDir = value; }
        }

        private string _FileSaveUrl;
        /// <summary>
        /// 模板文件保存的物理路径
        /// </summary>
        public string FileSaveUrl
        {
            get { return _FileSaveUrl; }
            set { _FileSaveUrl = value; }
        }

        private bool _isMixedSyntax = false;
        /// <summary>
        /// 是否有标签语法需要解析
        /// </summary>
        public bool IsMixedSyntax
        {
            get { return _isMixedSyntax; }
            set { _isMixedSyntax = value; }
        }

        private DateTime _ModifiedTime;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedTime
        {
            get { return _ModifiedTime; }
            set { _ModifiedTime = value; }
        }

        private bool _IsSystemBuild = false;
        /// <summary>
        /// 是否是系统发布时构建的必备模板
        /// </summary>
        public bool IsSystemBuild
        {
            get { return _IsSystemBuild; }
            set { _IsSystemBuild = value; }
        }

        private XmlPackage _resDependency;
        /// <summary>
        /// 模板所依赖的相关资源
        /// </summary>
        public XmlPackage ResDependency
        {
            get { return _resDependency; }
            set { _resDependency = value; }
        }



        #region IStorage Members
        /// <summary>
        /// 存储/更新实体对象是否成功
        /// </summary>
        public bool StoredSuccess(object Instance)
        {
            PageTemplet pageTpt = Instance as PageTemplet;
            if (pageTpt == null) return false;

            string strOp = (pageTpt.TempletID == 0) ? "I" : "U@[TempletID=" + pageTpt.TempletID.ToString() + "]";
            string sql = OleDbHelper.BuildSqlIU(OleDbHelper.DbDialect.MsAccess, "Tbl_Templets", strOp,
               new string[] { "TempletName", "SiteBaseDir", "FileSaveUrl", "Content", "TimeFlag", "MixedSyntax", "Category" },
               new object[] {
                    OleDbHelper.EscapeSQL(pageTpt.TempletName),
                    OleDbHelper.EscapeSQL(pageTpt.SiteBaseDir),
                    OleDbHelper.EscapeSQL(pageTpt.FileSaveUrl),
                    pageTpt.TempletRawContent,
                    "N'Now()",
                    Util.IsMatch(TagBase.TagDefinitionPattern, pageTpt.TempletRawContent),
                    pageTpt.TptCategory.GetHashCode()
                });
            strOp = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey).ExecuteNonQuery(sql).ToString();
            return (strOp != "0");

        }

        private bool _internalDbStoreReady = true;
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
            if (this.TempletID == 0)
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
                            return objIntial.GetInstanceById(this.TempletID);
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
