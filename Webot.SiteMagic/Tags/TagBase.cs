using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Text;
using Webot.Common;
using System.Data;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 标签分类
    /// </summary>
    public enum TagCategory
    { 
        /// <summary>
        /// 自定义标签
        /// </summary>
        CustomTag = 0x05,
        /// <summary>
        /// 系统标签
        /// </summary>
        SystemTag = 0x10,
        /// <summary>
        /// 分页标签
        /// </summary>
        PagerTag = 0x15,
        /// <summary>
        /// 外部定义设置标签
        /// </summary>
        DefineTag = 0x20,
        /// <summary>
        /// 服务器端变量标签
        /// </summary>
        ServerTag = 0x25,
        /// <summary>
        /// 数据列表标签
        /// </summary>
        DataListTag = 0x30,
        /// <summary>
        /// 自动标签(Pager)
        /// </summary>
        AutoTag = 0x35,
        /// <summary>
        /// HTML文本标签
        /// </summary>
        HtmlTextTag = 0x40
    }
    
    /// <summary>
    /// 标签定义基础类
    /// </summary>
    public class TagBase : IInternalTag
    {
        #region 匹配参考
        //备选匹配样式： \{#([\d\D]*?)#\}
        //C类注释： (\/\*(\s*|.*?)*\*\/)|(\/\/.*) 
        //Href地址： href\s*=\s*(?:”(?<1>[^\"]*)“|(?<1>\S+))
        #endregion
        
        /// <summary>
        /// 标签定义匹配模式
        /// </summary>
        public static string TagDefinitionPattern = "\\{#([\\d\\D]*?)#\\}";

        /// <summary>
        /// 容器标签
        /// </summary>
        public const string CONTAINER_TAGLIB = ",list,pager,autoitem,";

        /// <summary>
        /// 标签定义基础类
        /// </summary>
        public TagBase() { }

        /// <summary>
        /// 从字符串实例化标签定义
        /// </summary>
        /// <param name="tagdef">标签定义文本</param>
        public TagBase(string tagdef)
        {
            //if (tagdef.Length < 6) { throw new Exception("标签定义不正确！");  }
            TagDefinition = tagdef;
        }

        /// <summary>
        /// 相关属性
        /// </summary>
        private NameValueCollection Attributes = new NameValueCollection();
        /// <summary>
        /// 获取相关属性
        /// </summary>
        public string GetAttribute(string item)
        {
            return Attributes[item];
        }

        /// <summary>
        /// 获取属性集合
        /// </summary>
        public NameValueCollection GetObjAttributes()
        {
            return this.Attributes;
        }

        /// <summary>
        /// 设置相关属性
        /// </summary>
        public void SetAttribute(string item, object value)
        {
            Attributes[item] = value.ToString();
        }

        /// <summary>
        /// 从模板中加载标签定义
        /// </summary>
        /// <param name="tagdefStart">标签初始定义文本</param>
        /// <param name="idxStart">标签定义开始索引</param>
        /// <param name="SourceText">模板定义原始文本源</param>
        public TagBase(string tagdefStart, int idxStart, ref string SourceText)
        {
            this.DefinedIndexStart = idxStart;
            this.TagDefinition = tagdefStart;

            if (this.IsContainer == true && !tagdefStart.StartsWith("{#/"))
            {
                this.DefinedIndexEnd = GetTagDefinedEnd(idxStart + tagdefStart.Length, this.TagName, ref SourceText);
                if (this.DefinedIndexEnd > 0)
                {
                    this.OuterDefineText = SourceText.Substring(idxStart, this.DefinedIndexEnd - idxStart);
                }
            }
            else
            {
                this.DefinedIndexEnd = idxStart + tagdefStart.Length;
            }
        }

        private string _identity = Guid.NewGuid().ToString();
        /// <summary>
        /// 标签的唯一标志
        /// </summary>
        public string IDentity
        {
            set { this._identity = value; }
            get { return this._identity; }
        }

        private int GetTagDefinedEnd(int idxStart, string tagName, ref string SourceText)
        {
            string endTagName = "{#/" + tagName + "#}";
            string lastRightStr = "...";
            int idxEnd = SourceText.IndexOf(endTagName, idxStart);

        begin:
            if (idxEnd == -1)
            {
                throw new Exception(string.Format("模板标签定义错误：标签{0}没有定义结束(或嵌套)标签！\n正确解析至:{1}",
                    "{#" + tagName + "#}", lastRightStr));
            }
            else
            {
                int nestingTagIdx = SourceText.IndexOf("{#" + tagName, idxStart + tagName.Length);
                if (nestingTagIdx != -1 && nestingTagIdx < idxEnd)
                {
                    idxStart = nestingTagIdx + tagName.Length + 4;
                    lastRightStr = "..." + SourceText.Substring(nestingTagIdx, idxEnd - nestingTagIdx + endTagName.Length);
                    //跳出嵌套标签
                    idxEnd = SourceText.IndexOf(endTagName, idxEnd + endTagName.Length); 
                    goto begin; 
                }
                return idxEnd + endTagName.Length;
            }
        }
        
        #region IInternalTag Members
        
        private string _tagdef;
        /// <summary>
        /// 标签定义文本
        /// </summary>
        public string TagDefinition
        {
            get
            {
                return this._tagdef;
            }
            set
            {
                this._tagdef = value;
                IntializeObjectData(this._tagdef);
            }
        }

        [NonSerialized]
        private string _outerText = "";
        /// <summary>
        /// 该标签全部定义文本
        /// </summary>
        public string OuterDefineText
        {
            get 
            {
                if (IsContainer && this._outerText != "")
                {
                    return this._outerText;
                }
                else
                {
                    return this.TagDefinition;
                }
            }
            set { _outerText = value; }
        }


        private TagCategory _category = TagCategory.CustomTag;
        /// <summary>
        /// 获取标签的所属类型
        /// </summary>
        public TagCategory Category
        {
            set { this._category = value; }
            get { return this._category; }
        }

        private string _tagName = "";
        /// <summary>
        /// 获取标签名称
        /// </summary>
        public string TagName
        {
            set { this._tagName = value; }
            get  { return this._tagName; }
        }

        /// <summary>
        /// 是否有依赖资源的标签定义
        /// </summary>
        public bool IsDependencyTag
        {
            get { return Regex.IsMatch(TagDefinition, TagDefinitionPattern, RegexOptions.IgnoreCase); }
        }

        private IResourceDependency _resDependency = null;
        /// <summary>
        /// 获取该标签所依赖的资源
        /// </summary>
        public IResourceDependency GetResourceDependency()
        {
            return this._resDependency;
        }

        /// <summary>
        /// 设置该标签所依赖的资源
        /// </summary>
        public void SetResourceDependency(IResourceDependency value)
        {
            this._resDependency = value;
            //if (value is MultiResDependency)
            //{
            //    MultiResDependency mRes = (MultiResDependency)value;
            //    mRes.AddResDependency(value);
            //    this._resDependency = mRes;
            //}
            //else
            //{
            //    this._resDependency = new MultiResDependency(value, RequestResDependency.Instance);
            //}
        }
        #endregion

        private bool _isContainerTag = false;
        /// <summary>
        /// 是否是容器标签
        /// </summary>
        public bool IsContainer
        {
            get { return _isContainerTag; }
            set { _isContainerTag = value; }
        }


        [NonSerialized]
        private int _definedIndexStart = 0;
        /// <summary>
        /// 内部标签定义的索引开始位置
        /// </summary>
        public int DefinedIndexStart
        {
            get { return _definedIndexStart; }
            set { _definedIndexStart = value; }
        }

        [NonSerialized]
        private int _definedIndexEnd = 0;
        /// <summary>
        /// 内部标签定义的索引结束位置
        /// </summary>
        public int DefinedIndexEnd
        {
            get { return _definedIndexEnd; }
            set { _definedIndexEnd = value; }
        }

        /// <summary>
        /// 获取标签定义的值
        /// </summary>
        /// <returns>根据标签定义和所依赖的资源获取标签的最终值。</returns>
        public virtual object GetTagValue()
        {
            if (this.TagDefinition == null) return null;

            if (!this.IsDependencyTag)
            {
                return this.TagDefinition;
            }
            else
            {
                IResourceDependency res = this.GetResourceDependency();
                if (res != null && res.IsDefined(this.TagDefinition))
                {
                    return res.GetDefinition(this.TagDefinition);
                }
                else
                {
                    //直接返回定义继续解析
                    return this.OuterDefineText;
                }
            }
        }

        /// <summary>
        /// 获取标签定义的内部文本
        /// </summary>
        public static string GetTagDefineInnerText(string tagdef)
        {
            TagBase tag = new TagBase(tagdef);
            if (!tag.IsDependencyTag)
            {
                return tag.TagDefinition;
            }
            else
            {
                if (tag.IsContainer)
                {
                    int idxBegin = 0, idxEnd = 0;
                    idxBegin = tagdef.IndexOf("#}");
                    idxEnd = tagdef.LastIndexOf("{#");
                    return tagdef.Substring(idxBegin + 2, idxEnd - idxBegin - 2).Trim();
                }
                else
                {
                    return tagdef.Trim('{', '#', '}');
                }
            }
        }

        /// <summary>
        /// 检查标签是否是容器标签
        /// </summary>
        /// <param name="strTagName">标签名称</param>
        /// <returns>是否是定义的容器标签</returns>
        public static bool IsContainerTag(string strTagName)
        {
            return (CONTAINER_TAGLIB.IndexOf("," + strTagName + ",") != -1);
        }

        /// <summary>
        /// 删除字符串中的标签定义
        /// </summary>
        public static string TrimTagDefine(string strWithTagDef,params char[] trimChars)
        {
            string strRet = Regex.Replace(strWithTagDef, TagBase.TagDefinitionPattern, "");
            return (trimChars != null) ? strRet.Trim(trimChars) : strRet.Trim();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void IntializeObjectData(string tagDefFullData)
        {
            this._outerText = tagDefFullData;

            if (!tagDefFullData.StartsWith("{#") || !tagDefFullData.EndsWith("#}"))
            {
                this.Category = TagCategory.HtmlTextTag;
                return;
            }

            this.TagName = GetTagName(tagDefFullData);
            string rootTagName = this.GetRootTagName();

            if (IsContainerTag(rootTagName.ToLower()))
            {
                this.IsContainer = true;
                if (string.Compare(rootTagName, "list", true) == 0)
                {
                    this.Category = TagCategory.DataListTag;
                }

                if (string.Compare(rootTagName, "pager", true) == 0)
                {
                    this.Category = TagCategory.PagerTag;
                }

                if (string.Compare(rootTagName, "autoitem", true) == 0)
                {
                    this.Category = TagCategory.AutoTag;
                }

            }

            #region 设置其他标签类型
            if (tagDefFullData.StartsWith("{#%") && tagDefFullData.EndsWith("%#}"))
            {
                this.Category = TagCategory.DefineTag;
            }

            if (tagDefFullData.StartsWith("{#$") && tagDefFullData.EndsWith("$#}"))
            {
                //if (TagName.IndexOf("(") != -1 && TagName.IndexOf(")") != -1)
                //{
                //    this.Category = TagCategory.ServerTag;
                //}
                this.Category = TagCategory.SystemTag;
            }
            #endregion

            Regex regEX = new Regex(TagDefinitionPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection mc = regEX.Matches(tagDefFullData);
            if (mc.Count > 0)
            {
                string baseDefine = mc[0].Value;
                int idxBlank = baseDefine.IndexOf(' ');
                #region 初始化属性
                if (idxBlank != -1)
                {
                    baseDefine = baseDefine.Trim('{', '#', '}');
                    idxBlank = baseDefine.IndexOf(' ');
                    baseDefine = baseDefine.Substring(idxBlank+1);

                    int currentPos = 0;
                    int idxBegin = 0, idxEnd = 0;
                    char charAttributeEnd = ' ';
                    string[] NV = new string[] { "", "" };
                    #region 设置属性
                    while (currentPos < baseDefine.Length)
                    {
                        if (baseDefine[currentPos] == '=')
                        {
                            NV[0] = baseDefine.Substring(idxBegin, currentPos - idxBegin).TrimEnd('~').Trim();

                            //支持Like的数据库字段和原生SQL语句定义
                            if (currentPos > 1 && (baseDefine[currentPos - 1] == '~' || baseDefine[currentPos - 1] == '@'))
                            {
                                NV[0] = "%" + baseDefine[currentPos - 1].ToString() + NV[0].Trim('%') + "%";
                            }

                            if (baseDefine[currentPos + 1] == '\''
                                || baseDefine[currentPos + 1] == '"'
                                || baseDefine[currentPos + 1] == '\\'
                                || baseDefine[currentPos + 1] == '/'
                                || baseDefine[currentPos + 1] == '{')
                            {
                                charAttributeEnd = (baseDefine[currentPos + 1] == '{') ? '}' : baseDefine[currentPos + 1];
                                idxBegin = currentPos + 2;
                            }
                            else
                            {
                                charAttributeEnd = ' ';
                                idxBegin = currentPos + 1;
                            }

                            idxEnd = baseDefine.IndexOf(charAttributeEnd, idxBegin);
                            if (idxEnd == -1)
                            {
                                throw new Exception("标签属性定义错误！" + this.TagDefinition);
                            }
                            else
                            {
                                NV[1] = baseDefine.Substring(idxBegin, idxEnd-idxBegin);
                                idxBegin = idxEnd + 1;
                                currentPos = idxEnd;
                            }
                            //Util.Debug(false, "Name=" + NV[0].Trim('(', ')',' '), "Value=" + NV[1].Trim('(', ')', ' '));
                            Attributes.Add(NV[0].Trim('(', ')', ' '), NV[1].Trim('(', ')', ' '));
                        }
                        ++currentPos;
                    }
                    #endregion
                }
                #endregion
            }
        }

        /// <summary>
        /// 获取标签的根名称
        /// </summary>
        public string GetRootTagName()
        {
            int idx = this.TagName.IndexOf(':');
            string strRet = (idx != -1) ? this.TagName.Substring(0, idx) : this.TagName;
            return strRet.Trim('<', '>');
        }

        /// <summary>
        /// 获取标签的从属名称
        /// </summary>
        public string GetSubTagName()
        {
            int idx = this.TagName.IndexOf(':');
            string strRet = (idx != -1) ? this.TagName.Substring(idx+1) : this.TagName;
            return strRet.Trim('<', '>');
        }

        /// <summary>
        /// 获取标签名称
        /// </summary>
        /// <param name="tagdef">标签定义数据</param>
        public static string GetTagName(string tagdef)
        {
            StringBuilder sb = new StringBuilder();
            int idxStart = 0;
            if (tagdef.StartsWith("{#")) { idxStart = 2; }
            if (tagdef.StartsWith("{#/")) { idxStart = 3; }

            for (int i = idxStart; i < tagdef.Length; i++)
            {
                if (tagdef[i] != '{' && tagdef[i] != '#' && tagdef[i] != '='
                    && tagdef[i] != ' ' && tagdef[i] != '(' && tagdef[i] != '[')
                {
                    sb.Append(tagdef[i]);
                }
                else
                {
                    break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取数据表中的转义数据
        /// </summary>
        /// <param name="CurrentIndex">当前索引</param>
        /// <param name="Tpt">模板内容</param>
        /// <param name="dRow">数据行</param>
        /// <param name="Res">依赖资源</param>
        public static string GetDataEscapedValue(int CurrentIndex, string Tpt, DataRow dRow, IResourceDependency Res)
        {
            // 替换索引
            Tpt = Regex.Replace(Tpt, @"(\{#)?(\$Index\$)(#\})?", CurrentIndex.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);

            StringBuilder sb = new StringBuilder(Tpt.Length);
            Regex RE = new Regex(@"(\{#)?%([\w]+)%(#\})?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection mc = RE.Matches(Tpt);
            int idxBegin = 0, idxEnd = 0;
            foreach (Match m in mc)
            {
                idxEnd = m.Index;
                sb.Append(Tpt.Substring(idxBegin, idxEnd - idxBegin));
                sb.Append(dRow[m.Groups[2].Value]);
                idxBegin = idxEnd + m.Length;
            }
            if (idxBegin < Tpt.Length)
            {
                sb.Append(Tpt.Substring(idxBegin));
            }
            return InterpretContentWithTags(sb.ToString(), Res);
        }

        /// <summary>
        /// 获取数据表中的转义数据
        /// </summary>
        /// <param name="CurrentIndex">当前索引</param>
        /// <param name="Tpt">模板内容</param>
        /// <param name="dRow">数据行</param>
        public static string GetDataEscapedValue(int CurrentIndex, string Tpt, DataRow dRow)
        {
            return GetDataEscapedValue(CurrentIndex, Tpt, dRow, null);
        }

        /// <summary>
        /// 获取最终字符串表示形式
        /// </summary>
        public override string ToString()
        {
            object ret = GetTagValue();
            return (ret == null) ? "" : ret.ToString();
        }

        /// <summary>
        /// 解析包含标签的文本内容
        /// </summary>
        /// <param name="TxtContent">包含标签的文本内容</param>
        /// <param name="res">说依赖的相关资源对象</param>
        public static string InterpretContentWithTags(string TxtContent, IResourceDependency res)
        {
            if (!TxtContent.StartsWith("{#"))
            {
                TxtContent = "#}" + TxtContent + "{#";
            }

            PagerTag txtTag = new PagerTag();
            PagerTag.FilledInTagList(TxtContent, txtTag.tagList, res);
            txtTag.AlwaysShow = true;
            return txtTag.ToString();

        }

    }
}
