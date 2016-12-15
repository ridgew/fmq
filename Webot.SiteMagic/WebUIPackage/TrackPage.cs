using System;
using System.Text.RegularExpressions;
using System.Web;
using Webot.SiteMagic;
using Webot.Common;

namespace Webot.WebUIPackage
{
    public class TrackPage : System.Web.UI.Page
    {
        public virtual void Page_Load(object sender, EventArgs e)
        {
            return;
            //禁用汇报跟踪
            if (GetSiteDisableTrackReport() == true) return;

            Response.Clear();
            //---
            // /js/T8!20080125213802-B5!20080124152852-B16!20080124212750-B15!20080125144712
            // -B14!20080116150830-B22!20080115003756-B51!20080125211333-B21!20080124164615
            // -B8!20080124163840-B10!20080124165956-B4!20080124152729
            // %2fNews%2f55%2fShow-4.html
            // 
            string data = Request.PathInfo;
            if (data == null) return;
            // ^/(img|js)/(((T|B)(\d+)(!)(\d{14})(-?)){1,})
            string matchPattern = "^/(img|js)/(((T|B)(\\d+)(!)(\\d{14})(-?)){1,})";
            Match m = Regex.Match(data, matchPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string trackPath = "";
            if (!m.Success)
            {
                return;
            }
            else
            {
                trackPath = data.Substring(m.Length);

                if (string.Compare(m.Groups[1].Value, "img", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    DoTrack(m.Groups[2].Value, trackPath, false);
                    Response.AddHeader("Content-Length", "43"); //43bytes
                    Response.AddHeader("Last-Modified", "Fri, 25 Jan 2008 06:47:38 GMT");
                    Response.ContentType = "image/gif";
                    Response.BinaryWrite(Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw=="));
                }
                else
                {
                    Response.AddHeader("Last-Modified", "Fri, 25 Jan 2008 06:47:38 GMT");
                    Response.AddHeader("Content-Type", "application/x-javascript");
                    Response.Charset = "utf-8";
                    DoTrack(m.Groups[2].Value, trackPath, true);
                }
            }
            Response.End();
        }

        /// <summary>
        /// 站点是否配置为禁止更新跟踪汇报
        /// </summary>
        /// <returns></returns>
        public static bool GetSiteDisableTrackReport()
        {
            return (HttpContext.Current.Application["$MagicTrack$"] != null
                    && Convert.ToBoolean(HttpContext.Current.Application["$MagicTrack$"]) == true);
        }

        /// <summary>
        /// 站点是否配置为允许自动更新
        /// </summary>
        /// <returns></returns>
        public static bool GetSiteEnableAutoUpdate()
        {
            if (Util.GetAppConfig("MagicTrack-AutoUpdate") != null)
            {
                return Convert.ToBoolean(Util.GetAppConfig("MagicTrack-AutoUpdate"));
            }
            else
            {
                return (HttpContext.Current.Application["$MagicTrack-AutoUpdate$"] != null
                    && Convert.ToBoolean(HttpContext.Current.Application["$MagicTrack-AutoUpdate$"]));
            }
        }

        /// <summary>
        /// 设置应用程序内置数据
        /// </summary>
        /// <param name="key">数据键值</param>
        /// <param name="value">相关对象数据</param>
        public static void SetApplicationState(string key, object value)
        {
            HttpApplicationState appState = HttpContext.Current.Application;
            appState.Lock();
            appState[key] = value;
            appState.UnLock();
        }

        /// <summary>
        /// 设置是否允许更新汇报
        /// </summary>
        public static void SetSiteTrackUpdate(bool enable)
        {
            SetApplicationState("$MagicTrack$", enable);
        }

        /// <summary>
        /// 设置是否允许自动更新
        /// </summary>
        /// <param name="enable">自动更新是否开启</param>
        public static void SetAutoUpdate(bool enable)
        {
            SetApplicationState("$MagicTrack-AutoUpdate$", enable);
        }

        private void DoTrack(string data, string trackPath, bool writeJs)
        {
            string[] objCmpDat = data.Split('-');
            int idx = 0;
            TrackUnit unit = null;
            foreach (string d in objCmpDat)
            {
                idx = d.IndexOf('!');
                if (idx > 0)
                {
                    unit = new TrackUnit(d.Substring(0, idx), d.Substring(idx + 1));
                    if (unit.IsUpdatable() == true)
                    {
                        unit.SetRecord(trackPath);
                        if (GetSiteEnableAutoUpdate() == true)
                        {
                            PageGenerator.GeneratorByPath(trackPath);
                            //if (writeJs == true)
                            //{
                            //    Response.Write("location.href = location.href;");
                            //}
                        }
                        break;
                    }
                }
            }

            if (writeJs == true)
            {
                //Response.Write("alert('" + trackPath + "');");
            }
        }
    }
}
