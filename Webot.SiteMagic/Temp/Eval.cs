/*

动态编译类，可以计算表达式，也可以调用系统中的类

调用方式：     return new XXXXXXX.Eval().GetValue("System.DateTime.Now")

返回结果：     2005-08-04 15:00:24


*/
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.IO;

namespace XXXXXXX
{
    /*****************************************************************
    ** 文件名:       Eval.cs
    ** Copyright (c) 1999 -2003 
    ** 创建人:       Phoenix
    ** 创建日期: 
    ** 修改人:   BigmouthZ@163.net 
    ** 修改日期: 2005/08/04
    ** 描 述:         获取字符串所表示的逻辑意义
    ** 版 本:1.0
    ******************************************************************/
    public class Eval
    {
        public object GetValue(string value)
        {
            string codeSnippet = @"using System; 
            using System.Collections;
            namespace CzG 
            {
             public class $0$
             {
              public $0$(){}
              public object GetValue()
              {
               return $1$ ;
              }
             }
            }";

            string tmp = "Eval" + Convert.ToString(System.DateTime.Now.Ticks);
            codeSnippet = codeSnippet.Replace("$0$", tmp);
            codeSnippet = codeSnippet.Replace("$1$", value);
            object b = "";
            try
            {
                ICodeCompiler compiler = new CSharpCodeProvider().CreateCompiler();
                CompilerParameters para = new CompilerParameters();
                para.ReferencedAssemblies.Add("System.dll");
                string path = System.Environment.CurrentDirectory;
                para.GenerateInMemory = true;
                para.GenerateExecutable = false;
                para.OutputAssembly = path + "\\" + tmp + ".dll";

                CompilerResults cr = compiler.CompileAssemblyFromSource(para, codeSnippet);
                Assembly asm = cr.CompiledAssembly;

                object obj = asm.CreateInstance("CzG." + tmp);
                Type type = asm.GetType("CzG." + tmp);
                MethodInfo mi = type.GetMethod("GetValue");

                b = mi.Invoke(obj, null);
                GC.Collect();
                System.IO.File.Delete(path + "\\" + tmp + ".dll");
            }
            catch
            {
                b = "CallError!";
            }

            return b;
        }
    }
}


