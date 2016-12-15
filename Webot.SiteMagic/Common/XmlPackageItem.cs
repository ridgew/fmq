using System;

namespace Webot.Common
{
    [Serializable]
    public class XmlPackageItem : IVersion
    {
        /// <summary>
        /// Xml资源包项目(无初始化)
        /// </summary>
        public XmlPackageItem() { }

        /// <summary>
        /// Xml资源包项目
        /// </summary>
        /// <param name="RawText">文本资源项</param>
        public XmlPackageItem(string RawText)
        {
            this.RawTextContent = RawText;
            this._contentType = "text/plain";

            this._size = this.BinaryData.LongLength;
            this._packageID = (new Guid()).ToString();
            this._generatorPath = this._lastModifiedTime + ".txt";
            this._lastModifiedTime = DateTime.Now;
        }

        #region 项目属性
        private bool _IsBinaryData = false;
        /// <summary>
        /// 是否是二进制数据
        /// </summary>
        public bool IsBinaryData
        {
            get { return _IsBinaryData; }
            set { _IsBinaryData = value; }
        }


        private byte[] _binData = null;
        /// <summary>
        /// 二进制数据
        /// </summary>
        public byte[] BinaryData
        {
            get 
            {
                if (this._binData != null)
                {
                    return this._binData;
                }
                else
                {
                    if (this._RawTextContent != null)
                    {
                        this._binData = System.Text.Encoding.UTF8.GetBytes(this._RawTextContent);
                        return this._binData;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                this._binData = value;
            }
        }

        private string _contentType = "application/octet-stream";
        /// <summary>
        /// 资源类型(MIME)
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        private long _size = 0;
        /// <summary>
        /// 资源大小
        /// </summary>
        public long Size
        {
            get { return _size; }
            set { _size = value; }
        }

        private string _base64Content;
        /// <summary>
        /// Base64编码之后的内容
        /// </summary>
        public string Base64Content
        {
            get 
            {
                if (this._base64Content != null)
                {
                    return this._base64Content;
                }
                else if(this.BinaryData != null)
                {
                    this._base64Content = System.Convert.ToBase64String(this.BinaryData);
                    return _base64Content;
                }
                else
                {
                    return "";
                }
            }
            set { _base64Content = value; }
        }

        private string _RawTextContent;
        /// <summary>
        /// 原始文本内容
        /// </summary>
        public string RawTextContent
        {
            get
            {
                if (this._RawTextContent == null && this._base64Content != null)
                {
                    byte[] bytes = Convert.FromBase64String(this._base64Content);
                    this._RawTextContent = System.Text.Encoding.UTF8.GetString(bytes);
                }
                return _RawTextContent;
            }
            set { _RawTextContent = value; }
        }

        private string _generatorPath = "";
        /// <summary>
        /// 生成文件的相对路径
        /// </summary>
        public string GeneratorPath
        {
            get { return _generatorPath; }
            set { _generatorPath = value; }
        }

        private string _packageID;
        /// <summary>
        /// 资源标志ID
        /// </summary>
        public string PackageID
        {
            get { return _packageID; }
            set { _packageID = value; }
        }

        private DateTime _lastModifiedTime;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime LastModifiedTime
        {
            get { return _lastModifiedTime; }
            set { _lastModifiedTime = value; }
        }

        private string _charset = "utf-8";
        /// <summary>
        /// 字符集
        /// </summary>
        public string Charset
        {
            get { return _charset; }
            set { _charset = value; }
        } 
        #endregion


        #region IVersion Members
        /// <summary>
        /// 当前对象版本
        /// </summary>
        public Version CurrentVersion
        {
            get { return new Version(0, 0, 0, 1); }
        }

        #endregion
    }
}
