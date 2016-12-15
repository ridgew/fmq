using System;
using System.Collections.Generic;
using System.Text;
using Webot.SiteMagic;

namespace Webot.WebUIPackage
{
    /// <summary>
    /// 动态分页/列表内容输出
    /// </summary>
    public abstract class DynamicPagedContentBase : DynamicPageBase, IContainerCaller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPagedContentBase"/> class.
        /// </summary>
        public DynamicPagedContentBase()
            : base()
        { 
            
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

        private IPagedContent _pagedObject;
        /// <summary>
        /// 可分页对象
        /// </summary>
        public IPagedContent PagedObject
        {
            get { return _pagedObject; }
            set { _pagedObject = value; }
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
        /// 定义的分页对象字典
        /// </summary>
        public Dictionary<string, PagerTag> PagerDic = new Dictionary<string, PagerTag>();
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

        /// <summary>
        /// 是否是顶级容器
        /// </summary>
        public bool IsTopContainer()
        {
            return true;
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.PagedObject != null) { this.PagedObject.free(); }
            PagerDic = null;
        }
    }
}
