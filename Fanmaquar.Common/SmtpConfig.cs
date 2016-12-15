using System;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Reflection;
using System.Text;

namespace Fanmaquar.SmtpMail
{
    /// <summary>
    /// Smtp配置
    /// </summary>
    public class SmtpConfig
    {
        /// <summary>
        /// SMTP在配置文件中的默认配置
        /// </summary>
        public SmtpConfig()
            : this(Encoding.Default)
        {

        }

        /// <summary>
        /// SMTP在配置文件中的默认配置
        /// </summary>
        /// <param name="contentEncoding">指定内容编号方式</param>
        public SmtpConfig(Encoding contentEncoding)
        {
            setWebConfigBindding();
            ContentEncoding = contentEncoding;
        }

        private void setWebConfigBindding()
        {
            System.Configuration.Configuration config = getBaseConfig();
            MailSettingsSectionGroup sectionGroup = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
            if (sectionGroup == null)
            {
                SmtpServer = "localhost";
                Port = 25;
                SSLConnect = false;
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(sectionGroup.Smtp.Network.Host))
                        SmtpServer = sectionGroup.Smtp.Network.Host;
                    Port = sectionGroup.Smtp.Network.Port;
                    UserName = sectionGroup.Smtp.Network.UserName;
                    Password = sectionGroup.Smtp.Network.Password;
                    if (!string.IsNullOrEmpty(sectionGroup.Smtp.From))
                        FromAddress = sectionGroup.Smtp.From;

                    if (sectionGroup.Smtp.Network.DefaultCredentials == true)
                    {
                        Credentials = null;
                    }
                    else
                    {
                        Credentials = new NetworkCredential(UserName, Password);
                    }
                }
                catch (Exception configEx)
                {
                    throw configEx;
                }
            }
        }

        /// <summary>
        /// 发送邮件服务器
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// 服务器连接端口，默认为25。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 连接用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 连接密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 发件人信息
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// 是否是安全套接字连接，默认为否。
        /// </summary>
        public bool SSLConnect { get; set; }

        /// <summary>
        /// 邮件内容编码
        /// </summary>
        public Encoding ContentEncoding { get; set; }

        /// <summary>
        /// 访问凭据
        /// </summary>
        public NetworkCredential Credentials { get; set; }

        /// <summary>
        /// 保护该配置节点
        /// </summary>
        /// <returns></returns>
        public SmtpConfig Protect()
        {
            protectSection(true);
            return this;
        }

        System.Configuration.Configuration config = null;
        System.Configuration.Configuration getBaseConfig()
        {
            if (config != null)
                return config;

            if (System.Web.HttpContext.Current != null)
            {
                config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
            }
            else
            {
                Assembly asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                string appPath = asm.Location;
                if (System.IO.File.Exists(appPath))
                    config = ConfigurationManager.OpenExeConfiguration(appPath);
            }
            return config;
        }

        void protectSection(bool isProtect)
        {
            System.Configuration.Configuration config = getBaseConfig();
            ConfigurationSection section = config.GetSection("system.net/mailSettings/smtp");
            if (section != null)
            {
                if (isProtect)
                {
                    if (!section.SectionInformation.IsProtected)
                        section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                }
                else
                {
                    if (section.SectionInformation.IsProtected)
                        section.SectionInformation.UnprotectSection();
                }
                config.Save();
            }
        }

        /// <summary>
        /// 解除保护该配置节点
        /// </summary>
        /// <returns></returns>
        public SmtpConfig UnProtect()
        {
            protectSection(false);
            return this;
        }
    }

}
