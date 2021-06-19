using Noise.Perlin;
using System;
using System.Diagnostics;

namespace MapGenerator
{
    /// <summary>
    /// Типы генерируемых карт шумов Перлина.
    /// </summary>
    public enum NoiseMapType
    {
        //testedA,
        //testedB,
        //testedC,
        //testedD,
        simple1d,
        simple2d,
        domainWarped2d,
        domainWarped3d,
        simple3d,
        looped3d,
        looped4d,
    }

    /// <summary>
    /// Типы визуализируемых карт.
    /// </summary>
    public enum ShowedMapType
    {
        Noise,
        Landscape,
        BaseTemperature,
        ModeTemperature,
    }

    /// <summary>
    /// Класс генерации карт шумов, высот, температур.
    /// </summary>
    public static class MapGenerator
    {
        const double TAU = 2 * Math.PI;

        static readonly Stopwatch stopwatch = new Stopwatch();
        static double min, max;
        static double tx, ty, L, R;
        static double angle_a, angle_b;
        static int x, y, index;

        #region Генерация карт шумов

        /// <summary>
        /// Метод генерации карты для визуализации одномерных шумов Перлина.
        /// </summary>
        /// <param name="seed">Семя генерации.</param>
        /// <param name="size">Длина и ширина карты.</param>
        /// <param name="scale">Показатель приближения.</param>
        /// <param name="dx">Смещение от начала координат по оси oX.</param>
        /// <param name="octaves">Количество октав.</param>
        /// <param name="persistence">Устойчивость к наложению октав.</param>
        /// <returns>Карта одномерного шумов Перлина для визуализации.</returns>
        public static double[] NoiseMap_simple1d(int seed, int size, double scale, int dx,
        int octaves, double persistence)
        {
            var map = new double[size * size];
            var perlin = new PerlinFin(seed, octaves, persistence);
            int halfSize = (int)(size / 2);
            int yValue;
            stopwatch.Start();
            double yLinesNumber = (size / scale) + 1;
            if (yLinesNumber % 0 > 0) yLinesNumber++;
            int xValue = -(int)(((dx / scale) - Math.Floor(dx / scale)) * scale);
            for (int i = 0; i < yLinesNumber; i++)
            {
                Console.WriteLine(xValue);
                if (xValue < size && xValue >= 0)
                    for (y = 0; y < size; y++)
                        map[y * size + xValue] = 0.5;
                xValue += (int)scale;
            }
            for (x = 0; x < size; x++)
            {
                tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                map[halfSize * size + x] = 0.5;
                map[(halfSize - halfSize / 2) * size + x] = 0.25;
                map[(halfSize + halfSize / 2) * size + x] = 0.5;
                yValue = (int)(perlin.Noise(tx) * halfSize);
                //Console.WriteLine(perlin.Noise(tx));
                if (yValue <= -halfSize || yValue >= halfSize) continue;
                map[(halfSize - yValue)*size + x] = 1; // Плоская карта
            }

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        /// <summary>
        /// Метод генерации карты шумов перлина в двумерном пространстве.
        /// </summary>
        /// <param name="seed">Семя генерации.</param>
        /// <param name="size">Длина и ширина карты.</param>
        /// <param name="scale">Показатель приближения.</param>
        /// <param name="dx">Смещение от начала координат по оси oX.</param>
        /// <param name="dy">Смещение от начала координат по оси oY.</param>
        /// <param name="octaves">Количество октав.</param>
        /// <param name="persistence">Устойчивость к наложению октав.</param>
        /// <returns>Карта двумерного шумов Перлина.</returns>
        public static double[] NoiseMap_simple2d(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {
            var map = new double[size * size];
            var perlin = new PerlinFin(seed, octaves, persistence);
            stopwatch.Start();
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale; // y с учетом смещения и приближения карты
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                    map[index] = perlin.Noise(tx, ty); // Плоская карта
                    ++index;
                }
            }

            double localMax = double.NegativeInfinity;
            double localMin = double.PositiveInfinity;
            foreach (var v in map)
                if (v > localMax) localMax = v;
                else if (v < localMin) localMin = v;
            Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map);

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        /// <summary>
        /// Метод генерации двумерной карты шумов перлина в трехмерном пространстве.
        /// </summary>
        /// <param name="seed">Семя генерации.</param>
        /// <param name="size">Длина и ширина карты.</param>
        /// <param name="scale">Показатель приближения.</param>
        /// <param name="dx">Смещение от начала координат по оси oX.</param>
        /// <param name="dy">Смещение от начала координат по оси oY.</param>
        /// <param name="z">Координата, по которой берется двумерный срез шума..</param>
        /// <param name="octaves">Количество октав.</param>
        /// <param name="persistence">Устойчивость к наложению октав.</param>
        /// <returns>Карта шумов Перлина.</returns>
        public static double[] NoiseMap_simple3d(int seed, int size, double scale, int dx, int dy, float z,
        int octaves, double persistence)
        {
            var map = new double[size * size];
            var perlin = new PerlinFin(seed, octaves, persistence);
            stopwatch.Start();
            L = size / scale; // Длина окружности, сечения тородида
            R = L / TAU; // Радиус окружности, сечения тороида
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale; // y с учетом смещения и приближения карты
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                    map[index] = perlin.Noise(tx, ty, z); // Карта цилиндр
                    ++index;
                }
            }

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        /// <summary>
        /// Метод генерации двумерной карты шумов перлина, замкнутой по оси oX.
        /// </summary>
        /// <param name="seed">Семя генерации.</param>
        /// <param name="size">Длина и ширина карты.</param>
        /// <param name="scale">Показатель приближения.</param>
        /// <param name="dx">Смещение от начала координат по оси oX.</param>
        /// <param name="dy">Смещение от начала координат по оси oY.</param>
        /// <param name="octaves">Количество октав.</param>
        /// <param name="persistence">Устойчивость к наложению октав.</param>
        /// <returns>Карта шумов Перлина.</returns>
        public static double[] NoiseMap_looped3d(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {
            var map = new double[size * size];
            var perlin = new PerlinFin(seed, octaves, persistence);
            stopwatch.Start();
            L = size / scale; // Длина окружности, сечения тородида
            R = L / TAU; // Радиус окружности, сечения тороида
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale; // y с учетом смещения и приближения карты
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                    angle_a = TAU * tx / L; // Текущий угол поворота от координаты x
                    map[index] = perlin.Noise(R * Math.Cos(angle_a), R * Math.Sin(angle_a), ty); // Карта цилиндр
                    ++index;
                }
            }


