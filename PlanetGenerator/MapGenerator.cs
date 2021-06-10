﻿using Noise.Perlin;
using System;
using System.Diagnostics;

namespace MapGenerator
{
    public enum NoiseMapType
    {
        simple2d,
        simple3d,
        looped3d,
        looped4d,
    }
    public enum ShowedMapType
    {
        Noise,
        Landscape,
        BaseTemperature,
        ModeTemperature,
    }

    /// <summary>
    ///     Класс генерации карт шумов, высот, температур, осадков и всего такого прочего.
    /// </summary>
    public static class MapGenerator
    {
        public const double TAU = 2 * Math.PI;

        private const double scale = 20;
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static double min, max, r;
        private static double tx, ty, L, R;
        private static double angle_a, angle_b;
        private static int x, y, index;

        public static double[] NoiseMap(NoiseMapType noiseMapType, int seed, int size, double scale, int dx = 0, int dy = 0,
        int octaves = 3, double persistence = 0.5f)
        {
            switch (noiseMapType)
            {
                case NoiseMapType.simple2d:
                    break;
                case NoiseMapType.simple3d:
                    break;
                case NoiseMapType.looped3d:
                    break;
                case NoiseMapType.looped4d:
                    break;
            }

            throw new NotImplementedException();
        }



        /// <summary>
        ///     Функция генерирует цикличную карту шумов в диапазоне значений [0, 1].
        /// </summary>
        /// <param name="seed">Семя генерации карты шумов.</param>
        /// <param name="size">Размер карты.</param>
        /// <param name="scale">Масштаб карты.</param>
        /// <param name="dx">Смещение карты на значение dx.</param>
        /// <param name="dy">Смещение карты на значение dy.</param>
        /// <param name="octaves">Увеличение количества деталей и неровностей на карте.</param>
        /// <param name="persistence">Устойчивость к шуму.</param>
        /// <returns>Карта шумов.</returns>
        public static double[] NoiseMap(int seed, int size, double scale, int dx = 0, int dy = 0,
        int octaves = 3,
        double persistence = 0.5f)
        {

            double d1 = 1.2 / scale;
            double d2 = 2.3 / scale;
            double d3 = 3.5 / scale;
            double d4 = 4.2 / scale;

            //double preadictedMin = -0.5 * Math.Sqrt(2);

            var map = new double[size * size];
            var perlin = new Perlin(seed);
            stopwatch.Start();
            L = size / scale; // Длина окружности, сечения тородида
            R = L / TAU; // Радиус окружности, сечения тороида
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale; // y с учетом смещения и приближения карты
                angle_b = TAU * ty / L; // Текущий угол поворота от координаты y
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                    angle_a = TAU * tx / L; // Текущий угол поворота от координаты x

                    map[index] = perlin.Noise(tx, ty, octaves, persistence); // Плоская карта
                    //x +noise(x + ?,y + ?), y + noise(x + ?,y + ?) // domain warped noise

                    // Удалить
                    //map[index] = perlin.Noise(
                    //    tx + 4* perlin.Noise(tx + persistence, ty + d2), ty + 4*perlin.Noise(tx + d3, ty + d4),
                    //    octaves);

                    //double xq = perlin.Noise(tx + 1.1, ty + 5.2, octaves);
                    //double yq = perlin.Noise(tx + 5.2, ty + 1.3, octaves);
                    //double mode = 4.0;
                    //map[index] = perlin.Noise(tx + mode * xq, ty + mode * yq, octaves); // Плоская карта с домэин деформрмэтион


                    //map[index] = perlin.Noise(R * Math.Cos(angle_a), R * Math.Sin(angle_a), ty, octaves, persistence); // Карта цилиндр

                    //double rx = R * Math.Cos(angle_a);
                    //double ry = R * Math.Sin(angle_a);
                    //double xq = perlin.Noise(rx + 1.1, ry + 5.2, ty + 1.2, octaves);
                    //double yq = perlin.Noise(rx + 5.2, ry + 1.3, ty + 2.4, octaves);
                    //double zq = perlin.Noise(rx + 3.2, ry + 2.3, ty + 4.1, octaves);
                    //double mode = 4.0;
                    //map[index] = perlin.Noise(rx + mode * xq, ry + mode * yq, ty + mode * zq, octaves); // Карта цилиндр с домэин деформрмэтион

                    //map[index] = perlin.Noise(R * Math.Cos(angle_a), // Карта тороид
                    //    R * Math.Sin(angle_a),
                    //    R * Math.Sin(angle_b),
                    //    R * Math.Cos(angle_b),
                    //    octaves, persistence);

                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        public static void NormalizeMatrix(ref double[] matrix)
        {
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            foreach (var v in matrix)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }
            NormalizeMatrix(ref matrix, min, max);
        }
        public static void NormalizeMatrix(ref double[] matrix, double min, double max)
        {
            double d = max - min;
            for (int i = 0; i < matrix.Length; i++)
                matrix[i] = (matrix[i] - min) / d;
        }

