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
            this.components = new System.ComponentModel.Container();
            this.PanelForCanvas = new System.Windows.Forms.Panel();
            this.PanelCanvas = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.status_label1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
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
            this.PanelCanvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseDown);
            this.PanelCanvas.MouseLeave += new System.EventHandler(this.panel2_MouseLeave);
            this.PanelCanvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseMove);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_label1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 741);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(743, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // status_label1
            // 
            this.status_label1.Name = "status_label1";
            this.status_label1.Size = new System.Drawing.Size(32, 17);
            this.status_label1.Text = "label";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
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
        private System.Windows.Forms.ToolStripStatusLabel status_label1;
        private System.Windows.Forms.Timer timer1;
    }
}

