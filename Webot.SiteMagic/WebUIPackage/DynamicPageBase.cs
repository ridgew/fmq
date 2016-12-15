using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI;
using Webot.SiteMagic;
using Webot.Common;

namespace Webot.WebUIPackage
{
    /// <summary>
    /// 动态页面内容基类
    /// </summary>
    public abstract class DynamicPageBase : Page, IResourceDependency, IDynamicResponse
    {
        /// <summary>
        /// 实体化
        /// </summary>
        public DynamicPageBase() : base()
        {
            base.Init += new EventHandler(DynamicPageBase_Init);
            base.Load += new EventHandler(DynamicPageBase_Load);
            base.Unload += new EventHandler(DynamicPageBase_Unload);
        }

        /// <summary>
        /// 当请求被阻止时激发
        /// </summary>
        public event EventHandler OnBlockedRequest;

        /// <summary>
        /// 当请求的参数/数据错误时激发
        /// </summary>
        public event EventHandler OnErrorRequest;

        /// <summary>
        /// 初始化时触发
        /// </summary>
        public event EventHandler OnInitial;
        void DynamicPageBase_Init(object sender, EventArgs e)
        {
            if (this.OnInitial != null) OnInitial(sender, e);
        }

        void DynamicPageBase_Unload(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 内部判断是否本站请求
        /// </summary>
        private bool IsSelfDomainSubmit()
        {
            Uri sHttp_Referer = Request.UrlReferrer;
            Uri sHttp_Request = Request.Url;
            if (sHttp_Referer != null)
            {
                if (this.RefererHostPattern != "")
                {
                    return Regex.IsMatch(sHttp_Referer.Host, this.RefererHostPattern, RegexOptions.IgnoreCase);
                }
                else
                {
                    if (sHttp_Request.Host.IndexOf(".") != -1)
                    {
                        string strTemp = "";
                        //默认为同二级域名相同:*.webot.cn,*.vbyte.com
                        int idx = sHttp_Request.Host.LastIndexOf('.');
                        strTemp = sHttp_Request.Host.Substring(0, idx);
                        if (strTemp.LastIndexOf('.') != -1)
                        {
                            idx = strTemp.LastIndexOf('.');
                            this.RefererHostPattern = string.Concat(@"((\w+)\.)?", Regex.Escape(sHttp_Request.Host.Substring(idx + 1)), "$");
                        }
                        else
                        {
                            this.RefererHostPattern = string.Concat(@"((\w+)\.)?", Regex.Escape(sHttp_Request.Host), "$");
                        }
                        return Regex.IsMatch(sHttp_Referer.Host, this.RefererHostPattern, RegexOptions.IgnoreCase);
                    }
                    else
                    { 
                        return (string.Compare(sHttp_Referer.Host, sHttp_Request.Host, true) == 0);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 页面装载事件(封装预处理)
        /// </summary>
        void DynamicPageBase_Load(object sender, EventArgs e)
        {
            if (this.ForceInternalSiteRequest == true)
            {
                if (!IsSelfDomainSubmit())
                {
                    if (this.OnBlockedRequest != null) { this.OnBlockedRequest(this, null); }
                    return;
                }
            }

            if (UsePathInfoData == true)
            {
                string data = Request.PathInfo;
                if (data == null /*|| !Util.IsSelfDomainSubmit()*/) return;
                Match m = Regex.Match(data, StripKeyPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (!m.Success)
                {
                    if (this.OnErrorRequest != null) OnErrorRequest(this, null);
                    return;
                }
                else
                {
                    if (m.Value.EndsWith("/"))
                    {
                        SetDefineDicData(data.Substring(m.Index + m.Length - 1));
                    }
                    else
                    {
                        SetDefineDicData(data.Substring(m.Index + m.Length));
                    }

                    this.OutPutText = ExecuteQuery(m.Groups[1].Value);
                }
            }
            else
            {
                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (key != null && key.ToLower() != QueryIDKey.ToLower())
                    {
                        SetDefined(key.ToLower(), Request.QueryString[key]);
                    }
                }

                if (Request.QueryString[QueryIDKey] != null)
                {
                    this.OutPutText = ExecuteQuery(Request.QueryString[QueryIDKey]);
                }
                else
                {
                    this.OutPutText = ExecuteQuery(QueryIDKey);
                }
            }
            
        }

        private string _qidKey = "qid";
        /// <summary>
        /// 查询的主标识(默认qid) 主要针对使用QueryString传递参数。
        /// </summary>
        public string QueryIDKey
        {
            get { return _qidKey; }
            set { _qidKey = value; }
        }

        private string _keyPattern = @"^/(\d+)/";
        /// <summary>
        /// 主标识值的匹配模式(默认@"^/(\d+)/")。主要针对使用路径信息传递参数。
        /// </summary>
        public string StripKeyPattern
        {
            get { return this._keyPattern; }
            set { this._keyPattern = value; }
        }

        private string _refererHostPattern = "";
        /// <summary>
        /// 引用页主机名匹配模式(默认为同二级域名相同:*.webot.cn,*.vbyte.com。)
        /// </summary>
        public string RefererHostPattern
        {
            get { return _refererHostPattern; }
            set { _refererHostPattern = value; }
        }

        private bool _forceInternalSiteRequest = false;
        /// <summary>
        /// 是否强制站内引用资源(默认关闭)
        /// </summary>
        public bool ForceInternalSiteRequest
        {
            get { return _forceInternalSiteRequest; }
            set { _forceInternalSiteRequest = value; }
        }


        /// <summary>
        /// 小写键名
        /// </summary>
        public Dictionary<string, object> DefineDic = new Dictionary<string, object>();
        #region IResourceDependency Members
        public object GetDefinition(string x)
        {
            x = x.Trim('{', '#', '$', '%', '}').ToLower();
            if (DefineDic.ContainsKey(x))
            {
                return DefineDic[x];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取是否已定义相关标签
        /// </summary>
        public bool IsDefined(string x)
        {
            x = x.Trim('{', '#', '$', '%', '}').ToLower();
            return DefineDic.ContainsKey(x);
        }
        #endregion

        /// <summary>
        /// 设置定义(小写键名)
        /// </summary>
        public void SetDefined(string key, object value)
        {
            key = key.ToLower();
            if (!DefineDic.ContainsKey(key))
            {
                DefineDic.Add(key, value);
            }
            else
            {
                DefineDic[key] = value;
            }
        }

        /// <summary>
        /// 设置查询条件及数据
        /// </summary>
        /// <param name="friendlyQueryDat">友好的URL地址数据</param>
        private void SetDefineDicData(string friendlyQueryDat)
        {
            // /1/username/于秀东/passwd/a/x/34/y/6 
            if (friendlyQueryDat.IndexOf('/') != -1)
            {
                string[] objQueryDat = friendlyQueryDat.Split('/');
                if (objQueryDat.Length > 2)
                {
                    string key = "", value;
                    for (int i = 0, j = objQueryDat.Length; i < j; i++)
                    {
                        if (objQueryDat[i] != "/" && objQueryDat[i] != "")
                        {
                            key = objQueryDat[i].ToLower();
                            if (i + 1 < j)
                            {
                                value = objQueryDat[i + 1];
                                i += 1;
                                if (!DefineDic.ContainsKey(key)) { DefineDic.Add(key, value); }
                            }
                        }
                    }
                }

                //foreach (string k in DefineDic.Keys)
                //{
                //    Util.Debug(false, k + "=" + DefineDic[k].ToString());
                //}
            }
        }

        #region IDynamicResponse Members
        private bool _usePathInfoData = true;
        /// <summary>
        /// 使用路径信息传递参数
        /// </summary>
        public bool UsePathInfoData
        {
            get { return _usePathInfoData; }
            set { _usePathInfoData = value; }
        }

        private bool _isPagedContent = false;
        public bool IsPagedContent
        {
            get
            {
                return this._isPagedContent;
            }
            set
            {
                this._isPagedContent = value;
            }
        }

        /// <summary>
        /// 处理动态内容
        /// </summary>
        public virtual string ExecuteQuery(string qid)
        {
            throw new NotImplementedException();
        }

        #endregion

        private string _outPutText = "";
        /// <summary>
        /// 动态页面输出文本内容
        /// </summary>
        public string OutPutText
        {
            get { return _outPutText; }
            set { _outPutText = value; }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.DefineDic = null;
        }

        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        public virtual string DependencyIdentity  
        {
            get { return "DynamicPageDependency"; }
        }

    }

}
