using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;

namespace Webot.Common
{
    public static class ThumbnailUrl
    {
        /// <summary>
        /// 获取相关网址的缩略图
        /// </summary>
        /// <param name="UrlAddress">浏览资源的完整地址</param>
        /// <param name="browserWidth">浏览器的宽度</param>
        /// <param name="browserHeight">浏览器的高度</param>
        /// <returns>该资源的位图对象</returns>
        public static Bitmap GetUrlThumbnail(string UrlAddress, int browserWidth, int browserHeight)
        {
            return ThumbnailImageGenerator.GetWebSiteThumbnail(UrlAddress, browserWidth, browserHeight, 240, 320);
        }

        /// <summary>
        /// 按1024*768窗体获取缩略图
        /// </summary>
        /// <param name="urlAddress">浏览资源的完整地址</param>
        /// <returns>该资源地址的缩略图</returns>
        public static Bitmap GetUrlThumbnail(string urlAddress)
        {
            return GetUrlThumbnail(urlAddress, 1024, 768);
        }
    }

    public class ThumbnailImageGenerator
    {
        /// <summary>
        /// 生成相应网址的位图缩略图
        /// </summary>
        /// <param name="Url">相应网址</param>
        /// <param name="BrowserWidth">浏览器宽度</param>
        /// <param name="BrowserHeight">浏览器高度</param>
        /// <param name="ThumbnailWidth">缩略图宽度</param>
        /// <param name="ThumbnailHeight">缩略图高度</param>
        /// <example>
        /// 
        /// </example>
        public static Bitmap GetWebSiteThumbnail(string Url, int BrowserWidth, int BrowserHeight, int ThumbnailWidth, int ThumbnailHeight)
        {
            WebsiteThumbnailImage thumbnailGenerator = new WebsiteThumbnailImage(Url, BrowserWidth, BrowserHeight, ThumbnailWidth, ThumbnailHeight);
            return thumbnailGenerator.GenerateWebSiteThumbnailImage();
        }

        /// <summary>
        /// 网站缩略图生成
        /// </summary>
        private class WebsiteThumbnailImage
        {
            public WebsiteThumbnailImage(string Url, int BrowserWidth, int BrowserHeight, int ThumbnailWidth, int ThumbnailHeight)
            {
                this.m_Url = Url;
                this.m_BrowserWidth = BrowserWidth;
                this.m_BrowserHeight = BrowserHeight;
                this.m_ThumbnailHeight = ThumbnailHeight;
                this.m_ThumbnailWidth = ThumbnailWidth;
            }

            #region 相关属性
            private string m_Url = null;
            /// <summary>
            /// 网站完整网址
            /// </summary>
            public string Url
            {
                get
                {
                    return m_Url;
                }
                set
                {
                    m_Url = value;
                }
            }

            private Bitmap m_Bitmap = null;
            /// <summary>
            /// 浏览器绘图实体
            /// </summary>
            public Bitmap ThumbnailImage
            {
                get
                {
                    return m_Bitmap;
                }
            }

            private int m_ThumbnailWidth;
            /// <summary>
            /// 缩略图宽度(像素)
            /// </summary>
            public int ThumbnailWidth
            {
                get
                {
                    return m_ThumbnailWidth;
                }
                set
                {
                    m_ThumbnailWidth = value;
                }
            }

            private int m_ThumbnailHeight;
            /// <summary>
            /// 缩略图高度(像素)
            /// </summary>
            public int ThumbnailHeight
            {
                get
                {
                    return m_ThumbnailHeight;
                }
                set
                {
                    m_ThumbnailHeight = value;
                }
            }

            private int m_BrowserWidth;
            /// <summary>
            /// 浏览器宽度(像素)
            /// </summary>
            public int BrowserWidth
            {
                get
                {
                    return m_BrowserWidth;
                }
                set
                {
                    m_BrowserWidth = value;
                }
            }

            private int m_BrowserHeight;
            /// <summary>
            /// 浏览器高度(像素)
            /// </summary>
            public int BrowserHeight
            {
                get
                {
                    return m_BrowserHeight;
                }
                set
                {
                    m_BrowserHeight = value;
                }
            } 
            #endregion

            public Bitmap GenerateWebSiteThumbnailImage()
            {
                Thread m_thread = new Thread(new ThreadStart(_GenerateWebSiteThumbnailImage));
                m_thread.SetApartmentState(ApartmentState.STA);
                m_thread.Start();
                m_thread.Join();
                return m_Bitmap;
            }

            private void _GenerateWebSiteThumbnailImage()
            {
                WebBrowser m_WebBrowser = new WebBrowser();
                m_WebBrowser.ScriptErrorsSuppressed = false;
                m_WebBrowser.ScrollBarsEnabled = true;
                m_WebBrowser.Navigate(m_Url);
                m_WebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowser_DocumentCompleted);
                while (m_WebBrowser.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();
                m_WebBrowser.Dispose();
            }

            private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
            {
                WebBrowser m_WebBrowser = (WebBrowser)sender;
                //自适应窗口大小
                //scrollLeft, scrollTop, scrollWidth, scrollHeight, clientWidth, clientHeight
                //---------------------------------------------------------------------------------
                //Util.Debug(m_WebBrowser.Document.Body.GetAttribute("scrollHeight"));
                //Util.Debug(m_WebBrowser.Document.Body.GetAttribute("scrollWidth"));
                //-------------------------------------------------------------------------------
                //OleDbHelper.AppendToFile(@"D:\wwwroot\chinaqmzxco29607\upload\debug.txt", Environment.NewLine
                //    + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + ": "+ m_WebBrowser.Document.Url.ToString()
                //    + Environment.NewLine);
                //OleDbHelper.AppendToFile(@"D:\wwwroot\chinaqmzxco29607\upload\debug.txt",
                //    "scrollHeight = " + m_WebBrowser.Document.Body.GetAttribute("scrollHeight") + Environment.NewLine
                //    + "scrollWidth = " + m_WebBrowser.Document.Body.GetAttribute("scrollWidth") + Environment.NewLine);

                if (this.m_BrowserWidth != 0)
                {
                    this.m_BrowserWidth = int.Parse(m_WebBrowser.Document.Body.GetAttribute("scrollWidth"));
                }
                if (this.m_BrowserHeight != 0)
                { 
                    this.m_BrowserHeight = int.Parse(m_WebBrowser.Document.Body.GetAttribute("scrollHeight"));
                }

                m_WebBrowser.ClientSize = new Size(this.m_BrowserWidth, this.m_BrowserHeight);
                m_WebBrowser.ScrollBarsEnabled = false;
                m_Bitmap = new Bitmap(m_WebBrowser.Bounds.Width, m_WebBrowser.Bounds.Height);
                m_WebBrowser.BringToFront();
                m_WebBrowser.DrawToBitmap(m_Bitmap, m_WebBrowser.Bounds);
                //m_Bitmap = (Bitmap)m_Bitmap.GetThumbnailImage(m_ThumbnailWidth, m_ThumbnailHeight, null, IntPtr.Zero);
                m_Bitmap = (Bitmap)m_Bitmap.GetThumbnailImage(m_ThumbnailWidth, this.m_BrowserHeight, null, IntPtr.Zero);
            }
        }
    }

}
