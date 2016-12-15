using System;

namespace Webot.SiteMagic
{
    public class HtmlTextTag : IInternalTag
    {
        public HtmlTextTag(string htmlTxt)
        {
            this.Html = htmlTxt;
        }

        private string _html = "";
        /// <summary>
        /// 设置HTML文本
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        public override string ToString()
        {
            return this.Html;
        }


        #region IInternalTag Members
        public string TagDefinition
        {
            get
            {
                return this.Html;
            }
            set
            {
                this.Html = value;
            }
        }

        public bool IsDependencyTag
        {
            get { return false; }
        }

        public IResourceDependency GetResourceDependency()
        {
            return null;
        }

        public void SetResourceDependency(IResourceDependency value)
        {
            
        }

        #endregion
    }
}
