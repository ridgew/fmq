using System;
using System.Collections.Generic;
using System.Text;

namespace Webot.SiteMagic.Common
{
    public class HttpTestModule : System.Web.IHttpModule
    {

        #region IHttpModule Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Inits the specified app.
        /// </summary>
        /// <param name="app">The app.</param>
        public void Init(System.Web.HttpApplication app)
        {
            #region 执行顺序

            //1.在 ASP.NET 响应请求时作为 HTTP 执行管线链中的第一个事件发生。
            app.BeginRequest += new EventHandler(app_BeginRequest);

            //2当安全模块已建立用户标识时发生。
            app.AuthenticateRequest += new EventHandler(app_AuthenticateRequest);


            //3.当安全模块已建立用户标识时发生。
            app.PostAuthenticateRequest += new EventHandler(app_PostAuthenticateRequest);

            //4.当安全模块已验证用户授权时发生。
            app.AuthorizeRequest += new EventHandler(app_AuthorizeRequest);

            //5.在当前请求的用户已获授权时发生。
            app.PostAuthorizeRequest += new EventHandler(app_PostAuthorizeRequest);

            //6.当 ASP.NET 完成授权事件以使缓存模块从缓存中为请求提供服务时发生，
            //从而跳过事件处理程序（例如某个页或 XML Web services）的执行。
            app.ResolveRequestCache += new EventHandler(app_ResolveRequestCache);

            //7.在 ASP.NET 跳过当前事件处理程序的执行并允许缓存模块满足来自缓存的请求时发生。
            app.PostResolveRequestCache += new EventHandler(app_PostResolveRequestCache);

            //8.Occurs when the handler is selected to respond to the request.
            app.MapRequestHandler += new EventHandler(app_MapRequestHandler);

            //在 ASP.NET 已将当前请求映射到相应的事件处理程序时发生。
            app.PostMapRequestHandler += new EventHandler(app_PostMapRequestHandler);

            //9.当 ASP.NET 获取与当前请求关联的当前状态（如会话状态）时发生。
            app.AcquireRequestState += new EventHandler(app_AcquireRequestState);

            //10.在已获得与当前请求关联的请求状态（例如会话状态）时发生。
            app.PostAcquireRequestState += new EventHandler(app_PostAcquireRequestState);

            //11.恰好在 ASP.NET 开始执行事件处理程序（例如，某页或某个 XML Web service）前发生。
            app.PreRequestHandlerExecute += new EventHandler(app_PreRequestHandlerExecute);

            //12.在 ASP.NET 事件处理程序（例如，某页或某个 XML Web service）执行完毕时发生。
            app.PostRequestHandlerExecute += new EventHandler(app_PostRequestHandlerExecute);


            //13.在 ASP.NET 执行完所有请求事件处理程序后发生。该事件将使状态模块保存当前状态数据。
            app.ReleaseRequestState += new EventHandler(app_ReleaseRequestState);

            //14.在 ASP.NET 已完成所有请求事件处理程序的执行并且请求状态数据已存储时发生。
            app.PostReleaseRequestState += new EventHandler(app_PostReleaseRequestState);

            //15.当 ASP.NET 执行完事件处理程序以使缓存模块存储将用于从缓存为后续请求提供服务的响应时发生。
            app.UpdateRequestCache += new EventHandler(app_UpdateRequestCache);

            //16.在 ASP.NET 完成了缓存模块的更新并存储了以下响应时发生，这些响应用于满足来自缓存的后续请求。
            app.PostUpdateRequestCache += new EventHandler(app_PostUpdateRequestCache);


            //17.在 ASP.NET 响应请求时作为 HTTP 执行管线链中的最后一个事件发生。
            app.EndRequest += new EventHandler(app_EndRequest);



            //当引发未处理的异常时发生。
            app.Error += new EventHandler(app_Error);

            //Occurs just before ASP.NET performs any logging for the current request.
            app.LogRequest += new EventHandler(app_LogRequest);

            //  Occurs when ASP.NET has completed processing all the event handlers for the
            //  System.Web.HttpApplication.LogRequest event.
            app.PostLogRequest += new EventHandler(app_PostLogRequest);

            //恰好在 ASP.NET 向客户端发送 HTTP 标头之前发生。
            app.PreSendRequestHeaders += new EventHandler(app_PreSendRequestHeaders);

            //恰好在 ASP.NET 向客户端发送内容之前发生。
            app.PreSendRequestContent += new EventHandler(app_PreSendRequestContent);

            //添加事件处理程序以侦听应用程序上的事件。
            app.Disposed += new EventHandler(app_Disposed); 

            
            #endregion

        }

        void app_MapRequestHandler(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_UpdateRequestCache(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_ResolveRequestCache(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_ReleaseRequestState(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PreSendRequestHeaders(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PreSendRequestContent(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostUpdateRequestCache(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostResolveRequestCache(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostReleaseRequestState(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostMapRequestHandler(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostLogRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostAuthorizeRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostAuthenticateRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_PostAcquireRequestState(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_LogRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_Error(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_EndRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_Disposed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 在 ASP.NET 响应请求时作为 HTTP 执行管线链中的第一个事件发生。
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void app_BeginRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_AuthorizeRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_AuthenticateRequest(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void app_AcquireRequestState(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
