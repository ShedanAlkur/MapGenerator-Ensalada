using System;

namespace Noise.Perlin
{
#if DEBUG
    /// <summary>
    ///     Класс для генерации шума Перлина в 2D,3D пространстве.
    /// </summary>
    public class OldPerlin
    {
        private readonly byte[] permutationTable;
        private readonly int seed;

        public OldPerlin(int seed = 0)
        {
            this.seed = seed;
            var rand = new Random(seed);
            permutationTable = new byte[1024]; // 1024
            rand.NextBytes(permutationTable); // заполняем случайными байтами
        }

        #region Noise functions

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy) для двумерного пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X</param>
        /// <param name="fy">Координата по оси Y</param>
        /// <returns>Значение шума в точке (fx, fy). Ограничено отрезком [-1; +1]]</returns>
        public float Noise(float fx, float fy)
        {
            // Координаты левой верхней вершины квадрата точки.
            var left = (int) Math.Floor(fx);
            var top = (int) Math.Floor(fy);

            // Локальные координаты точки внутри квадрата.
            var pointInQuadX = fx - left;
            var pointInQuadY = fy - top;

            // Извлекаем градиентные векторы для всех вершин квадрата.
            var topLeftGradient = GetPseudoRandomGradientVector(left, top);
            var topRightGradient = GetPseudoRandomGradientVector(left + 1, top);
            var bottomLeftGradient = GetPseudoRandomGradientVector(left, top + 1);
            var bottomRightGradient = GetPseudoRandomGradientVector(left + 1, top + 1);

            // Векторы от вершин квадрата до точки внутри квадрата.
            float[] distanceToTopLeft = {pointInQuadX, pointInQuadY};
            float[] distanceToTopRight = {pointInQuadX - 1, pointInQuadY};
            float[] distanceToBottomLeft = {pointInQuadX, pointInQuadY - 1};
            float[] distanceToBottomRight = {pointInQuadX - 1, pointInQuadY - 1};

            // Считаем скалярные произведения векторов между которыми будем интерполировать.
            /*
             tx1--tx2
              |    |
             bx1--bx2
            */
            var tx1 = Dot(distanceToTopLeft, topLeftGradient);
            var tx2 = Dot(distanceToTopRight, topRightGradient);
            var bx1 = Dot(distanceToBottomLeft, bottomLeftGradient);
            var bx2 = Dot(distanceToBottomRight, bottomRightGradient);

            // Готовим параметры интерполяции, чтобы она не была линейной.
            pointInQuadX = QunticCurve(pointInQuadX);
            pointInQuadY = QunticCurve(pointInQuadY);

            //Интерполируем.
            var tx = Lerp(pointInQuadX, tx1, tx2);
            var bx = Lerp(pointInQuadX, bx1, bx2);
            var tb = Lerp(pointInQuadY, tx, bx);

            // Возвращаем результат.
            return tb;
        }

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy) для двумерного пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X.</param>
        /// <param name="fy">Координата по оси Y.</param>
        /// <param name="octaves">Количество повторных наложений шума. Увеличивает детальность.</param>
        /// <param name="persistence">Устойчивость к шуму. Выше значение - сильнее шум.</param>
        /// <returns>Значение шума в точке (fx, fy). Ограничено отрезком [-1; +1].</returns>
        public float Noise(float fx, float fy, int octaves, float persistence = 0.5f)
        {
            float amplitude = 1;
            float max = 0;
            float result = 0;

            while (octaves-- > 0)
            {
                max += amplitude;
                result += Noise(fx, fy) * amplitude;
                amplitude *= persistence;
                fx *= 2;
                fy *= 2;
            }

            return result / max;
        }

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy,fz) для трехмерного пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X.</param>
        /// <param name="fy">Координата по оси Y.</param>
        /// <param name="fz">Координата по оси Z.</param>
        /// <returns>Значение шума в точке (fx, fy, fz). Ограничено отрезком [-1; +1].</returns>
        public float Noise(float fx, float fy, float fz)
        {
            // Работает аналогично с шумом для двумерного пространства. Просто достраиваются четыре узла сзади до куба.
            var left = (int) Math.Floor(fx);
            var top = (int) Math.Floor(fy);
            var zoom = (int) Math.Floor(fz);

            var pointInQuadX = fx - left;
            var pointInQuadY = fy - top;
            var pointInQuadZ = fz - zoom;


            var ForwardTopLeftGradient = GetPseudoRandomGradientVector(left, top, zoom);
            var ForwardTopRightGradient = GetPseudoRandomGradientVector(left + 1, top, zoom);
            var ForwardBottomLeftGradient = GetPseudoRandomGradientVector(left, top + 1, zoom);
            var ForwardBottomRightGradient = GetPseudoRandomGradientVector(left + 1, top + 1, zoom);
            var ZoomTopLeftGradient = GetPseudoRandomGradientVector(left, top, zoom + 1);
            var ZoomTopRightGradient = GetPseudoRandomGradientVector(left + 1, top, zoom + 1);
            var ZoomBottomLeftGradient = GetPseudoRandomGradientVector(left, top + 1, zoom + 1);
            var ZoomBottomRightGradient = GetPseudoRandomGradientVector(left + 1, top + 1, zoom + 1);

            float[] ForwardDistanceToTopLeft = {pointInQuadX, pointInQuadY, pointInQuadZ};
            float[] ForwardDistanceToTopRight = {pointInQuadX - 1, pointInQuadY, pointInQuadZ};
            float[] ForwardDistanceToBottomLeft = {pointInQuadX, pointInQuadY - 1, pointInQuadZ};
            float[] ForwardDistanceToBottomRight = {pointInQuadX - 1, pointInQuadY - 1, pointInQuadZ};
            float[] ZoomDistanceToTopLeft = {pointInQuadX, pointInQuadY, pointInQuadZ - 1};
            float[] ZoomDistanceToTopRight = {pointInQuadX - 1, pointInQuadY, pointInQuadZ - 1};
            float[] ZoomDistanceToBottomLeft = {pointInQuadX, pointInQuadY - 1, pointInQuadZ - 1};
            float[] ZoomDistanceToBottomRight = {pointInQuadX - 1, pointInQuadY - 1, pointInQuadZ - 1};

            /*
                 ftx1------ftx2
                  |  \       |  \
                  |  ztx1----|---ztx2
                  |   |      |   |
                 fbx1------fbx2  |
                     \|        \ |
                    zbx1---------zbx2
            */

            var ftx1 = Dot(ForwardDistanceToTopLeft, ForwardTopLeftGradient);
            var ftx2 = Dot(ForwardDistanceToTopRight, ForwardTopRightGradient);
            var fbx1 = Dot(ForwardDistanceToBottomLeft, ForwardBottomLeftGradient);
            var fbx2 = Dot(ForwardDistanceToBottomRight, ForwardBottomRightGradient);

            var ztx1 = Dot(ZoomDistanceToTopLeft, ZoomTopLeftGradient);
            var ztx2 = Dot(ZoomDistanceToTopRight, ZoomTopRightGradient);
            var zbx1 = Dot(ZoomDistanceToBottomLeft, ZoomBottomLeftGradient);
            var zbx2 = Dot(ZoomDistanceToBottomRight, ZoomBottomRightGradient);

            pointInQuadX = QunticCurve(pointInQuadX);
            pointInQuadY = QunticCurve(pointInQuadY);
            pointInQuadZ = QunticCurve(pointInQuadZ);

            var ftx = Lerp(pointInQuadX, ftx1, ftx2);
            var fbx = Lerp(pointInQuadX, fbx1, fbx2);
            var ftb = Lerp(pointInQuadY, ftx, fbx);

            var ztx = Lerp(pointInQuadX, ztx1, ztx2);
            var zbx = Lerp(pointInQuadX, zbx1, zbx2);
            var ztb = Lerp(pointInQuadY, ztx, zbx);

            var res = Lerp(pointInQuadZ, ftb, ztb);

            return res;
        }

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy,fz) для трехмерного пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X.</param>
        /// <param name="fy">Координата по оси Y.</param>
        /// <param name="fz">Координата по оси Z.</param>
        /// <param name="octaves">Количество повторных наложений шума. Увеличивает детальность.</param>
        /// <param name="persistence">Устойчивость к шуму. Выше значение - сильнее шум.</param>
        /// <returns>Значение шума в точке (fx, fy, fz). Ограничено отрезком [-1; +1].</returns>
        public float Noise(float fx, float fy, float fz, int octaves, float persistence = 0.5f)
        {
            float amplitude = 1;
            float max = 0;
            float result = 0;

            while (octaves-- > 0)
            {
                max += amplitude;
                result += Noise(fx, fy, fz) * amplitude;
                amplitude *= persistence;
                fx *= 2;
                fy *= 2;
                fz *= 2;
            }

            return result / max;
        }

        #endregion


        #region Other functions

        /// <summary>
        ///     Функция считает координаты единичного вектора в заданных координатах.
        /// </summary>
        /// <param name="x">Координата начала вектора по оси X.</param>
        /// <param name="y">Координата начала вектора по оси Y.</param>
        /// <returns>Координаты вектора.</returns>
        private float[] GetPseudoRandomGradientVector(int x, int y)
        {
            // хэш-функция с Простыми числами, обрезкой результата до размера массива со случайными байтами
            var v = (int) (((x * 1836311903) ^ (y * 2971215073) ^ (y * 1836311903 + 4807526976)) &
                           1023); // псевдо-случайное число от 0 до 3 которое всегда неизменно при данных x и y
            //var rand = new Random(seed * v);
            v = permutationTable[v] & 3;
            //return new[]
            //    {(float) Math.Cos(rand.NextDouble() * 2 * Math.PI), (float) Math.Sin(rand.NextDouble() * 2 * Math.PI)};
            switch (v)
            {
                case 0: return new float[] { 1, 0 };
                case 1: return new float[] { -1, 0 };
                case 2: return new float[] { 0, 1 };
                default: return new float[] { 0, -1 };
            }
        }

        /// <summary>
        ///     Функция возвращает координаты единичного вектора в заданных координатах.
        /// </summary>
        /// <param name="x">Координата начала вектора по оси X</param>
        /// <param name="y">Координата начала вектора по оси Y</param>
        /// <param name="z">Координата начала вектора по оси Z</param>
        /// <returns>Координаты вектора.</returns>
        private float[] GetPseudoRandomGradientVector(int x, int y, int z)
        {
            // хэш-функция с Простыми числами, обрезкой результата до размера массива со случайными байтами
            var v = (int) (((x * 1836311903) ^ (y * 2971215073) ^ (z * 3735486054 + 4807526976)) &
                           1023); // псевдо-случайное число от 0 до 3 которое всегда неизменно при данных x и y
            var rand = new Random(seed * v);
            return new[]
            {
                (float) Math.Cos(rand.NextDouble() * 2 * Math.PI), (float) Math.Sin(rand.NextDouble() * 2 * Math.PI),
                (float) Math.Cos(rand.NextDouble() * 2 * Math.PI)
            };
        }

        /// <summary>
        ///     Функция считает скалярное произведени векторов.
        /// </summary>
        /// <param name="a">Координаты первого вектора.</param>
        /// <param name="b">Координаты второго вектора.</param>
        /// <returns>Скалярное произведение</returns>
        private static float Dot(float[] a, float[] b)
        {
            float res;
            if (a.Length == 2)
                return a[0] * b[0] + a[1] * b[1];
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        /// <summary>
        ///     Фунция считаем параметр интерполяции.
        /// </summary>
        /// <param name="t">Локальная координата точки для интерполяции.</param>
        /// <returns>Параметр интерполяции.</returns>
        private static float QunticCurve(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        /// <summary>
        ///     Функция выполняет интерполяцию.
        /// </summary>
        /// <param name="t">Локальная координата для интерполяции.</param>
        /// <param name="a">Левая граница интервала интерполяции.</param>
        /// <param name="b">Правая граница интервала интерполяции.</param>
        /// <returns>Результат интерполяции.</returns>
        private static float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        #endregion
    }
#endif
}