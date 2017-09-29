using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace GenericSharpTool
{

    public class SqlReportFile : IDisposable
    {
        string _txtContent = null;

        public SqlReportFile(string rptFilePath)
        {
            if (!File.Exists(rptFilePath))
            {
                throw new FileNotFoundException("文件不存在！");
            }

            _txtContent = File.ReadAllText(rptFilePath);
        }

        public string TextContent()
        {
            return _txtContent;
        }

        List<FileColumn> _rawColumn = new List<FileColumn>();
        bool columnParsed = false;

        /// <summary>
        /// 报表文件列字段分隔长度（空格）
        /// </summary>
        const int ReportFile_Space_Length = 1;

        /// <summary>
        /// 标题头行数大小
        /// </summary>
        const int ReportFile_HeadLine_Length = 2;

        /// <summary>
        /// 获取报表文件的列名集合
        /// </summary>
        public List<FileColumn> Columns
        {
            get
            {
                if (columnParsed)
                    return _rawColumn;

                parseColumns();
                return _rawColumn;
            }
        }

        void parseColumns()
        {
            using (StringReader sr = new StringReader(_txtContent))
            {
                string lineStr = sr.ReadLine();
                string headerStr = lineStr;
                int idx = 0;
                lineStr = sr.ReadLine();
                string[] allColsLen = lineStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0, j = allColsLen.Length; i < j; i++)
                {
                    FileColumn col = new FileColumn();
                    col.StartIndex = idx;
                    col.ColumnLength = allColsLen[i].Length;
                    col.ColumnName = ((i < j - 1) ? headerStr.Substring(idx, col.ColumnLength) : headerStr.Substring(idx)).TrimEnd();
                    _rawColumn.Add(col);
                    idx += col.ColumnLength + ReportFile_Space_Length;
                }

                columnParsed = true;
            }
        }

        /// <summary>
        /// 提取所有符合条件的数据项
        /// </summary>
        public List<ReportRow> QueryMatchItems(Predicate<ReportRow> match = null)
        {
            List<ReportRow> items = new List<ReportRow>();
            using (var c = new ReaderCursor(_txtContent))
            {
                c.SkipLine(ReportFile_HeadLine_Length);
                int chrPeek = -1;
                while ((chrPeek = c.__r__.Peek()) != -1)
                {
                    if (chrPeek == (int)'\r' || chrPeek == (int)'\n')
                        break;

                    ReportRow item = new ReportRow();
                    for (int i = 0, j = Columns.Count; i < j; i++)
                    {
                        var col = Columns[i];
                        if (i < j - 1)
                        {
                            char[] varChars = new char[col.ColumnLength];
                            int totalRead = c.__r__.Read(varChars, 0, varChars.Length);
                            item[col.ColumnName] = (new string(varChars, 0, totalRead)).TrimEnd();
                            c.SkipColumn(ReportFile_Space_Length);
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            int readChar = -1;
                            while ((readChar = c.__r__.Read()) != -1)
                            {
                                char chr = (char)readChar;
                                if (chr == '\n')
                                {
                                    break;
                                }
                                else
                                {
                                    sb.Append(chr);
                                }
                            }
                            item[col.ColumnName] = sb.ToString().TrimEnd();
                            continue;
                        }
                    }
                    if (match == null || match(item))
                        items.Add(item);
                }
            }
            return items;
        }

        public void Dispose()
        {

        }

        internal class ReaderCursor : IDisposable
        {
            internal StringReader __r__ = null;
            public ReaderCursor(string content)
            {
                __r__ = new StringReader(content);
                Position = 0;
            }

            public int Position { get; private set; }

            /// <summary>
            /// 跳过多少行
            /// </summary>
            public ReaderCursor SkipLine(int lineNum)
            {
                int lineCount = 0;
                while (lineCount < lineNum)
                {
                    ReadLine();
                    lineCount++;
                }
                return this;
            }

            /// <summary>
            /// 跳过多少列
            /// </summary>
            public ReaderCursor SkipColumn(int colNum)
            {
                int colCount = 0;
                while (colCount < colNum)
                {
                    Position = __r__.Read();
                    colCount++;
                }
                return this;
            }

            /// <summary>
            /// 间隔多少空格
            /// </summary>
            public ReaderCursor Space(int spaceLen = 1)
            {
                SkipColumn(spaceLen);
                return this;
            }

            /// <summary>
            /// 读取多长的字符串
            /// </summary>
            public string ReadText(int textLen)
            {
                char[] ret = new char[textLen];
                Position = __r__.Read(ret, 0, textLen);
                return new string(ret);
            }

            /// <summary>
            /// 读取单行文本
            /// </summary>
            public string ReadLine()
            {
                string lineStr = __r__.ReadLine();
                Position += lineStr.Length;
                return lineStr;
            }

            public void Dispose()
            {
                if (__r__ != null)
                    __r__.Dispose();
            }
        }
    }

    /// <summary>
    /// 文件包含的列
    /// </summary>
    [Serializable]
    public class FileColumn
    {
        /// <summary>
        /// 列名称
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 列长度
        /// </summary>
        public int ColumnLength { get; set; }

        /// <summary>
        /// 列所在行起始位置
        /// </summary>
        public int StartIndex { get; set; }
    }

    [Serializable]
    public class ReportRow : NameValueCollection
    {
        public ReportRow()
            : base()
        {

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in Keys)
            {
                sb.AppendFormat("{0}={1}, ", key, this[key]);
            }
            return sb.ToString().TrimEnd(',', ' ');
        }

    }
}
