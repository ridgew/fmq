using System;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 能够通过ID填充实例对象
    /// </summary>
    public interface IFilledByID
    {
        /// <summary>
        /// 通过ID标志获取实例
        /// </summary>
        object GetInstanceById(int InstanceID);

        /// <summary>
        /// 判断是否已填充实体
        /// </summary>
        bool IsFilled { get; set; }
    }
}
