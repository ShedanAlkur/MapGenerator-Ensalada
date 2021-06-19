﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Noise.Perlin
{
    public class PerlinFinNormalized
    {
        int left, top, zoom, time, v;

        int magical1 = 256639923;
        int magical2 = 807526976;
        int magical3 = 836311903;
        int magical4 = 735486054;
        int magical5 = 971215073;
        int magical6 = 623650874;
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
        double res;
        readonly double[] permutationTable;
        double[][] permutationVector;
        public int seed;
        int octave;
        int preparedDimension;
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

        void NormalizeVector(ref double[] vector)
        {
            double vectorLenght = 0;
            for (int i = 0; i < vector.Length; i++)
                vectorLenght += vector[i] * vector[i];
            vectorLenght = Math.Sqrt(vectorLenght);
            for (int i = 0; i < vector.Length; i++)
                vector[i] = vector[i] / vectorLenght;
        }

        public PerlinFinNormalized(int seed, int octave, double persistence = 0.5)
        {
            this.seed = seed;
            this.Octave = octave;
            this.Persistence = persistence;

            var rnd = new Random(seed);
            permutationVector = new double[1024][];
        }

        void PrepareDimension(int dimension)
        {
            var rnd = new Random(seed);
            for (int i = 0; i < permutationVector.Length; i++)
            {
                permutationVector[i] = new double[dimension];
                for (int j = 0; j < permutationVector[0].Length; j++)
                    permutationVector[i][j] = rnd.NextDouble() * 2 - 1;
                NormalizeVector(ref permutationVector[i]);
            }
            preparedDimension = dimension;
        }

        static double SmoothStep(double t) => t * t * (3 - 2 * t);
        static double QunticCurve(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        static double Lerp(double t, double a, double b) => a + t * (b - a);
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
                ftx1 = permutationVector[v & 1023][0];

                v = (int)(((left + 1) * magical3));
                ftx2 = permutationVector[v & 1023][0];
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

        public double Noise(double fx, double fy)
        {
            if (preparedDimension != 2) PrepareDimension(2);
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
                ftx1 = pointInQuadX * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1];

                v = (int)(((left + 1) * magical3) ^ top * magical5 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1];

                v = (int)(left * magical3 ^ ((top + 1) * magical5) ^ c1);
                fbx1 = pointInQuadX * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1];

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
            return res / octaveFactor / max2dNoiseValue;
        }

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
                ftx1 = pointInQuadX * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                       pointInQuadZ * permutationVector[v & 1023][2];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                       pointInQuadZ * permutationVector[v & 1023][2];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ c1);
                fbx1 = pointInQuadX * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                       pointInQuadZ * permutationVector[v & 1023][2];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] +
                       (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                       pointInQuadZ * permutationVector[v & 1023][2];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ c1);
                ztx1 = pointInQuadX * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVector[v & 1023][2];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ c1);
                ztx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVector[v & 1023][2];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c1);
                zbx1 = pointInQuadX * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVector[v & 1023][2];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c1);
                zbx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVector[v & 1023][2];

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
            return res / octaveFactor / max3dNoiseValue;
        }

        public double Noise(double fx, double fy, double fz, double ft)
        {
            res = 0;
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

                c1 = seed * magical1 + magical2;
                c2 = left * magical3;
                c3 = time * magical6;
                c4 = zoom * magical4;
                c5 = top * magical5;

                v = (int)(c2 ^ c5 ^ c4 ^ c3 ^ c1);
                ftx1 = pointInQuadX * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                       pointInQuadZ * permutationVector[v & 1023][2] +pointInQuadT * permutationVector[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ c3 ^ c1);
                ftx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                       pointInQuadZ * permutationVector[v & 1023][2] +
                       pointInQuadT * permutationVector[v & 1023][3];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ c3 ^ c1);
                fbx1 = pointInQuadX * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                       pointInQuadZ * permutationVector[v & 1023][2] +
                       pointInQuadT * permutationVector[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ c3 ^ c1);
                fbx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] +
                       (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                       pointInQuadZ * permutationVector[v & 1023][2] +
                       pointInQuadT * permutationVector[v & 1023][3];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                ztx1 = pointInQuadX * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVector[v & 1023][2] +
                       pointInQuadT * permutationVector[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                ztx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVector[v & 1023][2] +
                       pointInQuadT * permutationVector[v & 1023][3];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                zbx1 = pointInQuadX * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVector[v & 1023][2] +
                       pointInQuadT * permutationVector[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ c3 ^ c1);
                zbx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] +
                       (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                       (pointInQuadZ - 1) * permutationVector[v & 1023][2] +
                       pointInQuadT * permutationVector[v & 1023][3];

                ///

                v = (int)(c2 ^ c5 ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tftx1 = pointInQuadX * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                        pointInQuadZ * permutationVector[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVector[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ c5 ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tftx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                        pointInQuadZ * permutationVector[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVector[v & 1023][3];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tfbx1 = pointInQuadX * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                        pointInQuadZ * permutationVector[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVector[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ c4 ^ ((time + 1) * magical6) ^ c1);
                tfbx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] +
                        (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                        pointInQuadZ * permutationVector[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVector[v & 1023][3];

                v = (int)(c2 ^ c5 ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tztx1 = pointInQuadX * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                        (pointInQuadZ - 1) * permutationVector[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVector[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ c5 ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tztx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] + pointInQuadY * permutationVector[v & 1023][1] +
                        (pointInQuadZ - 1) * permutationVector[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVector[v & 1023][3];

                v = (int)(c2 ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tzbx1 = pointInQuadX * permutationVector[v & 1023][0] + (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                        (pointInQuadZ - 1) * permutationVector[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVector[v & 1023][3];

                v = (int)(((left + 1) * magical3) ^ ((top + 1) * magical5) ^ ((zoom + 1) * magical4) ^ ((time + 1) * magical6) ^ c1);
                tzbx2 = (pointInQuadX - 1) * permutationVector[v & 1023][0] +
                        (pointInQuadY - 1) * permutationVector[v & 1023][1] +
                        (pointInQuadZ - 1) * permutationVector[v & 1023][2] +
                        (pointInQuadT - 1) * permutationVector[v & 1023][3];

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
            return res / octaveFactor / max4dNoiseValue;
        }


        #endregion
    }
}
