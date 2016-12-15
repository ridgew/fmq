using System;
using System.Xml.Serialization;

namespace Fanmaquar.Configuration
{
    /// <summary>
    /// 实例映射
    /// </summary>
    [Serializable]
    public class EntryMapping
    {
        /// <summary>
        /// 类型键名
        /// </summary>
        [XmlAttribute("key")]
        public string Key { get; set; }

        /// <summary>
        /// 是否是正则匹配模式键
        /// </summary>
        [XmlAttribute("pattern")]
        public bool Pattern { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        [XmlAttribute("type")]
        public string TypeFullName { get; set; }

    }

    /// <summary>
    /// 实例映射配置集合
    /// </summary>
    [XmlRoot("EntryMappingConfig")]
    public class EntryMappingConfig
    {
        private static EntryMappingConfig _instance;
        /// <summary>
        /// 获取配置文件中的实例
        /// </summary>
        public static EntryMappingConfig ConfigInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = XmlSerializeSectionHandler.GetObject<EntryMappingConfig>("EntryMappingConfig");
                }
                return _instance;
            }
        }

        /// <summary>
        /// 是否限制于映射使用
        /// </summary>
        [XmlAttribute]
        public bool MapOnly { get; set; }

        /// <summary>
        /// 类型匹配集合
        /// </summary>
        [XmlElement("add")]
        public EntryMapping[] MappingCollection = new EntryMapping[0];

    }
}
