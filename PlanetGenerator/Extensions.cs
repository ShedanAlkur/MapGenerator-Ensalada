using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MapGenerator
{
    public static class Extensions
    {
        /// <summary>Смешивает указанные цвета.</summary>
        /// <param name="color">Цвет для смешивания с цветом фона.</param>
        /// <param name="backColor">Цвет для смешивания с цветом переда.</param>
        /// <param name="amount">Сколько <paramref name="color"/> использовать поверх <paramref name="backColor"/>.</param>
        /// <returns>Смешанный цвет.</returns>
        public static Color Blend(this Color color, Color backColor, double amount)
        {
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return Color.FromArgb(r, g, b);
        }
    }
}
