using System;
using System.Data;
using System.Text.RegularExpressions;
using Webot.Common;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 持久存储的自定义标签类，形如：{#标签名称#}
    /// </summary>
    public class DbStoredCustomTag : StoredTags, ICustomTag, IContainerCaller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbStoredCustomTag"/> class.
        /// </summary>
        public DbStoredCustomTag() : base ()
        { 
            
        }

        /// <summary>
        /// 数据库存储的自定义标签
        /// </summary>
        /// <param name="tagdef">标签定义文本</param>
        public DbStoredCustomTag(string tagdef) : base( tagdef)
        {
            LoadFromStorag(tagdef);
        }

        ///// <summary>
        ///// 标签列表
        ///// </summary>
        //public List<TagBase> TagList = new List<TagBase>();

        private IContainerCaller _caller;
        /// <summary>
        /// 设置上级容器
        /// </summary>
        public IContainerCaller Caller
        {
            get { return _caller; }
            set { _caller = value; }
        }


        #region ICustomTag Members

        public string GetDefined(string tagDef)
        {
            return tagDef;
        }

        /// <summary>
        /// 是否包含内部标签
        /// </summary>
        public bool ContainTags()
        {
            return Regex.IsMatch(this.Content, TagBase.TagDefinitionPattern, 
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #endregion

        private bool _isExist = false;
        /// <summary>
        /// 该自定义标签是否存在
        /// </summary>
        public bool IsExist
        {
            get { return _isExist; }
            set { _isExist = value; }
        }

        #region 数据库内置属性
        private string _topic = "";
        /// <summary>
        /// HTMLBlock的主题
        /// </summary>
        public string Topic
        {
            get { return _topic; }
            set { _topic = value; }
        }

        private string _content = "";
        /// <summary>
        /// 数据库内容
        /// </summary>
        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        private string _siteBaseDir = "/";
        /// <summary>
        /// 该标签或内容块定义的基础路径
        /// </summary>
        public string SiteBaseDir
        {
            get { return _siteBaseDir; }
            set { _siteBaseDir = value; }
        }

        private bool _isSysBuild = false;
        /// <summary>
        /// 是否是系统内置标签
        /// </summary>
        public bool IsSysBuild
        {
            get { return _isSysBuild; }
            set { _isSysBuild = value; }
        }

        private string _htmlContainer = "#";
        /// <summary>
        /// 所属的HTML容器(默认为#,即不需要任何容器)
        /// </summary>
        public string HtmlContainer
        {
            get { return _htmlContainer; }
            set { _htmlContainer = value; }
        }

        private int _width = 0;
        /// <summary>
        /// 在网页中所占的宽度(像素)
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private int _height = 0;
        /// <summary>
        /// 在网页中所占的高度(像素)
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private string _Property = "";
        /// <summary>
        /// 相关属性
        /// </summary>
        public string Property
        {
            get { return _Property; }
            set { _Property = value; }
        }

        private DateTime _publishDate;
        /// <summary>
        /// 发布修改时间
        /// </summary>
        public DateTime PublishDate
        {
            get { return _publishDate; }
            set { _publishDate = value; }
        }

        private string[] _resDependency = new string[] { };
        /// <summary>
        /// 相关依赖资料(网页呈现相关)
        /// </summary>
        public string[] ResDependency
        {
            get { return _resDependency; }
            set { _resDependency = value; }
        }

        #endregion

        /// <summary>
        /// 获取完整的HTML代码输出
        /// </summary>
        public string GetHtmlOutPut()
        {
            return HtmlTextGenerator.GetRightHtmlText(this.SiteBaseDir, this.Content);
        }

        /// <summary>
        /// 设置相关属性
        /// </summary>
        /// <param name="syntaxTag">标签定义语法</param>
        public void LoadFromStorag(string syntaxTag)
        {
            //OleDbHelper.AppendToFile("~/debug.log", "Load ..." + syntaxTag + Environment.NewLine + Environment.NewLine);
            string sql = string.Format("select top 1 * from Tbl_HtmlBlock where SyntaxTag='{0}'",
                OleDbHelper.EscapeSQL(syntaxTag));

            DataRow dRow = tagStoreHelper.GetDataRow(sql);
            if (dRow != null)
            {
                this.IsExist = true;
                this.IDentity = dRow["BlockID"].ToString();
                this.Topic = dRow["Topic"].ToString();
                this.SiteBaseDir = dRow["SiteBaseDir"].ToString();
                this.IsSysBuild = Convert.ToBoolean(dRow["IsSysBuild"]);
                this.HtmlContainer = dRow["HtmlContainer"].ToString();
                this.Width = Convert.ToInt32(dRow["Width"]);
                this.Height = Convert.ToInt32(dRow["Height"]);
                this.Property = dRow["Property"].ToString();
                this.PublishDate = Convert.ToDateTime(dRow["PublishDate"]);
                this.ResDependency = dRow["ResDependency"].ToString().Split('\n', ',');
                this.Content = dRow["Content"].ToString();
            }
        }

        /// <summary>
        /// 从基础标签转换为数据存储标签
        /// </summary>
        /// <returns></returns>
        public static DbStoredCustomTag Parse(TagBase tag)
        {
            DbStoredCustomTag dbTag = new DbStoredCustomTag(tag.OuterDefineText);
            return dbTag;
        }

        /// <summary>
        /// 获取标签的最终输出值
        /// </summary>
        public override object GetTagValue()
        {
            if (this.IsExist == true)
            {
                //OleDbHelper.AppendToFile("~/debug.log", this.Category.ToString() + Environment.NewLine + Environment.NewLine);
                //OleDbHelper.AppendToFile("~/debug.log", this.OuterDefineText + Environment.NewLine + Environment.NewLine);
                //OleDbHelper.AppendToFile("~/debug.log", this.Content + Environment.NewLine + Environment.NewLine);
                    
                if (!ContainTags())
                {
                    return this.GetHtmlOutPut();
                }
                else
                {
                    TagBase tag = new TagBase(this.Content);
                    if (tag.Category == TagCategory.DataListTag)
                    {
                        ListTag list = ListTag.Parse(tag, this.GetResourceDependency());
                        if (list.PageSize > 0)
                        {
                            string pagedAliaContent = (string.IsNullOrEmpty(list.IDentity)) ? Guid.NewGuid().ToString() : list.IDentity;
                            SetDynamicPagedAlia(pagedAliaContent, list);
                            return pagedAliaContent;
                        }
                        else
                        {
                            return list.GetTagValue();
                        }
                    }
                    else if (tag.Category == TagCategory.PagerTag)
                    {
                        PagerTag pager = PagerTag.Parse(tag);
                        string pagerAlia = (string.IsNullOrEmpty(pager.IDentity)) ? Guid.NewGuid().ToString() : pager.IDentity;
                        SetDynamicPagerDependency(pagerAlia, pager);
                        return pagerAlia;
                    }
                    else
                    {
                        //Util.Debug(false, GetResourceDependency());
                        return TagBase.InterpretContentWithTags(GetHtmlOutPut(), GetResourceDependency());
                    }
                }
            }
            else
            {
                //OleDbHelper.AppendToFile("~/debug.log", "NULL:" + this.OuterDefineText + Environment.NewLine + Environment.NewLine);
                return null;
            }
        }

        #region IContainerCaller Members

        public void SetDynamicPagedAlia(string alia, IPagedContent PagedInstance)
        {
            if (IsTopContainer() == false && this.Caller != null)
            {
                Caller.SetDynamicPagedAlia(alia, PagedInstance);
            }
        }

        /// <summary>
        /// 是否是顶级容器
        /// </summary>
        public bool IsTopContainer()
        {
            return false;
        }

        public void SetDynamicPagerDependency(string alia, PagerTag tag)
        {
            if (IsTopContainer() == false && this.Caller != null)
            {
                Caller.SetDynamicPagerDependency(alia, tag);
            }
        }
        #endregion
    }
}
