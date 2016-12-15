using System;

namespace Webot.SiteMagic
{
    public interface IContainerCaller
    {
        /// <summary>
        /// 设置分页内容临时占位字符，并传递相关可分页对象。
        /// </summary>
        /// <param name="alia">占位字符串（别名）</param>
        /// <param name="PagedInstance">可分页的对象</param>
        void SetDynamicPagedAlia(string alia, IPagedContent PagedInstance);

        /// <summary>
        /// 设置动态分页的显示信息
        /// </summary>
        /// <param name="alia">占位字符串键值</param>
        /// <param name="tag">当前分页信息的对象</param>
        void SetDynamicPagerDependency(string alia, PagerTag tag);
        
        /// <summary>
        /// 是否是顶级容器
        /// </summary>
        bool IsTopContainer();
    }
}
