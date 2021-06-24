namespace MapGenerator
{
    partial class Form_grid
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.PanelForCanvas = new System.Windows.Forms.Panel();
            this.PanelCanvas = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lbl_coord = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_noise = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_height = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_baseTemp = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_modeTemp = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_save = new System.Windows.Forms.ToolStripStatusLabel();
            this.PanelForCanvas.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelForCanvas
            // 
            this.PanelForCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelForCanvas.AutoScroll = true;
            this.PanelForCanvas.Controls.Add(this.PanelCanvas);
            this.PanelForCanvas.Location = new System.Drawing.Point(12, 10);
            this.PanelForCanvas.Name = "PanelForCanvas";
            this.PanelForCanvas.Size = new System.Drawing.Size(719, 728);
            this.PanelForCanvas.TabIndex = 1;
            // 
            // PanelCanvas
            // 
            this.PanelCanvas.Location = new System.Drawing.Point(3, 3);
            this.PanelCanvas.Name = "PanelCanvas";
            this.PanelCanvas.Size = new System.Drawing.Size(712, 712);
            this.PanelCanvas.TabIndex = 1;
            this.PanelCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.CanvasPanel_Paint);
            this.PanelCanvas.MouseLeave += new System.EventHandler(this.panelCanvas_MouseLeave);
            this.PanelCanvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelCanvas_MouseMove);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbl_save,
            this.lbl_coord,
            this.lbl_noise,
            this.lbl_height,
            this.lbl_baseTemp,
            this.lbl_modeTemp});
            this.statusStrip1.Location = new System.Drawing.Point(0, 739);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(743, 24);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lbl_coord
            // 
            this.lbl_coord.Name = "lbl_coord";
            this.lbl_coord.Size = new System.Drawing.Size(32, 19);
            this.lbl_coord.Text = "label";
            // 
            // lbl_noise
            // 
            this.lbl_noise.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lbl_noise.Name = "lbl_noise";
            this.lbl_noise.Size = new System.Drawing.Size(35, 19);
            this.lbl_noise.Text = "noise";
            // 
            // lbl_height
            // 
            this.lbl_height.Name = "lbl_height";
            this.lbl_height.Size = new System.Drawing.Size(41, 19);
            this.lbl_height.Text = "height";
            // 
            // lbl_baseTemp
            // 
            this.lbl_baseTemp.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lbl_baseTemp.Name = "lbl_baseTemp";
            this.lbl_baseTemp.Size = new System.Drawing.Size(62, 19);
            this.lbl_baseTemp.Text = "baseTemp";
            // 
            // lbl_modeTemp
            // 
            this.lbl_modeTemp.Name = "lbl_modeTemp";
            this.lbl_modeTemp.Size = new System.Drawing.Size(69, 19);
            this.lbl_modeTemp.Text = "modeTemp";
            // 
            // lbl_save
            // 
            this.lbl_save.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lbl_save.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lbl_save.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.lbl_save.Name = "lbl_save";
            this.lbl_save.Size = new System.Drawing.Size(69, 19);
            this.lbl_save.Text = "Сохранить";
            this.lbl_save.Click += new System.EventHandler(this.lbl_save_Click);
            // 
            // Form_grid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 763);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.PanelForCanvas);
            this.Name = "Form_grid";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Окно визуализации";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_grid_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.PanelForCanvas.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel PanelForCanvas;
        private System.Windows.Forms.Panel PanelCanvas;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lbl_coord;
        private System.Windows.Forms.ToolStripStatusLabel lbl_noise;
        private System.Windows.Forms.ToolStripStatusLabel lbl_height;
        private System.Windows.Forms.ToolStripStatusLabel lbl_baseTemp;
        private System.Windows.Forms.ToolStripStatusLabel lbl_modeTemp;
        private System.Windows.Forms.ToolStripStatusLabel lbl_save;
    }
}

