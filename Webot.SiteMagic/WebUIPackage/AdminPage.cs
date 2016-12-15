using System;
using System.Data;
using System.Web;
//using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using Webot.Common;
using Webot.SiteMagic;

namespace Webot.WebUIPackage
{
    /// <summary>
    /// AdminPage 的摘要说明。
    /// 后台管理页面基类
    /// </summary>
    public class AdminPage : Page
    {
        private string username = "";
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get { return username; }
            set { username = value; }
        }
        
        /// <summary>
        /// 控件初始化时计算执行时间
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            m_processtime = (Environment.TickCount - m_starttick) / 1000;
            base.OnInit(e);
        }


        /// <summary>
        /// 得到当前页面的载入时间供模板中调用(单位:毫秒)
        /// </summary>
        /// <returns>当前页面的载入时间</returns>
        public float Processtime
        {
            get { return m_processtime; }
        }

        /// <summary>
        /// 当前页面开始载入时间(毫秒)
        /// </summary>
        public float m_starttick = Environment.TickCount;

        /// <summary>
        /// 当前页面执行时间(毫秒)
        /// </summary>
        public float m_processtime;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminPage"/> class.
        /// </summary>
        public AdminPage() : base ()
        {
            if (!IsLogon())
            {
                HttpContext.Current.Response.Redirect(Util.GetAppConfig("SystemLoginURL"), true);
                return;
            }
            else
            {
                this.username = GetUserName();
                if (!IsBackgroundManager())
                {
                    //Response.Clear();
                    //Response.Write("");
                    //Response.End();
                }
            }
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        public static string GetVeifyCode()
        {
            if (HttpContext.Current.Session["WebotVerifyCode"] == null)
            {
                return "";
            }
            else
            {
                return HttpContext.Current.Session["WebotVerifyCode"].ToString();
            }
        }

        /// <summary>
        /// 登录控制
        /// </summary>
        public static string TryLogin(string username, string password, string verifyCode, string domain)
        {
            if (verifyCode != GetVeifyCode()) return "验证码不正确";
            OleDbHelper hp = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey);
            string sql = "select top 1 UserName,IsManager,LoginTimes,LastLogon,LastLoginIP from Tbl_Users where Disabled=false and UserName='"
             + OleDbHelper.EscapeSQL(username) + "' and Pwd='" + Util.Md5(password) + "'";
            //Util.Debug(sql);
            DataRow mRow = hp.GetDataRow(sql);
            if (mRow == null)
            {
                return "登录名或密码不正确，或者该帐号被系统禁止登录。";
            }
            else
            {

                sql = string.Format("update Tbl_Users set LoginTimes=LoginTimes+1,LastLogon=Now(),LastLoginIP='{1}' where Username='{0}'",
                    OleDbHelper.EscapeSQL(username),
                    Util.GetIP());
                hp.ExecuteNonQuery(sql);

                LoginUserData uDat = new LoginUserData
                {
                    UserName = username,
                    IsLogon = true,
                    IsManager = Convert.ToBoolean(mRow["IsManager"])
                };

                FormsAuthenticationTicket tkt = new FormsAuthenticationTicket(1,
                    username, DateTime.Now, DateTime.Now.AddDays(1.00), false,
                    ToJSON(uDat));
                HttpCookie mCookie = new HttpCookie(FormsAuthentication.FormsCookieName);
                mCookie.Value = FormsAuthentication.Encrypt(tkt);
                mCookie.Expires = System.DateTime.Now.AddDays(1.00);
                if (domain != null)
                {
                    mCookie.Domain = domain;
                }
                HttpContext.Current.Response.Cookies.Add(mCookie);
                return "0";
            }
        }

        /// <summary>
        /// 是否已经登录
        /// </summary>
        public static bool IsLogon()
        {
            return HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] != null;

            bool isExist = (HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] != null);
            if (isExist == false)
            {
                return false;
            }
            else
            {
                bool isValidExist = true;
                HttpCookie uCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (string.IsNullOrEmpty(uCookie.Value))
                {
                    isValidExist = false;
                }
                else
                {
                    try
                    {
                        FormsAuthenticationTicket tkt = FormsAuthentication.Decrypt(uCookie.Value);
                    }
                    catch (Exception)
                    {
                        isValidExist = false;
                    }
                }
                return isValidExist;
            }
        }

        /// <summary>
        /// 是否是后台管理员、还是普通登录用户
        /// </summary>
        public static bool IsBackgroundManager()
        { 
            if (IsLogon() == false) return false;
            return true;

            //HttpCookie uCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            //FormsAuthenticationTicket tkt = FormsAuthentication.Decrypt(uCookie.Value);
            //LoginUserData uData = LoadFromJson<LoginUserData>(tkt.UserData);
            //return uData.IsManager;
        }

        /// <summary>
        /// 登录之后的用户名
        /// </summary>
        public static string GetUserName()
        {
            if (IsLogon() == false) return "";
            HttpCookie uCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket tkt = FormsAuthentication.Decrypt(uCookie.Value);
            return tkt.Name;
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <param name="returnUrl">注销之后转向地址</param>
        public static void Logout(string returnUrl)
        {
            Logout(returnUrl, null);
        }

        /// <summary>
        /// 注销登录（域）
        /// </summary>
        /// <param name="returnUrl">注销之后转向地址</param>
        /// <param name="domainName">注销域</param>
        public static void Logout(string returnUrl, string domainName)
        {
            HttpCookie mCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (mCookie != null)
            {
                if (domainName != null) mCookie.Domain = domainName;
                mCookie.Expires = System.DateTime.Now.AddDays(-10.0);
                HttpContext.Current.Response.Cookies.Set(mCookie);
            }
            HttpContext.Current.Response.Redirect(returnUrl, true);
        }

        /// <summary>
        /// 从JSON数据获取对象实例
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="jsonData">JSON序列化字符</param>
        /// <returns></returns>
        public static T LoadFromJson<T>(string jsonData)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            return s.Deserialize<T>(jsonData);
        }

        /// <summary>
        /// 将对象转换成JSON字符串
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(object obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

    }

    /// <summary>
    /// 登录用户数据
    /// </summary>
    [Serializable]
    public struct LoginUserData
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is logon.
        /// </summary>
        /// <value><c>true</c> if this instance is logon; otherwise, <c>false</c>.</value>
        public bool IsLogon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is manager.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is manager; otherwise, <c>false</c>.
        /// </value>
        public bool IsManager { get; set; }
    }
}
