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

        /// <summary>
        /// Таймер для замера двительности генерации карты.
        /// </summary>
        static readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// Текущая координата по оси oX, соответствующая рассматриваемой клетке карты с учетом масштаба и смещения.
        /// </summary>
        static double tx;

        /// <summary>
        /// Текущая координата по оси oY, соответствующая рассматриваемой клетке карты с учетом масштаба и смещения.
        /// </summary>
        static double ty;

        /// <summary>
        /// Длина окружности, сечения тородида.
        /// </summary>
        static double lenght;

        /// <summary>
        /// Радиус окружности, сечения тороида.
        /// </summary>
        static double radius;

        /// <summary>
        /// Угол для взятия проекции значения шума по координате <c>tx</c> из цилиндра в трехмерном шуме / из сечения тороида в трехмерном шуме.
        /// </summary>
        static double angleX;

        /// <summary>
        /// Угол для взятия проекции значения шума по координате <c>ty</c> из тороида в четырехмерном шуме.
        /// </summary>
        static double angleY;

        /// <summary>
        /// Текущая клетка карты по оси oX. Используется в счетчиках циклов.
        /// </summary>
        static int x;

        /// <summary>
        /// Текущая клетка карты по оси oY. Используется в счетчиках циклов.
        /// </summary>
        static int y;

        /// <summary>
        /// Индекс рассматриваемой ячейки карты.
        /// </summary>
        static int index;

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
            var perlin = new Perlin(seed, octaves, persistence);
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
            var perlin = new Perlin(seed, octaves, persistence);
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
            //PrintMinMaxValueOfMatrix(map);
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
            var perlin = new Perlin(seed, octaves, persistence);
            stopwatch.Start();
            lenght = size / scale; // Длина окружности, сечения тородида
            radius = lenght / TAU; // Радиус окружности, сечения тороида
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
            //PrintMinMaxValueOfMatrix(map);
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
            var perlin = new Perlin(seed, octaves, persistence);
            stopwatch.Start();
            lenght = size / scale; // Длина окружности, сечения тородида
            radius = lenght / TAU; // Радиус окружности, сечения тороида
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale;
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale;
                    angleX = TAU * tx / lenght;
                    map[index] = perlin.Noise(radius * Math.Cos(angleX), radius * Math.Sin(angleX), ty); // Карта цилиндр
                    ++index;
                }
            }
            PrintMinMaxValueOfMatrix(map);
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
            var perlin = new Perlin(seed, octaves, persistence);
            stopwatch.Start();
            lenght = size / scale; // Длина окружности, сечения тородида.
            radius = lenght / TAU; // Радиус окружности, сечения тороида.
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale;
                angleY = TAU * ty / lenght;
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale;
                    angleX = TAU * tx / lenght;
                    map[index] = perlin.Noise(radius * Math.Cos(angleX), // Карта тороид
                        radius * Math.Sin(angleX),
                        radius * Math.Sin(angleY),
                        radius * Math.Cos(angleY));
                    ++index;
                }
            }
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
            var perlin = new Perlin(seed, octaves, persistence);
            stopwatch.Start();
            lenght = size / scale; // Длина окружности, сечения тородида
            radius = lenght / TAU; // Радиус окружности, сечения тороида
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale; // y с учетом смещения и приближения карты
                angleY = TAU * ty / lenght; // Текущий угол поворота от координаты y
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                    angleX = TAU * tx / lenght; // Текущий угол поворота от координаты x
                    double xq = perlin.Noise(tx + domWaprParam11, ty + domWaprParam12);
                    double yq = perlin.Noise(tx + domWaprParam21, ty + domWaprParam22);
                    map[index] = perlin.Noise(tx + mode * xq, ty + mode * yq); // Плоская карта со смещением по областям.
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
            var perlin = new Perlin(seed, octaves, persistence);
            stopwatch.Start();
            lenght = size / scale; // Длина окружности, сечения тородида
            radius = lenght / TAU; // Радиус окружности, сечения тороида
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale; // y с учетом смещения и приближения карты
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                    angleX = TAU * tx / lenght; // Текущий угол поворота от координаты x

                    double rx = radius * Math.Cos(angleX);
                    double ry = radius * Math.Sin(angleX);
                    double xq = perlin.Noise(rx + domWaprParam11, ry + domWaprParam12, ty + domWaprParam13);
                    double yq = perlin.Noise(rx + domWaprParam21, ry + domWaprParam22, ty + domWaprParam23);
                    double zq = perlin.Noise(rx + domWaprParam31, ry + domWaprParam32, ty + domWaprParam33);
                    map[index] = perlin.Noise(rx + mode * xq, ry + mode * yq, ty + mode * zq); // Карта цилиндр со смещением по областям.
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
        ///  Функция выводит в консоль минимальное и максимальное значение заданной матрица.
        /// </summary>
        /// <param name="matrix">Матрица, в которой осуществляется поиск минимального и максимального значений.</param>
        private static void PrintMinMaxValueOfMatrix(double[] matrix)
        {
            double max = double.NegativeInfinity;
            double min = double.PositiveInfinity;

            foreach (var v in matrix)
                if (v > max) max = v;
                else if (v < min) min = v;

            Console.WriteLine($"max={max}; min={min}");
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