using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxSPPIDRADViewCtl;
using System.Runtime.CompilerServices;

namespace RadViewUC
{
    public partial class RadViewUserControl: UserControl
    {
        public AxRADViewCtl412 RadOCX;

        //// 1: Delegates
        //public delegate void RadFitEventHandler(object source, EventArgs args);
        //// 2: Events
        //public event RadFitEventHandler RadFitView;


        public RadViewUserControl()
        {
            InitializeComponent();

            // Assign local server
            this.axRADViewCtl4121.SetLocalServer("radsrv.dll");
            this.RadOCX = this.axRADViewCtl4121;
            this.RadOCX.MouseScroll += RadOCX_MouseScroll;
            this.RadOCX.DblClick += RadOCX_DblClick;
            this.SizeChanged += RadViewUserControl_SizeChanged;

            this.RadOCX.FileName = @"D:\SPPIDORCL\Plant\00\01\T-00-01-001.pid";
            //// 3: Raise Events
            //OnRadFitView();
        }

        private void RadViewUserControl_SizeChanged(object sender, EventArgs e)
        {
            this.RadOCX.Fit();
        }

        private void RadOCX_DblClick(object sender, EventArgs e)
        {
            this.RadOCX.Fit();
        }

        private void RadOCX_MouseScroll(object sender, _DRadViewerEvents_MouseScrollEvent e)
        {
            if (e.delta < 0)
            {
                this.RadOCX.Zoom(SPPIDRADViewCtl.CoordinateSpaceConstants.CoordinateSpacePixel, e.x, e.y, 0.95);
            }
            else
            {
                this.RadOCX.Zoom(SPPIDRADViewCtl.CoordinateSpaceConstants.CoordinateSpacePixel, e.x, e.y, 1.1);
            }
        }





        //// 2a: Define Events
        //protected virtual void OnRadFitView()
        //{
        //    if(RadFitView != null)
        //    {
        //        RadFitView(this, EventArgs.Empty);
        //    }

        //}
    }
}
