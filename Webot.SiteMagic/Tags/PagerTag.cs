/***************************
 * $Id: PagerTag.cs 216 2009-07-21 10:24:15Z fanmaquar@staff.vbyte.com $
 * $Author: fanmaquar@staff.vbyte.com $
 * $Rev: 216 $
 * $Date: 2009-07-21 18:24:15 +0800 (星期二, 21 七月 2009) $
 * ***************************/
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Webot.Common;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 分页标签解析
    /// </summary>
    public class PagerTag : TagBase, IResourceDependency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagerTag"/> class.
        /// </summary>
        public PagerTag() : base()
        { 
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerTag"/> class.
        /// </summary>
        /// <param name="tagdef">The tagdef.</param>
        public PagerTag(string tagdef)
            : base(tagdef)
        {
            TagBase tag = new TagBase(tagdef);
            InheriteFromBase(tag);
            FilledInTagList(tagdef, tagList, this);
        }

        internal const string KEYWORDS = ",TOTAL,BIDX,EIDX,CIDX,PageCount,PageSize,FirstUrl,PreUrl,NextUrl,LastUrl,CurUrl,";


        private int _startIndex = 0;
        /// <summary>
        /// 开始索引
        /// </summary>
        public int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }

        private int _endIndex = 0;
        /// <summary>
        /// 结束索引
        /// </summary>
        public int EndIndex
        {
            get { return _endIndex; }
            set { _endIndex = value; }
        }

        private int _pageSize = 0;
        /// <summary>
        /// 每页显示条数
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        private int _totalRecordCount = 0;
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalRecordCount
        {
            get { return _totalRecordCount; }
            set { _totalRecordCount = value; }
        }

        private string _pageFormat = "";
        /// <summary>
        /// 分页格式化
        /// </summary>
        public string PageFormat
        {
            get { return _pageFormat; }
            set { _pageFormat = value; }
        }

        private string _defaultPageFormat = "?Page={0}";
        /// <summary>
        /// 默认标准分页格式化字符(例:?Page={0})
        /// </summary>
        /// <remarks>
        /// 字符{0}代表当前分页符号
        /// </remarks>
        public string DefaultPageFormat
        {
            get { return _defaultPageFormat; }
            set { _defaultPageFormat = value; }
        }


        private int _currentPage = 1;
        /// <summary>
        /// 当前页次
        /// </summary>
        public int CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; }
        }

        /// <summary>
        /// 当前记录的总页数
        /// </summary>
        public int PageCount
        {
            get
            {
                if (this.TotalRecordCount > 0)
                {
                    int totalPage = TotalRecordCount / PageSize;
                    return (TotalRecordCount % PageSize == 0) ? totalPage : (totalPage + 1);
                }
                else
                {
                    return 0;
                }
            }
        }

        private string _firstPageUrl = "";
        /// <summary>
        /// 第1页的URL地址
        /// </summary>
        public string FirstPageUrl
        {
            get { return _firstPageUrl; }
            set { _firstPageUrl = value; }
        }

        private string _previewPageUrl = "";
        /// <summary>
        /// 上一页的URL地址
        /// </summary>
        public string PreviewPageUrl
        {
            get { return _previewPageUrl; }
            set { _previewPageUrl = value; }
        }

        private string _nextPageUrl = "";
        /// <summary>
        /// 下一页的URL地址
        /// </summary>
        public string NextPageUrl
        {
            get { return _nextPageUrl; }
            set { _nextPageUrl = value; }
        }

        private string _lastPageUrl = "";
        /// <summary>
        /// 最后一页的URL地址
        /// </summary>
        public string LastPageUrl
        {
            get { return _lastPageUrl; }
            set { _lastPageUrl = value; }
        }

        private bool _alwaysShow = false;
        /// <summary>
        /// 是否总是显示分页标签
        /// </summary>
        public bool AlwaysShow
        {
            get { return _alwaysShow; }
            set { _alwaysShow = value; }
        }

        internal void InheriteFromBase(TagBase tag)
        {
            string strTemp = tag.GetAttribute("ID");
            //标志
            if (strTemp != null) { this.IDentity = strTemp; }

            //是否总是显示
            strTemp = tag.GetAttribute("AlwaysShow");
            if (strTemp != null) { this.AlwaysShow = (strTemp.ToLower()=="true"); }

            //分页格式定义
            strTemp = tag.GetAttribute("PageFormat");
            if (strTemp != null) { this.PageFormat = strTemp; }

        }

        internal ArrayList tagList = new ArrayList();

        /// <summary>
        ///  转化为分页标签
        /// </summary>
        public static PagerTag Parse(TagBase tag)
        {
            PagerTag pager = new PagerTag(tag.OuterDefineText);
            pager.InheriteFromBase(tag);
            return pager;
        }

        internal static void FilledInTagList(string tagdef, ArrayList tagList, IResourceDependency res)
        {
            int idxBegin = 0, idxEnd = 0;
            idxBegin = tagdef.IndexOf("#}");
            idxEnd = tagdef.LastIndexOf("{#");
            tagdef = tagdef.Substring(idxBegin+2, idxEnd - idxBegin-2).Trim();

            Regex regEx = new Regex(TagBase.TagDefinitionPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            idxBegin = idxEnd = 0;
            MatchCollection mc = regEx.Matches(tagdef, idxBegin);
            HtmlTextTag htmlTag = null;
            while (mc.Count > 0)
            {
                Match m = mc[0];
                idxEnd = m.Index;

                if (idxEnd > idxBegin) 
                {
                    htmlTag = new HtmlTextTag(tagdef.Substring(idxBegin, idxEnd - idxBegin));
                    tagList.Add(htmlTag);
                }

                TagBase tag = new TagBase(m.Value, m.Index, ref tagdef);
                //Util.Debug(tag.Category);
                if (tag.Category == TagCategory.AutoTag)
                {
                    AutoItem item = AutoItem.Parse(tag);
                    if (res.GetType() == typeof(PagerTag))
                    {
                        item.CallerTag = (PagerTag)res;
                    }
                    tagList.Add(item);
                }
                else if (tag.Category == TagCategory.DefineTag)
                {
                    //数据库标签定义格式为 {#%FieldName%#}
                    if (res != null)
                    {
                        htmlTag = new HtmlTextTag(res.GetDefinition(tag.OuterDefineText).ToString());
                        tagList.Add(htmlTag);
                    }
                }
                else if (tag.Category == TagCategory.CustomTag)
                {
                    #region 数据定义标签内包含数据定义标签
                    DbStoredCustomTag dbTag = DbStoredCustomTag.Parse(tag);
                    dbTag.SetResourceDependency(res);
                    if (dbTag.IsExist == true)
                    {
                        htmlTag = new HtmlTextTag(dbTag.GetTagValue().ToString());
                        tagList.Add(htmlTag);
                    }
                    #endregion
                }
                else if (tag.Category == TagCategory.SystemTag)
                {
                    string pageURLKey = ",FirstUrl,PreUrl,NextUrl,LastUrl,CurUrl,";
                    string tagName = tag.TagName.Trim('$');
                    if (pageURLKey.IndexOf("," + tagName + ",", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        tagName = tagName.ToLower();
                        PageUrl url = new PageUrl(tag.OuterDefineText);
                        url.IsAutoItem = (res != null && res.GetType() == typeof(AutoItem));
                        url.SetResourceDependency(res);
                        switch (tagName)
                        {
                            case "firsturl": url.UrlCategory = PageUrlCategory.FirstPage; break;
                            case "preurl": url.UrlCategory = PageUrlCategory.PrePage; break;
                            case "nexturl": url.UrlCategory = PageUrlCategory.NextPage; break;
                            case "lasturl": url.UrlCategory = PageUrlCategory.LastPage; break;
                            case "cururl": url.UrlCategory = PageUrlCategory.ThisPage; break;
                            default:
                                url.UrlCategory = PageUrlCategory.ThisPage;
                                break;
                        }
                        tagList.Add(url);
                    }
                    else
                    {
                        SystemTag sys = new SystemTag(tag.OuterDefineText);
                        //Util.Debug(false, res.GetType().ToString());
                        sys.SetResourceDependency(res);
                        tagList.Add(sys);
                    }
                }
                else
                {
                    tagList.Add(tag);
                }
                //Util.Debug(false, "#####\n" + tag.OuterDefineText + "\n#####");
                //TagList.Add(tag);

                idxBegin = tag.DefinedIndexEnd;
                mc = regEx.Matches(tagdef, idxBegin);
            }

            if (idxBegin < tagdef.Length)
            {
                htmlTag = new HtmlTextTag(tagdef.Substring(idxBegin));
                tagList.Add(htmlTag);
            }
        }

        private string GetOutputPageFormat()
        {
            if (this.PageFormat.Length > 0)
            {
                return this.PageFormat;
            }
            else
            {
                return this.DefaultPageFormat;
            }
        }

        internal const string EMPTYURL = "javascript:void(0);";
        /// <summary>
        /// 获取上一页URL地址
        /// </summary>
        /// <returns></returns>
        private string GerPreUrlStr()
        { 
           if (CurrentPage == 1)
           {
               return EMPTYURL;
           }
           else
           {
               return string.Format(GetOutputPageFormat(), CurrentPage - 1);
           }
        }

        /// <summary>
        /// 获取下一页URL地址
        /// </summary>
        /// <returns></returns>
        private string GetNextUrlStr()
        {
            if (CurrentPage == PageCount)
            {
                return EMPTYURL;
            }
            else
            {
                return string.Format(GetOutputPageFormat(), CurrentPage + 1);
            }
        }

        #region IResourceDependency Members (ST:TODO)
        /// <summary>
        /// 内部对象定义
        /// </summary>
        public object GetDefinition(string x)
        {
            x = x.Trim('$', '{', '#', '}');
            //TOTAL,BIDX,EIDX,CIDX,PageCount,PageSize,FirstUrl,PreUrl,NextUrl,LastUrl,CurUrl
            object objRet = "";
            switch (x.ToLower())
            {
                case "total": objRet = this.TotalRecordCount; break;
                case "bidx": objRet = this.StartIndex; break;
                case "cidx": objRet = this.CurrentPage; break;
                case "eidx": objRet = this.EndIndex; break;
                case "pagecount": objRet = this.PageCount; break;
                case "pagesize": objRet = this.PageSize; break;
                case "firsturl": objRet = String.Format(GetOutputPageFormat(), 1); break;
                case "preurl": objRet = GerPreUrlStr(); break;
                case "nexturl": objRet = GetNextUrlStr(); break;
                case "lasturl": objRet = string.Format(GetOutputPageFormat(), PageCount); break;
                case "cururl": objRet = string.Format(GetOutputPageFormat(), CurrentPage); break;
                default:
                    objRet = "";
                    break;
            }
            return objRet.ToString();
        }

        public bool IsDefined(string x)
        {
            x = x.Trim('$','{','#','}');
            return (KEYWORDS.IndexOf("," + x + ",", StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        #endregion

        /// <summary>
        /// 获取HTML字符输出
        /// </summary>
        public override string ToString()
        {
            if (this.AlwaysShow == false && this.PageCount < 2)
            {
                return "";
            }

            StringBuilder pb = new StringBuilder();
            for (int i = 0; i < tagList.Count; i++)
            {
                pb.Append(tagList[i].ToString());
            }
            return pb.ToString();
        }

        /// <summary>
        /// 分页地址分类
        /// </summary>
        public enum PageUrlCategory
        { 
            /// <summary>
            /// 首页
            /// </summary>
            FirstPage = 1,
            /// <summary>
            /// 上一页
            /// </summary>
            PrePage = 2,
            /// <summary>
            /// 下一页
            /// </summary>
            NextPage = 3,
            /// <summary>
            /// 最后一页
            /// </summary>
            LastPage = 4,
            /// <summary>
            /// 中间页
            /// </summary>
            MidPage = 5,
            /// <summary>
            /// 当前页
            /// </summary>
            ThisPage = 6
        }

        /// <summary>
        /// 分页标签里的自动项
        /// </summary>
        public class AutoItem : TagBase, IResourceDependency
        {
            /// <summary>
            /// 自动(隐藏/空)标签
            /// </summary>
            public AutoItem()
            { }

            public AutoItem(string tagdef)
                : base(tagdef)
            {
                PagerTag.FilledInTagList(tagdef, tagList, this);
            }

            private PagerTag _callerTag = null;
            /// <summary>
            /// 设置调用的分页标签对象
            /// </summary>
            public PagerTag CallerTag
            {
                get { return _callerTag; }
                set { _callerTag = value; }
            }


            private ArrayList tagList = new ArrayList();

            public static AutoItem Parse(TagBase tag)
            {
                return new AutoItem(tag.OuterDefineText); 
            }

            public override string ToString()
            {
                StringBuilder pb = new StringBuilder();
                string strTemp = "";
                bool retEmpty = false;
                for (int i = 0; i < tagList.Count; i++)
                {
                    strTemp = tagList[i].ToString();
                    if (strTemp == PagerTag.EMPTYURL)
                    {
                        retEmpty = true; 
                        break;
                    }
                    else
                    {
                        pb.Append(strTemp);
                    }
                }
                return (retEmpty == false) ? pb.ToString() : "";
            }

            #region IResourceDependency Members

            public object GetDefinition(string x)
            {
                return (this.CallerTag == null) ? null : this.CallerTag.GetDefinition(x);
            }

            public bool IsDefined(string x)
            {
                return (this.CallerTag != null && this.CallerTag.IsDefined(x));
            }

            /// <summary>
            /// 依赖标识
            /// </summary>
            /// <value>依赖标识号</value>
            public string DependencyIdentity
            {
                get { return "AutoItemDependency"; }
            }

            #endregion
        }

        /// <summary>
        /// 分页地址显示类
        /// </summary>
        public class PageUrl : TagBase
        {
            public PageUrl()
                : base()
            { }

            public PageUrl(string tagdef)
                : base(tagdef)
            {

            }

            private string _pageFormat = "";
            /// <summary>
            /// 分页格式化字符
            /// </summary>
            public string PageFormat
            {
                get { return _pageFormat; }
                set { _pageFormat = value; }
            }

            private PageUrlCategory _cat = PageUrlCategory.ThisPage;
            /// <summary>
            /// 分页的内页类型
            /// </summary>
            public PageUrlCategory UrlCategory
            {
                get { return _cat; }
                set { _cat = value; }
            }

            private bool _isAutoItem = false;
            /// <summary>
            /// 是否自动显示输出项
            /// </summary>
            public bool IsAutoItem
            {
                get { return _isAutoItem; }
                set { _isAutoItem = value; }
            }


            //public static PageUrl operator ++(PageUrl url)
            //{
            //    return null;
            //    //return url;
            //}

            //public static PageUrl operator --(PageUrl url)
            //{
            //    return url;
            //}

            /// <summary>
            /// 显示为输出形式
            /// </summary>
            public override string ToString()
            {
                return base.ToString();
            }

        }


        #region IResourceDependency Members

        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        public string DependencyIdentity
        {
            get { return "ListTagDependency#" + this.IDentity; }
        }

        #endregion
    }
}
