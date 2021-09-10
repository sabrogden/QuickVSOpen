namespace QuickVSOpen
{
    partial class QuickVSOpenDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mStatuStrip = new System.Windows.Forms.StatusStrip();
            this.mStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_refreshStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mInputText = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.mStatuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mStatuStrip
            // 
            this.mStatuStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mStatuStrip.AutoSize = false;
            this.mStatuStrip.BackColor = System.Drawing.Color.White;
            this.mStatuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.mStatuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.mStatuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mStatusLabel,
            this.m_refreshStatusLabel});
            this.mStatuStrip.Location = new System.Drawing.Point(0, 462);
            this.mStatuStrip.Name = "mStatuStrip";
            this.mStatuStrip.Size = new System.Drawing.Size(573, 22);
            this.mStatuStrip.SizingGrip = false;
            this.mStatuStrip.TabIndex = 0;
            this.mStatuStrip.Text = "statusStrip1";
            // 
            // mStatusLabel
            // 
            this.mStatusLabel.AutoSize = false;
            this.mStatusLabel.Name = "mStatusLabel";
            this.mStatusLabel.Size = new System.Drawing.Size(60, 17);
            this.mStatusLabel.Text = "Hello";
            this.mStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_refreshStatusLabel
            // 
            this.m_refreshStatusLabel.AutoSize = false;
            this.m_refreshStatusLabel.Name = "m_refreshStatusLabel";
            this.m_refreshStatusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.m_refreshStatusLabel.Size = new System.Drawing.Size(275, 17);
            this.m_refreshStatusLabel.Text = "Refresh Status";
            // 
            // mInputText
            // 
            this.mInputText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mInputText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mInputText.Location = new System.Drawing.Point(0, 0);
            this.mInputText.Name = "mInputText";
            this.mInputText.Size = new System.Drawing.Size(599, 20);
            this.mInputText.TabIndex = 0;
            this.mInputText.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.mInputText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mInputText_KeyDown);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 21);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(599, 441);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.VirtualMode = true;
            this.listView1.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listView1_RetrieveVirtualItem);
            this.listView1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listView1_KeyPress);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            // 
            // QuickVSOpenDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(599, 481);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.mInputText);
            this.Controls.Add(this.mStatuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.Name = "QuickVSOpenDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "QuickVSOpen";
            this.Shown += new System.EventHandler(this.QuickVSOpenDialog_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.QuickVSOpenDialog_KeyDown);
            this.Resize += new System.EventHandler(this.QuickVSOpenDialog_Resize);
            this.mStatuStrip.ResumeLayout(false);
            this.mStatuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip mStatuStrip;
        private System.Windows.Forms.TextBox mInputText;
        private System.Windows.Forms.ToolStripStatusLabel mStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel m_refreshStatusLabel;
        private System.Windows.Forms.ListView listView1;
    }
}
