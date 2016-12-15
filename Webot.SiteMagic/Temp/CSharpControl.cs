using System;
using System.IO;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Collections;
using System.Web.UI.WebControls;
using Microsoft.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using OWC;

namespace JSL.MVC.Page
{
    /// <summary>
    /// Controls 的摘要说明。
    /// </summary>
    public class Controls
    {
        private Controls()
        {
        }
        #region string
        public static string Encrypto(string Source)
        {
            System.Security.Cryptography.HashAlgorithm HashCryptoService;
            HashCryptoService = new System.Security.Cryptography.SHA1Managed();

            byte[] bytIn = System.Text.UTF8Encoding.UTF8.GetBytes(Source);
            byte[] bytOut = HashCryptoService.ComputeHash(bytIn);
            return Convert.ToBase64String(bytOut);
        }
        #endregion

        #region BaseDataList
        public void BindBaseList(System.Web.UI.WebControls.BaseDataList bdl_object, DataSet ds, string tbName)
        {
            bdl_object.DataSource = ds.Tables[tbName];
            bdl_object.DataBind();
        }

        public void BindBaseList(System.Web.UI.WebControls.BaseDataList bdl_object, DataTable dt)
        {
            bdl_object.DataSource = dt;
            bdl_object.DataBind();
        }
        #endregion

        #region DropDownList
        public static void CreateDropDownList(DropDownList ddl_object, DataTable dt, string s_text, string s_value)
        {
            ddl_object.DataSource = dt;
            ddl_object.DataTextField = s_text;
            ddl_object.DataValueField = s_value;
            ddl_object.DataBind();
        }

        public static void CreateDropDownList(DropDownList ddl_object, DataSet ds, string s_text, string s_value)
        {
            ddl_object.DataSource = ds;
            ddl_object.DataTextField = s_text;
            ddl_object.DataValueField = s_value;
            ddl_object.DataBind();
        }

        public static void CreateDropDownList(DropDownList ddl_object, string s_text, string s_value)
        {
            ListItem li = new ListItem(s_text, s_value);
            ddl_object.Items.Add(li);
        }

        public static void CreateDropDownList(DropDownList ddl_object, string[] s_text, string[] s_value)
        {
            for (int i = 0; i < s_text.Length; i++)
            {
                ListItem li = new ListItem(s_text[i], s_value[i]);
                ddl_object.Items.Add(li);
            }
        }

        public static void CopyDropDownList(DropDownList ddl_source, DropDownList ddl_target)
        {
            ddl_target.Items.Clear();
            ddl_target.DataTextField = ddl_source.DataTextField;
            ddl_target.DataTextFormatString = ddl_source.DataTextFormatString;
            ddl_target.DataValueField = ddl_source.DataValueField;
            for (int i = 0; i < ddl_source.Items.Count; i++)
            {
                ddl_target.Items.Add(ddl_source.Items[i]);
            }
        }

        public static void SetDropDownListValue(DropDownList ddl_object, string s_value)
        {
            foreach (ListItem li in ddl_object.Items)
            {
                if (li.Value == s_value)
                {
                    ddl_object.SelectedValue = s_value;
                    break;
                }
                else
                {
                    ddl_object.SelectedValue = null;
                }
            }
        }

        public static void SetDropDownListValue(DropDownList[] ddl_object, string[] s_value)
        {
            int i = 0;
            foreach (DropDownList ddl in ddl_object)
            {
                SetDropDownListValue(ddl, s_value[i++]);
            }
        }
        #endregion

        #region TextBox
        protected static void CopyTextBox(TextBox tb_source, TextBox tb_target)
        {
            tb_target.Text = tb_source.Text;
        }

        protected static void SetTextBoxValue(TextBox tb_object, string s_value)
        {
            tb_object.Text = s_value;
        }

        protected static void SetTextBoxValue(TextBox[] tb_object, string[] s_value)
        {
            int i = 0;
            foreach (TextBox dt in tb_object)
            {
                SetTextBoxValue(dt, s_value[i++]);
            }
        }
        #endregion

