using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fanmaquar.Common
{
    /// <summary>
    /// 扩展函数
    /// </summary>
    public static class ExtensionMethod
    {
        /// <summary>
        /// 获取堆栈中最底层触发的异常
        /// </summary>
        /// <param name="exp">当前异常</param>
        /// <returns></returns>
        public static Exception GetTriggerException(this Exception exp)
        {
            while (exp.InnerException != null)
            {
                exp = exp.InnerException;
            }
            return exp;
        }

        /// <summary>
        /// 从JSON数据获取对象实例
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="jsonData">JSON序列化字符</param>
        /// <returns></returns>
        public static T LoadFromJson<T>(this string jsonData)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            return s.Deserialize<T>(jsonData);
        }

        /// <summary>
        /// 将对象转换成JSON字符串
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this object obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

        /// <summary>
        /// 将对象转换成JSON字符串
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <param name="recursionDepth">对象内嵌级别深度</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this object obj, int recursionDepth)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            s.RecursionLimit = recursionDepth;
            return s.Serialize(obj);
        }

        /// <summary>
        /// 转换到特定数值类型
        /// </summary>
        public static TData To<TData>(this string rawString)
        {
            Converter<string, TData> gConvert = new Converter<string, TData>(s =>
            {
                if (rawString == null)
                {
                    return default(TData);
                }
                else
                {
                    return (TData)Convert.ChangeType(s, typeof(TData));
                }
            });
            return gConvert(rawString);
        }

        /// <summary>
        /// 无异常转换到特定数值类型
        /// </summary>
        public static TData As<TData>(this string rawString)
        {
            TData result = default(TData);
            try
            {
                result = To<TData>(rawString);
            }
            catch { }
            return result;
        }

        /// <summary>
        /// 获取配置文件AppSettings节点中的键值
        /// </summary>
        /// <typeparam name="TResult">需要值类型</typeparam>
        /// <param name="appsetKey">键值名称</param>
        /// <param name="default">没有则采用的默认值</param>
        /// <returns></returns>
        public static TResult AppSettings<TResult>(this string appsetKey, TResult @default)
        {
            string configVal = System.Configuration.ConfigurationManager.AppSettings[appsetKey];
            if (string.IsNullOrEmpty(configVal))
            {
                return @default;
            }
            else
            {
                return As<TResult>(configVal);
            }
        }

        /// <summary>
        /// 从十进制转换为全字母形式的二十六进制(z=0,y=25)
        /// </summary>
        public static string NumToAz(this int num)
        {
            if (num < 0) throw new ArgumentOutOfRangeException("num");
            string azString = "ZABCDEFGHIJKLMNOPQRSTUVWXY";
            string ret = "";
            do
            {
                ret = azString[num % 26] + ret;
                num /= 26;
            } while (num != 0);
            return ret;
        }

        /// <summary>
        /// 检查实例类型是否实现某接口
        /// </summary>
        /// <param name="instanceType">实例类型</param>
        /// <param name="interfaceType">接口类型，对于泛型则为泛型的重整名称类型。</param>
        /// <returns>是否实现了指定类型的接口</returns>
        public static bool HasImplementInterface(this Type instanceType, Type interfaceType)
        {
            return instanceType.GetInterface(interfaceType.FullName) != null;
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        public static string GetRemoteIP(this HttpContext context)
        {
            string result = String.Empty;
            result = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (null == result || result == String.Empty)
            {
                result = context.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (null == result || result == String.Empty)
            {
                result = context.Request.UserHostAddress;
            }
            if (null == result || result == String.Empty ||
                !Regex.IsMatch(result, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$"))
            {
                return "0.0.0.0";
            }

            string append = "";
            #region 本机登录的主机名称
            if (context.Request.IsLocal && context.Request.LogonUserIdentity != null)
            {
                append = string.Format("({0})", context.Request.LogonUserIdentity.Name);
            }
            #endregion

            return result + append;
        }

        /// <summary>
        /// 获取对象序列化的XmlDocument版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <param name="noNamespaceAttr">属性是否添加默认命名空间</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static XmlDocument GetXmlDoc(this object pObj, bool noNamespaceAttr)
        {
            if (pObj == null) { return null; }
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(pObj.GetType(), string.Empty);
                XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.UTF8);
                if (noNamespaceAttr)
                {
                    XmlSerializerNamespaces xn = new XmlSerializerNamespaces();
                    xn.Add("", "");
                    xs.Serialize(xtw, pObj, xn);
                }
                else
                {
                    xs.Serialize(xtw, pObj);
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(Encoding.UTF8.GetString(ms.ToArray()).Trim());
                return xml;
            }
        }

        /// <summary>
        /// 获取对象序列化的XmlDocument版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static XmlDocument GetXmlDoc(this object pObj)
        {
            return GetXmlDoc(pObj, false);
        }

        /// <summary>
        /// 获取对象序列化的Xml字符串版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <param name="noNamespaceAttr">属性是否添加默认命名空间</param>
        /// <param name="xmlEncoding">xml字符串的编码</param>
        /// <returns></returns>
        public static string GetXmlString(this object pObj, bool noNamespaceAttr, Encoding xmlEncoding)
        {
            if (pObj == null) { return null; }
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(pObj.GetType(), string.Empty);
                XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.UTF8);
                if (noNamespaceAttr)
                {
                    XmlSerializerNamespaces xn = new XmlSerializerNamespaces();
                    xn.Add("", "");
                    xs.Serialize(xtw, pObj, xn);
                }
                else
                {
                    xs.Serialize(xtw, pObj);
                }
                return xmlEncoding.GetString(ms.ToArray()).Trim();
            }
        }

        /// <summary>
        /// 通过xml原始字符加载为文档对象
        /// </summary>
        /// <param name="xmlRaw">xml原始字符</param>
        /// <returns></returns>
        public static XmlDocument AsXmlDocument(this string xmlRaw)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlRaw);
            return xml;
        }

        /// <summary>
        /// 从配置静态方法创建符合特定委托类型的委托
        /// </summary>
        /// <typeparam name="TDelegate">委托类型</typeparam>
        /// <param name="staticCfgMethod">特定类型的静态方法描述，形如ClrServiceHost.Management.Communication::GetApplicationList, ClrServiceHost。</param>
        /// <returns></returns>
        public static TDelegate CreateFromConfig<TDelegate>(this string staticCfgMethod)
            where TDelegate : class
        {
            string pattern = "::([^,]+)";
            Match m = Regex.Match(staticCfgMethod, pattern, RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                throw new System.Configuration.ConfigurationErrorsException("服务端获取通信配置的委托方法(" + staticCfgMethod + ")配置错误！");
            }
            else
            {
                Type methodType = Type.GetType(staticCfgMethod.Replace(m.Value, string.Empty));
                MethodInfo method = methodType.GetMethod(m.Groups[1].Value, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                return Delegate.CreateDelegate(typeof(TDelegate), method, true) as TDelegate;
            }
        }

        /// <summary>
        /// 转换为委托字符串形式
        /// </summary>
        public static string ToDelegateString(this MemberInfo targetMethod)
        {
            Type targetType = targetMethod.ReflectedType;
            return string.Format("{0}::{1}, {2}", targetType, targetMethod.Name,
                Path.GetFileNameWithoutExtension(targetType.Assembly.Location) //targetType.Assembly.FullName.TrimAfter(",")
                );
        }

        /// <summary>
        /// 输出带缩进格式的XML文档
        /// </summary>
        /// <param name="xDoc">XML文档对象</param>
        /// <param name="writer">文本输出器</param>
        public static void WriteIndentedContent(this XmlDocument xDoc, TextWriter writer)
        {
            XmlTextWriter xWriter = new XmlTextWriter(writer);
            xWriter.Formatting = Formatting.Indented;
            xDoc.WriteContentTo(xWriter);
        }

        /// <summary>
        /// 从已序列化数据(XmlDocument)中获取对象实体
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="xmlDoc">已序列化的文档对象</param>
        /// <returns>对象实体</returns>
        public static T GetObject<T>(this XmlDocument xmlDoc)
        {
            if (xmlDoc == null) { return default(T); }
            XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc.DocumentElement);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xmlReader);
        }

        /// <summary>
        /// 获取对象序列化的二进制版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static byte[] GetBytes(this object pObj)
        {
            if (pObj == null) { return null; }
            MemoryStream serializationStream = new MemoryStream();
            new BinaryFormatter().Serialize(serializationStream, pObj);
            serializationStream.Position = 0L;
            byte[] buffer = new byte[serializationStream.Length];
            serializationStream.Read(buffer, 0, buffer.Length);
            serializationStream.Close();
            return buffer;
        }

        /// <summary>
        /// 从已序列化数据中(byte[])获取对象实体
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="binData">二进制数据</param>
        /// <returns>对象实体</returns>
        public static T GetObject<T>(this byte[] binData)
        {
            if (binData == null) { return default(T); }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream(binData);
            return (T)formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// 转义为xml字符
        /// </summary>
        /// <param name="objSource">待转义对象</param>
        /// <returns>如果对象为空则为字符null，否则为相关的字符串表示。</returns>
        public static string Escape2Xml(this object objSource)
        {
            if (objSource == null)
            {
                return "null";
            }
            else
            {
                if (objSource.GetType() != typeof(string))
                {
                    return objSource.ToString();
                }
                else
                {
                    return objSource.ToString().Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("&", "&amp;");
                }
            }
        }

        /// <summary>
        /// 判定某成员信息是否包含特定属性的配置
        /// </summary>
        /// <param name="mInfo">成员属性实例</param>
        /// <param name="AttrType">自定义属性类型</param>
        /// <param name="inherit">是否从集成对象中查找</param>
        /// <returns>是否存在特定属性的配置</returns>
        public static bool HasAttribute(this MemberInfo mInfo, Type AttrType, bool inherit)
        {
            object[] attrs = mInfo.GetCustomAttributes(AttrType, inherit);
            return (attrs != null && attrs.Length > 0);
        }

        /// <summary>
        /// 判定某实例类型是否有指定属性的配置
        /// </summary>
        /// <param name="instanceType">实例类型</param>
        /// <param name="AttrType">自定义属性类型</param>
        /// <param name="inherit">是否从集成对象中查找</param>
        /// <returns>是否存在特定属性的配置</returns>
        public static bool HasAttribute(this Type instanceType, Type AttrType, bool inherit)
        {
            object[] attrs = instanceType.GetCustomAttributes(AttrType, inherit);
            return (attrs != null && attrs.Length > 0);
        }

        /// <summary>
        /// 获取成员信息的组件描述字符，没有则放回为null。
        /// </summary>
        /// <param name="mInfo">类型成员</param>
        public static string GetDescription(this MemberInfo mInfo)
        {
            object[] objAttr = mInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objAttr != null && objAttr.Length > 0)
                return ((DescriptionAttribute)objAttr[0]).Description;
            return null;
        }

        /// <summary>
        /// 获取枚举项的字符描述，没有则放回枚举字段名称。
        /// </summary>
        public static string GetDescription(this Enum enumItem)
        {
            Type type = enumItem.GetType();
            MemberInfo[] memberInfos = type.GetMember(enumItem.ToString());
            if (memberInfos != null && memberInfos.Length > 0)
            {
                return GetDescription(memberInfos[0]) ?? enumItem.ToString();
            }
            return enumItem.ToString();
        }

        /// <summary>
        /// 获取不包含版本的类型全称，形如：BizService.Interface.Services.LogService, BizService.Interface。
        /// </summary>
        /// <param name="instanceType">对象类型</param>
        /// <returns></returns>
        public static string GetNoVersionTypeName(this Type instanceType)
        {
            if (!instanceType.IsGenericType)
            {
                return string.Format("{0}, {1}",
                instanceType.FullName,
                Path.GetFileNameWithoutExtension(instanceType.Assembly.Location));
            }
            else
            {
                string rawFullName = instanceType.FullName;
                string baseTypeName = rawFullName.Substring(0, rawFullName.IndexOf('`'));
                return string.Format("{0}<{1}>, {2}",
                    baseTypeName,
                    string.Join(",", Array.ConvertAll<Type, string>(instanceType.GetGenericArguments(),
                    t => t.ToSimpleType())),
                Path.GetFileNameWithoutExtension(instanceType.Assembly.Location));
            }

        }

        /// <summary>
        /// 二进制序列的16进制视图形式（16字节换行）
        /// </summary>
        public static string ByteArrayToHexString(this byte[] tBinBytes)
        {
            string draftStr = System.Text.RegularExpressions.Regex.Replace(BitConverter.ToString(tBinBytes),
            "([A-z0-9]{2}\\-){16}",
            m =>
            {
                return m.Value.Replace("-", " ") + Environment.NewLine;
            });
            return draftStr.Replace("-", " ");
        }

        /// <summary>
        /// 从原始16进制字符还原到字节序列
        /// </summary>
        public static byte[] HexPatternStringToByteArray(this string hexrawStr)
        {
            if (string.IsNullOrEmpty(hexrawStr))
            {
                return new byte[0];
            }

            string trueRaw = hexrawStr.Replace(" ", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("-", "")
                .Replace("\t", "").Trim();

            int totalLen = trueRaw.Length;
            if (totalLen % 2 != 0)
            {
                throw new InvalidCastException("hex string size invalid.");
            }
            else
            {
                byte[] rawBin = new byte[totalLen / 2];
                for (int i = 0; i < totalLen; i = i + 2)
                {
                    rawBin[i / 2] = Convert.ToByte(int.Parse(trueRaw.Substring(i, 2),
                        System.Globalization.NumberStyles.AllowHexSpecifier));
                }
                return rawBin;
            }
        }

        /// <summary>
        /// 获取某成员信息的特定属性的配置
        /// </summary>
        /// <typeparam name="T">自定义属性类型</typeparam>
        /// <param name="mInfo">成员属性实例</param>
        /// <param name="inherit">是否从集成对象中查找</param>
        /// <returns>如果不存在则放回空数组</returns>
        public static T[] GetMemberInfoCustomAttributes<T>(this MemberInfo mInfo, bool inherit)
        {
            object[] attrs = mInfo.GetCustomAttributes(typeof(T), inherit);
            if (attrs != null && attrs.Length > 0)
            {
                return Array.ConvertAll<object, T>(attrs, new Converter<object, T>(delegate(object o)
                {
                    return (T)o;
                }));
            }
            else
            {
                return new T[0];
            }
        }

        #region XMLNode扩展

        /// <summary>
        /// 安全转换节点属性为特定类型的值
        /// </summary>
        /// <typeparam name="TValue">返回属性数据类型</typeparam>
        /// <param name="node">当前节点</param>
        /// <param name="attrName">节点属性</param>
        /// <param name="default">如不存在，则设置的默认值。</param>
        /// <returns></returns>
        public static TValue Attr<TValue>(this XmlNode node, string attrName, TValue @default)
        {
            XmlAttribute nodeAttr = node.Attributes[attrName];
            if (nodeAttr == null)
            {
                return @default;
            }
            else
            {
                return nodeAttr.Value.As<TValue>();
            }
        }

        /// <summary>
        /// 安全转换节点属性为特定类型的值
        /// </summary>
        public static TValue Attr<TValue>(this XmlNode node, string attrName)
        {
            return Attr<TValue>(node, attrName, default(TValue));
        }

        /// <summary>
        /// 在满足不为默认值的情况下添加节点属性
        /// </summary>
        /// <param name="node">当前需要设置的节点</param>
        /// <param name="attrName">属性名称</param>
        /// <param name="attrVal">属性值，当值为null值移除该节点属性</param>
        /// <param name="ignoreFun">判断方法</param>
        /// <returns></returns>
        public static XmlNode IgnoreDefaultValue(this XmlNode node, string attrName, object attrVal, Func<bool> ignoreFun)
        {
            if (!ignoreFun())
            {
                return WithAttr(node, attrName, attrVal);
            }
            else
            {
                return node;
            }
        }

        /// <summary>
        /// 附加或设置节点属性
        /// </summary>
        /// <param name="node">当前需要设置的节点</param>
        /// <param name="attrName">属性名称</param>
        /// <param name="attrVal">属性值，当值为null值移除该节点属性</param>
        /// <returns>应用了该属性的节点</returns>
        public static XmlNode WithAttr(this XmlNode node, string attrName, object attrVal)
        {
            XmlAttribute attr = node.Attributes[attrName];
            if (attrVal != null && attr == null)
            {
                attr = node.OwnerDocument.CreateAttribute(attrName);
                node.Attributes.Append(attr);
            }

            if (attrVal == null)
            {
                if (attr != null)
                    node.Attributes.Remove(attr);
            }
            else
            {
                attr.Value = attrVal.ToString();
            }

            return node;
        }

        /// <summary>
        /// 删除节点属性
        /// </summary>
        /// <param name="node">当前需要设置的节点</param>
        /// <param name="attrName">属性名称</param>
        /// <returns>删除了该属性的节点</returns>
        public static XmlNode RemoveAttr(this XmlNode node, string attrName)
        {
            XmlAttribute attr = node.Attributes[attrName];
            if (attr != null) node.Attributes.Remove(attr);
            return node;
        }

        /// <summary>
        /// 创建当前节点的子节点
        /// </summary>
        public static XmlNode CrateChildElement(this XmlNode parentNode, string nodeName)
        {
            XmlNode xNode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, nodeName, null);
            parentNode.AppendChild(xNode);
            return xNode;
        }

        /// <summary>
        /// 基于接口/基类实现的子类型写入
        /// </summary>
        /// <typeparam name="TBase">接口/基类类型</typeparam>
        /// <param name="children">子类型实例列表</param>
        /// <param name="writer">序列化写入器</param>
        public static void WriteXmlEx<TBase>(this List<TBase> children, XmlWriter writer)
        {
            XmlSerializerNamespaces xn = new XmlSerializerNamespaces();
            xn.Add("", "");
            string rawChildStr = null;

            foreach (TBase item in children)
            {
                Type t = item.GetType();
                StringBuilder cb = new StringBuilder();
                using (StringWriter sw = new System.IO.StringWriter(cb))
                {
                    XmlSerializer xs = new XmlSerializer(t);
                    xs.Serialize(sw, item, xn);
                    sw.Close();
                }
                rawChildStr = cb.ToString();

                XmlDocument rawDoc = new XmlDocument();
                rawDoc.LoadXml(rawChildStr);

                XmlAttribute attr = rawDoc.CreateAttribute("type");
                attr.Value = t.GetNoVersionTypeName();

                rawDoc.DocumentElement.Attributes.Append(attr);
                writer.WriteRaw(rawDoc.DocumentElement.OuterXml);
            }
        }

        /// <summary>
        /// 写入对象内容到序列化xml写入器
        /// </summary>
        /// <param name="objInstance">当前对象值</param>
        /// <param name="writer">序列化写入器</param>
        public static void WriteXmlEx(this object objInstance, XmlWriter writer)
        {
            if (objInstance == null) return;
            Type objectType = objInstance.GetType();
            TypeCode typeCode = Type.GetTypeCode(objectType);
            if (typeCode != TypeCode.Object)
            {
                writer.WriteRaw(objInstance.Escape2Xml());
            }
            else
            {
                string rawChildStr = null;
                XmlSerializerNamespaces xn = new XmlSerializerNamespaces();
                xn.Add("", "");

                StringBuilder cb = new StringBuilder();
                using (StringWriter sw = new System.IO.StringWriter(cb))
                {
                    XmlSerializer xs = new XmlSerializer(objectType);
                    xs.Serialize(sw, objInstance, xn);
                    sw.Close();
                }
                rawChildStr = cb.ToString();

                XmlDocument rawDoc = new XmlDocument();
                rawDoc.LoadXml(rawChildStr);

                XmlAttribute attr = rawDoc.CreateAttribute("type");
                attr.Value = objectType.GetNoVersionTypeName();

                rawDoc.DocumentElement.Attributes.Append(attr);
                writer.WriteRaw(rawDoc.DocumentElement.OuterXml);
            }
        }

        /// <summary>
        /// 读到下一个指定类型节点
        /// </summary>
        /// <param name="reader">序列化读取器</param>
        /// <param name="nodeType">节点类型</param>
        public static void ReadToNodeType(this XmlReader reader, XmlNodeType nodeType)
        {
            int entDepth = reader.Depth;
            while (reader.Read()
                && reader.Depth >= entDepth)
            {
                if (reader.NodeType == nodeType)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 读取到特定节点名称,忽略其他节点
        /// </summary>
        /// <param name="reader">序列化读取器</param>
        /// <param name="elementName">节点名称</param>
        public static void ReadToElement(this XmlReader reader, string elementName)
        {
            int entDepth = reader.Depth;
            while (reader.Read()
                && reader.Depth >= entDepth)
            {
                if (reader.NodeType == XmlNodeType.Element
                    && reader.Name.Equals(elementName))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 基于接口/基类实现的子类型读取
        /// </summary>
        /// <typeparam name="TBase">接口/基类类型</typeparam>
        /// <param name="children">子类型实例列表</param>
        /// <param name="reader">序列化读取器</param>
        /// <returns></returns>
        public static void ReadXmlEx<TBase>(this List<TBase> children, XmlReader reader)
            where TBase : class
        {
            string cTypeVal = null;
            Type cType = null;
            TBase subChildInstance = default(TBase);
            string interfaceTypeString = typeof(TBase).FullName;

            #region 处理开始节点[TODO]
            if (reader.NodeType == XmlNodeType.Element)
            {
                cTypeVal = reader.GetAttribute("type");
                if (string.IsNullOrEmpty(cTypeVal))
                {
                    if (!KnownTypeConfig.Instance.IsKnownTaskByElementName(reader.Name, ref cType))
                    {
                        cType = typeof(TBase);
                        if (cType.IsInterface || cType.IsAbstract)
                        {
                            throw new System.Configuration.ConfigurationErrorsException(string.Format("抽象类型或接口{1}不能序列化，节点名称{0}没有执行类型属性(type)!",
                                reader.Name, cType.FullName));
                        }
                    }
                }
                else
                {
                    cType = TypeCache.GetRuntimeType(cTypeVal);
                    if (typeof(TBase).IsInterface)
                    {
                        if (cType.GetInterface(interfaceTypeString, false) == null)
                        {
                            throw new System.Configuration.ConfigurationErrorsException(string.Format("类型{0}不是接口{1}的实现类!", cType.FullName, interfaceTypeString));
                        }
                    }
                    else
                    {
                        if (!cType.IsSubclassOf(typeof(TBase)))
                        {
                            throw new System.Configuration.ConfigurationErrorsException(string.Format("类型{0}不是{1}的继承类!", cType.FullName, interfaceTypeString));
                        }
                    }
                }

                XmlSerializer xChildTask = new XmlSerializer(cType, new XmlRootAttribute(reader.Name));
                subChildInstance = xChildTask.Deserialize(reader) as TBase;
                if (subChildInstance != null)
                {
                    children.Add(subChildInstance);
                }
            }
            #endregion

        }

        /// <summary>
        /// 写入实例的xml序列化
        /// </summary>
        /// <param name="instance">当前实例</param>
        /// <param name="writer">写入器</param>
        /// <param name="rootName">根节点名称</param>
        public static void ObjectWriteXml(this object instance, XmlWriter writer, string rootName)
        {
            XmlSerializerNamespaces xn = new XmlSerializerNamespaces();
            xn.Add("", "");
            XmlSerializer xsc = null;
            if (string.IsNullOrEmpty(rootName))
            {
                xsc = new XmlSerializer(instance.GetType());
            }
            else
            {
                xsc = new XmlSerializer(instance.GetType(), new XmlRootAttribute(rootName));
            }
            xsc.Serialize(writer, instance, xn);
        }

        /// <summary>
        /// 写入实例的xml序列化
        /// </summary>
        /// <param name="instance">当前实例</param>
        /// <param name="writer">写入器</param>
        public static void ObjectWriteXml(this object instance, XmlWriter writer)
        {
            ObjectWriteXml(instance, writer, null);
        }

        /// <summary>
        /// 读取为特定实例
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        public static T ObjectReadXml<T>(this XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(reader.Name));
            return (T)serializer.Deserialize(reader);
        }

        /// <summary>
        /// 读取为特定实例
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="targetType">要返回的数据类型</param>
        /// <returns></returns>
        public static object ObjectReadXml(this XmlReader reader, Type targetType)
        {
            XmlSerializer serializer = new XmlSerializer(targetType, new XmlRootAttribute(reader.Name));
            return serializer.Deserialize(reader);
        }

        #endregion

        #region 时间转换
        /// <summary>
        /// 获取当前时间与协调时间(utc)1970年1月1日午夜的间隔
        /// </summary>
        /// <param name="dateTime">当前时间</param>
        /// <returns>去掉毫秒数的时间间隔值</returns>
        public static TimeSpan ToUtcTimeSpan(this DateTime dateTime)
        {
            TimeSpan tspan = dateTime - new DateTime(1970, 1, 1, 0, 0, 0);
            return new TimeSpan(tspan.Days, tspan.Hours, tspan.Minutes, tspan.Seconds);
        }

        /// <summary>
        /// 清除毫秒数的值
        /// </summary>
        /// <param name="tspan">需要清除的值</param>
        /// <returns></returns>
        public static TimeSpan ClearMilliseconds(this TimeSpan tspan)
        {
            return new TimeSpan(tspan.Days, tspan.Hours, tspan.Minutes, tspan.Seconds);
        }

        /// <summary>
        /// 从utc时间间隔返回实际时间
        /// </summary>
        /// <param name="utcTimeSpan"></param>
        /// <returns></returns>
        public static DateTime ToUtcDateTime(this TimeSpan utcTimeSpan)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).Add(utcTimeSpan);
        }

        /// <summary>
        /// 转换为友好的字符串形式，精确到秒。
        /// </summary>
        /// <param name="utcTmeSpan"></param>
        /// <returns></returns>
        public static string ToUtcDateTimeOrTimeSpan(this TimeSpan utcTmeSpan)
        {
            if (utcTmeSpan.TotalDays > 1)
                return ToUtcDateTime(utcTmeSpan).ToString("yyyy-MM-dd HH:mm:ss");
            else
                return utcTmeSpan.ClearMilliseconds().ToString();
        }

        /// <summary>
        /// 当前时间与协调世界时(utc)1970年1月1日午夜之间的时间差（以毫秒为单位测量）
        /// </summary>
        /// <param name="dateTimeUtc"></param>
        public static string GetTimeMilliseconds(this DateTime dateTimeUtc)
        {
            return (dateTimeUtc - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds.ToString("0");
        }

        /// <summary>
        /// 获取时间差微秒数原始时间值
        /// </summary>
        /// <param name="timeMillisecondsString"></param>
        /// <returns></returns>
        public static DateTime GetDatetimeFromTimeMilliseconds(this string timeMillisecondsString)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(Convert.ToDouble(timeMillisecondsString));
            return new DateTime(1970, 1, 1, 0, 0, 0).Add(timeSpan);
        }

        /// <summary>
        /// Dates the time to dos time.
        /// </summary>
        /// <param name="_dt">The _DT.</param>
        /// <returns></returns>
        public static uint DateTimeToDosTime(this DateTime _dt)
        {
            return (uint)(
                (_dt.Second / 2) | (_dt.Minute << 5) | (_dt.Hour << 11) |
                (_dt.Day << 16) | (_dt.Month << 21) | ((_dt.Year - 1980) << 25));
        }

        /// <summary>
        /// Doses the time to date time.
        /// </summary>
        /// <param name="_dt">The _DT.</param>
        /// <returns></returns>
        public static DateTime DosTimeToDateTime(this uint _dt)
        {
            return new DateTime(
                (int)(_dt >> 25) + 1980,
                (int)(_dt >> 21) & 15,
                (int)(_dt >> 16) & 31,

                (int)(_dt >> 11) & 31,
                (int)(_dt >> 5) & 63,
                (int)(_dt & 31) * 2);
        }
        #endregion

    }
}
