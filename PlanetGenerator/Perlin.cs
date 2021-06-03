using System;

namespace MapGenerator
{
    /// <summary>
    ///     Класс для генерации шума Перлина в 2D,3D пространстве.
    /// </summary>
    public class Perlin
    {
        private static int left, top, zoom, time, v;

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

        public Perlin(int seed = 0)
        {
            this.seed = seed;
            var rand = new Random(seed);
            permutationTable = new double[1024];
            for (var i = 0; i < 1024; i++)
                permutationTable[i] = rand.NextDouble() * 2 - 1;
        }


        #region Other functions

        /// <summary>
        ///     Функция выполняет интерполяцию.
        /// </summary>
        /// <param name="t">Локальная координата для интерполяции.</param>
        /// <param name="a">Левая граница интервала интерполяции.</param>
        /// <param name="b">Правая граница интервала интерполяции.</param>
        /// <returns>Результат интерполяции.</returns>
        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        #endregion

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

            // Координаты левой верхней вершины квадрата точки.
            var left = (int) Math.Floor(fx);
            var top = (int) Math.Floor(fy);

            // Локальные координаты точки внутри квадрата.
            var pointInQuadX = fx - left;
            var pointInQuadY = fy - top;


            var c1 = seed * 1256639923 + 4807526976;
            var c2 = left * 1836311903;
            var c5 = top * 2971215073;