        #region TreeView
        /// <summary>
        /// 创建树形结构
        /// </summary>
        /// <param name="dt">要传入的数据表</param>
        /// <param name="treenodecollection">树结点容器集合</param>
        /// <param name="MapNodeData">结点主键</param>
        /// <param name="MapText">显示内容</param>
        /// <param name="MapNavigateURL">导航路径</param>
        /// <param name="ImageURL">图片地址</param>
        /// <param name="ParentName">结点父键</param>
        /// <param name="RelationOperation">结点过滤关系运算符</param>
        /// <param name="ParentValue">运算比较值</param>
        /// <param name="ParentValueType">比较值类型</param>
        /// <param name="SortExpression">排序方式</param>
        /// <param name="bcheckbox">选择框显示与否</param>
        /// <param name="MapStatus">选择框的内容</param>
        /// <param name="MapTarget">用于树保存时的分层标记</param>
        public static void CreateViewTree(DataTable dt, TreeNodeCollection treenodecollection,
         string MapNodeData, string MapText, string MapNavigateURL, string ParentName,
         string RelationOperation, string ParentValue, string ParentValueType, string SortExpression,
         bool bcheckbox, string MapStatus, string MapTarget)
        {
            if (ParentValueType == "'") ParentValue = ParentValueType + ParentValue + ParentValueType;

            string str = ParentName + RelationOperation + ParentValue;
            DataRow[] drs = dt.Select(ParentName + RelationOperation + ParentValue, SortExpression);
            foreach (DataRow dr in drs)
            {
                TreeNode treenode = new TreeNode();
                treenode.Text = dr[MapText].ToString();
                try
                {
                    treenode.NavigateUrl = dr[MapNavigateURL].ToString();
                }
                catch { treenode.NavigateUrl = ""; }
                treenode.CheckBox = bcheckbox;
                if (MapTarget != "" && MapTarget != null) treenode.TreeNodeXsltSrc = dr[MapTarget].ToString();
                if (treenode.CheckBox)
                {
                    bool bcheck;
                    try
                    {
                        bcheck = (dr[MapStatus].ToString() == "1");
                    }
                    catch { bcheck = false; }
                    treenode.Checked = bcheck;
                }
                string treenodenodedata = treenode.NodeData = dr[MapNodeData].ToString();
                treenodecollection.Add(treenode);
                if (ParentValueType == "'") treenodenodedata = ParentValueType + treenodenodedata + ParentValueType;

                DataRow[] tmpdrs = dt.Select(ParentName + RelationOperation + treenodenodedata, SortExpression);
                if (tmpdrs.Length > 0)
                {
                    CreateViewTree(dt, treenode.Nodes, MapNodeData, MapText, MapNavigateURL, ParentName, RelationOperation, treenode.NodeData, ParentValueType, SortExpression, bcheckbox, MapStatus, MapTarget);
                }
            }
        }
        #endregion

        #region Session
        public static bool CheckSession(string CheckSession, System.Web.UI.Page WebPage)
        {
            bool b = false;
            foreach (string sTmp in WebPage.Session.Keys)
            {
                if (sTmp.Equals(CheckSession))
                {
                    b = true;
                    break;
                }
            }
            return b;
        }

        public static void CheckSession(string CheckSession, System.Web.UI.Page WebPage, string ErrorPage)
        {
            bool b = false;
            foreach (string sTmp in WebPage.Session.Keys)
            {
                if (sTmp.Equals(CheckSession))
                {
                    b = true;
                    break;
                }
            }

            if (!b) WebPage.Response.Redirect(ErrorPage);
        }
        #endregion

        #region Reponse
        public static void ResponsePage(System.Web.UI.Page WebPage, string sUrl, string sChoose)
        {
            try
            {
                switch (sChoose)
                {
                    case "1":
                        WebPage.Response.Write("<script>window.location.href='" + sUrl + "';</script>");
                        break;
                    case "2":
                        WebPage.Server.Transfer(sUrl);
                        break;
                    case "3":
                        WebPage.Response.Redirect(sUrl);
                        break;
                    case "debug":
                        WebPage.Response.Write("<script>alert('" + sUrl + "')</script>");
                        break;
                    default:
                        WebPage.Response.Write(sUrl);
                        break;
                }
            }
            catch
            {
                WebPage.Response.Write("Error URL:" + sUrl);
            }
        }
        #endregion

        #region Server To Client JAVASCRIPT
        public void javaScript(string alert, System.Web.UI.Page WebPage)
        {
            WebPage.RegisterStartupScript("script", "<script language=javascript>alert('" + alert + "');</script>");
        }
        #endregion

