using System;
using System.Collections.Generic;
using System.Web;
using Webot.Common;
using Webot.DataAccess;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 内置的动态内容查看
    /// </summary>
    public class SiteMageicView : IHttpHandler, IContainerCaller, IDisposable
    {

        #region IHttpHandler 成员

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest Request = context.Request;
            if (Util.IsNumerical(Request.QueryString["ChannelID"]))
            {
                AccessWebChannel aWebChannel = new AccessWebChannel(Int32.Parse(Request.QueryString["ChannelID"]));
                if (aWebChannel.IsFilled == false)
                {
                    aWebChannel = (new AccessWebChannel()).GetInstanceById(aWebChannel.ChannelID) as AccessWebChannel;
                }

                if (aWebChannel != null && aWebChannel.IsExist == true && aWebChannel.IsFilled == true)
                {
                    TempletSetting tptConfig = aWebChannel.TempletConfig;
                    if (tptConfig == null)
                    {
                        ShowMessage("该频道没有配置模板，不能动态显示相关内容！");
                        return;
                    }

                    if (Request.QueryString["Action"] == "list")
                    {
                        if (tptConfig.GenerateListPage == true)
                        {
                            #region 列表页面生成
                            PageTemplet pageTpt = (new PageTemplet()).GetInstanceById(tptConfig.ListPageTemplet.TempletID) as PageTemplet;
                            if (pageTpt != null && pageTpt.IsExist == true && pageTpt.IsFilled == true)
                            {
                                #region 根据模板内的列表动态显示当前页内容
                                if (pageTpt.IsMixedSyntax == false)
                                {
                                    DisplayContent(HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, pageTpt.TempletRawContent));
                                    return;
                                }
                                else
                                {

                                    string realContent = "", myPageHtml = "";
                                    TempletParse tpp = new TempletParse();

                                    tpp.Caller = this;
                                    tpp.SetResourceDependency(aWebChannel);
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
                                        DisplayContent(HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, myPageHtml));
                                    }
                                    else
                                    {
                                        string dynTpt = tpp.ParsedResult;
                                        string currentPageMatch = "";
                                        PagedObject.SetResourceDependency(aWebChannel);
                                        if (Request.QueryString["Page"] != null && Util.IsMatch(@"^(\d+)$", Request.QueryString["Page"], 1, 1, PagedObject.GetPageCount(), out currentPageMatch))
                                        {
                                            PagedObject.CurrentPageIndex = int.Parse(Request.QueryString["Page"]);
                                        }
                                        else
                                        {
                                            PagedObject.CurrentPageIndex = 1;
                                        }

                                        #region 显示分页内容
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

                                                Pager.DefaultPageFormat = Request.ServerVariables["SCRIPT_NAME"] + "?ChannelID=" + aWebChannel.ChannelID.ToString() + "&action=list&Page={0}";
                                                myPageHtml = myPageHtml.Replace(pagerKey, Pager.ToString());
                                            }
                                        }
                                        DisplayContent(HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, myPageHtml));
                                        #endregion
                                    }
                                    tpp.Dispose();
                                }
                                #endregion
                            }
                            else
                            {
                                ShowMessage("页面模板实体还原失败，请检查配置是否正确！");
                            }
                            #endregion
                        }
                        else
                        {
                            ShowMessage("没有可以直接查看的分页内容！");
                        }
                    }
                    else
                    {
                        if (tptConfig.GenerateSinglePage == true)
                        {
                            #region 单页综合生成
                            PageTemplet pageTpt = (new PageTemplet()).GetInstanceById(tptConfig.MixedSingleTemplet.TempletID) as PageTemplet;
                            if (pageTpt != null && pageTpt.IsExist == true && pageTpt.IsFilled == true)
                            {
                                if (pageTpt.IsMixedSyntax == false)
                                {
                                    DisplayContent(HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, pageTpt.TempletRawContent));
                                }
                                else
                                {
                                    TempletParse tpp = new TempletParse();
                                    tpp.Caller = this;
                                    tpp.SetResourceDependency(aWebChannel);
                                    tpp.SetTaggedObjectCollection(pageTpt.TempletRawContent);
                                    DisplayContent(HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, tpp.ParsedResult));
                                }
                            }
                            else
                            {
                                ShowMessage("页面模板实体还原失败，请检查配置是否正确！");
                            }
                            #endregion
                        }
                        else
                        {
                            ShowMessage("没有可以直接查看的单页综合内容！");
                        }
                    }
                }
                else
                {
                    ShowMessage("频道资料无效！");
                }
            }
            else
            {
                //根据ID找到频道、 应用频道模板、分析模板内容输出
                if (Util.IsNumerical(Request.QueryString["ID"]))
                {
                    ViewNews(Int32.Parse(Request.QueryString["ID"]));
                }
                else
                {
                    ShowMessage("请指定要查看的动态内容！");
                }
            }
        }

        #endregion

        private void ViewNews(int newsid)
        {
            AccessWebNews news = new AccessWebNews(newsid);
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
                        ShowMessage("该频道没有配置模板，不能浏览动态内容！");
                    }
                    else
                    {
                        if (tptConfig.GenerateDetailPage == false)
                        {
                            ShowMessage("该频道模板设置关闭了详细内容显示，请先打开再浏览动态内容！");
                            return;
                        }

                        using (TempletParse tpp = new TempletParse())
                        {
                            PageTemplet pageTpt = (new PageTemplet()).GetInstanceById(tptConfig.DetailPageTemplet.TempletID) as PageTemplet;
                            tptConfig.DetailPageTemplet.FileNameConfig.FileNameTag.SetResourceDependency(news);

                            MultiResDependency newsRes = new MultiResDependency(newsChannel, news);
                            tpp.SetResourceDependency(newsRes);
                            tpp.SetTaggedObjectCollection(pageTpt.TempletRawContent);

                            string strGenFileContent = HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, tpp.ParsedResult);
                            DisplayContent(strGenFileContent);
                        }
                    }
                }
            }
            else
            {
                ShowMessage("该资讯无效！");
            }
            #endregion
        }

        private void ShowMessage(string msg)
        {
            System.Web.HttpContext.Current.Response.Write(msg);
        }

        private void DisplayContent(string content)
        {
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(content);
            System.Web.HttpContext.Current.Response.End();
        }

        #region IContainerCaller Members
        private string _pagedContentAlia;
        /// <summary>
        /// 模板内分页内容占位别名
        /// </summary>
        public string PagedContentAlia
        {
            get { return _pagedContentAlia; }
            set { _pagedContentAlia = value; }
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
