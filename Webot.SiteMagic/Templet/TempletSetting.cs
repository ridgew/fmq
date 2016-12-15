using System;
using Webot.Common;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 模板设置
    /// </summary>
    [Serializable]
    public class TempletSetting : IVersion
    {

        public TempletSetting()
        { 
            
        }
        
        private PageTemplet _mixedSingleTpt;
        /// <summary>
        /// 综合单一页面模板(首页/专题)
        /// </summary>
        public PageTemplet MixedSingleTemplet
        {
            get { return _mixedSingleTpt; }
            set { _mixedSingleTpt = value; }
        }

        private PageTemplet _listPageTpt;
        /// <summary>
        /// 列表页面模板
        /// </summary>
        public PageTemplet ListPageTemplet
        {
            get { return _listPageTpt; }
            set { _listPageTpt = value; }
        }

        private PageTemplet _detailPageTpt;
        /// <summary>
        /// 详细页面模板
        /// </summary>
        public PageTemplet DetailPageTemplet
        {
            get { return _detailPageTpt; }
            set { _detailPageTpt = value; }
        }

        private bool _genSinglePage = false;
        /// <summary>
        /// 生成单页内容
        /// </summary>
        public bool GenerateSinglePage
        {
            get { return _genSinglePage; }
            set { _genSinglePage = value; }
        }


        private bool _genListPage = false;
        /// <summary>
        /// 生成列表页面
        /// </summary>
        public bool GenerateListPage
        {
            get { return _genListPage; }
            set { _genListPage = value; }
        }

        private bool _genDetailPage = false;
        /// <summary>
        /// 生成详细页面
        /// </summary>
        public bool GenerateDetailPage
        {
            get { return _genDetailPage; }
            set { _genDetailPage = value; }
        }

        /// <summary>
        /// 是否有单页生成模板
        /// </summary>
        /// <returns>判断是否存在该设置</returns>
        public bool HasMixedSingleTemplet() { return this._mixedSingleTpt != null; }

        /// <summary>
        /// 是否存在列表页面模板
        /// </summary>
        /// <returns>判断是否存在该设置</returns>
        public bool HasListTemplet() { return this._listPageTpt != null; }

        /// <summary>
        /// 是否存在详细页面模板
        /// </summary>
        /// <returns>判断是否存在该设置</returns>
        public bool HasDetailTemplet() { return this._detailPageTpt != null; }

        #region IVersion Members

        /// <summary>
        /// 获取当前应用对象的版本
        /// </summary>
        public Version CurrentVersion
        {
            get
            {
                return new Version(0, 0, 0, 1);
            }
        }

        /// <summary>
        /// 获取该同类对象的版本是否高于另一对象的版本
        /// </summary>
        /// <param name="Ver">实现同一接口的对象版本</param>
        /// <returns>如果高于该版本则返回真,否则返回假.</returns>
        public bool VersionIsHigherThan(IVersion Ver)
        {
            return Util.VersionIsHigherThan(this, Ver);
        }

        #endregion
    }
}
