using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using Webot.Common;
using Webot.DataAccess;
using Webot.WebUIPackage;
using System.IO;

namespace Webot.SiteMagic
{
    public class PageGenerator :  SupportProgressBase, IContainerCaller, IDisposable
    {
        /// <summary>
        /// 网页文件生成机
        /// </summary>
        public PageGenerator() : base ()
        {

        }

        private IPagedContent _pagedObject;
        /// <summary>
        /// 可分页对象
        /// </summary>
        public IPagedContent PagedObject
        {
            get { return _pagedObject; }
            set { _pagedObject = value; }
        }

        private string _siteRootDir = "";
        /// <summary>
        /// 站点的物理文件根目录
        /// </summary>
        public string SiteRootDir
        {
            get { return _siteRootDir; }
            set { _siteRootDir = value; }
        }

        private string _pagedContentAlia;
        /// <summary>
        /// 模板内分页内容占位别名
        /// </summary>
        public string PagedContentAlia
        {
            get { return _pagedContentAlia; }
            set { _pagedContentAlia = value; }
        }

        /// <summary>
        /// 当产生静态文件时触发事件
        /// </summary>
        public EventHandler onGeneratorFile
        {
            get;
            set;
        }

        private void FireGeneratorFileFinished(string filePath)
        {
            if (this.onGeneratorFile != null)
            {
                onGeneratorFile(new FileInfo(Util.ParseAppPath(filePath)), EventArgs.Empty);
            }
        }

        /// <summary>
        /// 获取站点文件的基于根目录的相对URL地址
        /// </summary>
        /// <param name="urlPhysicalPath">站点文件的物理完整地址</param>
        public string GetSiteRelativeUrl(string urlPhysicalPath)
        {
            if (this.SiteRootDir.Length > 0)
            {
                return urlPhysicalPath.Replace(this.SiteRootDir, "/").Replace("\\", "/");
            }
            else if (HttpContext.Current != null)
            {
                //Request.ServerVariables["APPL_PHYSICAL_PATH"]
                return urlPhysicalPath.Replace(HttpContext.Current.Server.MapPath("/"), "/").Replace("\\", "/");
            }
            else
            {
                return urlPhysicalPath.Replace("\\", "/");
            }
        }

        #region 重写进度状态
        /// <summary>
        /// 不支持暂停
        /// </summary>
        public override bool IsSupportPause()
        {
            return false;
        }

        #endregion

        /// <summary>
        /// 根据网站页面完整相对地址自动更新页面内容
        /// </summary>
        /// <param name="fileRelatviePath">页面完整相对地址</param>
        public static void GeneratorByPath(string fileRelatviePath)
        {
            string trackPath = fileRelatviePath.Replace("'", "");
            //先找新闻表，再找频道表
            OleDbHelper hp = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey);
            object tid = hp.ExecuteScalar("select top 1 NewsID from Tbl_Articles where VirtualPath='" + trackPath + "'");
            if (tid != null)
            {
                AccessWebNews news = new AccessWebNews(Convert.ToInt32(tid));
                (new PageGenerator()).Generate(news);
            }
            else
            {
                tid = hp.ExecuteScalar("select top 1 TypeID from Tbl_Type where OuterLinkUrl='" + trackPath + "'");
                if (tid != null)
                {
                    AccessWebChannel channel = new AccessWebChannel(Convert.ToInt32(tid));
                   (new PageGenerator()).Generate(channel);
                }
            }

        }

