using Noise.Perlin;
using System;
using System.Diagnostics;

namespace MapGenerator
{
    public enum NoiseMapType
    {
        testedA,
        testedB,
        testedC,
        testedD,
        simple1d,
        simple2d,
        domainWarped2D,
        domainWarped3D,
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
        const double TAU = 2 * Math.PI;

        static readonly Stopwatch stopwatch = new Stopwatch();
        static double min, max, r;
        static double tx, ty, L, R;
        static double angle_a, angle_b;
        static int x, y, index;

        #region Генерация карт шумов

        public static double[] NoiseMap_testedA( int seed, int size, double scale, int dx = 0, int dy = 0,
        int octaves = 3, double persistence = 0.5f)
        {
            var map = new double[size * size];
            var perlin = new PerlinExp1(seed, 2, octaves, persistence);
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

            NormalizeMatrix(ref map, -1, 1); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию NoiseMap_testedA: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        public static double[] NoiseMap_testedB(int seed, int size, double scale, int dx = 0, int dy = 0,
        int octaves = 3, double persistence = 0.5f)
        {
            double octaveScale = (2 - Math.Pow(2, 1 - octaves));
            var map = new double[size * size];
            var perlin = new PerlinExp1(seed, 3, octaves, persistence);
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


                    //map[index] = perlin.Noise(R * Math.Cos(angle_a), R * Math.Sin(angle_a), ty); // Карта цилиндр
                    map[index] = perlin.Noise(R * Math.Cos(angle_a), R * Math.Sin(angle_a), ty); // Плоская карта


                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            int dimension = 2;
            max = 0.5 * Math.Sqrt(dimension);
            double scaleFactor = 2 * Math.Pow(dimension, -0.5);

            double localMax = double.NegativeInfinity;
            double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map.Length; i++)
            //    map[i] *= scaleFactor / octaveScale;

            foreach (var v in map)
                if (v > localMax) localMax = v;
                else if (v < localMin) localMin = v;

            Console.WriteLine($"max?={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию NoiseMap_testedB: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        public static double[] NoiseMap_testedC(int seed, int size, double scale, int dx = 0, int dy = 0,
        int octaves = 3, double persistence = 0.5f)
        {
            var map = new double[size * size];
            var perlin = new PerlinExp1(seed, 4, octaves, persistence);
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

                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            //int dimension = 2;
            //max = 0.5 * Math.Sqrt(dimension);
            //double scaleFactor = 2 * Math.Pow(dimension, -0.5);
            //double octaveFactor = 2 - Math.Pow(2, 1 - octaves);

            //double localMax = double.NegativeInfinity;
            //double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map.Length; i++)
            //    map[i] /= octaveFactor;

            //foreach (var v in map)
            //    if (v > localMax) localMax = v;
            //    else if (v < localMin) localMin = v;

            //Console.WriteLine($"max?={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию NoiseMap_testedC: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        public static double[] NoiseMap_testedD(int seed, int size, double scale, int dx = 0, int dy = 0,
        int octaves = 3, double persistence = 0.5f)
        {
            var map = new double[size * size];
            var perlin = new OldPerlin(seed);
            stopwatch.Start();
            for (y = 0; y < size; y++)
            {
                index = y * size;
                ty = (y + dy) / scale; // y с учетом смещения и приближения карты
                for (x = 0; x < size; x++)
                {
                    tx = (x + dx) / scale; // x с учетом смещение и приближения карты
                    map[index] = perlin.Noise((float)tx, (float)ty, octaves); // Плоская карта
                    ++index;
                }
            }

            int dimension = 2;
            max = 0.5 * Math.Sqrt(dimension);
            double scaleFactor = 2 * Math.Pow(dimension, -0.5);
            double octaveFactor = 2 - Math.Pow(2, 1 - octaves);

            double localMax = double.NegativeInfinity;
            double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map.Length; i++)
            //    map[i] /= octaveFactor;

            foreach (var v in map)
                if (v > localMax) localMax = v;
                else if (v < localMin) localMin = v;

            Console.WriteLine($"max?={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию NoiseMap_testedC: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
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
        public static double[] NoiseMap(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {

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

                    //map[index] = perlin.Noise(tx, ty, octaves, persistence); // Плоская карта

                    //x +noise(x + ?,y + ?), y + noise(x + ?,y + ?) // domain warped noise

                    // Удалить
                    //map[index] = perlin.Noise(
                    //    tx + 4* perlin.Noise(tx + persistence, ty + d2), ty + 4*perlin.Noise(tx + d3, ty + d4),
                    //    octaves);

                    //double xq = perlin.Noise(tx + 1.1, ty + 5.2, octaves);
                    //double yq = perlin.Noise(tx + 5.2, ty + 1.3, octaves);
                    //double mode = 4.0;
                    //map[index] = perlin.Noise(tx + mode * xq, ty + mode * yq, octaves); // Плоская карта с домэин деформрмэтион


                    map[index] = perlin.Noise(R * Math.Cos(angle_a), R * Math.Sin(angle_a), ty, octaves, persistence); // Карта цилиндр

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

            /*
            Базовый диапазое шума и 1 октаве в n-мерном пространстве
            f(n) = +-0.5*sqrt(n)

            При 1/2/3/4 октавах происходит увеличение на 0/50/75/87.5%
            g(o) = 2 - 2^(1-o)

            Итоговый диапазон f(n)*g(o) = (+-0.5*sqrt(n))*(2 - 2^(1-o)) = 
             */

            //int dimension = 3;
            //max = 0.5 * Math.Sqrt(dimension); // / (2 - Math.Pow(2, 1 - octaves));

            //double localMax = double.NegativeInfinity;
            //double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map[i]; i++)
            //    map[i] *= (2 - Math.Pow(2, 1 - octaves));

            //foreach (var v in map)
            //    if (v > localMax) localMax = v;
            //    else if (v < localMin) localMin = v;

            //Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }
        public static double[] NoiseMap_simple1d(int seed, int size, double scale, int dx, int dy,
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
                //if (yValue < 0 || yValue > size) continue;
                map[(halfSize - yValue)*size + x] = 1; // Плоская карта
            }

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }
        public static double[] NoiseMap_simple2d(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {
            //double preadictedMin = -0.5 * Math.Sqrt(2);

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

                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            /*
            Базовый диапазое шума и 1 октаве в n-мерном пространстве
            f(n) = +-0.5*sqrt(n)

            При 1/2/3/4 октавах происходит увеличение на 0/50/75/87.5%
            g(o) = 2 - 2^(1-o)

            Итоговый диапазон f(n)*g(o) = (+-0.5*sqrt(n))*(2 - 2^(1-o)) = 
             */

            //int dimension = 3;
            //max = 0.5 * Math.Sqrt(dimension); // / (2 - Math.Pow(2, 1 - octaves));

            //double localMax = double.NegativeInfinity;
            //double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map[i]; i++)
            //    map[i] *= (2 - Math.Pow(2, 1 - octaves));

            //foreach (var v in map)
            //    if (v > localMax) localMax = v;
            //    else if (v < localMin) localMin = v;

            //Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }
        public static double[] NoiseMap_simple3d(int seed, int size, double scale, int dx, int dy, float z,
        int octaves, double persistence)
        {
            //double preadictedMin = -0.5 * Math.Sqrt(2);

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


                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            /*
            Базовый диапазое шума и 1 октаве в n-мерном пространстве
            f(n) = +-0.5*sqrt(n)

            При 1/2/3/4 октавах происходит увеличение на 0/50/75/87.5%
            g(o) = 2 - 2^(1-o)

            Итоговый диапазон f(n)*g(o) = (+-0.5*sqrt(n))*(2 - 2^(1-o)) = 
             */

            //int dimension = 3;
            //max = 0.5 * Math.Sqrt(dimension); // / (2 - Math.Pow(2, 1 - octaves));

            //double localMax = double.NegativeInfinity;
            //double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map[i]; i++)
            //    map[i] *= (2 - Math.Pow(2, 1 - octaves));

            //foreach (var v in map)
            //    if (v > localMax) localMax = v;
            //    else if (v < localMin) localMin = v;

            //Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }
        public static double[] NoiseMap_looped3d(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {
            //double preadictedMin = -0.5 * Math.Sqrt(2);

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

                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            /*
            Базовый диапазое шума и 1 октаве в n-мерном пространстве
            f(n) = +-0.5*sqrt(n)

            При 1/2/3/4 октавах происходит увеличение на 0/50/75/87.5%
            g(o) = 2 - 2^(1-o)

            Итоговый диапазон f(n)*g(o) = (+-0.5*sqrt(n))*(2 - 2^(1-o)) = 
             */

            //int dimension = 3;
            //max = 0.5 * Math.Sqrt(dimension); // / (2 - Math.Pow(2, 1 - octaves));

            //double localMax = double.NegativeInfinity;
            //double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map[i]; i++)
            //    map[i] *= (2 - Math.Pow(2, 1 - octaves));

            //foreach (var v in map)
            //    if (v > localMax) localMax = v;
            //    else if (v < localMin) localMin = v;

            //Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }
        public static double[] NoiseMap_looped4d(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {
            //double preadictedMin = -0.5 * Math.Sqrt(2);

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

                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            /*
            Базовый диапазое шума и 1 октаве в n-мерном пространстве
            f(n) = +-0.5*sqrt(n)

            При 1/2/3/4 октавах происходит увеличение на 0/50/75/87.5%
            g(o) = 2 - 2^(1-o)

            Итоговый диапазон f(n)*g(o) = (+-0.5*sqrt(n))*(2 - 2^(1-o)) = 
             */

            //int dimension = 3;
            //max = 0.5 * Math.Sqrt(dimension); // / (2 - Math.Pow(2, 1 - octaves));

            //double localMax = double.NegativeInfinity;
            //double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map[i]; i++)
            //    map[i] *= (2 - Math.Pow(2, 1 - octaves));

            //foreach (var v in map)
            //    if (v > localMax) localMax = v;
            //    else if (v < localMin) localMin = v;

            //Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }
        public static double[] NoiseMap_domainWarped2D(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {
            //double preadictedMin = -0.5 * Math.Sqrt(2);

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


                    double xq = perlin.Noise(tx + 1.1, ty + 5.2);
                    double yq = perlin.Noise(tx + 5.2, ty + 1.3);
                    double mode = 4.0;
                    map[index] = perlin.Noise(tx + mode * xq, ty + mode * yq); // Плоская карта с домэин деформрмэтион

                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            /*
            Базовый диапазое шума и 1 октаве в n-мерном пространстве
            f(n) = +-0.5*sqrt(n)

            При 1/2/3/4 октавах происходит увеличение на 0/50/75/87.5%
            g(o) = 2 - 2^(1-o)

            Итоговый диапазон f(n)*g(o) = (+-0.5*sqrt(n))*(2 - 2^(1-o)) = 
             */

            //int dimension = 3;
            //max = 0.5 * Math.Sqrt(dimension); // / (2 - Math.Pow(2, 1 - octaves));

            //double localMax = double.NegativeInfinity;
            //double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map[i]; i++)
            //    map[i] *= (2 - Math.Pow(2, 1 - octaves));

            //foreach (var v in map)
            //    if (v > localMax) localMax = v;
            //    else if (v < localMin) localMin = v;

            //Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }
        public static double[] NoiseMap_domainWarped3D(int seed, int size, double scale, int dx, int dy,
        int octaves, double persistence)
        {
            //double preadictedMin = -0.5 * Math.Sqrt(2);

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
                    double xq = perlin.Noise(rx + 1.1, ry + 5.2, ty + 1.2);
                    double yq = perlin.Noise(rx + 5.2, ry + 1.3, ty + 2.4);
                    double zq = perlin.Noise(rx + 3.2, ry + 2.3, ty + 4.1);
                    double mode = 4.0;
                    map[index] = perlin.Noise(rx + mode * xq, ry + mode * yq, ty + mode * zq); // Карта цилиндр с домэин деформрмэтион


                    //if (map[index] < min)
                    //    min = map[index]; // Сохраняем минимальные и максимальные значения для приведения шумов к [0, 1]
                    //if (map[index] > max) max = map[index];
                    ++index;
                }
            }

            /*
            Базовый диапазое шума и 1 октаве в n-мерном пространстве
            f(n) = +-0.5*sqrt(n)

            При 1/2/3/4 октавах происходит увеличение на 0/50/75/87.5%
            g(o) = 2 - 2^(1-o)

            Итоговый диапазон f(n)*g(o) = (+-0.5*sqrt(n))*(2 - 2^(1-o)) = 
             */

            //int dimension = 3;
            //max = 0.5 * Math.Sqrt(dimension); // / (2 - Math.Pow(2, 1 - octaves));

            //double localMax = double.NegativeInfinity;
            //double localMin = double.PositiveInfinity;

            //for (int i = 0; i < map[i]; i++)
            //    map[i] *= (2 - Math.Pow(2, 1 - octaves));

            //foreach (var v in map)
            //    if (v > localMax) localMax = v;
            //    else if (v < localMin) localMin = v;

            //Console.WriteLine($"max={max}; localMax={localMax}; localMin={localMin}");

            NormalizeMatrix(ref map); // Сводим карту шумов от диапазона [-1; +1] (почти) к [0; 1]

            stopwatch.Stop();
            Console.WriteLine("Время, ушедшее на генерацию карты шумов: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            return map;
        }

        #endregion

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