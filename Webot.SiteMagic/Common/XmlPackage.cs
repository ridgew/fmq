using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;

namespace Webot.Common
{
    [Serializable]
    public class XmlPackage : IVersion
    {
        /// <summary>
        /// Xml格式资源包(无初始化)
        /// </summary>
        public XmlPackage() { }

        /// <summary>
        /// 从序列化的Xml文档中还原对象
        /// </summary>
        public XmlPackage(XmlDocument xmlDoc)
        {
            XmlNodeReader reader = new XmlNodeReader(xmlDoc.DocumentElement);
            XmlSerializer ser = new XmlSerializer(typeof(XmlPackage));
            XmlPackage pkg = (XmlPackage)ser.Deserialize(reader);
            this.SetPackageDictionary(pkg.GetPackageDictionary());
        }

        /// <summary>
        /// 从序列化的数据中还原对象
        /// </summary>
        public XmlPackage(byte[] binData)
        {
            XmlPackage pkg = (XmlPackage)GetDeserializeObject(binData);
            this.SetPackageDictionary(pkg.GetPackageDictionary());
        }

        private IDictionary<string, XmlPackageItem> _PackageDictionary;

        /// <summary>
        /// 内容包集合获取
        /// </summary>
        public IDictionary<string, XmlPackageItem> GetPackageDictionary()
        {
            return this._PackageDictionary;
        }

        /// <summary>
        /// 设置内容包集合
        /// </summary>
        public void SetPackageDictionary(IDictionary<string, XmlPackageItem> pkgDic)
        {
            _PackageDictionary = pkgDic;
        }
        
        /// <summary>
        /// 二进制序列化对象
        /// </summary>
        /// <param name="pObj">要序列化的对象</param>
        /// <returns>二进制序列化后的数据流</returns>
        public static byte[] GetSerializeObject(object pObj)
        {
            if (pObj == null) return null;
            System.IO.MemoryStream _memory = new System.IO.MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(_memory, pObj);
                _memory.Position = 0;

            byte[] read = new byte[_memory.Length];
                _memory.Read(read, 0, read.Length);
                _memory.Close();

            return read;
        }

        /// <summary>
        /// 二进制反序列化对象
        /// </summary>
        /// <param name="binData">二进制数据</param>
        /// <returns>实例对象</returns>
        public static object GetDeserializeObject(byte[] binData)
        {
            if (binData == null) return null;
            BinaryFormatter formmater = new BinaryFormatter();
            System.IO.MemoryStream _memory = new System.IO.MemoryStream(binData);
            return formmater.Deserialize(_memory);
        }

        /// <summary>
        /// 获取包的原数据
        /// </summary>
        public byte[] GetBinData()
        {
            return GetSerializeObject(this);
        }

        /// <summary>
        /// 生成Xml文档格式
        /// </summary>
        /// <returns></returns>
        public XmlDocument ToXmlDoc()
        {
            //XmlSerializer xSerializer = new XmlSerializer(typeof(XmlPackage));
            //StringWriter sWriter = new StringWriter(CultureInfo.InvariantCulture);
            //XmlTextWriter xTextWriter = new XmlTextWriter(sWriter);
            //XmlDocument xmlDoc = new XmlDocument();
            //xSerializer.Serialize(xTextWriter, this);
            //xmlDoc.LoadXml(sWriter.ToString());
            //sWriter.Close();
            //return xmlDoc;

            XmlSerializer ser = new XmlSerializer(typeof(XmlPackage));
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            ser.Serialize(writer, this);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sb.ToString());
            writer.Close();
            return doc;
        }


        #region IVersion Members
        /// <summary>
        /// 当前对象版本
        /// </summary>
        public Version CurrentVersion
        {
            get { return new Version(0, 0, 0, 1); }
        }

        #endregion
    }
}
