using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DownloadManager
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        

        private bool _bFullScreenMode = false;

        private void Form2_Load(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Maximized;
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.axAcroPDF1.TabIndex = -1;
        }
        private void Form1_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {            
            if (e.KeyData == Keys.F11)
            {
                if (_bFullScreenMode == false)
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.FormBorderStyle = FormBorderStyle.None;
                    _bFullScreenMode = true;
                }
                else
                {                    
                    this.Left = (Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2);
                    this.Top = (Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2);
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    _bFullScreenMode = false;
                }
            }
        }
    }
}