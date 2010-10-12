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
namespace DownloadManager
{
    public partial class Form1 : Form
    {
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

        public Form1()
        {
            InitializeComponent();
        }
        private void btnDownload_Click(object sender, EventArgs e)
        {
            // Let the user know we are connecting to the server
            lblProgress.Text = "Getting";
            // Create a new thread that calls the Download() method
            thrDownload = new Thread(Download);
            // Start the thread, and thus call Download()
            thrDownload.Start();
        }
        int DownloadCount = 0;
        private void DownloadInQueue()
        {
            
            int LocalDownloadCount = -1;
            //thrDisplayPageNames = new Thread(DisplayPageNames);
            //thrDisplayPageNames.Start();
            //DisplayPageNames();

            try
            {
                while (DownloadCount >= 0)
                {
                    //this.Invoke(new UpdateTextBoxCallback(this.UpdateTextBox), new object[] { "chk" + (DownloadCount + 1).ToString() });
                    CheckBox chkTemp = this.Controls["chk" + (DownloadCount + 1).ToString()] != null ? (CheckBox)this.Controls["chk" + (DownloadCount + 1).ToString()] : null;
                    if ((chkTemp != null) && (chkTemp.Checked == false))
                    {
                        LocalDownloadCount = DownloadCount;
                        DownloadCount++;
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
                        if (LocalDownloadCount != DownloadCount)
                        {
                            fileSize = 0;
                            LocalDownloadCount = DownloadCount;
                            SetFileURL(DownloadCount + 1);
                            SetSavePath();
                            this.Invoke(new UpdateProgessCallback(this.UpdateProgress), new object[] { 0, 1 });
                            this.Invoke(new DisplayFileNameCallback(this.DisplayFileName), new object[] { false });
                            // Create a new thread that calls the Download() method
                            //--thrDownload = new Thread(Download);
                            // Start the thread, and thus call Download()
                            //--thrDownload.Start();
                            Download();
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
        string strPDFFileToDownLoad = string.Empty;
        private void SetFileURL(int SeqNum)
        {//"/TOIBG/2010/01/12/TOIBG_2010_1_12_1.pdf"
            string twodigitday = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string twodigitmonth = DateTime.Now.Month.ToString().Length == 1 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            strPDFFileToDownLoad = "TOIBG_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + SeqNum + ".pdf";
            strURL = @"http://epaper.timesofindia.com/Repository/TOIBG/" + DateTime.Now.Year + "/" +
                twodigitmonth + "/" + twodigitday + "/" + strPDFFileToDownLoad;
        }
        private string GetFileURL(int SeqNum)
        {
            string twodigitday = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string twodigitmonth = DateTime.Now.Month.ToString().Length == 1 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            strPDFFileToDownLoad = "TOIBG_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + SeqNum + ".pdf";
            strURL = @"http://epaper.timesofindia.com/Repository/TOIBG/" + DateTime.Now.Year + "/" +
                twodigitmonth + "/" + twodigitday + "/" + strPDFFileToDownLoad;
            strPDFFileToDownLoad = string.Empty;
            return strURL;
        }
        private void SetSavePath()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\TOI"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\TOI");
            }
            strPath = AppDomain.CurrentDomain.BaseDirectory + @"\TOI\" + strPDFFileToDownLoad;
        }
        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            // Calculate the download progress in percentages
            PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);
            // Make progress on the progress bar
            prgDownload.Value = PercentProgress;
            // Display the current progress on the form
            lblProgress.Text = "Downloaded Page" + (DownloadCount + 1).ToString() + " " + BytesRead + " out of " + TotalBytes + " (" + PercentProgress + "%)";
        }
        void DisplayFileName(bool clear)
        {
            // Let the user know we are connecting to the server
            if (clear)
                lblProgress.Text = "You got the pages!";
            else
                lblProgress.Text = "Getting Page " + (DownloadCount + 1).ToString();
        }
        void DisplayLinkLabel(string FilePath, int DownloadCount)
        {
            LinkLabel PageName = new LinkLabel();
            Label lblTemp = this.Controls[(DownloadCount + 1).ToString()] != null ? (Label)this.Controls[(DownloadCount + 1).ToString()] : null;
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
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel objLinkLabel = (LinkLabel)sender;
            System.Diagnostics.Process.Start(objLinkLabel.Name);
        }
        void DisplayPageNames()
        {
            Label PageName;
            CheckBox ChkPageName;
            int LocalDownloadCount = -1;
            int LocationX = 0;
            int LocationY = 40;
            int DownloadCountLocal = 0;
            long fileSizeLocal = 0;
            bool PagesFileExists = false;
            int StoredPageNo = 0;
            PagesFileExists = File.Exists("PagesOf" + DateTime.Now.Day + " - " + DateTime.Now.Month + " - " + DateTime.Now.Year + ".Txt");
            if (PagesFileExists)
                StoredPageNo = Convert.ToInt32(File.ReadAllLines("PagesOf" + DateTime.Now.Day + " - " + DateTime.Now.Month + " - " + DateTime.Now.Year + ".Txt")[0].ToString().Trim());
            try
            {
                while (DownloadCountLocal >= 0)
                {
                    if (fileSizeLocal == -1)
                    {
                        fileSizeLocal = 0;
                        break;
                    }
                    if ((PagesFileExists) && (StoredPageNo == DownloadCountLocal))
                        break;
                    if (LocalDownloadCount != DownloadCountLocal)
                    {
                        fileSizeLocal = 0;
                        LocalDownloadCount = DownloadCountLocal;
                        if (!PagesFileExists)
                        {
                            webRequest = (HttpWebRequest)WebRequest.Create(GetFileURL(DownloadCountLocal + 1));
                            // Set default authentication for retrieving the file
                            webRequest.Credentials = CredentialCache.DefaultCredentials;
                            // Retrieve the response from the server
                            webResponse = (HttpWebResponse)webRequest.GetResponse();
                            // Ask the server for the file size and store it
                            fileSizeLocal = webResponse.ContentLength;
                            webResponse.Close();
                        }
                        if ((fileSizeLocal > 0)||(PagesFileExists))
                        {
                            PageName = new Label();
                            ChkPageName = new CheckBox();
                            PageName.Name = (DownloadCountLocal + 1).ToString();
                            PageName.BorderStyle = BorderStyle.FixedSingle;
                            PageName.TextAlign = ContentAlignment.MiddleLeft;
                            ChkPageName.Name = "chk" + (DownloadCountLocal + 1).ToString();
                            
                            if ((DownloadCountLocal + 1) > 9)
                            {
                                PageName.Width = 25;
                                ChkPageName.Width = 20;
                            }
                            else
                            {
                                PageName.Width = 20;
                                ChkPageName.Width = 20;
                            }
                            PageName.Text = (DownloadCountLocal + 1).ToString();
                            //if (DownloadCountLocal > 0)
                            //    PageName.Text = ", " + PageName.Text;
                            if (((DownloadCountLocal + 1) > 9) && (LocationX > 0))
                                LocationX += 50;
                            else
                                LocationX += 45;

                            PageName.Location = new Point(LocationX, LocationY);
                            ChkPageName.Location = new Point(LocationX + 25, LocationY);
                            // Invoke the method that updates the form's label and progress bar
                            this.Invoke(new AddControlCallback(this.AddControl), new object[] { this, PageName });
                            this.Invoke(new AddControlCallback(this.AddControl), new object[] { this, ChkPageName });
                            //this.Controls.Add(PageName);

                            DownloadCountLocal++;
                            if (DownloadCountLocal % 9 == 0)
                            {
                                LocationX = 0;
                                LocationY += 30;
                            }
                        }
                        else
                            fileSizeLocal = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                DownloadCountLocal = 0;
            }
            finally
            {
                if(!PagesFileExists)
                    File.WriteAllLines("PagesOf"+DateTime.Now.Day+" - "+DateTime.Now.Month+" - "+DateTime.Now.Year+".Txt", (LocalDownloadCount).ToString().Split(".".ToCharArray()));
                thrDisplayPageNames.Abort();
            }
            
        }
        void AddControl(Control parent, Control ctrl)
        {
            parent.Controls.Add(ctrl);
        }
        Int64 fileSize;
        private void Download()
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
                        DownloadCount++;
                    }
                }
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            // Close the web response and the streams
            webResponse.Close();
            strResponse.Close();
            strLocal.Close();
            // Abort the thread that's downloading
            thrDownload.Abort();
            // Set the progress bar back to 0 and the label
            prgDownload.Value = 0;
            lblProgress.Text = "Download Stopped";
        }
        ArrayList arrswfFiles = new ArrayList();
        private void Form1_Load(object sender, EventArgs e)
        {
            thrDisplayPageNames = new Thread(DisplayPageNames);
            thrDisplayPageNames.Start();           
            
            //// Create a new thread that calls the DownloadInQueue() method            
            //thrSequencer = new Thread(DownloadInQueue);
            //// Start the thread, and thus call DownloadInQueue()
            //thrSequencer.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            DownloadCount = 0;
            fileSize = 0;
            //DownloadInQueue();
            // Create a new thread that calls the DownloadInQueue() method            
            thrSequencer = new Thread(DownloadInQueue);
            // Start the thread, and thus call DownloadInQueue()
            thrSequencer.Start();
            //button1.Enabled = false;
        }
        private void UpdateTextBox(string text)
        {
            this.textBox1.Text = textBox1.Text + " " + text;
        }
    }
}