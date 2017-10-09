using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericSharpTool
{
    class Program
    {
        static void Main(string[] args)
        {
            BankUtil.MainTest(args);

            if (args != null && args.Length > 0)
            {
                #region SqlReportFile Test
                using (SqlReportFile file = new SqlReportFile(args[0]))
                {
                    //string content = file.TextContent();
                    //using (SqlReportFile.ReaderCursor r = new SqlReportFile.ReaderCursor(content))
                    //{
                    //    string testToken = r.SkipLine(2)
                    //   .SkipColumn(36)
                    //   .Space(1)
                    //   .SkipColumn(50)
                    //   .Space(1)
                    //   .ReadText(32);

                    //    Console.WriteLine(string.Format("[{0}]", testToken));
                    //}

                    //var column = file.Columns;
                    //foreach(var col in file.Columns)
                    //{
                    //    Console.WriteLine(string.Format("{0} nvarchar({1})", col.ColumnName, col.ColumnLength));
                    //}

                    //var list = file.QueryMatchItems(m => m["Id"] == "FCDBBD4E-5EAA-4501-9BAF-13D201230018");
                    var list = file.QueryMatchItems();
                    if (list.Any())
                        Console.WriteLine(list.First().ToString());
                }
                #endregion

            }

            Console.WriteLine("处理完成！");
            Console.Read();

        }
    }
}
