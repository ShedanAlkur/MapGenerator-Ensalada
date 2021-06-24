using System;
using System.Drawing;
using System.Windows.Forms;

namespace MapGenerator
{
    /// <summary>
    /// Форма для визуализации сгенерированных карт.
    /// </summary>
    public partial class Form_grid : Form
    {
        /// <summary>
        /// Диалог для сохранения сгенерированного изображения в файл.
        /// </summary>
        private SaveFileDialog saveFileDialog = new SaveFileDialog();

        /// <summary>
        /// bmp визуализируемого изображения.
        /// </summary>
        private Bitmap bmp;

        /// <summary>
        /// Экземпляр <c>Graphics</c> для изменения изображения на полотне.
        /// </summary>
        private Graphics canvas;

        /// <summary>
        /// Экземпляр <c>Graphics</c> для изображения к визуализации на полотне.
        /// </summary>
        private Graphics bmpG;

        /// <summary>
        /// Размер каждой клетки <c>cellSize</c>*<c>cellSize</c> в пикселях.
        /// </summary>
        private int cellSize;

        /// <summary>
        /// Размер полотна <c>mapSize</c>*<c>mapSize</c> в клетках <c>cellSize</c>.
        /// </summary>
        private int mapSize;

        /// <summary>
        /// Форма, вызвавшая экземпляр <c>Form_grid</c>.
        /// </summary>
        private Form_contol form_contol;

        public Form_grid()
        {
            InitializeComponent();
            cellSize = 7;
            mapSize = 100;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            form_contol = Owner as Form_contol;
            canvas = PanelCanvas.CreateGraphics();
            SetCanvasSize(cellSize, mapSize);

            saveFileDialog.DefaultExt = "*.bmp";
            saveFileDialog.FileName = "New image";
            saveFileDialog.AddExtension = false;
            saveFileDialog.Filter = "(*.bmp)|*.bmp|(*.*)|*.*";
        }

        /// <summary>
        /// Метод закрашивает клетку на полотне неоходимым цветом.
        /// </summary>
        /// <param name="x">Номер клетки вдоль оси oX.</param>
        /// <param name="y">Номер клетки вдоль оси oY.</param>
        /// <param name="color">Цвет клетки.</param>
        public void DrawCell(int x, int y, Color color)
        {
            bmpG.FillRectangle(new SolidBrush(color),
                x * cellSize, y * cellSize, cellSize, cellSize);
        }

        public void Redraw()
        {
            if (bmp != null) canvas.DrawImage(bmp, 0, 0);
        }

        /// <summary>
        /// Метод устанавливает размер полотна для визуализации.
        /// </summary>
        /// <param name="cellSize">Размер каждой клетки <c>cellSize</c>*<c>cellSize</c> в пикселях.</param>
        /// <param name="mapSize">Размер полотна <c>mapSize</c>*<c>mapSize</c> в клетках <c>cellSize</c>.</param>
        public void SetCanvasSize(int cellSize, int mapSize)
        {
            this.cellSize = cellSize;
            this.mapSize = mapSize;
            PanelCanvas.Size = new Size(cellSize * mapSize + 1, cellSize * mapSize + 1);
            bmp = new Bitmap(PanelCanvas.Width, PanelCanvas.Height);
            canvas = PanelCanvas.CreateGraphics();
            bmpG = Graphics.FromImage(bmp);
            bmpG.Clear(Color.Pink);
        }

        #region Обработчики событий

        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            Redraw();
        }

        private void Form_grid_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void panelCanvas_MouseLeave(object sender, EventArgs e)
        {
            lbl_coord.Text = "label";
        }

        private void lbl_save_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel) return;
            bmp.Save(saveFileDialog.FileName);
        }

        private void panelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var x = e.X / cellSize;
            if (x >= mapSize) x = mapSize - 1;
            var y = e.Y / cellSize;
            if (y >= mapSize) y = mapSize - 1;
            lbl_coord.Text = $"x: {x}, y: {y}";
            var index = x + y * mapSize;
            if (form_contol.NoiseMapIsReady) lbl_noise.Text = $", Шум: {form_contol.NoiseMap[index]}";
            if (form_contol.HeightMapIsReady) lbl_height.Text = $", Высота: {form_contol.HeightMap[index]}";
            if (form_contol.BaseTemperatureMapIsReady)
                lbl_baseTemp.Text = $", Базовая температура: {form_contol.BaseTemperatureMap[index] - 273} C";
            if (form_contol.ModeTemperatureMapIsReady)
                lbl_modeTemp.Text = $", Температура: {form_contol.ModeTemperatureMap[index] - 273} C";
        }

        #endregion
    }
}