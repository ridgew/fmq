using System.Collections.Generic;
using System;
using Webot.Common;
using System.Text;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 多依赖资源容器
    /// </summary>
    public class MultiResDependency : IResourceDependency
    {
        /// <summary>
        /// 多依赖资源容器
        /// </summary>
        public MultiResDependency() { }

        /// <summary>
        /// 按参数顺序填充依赖资源容器
        /// </summary>
        /// <param name="Res">依赖资源集合</param>
        public MultiResDependency(params IResourceDependency[] Res)
        {
            if (Res.Length < 1) { throw new Exception("请至少指派一个依赖资源");  }
            foreach (IResourceDependency res in Res)
            {
                if (res != null && !ResTab.ContainsKey(res.DependencyIdentity))
                {
                    ResTab.Add(res.DependencyIdentity, res);
                }
            }
        }

        //private List<IResourceDependency> ResTab = new List<IResourceDependency>();
        private Dictionary<string, IResourceDependency> ResTab = new Dictionary<string, IResourceDependency>(StringComparer.InvariantCultureIgnoreCase);

        ///// <summary>
        ///// 获取依赖资源容器集合
        ///// </summary>
        //public List<IResourceDependency> GetResTab()
        //{
        //    return ResTab;
        //}

        /// <summary>
        /// 检测是否包含指定的资源依赖项
        /// </summary>
        /// <param name="res">资源依赖项</param>
        public bool ContainsResDependency(IResourceDependency res)
        {
            return ResTab.ContainsKey(res.DependencyIdentity);
        }

        /// <summary>
        /// Adds the res dependency.
        /// </summary>
        /// <param name="res">The res.</param>
        public void AddResDependency(IResourceDependency res)
        {
            if (!ContainsResDependency(res))
            {
                ResTab.Add(res.DependencyIdentity, res);
            }
        }

        #region IResourceDependency Members
        /// <summary>
        /// 获取某项标签项已定义的值
        /// </summary>
        public object GetDefinition(string x)
        {
            object objRet = null;
            bool gotValue = false;
            foreach (IResourceDependency res in ResTab.Values)
            {
                if (res.IsDefined(x))
                {
                    objRet = res.GetDefinition(x);
                    if (objRet != null && objRet.ToString() != "")
                    {
                        //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " " + x);
                        //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " " + objRet.ToString());
                        //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " " + res.ToString());
                        gotValue = true;
                        break;
                    }
                }
            }

            //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " 混合资源依赖： " + x);
                

            //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " 函数系统标签解析 ");
            if (!gotValue && x.StartsWith("{#$") && x.EndsWith(")$#}"))
            {
                using (TagParse tgp = new TagParse(x, RequestResDependency.Instance))
                {
                    objRet = tgp.GetValue();
                }
            }
            return objRet;
        }

        /// <summary>
        /// 返回特定对象是否有定义
        /// </summary>
        /// <param name="x">要查询的对象</param>
        /// <returns>是否定义过该对象</returns>
        public bool IsDefined(string x)
        {
            //OleDbHelper.AppendToFile("~/debug.log", System.Environment.NewLine + " " + x); 
            bool blnRet = false;
            foreach (IResourceDependency res in ResTab.Values)
            {
                if (res.IsDefined(x)) 
                {
                    blnRet = true;
                    break;
                }
            }
            return blnRet;
        }

        /// <summary>
        /// 依赖标识
        /// </summary>
        /// <value>依赖标识号</value>
        public string DependencyIdentity
        {
            get { return "DependencyContainer"; }
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder resB = new StringBuilder();
            foreach (IResourceDependency res in ResTab.Values)
            {
                resB.AppendFormat("{0}:{1}\r\n", res.DependencyIdentity, res.ToString()); 
            }
            return resB.ToString();
        }
    }
}
