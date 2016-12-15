using System;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 内部标签定义
    /// </summary>
    public interface IInternalTag
    {
        /// <summary>
        /// 标签定义文本
        /// </summary>
        string TagDefinition { get; set; }

        /// <summary>
        /// 是否有依赖性
        /// </summary>
        bool IsDependencyTag { get; }

        /// <summary>
        /// 获取该标题所依赖的资源
        /// </summary>
        /// <returns></returns>
        IResourceDependency GetResourceDependency();
        /// <summary>
        /// 设置该标题所依赖的资源
        /// </summary>
        /// <returns></returns>
        void SetResourceDependency(IResourceDependency value);
    }
}
