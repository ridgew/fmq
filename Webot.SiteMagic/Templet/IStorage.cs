using System;
using System.Collections.Generic;
using System.Text;

namespace Webot.SiteMagic
{
    /// <summary>
    ///  存储库实现接口
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// 实例对象是否保存成功
        /// </summary>
        /// <param name="Instance">该实例对象</param>
        /// <returns>返回保存是否成功</returns>
        bool StoredSuccess(object Instance);

        /// <summary>
        /// 当前数据源是否准备就绪
        /// </summary>
        bool DataSourceIsReady { get; set; }

        /// <summary>
        /// 从存储设置恢复
        /// </summary>
        /// <remarks>
        /// (获取前，请检查数据源是否已经准备就绪，否则将返回为空对象。)
        /// </remarks>
        /// <returns>获取当前对象的相关实例</returns>
        object GetInstance();
    }
}
