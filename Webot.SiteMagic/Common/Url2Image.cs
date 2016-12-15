/*   ---------------------------------------------------------------------------   
*   
*   Copyright   (c)   Doug   Weems.   
*     
*   You   may   use   this   code   for   fun   and   knowledge.   
*   You   can   compile   and   use   the   application   as   is   or   copy   out   what   you   need.       
*   This   code   makes   for   a   really   useful   tool.   
*     
*   ---------------------------------------------------------------------------   
*/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using SHDocVw;
using mshtml;


namespace IEWindowCapture
{
    ///   <summary>   
    ///   This   is   the   working   code   for   "IE   Complete   Web   Window   Image   Capture".     IECWWIC   for   short.       
    ///   Just   kidding.   ;-)     Call   it,   "WebPageToImage".   
    ///   This   tool   will   pick   up   one   open   IE   window   and   capture   the   entire   web   page   to   a   single   jpeg.   
    ///   It   is   best   if   you   only   are   running   the   IE   instance   that   you   are   interested   in.   
    ///   The   quality   and   size   of   the   image   can   be   adjusted   and   standard   resolution   screen   sizes   and   web   page   name   
    ///   can   be   added   to   the   image.   
    ///   Author:     Doug   Weems   
    ///   </summary>   
    public class frmMain : System.Windows.Forms.Form
    {
        private System.Windows.Forms.GroupBox grpWebCapture;
        private System.Windows.Forms.LinkLabel lnkOpenCapture;
        private System.Windows.Forms.CheckBox chkShowGuides;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbResolution;
        private System.Windows.Forms.Label lblResolution;
        private System.Windows.Forms.Label lblQuality;
        private System.Windows.Forms.ComboBox cmbQuality;
        private System.Windows.Forms.CheckBox chkWriteURL;
        private System.Windows.Forms.Button button1;
        ///   <summary>   
        ///   Required   designer   variable.   
        ///   </summary>   
        private System.ComponentModel.Container components = null;



        public frmMain()
        {
            InitializeComponent();
        }

