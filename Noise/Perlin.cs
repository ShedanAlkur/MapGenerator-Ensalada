using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Noise.Perlin
{
    /// <summary>
    /// Класс для генерации процедурного шума Перлина в одно-, дву-, трех- и четырехмерном пространстве.
    /// </summary>
    public class Perlin 
    {
        int left, top, zoom, time, v;

        const int magical1 = 256639923;
        const int magical2 = 807526976;
        const int magical3 = 836311903;
        const int magical4 = 735486054;
        const int magical5 = 971215073;
        const int magical6 = 623650874;
        int c1, c2, c3, c4, c5;

        double ftx1, ftx2,
            fbx1, fbx2,
            ztx1, ztx2,
            zbx1, zbx2;

        double tftx1, tftx2,
            tfbx1, tfbx2,
            tztx1, tztx2,
            tzbx1, tzbx2;

        private int k;
        private double amplitude;

        double pointInQuadX, pointInQuadY, pointInQuadZ, pointInQuadT;
        double VectorX, VectorY, VectorZ, VectorT;
        double res;
        readonly double[] permutationTable;
        public int seed;
        int octave;
        public int Octave
        {
            get => octave;
            set
            {
                octave = value;
                octaveFactor = 2 - Math.Pow(persistence, octave - 1);
            }
        }
        double persistence;
        public double Persistence
        {
            get => persistence; set
            {
                persistence = value;
                octaveFactor = 2 - Math.Pow(persistence, octave - 1);
            }
        }

        static readonly double max2dNoiseValue = 0.5 * Math.Sqrt(2);
        static readonly double max3dNoiseValue = 0.5 * Math.Sqrt(3);
        static readonly double max4dNoiseValue = 0.5 * Math.Sqrt(4);
        double octaveFactor;

        /// <summary>
        /// Инициализирует новый экземпляр класса PerlinFin с помощью указанных начальных значений.
        /// </summary>
        /// <param name="seed">Семя генерации шума.</param>
        /// <param name="octave">Количество октав, которое рассчитывается для шума в каждой точке.</param>
        /// <param name="persistence">Устойчивость к наложению октав. Больше величина - сильнее влияние октав.</param>
        public Perlin(int seed, int octave, double persistence = 0.5)
        {
            this.seed = seed;
            this.Octave = octave;
            this.Persistence = persistence;

            var rnd = new Random(seed);
            permutationTable = new double[1024];
            for (var i = 0; i < permutationTable.Length; i++)
                permutationTable[i] = rnd.NextDouble() * 2 - 1;
        }

        #region Вспомогательные методы

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

        /// <summary>
        /// Функция Косинусной интерполяции.
        /// </summary>
        /// <param name="t">Координата интерполяции, лежащая в отрезке [0; 1].</param>
        /// <param name="a">Значение функции в координате {0}.</param>
        /// <param name="b">Значение функции в координате {1}.</param>
        /// <returns>Результат интерполяции. Лежит в отрезке [0; 1].</returns>
        public static double CosineInterpolation(double x, double a, double b)
        {
            x = (1 - Math.Cos(x * Math.PI)) * 0.5;
            return a * (1 - x) + b * x;
        }

        #endregion

        #region Методы генерации шума

        /// <summary>
        /// Функция генерации шума в одномерном пространстве.
        /// </summary>
        /// <param name="fx">Координата нахождения шума по оси oX.</param>
        /// <returns>Значение шума в заданных координатах.</returns>
        public double Noise(double fx)
        {
            res = 0;
            amplitude = 1;
            for (k = 0; k <= octave; k++)
            {
                // Координаты левой верхней вершины квадрата, в между узлов которого списана точка.
                if (fx < 0 && fx % -1 != 0) left = (int)(fx - 1);
                else left = (int)fx;

                // Локальные координаты точки внутри квадрата.
                pointInQuadX = fx - left;

                c1 = seed * magical1;

                v = (int)(left * magical3);
                ftx1 = permutationTable[v & 1023];

                v = (int)(((left + 1) * magical3));
                ftx2 = permutationTable[v & 1023];
                // y = 2(a-b)x^4 - (3a-5b)x^3 - 3bx^2 + ax
                res += (2 * (ftx1 - ftx2) * pointInQuadX * pointInQuadX * pointInQuadX * pointInQuadX
                    - (3 * ftx1 - 5 * ftx2) * pointInQuadX * pointInQuadX * pointInQuadX
                    - 3 * ftx2 * pointInQuadX * pointInQuadX
                    + ftx1 * pointInQuadX)
                    * amplitude;

                amplitude *= persistence;
                fx /= persistence;
            }
            return res / octaveFactor;
        }

        /// <summary>
        /// Функция генерации шума в двумерном пространстве.
        /// </summary>
        /// <param name="fx">Координата нахождения шума по оси oX.</param>
        /// <param name="fy">Координата нахождения шума по оси oY</param>
        /// <returns>Значение шума в заданных координатах.</returns>
        public double Noise(double fx, double fy)
        {
            res = 0;
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

                c1 = seed * magical1 + magical2;
                c2 = left * magical3;
                c5 = top * magical5;

                // 1. Извлекаем градиентные векторы для всех вершин квадрата.
                // 2. Используем найденные векторы от вершин квадрата до точки внутри квадрата.
                // 3. Считаем скалярные произведения векторов между которыми будем интерполировать.
                v = (int)(left * magical3 ^ top * magical5 ^ c1);
                ftx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023];

                v = (int)(((left + 1) * magical3) ^ top * magical5 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023];

                v = (int)(left * magical3 ^ ((top + 1) * magical5) ^ c1);
                fbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023];

                // Готовим параметры интерполяции, чтобы она не была линейной.
                //QunticCurve = t * t * t * (t * (t * 6 - 15) + 10)
                pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
                pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);

                // Интерполируем между (ftx2 и ftx1), затем (fbx2 и fbx1), затем между двумя результатами этих интерполяций.
                res += (ftx1 + pointInQuadX * (ftx2 - ftx1) + pointInQuadY *
                    (fbx1 + pointInQuadX * (fbx2 - fbx1)
                    - (ftx1 + pointInQuadX * (ftx2 - ftx1))))
                    * amplitude;
                amplitude *= persistence;
                fx /= persistence;
                fy /= persistence;
            }
            return res / octaveFactor;
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

            res = 0;
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


                c1 = seed * magical1 + magical2;
                c2 = left * magical3;
                c4 = zoom * magical4;
                c5 = top * magical5;

                v = (int)(c2 ^ c5 ^ c4 ^ c1);
                ftx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                       pointInQuadZ * permutationTable[(v * 3) & 1023];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                       pointInQuadZ * permutationTable[(v * 3) & 1023];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ c1);
                fbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                       pointInQuadZ * permutationTable[(v * 3) & 1023];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                       (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                       pointInQuadZ * permutationTable[(v * 3) & 1023];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ c1);
                ztx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                       (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ c1);
                ztx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                       (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c1);
                zbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                       (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c1);
                zbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                       (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                       (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023];

                pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
                pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);
                pointInQuadZ = pointInQuadZ * pointInQuadZ * pointInQuadZ * (pointInQuadZ * (pointInQuadZ * 6 - 15) + 10);
                       
                res += Lerp(pointInQuadZ,
                    Lerp(pointInQuadY, Lerp(pointInQuadX, ftx1, ftx2), Lerp(pointInQuadX, fbx1, fbx2)),
                    Lerp(pointInQuadY, Lerp(pointInQuadX, ztx1, ztx2), Lerp(pointInQuadX, zbx1, zbx2)))
                    * amplitude;

                amplitude *= persistence;
                fx /= persistence;
                fy /= persistence;
                fz /= persistence;
            }
            return res / octaveFactor;
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
            res = 0;
            amplitude = 1;
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

                c1 = seed * magical1 + magical2;
                c2 = left * magical3;
                c3 = time * magical6;
                c4 = zoom * magical4;
                c5 = top * magical5;

                v = (int)(c2 ^ c5 ^ c4 ^ c3 ^ c1);
                ftx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                       pointInQuadZ * permutationTable[(v * 3) & 1023] + pointInQuadT * permutationTable[(v * 4) & 1023];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ c3 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                       pointInQuadZ * permutationTable[(v * 3) & 1023] +
                       pointInQuadT * permutationTable[(v * 4) & 1023];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ c3 ^ c1);
                fbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                       pointInQuadZ * permutationTable[(v * 3) & 1023] +
                       pointInQuadT * permutationTable[(v * 4) & 1023];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ c3 ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                       (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                       pointInQuadZ * permutationTable[(v * 3) & 1023] +
                       pointInQuadT * permutationTable[(v * 4) & 1023];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                ztx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                       (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                       pointInQuadT * permutationTable[(v * 4) & 1023];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                ztx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                       (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                       pointInQuadT * permutationTable[(v * 4) & 1023];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                zbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                       (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                       pointInQuadT * permutationTable[(v * 4) & 1023];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                zbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                       (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                       (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                       pointInQuadT * permutationTable[(v * 4) & 1023];

                ///

                v = (int)(c2 ^ c5 ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tftx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                        pointInQuadZ * permutationTable[(v * 3) & 1023] +
                        (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tftx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                        pointInQuadZ * permutationTable[(v * 3) & 1023] +
                        (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tfbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                        pointInQuadZ * permutationTable[(v * 3) & 1023] +
                        (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tfbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                        (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                        pointInQuadZ * permutationTable[(v * 3) & 1023] +
                        (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tztx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                        (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                        (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tztx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                        (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                        (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tzbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                        (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                        (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tzbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                        (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                        (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                        (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

                pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
                pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);
                pointInQuadZ = pointInQuadZ * pointInQuadZ * pointInQuadZ * (pointInQuadZ * (pointInQuadZ * 6 - 15) + 10);
                pointInQuadT = pointInQuadT * pointInQuadT * pointInQuadT * (pointInQuadT * (pointInQuadT * 6 - 15) + 10);

                res += Lerp(pointInQuadT,
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
            return res / octaveFactor;
        }

        #endregion
    }
}
