using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Fanmaquar.Configuration
{
    /// <summary>
    /// 接口暴露属性配置
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class APIExportAttribute : Attribute
    {
        /// <summary>
        /// 接口导出类型
        /// </summary>
        public ExportType Category { get; set; }

        /// <summary>
        /// 获取或设置标识名称
        /// </summary>
        public string IdentityName { get; set; }

        /// <summary>
        /// 获取或设置导出描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 对所有内置程序集的导出标记应用
        /// </summary>
        /// <param name="category">导出类型</param>
        /// <param name="exportCfg">该导出配置实例</param>
        public static void ExportApply(ExportType category, Action<Type, APIExportAttribute> exportCfg)
        {
            foreach (Assembly loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                #region 跳过全局缓存程序集和微软公司的程序集
                if (loadedAssembly.GlobalAssemblyCache || loadedAssembly.ReflectionOnly) continue;

                object[] asmCom = loadedAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
                if (asmCom != null && asmCom.Length > 0)
                {
                    AssemblyCompanyAttribute asmCompany = (AssemblyCompanyAttribute)asmCom[0];
                    if (asmCompany.Company.Equals("Microsoft Corporation", StringComparison.InvariantCultureIgnoreCase)) continue;
                }
                #endregion

                asmCom = loadedAssembly.GetCustomAttributes(typeof(APIExportAttribute), true);
                if (asmCom == null || asmCom.Length < 1) continue;

                try
                {
                    #region 循环查找注册类型及方法
                    foreach (Type innerType in loadedAssembly.GetTypes())
                    {
                        if (innerType.IsAbstract || innerType.IsInterface || innerType.IsGenericType || innerType.IsNotPublic)
                        {
                            //|| !innerType.IsSubclassOf(typeof(NetTask.Core.XmlDefineTask))
                            //System.Diagnostics.Debugger.Log(0, "Info", "Skipped:" + innerType.FullName + Environment.NewLine);
                            continue;
                        }

                        object[] pAttr = innerType.GetCustomAttributes(typeof(APIExportAttribute), false);
                        if (pAttr == null || pAttr.Length < 1)
                        {
                            continue;
                        }
                        else
                        {
                            APIExportAttribute cfg = (APIExportAttribute)pAttr[0];
                            if (cfg.Category == category)
                            {
                                exportCfg(innerType, cfg);
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception typeLoadEx)
                {
                    if (typeLoadEx.InnerException != null) typeLoadEx = typeLoadEx.InnerException;
                    System.Diagnostics.Trace.TraceError("* 查找Assembly[{0}]时获取全部类型出现异常:{1}", loadedAssembly.FullName, typeLoadEx.Message);
                }
            }
        }
    }

    /// <summary>
    /// 导出类型
    /// </summary>
    public enum ExportType
    {
        /// <summary>
        /// 就标记所在类型
        /// </summary>
        AsItIs = 0,

        /// <summary>
        /// 导出为抽象类或接口扩展实现
        /// </summary>
        ImplementAPI,

        /// <summary>
        /// 导出为接口使用函数
        /// </summary>
        APIMethod,

        /// <summary>
        /// API接口规范
        /// </summary>
        InterfaceAPI,

        /// <summary>
        /// API接口数据规范
        /// </summary>
        APIDataContract
    }
}
