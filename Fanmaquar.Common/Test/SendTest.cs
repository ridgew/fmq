#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;

namespace Fanmaquar.SmtpMail.Test
{
    public class SendTest
    {

        public void GeneralSend()
        {
            SmtpConfig mailConfig = new SmtpConfig();

            SendStatus status = MailSend.MailTo(mailConfig,
                        new MailAddress("wangqj@Fanmaquar.com.cn", "王勤军", Encoding.Default),
                        new MailAddress("wangqj@Fanmaquar.com.cn", "王勤军", Encoding.Default),
                        null,
                        null,
                        "邮件测试",
                        "邮件内容",
                        true,
                        null);

            if (!status.Success)
            {
                //提示发送邮件错误
            }
        }

        public void SmtpConfigTest()
        {
            SmtpConfig mailConfig = new SmtpConfig().Protect();
        }

        public void EmailAddressTest()
        {
            MailAddress addr = new MailAddress("\"王勤军\"<wangqj@Fanmaquar.com.cn>");

            Console.Write(addr.DisplayName);
        }

    }
}
#endif
