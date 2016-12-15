using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Fanmaquar.SmtpMail
{
    /// <summary>
    /// 邮件模块封装
    /// </summary>
    [Serializable]
    public class MailTemplet
    {
        /// <summary>
        /// 新建一个邮件模板对象
        /// </summary>
        /// <param name="templetFilePath">HTML模块文件路径</param>
        /// <param name="fileEnc">模板内容编码方式</param>
        public MailTemplet(string templetFilePath, Encoding fileEnc)
        {
            if (!File.Exists(templetFilePath))
                throw new ArgumentException("模块文件在指定路径不存在！");

            tptContent = fileEnc.GetString(File.ReadAllBytes(templetFilePath));
        }

        /// <summary>
        /// 新建一个指定模块内容的邮件模板
        /// </summary>
        /// <param name="tptHtml">模块内容</param>
        public MailTemplet(string tptHtml)
        {
            tptContent = tptHtml;
        }

        /// <summary>
        /// 原始模板内容
        /// </summary>
        string tptContent = null;
        /// <summary>
        /// 变量存储词典
        /// </summary>
        Dictionary<string, object> VarDictStore = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);


        /// <summary>
        /// 设置模块变量的值
        /// </summary>
        /// <param name="verName">变量名称</param>
        /// <param name="varValue">变量绑定的值</param>
        /// <returns>Flunent方式返回自身</returns>
        public MailTemplet SetVariable(string verName, object varValue)
        {
            if (VarDictStore.ContainsKey(verName))
            {
                VarDictStore[verName] = varValue;
            }
            else
            {
                VarDictStore.Add(verName, varValue);
            }
            return this;
        }

        Func<string, string> tptKeyHandler = null;

        /// <summary>
        /// 注册变量丢失时自定义处理的委托
        /// </summary>
        /// <param name="keyHandler"></param>
        /// <returns>Flunent方式返回自身</returns>
        public MailTemplet RegisterKeyHandler(Func<string, string> keyHandler)
        {
            tptKeyHandler = keyHandler;
            return this;
        }

        /// <summary>
        /// 变量匹配模式
        /// </summary>
        string keyPattern = "\\{([a-zA-z_.'\"\\[$][a-zA-z_0-9.'\"\\]$]*)\\}";

        /// <summary>
        /// 匹配变量处理
        /// </summary>
        Func<string, string> keyTrim = s => s.Trim('{', '}');

        /// <summary>
        /// 获取HTML邮件内容
        /// </summary>
        /// <returns></returns>
        public string ToHtmlContent()
        {
            return Regex.Replace(tptContent, keyPattern, getTempletMatchValue, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 获取绑定实例的属性或字段值
        /// </summary>
        /// <param name="instance">属性实例</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns></returns>
        object getPropertyOrField(object instance, string fieldName)
        {
            if (fieldName.IndexOf('[') != -1 && fieldName.IndexOf(']') != -1)
            {
                int pIdx = fieldName.IndexOf('[');
                string realKey = fieldName.Substring(0, pIdx);
                object subObj = getPropertyOrField(instance, realKey);
                int pIdxEnd = fieldName.IndexOf(']', pIdx + 1);
                if (pIdxEnd > pIdx)
                {
                    string idxName = fieldName.Substring(pIdx, pIdxEnd - pIdx + 1);
                    object idxValue = getIndexProperty(idxName, subObj);
                    if (fieldName.Length > pIdxEnd + 2 && fieldName.Substring(pIdxEnd + 1, 1) == ".")
                        return getPropertyOrField(idxValue, fieldName.Substring(pIdxEnd + 2));
                    return idxValue;
                }
            }

            object objVal = null;

        FetchSub:
            Type instanceType = instance.GetType();
            int idx = fieldName.IndexOf('.');
            if (idx == -1)
            {
                PropertyInfo pi = instanceType.GetProperty(fieldName);
                if (pi != null)
                    objVal = pi.GetValue(instance, null);

                if (objVal == null)
                {
                    FieldInfo fi = instanceType.GetField(fieldName);
                    if (fi != null)
                        objVal = fi.GetValue(instance);
                }
            }
            else
            {
                string subField = fieldName.Substring(0, idx);
                instance = getPropertyOrField(instance, subField);
                fieldName = fieldName.Substring(idx + 1);
                goto FetchSub;
            }

            return objVal;
        }

        /// <summary>
        /// 获取绑定实例的索引命名属性
        /// </summary>
        /// <param name="idxName">索引名称或序号</param>
        /// <param name="instance">绑定实例</param>
        /// <returns></returns>
        object getIndexProperty(string idxName, object instance)
        {
            idxName = idxName.Trim('[', ']', '"', '\'');
            Type instanceType = instance.GetType();
            object objVal = null;
            bool isNum = Regex.IsMatch(idxName, "^\\d+$");
            MethodInfo mInfo = instanceType.GetMethod("Get", new Type[] { isNum ? typeof(int) : typeof(string) });
            if (mInfo != null)
            {
                if (isNum)
                {
                    objVal = mInfo.Invoke(instance, new object[] { Convert.ToInt32(idxName) });
                }
                else
                {
                    objVal = mInfo.Invoke(instance, new object[] { idxName });
                }
            }
            return objVal;
        }


        /// <summary>
        /// 处理变量匹配函数
        /// </summary>
        /// <param name="tptMatch">变量匹配</param>
        /// <returns></returns>
        string getTempletMatchValue(Match tptMatch)
        {
            object objVal = null;

            string key = keyTrim(tptMatch.Value);
            int subIdx = key.IndexOf('.');
            if (subIdx == -1)
            {
                #region 直接属性或命名属性
                if (key.IndexOf('[') != -1 && key.IndexOf(']') != -1)
                {
                    int pIdx = key.IndexOf('[');
                    string realKey = key.Substring(0, pIdx);
                    if (VarDictStore.ContainsKey(realKey))
                    {
                        int pIdxEnd = key.IndexOf(']', pIdx + 1);
                        if (pIdxEnd > pIdx)
                        {
                            string idxName = key.Substring(pIdx, pIdxEnd - pIdx + 1);
                            objVal = getIndexProperty(idxName, VarDictStore[realKey]);
                            if (key.Length > pIdxEnd + 2 && key.Substring(pIdxEnd + 1, 1) == ".")
                                objVal = getPropertyOrField(objVal, key.Substring(pIdxEnd + 2));
                        }
                    }
                }
                else
                {
                    if (VarDictStore.ContainsKey(key))
                        objVal = VarDictStore[key];
                }
                #endregion
            }
            else
            {
                #region 带从属性的语法
                string subKey = key.Substring(0, subIdx);
                int sPidx = subKey.IndexOf('[');
                int sPidxEnd = -1;
                if (sPidx != -1 && subKey.IndexOf(']') != -1)
                {
                    sPidxEnd = subKey.IndexOf(']', sPidx + 1);
                    subKey = subKey.Substring(0, sPidx);
                }

                object topInstance = null;
                if (VarDictStore.ContainsKey(subKey))
                    topInstance = VarDictStore[subKey];

                if (topInstance != null && sPidxEnd > 0 && sPidxEnd > sPidx)
                {
                    string idxName = key.Substring(sPidx, sPidxEnd - sPidx + 1);
                    topInstance = getIndexProperty(idxName, topInstance);
                }

                if (topInstance != null)
                {
                    string propName = key.Substring(subIdx + 1);
                    objVal = getPropertyOrField(topInstance, propName);
                }
                #endregion
            }

            if (objVal != null)
            {
                return objVal.ToString();
            }
            else
            {
                if (tptKeyHandler != null)
                    return tptKeyHandler(key) ?? "";
                return tptMatch.Value;
            }
        }
    }
}
