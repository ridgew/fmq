using System;
using System.Collections.Generic;
using System.Text;

namespace Webot.Common
{
    /// <summary>
    /// 版本接口
    /// </summary>
    public interface IVersion
    {
        /// <summary>
        /// 获取当前应用对象的版本,主+次+再次+修订版本,如:1.0.0.253.
        /// </summary>
        Version CurrentVersion { get; }
    }
}
