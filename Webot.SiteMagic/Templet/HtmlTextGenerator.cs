using System;
using Webot.Common;
using System.Text.RegularExpressions;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 通过模板设置产生相关频道的静态文本文件生成器
    /// </summary>
    public class HtmlTextGenerator
    {
        public HtmlTextGenerator()
        {

        }

        /// <summary>
        /// 处理HTML文件中的资源地址为绝对路径形式
        /// </summary>
        /// <param name="baseUrl">改HTML资源的基础地址</param>
        /// <param name="htmlContent">原始HTML文本内容</param>
        public static string GetRightHtmlText(string baseUrl, string htmlContent)
        {
            // 
            // -------------------
            //(src|href|url|background)\s*=\s*(?<jsescape>\\?)(('|")?)([^\s>]+)\2

            htmlContent = Util.GetCustomMatchReplace(htmlContent,
                new CustomMatchReplace("(src|href|url|background)\\s*=\\s*(?<jsescape>\\\\?)(('|\")?)([^\\s>]+)\\2",
                    (RegexOptions.IgnoreCase | RegexOptions.Compiled),
                    baseUrl),
                new CustomMatchReplace.ReplaceMatchedHandler(delegate(Match m, CustomMatchReplace cmp)
                {
                    return m.Groups[1].Value + "=" + m.Groups["jsescape"].Value + m.Groups[2].Value
                    + Util.GetAbsoluteUrl(m.Groups[4].Value, cmp.DataStorage[0].ToString()) + m.Groups[3].Value;
                }));

            // background:url(images/top_bei3.gif) 
            // background:#D9662F url(../images/jian2a.gif)
            // -----------------
            // (background:)?(url\()(?<jsescape>\\?)(('|")?)([^\s>]+)\3\)
            return Util.GetCustomMatchReplace(htmlContent,
                new CustomMatchReplace("(background:)?(url\\()(?<jsescape>\\\\?)(('|\")?)([^\\s>]+)\\3\\)",
                    (RegexOptions.IgnoreCase | RegexOptions.Compiled),
                    baseUrl),
                new CustomMatchReplace.ReplaceMatchedHandler(delegate(Match m, CustomMatchReplace cmp)
                {
                    return m.Groups[1].Value + m.Groups[2].Value + m.Groups["jsescape"].Value + m.Groups[3].Value
                    + Util.GetAbsoluteUrl(m.Groups[5].Value, cmp.DataStorage[0].ToString()) + m.Groups[4].Value + ")";
                })
            );
        }


    }
}
