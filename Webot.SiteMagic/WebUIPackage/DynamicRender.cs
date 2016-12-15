using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Webot.SiteMagic;
using Webot.Common;

namespace Webot.WebUIPackage
{
    /// <summary>
    /// 通过模板配置直接渲染输出
    /// </summary>
    public class DynamicRender : IHttpHandler, IContainerCaller, IDisposable
    {
        #region IHttpHandler Members

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            /***********************
             * 1.通过模板编号获取模板
             * 2.模板解析器解析输出
             *   2.1 设置模板解析依赖资源 TempletDriving
             *   2.2 解析内容并输出网页
             * 
             ***************************/
            TempletDriving tptRes = new TempletDriving(context);
            #region 页面渲染处理
            context.Response.Write(tptRes.GetDefinition("$TempletID$"));
            PageTemplet pageTpt = (new PageTemplet()).GetInstanceById(Convert.ToInt32(tptRes.GetDefinition("$TempletID$"))) as PageTemplet;
            if (pageTpt != null && pageTpt.IsExist == true && pageTpt.IsFilled == true)
            {
                #region 根据模板内的列表动态显示当前页内容
                if (pageTpt.IsMixedSyntax == false)
                {
                    DisplayContent(context, HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, pageTpt.TempletRawContent));
                    return;
                }
                else
                {

                    string realContent = "", myPageHtml = "";
                    TempletParse tpp = new TempletParse();
                    MultiResDependency res = new MultiResDependency(tptRes);

                    tpp.Caller = this;
                    tpp.SetResourceDependency(res);
                    tpp.SetTaggedObjectCollection(pageTpt.TempletRawContent);

                    //OleDbHelper.AppendToFile("~/debug.log", tpp.ParsedResult);
                    if (this.PagedObject == null)
                    {
                        myPageHtml = tpp.ParsedResult;
                        //+处理分页导航内容
                        if (this.PagerDic.Count > 0)
                        {
                            foreach (string pagerKey in PagerDic.Keys)
                            {
                                myPageHtml = myPageHtml.Replace(pagerKey, "");
                            }
                        }
                        DisplayContent(context, HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, myPageHtml));
                    }
                    else
                    {
                        string dynTpt = tpp.ParsedResult;
                        string currentPageMatch = "";
                        PagedObject.SetResourceDependency(res);
                        string currentPage = tptRes.GetDefinition("$CurrentPage$").ToString();
                        if (currentPage != null && Util.IsMatch(@"^(\d+)$", currentPage, 1, 1, PagedObject.GetPageCount(),
                            out currentPageMatch))
                        {
                            PagedObject.CurrentPageIndex = int.Parse(currentPage);
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

                                string formatDef = "$PagerFormat$";
                                if (tptRes.IsDefined(formatDef))
                                {
                                    Pager.DefaultPageFormat = tptRes.GetDefinition(formatDef).ToString();
                                }
                                myPageHtml = myPageHtml.Replace(pagerKey, Pager.ToString());
                            }
                        }
                        #endregion
                        DisplayContent(context, HtmlTextGenerator.GetRightHtmlText(pageTpt.SiteBaseDir, myPageHtml));
                    }
                    tpp.Dispose();
                }
                #endregion
            }
            else
            {
                ShowMessage(context, "页面模板实体还原失败，请检查配置是否正确！");
            }
            #endregion
        }

        #endregion

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
        /// <summary>
        /// 设置动态分页的显示信息
        /// </summary>
        /// <param name="alia">占位字符串键值</param>
        /// <param name="tag">当前分页信息的对象</param>
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.PagedObject != null) { this.PagedObject.free(); }
        }

        #endregion


        /// <summary>
        /// Displays the content.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        private void DisplayContent(HttpContext context, string content)
        {
            context.Response.Clear();
            context.Response.Write(content);
            context.Response.End();
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="msg">The MSG.</param>
        private void ShowMessage(HttpContext context, string msg)
        {
            context.Response.Write(msg);
        }
    }
}
