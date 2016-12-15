using System;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 文件名生成设置
    /// </summary>
    [Serializable]
    public class FileNameSetting
    {
        public FileNameSetting() { }

        private FileNameTag _fileNameTag;
        /// <summary>
        /// 文件名标签定义
        /// </summary>
        public FileNameTag FileNameTag
        {
            get { return _fileNameTag; }
            set { _fileNameTag = value; }
        }


        private string _fileExtentionName = ".html";
        /// <summary>
        /// 扩展名
        /// </summary>
        public string FileExtentionName
        {
            get { return _fileExtentionName; }
            set { _fileExtentionName = value; }
        }


    }
}
