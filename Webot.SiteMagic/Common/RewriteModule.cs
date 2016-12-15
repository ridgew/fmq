/***************************
 * $Id: RewriteModule.cs 217 2009-07-21 16:42:04Z fanmaquar@staff.vbyte.com $
 * $Author: fanmaquar@staff.vbyte.com $
 * $Rev: 217 $
 * $Date: 2009-07-22 00:42:04 +0800 (星期三, 22 七月 2009) $
 * ***************************/

// Author: Fabrice Marguerie
// http://weblogs.asp.net/fmarguerie/
// fabrice@madgeek.com
//
// Free for use
//
// Based on code from Fritz Onion: http://pluralsight.com/blogs/fritz/archive/2004/07/21/1651.aspx

using System;
using System.Collections;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Webot.Common;

namespace Webot.UrlRewriter
{
    /// <summary>
    /// 重写实例设置
    /// </summary>
	public class UrlRewriter
	{
        /// <summary>
        /// 匹配模式
        /// </summary>
		[XmlAttribute("Pattern")]
        public string Pattern;

        /// <summary>
        /// 目标文件
        /// </summary>
		[XmlAttribute("Replace")]
        public string Replace;
		
        /// <summary>
        /// 是否永久定向(301代码响应)
        /// </summary>
		[XmlAttribute("Permanent")]
		public bool Permanent = false;

        /// <summary>
        /// 匹配模式是否忽略大小写
        /// </summary>
		[XmlAttribute("IgnoreCase")]
		public bool IgnoreCase = true;

        /// <summary>
        /// 匹配主机名称
        /// </summary>
        [XmlAttribute("HostName")]
        public string HostName = "";

        /// <summary>
        /// 内部实在的正则匹配表达式
        /// </summary>
		[XmlIgnore]
		public Regex Regex;
	}

    /// <summary>
    /// UrlRewriter配置
    /// </summary>
    [XmlRoot("UrlRewriter")]
    public class ConfigRewriters
	{
		[XmlElement("add")]
        public UrlRewriter[] UrlRewriters;
	}

	/// <summary>
    /// 通过web.cofing的UrlRewriter节点配置的Url重写实现
    /// 请先在configuration!configSections区注册UrlRewriter节点
	/// </summary>
    /// <remarks>
    /// UrlRewriter之后的WebForm, 原始请求的地址请从HttpContext.Current.Items["VirtualURL"]中获取。
    /// </remarks>
    /// <example>
    /// 
    /// &lt;configuration&gt;
    ///  &lt;configSections&gt;
    ///     &lt;section name="UrlRewriter" type="Webot.UrlRewriter.RewriteSectionHandler, Webot.UrlRewriter" /&gt;
    ///  &lt;configSections&gt;
    ///  &lt;UrlRewriter type="Webot.UrlRewriter.ConfigRewriters, Webot.UrlRewriter"&gt;
	///	   &lt;add Pattern="^/Management/NewsType\.aspx" Replace="~/" IgnoreCase="true" /&gt;
    ///  &lt;/UrlRewriter&gt;
    ///  
    /// &lt;system.web&gt;
    /// &lt;httpModules&gt;
    ///		&lt;add name="UrlRewriteModule" type="Webot.UrlRewriter.RewriteModule, Webot.UrlRewriter"/&gt;
	///	&lt;/httpModules&gt;
    /// &lt;/system.web&gt;
    /// &lt;/configSections>
    /// 
    /// </example>
  public class RewriteModule : IHttpModule
  {
		private string	_ApplicationPath;
		private ArrayList urlRwList;

         /// <summary>
         /// 初始化实例
         /// </summary>
		public RewriteModule()
		{
			//设置应用程序根目录
            _ApplicationPath = (HttpRuntime.AppDomainAppVirtualPath.Length > 1) ? HttpRuntime.AppDomainAppVirtualPath : String.Empty;
		}

		#region Private members
        /// <summary>
        /// 获取包含转义的真实相对路径, ~/ 表示当前应用程序根路径。
        /// </summary>
		private string GetEscapedUrl(string url)
		{
			if ((url == null) || (url.Length < 1))
				return url;

			if (url.StartsWith("^~/"))
				return "^" + _ApplicationPath + url.Substring(2);
			else if (url.StartsWith("~/"))
				return _ApplicationPath + url.Substring(1);

			return url;
		}

