using System;

namespace Noise.Perlin
{
    /// <summary>
    ///     Класс для генерации шума Перлина в 2D,3D пространстве.
    /// </summary>
    public class PerlinExp2
    {
        int magical1 = 256639923;
        int magical2 = 807526976;
        int magical3 = 836311903;
        int magical4 = 735486054;
        int magical5 = 971215073;

        private static int left, top, zoom, time, v;
        static int c1, c2, c3, c4, c5;

        private static double ftx1,
            ftx2,
            fbx1,
            fbx2,
            ztx1,
            ztx2,
            zbx1,
            zbx2;

        private static double tftx1,
            tftx2,
            tfbx1,
            tfbx2,
            tztx1,
            tztx2,
            tzbx1,
            tzbx2;

        private static double pointInQuadX, pointInQuadY, pointInQuadZ, pointInQuadT;
        private static double VectorX, VectorY, VectorZ, VectorT;
        private static double res;
        private readonly double[] permutationTable;
        private readonly int seed;
        private static double[] vector;

        public PerlinExp2(int seed = 0)
        {
            this.seed = seed;
            var rand = new Random(seed);
            permutationTable = new double[1024];
            for (var i = 0; i < 1024; i++)
                permutationTable[i] = rand.NextDouble() * 2 - 1;
            vector = new double[2];
        }


        #region Методы интерполяции

        /// <summary>
        ///     Функция выполняет интерполяцию.
        /// </summary>
        /// <param name="x">Локальная координата для интерполяции.</param>
        /// <param name="a">Левая граница интервала интерполяции.</param>
        /// <param name="b">Правая граница интервала интерполяции.</param>
        /// <returns>Результат интерполяции.</returns>
        public static double Lerp(double x, double a, double b)
        {
            return a + x * (b - a);
        }

        public static double CosineInterpolation(double x, double a, double b)
        {
            x = (1 - Math.Cos(x * Math.PI)) * 0.5;
            return a * (1 - x) + b * x;
        }

        public static double CubicInterpolation(double v0, double v1, double v2, double v3, double x)
        {
            double P = (v3 - v2) - (v0 - v1);
            double Q = (v0 - v1) - P;
            double R = v2 - v0;
            double S = v1;

            return P * x * 3 + Q * x * 2 + R * x + S;
        }

        #endregion

        #region Методы подготовки параметров интерполяции (функции единичных кривых)
        public static double SmoothStep(double t)
        {
            // x^2*(3-2x)
            return t * t * (3 - 2 * t);
        }


        public static double QunticCurve(double t)
        {
            // SmootherStep
            // 6x^5 - 15x^4 + 10x^3
            // x^3 * ( x * ( x*6 - 16 ) + 10)
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        #endregion

        public void Normalize2DVector(ref double[] vector)
        {
            double lenght = Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1]);
            vector[0] = vector[0] / lenght;
            vector[1] = vector[1] / lenght;
        }

        #region Noise functions

        #region 2D

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy) для двумерного пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X</param>
        /// <param name="fy">Координата по оси Y</param>
        /// <returns>Значение шума в точке (fx, fy). Ограничено отрезком [-1; +1]]</returns>
        public double Noise(double fx, double fy)
        {
            /*
                tx1--tx2
                |    |
                bx1--bx2
            */

            // Координаты левой верхней вершины квадрата, в между узлов которого списана точка.
            if (fx < 0 && fx % -1 != 0) left = (int)(fx - 1);
            else left = (int)fx;
            if (fy < 0 && fy % -1 != 0) top = (int)(fy - 1);
            else top = (int)fy;

            // Локальные координаты точки внутри квадрата.
            pointInQuadX = fx - left;
            pointInQuadY = fy - top;


            c1 = seed * magical1 + magical2;
            c2 = left * magical3;
            c5 = top * magical5;

            // Извлекаем градиентные векторы для всех вершин квадрата.
            // Векторы от вершин квадрата до точки внутри квадрата.
            // Считаем скалярные произведения векторов между которыми будем интерполировать.
            v = (int)(c2 ^ c5 ^ c1);
            vector[0] = permutationTable[v & 1023];
            vector[1] = permutationTable[(v * 2) & 1023];
            Normalize2DVector(ref vector);
              ftx1 = pointInQuadX * vector[0] + pointInQuadY * vector[1];

            v = (int)(((left + 1) * magical3) ^ c5 ^ c1);
            vector[0] = permutationTable[v & 1023];
            vector[1] = permutationTable[(v * 2) & 1023];
            Normalize2DVector(ref vector);
            ftx2 = (pointInQuadX - 1) * vector[0] + pointInQuadY * vector[1];

            v = (int)(c2 ^ ((top + 1) * magical5) ^ c1);
            vector[0] = permutationTable[v & 1023];
            vector[1] = permutationTable[(v * 2) & 1023];
            Normalize2DVector(ref vector);
            fbx1 = pointInQuadX * vector[0] + (pointInQuadY - 1) * vector[1];

            v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c1);
            vector[0] = permutationTable[v & 1023];
            vector[1] = permutationTable[(v * 2) & 1023];
            Normalize2DVector(ref vector);
            fbx2 = (pointInQuadX - 1) * vector[0] + (pointInQuadY - 1) * vector[1];

            // Готовим параметры интерполяции, чтобы она не была линейной.
            //QunticCurve = t * t * t * (t * (t * 6 - 15) + 10)
            pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
            pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);

            res = ftx1 + pointInQuadX * (ftx2 - ftx1) + pointInQuadY *
                (fbx1 + pointInQuadX * (fbx2 - fbx1)
                - (ftx1 + pointInQuadX * (ftx2 - ftx1)));

            // Возвращаем результат.
            return res;
        }

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy,fz) для двумерного пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X.</param>
        /// <param name="fy">Координата по оси Y.</param>
        /// <param name="octaves">Количество повторных наложений шума. Увеличивает детальность.</param>
        /// <param name="persistence">Устойчивость к шуму. Выше значение - сильнее шум.</param>
        /// <returns>Значение шума в точке (fx, fy, fz). Ограничено отрезком [-1; +1].</returns>
        public double Noise(double fx, double fy, int octaves, double persistence = 0.5f)
        {
            double amplitude = 1;
            double max = 0;
            double result = 0;

            while (--octaves > -1)
            {
                max += amplitude;
                result += Noise(fx, fy) * amplitude;
                amplitude *= persistence;
                fx /= persistence;
                fy /= persistence;
            }

            return result;// / max;
        }


        #endregion

        #endregion
    }
}