namespace RadViewUC
{
    partial class RadViewUserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RadViewUserControl));
            this.axRADViewCtl4121 = new AxSPPIDRADViewCtl.AxRADViewCtl412();
            ((System.ComponentModel.ISupportInitialize)(this.axRADViewCtl4121)).BeginInit();
            this.SuspendLayout();
            // 
            // axRADViewCtl4121
            // 
            this.axRADViewCtl4121.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axRADViewCtl4121.Enabled = true;
            this.axRADViewCtl4121.Location = new System.Drawing.Point(0, 0);
            this.axRADViewCtl4121.Margin = new System.Windows.Forms.Padding(10);
            this.axRADViewCtl4121.Name = "axRADViewCtl4121";
            this.axRADViewCtl4121.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axRADViewCtl4121.OcxState")));
            this.axRADViewCtl4121.Padding = new System.Windows.Forms.Padding(10);
            this.axRADViewCtl4121.Size = new System.Drawing.Size(800, 450);
            this.axRADViewCtl4121.TabIndex = 0;
            // 
            // RadViewUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axRADViewCtl4121);
            this.Name = "RadViewUserControl";
            this.Size = new System.Drawing.Size(800, 450);
            ((System.ComponentModel.ISupportInitialize)(this.axRADViewCtl4121)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxSPPIDRADViewCtl.AxRADViewCtl412 axRADViewCtl4121;
    }
}