        ///   <summary>   
        ///   Clean   up   any   resources   being   used.   
        ///   </summary>   
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region   Windows   Form   Designer   generated   code
        ///   <summary>   
        ///   Required   method   for   Designer   support   -   do   not   modify   
        ///   the   contents   of   this   method   with   the   code   editor.   
        ///   </summary>   
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
            this.grpWebCapture = new System.Windows.Forms.GroupBox();
            this.lnkOpenCapture = new System.Windows.Forms.LinkLabel();
            this.chkShowGuides = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbResolution = new System.Windows.Forms.ComboBox();
            this.lblResolution = new System.Windows.Forms.Label();
            this.lblQuality = new System.Windows.Forms.Label();
            this.cmbQuality = new System.Windows.Forms.ComboBox();
            this.chkWriteURL = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.grpWebCapture.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            //     
            //   grpWebCapture   
            //     
            this.grpWebCapture.Controls.Add(this.lnkOpenCapture);
            this.grpWebCapture.Controls.Add(this.chkShowGuides);
            this.grpWebCapture.Controls.Add(this.groupBox1);
            this.grpWebCapture.Controls.Add(this.chkWriteURL);
            this.grpWebCapture.Controls.Add(this.button1);
            this.grpWebCapture.Location = new System.Drawing.Point(28, 12);
            this.grpWebCapture.Name = "grpWebCapture";
            this.grpWebCapture.Size = new System.Drawing.Size(256, 208);
            this.grpWebCapture.TabIndex = 41;
            this.grpWebCapture.TabStop = false;
            this.grpWebCapture.Text = "Capture   Web   Page";
            //     
            //   lnkOpenCapture   
            //     
            this.lnkOpenCapture.Location = new System.Drawing.Point(16, 136);
            this.lnkOpenCapture.Name = "lnkOpenCapture";
            this.lnkOpenCapture.Size = new System.Drawing.Size(128, 16);
            this.lnkOpenCapture.TabIndex = 36;
            this.lnkOpenCapture.TabStop = true;
            this.lnkOpenCapture.Text = "Open   Capture   Directory";
            this.lnkOpenCapture.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOpenCapture_LinkClicked);
            //     
            //   chkShowGuides   
            //     
            this.chkShowGuides.Location = new System.Drawing.Point(16, 112);
            this.chkShowGuides.Name = "chkShowGuides";
            this.chkShowGuides.Size = new System.Drawing.Size(200, 16);
            this.chkShowGuides.TabIndex = 34;
            this.chkShowGuides.Text = "draw   Standard   Resolution   Guides";
            //     
            //   groupBox1   
            //     
            this.groupBox1.Controls.Add(this.cmbResolution);
            this.groupBox1.Controls.Add(this.lblResolution);
            this.groupBox1.Controls.Add(this.lblQuality);
            this.groupBox1.Controls.Add(this.cmbQuality);
            this.groupBox1.ForeColor = System.Drawing.Color.Black;
            this.groupBox1.Location = new System.Drawing.Point(16, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 80);
            this.groupBox1.TabIndex = 39;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Reduce   File   Size   by   reducing   Quality   and/or   Resolution";
            //     
            //   cmbResolution   
            //     
            this.cmbResolution.Items.AddRange(new object[]   {   "100",  "90",    "80",   "70",    "60",  "50",  "40"});
            this.cmbResolution.Location = new System.Drawing.Point(120, 32);
            this.cmbResolution.Name = "cmbResolution";
            this.cmbResolution.Size = new System.Drawing.Size(48, 21);
            this.cmbResolution.TabIndex = 31;
            this.cmbResolution.Text = "90";
            //     
            //   lblResolution   
            //     
            this.lblResolution.Location = new System.Drawing.Point(24, 32);
            this.lblResolution.Name = "lblResolution";
            this.lblResolution.Size = new System.Drawing.Size(88, 16);
            this.lblResolution.TabIndex = 35;
            this.lblResolution.Text = "%   Capture   Size";
            this.lblResolution.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //     
            //   lblQuality   
            //     
            this.lblQuality.Location = new System.Drawing.Point(24, 56);
            this.lblQuality.Name = "lblQuality";
            this.lblQuality.Size = new System.Drawing.Size(88, 16);
            this.lblQuality.TabIndex = 38;
            this.lblQuality.Text = "Quality";
            this.lblQuality.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //     
            //   cmbQuality   
            //     
            this.cmbQuality.Items.AddRange(new object[]   {   
                                                                                                                          "100",   
                                                                                                                          "90",   
                                                                                                                          "80",   
                                                                                                                          "70",   
                                                                                                                          "50",   
                                                                                                                          "30",   
                                                                                                                          "10"});
            this.cmbQuality.Location = new System.Drawing.Point(120, 56);
            this.cmbQuality.Name = "cmbQuality";
            this.cmbQuality.Size = new System.Drawing.Size(48, 21);
            this.cmbQuality.TabIndex = 37;
            this.cmbQuality.Text = "70";
            //     
            //   chkWriteURL   
            //     
            this.chkWriteURL.Location = new System.Drawing.Point(16, 96);
            this.chkWriteURL.Name = "chkWriteURL";
            this.chkWriteURL.Size = new System.Drawing.Size(176, 16);
            this.chkWriteURL.TabIndex = 32;
            this.chkWriteURL.Text = "write   URL   name   on   Image";
            //     
            //   button1   
            //     
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(176, 144);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(48, 40);
            this.button1.TabIndex = 29;
            this.button1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //     
            //   frmMain   
            //     
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(304, 238);
            this.Controls.Add(this.grpWebCapture);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "Capture";
            this.grpWebCapture.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        ///   <summary>   
        ///   The   main   entry   point   for   the   application.   
        ///   </summary>   
        [STAThread]
        static void Main()
        {
            Application.Run(new frmMain());
        }