            double localMax = double.NegativeInfinity;
            double localMin = double.PositiveInfinity;

            foreach (var v in map)
                if (v > localMax) localMax = v;
                else if (v < localMin) localMin = v;

            Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map);

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        /// <summary>
        /// Метод генерации двумерной карты шумов перлина, замкнутой по осям oX и oY.
        /// </summary>
        /// <param name="seed">Семя генерации.</param>
        /// <param name="size">Длина и ширина карты.</param>
        /// <param name="scale">Показатель приближения.</param>
        /// <param name="dx">Смещение от начала координат по оси oX.</param>
        /// <param name="dy">Смещение от начала координат по оси oY.</param>
        /// <param name="octaves">Количество октав.</param>
        /// <param name="persistence">Устойчивость к наложению октав.</param>
        /// <returns>Карта шумов Перлина.</returns>
        public static double[] NoiseMap_looped4d(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {
            var map = new double[size * size];
            var perlin = new PerlinFin(seed, octaves, persistence);
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
                    map[index] = perlin.Noise(R * Math.Cos(angle_a), // Карта тороид
                        R * Math.Sin(angle_a),
                        R * Math.Sin(angle_b),
                        R * Math.Cos(angle_b));
                    ++index;
                }
            }


            double localMax = double.NegativeInfinity;
            double localMin = double.PositiveInfinity;

            foreach (var v in map)
                if (v > localMax) localMax = v;
                else if (v < localMin) localMin = v;

            Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map);

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        /// <summary>
        /// Метод генерации карты шумов перлина в двумерном пространстве с применение деформации по областям (domain warping).
        /// </summary>
        /// <param name="seed">Семя генерации.</param>
        /// <param name="size">Длина и ширина карты.</param>
        /// <param name="scale">Показатель приближения.</param>
        /// <param name="dx">Смещение от начала координат по оси oX.</param>
        /// <param name="dy">Смещение от начала координат по оси oY.</param>
        /// <param name="mode"></param>
        /// <param name="octaves">Количество октав.</param>
        /// <param name="persistence">Устойчивость к наложению октав.</param>
        /// <returns>Карта двумерного шумов Перлина.</returns>
        public static double[] NoiseMap_domainWarped2D(int seed, int size, double scale, int dx, int dy,
        double mode,
        double domWaprParam11, double domWaprParam12,
        double domWaprParam21, double domWaprParam22,
        int octaves, double persistence)
        {
            var map = new double[size * size];
            var perlin = new PerlinFin(seed, octaves, persistence);
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
                    double xq = perlin.Noise(tx + domWaprParam11, ty + domWaprParam12);
                    double yq = perlin.Noise(tx + domWaprParam21, ty + domWaprParam22);
                    map[index] = perlin.Noise(tx + mode * xq, ty + mode * yq); // Плоская карта с домэин деформрмэтион
                    ++index;
                }
            }

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        /// <summary>
        /// Метод генерации карты шумов перлина в двумерном пространстве, замкнутой по оси oX,
        /// с применение деформации по областям (domain warping).
        /// </summary>
        /// <param name="seed">Семя генерации.</param>
        /// <param name="size">Длина и ширина карты.</param>
        /// <param name="scale">Показатель приближения.</param>
        /// <param name="dx">Смещение от начала координат по оси oX.</param>
        /// <param name="dy">Смещение от начала координат по оси oY.</param>
        /// <param name="mode"></param>
        /// <param name="octaves"></param>
        /// <param name="persistence"></param>
        /// <returns></returns>
        public static double[] NoiseMap_domainWarped3D(int seed, int size, double scale, int dx, int dy,
        double mode,
        double domWaprParam11, double domWaprParam12, double domWaprParam13,
        double domWaprParam21, double domWaprParam22, double domWaprParam23,
        double domWaprParam31, double domWaprParam32, double domWaprParam33,
        int octaves, double persistence)
        {
            var map = new double[size * size];
            var perlin = new PerlinFin(seed, octaves, persistence);
            stopwatch.Start();
            L = size / scale; // Длина окружности, сечения тородида
            R = L / TAU; // Радиус окружности, сечения тороида
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale; // y с учетом смещения и приближения карты
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                    angle_a = TAU * tx / L; // Текущий угол поворота от координаты x

                    double rx = R * Math.Cos(angle_a);
                    double ry = R * Math.Sin(angle_a);
                    double xq = perlin.Noise(rx + domWaprParam11, ry + domWaprParam12, ty + domWaprParam13);
                    double yq = perlin.Noise(rx + domWaprParam21, ry + domWaprParam22, ty + domWaprParam23);
                    double zq = perlin.Noise(rx + domWaprParam31, ry + domWaprParam32, ty + domWaprParam33);
                    map[index] = perlin.Noise(rx + mode * xq, ry + mode * yq, ty + mode * zq); // Карта цилиндр с домэин деформрмэтион
                    index++;
                }
            }

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        #endregion

        /// <summary>
        /// Функция нормализации массива величин от начального минимума и максимума к [0; 1].
        /// </summary>
        /// <param name="matrix">Нормализуемый массив.</param>
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
        /// <summary>
        /// Функция нормализации массива величин от заданных величин к [0; 1].
        /// </summary>
        /// <param name="matrix">Нормализуемый массив</param>
        /// <param name="min">Минимальное значение массива.</param>
        /// <param name="max">Максимальное значение массива</param>
        public static void NormalizeMatrix(ref double[] matrix, double min, double max)
        {
            double d = max - min;
            for (int i = 0; i < matrix.Length; i++)
                matrix[i] = (matrix[i] - min) / d;
        }

        /// <summary>
        ///     Функция генерирует карту высот по заданным параметрам.
        /// </summary>
        /// <param name="noiseMap">Карту шумов, на основе которой генерируется карта высот.</param>
        /// <param name="multiplier">Множитель высоты.</param>
        /// <param name="oceanLevel">Уровень океана.</param>
        /// <param name="extraborder">
        ///     Граница дополнительного изменения высоты. Значения выше границы будут увеличиваться, ниже - уменьшаться.
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
        /// <param name="equator">Доля радиуса экватора от длины карты.</param>
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
        /// <param name="reduction">Скорость уменьшения температуры с высотой (градусы / 1 ед. высоты).</param>
        /// <returns>Карта температур.</returns>
        public static int[] ModeTemperatureMap(int[] baseTemperatureMap, int[] heightMap, double reduction)
        {
            var temperatureMap = new int[baseTemperatureMap.Length];
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