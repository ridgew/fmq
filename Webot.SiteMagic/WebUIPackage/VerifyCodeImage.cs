using System;
using System.IO;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;

namespace Webot.WebUIPackage
{
    public class VerifyCodeImage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string verifyCode = MakeCode(4);
            Session["WebotVerifyCode"] = verifyCode;
            MakeImg(verifyCode);
        }

        public static string MakeCode(int Num)
        {
            string[] textArray1 = "0,1,2,3,4,5,6,7,8,9".Split(new char[] { ',' });
            string text2 = string.Empty;
            int num1 = 0;
            Random random1 = new Random();
            for (int num2 = 1; num2 <= Num; num2++)
            {
                num1 = random1.Next(10);
                text2 = text2 + textArray1[num1];
            }
            return text2;
        }

        public static void MakeImg(string Str)
        {
            MemoryStream stream1 = new MemoryStream();
            int num1 = Convert.ToInt32((double)(Str.Length * 11.5));
            int num2 = 20;
            Bitmap bitmap1 = new Bitmap(num1, num2);
            Graphics graphics1 = Graphics.FromImage(bitmap1);
            Color[] colorArray3 = new Color[] { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Brown, Color.DarkCyan, Color.Purple };
            Color[] colorArray1 = colorArray3;
            colorArray3 = new Color[] { Color.LightBlue, Color.LightCoral, Color.LightCyan, Color.LightGoldenrodYellow, Color.LightGray, Color.LightGreen, Color.LightPink, Color.LightSalmon, Color.LightSeaGreen, Color.LightSkyBlue, Color.LightYellow };
            Color[] colorArray2 = colorArray3;
            Random random1 = new Random();
            Pen pen1 = new Pen(Color.LightBlue, 0f);
            int num3 = 20;
            for (int num4 = 0; num4 < num3; num4++)
            {
                pen1 = new Pen(colorArray2[random1.Next(11)], 0f);
                Point point1 = new Point(random1.Next(num1), random1.Next(num2));
                Point point2 = new Point(random1.Next(num1), random1.Next(num2));
                Point point3 = new Point(random1.Next(num1), random1.Next(num2));
                Point point4 = new Point(random1.Next(num1), random1.Next(num2));
                graphics1.DrawBezier(pen1, point1, point2, point3, point4);
            }
            string text1 = string.Empty;
            int num5 = 2;
            for (int num6 = 0; num6 < Str.Length; num6++)
            {
                text1 = Str.Substring(num6, 1);
                graphics1.DrawString(text1, new Font("Arial", 12f, FontStyle.Bold), new SolidBrush(colorArray1[random1.Next(6)]), (float)num5, 2f);
                num5 += 10;
            }
            bitmap1.Save(stream1, ImageFormat.Png);
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ContentType = "image/Png";
            HttpContext.Current.Response.BinaryWrite(stream1.ToArray());
            graphics1.Dispose();
            bitmap1.Dispose();
            HttpContext.Current.Response.End();
        }
    }
}
