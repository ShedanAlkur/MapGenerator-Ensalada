using System;
using System.Diagnostics;

namespace MapGenerator
{
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
        public static double[] GenerateNoiseMap(int seed, int size, double scale, int dx = 0, int dy = 0,
            int octaves = 3,
            double persistence = 0.5f)
        {
            //min = 2;
            //max = -2;
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

                    //map[index] = perlin.Noise(tx, ty, octaves, persistence); // Плоская карта

                    //map[index] = perlin.Noise(R * Math.Cos(angle_a), R * Math.Sin(angle_a), ty, octaves, persistence); // Карта цилиндр

                    map[index] = perlin.Noise(R * Math.Cos(angle_a), // Карта тороид
                        R * Math.Sin(angle_a),
                        R * Math.Sin(angle_b),
                        R * Math.Cos(angle_b),
                        octaves, persistence);

                    if (map[index] < min)
                        min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            //Console.WriteLine($"min {min}; max {max}");
            //min = -1;
            //max = 1;
            r = max - min; // Генерируем карту шумов, сводя её от диапазона [-1; +1] (почти) к [0; 1]
            for (y = 0; y < size; y++)
            {
                index = y * size;
                for (x = 0; x < size; x++)
                {
                    index = x + y * size;
                    map[index] = (map[index] - min) / r;
                    ++index;
                }
            }

            stopwatch.Stop();
            Console.WriteLine("Суммарное время, ушедшее на генерацию карт шумов: " + stopwatch.ElapsedMilliseconds);
            //stopwatch.Reset();
            return map;
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
        public static int[] GenerateHeightMap(double[] noiseMap, int multiplier, int oceanLevel, int extraborder,
            double extraParam)
        {
            var heightMap = new int[noiseMap.Length];
            var size = (int) Math.Sqrt(noiseMap.Length);
            for (var y = 0; y < size; y++)
            {
                index = y * size;
                for (var x = 0; x < size; x++)
                {
                    heightMap[index] =
                        (int) (noiseMap[index] * multiplier -
                               oceanLevel); // Определяем высоту от шума и уменьшаем всю высоту на уровень океана.
                    if (Math.Abs(heightMap[index]) >= extraborder)
                        heightMap[index] =
                            (int) (heightMap[index] *
                                   Math.Pow(1.2, extraParam)); // Увеличиваем значения выше extraborder.
                    else
                        heightMap[index] =
                            (int) (heightMap[index] /
                                   Math.Pow(1.1, extraParam)); // Уменьшаем значения ниже extraborder.
                    ++index;
                }
            }

            return heightMap;
        }

        /// <summary>
        ///     Функция генерирует цикличную карту высот по заданным параметрам.
        /// </summary>
        /// <param name="seed">Семя генерации карты высот.</param>
        /// <param name="size">Размер карты.</param>
        /// <param name="scale">Масштаб карты.</param>
        /// <param name="multiplier">Множитель высоты.</param>
        /// <param name="oceanLevel">Уровень океана.</param>
        /// <param name="extraborder">
        ///     Граница дополнительного изменения высоты. Значения выше границы будут увеличиваться, ниже -
        ///     уменьшаться.
        /// </param>
        /// <param name="extraParam">Параметр дополнительного изменения высота.</param>
        /// <returns>Карты высот.</returns>
        public static int[] GenerateHeightMap(int seed, int size, double scale, int multiplier, int oceanLevel,
            int extraborder = 0, double extraParam = 0)
        {
            return GenerateHeightMap(GenerateNoiseMap(seed, size, scale), multiplier, oceanLevel, extraborder,
                extraParam);
        }

        /// <summary>
        ///     Функция генерирует цикличную карту с диапазоном значений [0, multiplier].
        /// </summary>
        /// <param name="seed">Семя генерации карты.</param>
        /// <param name="size">Размер карты.</param>
        /// <param name="scale">Масштаб карты.</param>
        /// <param name="multiplier">Множитель.</param>
        /// <returns>Карта с диапазоном значений [0, multiplier].</returns>
        public static int[] GenerateMap(int seed, int size, double scale, int multiplier)
        {
            var noiseMap = GenerateNoiseMap(seed, size, scale, 0, 0, 1);
            var map = new int[size * size];
            for (var y = 0; y < size; y++)
            {
                index = y * size;
                for (var x = 0; x < size; x++)
                {
                    map[index] = (int) (noiseMap[index] * multiplier);
                    ++index;
                }
            }

            return map;
        }

        /// <summary>
        ///     Функция генерирует цикличную карту ресура с диапазоном значений [0, multiplier], учитывая характер ресурса.
        /// </summary>
        /// <param name="seed">Семя генерации карты.</param>
        /// <param name="size">Размер карты.</param>
        /// <param name="scale">Масштаб карты.</param>
        /// <param name="multiplier">Множитель.</param>
        /// <param name="resourceType">Характер ресурса. 0 - повсеместный, -1 - подводный, +1 - надводный.</param>
        /// <param name="heightMap">Карта высот, по которой определяется возможность появления ресурса</param>
        /// <returns>Карта с диапазоном значений [0, multiplier].</returns>
        public static int[] GenerateMap(int seed, int size, double scale, int multiplier, int resourceType,
            int[] heightMap)
        {
            if (heightMap == null) throw new ArgumentNullException(nameof(heightMap));
            if (size * size != heightMap.Length)
                throw new Exception("Размеры карты ресурсов и карты высот не совпадают.");
            var map = GenerateMap(seed, size, scale, multiplier);
            switch (resourceType)
            {
                //case 0: break;
                case -1:
                {
                    for (x = 0; x < size * size; x++)
                        if (heightMap[x] >= 0)
                            map[x] = 0;
                    break;
                }
                case 1:
                {
                    for (x = 0; x < size * size; x++)
                        if (heightMap[x] < 0)
                            map[x] = 0;
                    break;
                }
            }

            return map;
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
        public static int[] GenerateBaseTemperatureMap(int size, int temperature, double divisor = 3.4, double exp = 1,
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
        public static int[] GenerateTemperatureMap(int[] baseTemperatureMap, int[] heightMap, double reduction)
        {
            var temperatureMap = new int[baseTemperatureMap.Length];
            var size = (int) Math.Sqrt(baseTemperatureMap.Length);
            for (var y = 0; y < size; y++)
            {
                index = y * size;
                for (var x = 0; x < size; x++)
                {
                    temperatureMap[index] = heightMap[index] > 0
                        ? baseTemperatureMap[index] - (int) (heightMap[index] * reduction)
                        : baseTemperatureMap[index];
                    ++index;
                }
            }

            return temperatureMap;
        }

        /// <summary>
        ///     Функция генерирует карты температур с учетом падения температуры с высотой.
        /// </summary>
        /// <param name="heightMap">Карта высот.</param>
        /// <param name="temperature">Температура на уровне экватора В КЕЛЬВИНАХ</param>
        /// <param name="reduction">Скорость уменьшения температуры с высотой. (градусы / 1 ед. высоты)</param>
        /// <param name="divisor">Параметр, ограничивающий предельное падение температуры</param>
        /// <param name="exp">Параметр, определяющий скорость изменения температуры.</param>
        /// <param name="equator">Радиус линии экватора от центрального полюса планеты.</param>
        /// <returns></returns>
        public static int[] GenerateTemperatureMap(int[] heightMap, int temperature, double reduction,
            double divisor = 3.4, double exp = 1, double equator = 0.28)
        {
            return GenerateTemperatureMap(
                GenerateBaseTemperatureMap((int) Math.Floor(Math.Sqrt(heightMap.Length)), temperature, divisor, exp,
                    equator)
                , heightMap, reduction);
        }
    }
}