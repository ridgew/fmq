using System;
using System.Collections.Generic;
using System.Text;

namespace Webot.WebUIPackage
{
    /// <summary>
    /// 动态输出接口
    /// </summary>
    public interface IDynamicResponse
    {
        /// <summary>
        /// 使用路径信息传递参数
        /// </summary>
        bool UsePathInfoData { get; set; }

        /// <summary>
        /// 是否是分页/列表内容
        /// </summary>
        bool IsPagedContent { get; set; }

        /// <summary>
        /// 运行查询并获取内容输出
        /// </summary>
        /// <param name="qid">查询主标志</param>
        string ExecuteQuery(string qid);
    }
}