        #region Split DataGrid
        public static string[] SplitPage(System.Web.UI.Page CurPage, DataGrid dgSplit, DataTable dtSplit, int iRowsSplitPage)
        {
            string PAGE_OBJECT = "page" + dgSplit.ID;
            string sPage = "1";
            int iCurPage = 1;
            if (CurPage.Request.QueryString[PAGE_OBJECT] != null) sPage = CurPage.Request.QueryString[PAGE_OBJECT].ToString();
            try { iCurPage = Int32.Parse(sPage); }
            catch { iCurPage = 1; }

            int start = (int)((iCurPage - 1) * iRowsSplitPage);
            if (start < 0) start = 0;
            int to = (int)(iCurPage * iRowsSplitPage);

            DataView dv = dtSplit.DefaultView;
            int iResults = dv.Count;
            int a1 = 0;
            int iPageCount = Math.DivRem(iResults, iRowsSplitPage, out a1);
            if (a1 > 0) iPageCount++;
            if (iCurPage > iPageCount || iCurPage <= 0) iCurPage = 1;
            if (iCurPage == iPageCount) to = dv.Count;

            DataTable dt = dv.Table.Clone();
            DataRow dr;
            for (int i = start; i < iResults; i++)
            {
                if (i < to)
                {
                    dr = dt.NewRow();
                    for (int k = 0; k < dv.Table.Columns.Count; k++)
                    {
                        //      if (k==0)
                        //       dr[k] = iResults - i;
                        //      else
                        dr[k] = dv.Table.Rows[i][k];
                    }
                    dt.Rows.Add(dr);
                }
            }
            dt.AcceptChanges();

            DataView dvResult = new DataView(dt);
            dgSplit.DataSource = dvResult;
            dgSplit.DataBind();
            //   dgSplit.Visible = !(dvResult.Count == 0);

            string strNav = "";
            int iEndPage = iPageCount;
            strNav += "<a href='?" + PAGE_OBJECT + "=" + 1.ToString() + "'>首  页</a>  ";
            if (iCurPage > 1)
            {
                strNav += "<a href='?" + PAGE_OBJECT + "=" + (iCurPage - 1).ToString() + "'>上  页</a>  ";
            }
            else
            {
                strNav += " 上  页 ";
            }
            if (iCurPage > 11)
            {
                strNav += "<a href='?" + PAGE_OBJECT + "=1'>1</a> ...";
            }
            if (iPageCount > iCurPage + 10) iEndPage = iCurPage + 10;
            for (int i = iCurPage - 10; i < iEndPage + 1; i++)
            {
                if (i >= 1)
                {
                    if (i == iCurPage)
                    {
                        strNav += "<font color=#990000><strong>" + i.ToString() + "</strong></font> ";
                    }
                    else
                    {
                        strNav += "<a href='?" + PAGE_OBJECT + "=" + i.ToString() + "'>" + i.ToString() + "</a> ";
                    }
                }
            }

            if ((iCurPage + 10) < iPageCount)
                strNav += "... <a href='?" + PAGE_OBJECT + "=" + iPageCount.ToString() + "'>" + iPageCount.ToString() + "</a>";
            if (iCurPage < iPageCount)
            {
                strNav += " <a href='?" + PAGE_OBJECT + "=" + (iCurPage + 1).ToString() + "'>下  页</a>  ";
            }
            else
            {
                strNav += " 下  页 ";
            }
            strNav += " <a href='?" + PAGE_OBJECT + "=" + (iPageCount).ToString() + "'>末  页</a>  ";


            string[] strNavResult = new string[2];
            strNavResult[0] = strNav;
            strNavResult[1] = "[共" + iResults.ToString() + "条][第" + (start + 1).ToString() + "-" + to.ToString() + "条][共" + iPageCount.ToString() + "页]";

            return strNavResult;
        }
        #endregion

        #region DataGrid To Excel
        public static void DataGrid2Excel(DataGrid dg, string filename)
        {
            OWC.SpreadsheetClass xlsheet = new SpreadsheetClass();
            int ihead = 0;
            foreach (DataGridColumn dgc in dg.Columns)
            {
                xlsheet.ActiveSheet.Cells[1, ihead + 1] = dgc.HeaderText;
                //    xlsheet.get_Range(xlsheet.Cells[1, 1], xlsheet.Cells[1, i + 1]).Font.Bold = true;
                //    xlsheet.get_Range(xlsheet.Cells[1, 1], xlsheet.Cells[1, i + 1]).Font.Color = "red";
                ihead++;
            }

            int icols = dg.Items[0].Cells.Count;
            for (int j = 0; j < dg.Items.Count; j++)
            {
                for (int i = 0; i < icols; i++)
                {
                    xlsheet.ActiveSheet.Cells[j + 2, i + 1] = dg.Items[j].Cells[i].Text.Replace("&nbsp;", " ");
                }
            }

            try
            {
                //System.Web.HttpContext.Current.Server.MapPath
                xlsheet.ActiveSheet.Export(filename, OWC.SheetExportActionEnum.ssExportActionOpenInExcel);
            }
            catch
            {
                return;
            }
        }
        #endregion

