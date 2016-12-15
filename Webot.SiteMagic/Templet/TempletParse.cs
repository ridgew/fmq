/***************************
 * $Id: TempletParse.cs 216 2009-07-21 10:24:15Z fanmaquar@staff.vbyte.com $
 * $Author: fanmaquar@staff.vbyte.com $
 * $Rev: 216 $
 * $Date: 2009-07-21 18:24:15 +0800 (星期二, 21 七月 2009) $
 * ***************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using Webot.Common;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 模板解析类
    /// </summary>
    public class TempletParse : IDisposable, IContainerCaller
    {
        /// <summary>
        /// 模板解析初始化函数
        /// </summary>
        public TempletParse()
        { 
        
        }

        private IContainerCaller _caller;
        /// <summary>
        /// 设置上级调用容器
        /// </summary>
        public IContainerCaller Caller
        {
            get { return _caller; }
            set { _caller = value; }
        }

        /// <summary>
        /// 标签字典
        /// </summary>
        public Dictionary<string, string> TagTrack = new Dictionary<string, string>(); 

        /// <summary>
        /// 初始化模板并解析页面模板
        /// </summary>
        public TempletParse(PageTemplet tpt)
        {
            if (tpt.IsFilled && tpt.IsMixedSyntax)
            {
                SetTaggedObjectCollection(tpt.TempletRawContent);
            }
        }

        private string _parsedResult = "";
        /// <summary>
        /// 模板结果
        /// </summary>
        public string ParsedResult
        {
            get { return _parsedResult; }
            set { _parsedResult = value; }
        }

        private bool _generatorListContent = true;
        /// <summary>
        /// 解析时是否直接获取列表标签的内容(默认获取)
        /// </summary>
        public bool GeneratorListContent
        {
            get { return this._generatorListContent; }
            set { this._generatorListContent = value; }
        }

        /// <summary>
        /// 解析模板内容
        /// </summary>
        public void SetTaggedObjectCollection(string tptContent)
        {
            StringBuilder sb = new StringBuilder(tptContent.Length);
            Regex regEx = new Regex(TagBase.TagDefinitionPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            int idxBegin = 0, idxEnd = 0;

            //Util.Debug(false, "------------------------begin---------------");
            MatchCollection mc = regEx.Matches(tptContent, idxBegin);
            while (mc.Count > 0)
            {
                Match m = mc[0];
                idxEnd = m.Index;

                if (idxEnd > idxBegin) 
                {
                    //Util.Debug(false, tptContent.Substring(idxBegin, idxEnd - idxBegin));
                    sb.Append(tptContent.Substring(idxBegin, idxEnd - idxBegin)); 
                }

                TagBase tag = new TagBase(m.Value, m.Index, ref tptContent);
                //Util.Debug(false, "#####\n" + tag.OuterDefineText + " " + tag.Category.ToString() + "\n#####");
                //OleDbHelper.AppendToFile("~/debug.log", "parse Tag..." + tag.OuterDefineText + " \r\n " + tag.Category.ToString() + Environment.NewLine + Environment.NewLine);
                #region 处理标签内容
                if (tag.Category == TagCategory.CustomTag)
                {
                    DbStoredCustomTag dbTag = DbStoredCustomTag.Parse(tag);
                    dbTag.SetResourceDependency(this.GetResourceDependency());
                    //Util.Debug(false, "tpp: " + this.GetResourceDependency() + dbTag.OuterDefineText);
                    if (dbTag.IsExist == true)
                    {
                        dbTag.Caller = this;
                        sb.Append(dbTag.GetTagValue());
                        if (!this.TagTrack.ContainsKey("B" + dbTag.IDentity))
                        {
                            this.TagTrack.Add("B" + dbTag.IDentity, dbTag.PublishDate.ToString("yyyyMMddHHmmss"));
                        }
                    }
                }
                else if (tag.Category == TagCategory.DataListTag)
                {
                    //{#List:Repeater#}
                    ListTag list = ListTag.Parse(tag, this.GetResourceDependency());
                    //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " Res:" 
                    //    + this.GetResourceDependency().ToString());
                    if (this.GeneratorListContent == true)
                    {
                        #region 直接获取列表标签内容
                        if (list.GetPageCount() < 2)
                        {
                            sb.Append(list.GetTagValue());
                        }
                        else
                        {
                            string pagedAliaContent = (string.IsNullOrEmpty(list.IDentity)) ? new Guid().ToString() : list.IDentity;
                            SetDynamicPagedAlia(pagedAliaContent, list);
                            sb.Append(pagedAliaContent);
                        }
                        #endregion
                    }
                    else
                    {
                        //设置列表标签占位符号
                        string pagedAliaContent = (string.IsNullOrEmpty(list.IDentity)) ? new Guid().ToString() : list.IDentity;
                        SetDynamicPagedAlia(pagedAliaContent, list);
                        sb.Append(pagedAliaContent);
                    }
                }
                else if (tag.Category == TagCategory.PagerTag)
                {
                    //{#Pager#}
                    PagerTag pager = PagerTag.Parse(tag);
                    string pagerAlia = (string.IsNullOrEmpty(pager.IDentity)) ? new Guid().ToString() : pager.IDentity;
                    SetDynamicPagerDependency(pagerAlia, pager);
                    sb.Append(pagerAlia);
                }
                else if (tag.Category == TagCategory.ServerTag)
                {
                    //服务端标签定义格式为 
                }
                else if (tag.Category == TagCategory.SystemTag)
                {
                    //系统标签(函数)定义格式为 {#$ $#} {# ... () ... #}
                    // 系统标签： {#$this.PrimaryNews["Content"]$#}
                    if (tag.OuterDefineText.StartsWith("{#$this.") && this._resDependency != null)
                    {
                        sb.Append(this.GetResourceDependency().GetDefinition(tag.OuterDefineText));
                    }
                    else
                    {
                        SystemTag sysTag = new SystemTag(tag.TagDefinition);
                        sysTag.SetResourceDependency(GetResourceDependency());
                        sb.Append(sysTag.ToString());
                    }
                    //Util.Debug(false, tag.OuterDefineText);
                    //OleDbHelper.AppendToFile("~/log.txt", tag.OuterDefineText);
                }
                else if (tag.Category == TagCategory.DefineTag)
                {
                    //数据库标签定义格式为 {#%FieldName%#}
                    if (this._resDependency != null)
                    {
                        //Util.Debug(false, tag.OuterDefineText);
                        sb.Append(this.GetResourceDependency().GetDefinition(tag.OuterDefineText));
                    }
                }

                #endregion

                idxBegin = tag.DefinedIndexEnd;
                //Util.Debug(false, tptContent.Substring(idxBegin, 5));
                //Util.Debug(m.Value);
                mc = regEx.Matches(tptContent, idxBegin);
            }
            if (idxBegin < tptContent.Length)
            {
                sb.Append(tptContent.Substring(idxBegin));
                //Util.Debug(false, tptContent.Substring(idxBegin));
            }
            this.ParsedResult = sb.ToString();
            //Util.Debug(false, "------------------------End---------------");
            //OleDbHelper.SetTextFileContent(@"~/@docs/temp.html", "utf-8",
            //    HtmlTextGenerator.GetRightHtmlText("/templet/1/", sb.ToString()));
        }

        private IResourceDependency _resDependency = null;
        /// <summary>
        /// 获取该模板所依赖的资源
        /// </summary>
        public IResourceDependency GetResourceDependency()
        {
            return this._resDependency;
        }

        /// <summary>
        /// 设置该模板所依赖的资源
        /// </summary>
        public void SetResourceDependency(IResourceDependency value)
        {
            this._resDependency = value;
        }


        #region IDisposable Members
        /// <summary>
        /// 释放占用资源
        /// </summary>
        public void Dispose()
        {
            TagTrack = null;
            this._resDependency = null;
            this._parsedResult = null;
        }

        #endregion

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