        //We   need   some   system   dll   functions.   
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr parent   /*HWND*/, IntPtr next   /*HWND*/, string sClassName, IntPtr sWindowTitle);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport("user32.Dll")]
        public static extern void GetClassName(int h, StringBuilder s, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        public const int GW_CHILD = 5;
        public const int GW_HWNDNEXT = 2;

        private void button1_Click(object sender, System.EventArgs e)
        {
            //TODO   In   Next   Version:   
            //Add   cursor   capture   
            //Add   file   naming   option   
            //Add   visible   screen   capture   
            //Make   captured   image   a   DDB   not   a   DIB   bitmap.   

            Cursor.Current = Cursors.WaitCursor;

            SHDocVw.WebBrowser m_browser = null;

            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindowsClass();

            //Find   first   availble   browser   window.   
            //Application   can   easily   be   modified   to   loop   through   and   capture   all   open   windows.   
            string filename;
            foreach (SHDocVw.WebBrowser ie in shellWindows)
            {
                filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();

                if (filename.Equals("iexplore"))
                {
                    m_browser = ie;
                    break;
                }
            }
            if (m_browser == null)
            {
                MessageBox.Show("No   Browser   Open");
                return;
            }

            //Assign   Browser   Document   
            mshtml.IHTMLDocument2 myDoc = (mshtml.IHTMLDocument2)m_browser.Document;


            //URL   Location   
            string myLocalLink = myDoc.url;
            int URLExtraHeight = 0;
            int URLExtraLeft = 0;

            //Adjustment   variable   for   capture   size.   
            if (chkWriteURL.Checked == true)
                URLExtraHeight = 25;

            //TrimHeight   and   TrimLeft   trims   off   some   captured   IE   graphics.   
            int trimHeight = 3;
            int trimLeft = 3;

            //Use   UrlExtra   height   to   carry   trimHeight.   
            URLExtraHeight = URLExtraHeight - trimHeight;
            URLExtraLeft = URLExtraLeft - trimLeft;

            myDoc.body.setAttribute("scroll", "yes", 0);

            //Get   Browser   Window   Height   
            int heightsize = (int)myDoc.body.getAttribute("scrollHeight", 0);
            int widthsize = (int)myDoc.body.getAttribute("scrollWidth", 0);

            //Get   Screen   Height   
            int screenHeight = (int)myDoc.body.getAttribute("clientHeight", 0);
            int screenWidth = (int)myDoc.body.getAttribute("clientWidth", 0);

            //Get   bitmap   to   hold   screen   fragment.   
            Bitmap bm = new Bitmap(screenWidth, screenHeight, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            //Create   a   target   bitmap   to   draw   into.   
            Bitmap bm2 = new Bitmap(widthsize + URLExtraLeft, heightsize + URLExtraHeight - trimHeight, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            Graphics g2 = Graphics.FromImage(bm2);

            Graphics g = null;
            IntPtr hdc;
            Image screenfrag = null;
            int brwTop = 0;
            int brwLeft = 0;
            int myPage = 0;
            IntPtr myIntptr = (IntPtr)m_browser.HWND;
            //Get   inner   browser   window.   
            int hwndInt = myIntptr.ToInt32();
            IntPtr hwnd = myIntptr;
            hwnd = GetWindow(hwnd, GW_CHILD);
            StringBuilder sbc = new StringBuilder(256);
            //Get   Browser   "Document"   Handle   
            while (hwndInt != 0)
            {
                hwndInt = hwnd.ToInt32();
                GetClassName(hwndInt, sbc, 256);

                if (sbc.ToString().IndexOf("Shell   DocObject   View", 0) > -1)
                {
                    hwnd = FindWindowEx(hwnd, IntPtr.Zero, "Internet   Explorer_Server", IntPtr.Zero);
                    break;
                }
                hwnd = GetWindow(hwnd, GW_HWNDNEXT);

            }

            //Get   Screen   Height   (for   bottom   up   screen   drawing)   
            while ((myPage * screenHeight) < heightsize)
            {
                myDoc.body.setAttribute("scrollTop", (screenHeight - 5) * myPage, 0);
                ++myPage;
            }
            //Rollback   the   page   count   by   one   
            --myPage;

            int myPageWidth = 0;

            while ((myPageWidth * screenWidth) < widthsize)
            {
                myDoc.body.setAttribute("scrollLeft", (screenWidth - 5) * myPageWidth, 0);
                brwLeft = (int)myDoc.body.getAttribute("scrollLeft", 0);
                for (int i = myPage; i >= 0; --i)
                {
                    //Shoot   visible   window   
                    g = Graphics.FromImage(bm);
                    hdc = g.GetHdc();
                    myDoc.body.setAttribute("scrollTop", (screenHeight - 5) * i, 0);
                    brwTop = (int)myDoc.body.getAttribute("scrollTop", 0);
                    PrintWindow(hwnd, hdc, 0);
                    g.ReleaseHdc(hdc);
                    g.Flush();
                    screenfrag = Image.FromHbitmap(bm.GetHbitmap());
                    g2.DrawImage(screenfrag, brwLeft + URLExtraLeft, brwTop + URLExtraHeight);
                }
                ++myPageWidth;
            }

            //Draw   Standard   Resolution   Guides   
            if (chkShowGuides.Checked == true)
            {
                //   Create   pen.   
                int myWidth = 1;
                Pen myPen = new Pen(Color.Navy, myWidth);
                Pen myShadowPen = new Pen(Color.NavajoWhite, myWidth);
                //   Create   coordinates   of   points   that   define   line.   
                float x1 = -(float)myWidth - 1 + URLExtraLeft;
                float y1 = -(float)myWidth - 1 + URLExtraHeight;

                float x600 = 600.0F + (float)myWidth + 1;
                float y480 = 480.0F + (float)myWidth + 1;

                float x2 = 800.0F + (float)myWidth + 1;
                float y2 = 600.0F + (float)myWidth + 1;

                float x3 = 1024.0F + (float)myWidth + 1;
                float y3 = 768.0F + (float)myWidth + 1;

                float x1280 = 1280.0F + (float)myWidth + 1;
                float y1024 = 1024.0F + (float)myWidth + 1;

                //   Draw   line   to   screen.   
                g2.DrawRectangle(myPen, x1, y1, x600 + myWidth, y480 + myWidth);
                g2.DrawRectangle(myPen, x1, y1, x2 + myWidth, y2 + myWidth);
                g2.DrawRectangle(myPen, x1, y1, x3 + myWidth, y3 + myWidth);
                g2.DrawRectangle(myPen, x1, y1, x1280 + myWidth, y1024 + myWidth);

                //   Create   font   and   brush.   
                Font drawFont = new Font("Arial", 12);
                SolidBrush drawBrush = new SolidBrush(Color.Navy);
                SolidBrush drawBrush2 = new SolidBrush(Color.NavajoWhite);

                //   Set   format   of   string.   
                StringFormat drawFormat = new StringFormat();
                drawFormat.FormatFlags = StringFormatFlags.FitBlackBox;
                //   Draw   string   to   screen.   
                g2.DrawString("600   x   480", drawFont, drawBrush, 5, y480 - 20 + URLExtraHeight, drawFormat);
                g2.DrawString("800   x   600", drawFont, drawBrush, 5, y2 - 20 + URLExtraHeight, drawFormat);
                g2.DrawString("1024   x   768", drawFont, drawBrush, 5, y3 - 20 + URLExtraHeight, drawFormat);
                g2.DrawString("1280   x   1024", drawFont, drawBrush, 5, y1024 - 20 + URLExtraHeight, drawFormat);
            }

            //Write   URL   
            if (chkWriteURL.Checked == true)
            {       //Backfill   URL   paint   location   
                SolidBrush whiteBrush = new SolidBrush(Color.White);
                Rectangle fillRect = new Rectangle(0, 0, widthsize, URLExtraHeight + 2);
                Region fillRegion = new Region(fillRect);
                g2.FillRegion(whiteBrush, fillRegion);

                SolidBrush drawBrushURL = new SolidBrush(Color.Black);
                Font drawFont = new Font("Arial", 12);
                StringFormat drawFormat = new StringFormat();
                drawFormat.FormatFlags = StringFormatFlags.FitBlackBox;

                g2.DrawString(myLocalLink, drawFont, drawBrushURL, 0, 0, drawFormat);
            }

            //Reduce   Resolution   Size   
            double myResolution = Convert.ToDouble(cmbResolution.Text) * 0.01;
            int finalWidth = (int)((widthsize + URLExtraLeft) * myResolution);
            int finalHeight = (int)((heightsize + URLExtraHeight) * myResolution);
            Bitmap finalImage = new Bitmap(finalWidth, finalHeight, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            Graphics gFinal = Graphics.FromImage((Image)finalImage);
            gFinal.DrawImage(bm2, 0, 0, finalWidth, finalHeight);

            //Get   Time   Stamp   
            DateTime myTime = DateTime.Now;
            String format = "MM.dd.hh.mm.ss";

            //Create   Directory   to   save   image   to.   
            Directory.CreateDirectory("C:\\IECapture");

            //Write   Image.   
            EncoderParameters eps = new EncoderParameters(1);
            long myQuality = Convert.ToInt64(cmbQuality.Text);
            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, myQuality);
            ImageCodecInfo ici = GetEncoderInfo("image/jpeg");
            finalImage.Save(@"c:\\IECapture\Captured_" + myTime.ToString(format) + ".jpg", ici, eps);


            //Clean   Up.   
            myDoc = null;
            g.Dispose();
            g2.Dispose();
            gFinal.Dispose();
            bm.Dispose();
            bm2.Dispose();
            finalImage.Dispose();

            Cursor.Current = Cursors.Default;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private void lnkOpenCapture_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", "C:\\IECapture");
        }



    }
}