        #region HostName
        public static string GetHostName()
        {
            return System.Net.Dns.GetHostName();
        }

        public static System.Net.IPHostEntry GetHostByName(string hostname)
        {
            return System.Net.Dns.GetHostByName(hostname);
        }

        #endregion

        #region Process
        private void KillProcess(string processName)
        {
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();
            //得到所有打开的进程
            try
            {
                foreach (System.Diagnostics.Process thisproc in System.Diagnostics.Process.GetProcessesByName(processName))
                {
                    if (!thisproc.CloseMainWindow())
                    {
                        thisproc.Kill();
                    }
                }
            }
            catch (Exception Exc)
            {
                ;
            }
        }
        #endregion

        #region Execel
        /// <summary>
        /// Class to export a number of web controls to Excel.
        /// Pass BuildExcelTable your control, then call ExportToBrowser() to write down to the browser.  
        /// This will open a new instance of Excel.
        /// If you want to open the Excel file in the current browser window, comment out the Response.AddHEader line.
        /// </summary>
        public class ExcelWriter
        {
            protected string m_szExcelHtnml;
            private string szFileName;
            private bool bStripBreaks;

            public void BuildExcelTable(object DG)
            {
                //Header Styles
                StringWriter sw = new StringWriter();
                HtmlTextWriter ht = new HtmlTextWriter(sw);

                string sObjectTypeName = DG.GetType().Name.ToString();
                switch (sObjectTypeName)
                {
                    case "DataGrid":
                        ((DataGrid)DG).ShowHeader = true;
                        ((DataGrid)DG).RenderControl(ht);
                        break;
                    case "DataList":
                        ((DataList)DG).RenderControl(ht);
                        break;
                    case "TextBox":
                        ((TextBox)DG).RenderControl(ht);
                        break;
                    case "Label":
                        ht.Write(((Label)DG).Text);
                        break;
                    case "TableRow":
                        ht.Write("<table>");
                        ((TableRow)DG).RenderControl(ht);
                        ht.Write("</table>");
                        break;
                    case "Table":
                        ((Table)DG).RenderControl(ht);
                        break;
                    default:
                        break;
                }

                m_szExcelHtnml += sw.ToString();

            }

            public void ExportToBrowser()
            {
                if (szFileName == "" || szFileName == null)
                {
                    szFileName = "ExcelFile.xls";
                }

                if (bStripBreaks)
                {
                    m_szExcelHtnml = m_szExcelHtnml.Replace("<br>", " ");
                    m_szExcelHtnml = m_szExcelHtnml.Replace("<BR>", " ");
                }
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + szFileName);
                HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                HttpContext.Current.Response.Write(m_szExcelHtnml);
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            }

            public string FileName
            {
                get
                {
                    return szFileName;
                }
                set
                {
                    szFileName = value;
                }
            }

            public bool StripBreakTags
            {
                get
                {
                    return bStripBreaks;
                }
                set
                {
                    bStripBreaks = value;
                }
            }
        }
        #endregion

        #region PDF
        public class PDFGenerator
        {
            static float pageWidth = 594.0f;
            static float pageDepth = 828.0f;
            static float pageMargin = 30.0f;
            static float fontSize = 20.0f;
            static float leadSize = 10.0f;
            static StreamWriter pPDF = new StreamWriter("E:\\myPDF.pdf");
            static MemoryStream mPDF = new MemoryStream();
            static void ConvertToByteAndAddtoStream(string strMsg)
            {
                Byte[] buffer = null;
                buffer = ASCIIEncoding.ASCII.GetBytes(strMsg);
                mPDF.Write(buffer, 0, buffer.Length);
                buffer = null;
            }

            static string xRefFormatting(long xValue)
            {
                string strMsg = xValue.ToString();
                int iLen = strMsg.Length;
                if (iLen < 10)
                {
                    StringBuilder s = new StringBuilder();
                    int i = 10 - iLen;
                    s.Append('0', i);
                    strMsg = s.ToString() + strMsg;
                }
                return strMsg;
            }

