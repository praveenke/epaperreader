using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections;
using HtmlParser;
namespace DownloadManager
{
    public partial class eMat : Form
    {
        # region variables
        // The thread inside which the Page Names display happens
        private Thread thrDisplayPageNames;
        // The thread inside which the sequencing happens
        private Thread thrSequencer;
        // The thread inside which the download happens
        private Thread thrDownload;
        // The stream of data retrieved from the web server
        private Stream strResponse;
        // The stream of data that we write to the harddrive
        private Stream strLocal;
        // The request to the web server for file information
        private HttpWebRequest webRequest;
        // The response from the web server containing information about the file
        private HttpWebResponse webResponse;
        // The progress of the download in percentage
        private static int PercentProgress;
        // The delegate which we will call from the thread to update the form
        private delegate void UpdateProgessCallback(Int64 BytesRead, Int64 TotalBytes);
        private delegate void DisplayFileNameCallback(bool clear);
        private delegate void AddControlCallback(Control parent, Control ctrl);
        private delegate void DisplayLinkLabelCallback(string FilePath, int DownloadCount);
        private delegate void UpdateTextBoxCallback(string text);
        string strURL = string.Empty;
        string strPath = string.Empty;
        bool tmp = false;
        bool blnDisplayPagesDone = false;
        # endregion
        public eMat()
        {
            InitializeComponent();
        }
        private void Form3_Load(object sender, EventArgs e)
        {
            this.panel1.Height = 70;
            this.FormBorderStyle = FormBorderStyle.None;
            axAcroPDF1.Width = this.Width;
            axAcroPDF1.Height = this.Height;
            axAcroPDF1.Top = 0;
            axAcroPDF1.Left = 0;
            //axAcroPDF1.src = @"E:\Projects\epaperreader\MAT\bin\Release\Mathrubhoomi\2010-Oct-16_01_Dai_16558.pdf";            
            thrDisplayPageNames = new Thread(DisplayPageNames);
            thrDisplayPageNames.Start();        
        }
        #region DisplayPageNames
        bool GotIt = false;
        ArrayList arrswfFiles = new ArrayList();
        string divs = string.Empty;
        void DisplayPageNames()
        {
            GotIt = false;
            string twodigitday = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string twodigitmonth = DateTime.Now.Month.ToString().Length == 1 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string datestring = DateTime.Now.Year + "-" + twodigitmonth + "-" + twodigitday;

            try
            {
                WebRequest request = HttpWebRequest.Create("http://epaper.mathrubhumi.com/index.php?cat=14&pdf=Y&date=" + datestring);
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                HtmlParser.HtmlDocument document = HtmlParser.HtmlDocument.Create(stream);
                GetContainerDIV(document.Nodes);
                GenerateLinkLabels();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                //if(!PagesFileExists)
                //    File.WriteAllLines("PagesOf"+DateTime.Now.Day+" - "+DateTime.Now.Month+" - "+DateTime.Now.Year+".Txt", (LocalDownloadCount).ToString().Split(".".ToCharArray()));
                thrDisplayPageNames.Abort();
            }

        }
        void GetContainerDIV(HtmlNodeCollection nodes)
        {
            foreach (HtmlNode node in nodes)
            {
                HtmlParser.HtmlElement element = node as HtmlParser.HtmlElement;
                if ((null != element) && (((element.Name.ToLower() == "html")) || (element.Name.ToLower() == "body") || (element.Name.ToLower() == "table") || (element.Name.ToLower() == "tr") || (element.Name.ToLower() == "td") || (((element.Name.ToLower() == "div")) && (element.Attributes["id"] != null) && ((element.Attributes["id"].Value == "wrapper") || (element.Attributes["id"].Value == "maincol") || (element.Attributes["id"].Value == "centercol")))))
                {
                    GetContainerDIV(element.Nodes);
                    //if (GotIt == true)
                    //    break;
                }
                //if ((null != element) && ((element.Name.ToLower() == "div")) && (element.Attributes["id"]!=null) && (element.Attributes["id"].Value == "container"))
                if ((null != element) && ((element.Name.ToLower() == "div")) && (element.Html.Contains(".pdf")))
                {
                    //textBox1.Text = element.Html;
                    CreateTheURLs(element.Nodes);
                    //GotIt = true;
                    //break;
                }
            }

        }
        void GenerateLinkLabels()
        {
            Label PageName;
            CheckBox ChkPageName;
            //use the urls from arrswfFiles to generate lls
            //also add event handler to invokde clicked events
            int locationX = 0;
            int LocationY = 40;
            int PageNo = 0;
            for (int i = 0; i < arrswfFiles.Count; i++)
            {
                PageNo = i + 1;
                PageName = new Label();
                ChkPageName = new CheckBox();
                //if (PageNo > 9)
                //    PageName.Width = 25;
                //else
                //    PageName.Width = 20;
                //PageName.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
                PageName.Text = (PageNo).ToString();// "Page" + (i + 1);
                PageName.Name = (PageNo).ToString();
                PageName.BackColor = Color.White;
                PageName.BorderStyle = BorderStyle.FixedSingle;
                PageName.TextAlign = ContentAlignment.MiddleLeft;
                ChkPageName.Name = "chk" + PageNo.ToString();

                if ((PageNo + 1) > 9)
                {
                    PageName.Width = 25;
                    ChkPageName.Width = 20;
                }
                else
                {
                    PageName.Width = 20;
                    ChkPageName.Width = 20;
                }

                if (((PageNo + 1) > 9) && (locationX > 0))
                    locationX += 50;
                else
                    locationX += 45;


                PageName.Location = new Point(locationX, LocationY);
                ChkPageName.Location = new Point(locationX + 25, LocationY);
                //ChkPageName.Text = PageNo.ToString();

                this.Invoke(new AddControlCallback(this.AddControl), new object[] { this, PageName });
                this.Invoke(new AddControlCallback(this.AddControl), new object[] { this, ChkPageName });

                //if (PageNo % 9 == 0)
                //{
                //    locationX = 0;
                //    LocationY += 30;
                //}

                //if (PageNo > 9)
                //    locationX += 50;
                //else
                //    locationX += 40;
            }
        }
        void CreateTheURLs(HtmlNodeCollection nodes)
        {
            string url = string.Empty;
            DateTime dt = DateTime.Now;
            string datestring = string.Empty;
            string swfFileID = string.Empty;
            datestring = DateTime.Now.ToString("MMM"); //String.Format("{0:MMM}", DateTime.Today.ToString());
            //http://media.mathrubhumi.com/flashpaper/2010/Jan/08/2010-Jan-08_1_Dai_9802.swf
            //<a href='index.php?id=9802&amp;cat=14&amp;date=2010-01-08' id='page2'>1</a>
            foreach (HtmlNode node in nodes)
            {
                HtmlParser.HtmlElement element = node as HtmlParser.HtmlElement;
                if ((null != element) && (element.Name.ToLower() == "a"))
                {
                    //swfFileID = element.Attributes["href"].Value.Substring(13, element.Attributes["href"].Value.IndexOf("&")-13);
                    //string twodigitday = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
                    //url = "http://media.mathrubhumi.com/flashpaper/" + DateTime.Now.Year + "/" + DateTime.Now.ToString("MMM") + "/" + twodigitday + "/" + DateTime.Now.Year + "-" + DateTime.Now.ToString("MMM") + "-" + twodigitday + "_" + element.Text + "_Dai_" + swfFileID + ".swf";
                    url = element.Attributes["href"].Value;
                    arrswfFiles.Add(url);
                    divs += url + "\r\n";
                }
            }
            //textBox1.Text = divs;
            //Label l = new Label();

            //l.Location = new Point(5, 100);
            //l.Text = "Pages:";
            //l.Width = 45;

            //this.Invoke(new AddControlCallback(this.AddControl), new object[] { this, l });

        }
        void AddControl(Control parent, Control ctrl)
        {
            parent.Controls["panel1"].Controls.Add(ctrl);
        }
        #endregion
        #region Download
        int DownloadCount = 0;
        Int64 fileSize;
        string strPDFFileToDownLoad = string.Empty;
        private void DownloadInQueue()
        {
            int LocalDownloadCount = -1;
            //thrDisplayPageNames = new Thread(DisplayPageNames);
            //thrDisplayPageNames.Start();
            //DisplayPageNames();

            try
            {
                for (int i = 0; i < arrswfFiles.Count; i++)
                {
                    DownloadCount = i;
                    //this.Invoke(new UpdateTextBoxCallback(this.UpdateTextBox), new object[] { "chk" + (DownloadCount + 1).ToString() });
                    CheckBox chkTemp = this.Controls["panel1"].Controls["chk" + (i + 1).ToString()] != null ? (CheckBox)this.Controls["panel1"].Controls["chk" + (i + 1).ToString()] : null;
                    if ((chkTemp != null) && (chkTemp.Checked == false))
                    {
                        LocalDownloadCount = i;
                        continue;
                    }
                    else
                    {
                        if (tmp == true)
                        {
                            tmp = false;
                        }
                        if (fileSize == -1)
                        {
                            this.Invoke(new DisplayFileNameCallback(this.DisplayFileName), new object[] { true });
                            //thrDownload.Abort();
                            break;
                        }
                        if (LocalDownloadCount != i)
                        {
                            fileSize = 0;
                            LocalDownloadCount = i;
                            //SetFileURL(i + 1);
                            strURL = arrswfFiles[i].ToString();
                            strPDFFileToDownLoad = arrswfFiles[i].ToString().Split("/".ToCharArray())[7].ToString();
                            SetSavePath(strPDFFileToDownLoad);
                            this.Invoke(new UpdateProgessCallback(this.UpdateProgress), new object[] { 0, 1 });
                            this.Invoke(new DisplayFileNameCallback(this.DisplayFileName), new object[] { false });
                            // Create a new thread that calls the Download() method
                            //--thrDownload = new Thread(Download);
                            // Start the thread, and thus call Download()
                            //--thrDownload.Start();
                            Download(i);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {

            }
            finally
            {
                //button1.Enabled = true;
                thrSequencer.Abort();
            }
        }
        private void SetSavePath(string strFileName)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Mathrubhoomi"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\Mathrubhoomi");
            }
            strPath = AppDomain.CurrentDomain.BaseDirectory + @"\Mathrubhoomi\" + strFileName;
        }
        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            // Calculate the download progress in percentages
            PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);
            // Make progress on the progress bar
            ((ProgressBar)Controls["panel1"].Controls["prgDownload"]).Value = PercentProgress;
            // Display the current progress on the form
            Controls["panel1"].Controls["lblProgress"].Text = "Downloaded Page" + (DownloadCount + 1).ToString() + " " + BytesRead + " out of " + TotalBytes + " (" + PercentProgress + "%)";
        }
        void DisplayLinkLabel(string FilePath, int DownloadCount)
        {
            LinkLabel PageName = new LinkLabel();
            Label lblTemp = this.Controls["panel1"].Controls[(DownloadCount + 1).ToString()] != null ? (Label)this.Controls["panel1"].Controls[(DownloadCount + 1).ToString()] : null;
            if (lblTemp != null)
            {
                PageName.Text = (DownloadCount + 1).ToString();
                PageName.Name = FilePath;
                PageName.Location = lblTemp.Location;
                PageName.Width = lblTemp.Width;
                PageName.BorderStyle = BorderStyle.FixedSingle;
                lblTemp.Visible = false;
                PageName.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
                this.Invoke(new AddControlCallback(this.AddControl), new object[] { this, PageName });
            }
        }
        private void Download(int DownloadCount)
        {
            using (WebClient wcDownload = new WebClient())
            {
                try
                {
                    // Create a request to the file we are downloading
                    webRequest = (HttpWebRequest)WebRequest.Create(strURL);
                    // Set default authentication for retrieving the file
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    // Retrieve the response from the server
                    webResponse = (HttpWebResponse)webRequest.GetResponse();
                    // Ask the server for the file size and store it
                    fileSize = webResponse.ContentLength;
                    if ((File.Exists(strPath)) && (File.ReadAllBytes(strPath).Length == fileSize))//file already exists and completely downloaded previous time
                        this.Invoke(new UpdateProgessCallback(this.UpdateProgress), new object[] { fileSize, fileSize });
                    else
                    {
                        // Open the URL for download 
                        strResponse = wcDownload.OpenRead(strURL);
                        // Create a new file stream where we will be saving the data (local drive)
                        strLocal = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.None);

                        // It will store the current number of bytes we retrieved from the server
                        int bytesSize = 0;
                        // A buffer for storing and writing the data retrieved from the server
                        byte[] downBuffer = new byte[2048];

                        // Loop through the buffer until the buffer is empty
                        while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                        {
                            // Write the data from the buffer to the local hard drive
                            strLocal.Write(downBuffer, 0, bytesSize);
                            // Invoke the method that updates the form's label and progress bar
                            this.Invoke(new UpdateProgessCallback(this.UpdateProgress), new object[] { strLocal.Length, fileSize });
                        }
                    }
                }
                catch (Exception ex)
                {
                    string str = ex.Message;
                    tmp = true;
                }
                finally
                {
                    if (webResponse.StatusCode == HttpStatusCode.OK)
                        webResponse.Close();
                    if (strResponse != null)
                    {
                        // When the above code has ended, close the streams
                        strResponse.Close();
                        strLocal.Close();
                    }
                    if (fileSize == 0)
                    {
                        fileSize = -1; //to acknowledge the failure 404; to set a value different than initial
                    }
                    if (fileSize != 0)
                    {
                        this.Invoke(new DisplayLinkLabelCallback(this.DisplayLinkLabel), new object[] { strPath, DownloadCount });
                        //DownloadCount++;
                    }
                }
            }
        }
        void DisplayFileName(bool clear)
        {
            // Let the user know we are connecting to the server
            if (clear)
                Controls["panel1"].Controls["lblProgress"].Text = "You got the pages!";
            else
                Controls["panel1"].Controls["lblProgress"].Text = "Getting Page " + (DownloadCount + 1).ToString();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel objLinkLabel = (LinkLabel)sender;
            //System.Diagnostics.Process.Start(objLinkLabel.Name.ToString());
            axAcroPDF1.src = objLinkLabel.Name.ToString();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            DownloadCount = 0;
            fileSize = 0;
            //DownloadInQueue();
            // Create a new thread that calls the DownloadInQueue() method            
            thrSequencer = new Thread(DownloadInQueue);
            // Start the thread, and thus call DownloadInQueue()
            thrSequencer.Start();
            //button1.Enabled = false;
        }
        #endregion 
        private void panel1_MouseHover(object sender, EventArgs e)
        {
            //MessageBox.Show(sender.ToString());            
            this.panel1.Height = 70;
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //MessageBox.Show(e.Y.ToString());
            //label1.Text = e.Y.ToString();
            if(e.Y>65)
                this.panel1.Height = 10;
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void lblProgress_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }        
    }
}