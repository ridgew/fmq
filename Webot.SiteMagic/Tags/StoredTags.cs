using System;
using System.Collections.Generic;
using System.Text;
using Webot.Common;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 需要持久存储的标签
    /// </summary>
    public abstract class StoredTags : TagBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoredTags"/> class.
        /// </summary>
        public StoredTags()
            : base()
        { }

        /// <summary>
        /// 从字符串实例化标签定义
        /// </summary>
        /// <param name="tagdef">标签定义文本</param>
        public StoredTags(string tagDefine)
            : base(tagDefine)
        { }

        /// <summary>
        /// 从模板中加载标签定义
        /// </summary>
        /// <param name="tagdefStart">标签初始定义文本</param>
        /// <param name="idxStart">标签定义开始索引</param>
        /// <param name="SourceText">模板定义原始文本源</param>
        public StoredTags(string tagdefStart, int idxStart, ref string SourceText)
            : base(tagdefStart, idxStart, ref SourceText)
        { }

        /// <summary>
        /// 标签存储辅助
        /// </summary>
        protected OleDbHelper tagStoreHelper = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey);

        /// <summary>
        /// 通过连接字符串键值获取数据操作辅助对象
        /// </summary>
        /// <param name="connKey">The conn key.</param>
        /// <returns></returns>
        protected OleDbHelper getStoreHelper(string connKey)
        {
            return FanmaquerOleDbModule.GetOleDbInstance(connKey);
        }

    }
}
