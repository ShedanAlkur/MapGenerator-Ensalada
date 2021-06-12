using Noise.Perlin;
using System;
using System.Windows.Forms;

namespace MapGenerator
{
    internal static class Program
    {
        /// <summary>
        ///     Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            PerlinExp3 perlin = new PerlinExp3(0, 3, 1, 0.5);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form_contol());
        }
    }
}