using System;
using System.Drawing;
using System.Windows.Forms;

namespace MapGenerator
{
    public partial class Form_grid : Form
    {
        private readonly Pen pen = new Pen(Color.Black, 1);
        private Bitmap bmp;
        private Graphics canvas, bmpG;
        private int cellSize, mapSize;
        private Form_contol form_contol;
        private Form_info form_info;
        private RichTextBox text;

        public Form_grid()
        {
            InitializeComponent();
            cellSize = 7;
            mapSize = 100;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            form_contol = Owner as Form_contol;
            var random = new Random();
            canvas = PanelCanvas.CreateGraphics();
            SetCanvasSize(7, 100);
            //DrawGrid();
            timer1.Interval = 5000;
            timer1.Start();
        }

        public void DrawGrid()
        {
            bmpG.Clear(Color.Pink);
            for (var x = 0; x < 100; x++)
            {
                bmpG.DrawLine(pen, 0, x * cellSize, cellSize * mapSize, x * cellSize); // --
                bmpG.DrawLine(pen, x * cellSize, 0, x * cellSize, cellSize * mapSize); // |
            }

            bmpG.DrawLine(pen, 0, cellSize * mapSize, cellSize * mapSize, cellSize * mapSize); // --
            bmpG.DrawLine(pen, cellSize * mapSize, 0, cellSize * mapSize, cellSize * mapSize); // |
        }

        public void DrawCell(int x, int y, Color color)
        {
            bmpG.FillRectangle(new SolidBrush(color),
                x * cellSize, y * cellSize, cellSize, cellSize);
        }

        public void Redraw()
        {
            if (bmp != null) canvas.DrawImage(bmp, 0, 0);
        }

        public void SetCanvasSize(int cellSize, int mapSize)
        {
            this.cellSize = cellSize;
            this.mapSize = mapSize;
            PanelCanvas.Size = new Size(cellSize * mapSize + 1, cellSize * mapSize + 1);
            bmp = new Bitmap(PanelCanvas.Width, PanelCanvas.Height);
            canvas = PanelCanvas.CreateGraphics();
            bmpG = Graphics.FromImage(bmp);
            bmpG.Clear(Color.Pink);
            //DrawGrid();
        }

        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            Redraw();
        }

        #region Обработчики событий

        private void Form_grid_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            text = form_contol.form_info.richTextBox1;
            timer1.Stop();
        }

        private void panel2_MouseLeave(object sender, EventArgs e)
        {
            status_label1.Text = "label";
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            //int x = e.X / cellSize;
            //int y = e.Y / cellSize;
            //text.Clear();
            //try
            //{
            //    text.Text = $"Клетка #{x + y * 100}\n";
            //    text.Text += $"x = {x}; y = {y}\n";
            //    if (gen.NoiseMap != null) text.Text += $"Шум = {gen.NoiseMap[x, y]}\n";
            //    if (gen.HeightMap != null) text.Text += $"Высота = {gen.HeightMap[x, y]}\n";
            //    if (gen.RainFallMap != null) text.Text += $"Влажность = {gen.RainFallMap[x, y]}\n";
            //    if (gen.BaseTemperatureMap != null) text.Text += $"Базовая температура = {gen.BaseTemperatureMap[x, y]}\n";
            //    if (gen.TemperatureMap != null) text.Text += $"Температура = {gen.TemperatureMap[x, y]}\n";
            //    text.Text += $"Биом ...\n";
            //    text.Text += $"Тектоническая плита ...\n";
            //    text.Text += $"Ресурсы ...\n";
            //}
            //catch { }
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            var x = e.X / cellSize;
            if (x >= mapSize) x = mapSize - 1;
            var y = e.Y / cellSize;
            if (y >= mapSize) y = mapSize - 1;
            status_label1.Text = $"x: {x}, y: {y}";
            var index = x + y * mapSize;
            if (form_contol.NoiseMapIsReady) status_label1.Text += $", Шум: {form_contol.NoiseMap[index]}";
            if (form_contol.HeightMapIsReady) status_label1.Text += $", Высота: {form_contol.HeightMap[index]}";
            if (form_contol.BaseTemperatureMapIsReady)
                status_label1.Text += $", Базовая температура: {form_contol.BaseTemperatureMap[index] - 273} C";
            if (form_contol.TemperatureMapIsReady)
                status_label1.Text += $", Температура: {form_contol.TemperatureMap[index] - 273} C";
            if (form_contol.RainFallMapIsReady) status_label1.Text += $", Осадки: {form_contol.RainFallMap[index]}";
            // i: {e.X / size + e.Y / size * 100}
        }

        #endregion
    }
}