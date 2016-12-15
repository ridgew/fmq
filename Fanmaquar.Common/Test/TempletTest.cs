#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Fanmaquar.SmtpMail.Test
{
    public class TempletTest
    {
        public void TestAll()
        {
            SimpleFormatTest();

            SubPropertyFormat();

            SubPropertyIndexFormat();

            SubComplexPropertyIndexFormat();

            SubComplex2Format();
        }

        public void SimpleFormatTest()
        {
            string htmlContent = new MailTemplet(@"Hello，{userName}!").SetVariable("username", "wangqj")
                .ToHtmlContent();

            //Console.Write(htmlContent);
            Debug.Assert(htmlContent.Equals("Hello，wangqj!"));
        }

        public void SubPropertyFormat()
        {
            TempletObject obj = new TempletObject { IpAddress = "192.168.8.91", UserName = "wangqj" };

            string htmlContent = new MailTemplet(@"Hello，{user.IpAddress.Length}!").SetVariable("user", obj)
                .ToHtmlContent();

            //Console.Write(htmlContent);
            Debug.Assert(htmlContent.Equals("Hello，12!"));
        }

        public void SubPropertyIndexFormat()
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Set("IP", "192.168.8.91");
            nv.Set("UserName", "wangqj");

            string htmlContent = new MailTemplet(@"Hello，{Request[""IP""]}!").SetVariable("Request", nv)
               .ToHtmlContent();

            //Console.Write(htmlContent);
            Debug.Assert(htmlContent.Equals("Hello，192.168.8.91!"));
        }

        public void SubComplexPropertyIndexFormat()
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Set("IP", "192.168.8.91");
            nv.Set("UserName", "wangqj");

            ComplexObject obj = new ComplexObject { Headers = nv };
            string htmlContent = new MailTemplet(@"Hello，{Request.Headers[""IP""].Length}, TotalCount:{Request.Headers.Count}!").SetVariable("Request", obj)
               .ToHtmlContent();

            //Console.Write(htmlContent);
            Debug.Assert(htmlContent.Equals("Hello，12, TotalCount:2!"));
        }

        public void SubComplex2Format()
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Set("IP", "192.168.8.91");
            nv.Set("UserName", "wangqj");

            ComplexObject obj = new ComplexObject { Headers = nv };
            string htmlContent = new MailTemplet(@"Hello，{Request.Headers[""IP""].Length}, TotalCount:{Request.Headers.Count}! {Headers[""IP""].Length} = {Headers[0].Length}")
                .SetVariable("Headers", nv)
                .SetVariable("Request", obj)
               .ToHtmlContent();

            //Console.Write(htmlContent);
            Debug.Assert(htmlContent.Equals("Hello，12, TotalCount:2! 12 = 12"));
        }

    }

    [Serializable]
    public class TempletObject
    {
        public string IpAddress;
        public string UserName;
    }

    [Serializable]
    public class ComplexObject
    {
        public string IpAddress;
        public string UserName;
        public NameValueCollection Headers { get; set; }
    }
}
#endif