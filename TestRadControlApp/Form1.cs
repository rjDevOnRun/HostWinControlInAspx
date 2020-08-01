using AxSPPIDRADViewCtl;
using SPPIDRADViewCtl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestRadControlApp
{
    public partial class Form1 : Form
    {
        public AxRADViewCtl412 radView;

        public Form1()
        {
            InitializeComponent();

            if (this.radViewUserControl1 == null ||
                this.radViewUserControl1.RadOCX == null)
            {
                MessageBox.Show("Rad OCX Error, exiting now!");
                this.Close();
            }

            // Assign variable
            this.radView = this.radViewUserControl1.RadOCX;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetupdRadView();
        }

        private void SetupdRadView()
        {
            if (string.IsNullOrEmpty(this.txtFileName.Text))
            {
                radView.Visible = false;
                return;
            }

            if(this.radViewUserControl1 != null &&
                this.radViewUserControl1.RadOCX != null)
            {
                try
                {
                    if(radView.Visible == false) radView.Visible = true;

                    radView.FileName = this.txtFileName.Text;
                    //radView.FileName = @"D:\SPPIDORCL\Plant\00\01\T-00-01-001.pid";
                    //radView.FileName = @"D:\SPPIDORCL\Plant\10\101\T-101-0001.pid";
                    radView.Display = true;
                    radView.Fit();

                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    radView.Visible = false;
                    this.radView.SetLocalServer("radsrv.dll");
                }

            }
        }
    }
}
