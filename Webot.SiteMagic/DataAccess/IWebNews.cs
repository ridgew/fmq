using System;
using Webot.SiteMagic;

namespace Webot.DataAccess
{
    public interface IWebNews : IResourceDependency, IStorage
    {
        /// <summary>
        /// 新闻作者
        /// </summary>
        string Author { get; set; }

        /// <summary>
        /// 新闻频道/栏目
        /// </summary>
        IWebChannel Channel { get; set; }

        /// <summary>
        /// 新闻内容
        /// </summary>
        string Content { get; set; }

        /// <summary>
        /// 点击次数
        /// </summary>
        int Hits { get; set; }

        /// <summary>
        /// 是否所属频道下的默认新闻
        /// </summary>
        bool IsPrimary { get; set; }

        /// <summary>
        /// 是否已发布的新闻
        /// </summary>
        bool IsPubed { get; set; }

        /// <summary>
        /// 新闻编号(不重复)
        /// </summary>
        int NewsID { get; set; }

        /// <summary>
        /// 更新虚拟文件路径
        /// </summary>
        void RefreshVirtualPath();

        /// <summary>
        /// 新闻同类排序
        /// </summary>
        int Sort { get; set; }

        /// <summary>
        /// 新闻摘要
        /// </summary>
        string Summary { get; set; }

        /// <summary>
        /// 发布修改时间
        /// </summary>
        DateTime TimeFlag { get; set; }

        /// <summary>
        ///  新闻标题
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// 新闻内容虚拟/物理文件相对地址
        /// </summary>
        string VirtualPath { get; set; }
    }
}
