using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DownloadManager
{
    public partial class Form2 : Form1
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            axAcroPDF1.Width = this.Width;
            axAcroPDF1.Height = this.Height;
            axAcroPDF1.Top = 0;
            axAcroPDF1.Left = 0;
            axAcroPDF1.Visible = false;
            panel1.Visible = false;
            //axAcroPDF1.src = @"E:\Projects\epaperreader\MAT\bin\Release\Mathrubhoomi\2010-Oct-16_01_Dai_16558.pdf";            
            
        }
        private void panel1_MouseHover(object sender, EventArgs e)
        {
            //MessageBox.Show(this.panel1.Height.ToString() + " " +this.panel1.Width.ToString());
            this.panel1.Width = 75;
        }
        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            this.panel1.Width = 15;
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}