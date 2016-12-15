using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using Webot.Common;

namespace Webot.SiteMagic.IO
{
    /// <summary>
    /// 文件上传封装
    /// (Coding By Ridge Wong)
    /// </summary>
    public class FileUp
    {
        /// <summary>
        /// 文件上传实例
        /// </summary>
        public FileUp()
        {
            this.msgCol = new NameValueCollection();
            this.strFileBasePath = "/upfile/";
            this.strEnableExt = "gif|jpg|mid|jar|jad";
            this.iMaxFileSize = 0x100000;
            this.iMinFileSize = 1;
            this.strFileFullPath = string.Empty;
            this.FileExt = "";
        }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string FileExt;
        /// <summary>
        /// 默认最大大小1M，以字节为单位。
        /// </summary>
        private int iMaxFileSize;
        /// <summary>
        /// 默认最小大小1字节，以字节为单位。
        /// </summary>
        private int iMinFileSize;
        /// <summary>
        /// 运行时消息集合
        /// </summary>
        private NameValueCollection msgCol;
        /// <summary>
        /// 默认允许的上传文件类型
        /// </summary>
        private string strEnableExt;
        /// <summary>
        /// 默认文件保存的基础路径
        /// </summary>
        private string strFileBasePath;
        /// <summary>
        /// 上传文件完整路径
        /// </summary>
        private string strFileFullPath;

        /// <summary>
        /// 删除本机文件，如果存在。
        /// </summary>
        /// <param name="filePath">文件相对路径</param>
        /// <returns>成功则返回字符0，否则返回异常的消息提示。</returns>
        public static string DeleteIfExists(string filePath)
        {
            string localPath = Util.ParseAppPath(filePath);
            string opResult = "0";
            try
            {
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
            }
            catch (Exception exp)
            {
                opResult = exp.Message;
            }
            return opResult;
        }

        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="strTopic">消息主题</param>
        /// <param name="strMsg">消息传递</param>
        private void AttachMessage(string strTopic, string strMsg)
        {
            if (this.msgCol[strTopic] == null)
            {
                this.msgCol.Add(strTopic, strMsg);
            }
            else
            {
                this.msgCol[strTopic] = strMsg;
            }
        }

