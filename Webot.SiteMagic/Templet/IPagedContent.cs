using System;
using System.Collections.Generic;
using System.Text;

namespace Webot.SiteMagic
{
    public interface IPagedContent
    {
        /// <summary>
        /// 是否有下一页
        /// </summary>
        bool HasNextPage();

        /// <summary>
        /// 移动到下一页
        /// </summary>
        void MoveNextPage();

        /// <summary>
        /// 列表页数
        /// </summary>
        int GetPageCount();

        /// <summary>
        /// 当前分页所在页数
        /// </summary>
        int CurrentPageIndex { get; set; }

        /// <summary>
        /// 每页显示条数
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// 获取数据源的总记录条数
        /// </summary>
        int GetTotalRecordCount();

        /// <summary>
        /// 开始索引
        /// </summary>
        int StartIndex { get; }

        /// <summary>
        /// 结束索引
        /// </summary>
        int EndIndex { get; }

        /// <summary>
        /// 获取当前页的内容
        /// </summary>
        string GetCurrentPageContent();

        /// <summary>
        /// 获取依赖资源
        /// </summary>
        IResourceDependency GetResourceDependency();

        /// <summary>
        /// 设置依赖资源
        /// </summary>
        void SetResourceDependency(IResourceDependency value);

        /// <summary>
        /// 释放内存资源
        /// </summary>
        void free();
    }
}