            static void Main(string[] args)
            {
                ArrayList xRefs = new ArrayList();
                //Byte[]  buffer=null;  
                float yPos = 0f;
                long streamStart = 0;
                long streamEnd = 0;
                long streamLen = 0;
                string strPDFMessage = null;
                //PDF文档头信息  
                strPDFMessage = "%PDF-1.1\n";
                ConvertToByteAndAddtoStream(strPDFMessage);

                xRefs.Add(mPDF.Length);
                strPDFMessage = "1  0  obj\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                strPDFMessage = "<<  /Length  2  0  R  >>\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                strPDFMessage = "stream\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                ////////PDF文档描述  
                streamStart = mPDF.Length;
                //字体  
                strPDFMessage = "BT\n/F0  " + fontSize + "  Tf\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                //PDF文档实体高度  
                yPos = pageDepth - pageMargin;
                strPDFMessage = pageMargin + "  " + yPos + "  Td\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                strPDFMessage = leadSize + "  TL\n";
                ConvertToByteAndAddtoStream(strPDFMessage);

                //实体内容  
                strPDFMessage = "(http://www.wenhui.org)Tj\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                strPDFMessage = "ET\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                streamEnd = mPDF.Length;

                streamLen = streamEnd - streamStart;
                strPDFMessage = "endstream\nendobj\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                //PDF文档的版本信息  
                xRefs.Add(mPDF.Length);
                strPDFMessage = "2  0  obj\n" + streamLen + "\nendobj\n";
                ConvertToByteAndAddtoStream(strPDFMessage);

                xRefs.Add(mPDF.Length);
                strPDFMessage = "3  0  obj\n<</Type/Page/Parent  4  0  R/Contents  1  0  R>>\nendobj\n";
                ConvertToByteAndAddtoStream(strPDFMessage);

                xRefs.Add(mPDF.Length);
                strPDFMessage = "4  0  obj\n<</Type  /Pages  /Count  1\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                strPDFMessage = "/Kids[\n3  0  R\n]\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                strPDFMessage = "/Resources<</ProcSet[/PDF/Text]/Font<</F0  5  0  R>>  >>\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                strPDFMessage = "/MediaBox  [  0  0  " + pageWidth + "  " + pageDepth + "  ]\n>>\nendobj\n";
                ConvertToByteAndAddtoStream(strPDFMessage);

                xRefs.Add(mPDF.Length);
                strPDFMessage = "5  0  obj\n<</Type/Font/Subtype/Type1/BaseFont/Courier/Encoding/WinAnsiEncoding>>\nendobj\n";
                ConvertToByteAndAddtoStream(strPDFMessage);

                xRefs.Add(mPDF.Length);
                strPDFMessage = "6  0  obj\n<</Type/Catalog/Pages  4  0  R>>\nendobj\n";
                ConvertToByteAndAddtoStream(strPDFMessage);

                streamStart = mPDF.Length;
                strPDFMessage = "xref\n0  7\n0000000000  65535  f  \n";
                for (int i = 0; i < xRefs.Count; i++)
                {
                    strPDFMessage += xRefFormatting((long)xRefs[i]) + "  00000  n  \n";
                }
                ConvertToByteAndAddtoStream(strPDFMessage);
                strPDFMessage = "trailer\n<<\n/Size  " + (xRefs.Count + 1) + "\n/Root  6  0  R\n>>\n";
                ConvertToByteAndAddtoStream(strPDFMessage);

                strPDFMessage = "startxref\n" + streamStart + "\n%%EOF\n";
                ConvertToByteAndAddtoStream(strPDFMessage);
                mPDF.WriteTo(pPDF.BaseStream);

                mPDF.Close();
                pPDF.Close();
            }
        }

        #endregion

        #region Up Load Fille
        public static string UpFile(System.Web.UI.HtmlControls.HtmlInputFile infile, string savefilepath, string savefilename, string choose)
        {
            string filename = infile.PostedFile.FileName;
            if (infile.PostedFile != null && infile.Value != "")
            {
                int i;
                switch (choose)
                {
                    case ".":
                        i = filename.LastIndexOf(".");
                        savefilename += filename.Substring(i);
                        break;
                    default:
                        i = filename.LastIndexOf("/");
                        if (i <= 0) i = filename.LastIndexOf("\\");
                        savefilename += choose + filename.Substring(i + 1);
                        break;
                }
                if (savefilepath.EndsWith("/") || savefilepath.EndsWith("\\"))
                    filename = savefilepath + savefilename;
                else
                    filename = savefilepath + "/" + savefilename;
                savefilepath = System.Web.HttpContext.Current.Server.MapPath(filename);
                infile.PostedFile.SaveAs(savefilepath);
                return filename;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