        /// <summary>
        /// 生成新闻静态文件
        /// </summary>
        /// <param name="news">仅指定了NewsID的新闻实体</param>
        public void Generate(AccessWebNews news)
        {
            if (news.IsFilled == false)
            {
                news = (new AccessWebNews()).GetInstanceById(news.NewsID) as AccessWebNews;
            }

            #region 判断新闻有效性
            if (news != null && news.IsExist == true && news.IsFilled == true)
            {
                AccessWebChannel newsChannel = (new AccessWebChannel(news.Channel.ChannelID)).GetInstance() as AccessWebChannel;
                if (newsChannel == null || news.IsFilled == false)
                {
                    ShowMessage("获取改新闻所在的频道设置失败，生成终止！");
                    return;
                }
                else
                {
                    TempletSetting tptConfig = newsChannel.TempletConfig;
                    if (tptConfig == null)
                    {
                        ShowMessage("该频道没有配置模板，不能生成相关文件！");
                    }
                    else
                    {
                        if (tptConfig.GenerateDetailPage == false)
                        {
                            ShowMessage("该频道模板设置关闭了生成详细文件，请先打开再执行生成操作！");
                            return;
                        }

                        string strGenFilePath, strFileName, strFileExt, strGenResult = "";
                        if (newsChannel.StaticFileGenDir.IndexOf("{#$ChannelID$#}") != -1)
                        {
                            newsChannel.StaticFileGenDir = newsChannel.StaticFileGenDir.Replace("{#$ChannelID$#}", newsChannel.ChannelID.ToString());
                        } 
                        strGenFilePath = Util.ParseAppPath(newsChannel.StaticFileGenDir);
                        if (!strGenFilePath.EndsWith("\\")) strGenFilePath += "\\";

                        #region 单篇新闻生成
                        using (TempletParse tpp = new TempletParse())
                        {
                            PageTemplet pageTpt = (new PageTemplet()).GetInstanceById(tptConfig.DetailPageTemplet.TempletID) as PageTemplet;
                            tptConfig.DetailPageTemplet.FileNameConfig.FileNameTag.SetResourceDependency(news);
                            strFileName = tptConfig.DetailPageTemplet.FileNameConfig.FileNameTag.GetTagValue().ToString();
                            strFileExt = tptConfig.DetailPageTemplet.FileNameConfig.FileExtentionName;

                            MultiResDependency newsRes = new MultiResDependency(newsChannel, news);
                            tpp.SetResourceDependency(newsRes);
                            tpp.TagTrack = new System.Collections.Generic.Dictionary<string, string>();
                            tpp.TagTrack.Add("T" + pageTpt.TempletID.ToString(), pageTpt.ModifiedTime.ToString("yyyyMMddHHmmss"));
                            tpp.SetTaggedObjectCollection(pageTpt.TempletRawContent);

                            string newsfileWithFullPath = strGenFilePath + strFileName + strFileExt;
                            if (news.VirtualPath.Length > 5)
                            {
                                newsfileWithFullPath = Util.ParseAppPath(news.VirtualPath);
                            }
                            else
                            {
                                news.VirtualPath = newsChannel.StaticFileGenDir + strFileName + strFileExt;
                                news.RefreshVirtualPath();
                            }

                            string strGenFileContent = HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, tpp.ParsedResult);
                            //插入Track代码
                            string trackPath = GetSiteRelativeUrl(newsfileWithFullPath);
                            GetTrackHtmlInSertCode(tpp.TagTrack, trackPath, "</body>", ref strGenFileContent);
                            strGenResult = OleDbHelper.SetTextFileContent(newsfileWithFullPath, "utf-8", strGenFileContent);
                            TrackUnit.RemoveUpdateRecord(0, trackPath);

                            if (strGenResult == "0")
                            {
                                FireGeneratorFileFinished(newsfileWithFullPath);
                            }

                            ShowMessage((strGenResult == "0") ? string.Format("新闻内容页文件生成成功！<a href=\"{0}\" target=\"_blank\">{0}</a>", GetSiteRelativeUrl(newsfileWithFullPath)) : "生成失败：" + strGenResult);
                        }
                        #endregion
                    }
                }
            } 
            #endregion

