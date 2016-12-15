using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Fanmaquar.Common
{
    /// <summary>
    /// 配置服务的类型缓存
    /// </summary>
    public static class TypeCache
    {
        static readonly Dictionary<string, Type> FTD = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 直接构建新实例 <see cref="TypeCache"/> class.
        /// </summary>
        static TypeCache()
        {
            FTD.Add("string", typeof(string));
            FTD.Add("string[]", typeof(string[]));
            FTD.Add("bool", typeof(bool));

            FTD.Add("int", typeof(System.Int32));
            FTD.Add("int[]", typeof(System.Int32[]));
            FTD.Add("uint", typeof(UInt32));

            FTD.Add("ulong", typeof(UInt64));
            FTD.Add("long", typeof(System.Int64));
            FTD.Add("long[]", typeof(System.Int64[]));

            FTD.Add("ushort", typeof(UInt16));
            FTD.Add("short", typeof(System.Int16));
            FTD.Add("short[]", typeof(System.Int16[]));

            FTD.Add("char", typeof(char));
            FTD.Add("byte", typeof(byte));
            FTD.Add("sbyte", typeof(SByte));
            FTD.Add("byte[]", typeof(byte[]));

            FTD.Add("float", typeof(float));
            FTD.Add("decimal", typeof(decimal));
            FTD.Add("double", typeof(double));

            FTD.Add("datetime", typeof(System.DateTime));

            #region 常用复合类型
            FTD.Add("Dictionary<string,object>", typeof(Dictionary<string, object>));
            FTD.Add("Hashtable", typeof(Hashtable));
            FTD.Add("NameValueCollection", typeof(System.Collections.Specialized.NameValueCollection));
            #endregion
        }

        /// <summary>
        /// 获取运行时类型
        /// </summary>
        /// <param name="typeIdString">类型标识</param>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">System.Configuration.ConfigurationErrorsException</exception>
        /// <returns></returns>
        public static Type GetRuntimeType(this string typeIdString)
        {
            int gsIdx = typeIdString.IndexOf('<');
            int geIdx = typeIdString.LastIndexOf('>');
            if (gsIdx != -1 && geIdx > gsIdx)
            {
                //NetTask.Core.ScopeItemCompare<int>, NetTask.Interface
                //NetTask.Core.ScopeItemCompare`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
                #region 友好的泛型字符
                string gstr = typeIdString.Substring(gsIdx, geIdx - gsIdx + 1);
                string[] gtArr = gstr.Substring(1, gstr.Length - 2).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string gdStr = string.Concat(typeIdString.Substring(0, gsIdx),
                    "`" + gtArr.Length,
                    typeIdString.Substring(geIdx + 1));

                Type baseT = Type.GetType(gdStr, true);
                List<Type> garr = new List<Type>();
                foreach (string ga in gtArr)
                {
                    garr.Add(GetRuntimeType(ga));
                }
                return baseT.MakeGenericType(garr.ToArray());
                #endregion
            }
            else
            {
                if (FTD.ContainsKey(typeIdString)) return FTD[typeIdString];
                Type stepType = Type.GetType(typeIdString, false);
                if (stepType == null)
                {
                    throw new System.Configuration.ConfigurationErrorsException(string.Format("配置类型{0}未找到!", typeIdString));
                }
                return stepType;
            }
        }

        /// <summary>
        /// 转换为简易类型
        /// </summary>
        /// <param name="runtimeType">运行时类型</param>
        /// <returns></returns>
        public static string ToSimpleType(this Type runtimeType)
        {
            KeyValuePair<string, Type> existPair = FTD.FirstOrDefault<KeyValuePair<string, Type>>(t => t.Value.Equals(runtimeType));
            if (existPair.Key != null)
            {
                return existPair.Key;
            }
            else
            {
                return runtimeType.GetNoVersionTypeName();
            }
        }

        /// <summary>
        /// 转换为字符串形式的值
        /// </summary>
        /// <param name="objVal"></param>
        /// <returns></returns>
        public static string ToStringValue(this object objVal)
        {
            if (objVal == null)
            {
                return null;
            }
            else
            {
                Type valType = objVal.GetType();
                if (valType.Equals(typeof(byte[])))
                {
                    return ((byte[])objVal).ByteArrayToHexString();
                }
                else
                {
                    return objVal.ToString();
                }
            }
        }
    }

}