		private void OnBeginRequest(object sender, EventArgs e)
		{
            HttpApplication app = sender as HttpApplication;
            string requestUrl = app.Request.RawUrl;

			foreach (UrlRewriter rewriter in urlRwList)
			{
                #region 主机名匹配限制
                if (rewriter.HostName != "" && !Regex.IsMatch(app.Request.Url.Host, rewriter.HostName, RegexOptions.IgnoreCase))
                {
                    continue;
                } 
                #endregion

                #region 路径匹配限制
                if (rewriter.Regex.IsMatch(requestUrl))
                {
                    string urlReplace = rewriter.Regex.Replace(requestUrl, rewriter.Replace, 1);
                    urlReplace = GetEscapedUrl(urlReplace);

                    //if (app.Context.IsDebuggingEnabled)
                    //{
                    //    Util.Log2File("~/debug.log", "处理" + urlReplace + Environment.NewLine);
                    //}

                    if (rewriter.Permanent)
                    {
                        app.Response.StatusCode = 301; // make a permanent redirect
                        app.Response.AddHeader("Location", urlReplace);
                        app.Response.End();
                    }
                    else
                    {
                        // Keep track of the virtual URL because we'll need it to fix postbacks
                        // See http://weblogs.asp.net/jezell/archive/2004/03/15/90045.aspx
                        app.Context.Items["VirtualURL"] = requestUrl;
                        app.Context.Items["UrlMatchGroup"] = rewriter.Regex.Match(requestUrl).Groups;
                        if (Regex.IsMatch(urlReplace, "^((\\w+\\.)+)(\\w+)(,\\s?)((\\w+\\.)+)?(\\w+)", RegexOptions.IgnoreCase))
                        {
                            //Recommend.PlugIn.ProcessHandler, Recommend.PlugIn?
                            if (urlReplace.IndexOf("?") != -1) urlReplace = urlReplace.Substring(0, urlReplace.IndexOf("?"));
                            #region 实现IHttpHandler接口的类(2008-8-23)
                            Type ProcessType = Type.GetType(urlReplace, true, true);
                            if (ProcessType != null && ProcessType.GetInterface(typeof(IHttpHandler).ToString(), true) != null)
                            {
                                IHttpHandler handler = System.Activator.CreateInstance(ProcessType) as IHttpHandler;
                                if (handler != null)
                                {
                                    //if (app.Context.IsDebuggingEnabled)
                                    //{
                                    //    Util.Log2File("~/debug.log", "运行" + handler.ToString() + Environment.NewLine);
                                    //}
                                    handler.ProcessRequest(app.Context);
                                    app.CompleteRequest();
                                    break;
                                }
                            } 
                            #endregion
                        }
                        else
                        {
                            app.Context.RewritePath(urlReplace);
                        }
                    }
                    break;
                } 
                #endregion
			}
		}

		#endregion

        #region IHttpModule Members
        public void Init(HttpApplication context)
        {
            // for NET 1.1
            // redirections = (ConfigRewriters)ConfigurationSettings.GetConfig("UrlRewriter");
            // ----
            ConfigRewriters rewriters = (ConfigRewriters)ConfigurationManager.GetSection("UrlRewriter");
            this.urlRwList = new ArrayList();
            
            foreach (UrlRewriter redirection in rewriters.UrlRewriters)
            {
                string targetUrl;
                targetUrl = redirection.Pattern;
                targetUrl = GetEscapedUrl(targetUrl);
                //OleDbHelper.AppendToFile("~/rewrite.log", targetUrl + "\n");
                if (redirection.IgnoreCase)
                    redirection.Regex = new Regex(targetUrl, RegexOptions.IgnoreCase /* | RegexOptions.Compiled*/);
                else
                    redirection.Regex = new Regex(targetUrl/*, RegexOptions.Compiled*/);

                urlRwList.Add(redirection);
            }
            context.BeginRequest += new EventHandler(OnBeginRequest);
        }

        public void Dispose()
        {

        }
        #endregion
  }


  // A big thank you to Craig Andera for this one!
  // http://staff.develop.com/candera/weblog/stories/2003/02/20/theLastConfigurationSectionHandlerIllEverNeed.html

  /// <summary>
  /// Configuration section handler that deserializes connfiguration settings to an object.
  /// </summary>
  /// <remarks>The configuration node must have a type attribute defining the type to deserialize to.</remarks>
  public class RewriteSectionHandler : IConfigurationSectionHandler
  {
      /// <summary>
      /// Implemented by all configuration section handlers to parse the XML of the configuration section.
      /// The returned object is added to the configuration collection and is accessed by <see cref="System.Configuration.ConfigurationSettings.GetConfig"/>.
      /// </summary>
      /// <param name="parent">The configuration settings in a corresponding parent configuration section.</param>
      /// <param name="configContext">An <see cref="System.Web.Configuration.HttpConfigurationContext"/> when this method is called from the ASP.NET configuration system. Otherwise, this parameter is reserved and is a null reference (Nothing in Visual Basic).</param>
      /// <param name="section">The <see cref="System.Xml.XmlNode"/> that contains the configuration information from the configuration file. Provides direct access to the XML contents of the configuration section.</param>
      /// <returns>A configuration object.</returns>
      public object Create(object parent, object configContext, System.Xml.XmlNode section)
      {
          XPathNavigator navigator = section.CreateNavigator();
          string typeName = (string)navigator.Evaluate("string(@type)");
          Type type = Type.GetType(typeName, true);
          XmlSerializer serializer = new XmlSerializer(type);
          return serializer.Deserialize(new XmlNodeReader(section));
      }
  }

  /// <summary>
  /// 允许自定义属性的ReWriteForm方便重写
  /// </summary>
  public class RewriteForm : HtmlForm
  {
      /// <summary>
      /// 重写属性
      /// </summary>
      protected override void RenderAttributes(HtmlTextWriter writer)
      {
          writer.WriteAttribute("name", this.Name);
          base.Attributes.Remove("name");

          writer.WriteAttribute("method", base.Method);
          base.Attributes.Remove("method");

          if (string.IsNullOrEmpty(base.Attributes["action"]))
          {
              if (HttpContext.Current.Items["VirtualURL"] != null)
              {
                  writer.WriteAttribute("action", HttpContext.Current.Items["VirtualURL"].ToString());
              }
          }
          else
          {
              if (HttpContext.Current.Items["VirtualURL"] != null)
              {
                  base.Attributes.Remove("action");
                  writer.WriteAttribute("action", HttpContext.Current.Items["VirtualURL"].ToString());
              }
          }

          if (base.ID != null)
          {
              writer.WriteAttribute("id", base.ClientID);
          }
          base.Attributes.Render(writer);
      }

  }

}