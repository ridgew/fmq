/******************************
 *  $Id: ListTag.cs 231 2009-10-11 09:59:46Z fanmaquar@staff.vbyte.com $
 *  $Author: fanmaquar@staff.vbyte.com $
 *  $Rev: 231 $
 *  $Date: 2009-10-11 17:59:46 +0800 (星期日, 11 十月 2009) $
 * *********************************/
using System;
using System.Data;
using System.Text.RegularExpressions;
using Webot.Common;
using System.Text;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 数据列表标签
    /// </summary>
    public class ListTag : StoredTags, IPagedContent, IResourceDependency, IDisposable
    {
        /// <summary>
        /// 初始化一个列表标签对象
        /// </summary>
        public ListTag() : base ()
        { 
        
        }
        
        /// <summary>
        /// 初始化一个列表实例对象
        /// </summary>
        public ListTag(string tagdef)
            : base(tagdef)
        {
            TagBase tag = new TagBase(tagdef);
            InheriteFromBase(tag);
            SetListTemplet(tagdef);
        }

        /// <summary>
        /// Sets the list templet.
        /// </summary>
        /// <param name="tagdef">The tagdef.</param>
        public void SetListTemplet(string tagdef)
        {
            this.HeaderTemplet = GetSubDefineByTagDefine(tagdef, "{#List:Header#}", "{#/List:Header#}");
            this.ItemTemplet = GetSubDefineByTagDefine(tagdef, "{#List:Item#}", "{#/List:Item#}");
            this.SeparatorTemplet = GetSubDefineByTagDefine(tagdef, "{#List:Separator#}", "{#/List:Separator#}");
            this.AlterItemTemplet = GetSubDefineByTagDefine(tagdef, "{#List:AlterItem#}", "{#/List:AlterItem#}");
            this.FooterTemplet = GetSubDefineByTagDefine(tagdef, "{#List:Footer#}", "{#/List:Footer#}");
            this.RowBreakTemplet = GetSubDefineByTagDefine(tagdef, "{#List:RowBreak#}", "{#/List:RowBreak#}");
            this.EmptyTemplet = GetSubDefineByTagDefine(tagdef, "{#List:Empty#}", "{#/List:Empty#}");
        }

        private string _bindDbConnKey = FanmaquarConfig.ConnectionKey;
        /// <summary>
        /// 绑定数据库连接键值(2008-8-23)
        /// </summary>
        public string BindConnKey
        {
            get { return _bindDbConnKey; }
            set { _bindDbConnKey = value; }
        }

        private string _filterCondition = "";
        /// <summary>
        /// 过滤条件
        /// </summary>
        public string FilterCondition
        {
            get { return _filterCondition; }
            set { _filterCondition = value; }
        }


        private string _myQuery = "";
        /// <summary>
        /// 自定义查询条件
        /// </summary>
        public string Query
        {
            get { return this._myQuery; }
            set { this._myQuery = value; }
        }

        private string _bindTable = "Tbl_Articles";
        /// <summary>
        /// 绑定表格名称
        /// </summary>
        public string BindTableName
        {
            get { return _bindTable; }
            set { _bindTable = value; }
        }

        private string _fields = "*";
        /// <summary>
        /// 查询选择的列名集合
        /// </summary>
        public string Fields
        {
            get { return _fields; }
            set { _fields = value; }
        }


        private DataTable _DataSource;
        /// <summary>
        /// 数据源
        /// </summary>
        public DataTable DataSource
        {
            get { return _DataSource; }
            set { _DataSource = value; }
        }

        private string _HeaderTpt = "";
        /// <summary>
        /// 头模板
        /// </summary>
        public string HeaderTemplet
        {
            get { return _HeaderTpt; }
            set { _HeaderTpt = value; }
        }

        private string _separatorTpt = "";
        /// <summary>
        /// 项之间的间隔模板
        /// </summary>
        public string SeparatorTemplet
        {
            get { return _separatorTpt; }
            set { _separatorTpt = value; }
        }


        private string _ItemTpt = "";
        /// <summary>
        /// 单元项模板
        /// </summary>
        public string ItemTemplet
        {
            get { return _ItemTpt; }
            set { _ItemTpt = value; }
        }

        private string _AlterItemTpt = "";
        /// <summary>
        /// 交替项模板
        /// </summary>
        public string AlterItemTemplet
        {
            get { return _AlterItemTpt; }
            set { _AlterItemTpt = value; }
        }

        private string _EmptyTemplet = "";
        /// <summary>
        /// 数据为空时显示的模板
        /// </summary>
        public string EmptyTemplet
        {
            get { return _EmptyTemplet; }
            set { _EmptyTemplet = value; }
        }

        private string _FooterTpt = "";
        /// <summary>
        /// 尾模板
        /// </summary>
        public string FooterTemplet
        {
            get { return _FooterTpt; }
            set { _FooterTpt = value; }
        }

        private int _rowSize = 1;
        /// <summary>
        /// 每单元行所包含的项数
        /// </summary>
        public int RowSize
        {
            get { return _rowSize; }
            set { _rowSize = value; }
        }

        private string _rowBreakTpt = "";
        /// <summary>
        /// 单元行分隔模板
        /// </summary>
        public string RowBreakTemplet
        {
            get { return _rowBreakTpt; }
            set { _rowBreakTpt = value; }
        }

        private int _topCount = 0;
        /// <summary>
        /// 是否只选择前几项
        /// </summary>
        public int TopCount
        {
            get { return _topCount; }
            set { _topCount = value; }
        }

        private string _OrderBy = "";
        /// <summary>
        /// 排序字段设置
        /// </summary>
        public string OrderBy
        {
            get { return _OrderBy; }
            set { _OrderBy = value; }
        }

        private void SetDataSource()
        {
            string topFilter = (this.TopCount > 0) ? "Top " + this.TopCount.ToString() : "";
            string sql = string.Format("select {0} {1} from {2}{3}{4}",
                topFilter,
                this.Fields,
                this.BindTableName,
                string.IsNullOrEmpty(this.FilterCondition) ? "" : " where " + this.FilterCondition,
                string.IsNullOrEmpty(this.OrderBy) ? "" : " order by " + this.OrderBy);

            #region 自定义查询
            if (this.Query != "") { sql = this.Query.Replace("$Fields$", this.Fields).Replace("$PageSize$", this.PageSize.ToString()).Replace("$CurrentPageIndex$", CurrentPageIndex.ToString()); } 
            #endregion

            //Util.Debug(false, "依赖对象：" + this.GetResourceDependency());
            //if (this.GetResourceDependency() != null)
            //{
            //    Util.Debug(false, sql, "test:" + this.GetResourceDependency().GetDefinition("{#$ChannelID$#}"));
            //} 
            //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + sql);
            //if (base.GetAttribute("Debug") != null && base.GetAttribute("Debug").Equals("true", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + sql);
            //}

            if (this.PageSize > 0)
            {
                //Util.Debug(false, sql);
                this.DataSource = getStoreHelper(BindConnKey).GetDataTable(sql, CurrentPageIndex, this.PageSize);
            }
            else
            {
                this.DataSource = getStoreHelper(BindConnKey).GetDataTable(sql);
            }
        }

        /// <summary>
        /// 获取子定义项
        /// </summary>
        private string GetSubDefineByTagDefine(string tagdef, string internalTagStart, string internalTagEnd)
        {
            int idx = tagdef.IndexOf(internalTagStart, StringComparison.InvariantCultureIgnoreCase);
            if (idx != -1)
            {
                int idxBegin = idx + internalTagStart.Length;
                int idxEnd = tagdef.IndexOf(internalTagEnd, idxBegin, StringComparison.InvariantCultureIgnoreCase);
                if (idxEnd > idxBegin)
                {
                    return tagdef.Substring(idxBegin, idxEnd - idxBegin);
                }
                else
                {
                    throw new InvalidExpressionException("内部标签定义错误！(" + internalTagEnd + ")\n" + tagdef);
                }
            }
            return "";
        }

        /// <summary>
        /// 转化为列表标签
        /// </summary>
        public static ListTag Parse(TagBase tag)
        {
            return Parse(tag, null);
        }

        /// <summary>
        /// 转化为列表标签，并传递资源依赖对象
        /// </summary>
        public static ListTag Parse(TagBase tag, IResourceDependency res)
        {
            ListTag listTag = new ListTag();
            listTag.SetResourceDependency(res);
            listTag.InheriteFromBase(tag);
            listTag.SetListTemplet(tag.OuterDefineText);
            return listTag;
        }

        internal void InheriteFromBase(TagBase tag)
        {
            string strTemp = tag.GetAttribute("ID");
            //标志
            if (!string.IsNullOrEmpty(strTemp)) { this.IDentity = strTemp; }

            //指定另外的Access数据库
            strTemp = tag.GetAttribute("ConnKey");
            if (!string.IsNullOrEmpty(strTemp)) { this.BindConnKey = strTemp; }

            //指定前几条
            strTemp = tag.GetAttribute("Top");
            if (!string.IsNullOrEmpty(strTemp)) { this.TopCount = int.Parse(strTemp); }

            //指定每行显示的项数
            strTemp = tag.GetAttribute("RowSize");
            if (!string.IsNullOrEmpty(strTemp)) { this.RowSize = int.Parse(strTemp); }

            strTemp = tag.GetAttribute("PageSize");
            if (!string.IsNullOrEmpty(strTemp)) { this.PageSize = int.Parse(strTemp); }

            //指定绑定表格
            strTemp = tag.GetAttribute("Table");
            if (!string.IsNullOrEmpty(strTemp)) { this.BindTableName = strTemp; }

            //指定选取字段列表
            strTemp = tag.GetAttribute("Fields");
            if (!string.IsNullOrEmpty(strTemp)) { this.Fields = strTemp.Trim('"', '\''); }

            //指定选取字段列表
            strTemp = tag.GetAttribute("OrderBy");
            if (!string.IsNullOrEmpty(strTemp)) { this.OrderBy = strTemp; }

            //自定义查询条件
            strTemp = tag.GetAttribute("Query");
            if (!string.IsNullOrEmpty(strTemp)) { this.Query = strTemp; return; }
        
            object objTemp = null;
            #region sql查询条件
            foreach (string s in tag.GetObjAttributes())
            {
                string value = tag.GetAttribute(s);

                //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " Value:" + value); 

                if ((s[0] == '%' && s[s.Length - 1] == '%') ||
                    (s[0] == '(' && value[value.Length - 1] == ')'))
                {
                    strTemp = (s[1] != '@') ? ((s[1] == '~') ? " like " : "=") : "";
                    string newCondition = s.Trim('%', '(', '~', '@')
                        + strTemp 
                        + value.Replace('"', '\'').Trim(')');

                    #region 处理值中的系统标签或函数
                    if (value.StartsWith("$") && value.EndsWith("$"))
                    {
                        //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " Value:" + value); 
                        if (this.GetResourceDependency() != null)
                        {
                            //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " Res:" + this.GetResourceDependency().ToString());

                            objTemp = this.GetResourceDependency().GetDefinition("{#" + value + "#}");

                            if (string.IsNullOrEmpty(objTemp.ToString()))
                            {
                                newCondition = "";
                            }
                            else
                            {
                                newCondition = s.Trim('%', '(', '~', '@') + strTemp + objTemp.ToString();
                            }

                        }
                        else
                        {
                            newCondition = "";
                        }
                    }
                    #endregion

                    if (newCondition.Length > 0)
                    {
                        if (this._filterCondition != "")
                        {
                            this._filterCondition += " And " + newCondition;
                        }
                        else
                        {
                            this._filterCondition = newCondition;
                        }
                    }
                }
            }
            //Util.Debug(false, this.FilterCondition);
            #endregion
        }

        /// <summary>
        /// 获取该标签的HTML输出
        /// </summary>
        public override object GetTagValue()
        {
            SetDataSource();
            StringBuilder sb = new StringBuilder();
            sb.Append(TagBase.InterpretContentWithTags(this.HeaderTemplet, this));
            if (this.DataSource != null && this.DataSource.Rows.Count > 0)
            {
                int rowCount = 0, rowIndex = 0;

                #region 设置开始结束索引
                this.StartIndex = 1 + (PageSize * (CurrentPageIndex - 1));
                this.EndIndex = (this.StartIndex + this.DataSource.Rows.Count -1);
                #endregion

                for (int i = 0; i < this.DataSource.Rows.Count; i++)
                {
                    ++rowCount;
                    //行在整个数据源中的索引
                    rowIndex = rowCount + (PageSize*(CurrentPageIndex-1));

                    #region 单元项
                    if (rowCount % 2 == 0 && this.AlterItemTemplet != "")
                    {
                        sb.Append(TagBase.GetDataEscapedValue(rowIndex, this.AlterItemTemplet, this.DataSource.Rows[i], new MultiResDependency(this, GetResourceDependency()) ));
                    }
                    else
                    {
                        sb.Append(TagBase.GetDataEscapedValue(rowIndex, this.ItemTemplet, this.DataSource.Rows[i], new MultiResDependency(this, GetResourceDependency()) ));
                    } 
                    #endregion

                    #region 单元项间隔
                    if (this.RowSize > 1 && (rowCount % this.RowSize != 0))
                    {
                        //多项分行间隔
                        sb.Append(this.SeparatorTemplet);
                    }
                    else
                    {
                        //单项间隔
                        if (rowCount < this.DataSource.Rows.Count)
                        {
                            sb.Append(this.SeparatorTemplet);
                        }
                    } 
                    #endregion

                    #region 行分隔
                    if (this.RowSize > 1 && rowCount % this.RowSize == 0
                        && rowCount < this.DataSource.Rows.Count)
                    {
                        sb.Append(this.RowBreakTemplet);
                    } 
                    #endregion

                }
            }
            else
            {
                sb.Append(this.EmptyTemplet);
            }
            sb.Append(TagBase.InterpretContentWithTags(this.FooterTemplet,this));
            return sb.ToString();
        }

        #region IPagedContent Members
        /// <summary>
        /// 移动到下一页
        /// </summary>
        public void MoveNextPage()
        {
            ++_CurrentPageIndex;
        }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage()
        {
            if (this.PageSize == 0)
            {
                return false;
            }
            else
            {
                return (this.GetTotalRecordCount() > (this.PageSize * CurrentPageIndex));
            }
        }

        private int _CurrentPageIndex = 1;
        /// <summary>
        /// 当前数据分页所在页数
        /// </summary>
        public int CurrentPageIndex
        {
            get { return _CurrentPageIndex; }
            set 
            {
                if (value > 0 && value < GetPageCount())
                {
                    _CurrentPageIndex = value;
                }
                else
                {
                    _CurrentPageIndex = (value <= 0) ? 1 : GetPageCount();
                }
            }
        }

        private int _pageSize = 0;
        /// <summary>
        /// 每页显示条数
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        private int _startIndex = 0;
        /// <summary>
        /// 开始索引
        /// </summary>
        public int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }

        private int _endIndex = 0;
        /// <summary>
        /// 结束索引
        /// </summary>
        public int EndIndex
        {
            get { return _endIndex; }
            set { _endIndex = value; }
        }

        private int _PageCount = -1;
        /// <summary>
        /// 记录列表的中页数
        /// </summary>
        public int GetPageCount()
        {
            if (this._PageCount == -1)
            {
                int total = this.GetTotalRecordCount();
                if (total > 0 && this.PageSize > 0)
                {
                    int totalPage = total / this.PageSize;
                    this._PageCount = (total % PageSize == 0) ? totalPage : (totalPage + 1);
                }
                else
                {
                    this._PageCount = 0;
                }
            }
            return this._PageCount;
        }


        private int _totalRecordCount = -1;
        /// <summary>
        /// 获取当前分页数据源的总记录条数
        /// </summary>
        public int GetTotalRecordCount()
        {
            if (_totalRecordCount == -1)
            {
                if (this.DataSource == null && this.TopCount > 0)
                {
                    SetDataSource();
                    _totalRecordCount = (this.DataSource != null) ? this.DataSource.Rows.Count : 0;
                }
                else
                {
                    #region 获取满足条数的所有数据
                    string sql = string.Format("select count(*) from {0}{1}",
                        this.BindTableName,
                        string.IsNullOrEmpty(this.FilterCondition) ? "" : " where " + this.FilterCondition);

                    //Util.Debug(false, "Total:" + sql);
                    this._totalRecordCount = Convert.ToInt32(getStoreHelper(BindConnKey).ExecuteScalar(sql));
                    #endregion
                }
            }
            return this._totalRecordCount;
        }

        /// <summary>
        /// 获取当前页内容
        /// </summary>
        public string GetCurrentPageContent()
        {
            return GetTagValue().ToString();
        }

        /// <summary>
        /// 释放内存资源
        /// </summary>
        public void free()
        {
            this.Dispose();
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.DataSource != null) { this.DataSource.Dispose(); }
            this.HeaderTemplet = this.ItemTemplet = this.AlterItemTemplet = this.FooterTemplet = this.RowBreakTemplet = this.EmptyTemplet = null;
            this.OuterDefineText = this.OrderBy = this.BindTableName = null;
        }

        #endregion

        internal const string KEYWORDS = ",TOTAL,BIDX,EIDX,CIDX,PageCount,PageSize,";
        #region IResourceDependency Members

        /// <summary>
        /// 获取相关定义
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public object GetDefinition(string x)
        {
            x = x.Trim('$', '{', '#', '}');
            //TOTAL,BIDX,EIDX,CIDX,PageCount,PageSize,FirstUrl,PreUrl,NextUrl,LastUrl,CurUrl
            object objRet = "";
            switch (x.ToLower())
            {
                case "total": objRet = this.GetTotalRecordCount(); break;
                case "bidx": objRet = this.StartIndex; break;
                case "cidx": objRet = this.CurrentPageIndex; break;
                case "eidx": objRet = this.EndIndex; break;
                case "pagecount": objRet = this.GetPageCount(); break;
                case "pagesize": objRet = this.PageSize; break;
                default:
                    objRet = "";
                    break;
            }
            return objRet.ToString();
        }

        /// <summary>
        /// 返回特定对象是否有定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>是否定义过该对象</returns>
        public bool IsDefined(string x)
        {
            x = x.Trim('$', '{', '#', '}');
            return (KEYWORDS.IndexOf("," + x + ",", StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        public string DependencyIdentity
        {
            get { return "ListTagDependency#" + this.IDentity; }
        }

        #endregion
    }
}