            ShowMessage("已完成处理，如没有任何消息显示，这可能是该栏目没有内容或配置错误！");
            ShowMessage("$end$");
        }

        /// <summary>
        /// 频道生成选项
        /// </summary>
        public class ChannelGeneratorOptions
        {
            public ChannelGeneratorOptions(bool genSingle, bool genList, bool genDetail)
            {
                GenerateSinglePage = genSingle;
                GenerateListPage = genList;
                GenerateDetailPage = genDetail;
            }

            /// <summary>
            /// 生成单页综合
            /// </summary>
            public bool GenerateSinglePage { get; set; }

            /// <summary>
            /// 生成列表页面
            /// </summary>
            public bool GenerateListPage { get; set; }

            /// <summary>
            /// 生成详细内容页面
            /// </summary>
            public bool GenerateDetailPage { get; set; }

        }

        /// <summary>
        /// 生成网站栏目/频道静态文件
        /// </summary>
        /// <param name="aWebChannel">指定了ChannelID的实体</param>
        public void Generate(AccessWebChannel aWebChannel)
        {
            Generate(aWebChannel, null);
        }

        /// <summary>
        /// 生成网站栏目/频道静态文件
        /// </summary>
        /// <param name="aWebChannel">指定了ChannelID的实体</param>
        public void Generate(AccessWebChannel aWebChannel, ChannelGeneratorOptions options)
        {
            if (aWebChannel.IsFilled == false)
            {
                aWebChannel = (new AccessWebChannel()).GetInstanceById(aWebChannel.ChannelID) as AccessWebChannel;
            }

            if (aWebChannel != null && aWebChannel.IsExist == true && aWebChannel.IsFilled == true)
            {
                if (aWebChannel.StaticFileGenDir.IndexOf("/") == -1)
                {
                    ShowMessage("生成文件目录设置有错误，系统终止！");
                    return;
                }

                TempletSetting tptConfig = aWebChannel.TempletConfig;
                if (tptConfig == null)
                {
                    ShowMessage("该频道没有配置模板，不能生成相关文件！");
                }
                else
                {
                    string strGenFilePath, strFileName, strFileExt, strGenResult = "";
                    if (aWebChannel.StaticFileGenDir.IndexOf("{#$ChannelID$#}") != -1)
                    {
                        aWebChannel.StaticFileGenDir = aWebChannel.StaticFileGenDir.Replace("{#$ChannelID$#}", aWebChannel.ChannelID.ToString());
                    }
                    strGenFilePath = Util.ParseAppPath(aWebChannel.StaticFileGenDir);
                    //ShowMessage(strGenFilePath);
                    if (!strGenFilePath.EndsWith("\\")) strGenFilePath += "\\";

                    //最终页面内容
                    string strGenFileContent = "";
                    string trackPath = "";

                    this.UTCDateBegin = DateTime.Now.ToUniversalTime();

                    if (options != null && options.GenerateSinglePage == false)
                    {
                        ShowMessage("* 已禁止单页综合内容页生成！");
                    }
                    else
                    {
                        #region 单页综合生成
                        if (tptConfig.GenerateSinglePage == true)
                        {
                            strFileName = tptConfig.MixedSingleTemplet.FileNameConfig.FileNameTag.GetTagValue().ToString();
                            strFileExt = tptConfig.MixedSingleTemplet.FileNameConfig.FileExtentionName;

                            PageTemplet pageTpt = (new PageTemplet()).GetInstanceById(tptConfig.MixedSingleTemplet.TempletID) as PageTemplet;
                            if (pageTpt != null && pageTpt.IsExist == true && pageTpt.IsFilled == true)
                            {
                                if (pageTpt.IsMixedSyntax == false)
                                {
                                    if (!pageTpt.SiteBaseDir.EndsWith("/")) pageTpt.SiteBaseDir += "/";
                                    strGenResult = OleDbHelper.SetTextFileContent(strGenFilePath + strFileName + strFileExt,
                                      "utf-8",
                                      HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, pageTpt.TempletRawContent));

                                    if (strGenResult == "0")
                                    {
                                        FireGeneratorFileFinished(strGenFilePath + strFileName + strFileExt);
                                    }
                                }
                                else
                                {
                                    TempletParse tpp = new TempletParse();
                                    tpp.Caller = this;
                                    tpp.SetResourceDependency(aWebChannel);
                                    tpp.TagTrack = new System.Collections.Generic.Dictionary<string, string>();
                                    tpp.TagTrack.Add("T" + pageTpt.TempletID.ToString(), pageTpt.ModifiedTime.ToString("yyyyMMddHHmmss"));
                                    tpp.SetTaggedObjectCollection(pageTpt.TempletRawContent);

                                    strGenFileContent = HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, tpp.ParsedResult);
                                    //插入Track代码
                                    trackPath = GetSiteRelativeUrl(strGenFilePath + strFileName + strFileExt);
                                    GetTrackHtmlInSertCode(tpp.TagTrack, trackPath, "</body>", ref strGenFileContent);

                                    strGenResult = OleDbHelper.SetTextFileContent(trackPath, "utf-8", strGenFileContent);

                                    if (strGenResult == "0")
                                    {
                                        FireGeneratorFileFinished(trackPath);
                                    }
                                }

                                if (strGenResult == "0")
                                {
                                    TrackUnit.RemoveUpdateRecord(0, trackPath);
                                    ShowMessage(string.Format("单页内容生成成功！<a href=\"{0}\" target=\"_blank\">{0}</a>", GetSiteRelativeUrl(strGenFilePath + strFileName + strFileExt)));
                                }
                            }
                            else
                            {
                                ShowMessage("页面模板实体还原失败，请检查配置是否正确！");
                            }
                        }
                        #endregion
                    }

                    if (options != null && options.GenerateListPage == false)
                    {
                        ShowMessage("* 已禁止列表页生成！");
                    }
                    else
                    {
                        #region 列表页面生成
                        if (tptConfig.GenerateListPage == true)
                        {
                            strFileExt = tptConfig.ListPageTemplet.FileNameConfig.FileExtentionName;
                            tptConfig.ListPageTemplet.FileNameConfig.FileNameTag.SetResourceDependency(aWebChannel);
                            strFileName = tptConfig.ListPageTemplet.FileNameConfig.FileNameTag.GetTagValue().ToString();

                            PageTemplet pageTpt = (new PageTemplet()).GetInstanceById(tptConfig.ListPageTemplet.TempletID) as PageTemplet;
                            if (pageTpt != null && pageTpt.IsExist == true && pageTpt.IsFilled == true)
                            {
                                strGenResult = "";
                                #region 根据模板内的列表动态生成多个页
                                if (pageTpt.IsMixedSyntax == false)
                                {
                                    if (!pageTpt.SiteBaseDir.EndsWith("/")) pageTpt.SiteBaseDir += "/";
                                    strGenResult = OleDbHelper.SetTextFileContent(strGenFilePath + strFileName + strFileExt,
                                      "utf-8",
                                      HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, pageTpt.TempletRawContent));

                                    if (strGenResult == "0")
                                    {
                                        FireGeneratorFileFinished(strGenFilePath + strFileName + strFileExt);
                                        ShowMessage(string.Format("文件生成成功！<a href=\"{0}\" target=\"_blank\">{0}</a>", GetSiteRelativeUrl(strGenFilePath + strFileName + strFileExt)));
                                    }
                                }
                                else
                                {

                                    string realContent = "", myPageHtml = "";
                                    TempletParse tpp = new TempletParse();

                                    //获取要分页变化的模块内容(含Pager)
                                    //替换分页变化的内容 + 生成多页

                                    tpp.Caller = this;
                                    tpp.SetResourceDependency(aWebChannel);

                                    tpp.TagTrack = new System.Collections.Generic.Dictionary<string, string>();
                                    tpp.TagTrack.Add("T" + pageTpt.TempletID.ToString(), pageTpt.ModifiedTime.ToString("yyyyMMddHHmmss"));
                                    tpp.SetTaggedObjectCollection(pageTpt.TempletRawContent);

                                    if (this.PagedObject == null)
                                    {
                                        myPageHtml = tpp.ParsedResult;
                                        //处理分页导航内容
                                        if (this.PagerDic.Count > 0)
                                        {
                                            foreach (string pagerKey in PagerDic.Keys)
                                            {
                                                myPageHtml = myPageHtml.Replace(pagerKey, "");
                                            }
                                        }

                                        strGenFileContent = HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, myPageHtml);
                                        //插入Track代码
                                        trackPath = GetSiteRelativeUrl(strGenFilePath + TagBase.TrimTagDefine(strFileName, '_', '-') + strFileExt);
                                        GetTrackHtmlInSertCode(tpp.TagTrack, trackPath, "</body>", ref strGenFileContent);
                                        strGenResult = OleDbHelper.SetTextFileContent(trackPath, "utf-8", strGenFileContent);
                                        if (strGenResult == "0")
                                        {
                                            FireGeneratorFileFinished(trackPath);
                                        }
                                        TrackUnit.RemoveUpdateRecord(0, trackPath);
                                    }
                                    else
                                    {
                                        string dynTpt = tpp.ParsedResult;
                                        PagedObject.SetResourceDependency(aWebChannel);
                                        while (PagedObject.CurrentPageIndex <= PagedObject.GetPageCount())
                                        {
                                            #region 处理分页内容
                                            realContent = PagedObject.GetCurrentPageContent();
                                            //处理当前页内容
                                            myPageHtml = dynTpt.Replace(PagedContentAlia, realContent);

                                            //处理分页导航内容
                                            if (this.PagerDic.Count > 0)
                                            {
                                                foreach (string pagerKey in PagerDic.Keys)
                                                {
                                                    PagerTag Pager = PagerDic[pagerKey];
                                                    Pager.TotalRecordCount = PagedObject.GetTotalRecordCount();
                                                    Pager.CurrentPage = PagedObject.CurrentPageIndex;
                                                    Pager.StartIndex = PagedObject.StartIndex;
                                                    Pager.EndIndex = PagedObject.EndIndex;
                                                    Pager.PageSize = PagedObject.PageSize;
                                                    Pager.DefaultPageFormat = aWebChannel.StaticFileGenDir + strFileName.Replace("{#$CIDX$#}", "{0}") + strFileExt;

                                                    //Util.Debug(false, Pager.ToString());
                                                    myPageHtml = myPageHtml.Replace(pagerKey, Pager.ToString());
                                                    //Util.Debug(false, this.PagerDic[pagerKey].OuterDefineText);
                                                }
                                            }
                                            int currentPage = PagedObject.CurrentPageIndex;
                                            string currentFileName = strFileName.Replace("{#$CIDX$#}", currentPage.ToString());

                                            strGenFileContent = HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, myPageHtml);
                                            //插入Track代码
                                            trackPath = GetSiteRelativeUrl(strGenFilePath + currentFileName + strFileExt);
                                            GetTrackHtmlInSertCode(tpp.TagTrack, trackPath, "</body>", ref strGenFileContent);
                                            strGenResult = OleDbHelper.SetTextFileContent(trackPath, "utf-8", strGenFileContent);
                                            //清除更新请求
                                            TrackUnit.RemoveUpdateRecord(0, trackPath);

                                            if (strGenResult == "0")
                                            {
                                                FireGeneratorFileFinished(trackPath);
                                                ShowMessage(string.Format("列表页文件生成成功！<a href=\"{0}\" target=\"_blank\">{0}</a>", trackPath));
                                            }

                                            if (currentPage == 1)
                                            {
                                                trackPath = strGenFilePath + currentFileName.Trim('1', '-', '_') + strFileExt;
                                                OleDbHelper.SetTextFileContent(trackPath, "utf-8", strGenFileContent);
                                                FireGeneratorFileFinished(trackPath);
                                                TrackUnit.RemoveUpdateRecord(0, trackPath);
                                            }
                                            PagedObject.MoveNextPage();
                                            #endregion
                                        }
                                    }
                                    tpp.Dispose();
                                }
                                #endregion
                                if (strGenResult == "0")
                                {
                                    ShowMessage("列表页内容生成成功完成。");
                                }
                            }
                            else
                            {
                                ShowMessage("页面模板实体还原失败，请检查配置是否正确！");
                            }
                        }
                        #endregion
                    }

                    if (options != null && options.GenerateDetailPage == false)
                    {
                        ShowMessage("* 已禁止详细内容页生成！");
                    }
                    else
                    {
                        #region 详细页面生成
                        if (tptConfig.GenerateDetailPage == true)
                        {
                            strFileExt = tptConfig.DetailPageTemplet.FileNameConfig.FileExtentionName;
                            PageTemplet pageTpt = (new PageTemplet()).GetInstanceById(tptConfig.DetailPageTemplet.TempletID) as PageTemplet;
                            if (pageTpt != null && pageTpt.IsExist == true && pageTpt.IsFilled == true)
                            {
                                if (pageTpt.IsMixedSyntax == false)
                                {
                                    strFileName = tptConfig.DetailPageTemplet.FileNameConfig.FileNameTag.GetTagValue().ToString();
                                    if (!pageTpt.SiteBaseDir.EndsWith("/")) pageTpt.SiteBaseDir += "/";
                                    strGenResult = OleDbHelper.SetTextFileContent(strGenFilePath + strFileName + strFileExt,
                                      "utf-8",
                                      HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, pageTpt.TempletRawContent));

                                    if (strGenResult == "0")
                                    {
                                        FireGeneratorFileFinished(strGenFilePath + strFileName + strFileExt);
                                    }
                                }
                                else
                                {
                                    TempletParse tpp = new TempletParse();
                                    OleDbHelper hp = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey);

                                    DataTable dTab = hp.GetDataTable(string.Format("select * from Tbl_Articles where Pubed=True and Archived=false and TypeID={0} order by Sort asc,NewsID desc",
                                        aWebChannel.ChannelID));

                                    if (dTab != null && dTab.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < dTab.Rows.Count; i++)
                                        {
                                            AccessWebNews news = AccessWebNews.LoadFromDataRow(dTab.Rows[i]);
                                            tptConfig.DetailPageTemplet.FileNameConfig.FileNameTag.SetResourceDependency(news);
                                            strFileName = tptConfig.DetailPageTemplet.FileNameConfig.FileNameTag.GetTagValue().ToString();

                                            MultiResDependency newsRes = new MultiResDependency(aWebChannel, news);
                                            tpp.SetResourceDependency(newsRes);
                                            tpp.TagTrack = new System.Collections.Generic.Dictionary<string, string>();
                                            tpp.TagTrack.Add("T" + pageTpt.TempletID.ToString(), pageTpt.ModifiedTime.ToString("yyyyMMddHHmmss"));
                                            tpp.SetTaggedObjectCollection(pageTpt.TempletRawContent);

                                            string newsfileWithFullPath = strGenFilePath + strFileName + strFileExt;
                                            if (news.VirtualPath.Length > 5)
                                            {
                                                newsfileWithFullPath = Util.ParseAppPath(news.VirtualPath);
                                            }
                                            else
                                            {
                                                news.VirtualPath = aWebChannel.StaticFileGenDir + strFileName + strFileExt;
                                                news.RefreshVirtualPath();
                                            }

                                            strGenFileContent = HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, tpp.ParsedResult);
                                            //插入Track代码
                                            trackPath = GetSiteRelativeUrl(newsfileWithFullPath);
                                            GetTrackHtmlInSertCode(tpp.TagTrack, trackPath, "</body>", ref strGenFileContent);

                                            strGenResult = OleDbHelper.SetTextFileContent(newsfileWithFullPath, "utf-8", strGenFileContent);
                                            if (strGenResult == "0")
                                            {
                                                TrackUnit.RemoveUpdateRecord(0, trackPath);
                                                FireGeneratorFileFinished(trackPath);
                                                ShowMessage(string.Format("内容页文件生成成功！<a href=\"{0}\" target=\"_blank\">{0}</a>", trackPath));
                                            }
                                        }
                                    }

                                    tpp.Dispose();

                                }

                                if (strGenResult == "0")
                                {
                                    ShowMessage("详细页面内容生成成功完成。");
                                }

                            }
                            else
                            {
                                ShowMessage("页面模板实体还原失败，请检查配置是否正确！");
                            }

                        }
                        #endregion
                    }
                }

                ShowMessage("已完成所有处理，如没有任何消息显示，这可能是该栏目没有内容或配置错误！");
                ShowMessage("$end$");
            }

        }

        /// <summary>
        /// 插入更新跟踪记录的HTML代码
        /// </summary>
        public static void GetTrackHtmlInSertCode(Dictionary<string,string> TrackDic, string trackPath, string LocationTag, ref string OutPutHtml)
        {
            return;

            //插入Track代码
            StringBuilder sb = new StringBuilder();
            foreach (string key in TrackDic.Keys)
            {
                sb.AppendFormat("-{0}!{1}", key, TrackDic[key]);
            }
            sb.Append(trackPath);
            string trackCode = string.Format("<noscript><img src=\"/SiteMagic/Track.aspx/img/{0}\" width=\"1\" height=\"1\" border=\"0\" /></noscript>\r\n"
                + "<script language=\"javascript\" charset=\"utf-8\" src=\"/SiteMagic/Track.aspx/js/{0}\"></script>", sb.ToString().TrimStart('-'));
        
            int bodyEndIdx = OutPutHtml.LastIndexOf(LocationTag, StringComparison.InvariantCultureIgnoreCase);
            if (bodyEndIdx > 0)
            {
                OutPutHtml = String.Concat(OutPutHtml.Substring(0, bodyEndIdx), trackCode, OutPutHtml.Substring(bodyEndIdx));
            }
            else
            {
                OutPutHtml += trackCode;
            }
     }


        #region IContainerCaller Members
        /// <summary>
        /// 设置占位字符和可分页实体
        /// </summary>
        public void SetDynamicPagedAlia(string alia, IPagedContent PagedInstance)
        {
            this.PagedContentAlia = alia;
            this.PagedObject = PagedInstance;
        }

        /// <summary>
        /// 是否是顶级容器
        /// </summary>
        public bool IsTopContainer()
        {
            return true;
        }

        /// <summary>
        /// 该频道定义的其他对象字典
        /// </summary>
        private Dictionary<string, PagerTag> PagerDic = new Dictionary<string, PagerTag>();
        public void SetDynamicPagerDependency(string alia, PagerTag tag)
        {
            if (PagerDic.ContainsKey(alia))
            {
                PagerDic[alia] = tag;
            }
            else
            {
                PagerDic.Add(alia, tag);
            }
        }
        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (this.PagedObject != null) { this.PagedObject.free(); }
        }

        #endregion
    }
}
