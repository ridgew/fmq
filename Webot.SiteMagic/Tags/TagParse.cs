using System;
using System.Collections.Generic;
using System.Text;
using Webot.Common;
using System.Collections;
using System.Text.RegularExpressions;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 标签解析类
    /// </summary>
    public class TagParse : IInternalTag, IDisposable
    {
        /// <summary>
        /// 标签解析类实例
        /// </summary>
        public TagParse() 
        {

        }
        
        /// <summary>
        /// 标签解析类实例
        /// </summary>
        /// <remarks>
        /// a: {#$ TrimHTML(%Title%) $#}
        /// b: {#$ Repeat(5,%Title%) $#}
        /// c: {#$ Length(TrimHTML(%Title%)) $#}
        /// d: {#$ Length(TrimHTML(%Title%)) > 3 ? "太长" : "OK" $#}
        /// e: {#$ Replace(TrimHTML(%Title%), "电子"," ") $#}
        /// f: {#$ ReplaceX(TrimHTML(%Title%), "\w","") $#}
        /// g: {#$ [3,[4,5]][1][1] + 5 $#} = 10
        /// </remarks>
        /// <param name="tagdef">标签网站定义体</param>
        public TagParse(string tagdef)
        { 
            if (!tagdef.StartsWith("{#$") || !tagdef.EndsWith("$#}"))
            {
                throw new InvalidProgramException("标签定义错误！");
            }
            else
            {
                this._tagDef = tagdef;
                InitializeTag(tagdef.Substring(3, tagdef.Length - 6).Trim());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagParse"/> class.
        /// </summary>
        /// <param name="tagdef">The tagdef.</param>
        /// <param name="res">The res.</param>
        public TagParse(string tagdef, IResourceDependency res)
        {
            if (!tagdef.StartsWith("{#$") || !tagdef.EndsWith("$#}"))
            {
                throw new InvalidProgramException("标签定义错误！");
            }
            else
            {
                SetResourceDependency(res);
                this._tagDef = tagdef;
                InitializeTag(tagdef.Substring(3, tagdef.Length - 6).Trim());
            }
        }

        private List<InnerExpression> innerExp = null;

        private void InitializeTag(string tagBody)
        {
            this.innerExp = GetInnerExpressions(tagBody, this.GetResourceDependency());
        }

        /// <summary>
        /// 转换描述文本为表达式数组
        /// </summary>
        /// <remarks>
        /// a: {#$ TrimHTML(%Title%) $#}
        /// b: {#$ Repeat(5,%Title%) $#}
        /// c: {#$ Length(TrimHTML(%Title%)) $#}
        /// d: {#$ Length(TrimHTML(%Title%)) > 3 ? "太长" : "OK" $#}
        /// e: {#$ Replace(TrimHTML(%Title%), "电子"," ") $#}
        /// f: {#$ ReplaceX(TrimHTML(%Title%), "\w","") $#}
        /// </remarks>
        public static List<InnerExpression> GetInnerExpressions(string ExpPaper, IResourceDependency Res)
        {
            //OleDbHelper.AppendToFile("~/debug.txt", "\n\n 解析：" + ExpPaper);
            List<InnerExpression> expList = new List<InnerExpression>();

            int lenT = ExpPaper.Length;
            int cursor = 0, iflag = 0;
            char chr = ExpPaper[cursor];
            string strTemp = "", strExpression = "";
            ReservedWords words = null;
                
            while (cursor < lenT)
            {
                #region 字符扫描
                chr = ExpPaper[cursor];
                if (!ReservedWords.IsReservedChar(chr))
                {
                    ++cursor;
                    continue;
                }
                else
                {
                    if (cursor > iflag)
                    {
                        strTemp = ExpPaper.Substring(iflag, cursor - iflag).Trim();
                    }
                    iflag = cursor;

                    words = new ReservedWords(chr);
                    if (words.IsBraceChar())
                    {
                        #region 配对字符解析
                        ReservedWords.MoveToCharBrace(chr, ReservedWords.GetBraceChar(chr),
                                ref cursor, ref ExpPaper);

                        if (chr == '(')
                        {
                            //Function
                            strExpression = ExpPaper.Substring(iflag + 1, cursor - iflag - 1);
                            //OleDbHelper.AppendToFile("~/debug.txt", "\n 函数体：" + strExpression);
                            if (strTemp.Length == 0) strTemp = null;
                            expList.Add(new InnerExpression(strTemp, strExpression, Res));
                        }
                        else if (chr == '?')
                        {
                            strExpression = ExpPaper.Substring(iflag + 1, cursor - iflag - 1).Trim();
                            #region 跳出双引号里面的 : 操作符号
                            if (strExpression.IndexOf('"') != -1 && strExpression[0] != '"')
                            {
                                ReservedWords.MoveToCharBrace('"', '"', ref cursor, ref ExpPaper);
                                ReservedWords.MoveToCharBrace(ExpPaper[cursor], ':', ref cursor, ref ExpPaper);
                                strExpression = ExpPaper.Substring(iflag + 1, cursor - iflag - 1).Trim();
                            }
                            #endregion

                            #region 跳出单引号里面的 : 操作符号
                            if (strExpression.IndexOf('\'') != -1 && strExpression[0] != '\'')
                            {
                                ReservedWords.MoveToCharBrace('\'', '\'', ref cursor, ref ExpPaper);
                                ReservedWords.MoveToCharBrace(ExpPaper[cursor], ':', ref cursor, ref ExpPaper);
                                strExpression = ExpPaper.Substring(iflag + 1, cursor - iflag - 1).Trim();
                            }
                            #endregion

                            if (strTemp.Length > 0)
                            {
                                expList.Add(new InnerExpression(strTemp));
                            }
                            expList.Add(new InnerExpression("?"));
                            expList.Add(new InnerExpression(strExpression));
                            expList.Add(new InnerExpression(":"));

                            //Util.Debug(false, ExpPaper.Substring(cursor));
                        }
                        else if (chr == '[')
                        {
                            // {#$["首页","新闻","动态","联系"][2]$#}	= "动态"
                            #region 数组情况
                            if (cursor < lenT - 1)
                            {
                                char aIdx = ExpPaper[cursor + 1];
                                while (aIdx == '[')
                                {
                                    cursor++;
                                    ReservedWords.MoveToCharBrace(aIdx, ReservedWords.GetBraceChar(aIdx), ref cursor, ref ExpPaper);
                                    if (cursor < (lenT - 1))
                                    {
                                        aIdx = ExpPaper[cursor + 1];
                                    }
                                    else { break; }
                                }
                                strExpression = ExpPaper.Substring(iflag, cursor - iflag + 1);
                                expList.Add(new InnerExpression(strExpression, ',', Res));
                            }
                            else
                            {
                                #region 获取数组下标操作TODO
                                strExpression = ExpPaper.Substring(iflag, cursor - iflag + 1);
                                expList.Add(new InnerExpression(strExpression, Res)); 
                                #endregion
                            }
                            #endregion
                        }
                        else if (chr == '$')
                        {
                            #region 内置系统标签
                            strExpression = ExpPaper.Substring(iflag, cursor - iflag + 1);
                            SystemTag sysTag = new SystemTag(string.Concat("{#", strExpression, "#}"));
                            sysTag.SetResourceDependency(Res);
                            //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + string.Concat("{#", strExpression, "#}", "\n", sysTag.ToString()));
                            expList.Add(new InnerExpression(sysTag.ToString()));
                            #endregion
                        }
                        else if (chr == '"' || chr == '\'')
                        {
                            strExpression = ExpPaper.Substring(iflag, cursor - iflag + 1);
                            //Util.Debug(false, "Find String:" + strExpression);
                            expList.Add(new InnerExpression(strExpression));
                        }
                        #endregion

                        iflag = cursor + 1;
                    }
                    else if (words.IsOperator())
                    {
                        strExpression = strTemp;
                        if (strExpression.Length > 0)
                        {
                            InnerExpression exp = new InnerExpression(strExpression);
                            expList.Add(exp);
                        }

                        #region 处理操作符号

                    ParseOperator:

                        char chrNext = ExpPaper[cursor + 1];
                        if ((chr == '+' || chr == '-') && char.IsNumber(chrNext))
                        {
                            #region 正负号处理
                            ++cursor;
                            if (cursor < lenT)
                            {
                                ReservedWords.MoveToCharInRange(ref cursor, ref ExpPaper, ' ', '*', '/', '%', '+', '-', '>', '<', '=', '!', '&', '^', '|');
                                expList.Add(new InnerExpression(ExpPaper.Substring(iflag, cursor - iflag)));

                                #region 如遇操作符
                                if (cursor < lenT && ExpPaper[cursor] != ' ')
                                {
                                    iflag = cursor;
                                    chr = ExpPaper[cursor];
                                    //Util.Debug(false, "new char: = [" + chr.ToString() + "]");
                                    goto ParseOperator;
                                }
                                #endregion
                            }
                            #endregion
                        }
                        else
                        {
                            // *= += -= ++ -- <>
                            if (ReservedWords.IsCharInRange(chrNext, '=', '+', '-', '>'))
                            {
                                expList.Add(new InnerExpression(chr.ToString() + chrNext.ToString()));
                                ++cursor;
                            }
                            else
                            {
                                expList.Add(new InnerExpression(chr.ToString()));
                            }
                        }
                        #endregion

                        iflag = cursor + 1;
                    }
                    else
                    {
                        if (strTemp.Length > 0)
                        {
                            expList.Add(new InnerExpression(strTemp));
                        }
                        //Util.Debug(false, "11 - [" + strTemp.Trim() + "]" + "chr=[" + chr.ToString() + "]");
                    }
                }
                ++cursor;
                #endregion
            }

            if (iflag < cursor)
            {
                expList.Add(new InnerExpression(ExpPaper.Substring(iflag, cursor - iflag).Trim()));
            }

            //#region 解析结果查看
            //foreach (InnerExpression ext in expList)
            //{
            //    //Util.Debug(false, string.Concat("Exp定义：", ext.TagDefinition));
            //    OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + string.Concat("Exp定义：", ext.TagDefinition));
            //}
            //#endregion
            return expList;
        }

        #region IInternalTag Members

        private string _tagDef = "";
        /// <summary>
        /// 标签定义
        /// </summary>
        public string TagDefinition
        {
            get
            {
                return this._tagDef;
            }
            set
            {
                this._tagDef = value;
            }
        }

        /// <summary>
        /// 是否有依赖性
        /// </summary>
        /// <value></value>
        public bool IsDependencyTag
        {
            get { return true; }
        }

        private IResourceDependency res;
        /// <summary>
        /// 获取该标题所依赖的资源
        /// </summary>
        /// <returns></returns>
        public IResourceDependency GetResourceDependency()
        {
            return res;
        }

        /// <summary>
        /// 设置该标题所依赖的资源
        /// </summary>
        /// <param name="value"></param>
        public void SetResourceDependency(IResourceDependency value)
        {
            res = value;
        }

        #endregion

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            object strRet = null;
            if (this.innerExp != null)
            {
                strRet = ExecExpressions(this.innerExp);
            }
            return (strRet != null) ? strRet.ToString() : "";
        }

        /// <summary>
        /// 获取最高运算优先级的索引/索引列表(同级左结合式)
        /// </summary>
        public static List<int> GetMaxPriorityExpIndexes(List<InnerExpression> exps)
        {
            List<int> oResList = new List<int>();
            int iPriority = 20, iCurPriority = 0;
            InnerOperate op = null;
            for (int i = 0, j = exps.Count; i < j; i++)
            {
                //Util.Debug(false, string.Concat(i, " : [",  exps[i].GetValue(), "] OP:" , exps[i].IsOperator()));
                if (exps[i].IsOperator())
                {
                    op = new InnerOperate(exps[i].GetValue().ToString());
                    iCurPriority = op.GetPriority();

                    //输出当前优先级
                    //Util.Debug(false, string.Concat( "[" , exps[i].GetValue().ToString().Trim(), "] = ", iCurPriority));

                    if (iCurPriority <= iPriority)
                    {
                        if (iCurPriority < iPriority)
                        {
                            iPriority = iCurPriority;
                            oResList.Clear();
                            oResList.Add(i);
                        }
                        else
                        {
                            oResList.Add(i);
                        }
                    }

                }
            }

            return oResList;
        }

        /// <summary>
        /// 计算表达式
        /// </summary>
        public static object ExecExpressions(List<InnerExpression> exps)
        {
            if (exps.Count == 0) return "";
            while (exps.Count > 1)
            {
                List<int> priIdxes = GetMaxPriorityExpIndexes(exps);
                int total = exps.Count;

                int offSet = 0, cIdx = 0;
                InnerExpression tempExp = null;
                InnerOperate op = null;

                if (priIdxes.Count == 0)
                {
                    #region 没有操作符号的数学运算
                    while (exps.Count >= 2)
                    {
                        tempExp = new InnerExpression();
                        if (Regex.IsMatch(exps[1].TagDefinition,
                            InnerExpression.ArrayIndexFetchPattern, RegexOptions.Compiled))
                        {
                            #region 数组下标直接获取 "ab"[0] = "a"
                            string strExpValue = exps[0].GetValue().ToString();
                            int arrIdx = Convert.ToInt32(exps[1].TagDefinition.Trim('[', ']'));
                            tempExp.TagDefinition = (arrIdx < strExpValue.Length) ? strExpValue[arrIdx].ToString() : strExpValue;
                            tempExp.IsString = true;
                            #endregion
                        }
                        else
                        {
                            tempExp.TagDefinition = InnerExpression.ComputeTwo(exps[0], new InnerOperate("+"), exps[1]).ToString();
                            tempExp.IsString = exps[0].IsString;
                        }
                        tempExp.IsEntity = true;
                        exps[0] = tempExp;
                        exps.RemoveAt(1);
                    } 
                    #endregion
                }
                else
                {
                    #region 处理同级运算
                    foreach (int idx in priIdxes)
                    {
                        cIdx = idx + offSet;
                        //Util.Debug(false, cIdx);
                        op = new InnerOperate(exps[cIdx].GetValue().ToString());
                        if (op.IsUnary())
                        {
                            #region 一元 自增/减操作 直接运算
                            // ++i
                            if ((idx + 1) < total)
                            {
                                tempExp = new InnerExpression();
                                tempExp.TagDefinition = InnerExpression.ComputeOne(exps[cIdx + 1], op).ToString();
                                tempExp.IsEntity = true;
                                exps[cIdx + 1] = tempExp;

                                exps.RemoveAt(cIdx);
                                offSet -= 1;
                            }
                            else
                            {
                                //i++
                                if (idx > 0)
                                {
                                    tempExp = new InnerExpression();
                                    tempExp.TagDefinition = InnerExpression.ComputeOne(exps[cIdx - 1], op).ToString();
                                    tempExp.IsEntity = true;
                                    exps[cIdx - 1] = tempExp;

                                    exps.RemoveAt(cIdx);
                                    offSet -= 1;
                                }
                            }
                            #endregion
                        }
                        else if (op.IsTernary())
                        {
                            //最低优先级 ? a : b
                            #region 三目条件运算符
                            if (idx + 3 < total)
                            {
                                tempExp = new InnerExpression();
                                tempExp.TagDefinition = (Convert.ToBoolean(exps[cIdx - 1].GetValue()) ? exps[cIdx + 1].GetValue() : exps[cIdx + 3].GetValue()).ToString();
                                tempExp.IsEntity = true;

                                exps[cIdx - 1] = tempExp;

                                exps.RemoveAt(cIdx);
                                exps.RemoveAt(cIdx);
                                exps.RemoveAt(cIdx);
                                exps.RemoveAt(cIdx);
                                offSet -= 4;
                            }
                            #endregion
                        }
                        else
                        {
                            #region 双目运算
                            if (exps.Count >= 3)
                            {
                                tempExp = new InnerExpression();
                                tempExp.TagDefinition = InnerExpression.ComputeTwo(exps[cIdx - 1], op, exps[cIdx + 1]).ToString();
                                tempExp.IsString = exps[cIdx - 1].IsString;
                                tempExp.IsEntity = true;
                                exps[cIdx - 1] = tempExp;

                                exps.RemoveAt(cIdx);
                                exps.RemoveAt(cIdx);
                                offSet -= 2;
                            }
                            else
                            {
                                //无效运算操作
                                exps.RemoveAt(cIdx);
                                offSet -= 1;
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                //Util.Debug(false, "当前还剩余：" + exps.Count.ToString());
                //foreach (InnerExpression tex in exps)
                //{
                //    Util.Debug(false, tex.GetValue());
                //}
            }
            return exps[0].GetValue();
        }

        /// <summary>
        /// 保留关键字词
        /// </summary>
        public class ReservedWords
        {
            /// <summary>
            /// 保留关键字符检测实例
            /// </summary>
            public ReservedWords(char chr)
            {
                this._innerCharDef = chr;
            }

            /// <summary>
            /// 保留关键字符串检测实例
            /// </summary>
            public ReservedWords(string str)
            {
                this._innerStrDef = str;
            }


            private char _innerCharDef=' ';
            private string _innerStrDef;

            
            /// <summary>
            /// 特定字符集合
            /// </summary>
            internal static char[] ReservedChars = new char[] { ' ','{', '}', '(', ')', '[', ']', '%', '$', '"', '\'',
                '<', '>', '=', '!', '+', '-', '*', '/',
                '^', '|', '&', '?', ':'};

            /// <summary>
            /// 特定字符串集合
            /// </summary>
            internal static string[] ReservedStrings = new string[] { "true", "false", "++", "--", "+=", "-=", "*=", "/=",
            "%=", "&=", "|=", "^=", "??", "&&", "||", "==", "!=", "<>",
            ">=", "<=", "<<", ">>" };

            /// <summary>
            /// 判断是否有结束的配对字符
            /// </summary>
            public bool IsBraceChar()
            {
                // ? :
                return IsCharInRange(this._innerCharDef, '[', '(', '"', '\'', '?', '$');
            }

            /// <summary>
            /// 判断是否是操作符号
            /// </summary>
            public bool IsOperator()
            {
                if (this._innerCharDef > 0)
                {
                    return IsCharInRange(this._innerCharDef, '<', '>', '=', '!', '+', '-', '*', '/',
                                '^', '|', '&', '?');
                }
                else
                {
                    return !IsStringInRange(this._innerStrDef, "true", "false");
                }
            }

            /// <summary>
            /// 获取相关字符的结尾字符
            /// </summary>
            public static char GetBraceChar(char chrBegin)
            {
                char braceChar = chrBegin;
                switch (chrBegin)
                {
                    case '(': braceChar = ')'; break;
                    case '[': braceChar = ']'; break;
                    case '{': braceChar = '}'; break;
                    case '?': braceChar = ':'; break;
                    default: break;
                }
                return braceChar;
            }

            /// <summary>
            /// 判断指定字符是否在字符集合中
            /// </summary>
            public static bool IsCharInRange(char chr, params char[] chRange)
            {
                bool blnInRange = false;
                foreach (char rChar in chRange)
                {
                    if (chr == rChar)
                    {
                        blnInRange = true;
                        break;
                    }
                }
                return blnInRange;
            }

            /// <summary>
            /// 判断是否属于保留字符
            /// </summary>
            public static bool IsReservedChar(char chr)
            {
                return IsCharInRange(chr, ReservedWords.ReservedChars);
            }

            /// <summary>
            /// 判断指定字符串是否在字符集合中
            /// </summary>
            public static bool IsStringInRange(string strCheck, params string[] chkRange)
            {
                bool blnInRange = false;
                foreach (string strTemp in chkRange)
                {
                    if (strTemp == strCheck)
                    {
                        blnInRange = true;
                        break;
                    }
                }
                return blnInRange;
            }

            /// <summary>
            /// 判断是否属于保留字符串
            /// </summary>
            public static bool IsReservedString(string strCheck)
            {
                return IsStringInRange(strCheck, ReservedWords.ReservedStrings);
            }

            /// <summary>
            /// 位移游标，匹配()[]对，支持\转义相同字符。
            /// </summary>
            /// <param name="chr">开始字符</param>
            /// <param name="chrEnd">结束字符</param>
            /// <param name="cursor">当前开始字符所在索引</param>
            /// <param name="source">文本查找扫描源</param>
            public static void MoveToCharBrace(char chr, char chrEnd, ref int cursor, ref string source)
            {
                #region 2009-1-13 增加函数内第一个参数数据类型为字符型的兼容判断
                if (chr == '(' && source.Length > cursor + 2 &&
                            (source[cursor + 1] == '\'' || source[cursor + 1] == '"'))
                {
                    char strDefChar = source[cursor + 1];
                    cursor++;
                    //位移到字符定义结束
                    MoveToCharBrace(strDefChar, GetBraceChar(strDefChar), ref cursor, ref source);
                    //继续原有位移操作
                    MoveToCharBrace(chr, chrEnd, ref cursor, ref source);
                    return;
                } 
                #endregion

            begin:
                int iflag = cursor;
                int idxBegin = cursor + 1;
                int idx = source.IndexOf(chrEnd, idxBegin);
                if (idx != -1)
                {
                    cursor = idx;
                    //空内容()
                    if (cursor - iflag < 2) return;

                    //找到转义字符
                    if (source[cursor - 1] == '\\')
                    {
                        goto begin;
                    }

                }
                else
                {
                    //没有配对
                    return;
                }

            findNext:
                idx = source.IndexOf(chr, idxBegin);
                // 在结束标记和开始标记之间有新的开始标记
                if (idx != -1 && source[idx - 1] != '\\' && idx < cursor)
                {
                    idxBegin = idx + 1;
                    idx = source.IndexOf(chrEnd, cursor + 1);
                    if (idx != -1)
                    {
                        cursor = idx;
                        goto findNext;
                    }
                }
            }

            /// <summary>
            /// 位移游标，直至出现rangChars中的任一字符
            /// </summary>
            /// <param name="cursor">当前游标位置</param>
            /// <param name="source">文本查找源</param>
            /// <param name="rangChars">出现则终止的字符集合</param>
            public static void MoveToCharInRange(ref int cursor, ref string source, params char[] rangChars)
            {
                while (cursor < source.Length && !IsCharInRange(source[cursor], rangChars))
                {
                    //OleDbHelper.AppendToFile("~/debug.txt", "\n" + source[cursor].ToString());
                    ++cursor;
                }
            }

            /// <summary>
            /// 获取按照指定字符分隔的字符数组(忽略容器内的分隔符)
            /// (Reversion:1.0 by Ridge Wong @ 2008-2-3)
            /// </summary>
            /// <param name="Source">数组获取源</param>
            /// <param name="separator">分隔符号</param>
            /// <param name="ContainerCharBegin">匹配容器开始字符，目前支持",',(,[,{,%开始的字符识别。</param>
            /// <returns>相关分隔符号之后的字符数组</returns>
            /// <remarks>分隔容器内的分隔符号将被忽略。</remarks>
            public static string[] GetStringArray(string Source, char separator, char[] ContainerCharBegin)
            {
                int cursor = 0, iflag = 0;
                int lenT = Source.Length;
                ArrayList arrResult = new ArrayList();
                
                while (cursor < lenT)
                {
                    char curChr = Source[cursor];
                    while(IsCharInRange(curChr, ContainerCharBegin))
                    {
                        MoveToCharBrace(curChr, GetBraceChar(curChr), ref cursor, ref Source);
                        if (cursor < lenT - 1)
                        {
                            curChr = Source[cursor + 1];
                            cursor++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (curChr == separator)
                    {
                        //Util.Debug(false, cursor, Source.Substring(iflag, cursor - iflag)); 
                        arrResult.Add(Source.Substring(iflag, cursor - iflag));
                        iflag = cursor + 1;
                    }
                    cursor++;
                }

                if (iflag < lenT)
                {
                    //Util.Debug(false, Source.Substring(iflag));
                    arrResult.Add(Source.Substring(iflag));
                }
                return (string[])arrResult.ToArray(typeof(string));
            }

        }

        /// <summary>
        /// 内部操作符号
        /// </summary>
        public class InnerOperate
        {
            /// <summary>
            /// 操作符
            /// </summary>
            public InnerOperate(string opStr)
            {
                if (PriorityDic.Count < 1)
                {
                    InitialDicPrority();
                }
                this._opStr = opStr;
            }

            /// <summary>
            /// 运算符号优先级表
            /// </summary>
            public struct PriorityMap
            {
                public int Priority;
                public string Operator;
                public PriorityMap(int iPrior, string sOperator)
                {
                    Priority = iPrior;
                    Operator = sOperator;
                }
            }

            /// <summary>
            /// 优先级词典
            /// </summary>
            public static Dictionary<string, PriorityMap> PriorityDic = new Dictionary<string, PriorityMap>();

            private string _opStr;
            /// <summary>
            /// 操作符定义/获取
            /// </summary>
            public string OperatorDefine
            {
                get { return _opStr; }
                set { _opStr = value; }
            }


            private void InitialDicPrority()
            {
                //省略了正负号 + -
                string opLib = "(,[ !,~,++,-- *,/,% +,- <<,>> >,<,>=,<= ==,!= & ^ | && || ? =,+=,-=,*=,/=,%=,>>=,<<=,&=,^=,|= ";
                int iPrority = 1;
                string key = "";
                int idxBegin = 0, idxEnd = 0;
                for (int i = 0; i < opLib.Length; i++)
                {
                    if (opLib[i] == ',' || opLib[i] == ' ')
                    {
                        idxEnd = i;
                        key = opLib.Substring(idxBegin, idxEnd - idxBegin);
                        //Util.Debug(false, iPrority + ":" + key);
                        PriorityDic.Add(key, new PriorityMap(iPrority, key));

                        if (opLib[i] == ' ') ++iPrority;
                        idxBegin = i + 1;
                    }
                }
            }

            /// <summary>
            /// 获取改运算符号的优先级
            /// </summary>
            public int GetPriority()
            {
                if (PriorityDic.ContainsKey(this._opStr))
                {
                    return PriorityDic[this._opStr].Priority;
                }
                else
                {
                    return 0;
                }
            }

            /// <summary>
            /// Determines whether this instance is assignment.
            /// </summary>
            /// <returns>
            /// 	<c>true</c> if this instance is assignment; otherwise, <c>false</c>.
            /// </returns>
            public bool IsAssignment()
            {
                return ReservedWords.IsStringInRange(this._opStr, "=", "+=", "-=", "*=", "/=",
                    "%=", "&=", "|=", "^=", "??", "<<=", ">>=");
            }

            /// <summary>
            /// Determines whether this instance is conditional.
            /// </summary>
            /// <returns>
            /// 	<c>true</c> if this instance is conditional; otherwise, <c>false</c>.
            /// </returns>
            public bool IsConditional()
            {
                return ReservedWords.IsStringInRange(this._opStr, "&&", "||", "?", "<", ">", ">=", "<=", "!=", "==");
            }

            /// <summary>
            /// Determines whether this instance is logical.
            /// </summary>
            /// <returns>
            /// 	<c>true</c> if this instance is logical; otherwise, <c>false</c>.
            /// </returns>
            public bool IsLogical()
            {
                return ReservedWords.IsStringInRange(this._opStr, "&", "|", "^");
            }

            /// <summary>
            /// 单目运算符
            /// </summary>
            public bool IsUnary()
            {
                return ReservedWords.IsStringInRange(this._opStr, "!", "~", "++", "--", "true", "false");
            }

            /// <summary>
            /// 双目运算符
            /// </summary>
            public bool IsBinary()
            {
                return ReservedWords.IsStringInRange(this._opStr, "*", "/", "%", "+", "-", "<<", ">>",
                    ">", "<", ">=", "<=", "==", "!=", "&", "^", "|", "&&", "||");
            }

            /// <summary>
            /// 三目运算符
            /// </summary>
            public bool IsTernary()
            {
                return (this._opStr == "?");
            }

            /// <summary>
            /// 是不是左结合性
            /// </summary>
            /// <returns></returns>
            public bool IsLeftToRight()
            {
                return (!IsUnary() && !IsTernary());
            }

        }

        /// <summary>
        /// 内部表达式
        /// </summary>
        public class InnerExpression : IInternalTag
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InnerExpression"/> class.
            /// </summary>
            public InnerExpression()
            { 
            
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerExpression"/> class.
            /// </summary>
            /// <param name="exp">The exp.</param>
            public InnerExpression(string exp)
            {
                InitialBase(exp);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerExpression"/> class.
            /// </summary>
            /// <param name="exp">The exp.</param>
            /// <param name="res">The res.</param>
            public InnerExpression(string exp, IResourceDependency res)
            {
                SetResourceDependency(res);
                InitialBase(exp);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerExpression"/> class.
            /// </summary>
            /// <param name="funName">Name of the fun.</param>
            /// <param name="paramsBody">The params body.</param>
            /// <param name="res">The res.</param>
            public InnerExpression(string funName, string paramsBody, IResourceDependency res)
            {
                SetResourceDependency(res);
                this.IsFunction = true;
                this._equalFun = new InnerFunction(funName, paramsBody, res);
                this.TagDefinition = this._equalFun.TagDefinition;
            }

            /// <summary>
            /// 初始化一个数组表达式
            /// </summary>
            /// <param name="arryDefine">数组定义</param>
            /// <param name="Separator">分隔字符</param>
            /// <param name="res">依赖资源</param>
            public InnerExpression(string arryDefine, char Separator, IResourceDependency res)
            {
                SetResourceDependency(res);
                ArraySeparator = Separator;
                IsArrayDefine = true;
                InitialBase(arryDefine);
            }

            /// <summary>
            /// 数组下标获取操作匹配模式
            /// </summary>
            public const string ArrayIndexFetchPattern = @"^\[\d+\]$";

            private string _expDef;

            private void InitialBase(string exp)
            {
                if (exp == null) throw new InvalidProgramException("param must not be null.");
                this._expDef = exp;
                if ((exp.StartsWith("\"") && exp.EndsWith("\"")) || (exp.StartsWith("'") && exp.EndsWith("'")))
                {
                    //Util.Debug(false, "Set as String");
                    this.IsString = true;
                }
            }

            private InnerFunction _equalFun = null;

            private bool _isFunction = false;
            /// <summary>
            /// 是否是函数
            /// </summary>
            public bool IsFunction
            {
                get { return _isFunction; }
                set { _isFunction = value; }
            }

            private bool _isString = false;
            /// <summary>
            /// 实体类型为字符型
            /// </summary>
            public bool IsString
            {
                get { return _isString; }
                set { _isString = value; }
            }


            private bool isEntity = false;
            /// <summary>
            /// 是否是操作数实体
            /// </summary>
            public bool IsEntity
            {
                get { return isEntity; }
                set { isEntity = value; }
            }

            private bool isArrayDefine = false;
            /// <summary>
            /// 是否是数组定义
            /// </summary>
            public bool IsArrayDefine
            {
                get { return isArrayDefine; }
                set { isArrayDefine = value; }
            }

            private char _arraySeparator = ',';
            /// <summary>
            /// 数组分隔符号
            /// </summary>
            public char ArraySeparator
            {
                get { return _arraySeparator; }
                set { _arraySeparator = value; }
            }

            #region 表达式判断
            public bool IsOperator()
            {
                if (this._expDef.Length == 1)
                {
                    return ReservedWords.IsCharInRange(this._expDef[0], '+', '-', '*', '/', '>', '<', '=', '%',
                        '!', '?', '|', '&', '^', '~');
                }
                else
                {
                    if (this._expDef.Length == 2)
                    {
                        return ReservedWords.IsStringInRange(this._expDef, "++", "--", "+=", "-=", "*=", "/=",
                                "%=", "&=", "|=", "^=", "??", "&&", "||", "==", "!=", "<>",
                                ">=", "<=", "<<", ">>");
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            //public bool IsFunctionName()
            //{
            //    return Regex.IsMatch(this._expDef, InnerFunction.FunctionNamePattern, RegexOptions.IgnoreCase);
            //}

            //public bool IsParamsBody()
            //{
            //    return (this._expDef.StartsWith("(") && this._expDef.EndsWith(")"));
            //}

            //public bool IsArrayBody()
            //{
            //    return (this._expDef.StartsWith("[") && this._expDef.EndsWith("]"));
            //}

            //public bool IsArrayIndex()
            //{
            //    return Regex.IsMatch(this._expDef, @"^\[(\d+)\]$", RegexOptions.IgnoreCase);
            //} 
            #endregion

            /// <summary>
            /// 获取表达式的最综值
            /// </summary>
            public object GetValue()
            {
                if (IsFunction == true && this._equalFun != null)
                {
                    this._equalFun.SetResourceDependency(this.GetResourceDependency());
                    return this._equalFun.Execute();
                }
                else
                {
                    string strResult = this.TagDefinition;
                    //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + "Define:" + strResult);
                    if (Regex.IsMatch(strResult, @"^(\+|\-)?[\d\.]+$", RegexOptions.Compiled))
                    {
                        //Util.Debug(false, "double: " + strResult);
                        return double.Parse(strResult);
                    }
                    else if (this.IsString == true)
                    {
                        if (strResult.StartsWith("\"") && strResult.EndsWith("\""))
                        {
                            return strResult.Trim('"');
                        }
                        else if (strResult.StartsWith("\'") && strResult.EndsWith("\'"))
                        {
                            return strResult.Trim('\'');
                        }
                        else
                        {
                            return strResult.Trim();
                        }
                    }
                    else if (IsArrayDefine == true)
                    {
                        //{#$[["首页","新闻"],["动态","联系"]][1][1]$#} = "新闻"
                        //{#$["首页","新闻","动态","联系"][2]$#} = "新闻"
                        //{#$[["首页","新闻"],(aaa,bbb,ccc),["动a态","联b系"],["动c态","联d系"],"a"][1][1]$#}  = "新闻"
                        // Replace(TrimHTML(%Title%), "电,子"," ")[0]
                        // {#$[["首页","新闻"],["动态",(1+2)]][1][1] + 6 $#} = 9
                        int cursor = 0, iflag = 0;
                        int lenT = strResult.Length;
                        ReservedWords.MoveToCharBrace('[', ']', ref cursor, ref strResult);
                        string arrayBody = strResult.Substring(1, cursor - 1);
                        if (cursor >= lenT - 1)
                        {
                            return arrayBody;
                        }
                        else
                        {
                            cursor++;
                            iflag = cursor;
                            object arrResult = "";

                            //Util.Debug(false, strResult.Substring(cursor));
                        FetchArrayItem:

                            #region 根据数组数据定义获取下标

                            ReservedWords.MoveToCharBrace('[', ']', ref cursor, ref strResult);
                            string idxFetch = strResult.Substring(iflag, cursor - iflag + 1);
                            //Util.Debug(false, idxFetch);
                            if (!Regex.IsMatch(idxFetch, ArrayIndexFetchPattern, RegexOptions.Compiled))
                            {
                                //return "bad";
                                throw new InvalidOperationException("数组索引定义错误！");
                            }
                            else
                            {
                                string[] arrDat = ReservedWords.GetStringArray(arrayBody, ArraySeparator, new char[] { '[', '"', '\'', '(' });
                                arrResult = GetArrayItemValue(arrayBody, ArraySeparator,
                                    int.Parse(idxFetch.Trim('[', ']')), GetResourceDependency());

                                if (cursor < lenT - 1)
                                {
                                    #region 检查是否继续获取下级数组下标
                                    if (strResult[cursor + 1] == '[')
                                    {
                                        cursor++;
                                        iflag = cursor;
                                        arrayBody = arrResult.ToString();
                                        if (arrayBody.StartsWith("[") && arrayBody.EndsWith("]"))
                                        {
                                            arrayBody = arrayBody.Trim('[', ']');
                                            goto FetchArrayItem;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion

                            return arrResult;

                        }
                    }
                    else
                    {
                        return strResult;
                    }
                }
            }

            /// <summary>
            /// 计算两个表达式的值
            /// </summary>
            public static object ComputeTwo(InnerExpression lExp, InnerOperate op, InnerExpression rExp)
            {
                object oRet = "";
                object lValue = lExp.GetValue();
                object rValue = rExp.GetValue();

                //OleDbHelper.AppendToFile("~/debug.txt", "\n" + "Left:" + lValue.GetType().ToString()
                //    + " Right:" + rValue.GetType().ToString());
                
                //Util.Debug(false, lValue.GetType(), rValue.GetType());
                //return "";

                if (lValue.GetType() != rValue.GetType())
                {
                    try
                    {
                        rValue = Convert.ChangeType(rValue, lValue.GetType());
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException(string.Format("{0}与{1}不支持运算,且类型不兼容！", lValue, rValue));
                    }
                }

                string opDef = op.OperatorDefine;
                #region 处理比较操作
                if (ReservedWords.IsStringInRange(opDef, ">", "<", ">=", "<=", "!=", "==", "<>"))
                {
                    if (!(lValue is IComparable) || !(rValue is IComparable))
                    {
                        throw new InvalidOperationException(string.Format("{0}与{1}不支持比较！", lValue, rValue));
                    }
                    IComparable ica = lValue as IComparable;
                    IComparable icb = rValue as IComparable;
                    switch (opDef)
                    {
                        case ">": oRet = (ica.CompareTo(icb) > 0); break;
                        case "<": oRet = (ica.CompareTo(icb) < 0); break;
                        case ">=": oRet = (ica.CompareTo(icb) >= 0); break;
                        case "<=": oRet = (ica.CompareTo(icb) <= 0); break;
                        case "!=": oRet = (ica.CompareTo(icb) != 0); break;
                        case "<>": oRet = (ica.CompareTo(icb) != 0); break;
                        case "==": oRet = (ica.CompareTo(icb) == 0); break;
                        default: break;
                    }
                } 
                #endregion

                #region 双目基本运算
                if (ReservedWords.IsStringInRange(opDef, "*", "/", "%", "+", "-"))
                {
                    if (lValue.GetType() == typeof(string))
                    {
                        #region 字符型
                        if (op.OperatorDefine == "+")
                        {
                            oRet = String.Concat(lValue.ToString(), rValue.ToString());
                        }
                        else
                        {
                            oRet = lValue.ToString().Replace(rValue.ToString(), "");
                        } 
                        #endregion
                    }
                    else
                    {
                        #region 数字型
                        double dba = Convert.ToDouble(lValue);
                        double dbb = Convert.ToDouble(rValue);
                        switch (opDef)
                        {
                            case "*": oRet = (dba * dbb); break;
                            case "/": oRet = (dba / dbb); break;
                            case "%": oRet = (dba % dbb); break;
                            case "+": oRet = (dba + dbb); break;
                            case "-": oRet = (dba - dbb); break;
                            default: break;
                        } 
                        #endregion
                    }
                } 
                #endregion

                //return string.Format("{0}{1}{2}", lExp.GetValue(), op.OperatorDefine , rExp.GetValue());
                return oRet;
            }

            /// <summary>
            /// 计算一个表达式的值
            /// </summary>
            /// <param name="exp">The exp.</param>
            /// <param name="op">The op.</param>
            /// <returns></returns>
            public static object ComputeOne(InnerExpression exp, InnerOperate op)
            {
                //"!", "~", "++", "--", "true", "false"
                object oRet = exp.GetValue();
                if (op.OperatorDefine == "!")
                {
                    return !(Convert.ToBoolean(oRet));
                }
                else if (op.OperatorDefine == "~")
                {
                    return ~Convert.ToChar(oRet);
                }
                else if (op.OperatorDefine == "++" || op.OperatorDefine == "--")
                {
                    if (!Regex.IsMatch(oRet.ToString(), @"^(\+|\-)?[\d\.]+$", RegexOptions.Compiled))
                    {
                        throw new InvalidOperationException("数据不支持自增/减运算！");
                    }
                    else
                    {
                        double odb = double.Parse(oRet.ToString());
                        return (op.OperatorDefine == "++") ? odb++ : odb--;
                    }
                }
                return oRet;
            }

            /// <summary>
            /// 获取相关数组指定下标的值
            /// </summary>
            /// <param name="ArrayBodyDefine">数组定义体，形如：["a","b"]。</param>
            /// <param name="separator">数组元素分隔字符</param>
            /// <param name="idx">数组元素的下标</param>
            /// <param name="res">相关依赖资源定义</param>
            /// <returns>如果存在改下标的数据则返回，否则返回空字符。</returns>
            public static object GetArrayItemValue(string ArrayBodyDefine, char separator, int idx, IResourceDependency res)
            {
                string[] arrDat = ReservedWords.GetStringArray(ArrayBodyDefine, separator, new char[] { '[', '"', '\'', '(' });
                return GetArrayItemValue(arrDat, idx, res);
            }

            /// <summary>
            /// 获取相关数组指定下标的值
            /// </summary>
            /// <param name="ArrayDat">数组数据</param>
            /// <param name="idx">数组元素的下标</param>
            /// <param name="res">相关依赖资源定义</param>
            /// <returns>如果存在改下标的数据则返回，否则返回空字符。</returns>
            public static object GetArrayItemValue(string[] ArrayDat, int idx, IResourceDependency res)
            {
                if (ArrayDat == null || idx > ArrayDat.Length)
                {
                    return "";
                }
                else
                { 
                    InnerParam param = new InnerParam(ArrayDat[idx], res);
                    return param.GetValue();
                }
            }

            /// <summary>
            /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
            /// </returns>
            public override string ToString()
            {
                return "Expresion:" + this._expDef;
            }

            #region IInternalTag Members

            public string TagDefinition
            {
                get
                {
                    return this._expDef;
                }
                set
                {
                    this._expDef = value;
                }
            }

            public bool IsDependencyTag
            {
                get { return !this.IsFunction; }
            }

            private IResourceDependency Res;
            public IResourceDependency GetResourceDependency()
            {
                return this.Res;
            }

            public void SetResourceDependency(IResourceDependency value)
            {
                this.Res = value;
            }
            #endregion
        }

        /// <summary>
        /// 内部参数
        /// </summary>
        public class InnerParam : IInternalTag
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InnerParam"/> class.
            /// </summary>
            public InnerParam()
            { 
            
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerParam"/> class.
            /// </summary>
            /// <param name="paramdef">The paramdef.</param>
            public InnerParam(string paramdef)
            {
                this.ParamDefine = paramdef;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerParam"/> class.
            /// </summary>
            /// <param name="paramdef">The paramdef.</param>
            /// <param name="res">The res.</param>
            public InnerParam(string paramdef, IResourceDependency res)
            {
                SetResourceDependency(res);
                //OleDbHelper.AppendToFile("~/debug.txt", "\n\n 参数定义：" + paramdef);
                this.ParamDefine = paramdef;
            }

            private string _paramDef;
            /// <summary>
            /// 参数定义
            /// </summary>
            public string ParamDefine
            {
                get { return this._paramDef; }
                set 
                {
                    if (value.StartsWith("(") && value.EndsWith(")"))
                    {
                        //匿名函数
                        this.IsFunction = true;
                    }
                    else if (Regex.IsMatch(value, InnerFunction.FunctionNamePattern.TrimEnd('$') + "\\(",
                        RegexOptions.IgnoreCase))
                    {
                        //普通函数
                        this.IsFunction = true;
                    }
                    this._paramDef = value;
                }
            }

            private bool _isFunction = false;
            /// <summary>
            /// 参数是函数
            /// </summary>
            public bool IsFunction
            {
                get { return this._isFunction; }
                set  {  _isFunction = value;  }
            }

            private bool _isExpression = false;
            /// <summary>
            /// 参数是表达式
            /// </summary>
            public bool IsExpression
            {
                get { return _isExpression; }
                set { _isExpression = value; }
            }

            /// <summary>
            /// 获取改参数的真实值
            /// </summary>
            public object GetValue()
            {
                //Util.Debug(false, "Get Param Value:[" + this.ParamDefine + "] Is Fun:" + IsFunction.ToString());
                if (this.ParamDefine == null) return null;

                if (IsExpression == true)
                {
                    InnerExpression exp = new InnerExpression(this.ParamDefine, GetResourceDependency());
                    return exp.GetValue();
                }
                else if (IsFunction == true)
                {
                    int idx = this.ParamDefine.IndexOf('(');
                    string funName = (idx == 0) ? null : this.ParamDefine.Substring(0, idx);
                    //Util.Debug(false, this.ParamDefine.Substring(idx + 1).TrimEnd(')'));
                    InnerFunction fun = new InnerFunction(funName,
                        this.ParamDefine.Substring(idx+1).TrimEnd(')'),
                        this.GetResourceDependency());
                    return fun.Execute();
                }
                else
                {
                    #region 字符型
                    if (this.ParamDefine.StartsWith("\"") && this.ParamDefine.EndsWith("\""))
                    {
                        return this.ParamDefine.Trim('"');
                    }
                    else if (this.ParamDefine.StartsWith("'") && this.ParamDefine.EndsWith("'"))
                    {
                        return this.ParamDefine.Trim('\'');
                    } 
                    #endregion

                    if (this.ParamDefine.StartsWith("%") && this.ParamDefine.EndsWith("%"))
                    {
                        #region 已定义资源数据
                        string define = string.Concat("{#", this.ParamDefine, "#}");
                        if (this.Res != null && Res.IsDefined(define))
                        {
                            return Res.GetDefinition(define);
                        }
                        else
                        {
                            return "";
                        } 
                        #endregion
                    }
                    else if (this.ParamDefine.StartsWith("$") && this.ParamDefine.EndsWith("$"))
                    {
                        #region 系统标签
                        SystemTag sysTag = new SystemTag(string.Concat("{#", this.ParamDefine, "#}"));
                        sysTag.SetResourceDependency(GetResourceDependency());
                        return sysTag.ToString(); 
                        #endregion
                    }
                    else
                    {
                        #region 布尔及数字型
                        if (string.Compare(this.ParamDefine, "true", true) == 0)
                        {
                            return true;
                        }
                        else if (string.Compare(this.ParamDefine, "false", true) == 0)
                        {
                            return false;
                        }
                        else
                        {
                            if (Regex.IsMatch(this.ParamDefine, @"^[\d,]+$", RegexOptions.Compiled))
                            {
                                try
                                {
                                    double dec = double.Parse(this.ParamDefine);
                                    return dec;
                                }
                                catch (Exception)
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                return this.ParamDefine;
                            }
                        }
                        #endregion
                    }
                }
            }


            #region IInternalTag Members

            public string TagDefinition
            {
                get
                {
                    return this.ParamDefine;
                }
                set
                {
                    this.ParamDefine = value;
                }
            }

            public bool IsDependencyTag
            {
                get 
                {
                    return (this.ParamDefine != null && 
                        Regex.IsMatch(this.ParamDefine, @"%(\w)+%", RegexOptions.IgnoreCase)); 
                }
            }

            private IResourceDependency Res = null;
            public IResourceDependency GetResourceDependency()
            {
                return Res;
            }

            public void SetResourceDependency(IResourceDependency value)
            {
                this.Res = value;
            }

            #endregion
        }

        /// <summary>
        /// 内部函数
        /// </summary>
        public class InnerFunction : IInternalTag
        {
            /// <summary>
            /// 内部函数调用实例
            /// </summary>
            public InnerFunction(string funName, string paramsBody)
            {
                this.FunctionName = funName;
                SetFunctionParams(paramsBody);
                this.TagDefinition = string.Format("{0}({1})", funName, paramsBody);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerFunction"/> class.
            /// </summary>
            /// <param name="funName">Name of the fun.</param>
            /// <param name="paramsBody">The params body.</param>
            /// <param name="res">The res.</param>
            public InnerFunction(string funName, string paramsBody, IResourceDependency res)
            {
                SetResourceDependency(res);
                this.FunctionName = funName;
                SetFunctionParams(paramsBody);
                this.TagDefinition = string.Format("{0}({1})", funName, paramsBody);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerFunction"/> class.
            /// </summary>
            /// <param name="funName">Name of the fun.</param>
            /// <param name="res">The res.</param>
            /// <param name="objParams">The obj params.</param>
            public InnerFunction(string funName, IResourceDependency res, params InnerParam[] objParams)
            {
                this.FunctionName = funName;
                SetResourceDependency(res);
                StringBuilder paramsBody = new StringBuilder();
                foreach (InnerParam param in objParams)
                {
                    this.fParamList.Add(param);
                    paramsBody.Append("," + param.TagDefinition);
                }
                this.TagDefinition = string.Format("{0}({1})", funName, paramsBody.ToString().TrimStart(','));
            }

            /// <summary>
            /// 函数名称匹配模式
            /// </summary>
            public const string FunctionNamePattern = @"^[a-z\$]([a-z\$0-9_]+)?$";

            private void SetFunctionParams(string paramsBody)
            {
                this.ParamsBody = paramsBody;
                if (paramsBody != null)
                {
                    if (paramsBody.IndexOf(",") == -1)
                    {
                        fParamList.Add(new InnerParam(paramsBody, GetResourceDependency()));
                    }
                    else
                    {
                        //string[] objStrList = paramsBody.Split(',');
                        string[] objStrList = ReservedWords.GetStringArray(paramsBody, ',', new char[] { '(', '"', '\''});
                        foreach (string paramStr in objStrList)
                        {
                            fParamList.Add(new InnerParam(paramStr.Trim(), GetResourceDependency()));
                        }
                    }
                }
            }

            private string _paramsBody;
            /// <summary>
            /// 参数定义
            /// </summary>
            public string ParamsBody
            {
                get { return _paramsBody; }
                set { _paramsBody = value; }
            }
            

            private string _funName;
            /// <summary>
            /// 函数名称
            /// </summary>
            public string FunctionName
            {
                get { return _funName; }
                set { _funName = value; }
            }

            private List<InnerParam> fParamList = new List<InnerParam>();
            
            /// <summary>
            /// 函数参数
            /// </summary>
            public object[] GetFunctionParams()
            {
                object[] objParams = new object[fParamList.Count];
                for (int i = 0; i < fParamList.Count; i++)
                {
                    objParams[i] = fParamList[i].GetValue();
                    #region 参数结果调试
                    //if (objParams[i] != null)
                    //{
                    //    Util.Debug(false, "Params " + i + ":" + objParams[i].ToString());
                    //}
                    //else
                    //{
                    //    Util.Debug(false, "IsFun: " + fParamList[i].IsFunction.ToString() 
                    //        + " Params " + i + ":" + fParamList[i].TagDefinition);
                    //} 
                    #endregion
                }
                return objParams;
            }

            /// <summary>
            /// 获取函数的最终运行结果
            /// </summary>
            public object Execute()
            {

                #region 匿名函数
                if (this.FunctionName == null)
                {
                    if (this.ParamsBody == null) return null;
                    List<InnerExpression> expList = TagParse.GetInnerExpressions(this.ParamsBody,
                        GetResourceDependency());
                    //foreach (InnerExpression ext in expList)
                    //{
                    //    Util.Debug(false, string.Concat("匿名表达式定义：", ext.TagDefinition));
                    //}
                    return TagParse.ExecExpressions(expList);
                }

                #endregion

                Type tInvoke = typeof(SystemTag);
                if (this.FunctionName.IndexOf('.') != -1)
                {
                    int idx = this.FunctionName.LastIndexOf('.');
                    string strTemp = this.FunctionName.Substring(0, idx+1);
                    if (strTemp == ".") strTemp = "Webot.Common.Util";

                    tInvoke = Type.GetType(strTemp.TrimEnd('.'), true, true);
                    this.FunctionName = this.FunctionName.Substring(idx + 1);
                }
                return Util.InvokeFunction(tInvoke, this.FunctionName, GetFunctionParams());
            }

            /// <summary>
            /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
            /// </returns>
            public override string ToString()
            {
                return String.Format("Function {0}( {1} )", this.FunctionName, this.ParamsBody);
            }

            #region IInternalTag Members
            private string _tagdef = "";
            /// <summary>
            /// 标签定义文本
            /// </summary>
            /// <value></value>
            public string TagDefinition
            {
                get
                {
                    return this._tagdef;
                }
                set
                {
                    this._tagdef = value;
                }
            }

            /// <summary>
            /// 是否有依赖性
            /// </summary>
            /// <value></value>
            public bool IsDependencyTag
            {
                get 
                {
                    return (this.ParamsBody != null);
                }
            }

            private IResourceDependency res;
            /// <summary>
            /// 获取该标题所依赖的资源
            /// </summary>
            /// <returns></returns>
            public IResourceDependency GetResourceDependency()
            {
                return res;
            }

            /// <summary>
            /// 设置该标题所依赖的资源
            /// </summary>
            /// <param name="value"></param>
            public void SetResourceDependency(IResourceDependency value)
            {
                res = value;
            }

            #endregion
        }


        #region IDisposable Members
        public void Dispose()
        {
            this.innerExp = null;
        }
        #endregion
    }

}