            // Извлекаем градиентные векторы для всех вершин квадрата.
            // Векторы от вершин квадрата до точки внутри квадрата.
            // Считаем скалярные произведения векторов между которыми будем интерполировать.
            v = (int) (c2 ^ c5 ^ c1);
            VectorX = permutationTable[v & 1023];
            VectorY = permutationTable[(v * 2) & 1023];
            ftx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ c5 ^ c1);
            VectorX = permutationTable[v & 1023];
            VectorY = permutationTable[(v * 2) & 1023];
            ftx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023];

            v = (int) (c2 ^ ((top + 1) * 2971215073) ^ c1);
            VectorX = permutationTable[v & 1023];
            VectorY = permutationTable[(v * 2) & 1023];
            fbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ ((top + 1) * 2971215073) ^ c1);
            VectorX = permutationTable[v & 1023];
            VectorY = permutationTable[(v * 2) & 1023];
            fbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                   (pointInQuadY - 1) * permutationTable[(v * 2) & 1023];

            // Готовим параметры интерполяции, чтобы она не была линейной.
            //QunticCurve = t * t * t * (t * (t * 6 - 15) + 10)
            pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
            pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);

            //Интерполируем.
            res = Lerp(pointInQuadY,
                Lerp(pointInQuadX, ftx1, ftx2),
                Lerp(pointInQuadX, fbx1, fbx2));

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
                fx *= 2;
                fy *= 2;
            }

            return result / max;
        }


        #endregion

        #region 3D

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy,fz) для трехмерного пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X.</param>
        /// <param name="fy">Координата по оси Y.</param>
        /// <param name="fz">Координата по оси Z.</param>
        /// <returns>Значение шума в точке (fx, fy, fz). Ограничено отрезком [-1; +1].</returns>
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

            // Работает аналогично с шумом для двумерного пространства. Просто достраиваются четыре узла сзади до куба.
            left = (int) Math.Floor(fx);
            top = (int) Math.Floor(fy);
            zoom = (int) Math.Floor(fz);

            pointInQuadX = fx - left;
            pointInQuadY = fy - top;
            pointInQuadZ = fz - zoom;


            var c1 = seed * 1256639923 + 4807526976;
            var c2 = left * 1836311903;
            var c4 = zoom * 3735486054;
            var c5 = top * 2971215073;

            v = (int) (c2 ^ c5 ^ c4 ^
                       c1);

            ftx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                   pointInQuadZ * permutationTable[(v * 3) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ c5 ^ c4 ^
                       c1);

            ftx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                   pointInQuadZ * permutationTable[(v * 3) & 1023];

            v = (int) (c2 ^ ((top + 1) * 2971215073) ^ c4 ^
                       c1);

            fbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                   pointInQuadZ * permutationTable[(v * 3) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ ((top + 1) * 2971215073) ^ c4 ^
                       c1);

            fbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                   (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                   pointInQuadZ * permutationTable[(v * 3) & 1023];

            v = (int) (c2 ^ c5 ^ ((zoom + 1) * 3735486054) ^
                       c1);

            ztx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                   (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ c5 ^ ((zoom + 1) * 3735486054) ^
                       c1);

            ztx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                   (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023];

            v = (int) (c2 ^ ((top + 1) * 2971215073) ^ ((zoom + 1) * 3735486054) ^
                       c1);

            zbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                   (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ ((top + 1) * 2971215073) ^ ((zoom + 1) * 3735486054) ^
                       c1);

            zbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                   (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                   (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023];

            pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
            pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);
            pointInQuadZ = pointInQuadZ * pointInQuadZ * pointInQuadZ * (pointInQuadZ * (pointInQuadZ * 6 - 15) + 10);

            res = Lerp(pointInQuadZ,
                Lerp(pointInQuadY, Lerp(pointInQuadX, ftx1, ftx2), Lerp(pointInQuadX, fbx1, fbx2)),
                Lerp(pointInQuadY, Lerp(pointInQuadX, ztx1, ztx2), Lerp(pointInQuadX, zbx1, zbx2)));

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
        public double Noise(double fx, double fy, double fz, int octaves, double persistence = 0.5f)
        {
            double amplitude = 1;
            double max = 0;
            double result = 0;

            while (--octaves > -1)
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

        // Что за чудо?
        /*; RCX = 1
          ; RDX = 2
          ; R8 = 3
          ; R9 = 4
          ; STACK = 5 -> onwards
          
          .code
          TestMe PROC
          	add rcx, rdx
          	mov rax, rcx
          	ret
          TestMe ENDP
          END
*/

        #region 4D

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy,fz,ft) для четырехмерном пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X.</param>
        /// <param name="fy">Координата по оси Y.</param>
        /// <param name="fz">Координата по оси Z.</param>
        /// <param name="ft">Координата по оси T.</param>
        /// <returns>Значение шума в точке (fx, fy, fz, ft). Ограничено отрезком [-1; +1].</returns>
        public double Noise(double fx, double fy, double fz, double ft)
        {
            left = (int) Math.Floor(fx);
            top = (int) Math.Floor(fy);
            zoom = (int) Math.Floor(fz);
            time = (int) Math.Floor(ft);

            pointInQuadX = fx - left;
            pointInQuadY = fy - top;
            pointInQuadZ = fz - zoom;
            pointInQuadT = ft - time;

            var c1 = seed * 1256639923 + 4807526976;
            var c2 = left * 1836311903;
            var c3 = time * 1923650874;
            var c4 = zoom * 3735486054;
            var c5 = top * 2971215073;

            // Time такое
            v = (int) (c2 ^ c5 ^ c4 ^ c3 ^
                       c1);

            ftx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                   pointInQuadZ * permutationTable[(v * 3) & 1023] + pointInQuadT * permutationTable[(v * 4) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ c5 ^ c4 ^ c3 ^
                       c1);

            ftx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                   pointInQuadZ * permutationTable[(v * 3) & 1023] +
                   pointInQuadT * permutationTable[(v * 4) & 1023];

            v = (int) (c2 ^ ((top + 1) * 2971215073) ^ c4 ^ c3 ^
                       c1);

            fbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                   pointInQuadZ * permutationTable[(v * 3) & 1023] +
                   pointInQuadT * permutationTable[(v * 4) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ ((top + 1) * 2971215073) ^ c4 ^
                       c3 ^ c1);

            fbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                   (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                   pointInQuadZ * permutationTable[(v * 3) & 1023] +
                   pointInQuadT * permutationTable[(v * 4) & 1023];

            v = (int) (c2 ^ c5 ^ ((zoom + 1) * 3735486054) ^ c3 ^
                       c1);

            ztx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                   (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                   pointInQuadT * permutationTable[(v * 4) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ c5 ^ ((zoom + 1) * 3735486054) ^
                       c3 ^ c1);

            ztx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                   (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                   pointInQuadT * permutationTable[(v * 4) & 1023];

            v = (int) (c2 ^ ((top + 1) * 2971215073) ^ ((zoom + 1) * 3735486054) ^
                       c3 ^ c1);

            zbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                   (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                   pointInQuadT * permutationTable[(v * 4) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ ((top + 1) * 2971215073) ^ ((zoom + 1) * 3735486054) ^
                       c3 ^ c1);

            zbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                   (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                   (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                   pointInQuadT * permutationTable[(v * 4) & 1023];
            // Time другое
            v = (int) (c2 ^ c5 ^ c4 ^ ((time + 1) * 1923650874) ^
                       c1);

            tftx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                    pointInQuadZ * permutationTable[(v * 3) & 1023] +
                    (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ c5 ^ c4 ^
                       ((time + 1) * 1923650874) ^ c1);

            tftx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                    pointInQuadZ * permutationTable[(v * 3) & 1023] +
                    (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

            v = (int) (c2 ^ ((top + 1) * 2971215073) ^ c4 ^
                       ((time + 1) * 1923650874) ^ c1);

            tfbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                    pointInQuadZ * permutationTable[(v * 3) & 1023] +
                    (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ ((top + 1) * 2971215073) ^ c4 ^
                       ((time + 1) * 1923650874) ^ c1);

            tfbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                    (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                    pointInQuadZ * permutationTable[(v * 3) & 1023] +
                    (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

            v = (int) (c2 ^ c5 ^ ((zoom + 1) * 3735486054) ^
                       ((time + 1) * 1923650874) ^ c1);

            tztx1 = pointInQuadX * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                    (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                    (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ c5 ^ ((zoom + 1) * 3735486054) ^
                       ((time + 1) * 1923650874) ^ c1);

            tztx2 = (pointInQuadX - 1) * permutationTable[v & 1023] + pointInQuadY * permutationTable[(v * 2) & 1023] +
                    (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                    (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

            v = (int) (c2 ^ ((top + 1) * 2971215073) ^ ((zoom + 1) * 3735486054) ^
                       ((time + 1) * 1923650874) ^ c1);

            tzbx1 = pointInQuadX * permutationTable[v & 1023] + (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                    (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                    (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];

            v = (int) (((left + 1) * 1836311903) ^ ((top + 1) * 2971215073) ^ ((zoom + 1) * 3735486054) ^
                       ((time + 1) * 1923650874) ^ c1);

            tzbx2 = (pointInQuadX - 1) * permutationTable[v & 1023] +
                    (pointInQuadY - 1) * permutationTable[(v * 2) & 1023] +
                    (pointInQuadZ - 1) * permutationTable[(v * 3) & 1023] +
                    (pointInQuadT - 1) * permutationTable[(v * 4) & 1023];


            pointInQuadX = pointInQuadX * pointInQuadX * pointInQuadX * (pointInQuadX * (pointInQuadX * 6 - 15) + 10);
            pointInQuadY = pointInQuadY * pointInQuadY * pointInQuadY * (pointInQuadY * (pointInQuadY * 6 - 15) + 10);
            pointInQuadZ = pointInQuadZ * pointInQuadZ * pointInQuadZ * (pointInQuadZ * (pointInQuadZ * 6 - 15) + 10);
            pointInQuadT = pointInQuadT * pointInQuadT * pointInQuadT * (pointInQuadT * (pointInQuadT * 6 - 15) + 10);

            res = Lerp(pointInQuadT,
                Lerp(pointInQuadZ,
                    Lerp(pointInQuadY, Lerp(pointInQuadX, ftx1, ftx2), Lerp(pointInQuadX, fbx1, fbx2)),
                    Lerp(pointInQuadY, Lerp(pointInQuadX, ztx1, ztx2), Lerp(pointInQuadX, zbx1, zbx2))),
                Lerp(pointInQuadZ,
                    Lerp(pointInQuadY, Lerp(pointInQuadX, tftx1, tftx2), Lerp(pointInQuadX, tfbx1, tfbx2)),
                    Lerp(pointInQuadY, Lerp(pointInQuadX, tztx1, tztx2), Lerp(pointInQuadX, tzbx1, tzbx2))));

            return res;
        }

        /// <summary>
        ///     Функция генерирует шум в точке (fx,fy,fz,ft) для четырехмерном пространства.
        /// </summary>
        /// <param name="fx">Координата по оси X.</param>
        /// <param name="fy">Координата по оси Y.</param>
        /// <param name="fz">Координата по оси Z.</param>
        /// <param name="ft">Координата по оси T.</param>
        /// <param name="octaves">Количество повторных наложений шума. Увеличивает детальность.</param>
        /// <param name="persistence">Устойчивость к шуму. Выше значение - сильнее шум.</param>
        /// <returns>Значение шума в точке (fx, fy, fz, ft). Ограничено отрезком [-1; +1].</returns>
        public double Noise(double fx, double fy, double fz, double ft, int octaves, double persistence = 0.5f)
        {
            double amplitude = 1;
            double max = 0;
            double result = 0;

            while (--octaves > -1)
            {
                max += amplitude;
                result += Noise(fx, fy, fz,ft) * amplitude;
                amplitude *= persistence;
                fx *= 2;
                fy *= 2;
                fz *= 2;
                ft *= 2;
            }

            return result / max;
        }

        #endregion

        #endregion
    }
}