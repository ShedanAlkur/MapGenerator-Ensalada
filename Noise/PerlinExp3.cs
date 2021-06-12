using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Noise.Perlin
{
    public class PerlinExp3
    {

        int magical1 = 256639923;
        int magical2 = 807526976;
        int magicalLeft = 836311903;
        int magicalZoom = 735486054;
        int magicalTop = 971215073;

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

        Random rnd;
        int seed;
        int dimension;
        int octaves;
        double persistance;

        private readonly byte[] permutationTable;

        double[][] gradientVector;

        public PerlinExp3(int seed, int dimension, int octaves, double persistance = 0.5)
        {
            this.seed = seed;
            this.dimension = dimension;
            this.octaves = octaves;
            this.persistance = persistance;
            rnd = new Random(this.seed);

            permutationTable = new byte[1024];
            rnd.NextBytes(permutationTable);

            CreateGradientVector();
        }

        public double Noise(double fx, double fy)
        {
            double res = 0;
            double amplitude = 1;

            for (int i = 0; i < octaves; i++)
            {

                // Координаты левой верхней вершины квадрата точки.
                if (fx < 0 && fx % -1 != 0) left = (int)(fx - 1);
                else left = (int)fx;
                if (fy < 0 && fy % -1 != 0) top = (int)(fy - 1);
                else top = (int)fy;

                // Локальные координаты точки внутри квадрата.
                pointInQuadX = fx - left;
                pointInQuadY = fy - top;


                c1 = seed * magical1 + magical2;
                c2 = left * magicalLeft;
                c5 = top * magicalTop;

                // Извлекаем градиентные векторы для всех вершин квадрата.
                // Векторы от вершин квадрата до точки внутри квадрата.
                // Считаем скалярные произведения векторов между которыми будем интерполировать.

                v = (int)(left * magicalLeft ^ top * magicalTop ^ c1) & 1023;
                v = permutationTable[v] & 3;
                ftx1 = pointInQuadX * gradientVector[v & 3][0] + pointInQuadY * gradientVector[v & 3][1];

                v = (int)(((left + 1) * magicalLeft) ^ top * magicalTop ^ c1) & 1023;
                v = permutationTable[v] & 3;
                ftx2 = (pointInQuadX - 1) * gradientVector[v & 3][0] + pointInQuadY * gradientVector[v & 3][1];

                v = (int)(left * magicalLeft ^ ((top + 1) * magicalTop) ^ c1) & 1023;
                v = permutationTable[v] & 3;
                fbx1 = pointInQuadX * gradientVector[v & 3][0] + (pointInQuadY - 1) * gradientVector[v & 3][1];

                v = (int)(((left + 1) * magicalLeft) ^ ((top + 1) * magicalTop) ^ c1) & 1023;
                v = permutationTable[v] & 3;
                fbx2 = (pointInQuadX - 1) * gradientVector[v & 3][0] + (pointInQuadY - 1) * gradientVector[v & 3][1];

                // Готовим параметры интерполяции, чтобы она не была линейной.
                //QunticCurve = t * t * t * (t * (t * 6 - 15) + 10)
                pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
                pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);

                res += (ftx1 + pointInQuadX * (ftx2 - ftx1) + pointInQuadY *
                    (fbx1 + pointInQuadX * (fbx2 - fbx1)
                    - (ftx1 + pointInQuadX * (ftx2 - ftx1)))) * amplitude;

                amplitude /= 2;
                fx *= 2;
                fy *= 2;
            }
            // Возвращаем результат.
            return res;
        }

        public double Noise(double fx, double fy, double fz)
        {
            double res = 0;
            double amplitude = 1;

            for (int i = 0; i < octaves; i++)
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
                c2 = left * magicalLeft;
                c4 = zoom * magicalZoom;
                c5 = top * magicalTop;

                // Извлекаем градиентные векторы для всех вершин квадрата.
                // Векторы от вершин квадрата до точки внутри квадрата.
                // Считаем скалярные произведения векторов между которыми будем интерполировать.

                int vectorCount = 15;

                v = (int)(left * magicalLeft ^ top * magicalTop ^ zoom * magicalZoom ^ c1) & 1023;
                v = permutationTable[v] & vectorCount;
                ftx1 = pointInQuadX * gradientVector[v][0] + pointInQuadY * gradientVector[v][1]
                    + pointInQuadZ * gradientVector[v][2];

                v = (int)(((left + 1) * magicalLeft) ^ top * magicalTop ^ zoom * magicalZoom ^ c1) & 1023;
                v = permutationTable[v] & vectorCount;
                ftx2 = (pointInQuadX - 1) * gradientVector[v][0] + pointInQuadY * gradientVector[v][1]
                    + pointInQuadZ * gradientVector[v][2];

                v = (int)(left * magicalLeft ^ ((top + 1) * magicalTop) ^ zoom * magicalZoom ^ c1) & 1023;
                v = permutationTable[v] & vectorCount;
                fbx1 = pointInQuadX * gradientVector[v][0] + (pointInQuadY - 1) * gradientVector[v][1]
                    + pointInQuadZ * gradientVector[v][2];

                v = (int)(((left + 1) * magicalLeft) ^ ((top + 1) * magicalTop) ^ zoom * magicalZoom ^ c1) & 1023;
                v = permutationTable[v] & vectorCount;
                fbx2 = (pointInQuadX - 1) * gradientVector[v][0] + (pointInQuadY - 1) * gradientVector[v][1]
                    + pointInQuadZ * gradientVector[v][2];

                v = (int)(left * magicalLeft ^ top * magicalTop ^ (zoom + 1) * magicalZoom ^ c1) & 1023;
                v = permutationTable[v] & vectorCount;
                ztx1 = pointInQuadX * gradientVector[v][0] + pointInQuadY * gradientVector[v][1]
                    + (pointInQuadZ - 1) * gradientVector[v][2];

                v = (int)(((left + 1) * magicalLeft) ^ top * magicalTop ^ (zoom + 1) * magicalZoom ^ c1) & 1023;
                v = permutationTable[v] & vectorCount;
                ztx2 = (pointInQuadX - 1) * gradientVector[v][0] + pointInQuadY * gradientVector[v][1]
                    + (pointInQuadZ - 1) * gradientVector[v][2];

                v = (int)(left * magicalLeft ^ ((top + 1) * magicalTop) ^ (zoom + 1) * magicalZoom ^ c1) & 1023;
                v = permutationTable[v] & vectorCount;
                zbx1 = pointInQuadX * gradientVector[v][0] + (pointInQuadY - 1) * gradientVector[v][1]
                    + (pointInQuadZ - 1) * gradientVector[v][2];

                v = (int)(((left + 1) * magicalLeft) ^ ((top + 1) * magicalTop) ^ (zoom + 1) * magicalZoom ^ c1) & 1023;
                v = permutationTable[v] & vectorCount;
                zbx2 = (pointInQuadX - 1) * gradientVector[v][0] + (pointInQuadY - 1) * gradientVector[v][1]
                    + (pointInQuadZ - 1) * gradientVector[v][2];

                // Готовим параметры интерполяции, чтобы она не была линейной.
                //QunticCurve = t * t * t * (t * (t * 6 - 15) + 10)
                pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
                pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);
                pointInQuadZ = pointInQuadZ * pointInQuadZ * pointInQuadZ * (pointInQuadZ * (pointInQuadZ * 6 - 15) + 10);

                res += Lerp(pointInQuadZ,
                    Lerp(pointInQuadY, Lerp(pointInQuadX, ftx1, ftx2), Lerp(pointInQuadX, fbx1, fbx2)),
                    Lerp(pointInQuadY, Lerp(pointInQuadX, ztx1, ztx2), Lerp(pointInQuadX, zbx1, zbx2)));

                amplitude /= 2;
                fx *= 2;
                fy *= 2;
                fz *= 2;
            }
            // Возвращаем результат.
            return res;
        }

        void CreateGradientVector()
        {
            gradientVector = new double[1 << (dimension + 1)][];
            int zeroE = -1;
            int sign = 0;
            int signIndex;
            int v = 1 << (dimension - 1);
            double vecLenght = Math.Sqrt(1.0 / dimension);
            for (int i = 0; i < v * dimension; i++)
            {
                if ((i & (v - 1)) == 0) zeroE++;
                gradientVector[i] = new double[dimension];
                signIndex = 0;
                for (int j = 0; j < dimension; j++)
                {
                    if (j == zeroE) continue;
                    gradientVector[i][j] = (((sign >> signIndex) & 0b1) == 0) ? -vecLenght : vecLenght;
                    signIndex++;
                }
                sign++;
            }
            for (int i = v * dimension; i < gradientVector.Length; i++)
                gradientVector[i] = gradientVector[(i * 13631) & ((v * dimension) - 1)];
        }

        static double SmoothStep(double t) => t * t * (3 - 2 * t);
        static double QunticCurve(double t)=> t * t * t * (t * (t * 6 - 15) + 10);
        static double Lerp(double t, double a, double b) => a + t * (b - a);

    }
}
