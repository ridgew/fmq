using System;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 文件名标签对象
    /// </summary>
    public class FileNameTag : TagBase
    {
        /// <summary>
        /// 文件名标签对象
        /// </summary>
        public FileNameTag() : base ()
        { 
        }

        public FileNameTag(string tagdef) : base (tagdef)
        { 
        
        }

        /// <summary>
        /// 获取文件名标签的最终值
        /// </summary>
        /// <returns></returns>
        public override object GetTagValue()
        {
            return base.GetTagValue();
        }
    }
}
