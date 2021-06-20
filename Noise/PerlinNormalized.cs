using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Noise.Perlin
{
    public class PerlinNormalized
    {
        #region Поля и свойства

        /// <summary>
        /// Наименьшие координаты вершины куба, в который вписана точка генерации шума.
        /// </summary>
        private int left, top, zoom, time;

        /// <summary>
        /// Значение для генерации псевдослучайных градиентных векторов.
        /// </summary>
        private const int magical1 = 256639923,
        magical2 = 807526976,
        magical3 = 836311903,
        magical4 = 735486054,
        magical5 = 971215073,
        magical6 = 623650874;

        /// <summary>
        /// Временная переменная для вычислений.
        /// </summary>
        private int c1, c2, c3, c4, c5, v;

        /// <summary>
        /// Результат скалярных произведений в вершине куба, в который вписана точка генерации шума.
        /// </summary>
        private double ftx1, ftx2,
            fbx1, fbx2,
            ztx1, ztx2,
            zbx1, zbx2;

        /// <summary>
        /// Результат скалярных произведений в вершине куба, в который вписана точка генерации шума.
        /// </summary>
        private double tftx1, tftx2,
            tfbx1, tfbx2,
            tztx1, tztx2,
            tzbx1, tzbx2;

        /// <summary>
        /// Переменная - счётчик.
        /// </summary>
        private int k;

        /// <summary>
        /// Амплитуда текущей октавы шума.
        /// </summary>
        private double amplitude;

        /// <summary>
        /// Расстояние от точки генераии шума до вершин куба, в который она вписана.
        /// </summary>
        private double pointInQuadX, pointInQuadY, pointInQuadZ, pointInQuadT;

        /// <summary>
        /// Результат вычислений.
        /// </summary>
        private double result;

        /// <summary>
        /// Семя генерации шума.
        /// </summary>
        public int Seed;

        /// <summary>
        /// Массив псевдослучайных нормализованных градиентных векторов.
        /// </summary>
        private double[][] permutationVectors;

        /// <summary>
        /// Размерность пространства, к работе с которым подготовлен permutationVectors
        /// </summary>
        int preparedDimension;

        /// <summary>
        /// Количество октав, которое рассчитывается для шума в каждой точке.
        /// </summary>
        private int octave;
        /// <summary>
        /// Количество октав, которое рассчитывается для шума в каждой точке.
        /// </summary>
        public int Octave
        {
            get => octave;
            set
            {
                octave = value;
                octaveFactor = 2 - Math.Pow(persistence, octave - 1);
            }
        }

        /// <summary>
        /// Устойчивость к наложению октав. Больше величина - сильнее влияние октав.
        /// </summary>
        private double persistence;
        /// <summary>
        /// Устойчивость к наложению октав. Больше величина - сильнее влияние октав.
        /// </summary>
        public double Persistence
        {
            get => persistence; set
            {
                persistence = value;
                octaveFactor = 2 - Math.Pow(persistence, octave - 1);
            }
        }

        /// <summary>
        /// Фактор, показывающий кратное увеличение максимального значения шума после наложения всех октав.
        /// </summary>
        private double octaveFactor;

        /// <summary>
        /// Максимальное по модулю значение двумерного шума.
        /// </summary>
        private static readonly double max2dNoiseValue = 0.5 * Math.Sqrt(2);

        /// <summary>
        /// Максимальное по модулю значение трехмерного шума.
        /// </summary>
        private static readonly double max3dNoiseValue = 0.5 * Math.Sqrt(3);

        /// <summary>
        /// Максимальное по модулю значение четырехмерного шума.
        /// </summary>
        private static readonly double max4dNoiseValue = 0.5 * Math.Sqrt(4);

        #endregion

        /// <summary>
        /// Инициализирует новый экземпляр класса <c>PerlinNormalized</c> с помощью указанных начальных значений.
        /// </summary>
        /// <param name="seed">Семя генерации шума.</param>
        /// <param name="octave">Количество октав, которое рассчитывается для шума в каждой точке.</param>
        /// <param name="persistence">Устойчивость к наложению октав. Больше величина - сильнее влияние октав.</param>
        public PerlinNormalized(int seed, int octave, double persistence = 0.5)
        {
            this.Seed = seed;
            this.Octave = octave;
            this.Persistence = persistence;

            var rnd = new Random(seed);
            permutationVectors = new double[1024][];
        }

        /// <summary>
        /// Метож нормализует вектор.
        /// </summary>
        /// <param name="vector">Нормализуемый вектор.</param>
        void NormalizeVector(ref double[] vector)
        {
            double vectorLenght = 0;
            for (int i = 0; i < vector.Length; i++)
                vectorLenght += vector[i] * vector[i];
            vectorLenght = Math.Sqrt(vectorLenght);
            for (int i = 0; i < vector.Length; i++)
                vector[i] = vector[i] / vectorLenght;
        }

        /// <summary>
        /// Метод подготавливает массив псевдослучайных векторов <c>permutationVectors</c> для работы с пространством заданной размерности.
        /// </summary>
        /// <param name="dimension">Размерность пространства генерации шума.</param>
        void PrepareDimension(int dimension)
        {
            var rnd = new Random(Seed);
            for (int i = 0; i < permutationVectors.Length; i++)
            {
                permutationVectors[i] = new double[dimension];
                for (int j = 0; j < permutationVectors[0].Length; j++)
                    permutationVectors[i][j] = rnd.NextDouble() * 2 - 1;
                NormalizeVector(ref permutationVectors[i]);
            }
            preparedDimension = dimension;
        }

        /// <summary>
        /// Функция линейной интерполяции.
        /// </summary>
        /// <param name="t">Координата интерполяции, лежащая в отрезке [0; 1].</param>
        /// <param name="a">Значение функции в координате {0}.</param>
        /// <param name="b">Значение функции в координате {1}.</param>
        /// <returns>Результат интерполяции. Лежит в отрезке [0; 1].</returns>
        static double Lerp(double t, double a, double b) => a + t * (b - a);

        /// <summary>
        /// Сигмовидная функция интерполяции третьей степени.
        /// </summary>
        /// <param name="t">Координата интерполяции, лежащая в отрезке [0; 1]</param>
        /// <returns>Результат интерполяции. Лежит в отрезке [0; 1].</returns>
        static double SmoothStep(double t) => t * t * (3 - 2 * t);

        /// <summary>
        /// Сигмовидная функция интерполяции шестой степени.
        /// </summary>
        /// <param name="t">Координата интерполяции, лежащая в отрезке [0; 1].</param>
        /// <returns>Результат интерполяции. Лежит в отрезке [0; 1].</returns>
        static double QunticCurve(double t) => t * t * t * (t * (t * 6 - 15) + 10);
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

        #region Функции генерации шума

        /// <summary>
        /// Функция генерации шума в одномерном пространстве.
        /// </summary>
        /// <param name="fx">Координата нахождения шума по оси oX.</param>
        /// <returns>Значение шума в заданных координатах.</returns>
        public double Noise(double fx)
        {
            result = 0;
            amplitude = 1;
            for (k = 0; k <= octave; k++)
            {
                // Координаты левой верхней вершины квадрата, в между узлов которого списана точка.
                if (fx < 0 && fx % -1 != 0) left = (int)(fx - 1);
                else left = (int)fx;

                // Локальные координаты точки внутри квадрата.
                pointInQuadX = fx - left;

                c1 = Seed * magical1;

                v = (int)(left * magical3);
                ftx1 = permutationVectors[v & 1023][0];

                v = (int)(((left + 1) * magical3));
                ftx2 = permutationVectors[v & 1023][0];
                // y = 2(a-b)x^4 - (3a-5b)x^3 - 3bx^2 + ax
                result += (2 * (ftx1 - ftx2) * pointInQuadX * pointInQuadX * pointInQuadX * pointInQuadX
                    - (3 * ftx1 - 5 * ftx2) * pointInQuadX * pointInQuadX * pointInQuadX
                    - 3 * ftx2 * pointInQuadX * pointInQuadX
                    + ftx1 * pointInQuadX)
                    * amplitude;

                amplitude *= persistence;
                fx /= persistence;
            }
            return result / octaveFactor;
        }

        /// <summary>
        /// Функция генерации шума в двумерном пространстве.
        /// </summary>
        /// <param name="fx">Координата нахождения шума по оси oX.</param>
        /// <param name="fy">Координата нахождения шума по оси oY</param>
        /// <returns>Значение шума в заданных координатах.</returns>
        public double Noise(double fx, double fy)
        {
            if (preparedDimension != 2) PrepareDimension(2);
            result = 0;
            amplitude = 1;
            for (k = 0; k < octave; k++)
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

                c1 = Seed * magical1 + magical2;
                c2 = left * magical3;
                c5 = top * magical5;

                // 1. Извлекаем градиентные векторы для всех вершин квадрата.
                // 2. Используем найденные векторы от вершин квадрата до точки внутри квадрата.
                // 3. Считаем скалярные произведения векторов между которыми будем интерполировать.
                v = (int)(left * magical3 ^ top * magical5 ^ c1);
                ftx1 = pointInQuadX * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1];

                v = (int)(((left + 1) * magical3) ^ top * magical5 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1];

                v = (int)(left * magical3 ^ ((top + 1) * magical5) ^ c1);
                fbx1 = pointInQuadX * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1];

                // Готовим параметры интерполяции, чтобы она не была линейной.
                //QunticCurve = t * t * t * (t * (t * 6 - 15) + 10)
                pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
                pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);

                // Интерполируем между (ftx2 и ftx1), затем (fbx2 и fbx1), затем между двумя результатами этих интерполяций.
                result += (ftx1 + pointInQuadX * (ftx2 - ftx1) + pointInQuadY *
                    (fbx1 + pointInQuadX * (fbx2 - fbx1)
                    - (ftx1 + pointInQuadX * (ftx2 - ftx1))))
                    * amplitude;
                amplitude *= persistence;
                fx /= persistence;
                fy /= persistence;
            }
            return result / octaveFactor / max2dNoiseValue;
        }

        /// <summary>
        /// Функция генерации шума в трехмерном пространстве.
        /// </summary>
        /// <param name="fx">Координата нахождения шума по оси oX.</param>
        /// <param name="fy">Координата нахождения шума по оси oY</param>
        /// <param name="fz">Координата нахождения шума по оси oZ</param>
        /// <returns>Значение шума в заданных координатах.</returns>
        public double Noise(double fx, double fy, double fz)
        {
            /*
                 ftx1------ftx2
                  |  \       |  \
                  |  ztx1----|---ztx2
                  |   |      |   |
                 fbx1------fbx2  |
                     \|        \ |
                    zbx1---------zbx2
            */

            if (preparedDimension != 3) PrepareDimension(3);
            result = 0;
            amplitude = 1;
            for (k = 0; k < octave; k++)
            {
                // Координаты левой верхней вершины квадрата точки.
                if (fx < 0 && fx % -1 != 0) left = (int)(fx - 1);
                else left = (int)fx;
                if (fy < 0 && fy % -1 != 0) top = (int)(fy - 1);
                else top = (int)fy;
                if (fz < 0 && fz % -1 != 0) zoom = (int)(fz - 1);
                else zoom = (int)fz;

                // Локальные координаты точки внутри квадрата.
                pointInQuadX = fx - left;
                pointInQuadY = fy - top;
                pointInQuadZ = fz - zoom;


                c1 = Seed * magical1 + magical2;
                c2 = left * magical3;
                c4 = zoom * magical4;
                c5 = top * magical5;

                v = (int)(c2 ^ c5 ^ c4 ^ c1);
                ftx1 = pointInQuadX * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                       pointInQuadZ * permutationVectors[v & 1023][2];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                       pointInQuadZ * permutationVectors[v & 1023][2];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ c1);
                fbx1 = pointInQuadX * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                       pointInQuadZ * permutationVectors[v & 1023][2];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] +
                       (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                       pointInQuadZ * permutationVectors[v & 1023][2];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ c1);
                ztx1 = pointInQuadX * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVectors[v & 1023][2];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ c1);
                ztx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVectors[v & 1023][2];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c1);
                zbx1 = pointInQuadX * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVectors[v & 1023][2];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c1);
                zbx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVectors[v & 1023][2];

                pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
                pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);
                pointInQuadZ = pointInQuadZ * pointInQuadZ * pointInQuadZ * (pointInQuadZ * (pointInQuadZ * 6 - 15) + 10);

                result += Lerp(pointInQuadZ,
                    Lerp(pointInQuadY, Lerp(pointInQuadX, ftx1, ftx2), Lerp(pointInQuadX, fbx1, fbx2)),
                    Lerp(pointInQuadY, Lerp(pointInQuadX, ztx1, ztx2), Lerp(pointInQuadX, zbx1, zbx2)))
                    * amplitude;

                amplitude *= persistence;
                fx /= persistence;
                fy /= persistence;
                fz /= persistence;
            }
            return result / octaveFactor / max3dNoiseValue;
        }

        /// <summary>
        /// Функция генерации шума в четырехмерном пространстве.
        /// </summary>
        /// <param name="fx">Координата нахождения шума по оси oX.</param>
        /// <param name="fy">Координата нахождения шума по оси oY</param>
        /// <param name="fz">Координата нахождения шума по оси oZ</param>
        /// <param name="ft">Координата нахождения шума по оси oT</param>
        /// <returns>Значение шума в заданных координатах.</returns>
        public double Noise(double fx, double fy, double fz, double ft)
        {
            result = 0;
            amplitude = 1;
            if (preparedDimension != 4) PrepareDimension(4);
            for (k = 0; k < octave; k++)
            {
                #region
                if (fx < 0 && fx % -1 != 0) left = (int)(fx - 1);
                else left = (int)fx;
                if (fy < 0 && fy % -1 != 0) top = (int)(fy - 1);
                else top = (int)fy;
                if (fz < 0 && fz % -1 != 0) zoom = (int)(fz - 1);
                else zoom = (int)fz;
                if (ft < 0 && ft % -1 != 0) time = (int)(ft - 1);
                else time = (int)ft;

                pointInQuadX = fx - left;
                pointInQuadY = fy - top;
                pointInQuadZ = fz - zoom;
                pointInQuadT = ft - time;

                c1 = Seed * magical1 + magical2;
                c2 = left * magical3;
                c3 = time * magical6;
                c4 = zoom * magical4;
                c5 = top * magical5;

                v = (int)(c2 ^ c5 ^ c4 ^ c3 ^ c1);
                ftx1 = pointInQuadX * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                       pointInQuadZ * permutationVectors[v & 1023][2] +pointInQuadT * permutationVectors[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ c3 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                       pointInQuadZ * permutationVectors[v & 1023][2] +
                       pointInQuadT * permutationVectors[v & 1023][3];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ c3 ^ c1);
                fbx1 = pointInQuadX * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                       pointInQuadZ * permutationVectors[v & 1023][2] +
                       pointInQuadT * permutationVectors[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ c3 ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] +
                       (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                       pointInQuadZ * permutationVectors[v & 1023][2] +
                       pointInQuadT * permutationVectors[v & 1023][3];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                ztx1 = pointInQuadX * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVectors[v & 1023][2] +
                       pointInQuadT * permutationVectors[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                ztx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVectors[v & 1023][2] +
                       pointInQuadT * permutationVectors[v & 1023][3];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                zbx1 = pointInQuadX * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVectors[v & 1023][2] +
                       pointInQuadT * permutationVectors[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                zbx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] +
                       (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVectors[v & 1023][2] +
                       pointInQuadT * permutationVectors[v & 1023][3];

                ///

                v = (int)(c2 ^ c5 ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tftx1 = pointInQuadX * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                        pointInQuadZ * permutationVectors[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVectors[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tftx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                        pointInQuadZ * permutationVectors[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVectors[v & 1023][3];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tfbx1 = pointInQuadX * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                        pointInQuadZ * permutationVectors[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVectors[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tfbx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] +
                        (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                        pointInQuadZ * permutationVectors[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVectors[v & 1023][3];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tztx1 = pointInQuadX * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                        (pointInQuadZ - 1) * permutationVectors[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVectors[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tztx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] + pointInQuadY * permutationVectors[v & 1023][1] +
                        (pointInQuadZ - 1) * permutationVectors[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVectors[v & 1023][3];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tzbx1 = pointInQuadX * permutationVectors[v & 1023][0] + (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                        (pointInQuadZ - 1) * permutationVectors[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVectors[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tzbx2 = (pointInQuadX - 1) * permutationVectors[v & 1023][0] +
                        (pointInQuadY - 1) * permutationVectors[v & 1023][1] +
                        (pointInQuadZ - 1) * permutationVectors[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVectors[v & 1023][3];

                pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
                pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);
                pointInQuadZ = pointInQuadZ * pointInQuadZ * pointInQuadZ * (pointInQuadZ * (pointInQuadZ * 6 - 15) + 10);
                pointInQuadT = pointInQuadT * pointInQuadT * pointInQuadT * (pointInQuadT * (pointInQuadT * 6 - 15) + 10);

                result += Lerp(pointInQuadT,
                    Lerp(pointInQuadZ,
                        Lerp(pointInQuadY, Lerp(pointInQuadX, ftx1, ftx2), Lerp(pointInQuadX, fbx1, fbx2)),
                        Lerp(pointInQuadY, Lerp(pointInQuadX, ztx1, ztx2), Lerp(pointInQuadX, zbx1, zbx2))),
                    Lerp(pointInQuadZ,
                        Lerp(pointInQuadY, Lerp(pointInQuadX, tftx1, tftx2), Lerp(pointInQuadX, tfbx1, tfbx2)),
                        Lerp(pointInQuadY, Lerp(pointInQuadX, tztx1, tztx2), Lerp(pointInQuadX, tzbx1, tzbx2))))
                    * amplitude;
                #endregion
                amplitude *= persistence;
                fx /= persistence;
                fy /= persistence;
                fz /= persistence;
                ft /= persistence;
            }
            return result / octaveFactor / max4dNoiseValue;
        }

        #endregion
    }
}