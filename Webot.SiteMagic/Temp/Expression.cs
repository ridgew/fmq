using System;
using System.Collections;

namespace XXXXX
{
    /// <summary>
    /// 表达式解析的全部源码 C#版
    /// http://blog.csdn.net/verybigmouthz/archive/2005/07/29/438414.aspx
    /// </summary>
    public class Expression
    {
        private Expression() 
        { 
            
        }

        #region No01.表达式分割为ArrayList形式
        /// <summary>
        /// 要求表达式以空格\t作为分隔符
        /// 转换表达式折分为：
        /// 变量及数值 ,变量不允许为@
        /// 字符串“”
        /// 运算符号{+、-、*、/、++、+=、--、-=、*=、/=、!、!=、>、>=、>>、<、<=、<>、|、|=、||、&、&=、&&}
        /// 括号{包括（、）}
        /// </summary>
        /// <param name="sExpression"></param>
        /// <returns></returns>
        public static ArrayList ConvertExpression(string sExpression)
        {
            ArrayList alist = new ArrayList();

            string word = null;
            int i = 0;
            string c = "";

            while (i < sExpression.Length)
            {
                #region "
                if (word != null && word != "")
                    if (word.Substring(0, 1) == "\"")
                    {
                        do
                        {
                            c = sExpression[i++].ToString();
                            if (c == "\"") { alist.Add(word + c); word = c = null; break; }
                            else { word += c; c = null; }
                        } while (i < sExpression.Length);
                    }
                if (i > sExpression.Length - 1)
                { alist.Add(word); alist.Add(c); word = c = null; break; }
                #endregion

                #region 字符判别
                switch (c = sExpression[i++].ToString())
                {
                    #region ( )
                    case "\"":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; break; }
                        else
                        {
                            word = c; c = null;
                            do
                            {
                                c = sExpression[i++].ToString();
                                if (c == "\"") { alist.Add(word + c); word = c = null; break; }
                                else { word += c; c = null; }
                            } while (i < sExpression.Length);
                            break;
                        }

                    case "(": alist.Add(word); alist.Add(c); word = c = null; break;
                    case ")": alist.Add(word); alist.Add(c); word = c = null; break;
                    case " ": alist.Add(word); word = c = null; break;
                    #endregion

                    #region + - * / %
                    case "+":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "+": alist.Add(word); alist.Add("++"); word = c = null; break;
                                case "=": alist.Add(word); alist.Add("+="); word = c = null; break;
                                default: alist.Add(word); alist.Add("+"); word = c = null; i--; break;
                            }
                        break;
                    case "-":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "-": alist.Add(word); alist.Add("--"); word = c = null; break;
                                case "=": alist.Add(word); alist.Add("-="); word = c = null; break;
                                default: alist.Add(word); alist.Add("-"); word = c = null; i--; break;
                            }
                        break;
                    case "*":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "=": alist.Add(word); alist.Add("*="); word = c = null; break;
                                default: alist.Add(word); alist.Add("*"); word = c = null; i--; break;
                            }
                        break;
                    case "/":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "=": alist.Add(word); alist.Add("/="); word = c = null; break;
                                default: alist.Add(word); alist.Add("/"); word = c = null; i--; break;
                            }
                        break;
                    case "%":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "=": alist.Add(word); alist.Add("%="); word = c = null; break;
                                default: alist.Add(word); alist.Add("%"); word = c = null; i--; break;
                            }
                        break;
                    #endregion

                    #region > < =
                    case ">":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case ">": alist.Add(word); alist.Add(">>"); word = c = null; break;
                                case "=": alist.Add(word); alist.Add(">="); word = c = null; break;
                                default: alist.Add(word); alist.Add(">"); word = c = null; i--; break;
                            }
                        break;
                    case "<":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "<": alist.Add(word); alist.Add("<<"); word = c = null; break;
                                case ">": alist.Add(word); alist.Add("<>"); word = c = null; break;
                                case "=": alist.Add(word); alist.Add("<="); word = c = null; break;
                                default: alist.Add(word); alist.Add("<"); word = c = null; i--; break;
                            }
                        break;
                    case "=":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "=": alist.Add(word); alist.Add("=="); word = c = null; break;
                                default: alist.Add(word); alist.Add("="); word = c = null; i--; break;
                            }
                        break;
                    #endregion

                    #region ! | &
                    case "!":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "=": alist.Add(word); alist.Add("!="); word = c = null; break;
                                default: alist.Add(word); alist.Add("!"); word = c = null; i--; break;
                            }
                        break;
                    case "|":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "=": alist.Add(word); alist.Add("|="); word = c = null; break;
                                case "|": alist.Add(word); alist.Add("||"); word = c = null; break;
                                default: alist.Add(word); alist.Add("|"); word = c = null; i--; break;
                            }
                        break;
                    case "&":
                        if (i > sExpression.Length - 1)
                        { alist.Add(word); alist.Add(c); word = c = null; }
                        else
                            switch (c = sExpression[i++].ToString())
                            {
                                case "=": alist.Add(word); alist.Add("&="); word = c = null; break;
                                case "&": alist.Add(word); alist.Add("&&"); word = c = null; break;
                                default: alist.Add(word); alist.Add("&"); word = c = null; i--; break;
                            }
                        break;
                    #endregion
                    default:
                        word += c;
                        break;
                }
                if (i == sExpression.Length) alist.Add(word);
                #endregion
            }

            ArrayList alresult = new ArrayList();
            foreach (object a in alist)
            {
                if (a == null) continue;
                if (a.ToString().Trim() == "") continue;
                alresult.Add(a);
            }
            return alresult;
        }

        /// <summary>
        /// 对返回的表达式，已经分好放于ArrayList中的变量进行替换为实际常量
        /// </summary>
        /// <param name="alExpression"></param>
        /// <param name="mapVar"></param>
        /// <param name="mapValue"></param>
        /// <returns></returns>
        public static ArrayList ConvertExpression(ArrayList alExpression, string mapVar, string mapValue)
        {
            for (int i = 0; i < alExpression.Count; i++)
            {
                if (alExpression[i].ToString() == mapVar) { alExpression[i] = mapValue; break; }
            }
            return alExpression;
        }
        /// <summary>
        /// 对返回的表达式，已经分好放于ArrayList中的变量进行替换为实际常量
        /// </summary>
        /// <param name="alExpression"></param>
        /// <param name="name"></param>
        /// <param name="mapvalue"></param>
        /// <returns></returns>
        public static ArrayList ConvertExpression(ArrayList alExpression, string[] mapVar, string[] mapValue)
        {
            for (int i = 0; i < alExpression.Count; i++)
            {
                for (int j = 0; j < mapVar.Length; j++)
                {
                    if (alExpression[i].ToString() == mapVar[j])
                    {
                        alExpression[i] = mapValue[j];
                        break;
                    }
                    //     System.Console.WriteLine("Expression: {0}  >>>  {1}",mapVar[j], mapValue[j]);
                }
            }
            return alExpression;
        }

        #endregion

        #region No02.后缀表达式方式 解析表达式
        /// <summary>
        /// 找出第一个闭括号
        /// </summary>
        /// <param name="alExpression"></param>
        /// <returns></returns>
        public static int Find_First_RightBracket(ArrayList alExpression)
        {
            for (int i = 0; i < alExpression.Count; i++)
            { if (OperatorMap.CheckRightBracket(alExpression[i].ToString())) return i; }
            return 0;
        }
        /// <summary>
        /// 找出匹配的开括号
        /// </summary>
        /// <param name="alExpression"></param>
        /// <param name="iRightBracket"></param>
        /// <returns></returns>
        public static int Find_Near_LeftBracket(ArrayList alExpression, int iRightBracket)
        {
            int i = iRightBracket - 2;
            while (i >= 0)
            {
                if (OperatorMap.CheckLeftBracket(alExpression[i].ToString())) return i;
                i--;
            }
            return 0;
        }
        /// <summary>
        /// 中缀表达式转换为后缀表达式
        /// </summary>
        /// <param name="alexpression"></param>
        /// <returns></returns>
        public static ArrayList ConvertToPostfix(ArrayList alexpression)
        {
            ArrayList alOutput = new ArrayList();
            Stack sOperator = new Stack();
            string word = null;
            int count = alexpression.Count;
            int i = 0;
            while (i < count)
            {
                word = alexpression[i++].ToString();

                //·读到左括号时总是将它压入栈中
                if (OperatorMap.CheckLeftBracket(word))
                { sOperator.Push(word); }
                else

                    //·读到右括号时，将*近栈顶的第一个左括号上面的运算符全部依次弹出，送至输出队列后，再丢弃左括号。
                    if (OperatorMap.CheckRightBracket(word))
                    {
                        while (true)
                        {
                            if (sOperator.Count == 0) break;
                            string sTop = sOperator.Peek().ToString();
                            if (sTop == "(") { sOperator.Pop(); break; }
                            else alOutput.Add(sOperator.Pop());
                        }
                    }
                    else

                        //·当读到数字直接送至输出队列中
                        if (OperatorMap.IsVar(word))
                        { alOutput.Add(word); }
                        else

                            //·当读到运算符t时，
                            //　　　　　a.将栈中所有优先级高于或等于t的运算符弹出，送到输出队列中； 
                            //　　　　　b.t进栈
                            if (OperatorMap.CheckOperator(word))
                            {
                                while (sOperator.Count > 0)
                                {
                                    string sPop = sOperator.Peek().ToString();

                                    if (sPop == "(") break;

                                    if (OperatorMap.GetMaxprior(word, sPop) >= 0)
                                    {
                                        //       sPop = sOperator.Pop().ToString();
                                        alOutput.Add(sOperator.Pop().ToString());
                                    }
                                    else
                                        break;
                                    //      System.Console.WriteLine("XH{0}",sPop);

                                }
                                sOperator.Push(word);
                            }

                //    System.Console.WriteLine("{0}",word.ToString());
            }

            //中缀表达式全部读完后，若栈中仍有运算符，将其送到输出队列中
            while (sOperator.Count > 0)
            {
                string s = sOperator.Pop().ToString();
                alOutput.Add(s);
                //    System.Console.WriteLine("{0}:{1}",sOperator.Count,s.ToString());
            }

            return alOutput;
        }


        /// <summary>
        /// 计算后缀表达式
        /// </summary>
        /// <param name="alexpression"></param>
        /// <returns></returns>
        public static object ComputePostfix(ArrayList alexpression)
        {
            try
            {
                //·建立一个栈S
                Stack s = new Stack();
                int count = alexpression.Count;
                int i = 0;
                while (i < count)
                {
                    //·从左到右读后缀表达式，读到数字就将它转换为数值压入栈S中，
                    string word = alexpression[i++].ToString();
                    if (OperatorMap.IsVar(word))
                    {
                        s.Push(word);
                        //      System.Console.WriteLine("Push:{0}",word);
                    }
                    else//读到运算符则从栈中依次弹出两个数分别到Y和X，
                        if (OperatorMap.CheckOperator(word))
                        {
                            string y, x, sResult;
                            if (!CheckOneOperator(word))
                            {
                                y = s.Pop().ToString();
                                x = s.Pop().ToString();
                                //然后以“X 运算符 Y”的形式计算机出结果，再压加栈S中 
                                sResult = ComputeTwo(x, y, word).ToString();
                                s.Push(sResult);
                            }
                            else
                            {
                                x = s.Pop().ToString();
                                sResult = ComputeOne(x, word).ToString();
                                s.Push(sResult);
                            }
                        }
                }
                string spop = s.Pop().ToString();
                //    System.Console.WriteLine("Result:{0}",spop);
                return spop;
            }
            catch
            {
                System.Console.WriteLine("Result:表达式不符合运算规则!Sorry!");
                return "Sorry!Error!";
            }

        }

        public static object ComputeExpression(string sExpression)
        {
            return Expression.ComputePostfix(Expression.ConvertToPostfix(Expression.ConvertExpression(sExpression)));
        }

        public static object ComputeExpression(string sExpression, string mapVar, string mapValue)
        {
            return Expression.ComputePostfix(Expression.ConvertToPostfix(Expression.ConvertExpression(Expression.ConvertExpression(sExpression), mapVar, mapValue)));
        }

        public static object ComputeExpression(string sExpression, string[] mapVar, string[] mapValue)
        {

            return Expression.ComputePostfix(Expression.ConvertToPostfix(Expression.ConvertExpression(Expression.ConvertExpression(sExpression), mapVar, mapValue)));
        }

        #endregion

        #region No03. 简单无括号表达式的计算

        #region 检查字符可以转换的类型
        public static bool CheckNumber(string str)
        {
            try { Convert.ToDouble(str); return true; }
            catch { return false; }
        }

        public static bool CheckBoolean(string str)
        {
            try { Convert.ToBoolean(str); return true; }
            catch { return false; }
        }

        public static bool CheckString(string str)
        {
            try
            {
                str = str.Replace("\"", "");
                char c = (char)(str[0]);
                if ((c >= 'a') && (c <= 'z') || (c >= 'A') && (c <= 'Z'))
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }
        public static bool CheckOneOperator(string sOperator)
        {
            if (sOperator == "++" || sOperator == "--" || sOperator == "!")
                return true;
            else
                return false;
        }

        #endregion

        #region 双目运算
        public static object ComputeTwoNumber(double dL, double dR, string sO)
        {
            switch (sO)
            {
                case "+": return (dL + dR);
                case "-": return (dL - dR);
                case "*": return (dL * dR);
                case "%": return (dL % dR);
                case "/": try { return (dL / dR); }
                    catch
                    {
                        return false;
                        //return "ComputeTwoNumber ["+sO+"] Sorry!";
                    }

                case "+=": return (dL += dR);
                case "-=": return (dL -= dR);
                case "*=": return (dL *= dR);
                case "/=": try { return (dL /= dR); }
                    catch
                    {
                        return false;
                        //return "ComputeTwoNumber ["+sO+"] Sorry!";
                    }

                case "=": return (dL == dR);
                case "==": return (dL == dR);
                case "!=": return (dL != dR);
                case "<>": return (dL != dR);
                case ">": return (dL.CompareTo(dR) > 0);
                case ">=": return (dL.CompareTo(dR) >= 0);
                case "<": return (dL.CompareTo(dR) < 0);
                case "<=": return (dL.CompareTo(dR) <= 0);

                case ">>": return (int)dL >> (int)dR;
                case "<<": return (int)dL << (int)dR;
                case "|": return (int)dL | (int)dR;
                case "&": return (int)dL & (int)dR;
                case "|=":
                    {
                        int iL = (int)dL;
                        int iR = (int)dR;
                        return iL |= iR;
                    }
                case "&=":
                    {
                        int iL = (int)dL;
                        int iR = (int)dR;
                        return iL &= iR;
                    }
                default:
                    return false;
                //return "ComputeTwoNumber ["+sO+"] Sorry!";
            }
        }

        public static object ComputeTwoBoolean(bool bL, bool bR, string sO)
        {
            switch (sO)
            {
                case ">": return bL.CompareTo(bR) > 0;
                case ">=": return bL.CompareTo(bR) >= 0;
                case "<": return bL.CompareTo(bR) < 0;
                case "<=": return bL.CompareTo(bR) <= 0;
                case "=": return bL == bR;
                case "==": return bL == bR;
                case "!=": return bL != bR;
                case "<>": return bL != bR;

                case "||": return bL || bR;
                case "&&": return bL && bR;
                default: return false;
                //return "ComputeTwoBoolean ["+sO+"] Sorry!";
            }
        }

        public static object ComputeTwoString(string sL, string sR, string sO)
        {
            switch (sO)
            {
                case "+": return sL + sR;
                case "=": return (sL == sR);
                case "==": return (sL == sR);
                case "!=": return (sL != sR);
                case "<>": return (sL != sR);
                case ">": return (sL.CompareTo(sR) > 0);
                case ">=": return (sL.CompareTo(sR) >= 0);
                case "<": return (sL.CompareTo(sR) < 0);
                case "<=": return (sL.CompareTo(sR) <= 0);
                default: return false;
                //return "ComputeTwoString ["+sO+"] Sorry!";
            }
        }

        public static object ComputeTwo(string sL, string sR, string sO)
        {
            if (CheckNumber(sL))
            {
                if (CheckNumber(sR))
                    return ComputeTwoNumber(Convert.ToDouble(sL), Convert.ToDouble(sR), sO);
                else
                    if (CheckString(sR)) return ComputeTwoString(sL, sR, sO);
            }
            else if (CheckBoolean(sL))
            {
                if (CheckBoolean(sR))
                    return ComputeTwoBoolean(Convert.ToBoolean(sL), Convert.ToBoolean(sR), sO);
                else
                    if (CheckString(sR)) return ComputeTwoString(sL, sR, sO);
            }
            else if (CheckString(sL)) return ComputeTwoString(sL, sR, sO);

            return "ComputeTwo [" + sL + "][" + sO + "][" + sR + "] Sorry!";
        }

        #endregion

        #region 单目运算
        public static object ComputeOneNumber(double dou, string sO)
        {
            switch (sO)
            {
                case "++": return (dou + 1);
                case "--": return (dou - 1);
                default: return false;
                //return "ComputeOneNumber ["+sO+"] Sorry!";
            }
        }

        public static object ComputeOneString(string str, string sO)
        {
            switch (sO)
            {
                case "++": return (str + str);
                default: return false;
                //return "ComputeOneString ["+sO+"] Sorry!";
            }
        }

        public static object ComputeOneBoolean(bool bo, string sO)
        {
            switch (sO)
            {
                case "!": return (!bo);
                default: return false;
                //   return "ComputeOneBoolean ["+sO+"] Sorry!";
            }
        }

        public static object ComputeOne(string str, string sO)
        {
            if (CheckNumber(str))
                return ComputeOneNumber(Convert.ToDouble(str), sO);
            if (CheckBoolean(str))
                return ComputeOneBoolean(Convert.ToBoolean(str), sO);
            if (CheckString(str))
                return ComputeOneString(str, sO);
            return "ComputerOne [" + str + "][" + sO + "] Sorry!";
        }


        #endregion

        #endregion

        #region No04. 实用工具类
        /// <summary>
        /// ArrayList子集操作
        /// </summary>
        public class ArrayListCopy
        {
            private ArrayListCopy() { }
            /// <summary>
            /// 返回ArrayList子集{L--R}内容
            /// </summary>
            /// <param name="alist"></param>
            /// <param name="iLeft"></param>
            /// <param name="iRight"></param>
            /// <returns></returns>
            public static ArrayList CopyBewteenTo(ArrayList alist, int iLeft, int iRight)
            {
                ArrayList alResult = new ArrayList();
                bool b = false;
                for (int i = iLeft; i < iRight; i++)
                {
                    alResult.Add(alist[i]);
                    b = true;
                }
                if (b) return alResult;
                else return null;
            }

            /// <summary>
            /// 返回ArrayList子集{L--R}的补集内容
            /// </summary>
            /// <param name="alist"></param>
            /// <param name="iLeft"></param>
            /// <param name="iRight"></param>
            /// <returns></returns>
            public static ArrayList CopyNotBetweenTo(ArrayList alist, int iLeft, int iRight)
            {
                ArrayList alResult = new ArrayList();
                bool b = false;
                for (int i = 0; i < iLeft - 1; i++)
                {
                    alResult.Add(alist[i]);
                    b = true;
                }

                if (b)
                {
                    alResult.Add("@");

                    for (int i = iRight + 1; i < alist.Count; i++)
                    {
                        alResult.Add(alist[i]);
                        b = true;
                    }
                }
                if (b) return alResult;
                else return null;
            }

            /// <summary>
            /// 统计字符串sin在str中出现的次数
            /// </summary>
            /// <param name="str"></param>
            /// <param name="sin"></param>
            /// <returns></returns>
            public static int GetSubStringCount(string str, string sin)
            {
                int i = 0;
                int ibit = 0;
                while (true)
                {
                    ibit = str.IndexOf(sin, ibit);
                    if (ibit > 0)
                    {
                        ibit += sin.Length;
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                return i;
            }
        }


        /// <summary>
        /// 算符的优先级实体
        /// </summary>
        public class OperatorMap
        {
            public struct Map
            {
                public int Priority;
                public string Operator;
                public Map(int iPrior, string sOperator)
                {
                    Priority = iPrior;
                    Operator = sOperator;
                }
            }
            private OperatorMap() { }
            public static Map[] map()
            {
                Map[] om;
                om = new Map[30];

                om[0] = new Map(5, "*");
                om[1] = new Map(5, "/");
                om[29] = new Map(5, "%");

                om[2] = new Map(10, "+");
                om[3] = new Map(10, "-");

                om[4] = new Map(20, ">");
                om[5] = new Map(20, ">=");
                om[6] = new Map(20, "<");
                om[7] = new Map(20, "<=");
                om[8] = new Map(20, "<>");
                om[9] = new Map(20, "!=");
                om[10] = new Map(20, "==");
                om[11] = new Map(20, "=");

                om[12] = new Map(41, "!");
                om[13] = new Map(42, "||");
                om[14] = new Map(43, "&&");

                om[15] = new Map(40, "++");
                om[16] = new Map(40, "--");
                om[17] = new Map(40, "+=");
                om[18] = new Map(40, "-=");
                om[19] = new Map(40, "*=");
                om[20] = new Map(40, "/=");
                om[21] = new Map(40, "&");
                om[22] = new Map(40, "|");
                om[23] = new Map(40, "&=");
                om[24] = new Map(40, "|=");
                om[25] = new Map(40, ">>");
                om[26] = new Map(40, "<<");

                om[27] = new Map(3, "(");
                om[28] = new Map(3, ")");
                return om;
            }
            public static bool CheckLeftBracket(string str) { return (str == "("); }
            public static bool CheckRightBracket(string str) { return (str == ")"); }

            public static bool CheckBracket(string str) { return (str == "(" || str == ")"); }
            public static bool CheckOperator(string scheck)
            {
                string[] Operator = {"+", "-", "*", "/", "%",
                      ">", ">=", "<", "<=", "<>", "!=", "==", "=",
                      "!", "||", "&&",
                      "++", "--", "+=", "-=", "*=", "/=", 
                      "&", "|", "&=", "|=", 
                      ">>", "<<",
                      ")", "("
                     };

                bool bl = false;
                for (int i = 0; i < Operator.Length - 1; i++) { if (Operator[i] == scheck) { bl = true; break; } }
                return bl;
            }

            public static Map GetMap(string Operator)
            {
                if (CheckOperator(Operator)) foreach (Map tmp in map()) { if (tmp.Operator == Operator) return tmp; }
                return new Map(99, Operator);
            }

            public static int Getprior(string Operator) { return GetMap(Operator).Priority; }

            public static int GetMaxprior(string Loperator, string Roperator)
            { return GetMap(Loperator).Priority - GetMap(Roperator).Priority; }

            public static bool IsVar(string svar)
            {
                if ((svar[0] >= '0' && svar[0] <= '9') || (svar[0] >= 'a' && svar[0] <= 'z') || (svar[0] >= 'A' && svar[0] <= 'Z'))
                    return true;
                else
                    return false;
            }
        }
        #endregion
    }

}


