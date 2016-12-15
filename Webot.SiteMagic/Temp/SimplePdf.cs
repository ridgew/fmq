//write by wenhui.org
using System;
using System.IO;
using System.Text;
using System.Collections;

namespace PDFGenerator
{

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
            //Byte[] buffer=null;
            float yPos = 0f;
            long streamStart = 0;
            long streamEnd = 0;
            long streamLen = 0;
            string strPDFMessage = null;
            //PDF文档头信息
            strPDFMessage = "%PDF-1.1\n";
            ConvertToByteAndAddtoStream(strPDFMessage);

            xRefs.Add(mPDF.Length);
            strPDFMessage = "1 0 obj\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            strPDFMessage = "<< /Length 2 0 R >>\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            strPDFMessage = "stream\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            ////////PDF文档描述
            streamStart = mPDF.Length;
            //字体
            strPDFMessage = "BT\n/F0 " + fontSize + " Tf\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            //PDF文档实体高度
            yPos = pageDepth - pageMargin;
            strPDFMessage = pageMargin + " " + yPos + " Td\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            strPDFMessage = leadSize + " TL\n";
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
            strPDFMessage = "2 0 obj\n" + streamLen + "\nendobj\n";
            ConvertToByteAndAddtoStream(strPDFMessage);

            xRefs.Add(mPDF.Length);
            strPDFMessage = "3 0 obj\n<</Type/Page/Parent 4 0 R/Contents 1 0 R>>\nendobj\n";
            ConvertToByteAndAddtoStream(strPDFMessage);

            xRefs.Add(mPDF.Length);
            strPDFMessage = "4 0 obj\n<</Type /Pages /Count 1\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            strPDFMessage = "/Kids[\n3 0 R\n]\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            strPDFMessage = "/Resources<</ProcSet[/PDF/Text]/Font<</F0 5 0 R>> >>\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            strPDFMessage = "/MediaBox [ 0 0 " + pageWidth + " " + pageDepth + " ]\n>>\nendobj\n";
            ConvertToByteAndAddtoStream(strPDFMessage);

            xRefs.Add(mPDF.Length);
            strPDFMessage = "5 0 obj\n<</Type/Font/Subtype/Type1/BaseFont/Courier/Encoding/WinAnsiEncoding>>\nendobj\n";
            ConvertToByteAndAddtoStream(strPDFMessage);

            xRefs.Add(mPDF.Length);
            strPDFMessage = "6 0 obj\n<</Type/Catalog/Pages 4 0 R>>\nendobj\n";
            ConvertToByteAndAddtoStream(strPDFMessage);

            streamStart = mPDF.Length;
            strPDFMessage = "xref\n0 7\n0000000000 65535 f \n";
            for (int i = 0; i < xRefs.Count; i++)
            {
                strPDFMessage += xRefFormatting((long)xRefs[i]) + " 00000 n \n";
            }
            ConvertToByteAndAddtoStream(strPDFMessage);
            strPDFMessage = "trailer\n<<\n/Size " + (xRefs.Count + 1) + "\n/Root 6 0 R\n>>\n";
            ConvertToByteAndAddtoStream(strPDFMessage);

            strPDFMessage = "startxref\n" + streamStart + "\n%%EOF\n";
            ConvertToByteAndAddtoStream(strPDFMessage);
            mPDF.WriteTo(pPDF.BaseStream);

            mPDF.Close();
            pPDF.Close();
        }
    }
}

