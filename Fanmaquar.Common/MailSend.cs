using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Fanmaquar.SmtpMail
{
    /// <summary>
    /// SMTP邮件发送
    /// </summary>
    public class MailSend
    {
        /// <summary>
        /// 默认发送配置
        /// </summary>
        static readonly SmtpConfig DefaultSmtpConfig = new SmtpConfig(Encoding.UTF8);

        /// <summary>
        /// 发送HTML格式邮件(UTF8)
        /// </summary>
        public static SendStatus MailTo(SmtpConfig config, MailAddress AddrFrom, MailAddress AddrTo, MailAddressCollection cc, MailAddressCollection bCC,
            string Subject, string BodyContent, bool isHtml, List<Attachment> attC)
        {
            MailMessage msg = new MailMessage(AddrFrom == null ? new MailAddress(config.FromAddress) : AddrFrom, AddrTo);
            msg.Priority = MailPriority.High;

            #region 抄送
            if (cc != null && cc.Count > 0)
            {
                foreach (MailAddress cAddr in cc)
                {
                    msg.CC.Add(cAddr);
                }
            }
            #endregion

            #region 密送
            if (bCC != null && bCC.Count > 0)
            {
                foreach (MailAddress cAddr in bCC)
                {
                    msg.Bcc.Add(cAddr);
                }
            }
            #endregion

            #region 附件列表
            if (attC != null && attC.Count > 0)
            {
                foreach (Attachment item in attC)
                {
                    msg.Attachments.Add(item);
                }
            }
            #endregion

            msg.Subject = Subject;
            msg.SubjectEncoding = config.ContentEncoding;
            msg.BodyEncoding = config.ContentEncoding;
            msg.IsBodyHtml = isHtml;
            msg.Body = BodyContent;
            SmtpClient client = new SmtpClient(config.SmtpServer, config.Port);
            if (config.Credentials != null)
                client.Credentials = config.Credentials;
            client.EnableSsl = config.SSLConnect;

            SendStatus status = new SendStatus();
            try
            {
                client.Send(msg);
                status.Success = true;
            }
            catch (Exception exp)
            {
                status.Message = exp.Message;
            }
            return status;
        }


        /// <summary>
        /// 发送HTML格式邮件
        /// </summary>
        /// <param name="config">SMTP配置</param>
        /// <param name="AddrFrom">发件人邮箱</param>
        /// <param name="AddrTo">收件人邮箱</param>
        /// <param name="Subject">主题</param>
        /// <param name="BodyContent">内容</param>
        /// <returns></returns>
        public static SendStatus MailTo(SmtpConfig config, MailAddress AddrFrom, MailAddress AddrTo, string Subject, string BodyContent)
        {
            return MailTo(config, AddrFrom, AddrTo, null, null, Subject, BodyContent, true, null);
        }

        /// <summary>
        /// 默认配置发送邮件
        /// </summary>
        /// <param name="AddrFrom">发件人邮箱</param>
        /// <param name="AddrTo">收件人邮箱</param>
        /// <param name="Subject">主题</param>
        /// <param name="BodyContent">内容</param>
        /// <param name="isHtml">是否是HTML格式邮件</param>
        /// <returns></returns>
        public static SendStatus MailTo(MailAddress AddrFrom, MailAddress AddrTo, string Subject, string BodyContent, bool isHtml)
        {
            return MailTo(DefaultSmtpConfig, AddrFrom, AddrTo, null, null, Subject, BodyContent, isHtml, null);
        }

        /// <summary>
        /// 采用系统配置的默认发件人发送邮件
        /// </summary>
        /// <param name="AddrFrom">发件人邮箱</param>
        /// <param name="AddrTo">收件人邮箱</param>
        /// <param name="Subject">主题</param>
        /// <param name="BodyContent">内容</param>
        /// <param name="isHtml">是否是HTML格式邮件</param>
        /// <returns>发送邮件状态</returns>
        public static SendStatus MailTo(MailAddress AddrTo, string Subject, string BodyContent, bool isHtml)
        {
            return MailTo(DefaultSmtpConfig, null, AddrTo, null, null, Subject, BodyContent, isHtml, null);
        }

    }

    /// <summary>
    /// 邮件发送状态
    /// </summary>
    [Serializable]
    public struct SendStatus
    {
        /// <summary>
        /// 发送是否成功
        /// </summary>
        public bool Success;

        /// <summary>
        /// 失败消息
        /// </summary>
        public string Message;
    }
}
