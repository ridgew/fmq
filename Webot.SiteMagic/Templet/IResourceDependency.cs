using System;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 依赖资源接口
    /// </summary>
    public interface IResourceDependency
    {
        /// <summary>
        /// 获取特定对象的定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>返回定义的对象</returns>
        object GetDefinition(string x);

        /// <summary>
        /// 返回特定对象是否有定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>是否定义过该对象</returns>
        bool IsDefined(string x);

        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        string DependencyIdentity { get; }
    }
}
