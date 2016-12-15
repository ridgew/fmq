using System;
using Webot.SiteMagic;

namespace Webot.DataAccess
{
    public interface IWebChannel : IResourceDependency, IStorage
    {
        /// <summary>
        /// 归档天数
        /// </summary>
        int ArchiveDays { get; set; }

        /// <summary>
        /// 频道编号（系统内不重复）
        /// </summary>
        int ChannelID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ChannelName { get; set; }

        /// <summary>
        /// 频道默认新闻
        /// </summary>
        IWebNews GetPrimaryNews();

        /// <summary>
        /// 是否是外部链接
        /// </summary>
        bool IsOuterLink { get; set; }

        /// <summary>
        /// 外部链接URL地址
        /// </summary>
        string OuterLinkUrl { get; set; }

        /// <summary>
        /// 上级频道
        /// </summary>
        IWebChannel ParentChannel { get; set; }

        /// <summary>
        /// 设置定义键值对象
        /// </summary>
        void SetDefined(string key, object value);

        /// <summary>
        /// 同级排序
        /// </summary>
        int Sort { get; set; }

        /// <summary>
        /// 静态物理文件生成基础路径，需要以'/'结尾。
        /// </summary>
        string StaticFileGenDir { get; set; }

        /// <summary>
        /// 模板定义设置
        /// </summary>
        TempletSetting TempletConfig { get; set; }
    }
}