        /// <summary>
        ///     Функция генерирует цикличную карту высот по заданным параметрам.
        /// </summary>
        /// <param name="noiseMap">Карту шумов, на основе которой генерируется карта высот.</param>
        /// <param name="multiplier">Множитель высоты.</param>
        /// <param name="oceanLevel">Уровень океана.</param>
        /// <param name="extraborder">
        ///     Граница дополнительного изменения высоты. Значения выше границы будут увеличиваться, ниже -
        ///     уменьшаться.
        /// </param>
        /// <param name="extraParam">Параметр дополнительного изменения высота.</param>
        /// <returns>Карты высот.</returns>
        public static int[] HeightMap(double[] noiseMap, int multiplier, int oceanLevel, int extraborder,
            double extraParam)
        {
            var heightMap = new int[noiseMap.Length];
            for (var index = 0; index < noiseMap.Length; index++)
            {
                heightMap[index] =
                    (int)(noiseMap[index] * multiplier -
                           oceanLevel); // Определяем высоту от шума и уменьшаем всю высоту на уровень океана.
                if (extraParam != 0)
                {
                    if (Math.Abs(heightMap[index]) >= extraborder)
                        heightMap[index] =
                            (int)(heightMap[index] *
                                   Math.Pow(1.2, extraParam)); // Увеличиваем значения выше extraborder.
                    else
                        heightMap[index] =
                            (int)(heightMap[index] /
                                   Math.Pow(1.1, extraParam)); // Уменьшаем значения ниже extraborder.
                }

            }

            return heightMap;
        }

        /// <summary>
        ///     Функция генерирует карту температур на нулевой высоте.
        /// </summary>
        /// <param name="size">Размер карты.</param>
        /// <param name="temperature">Температура на уровне экватора В КЕЛЬВИНАХ</param>
        /// <param name="divisor">Параметр, ограничивающий предельное падение температуры</param>
        /// <param name="exp">Параметр, определяющий скорость изменения температуры.</param>
        /// <param name="equator">Доля радиус линии экватора от центрального полюса планеты.</param>
        /// <returns>Карта температур на нулевой высоте</returns>
        public static int[] BaseTemperatureMap(int size, int temperature, double divisor = 3.4, double exp = 1,
            double equator = 0.28)
        {
            var baseTemperatureMap = new int[size * size];
            equator *= size;
            for (var y = 0; y < size; y++)
            {
                index = y * size;
                for (var x = 0; x < size; x++)
                {
                    var radius = Math.Sqrt(Math.Pow(x - size / 2, 2) + Math.Pow(y - size / 2, 2));
                    if (radius <= 2 * equator) // Если внутри двойного экватора.
                        baseTemperatureMap[index] =
                            // temp * (( 1 - ( | sin ( (R - equator ) * pi / equator / 2 ) | ) ^ exp) / divisor)
                            (int) (temperature *
                                   (1 - Math.Pow(Math.Abs(Math.Sin((radius - equator) * Math.PI / equator / 2)), exp) /
                                       divisor));
                    else // Если за пределами двойного экватора.
                        baseTemperatureMap[index] = (int) (temperature * (1 - 1 / divisor));
                    if (baseTemperatureMap[index] < 0) baseTemperatureMap[index] = 0;
                    ++index;
                }
            }

            return baseTemperatureMap;
        }

        /// <summary>
        ///     Функция генерирует карты температур с учетом падения температуры с высотой.
        /// </summary>
        /// <param name="baseTemperatureMap">Карты температур на уровне экватора.</param>
        /// <param name="heightMap">Карта высот.</param>
        /// <param name="reduction">Скорость уменьшения температуры с высотой. (градусы / 1 ед. высоты)</param>
        /// <returns>Карта температур.</returns>
        public static int[] ModeTemperatureMap(int[] baseTemperatureMap, int[] heightMap, double reduction)
        {
            var temperatureMap = new int[baseTemperatureMap.Length];
            var size = (int) Math.Sqrt(baseTemperatureMap.Length);
            for (var index = 0; index < baseTemperatureMap.Length; index++)
            {
                temperatureMap[index] = heightMap[index] > 0
                    ? baseTemperatureMap[index] - (int)(heightMap[index] * reduction)
                    : baseTemperatureMap[index];
            }

            return temperatureMap;
        }
        
    }
}