        /// <summary>
        /// 检查是否是合法有效的文件上载
        /// </summary>
        /// <param name="file">传递HttpPostedFile对象</param>
        /// <returns>是否合法</returns>
        private bool CheckValidFile(HttpPostedFile file)
        {
            string text1 = this.GetPostFileExt(file);
            string text2 = ("|" + this.strEnableExt + "|").ToLower();
            if (text1 == string.Empty)
            {
                return false;
            }
            this.FileExt = text1.ToLower();
            int num1 = text2.IndexOf("|" + text1 + "|");
            long num2 = file.ContentLength;
            if ((num2 < this.iMinFileSize) && (num2 > this.iMaxFileSize))
            {
                this.AttachMessage("文件大小", string.Concat(new object[] { "文件大小不在限制范围(", this.iMinFileSize, "-", this.iMaxFileSize, "字节)之内。" }));
                return false;
            }
            return (num1 >= 0);
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="strTopic">消息主题</param>
        private void DetachMessage(string strTopic)
        {
            if (this.msgCol[strTopic] != null)
            {
                this.msgCol.Remove(strTopic);
            }
        }

        /// <summary>
        /// 获取所有运行时消息
        /// </summary>
        /// <returns>运行时消息</returns>
        private string GetMessage()
        {
            StringBuilder builder1 = new StringBuilder();
            string text1 = "{0}\uff1a{1}\n";
            for (int num1 = 0; num1 < this.msgCol.Count; num1++)
            {
                builder1.AppendFormat(text1, this.msgCol.GetKey(num1), this.msgCol[num1]);
            }
            return builder1.ToString();
        }

        /// <summary>
        /// 获取上传文件扩展名
        /// </summary>
        /// <param name="file">传递HttpPostedFile对象</param>
        /// <returns>返回文件的扩展名，gif、jad、bmp等。</returns>
        private string GetPostFileExt(HttpPostedFile file)
        {
            string text1 = file.FileName;
            int num1 = text1.LastIndexOf(".");
            if (num1 <= 0)
            {
                return "";
            }
            return text1.Substring(num1 + 1);
        }

        /// <summary>
        /// 返回当前时间的文本形式
        /// </summary>
        /// <returns>当前时刻的2005/11/23153524124格式</returns>
        public string GetTimeFileName()
        {
            return DateTime.Now.ToString("yyyy/MM/ddHHmmssfff").Replace("-", "/");
        }

        /// <summary>
        /// 返回当前时间的文本形式
        /// </summary>
        /// <param name="strDatetimeFormatInfo"></param>
        /// <returns>当前时刻的根据参数格式化的格式</returns>
        /// <remarks>
        /// <see href="ms-help://MS.VSCC.2003/MS.MSDNQTR.2003FEB.2052/cpref/html/frlrfsystemglobalizationdatetimeformatinfoclasstopic.htm"><c>DateTimeFormatInfo</c>类</see>
        /// </remarks>
        public string GetTimeFileName(string strDatetimeFormatInfo)
        {
            return DateTime.Now.ToString(strDatetimeFormatInfo);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">传递HttpPostedFile对象</param>
        /// <returns>上传文件的路径</returns>
        public string Upload(HttpPostedFile file)
        {
            //this.strFileFullPath = this.FileSaveDirectory + this.GetTimeFileName() + "." + GetPostFileExt(file);
            this.strFileFullPath = this.FileSaveDirectory 
                + Path.GetRandomFileName().Replace(".", "") 
                + "." + GetPostFileExt(file);
            return Upload(file, this.strFileFullPath);
        }

        /// <summary>
        /// 上传文件,并指定文件名
        /// </summary>
        /// <param name="file">传递HttpPostedFile对象</param>
        /// <param name="strFileFullPathName">文件完整保存路径，包含名称，包含扩展名。</param>
        /// <returns>上传文件的路径</returns>
        public string Upload(HttpPostedFile file, string strFileFullPathName)
        {
            string msg = "";
            if (!this.CheckValidFile(file))
            {
                this.AttachMessage("文件格式", "文件上传格式不符合要求，需要上传扩展名为(" + this.FileExtValid + ")的文件。");
                return msg;
            }

            this.strFileFullPath = strFileFullPathName;
            string localPath = HttpContext.Current.Server.MapPath(this.strFileFullPath);
            string dir = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
            try
            {
                file.SaveAs(localPath);
                msg = strFileFullPath;
            }
            catch (Exception exp) 
            {
                AttachMessage("保存文件出错", exp.Message);
            }
            return msg;
        }

        /// <summary>
        /// 文件所在文件夹
        /// </summary>
        public string FileDirectory
        {
            get
            {
                if (this.strFileFullPath != string.Empty)
                {
                    return Path.GetDirectoryName(this.strFileFullPath).Replace(@"\", "/");
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// 合法上传文件扩展名，用|分隔。（例：gif|jpg|mid，即允许上传gif、jpg和mid格式的文件。）
        /// </summary>
        public string FileExtValid
        {
            get
            {
                return this.strEnableExt;
            }
            set
            {
                this.strEnableExt = value;
            }
        }

        /// <summary>
        /// 文件完整路径
        /// </summary>
        public string FileFullPath
        {
            get
            {
                return this.strFileFullPath;
            }
        }

        /// <summary>
        /// 文件完整的文件名
        /// </summary>
        public string FileName
        {
            get
            {
                if (this.strFileFullPath != string.Empty)
                {
                    return Path.GetFileName(this.strFileFullPath);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// 文件保存的基础路径，默认为upfile。
        /// </summary>
        public string FileSaveDirectory
        {
            get
            {
                return this.strFileBasePath;
            }
            set
            {
                string text1 = value.ToString();
                if (!text1.StartsWith("/"))
                {
                    text1 = "/" + text1;
                }
                if (!text1.EndsWith("/"))
                {
                    text1 = text1 + "/";
                }
                this.strFileBasePath = text1;
            }
        }

        /// <summary>
        /// 限制最大大小
        /// </summary>
        public int MaxSize
        {
            get
            {
                return this.iMaxFileSize;
            }
            set
            {
                this.iMaxFileSize = value;
            }
        }

        /// <summary>
        /// 消息，通常为最近处理过程的结果
        /// </summary>
        public string Message
        {
            get
            {
                return this.GetMessage();
            }
        }

        /// <summary>
        /// 限制最小大小
        /// </summary>
        public int MinSize
        {
            get
            {
                return this.iMinFileSize;
            }
            set
            {
                this.iMinFileSize = value;
            }
        }

    }

}
