using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HtmlParser;
using System.Xml;
using System.Net;
using System.IO;
using System.Collections;
namespace ePaperParser
{
    public partial class Form1 : Form
    {
        bool GotIt;
        string baseHtml = string.Empty;
        ArrayList arrswfFiles = new ArrayList();
        LinkLabel ll = null;
        public Form1()
        {
            InitializeComponent();
        }
        string LoadBaseHtml()
        {
            baseHtml = "<html xmlns='http://www.w3.org/1999/xhtml'>";
            baseHtml += "<head>";
            baseHtml += "<title>EPaper</title>";
            baseHtml += "</head>";
            baseHtml += "<body>";
            baseHtml += "<div id='container' style='padding: 0px; position: absolute; top: 0px;'>";
            baseHtml += "<object height='680' align='middle' width='100%' codebase='http://fpdownload.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=8,0,0,0' classid='clsid:d27cdb6e-ae6d-11cf-96b8-444553540000'>";
            baseHtml += "<param value='swfurlplaceholder' name='movie'/>";
            baseHtml += "</object>";
            baseHtml += "</div>";
            baseHtml += "</body>";
            baseHtml += "</html>";
            return baseHtml;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            GotIt = false;
            string twodigitday = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string twodigitmonth = DateTime.Now.Month.ToString().Length == 1 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string datestring = DateTime.Now.Year + "-" + twodigitmonth + "-" + twodigitday;

            WebRequest request = HttpWebRequest.Create("http://epaper.mathrubhumi.com/index.php?cat=14&date=" + datestring);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            HtmlParser.HtmlDocument document = HtmlParser.HtmlDocument.Create(stream);
            GetContainerDIV(document.Nodes);
            //textBox1.Text = document.Html;

        }
        Point lastClick; //Holds where the Form was clicked

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lastClick = new Point(e.X, e.Y); //We'll need this for when the Form starts to move
            this.Cursor = Cursors.SizeAll;
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        //private void Form1_Paint(object sender, PaintEventArgs e)
        //{
        //    //Draws a border to make the Form stand out
        //    //Just done for appearance, not necessary

        //    Pen p = new Pen(Color.Gray, 3);
        //    e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
        //    p.Dispose();
        //}
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //Point newLocation = new Point(e.X - lastE.X, e.Y - lastE.Y);
            if (e.Button == MouseButtons.Left) //Only when mouse is clicked
            {
                //Move the Form the same difference the mouse cursor moved;
                this.Left += e.X - lastClick.X;
                this.Top += e.Y - lastClick.Y;
            }
        }
        void GetContainerDIV(HtmlNodeCollection nodes)
        {

            foreach (HtmlNode node in nodes)
            {

                HtmlParser.HtmlElement element = node as HtmlParser.HtmlElement;
                if ((null != element) && (((element.Name.ToLower() == "html")) || (element.Name.ToLower() == "body") || (((element.Name.ToLower() == "div")) && (element.Attributes["id"] != null) && (element.Attributes["id"].Value == "div1"))))// || (element.Name.ToLower() == "td") && (element.Text.Trim() != "")))// && element.Attributes.Contains("href"))
                {
                    GetContainerDIV(element.Nodes);
                    if (GotIt == true)
                        break;
                }
                //if ((null != element) && ((element.Name.ToLower() == "div")) && (element.Attributes["id"]!=null) && (element.Attributes["id"].Value == "container"))
                if ((null != element) && ((element.Name.ToLower() == "div")) && (element.Html.Contains("index.php?id=")))
                {
                    //textBox1.Text = element.Html;
                    CreateTheURLs(element.Nodes);
                    GotIt = true;
                    break;
                }
            }
        }
        string divs = string.Empty;
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
                    swfFileID = element.Attributes["href"].Value.Substring(13, element.Attributes["href"].Value.IndexOf("&")-13);
                    string twodigitday = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
                    url = "http://media.mathrubhumi.com/flashpaper/" + DateTime.Now.Year + "/" + DateTime.Now.ToString("MMM") + "/" + twodigitday + "/" + DateTime.Now.Year + "-" + DateTime.Now.ToString("MMM") + "-" + twodigitday + "_" + element.Text + "_Dai_" + swfFileID + ".swf";
                    arrswfFiles.Add(url);
                    divs += url + "\r\n";
                }
            }
            textBox1.Text = divs;
            Label l = new Label();
            l.Location = new Point(5, 100);
            l.Text = "Pages:";
            l.Width = 45;
            this.Controls.Add(l);
            GenerateLinkLabels();
        }

        void GenerateLinkLabels()
        {
            //use the urls from arrswfFiles to generate lls
            //also add event handler to invokde clicked events
            int locationY = 50;
            int PageNo = 0;
            for (int i = 0; i < arrswfFiles.Count; i++)
            {
                PageNo = i + 1;
                ll = new LinkLabel();
                if (PageNo > 9)
                    ll.Width = 25;
                else
                    ll.Width = 20;
                ll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
                ll.Text = (PageNo).ToString();// "Page" + (i + 1);
                ll.Name = (PageNo).ToString();
                ll.Location = new Point(locationY, 100);
                //ll.BorderStyle = BorderStyle.FixedSingle;
                this.Controls.Add(ll);
                if (PageNo > 9)
                    locationY += 25;
                else
                    locationY += 20;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel objLinkLabel = (LinkLabel)sender;
            string twodigitday = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Mathrubhoomi"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\Mathrubhoomi");
            }
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Mathrubhoomi" + @"\Mathrubhoomi" + DateTime.Now.Year + "-" + DateTime.Now.ToString("MMM") + "-" + twodigitday + objLinkLabel.Name + ".htm"))
            {
                // create a writer and open the file
                TextWriter tw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Mathrubhoomi" + @"\Mathrubhoomi" + DateTime.Now.Year + "-" + DateTime.Now.ToString("MMM") + "-" + twodigitday + objLinkLabel.Name + ".htm");

                // write a line of text to the file
                tw.WriteLine(LoadBaseHtml().Replace("swfurlplaceholder", arrswfFiles[Convert.ToInt16(objLinkLabel.Name) - 1].ToString()));

                // close the stream
                tw.Close();
            }
            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\Mathrubhoomi" + @"\Mathrubhoomi" + DateTime.Now.Year + "-" + DateTime.Now.ToString("MMM") + "-" + twodigitday + objLinkLabel.Name + ".htm");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}