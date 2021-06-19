using System;

namespace Noise.Perlin
{
    /// <summary>
    /// Класс для генерации шума Перлина в пространстве произвольной размерности.
    /// </summary>
    public class PerlinMultidimensional
    {
        int v;
        int[] magical;
        int[] vertex;
        double[] dotProduct;
        double[] pointInQuad;
        double[] tempVector;

        double res;
        readonly double[] permutationTable;
        int seed;
        public int Seed
        {
            get => seed;
            set
            {
                seed = value;
                if (dimension > 0) FillMagicalArray();
            }
        }
        int dimension;
        public int Dimension
        {
            get => dimension;
            set
            {
                dimension = value;
                maxNoiseValue = 0.5 * Math.Sqrt(dimension);
                magical = new int[dimension + 1];
                FillMagicalArray();
                vertex = new int[dimension];
                pointInQuad = new double[dimension];
                dotProduct = new double[1 << dimension];
                tempVector = new double[dimension];
            }
        }
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
        double Persistence
        {
            get => persistence; set
            {
                persistence = value;
                octaveFactor = 2 - Math.Pow(persistence, octave - 1);
            }
        }

        double maxNoiseValue;
        double octaveFactor;
        private int k, j, l, m, n, dotFindingStep;
        private double amplitude;

        /// <summary>
        /// Инициализирует новый экземпляр класса PerlinFin с помощью указанных начальных значений.
        /// </summary>
        /// <param name="seed">Семя генерации шума.</param>
        /// <param name="dimension">Размерность пространства, в котором генерируется шум.</param>
        /// <param name="octave">Количество октав, которое рассчитывается для шума в каждой точке.</param>
        /// <param name="persistence">Устойчивость к наложению октав. Больше величина - сильнее влияние октав.</param>
        public PerlinMultidimensional(int seed, int dimension, int octave, double persistence = 0.5)
        {
            this.seed = seed;
            this.Dimension = dimension;
            this.Octave = octave;
            this.Persistence = persistence;

            var rnd = new Random(seed);
            permutationTable = new double[1024];
            for (var i = 0; i < permutationTable.Length; i++)
                permutationTable[i] = rnd.NextDouble() * 2 - 1;
        }


        #region Вспомогательные функции

        public static double Lerp(double x, double a, double b) => a + x * (b - a);

        public static double CosineInterpolation(double x, double a, double b)
        {
            x = (1 - Math.Cos(x * Math.PI)) * 0.5;
            return a * (1 - x) + b * x;
        }

        // x^2*(3-2x)
        public static double SmoothStep(double t) => t * t * (3 - 2 * t);

        // 6x^5 - 15x^4 + 10x^3
        public static double SmootherStep(double t) => 6 * t * t * t * t * t - 15 * t * t * t * t + 10 * t * t * t;

        public static double QunticCurve(double t) => t * t * t * (t * (t * 6 - 15) + 10);

        static void NormalizeVector(ref double[] vector)
        {
            double vectorLenght = 0;
            for (byte i = 0; i < vector.Length; i++)
                vectorLenght += vector[i] * vector[i];
            vectorLenght = Math.Sqrt(vectorLenght);
            for (byte i = 0; i < vector.Length; i++)
                vector[i] = vector[i] / vectorLenght;
        }

        void FillMagicalArray()
        {
            var rnd = new Random(seed);
            for (int i = 0; i < magical.Length; i++)
                magical[i] = rnd.Next(int.MaxValue);
        }

        #endregion

        /// <summary>
        /// Функция генерации шума по заданным координатам.
        /// </summary>
        /// <param name="coords">Координаты генерации шума. Количество параметров должно соответствовать выбранной размерности пространства.</param>
        /// <returns>Значение шума в заданных координатах. Находится в диапазоне [-1; 1].</returns>
        public double Noise(params double[] coords)
        {
            res = 0;
            amplitude = 1;
            for (k = 0; k < octave; k++)
            {
                // Координаты узла, к которому относится точка
                for (j = 0; j < dimension; j++)
                {
                    vertex[j] = (int)Math.Floor(coords[j]);
                    pointInQuad[j] = coords[j] - vertex[j];
                }

                // Скалярные произведения в вершинах
                for (j = 0; j < dotProduct.Length; j ++)
                {
                    // Коэффициент псевдорандома
                    v = seed * magical[0];
                    for (this.l = dimension - 1; this.l >= 0; this.l--)
                        v = v ^ ((((j >> this.l) & 0b1) == 0 ? vertex[this.l] : vertex[this.l] + 1) * magical[this.l + 1]);
                    // Градиентный единичный вектор
                    for (this.l = 0; this.l < dimension; this.l++)
                        tempVector[this.l] = permutationTable[(v * (this.l + 1)) & 1023];
                    NormalizeVector(ref tempVector);
                    // Скалярные произведения в вершинах
                    dotProduct[j] = 0;
                    for (this.l = dimension - 1; this.l >= 0; this.l--) 
                    {
                        dotProduct[j] += (((j >> this.l) & 0b1) == 0 ? pointInQuad[this.l] : pointInQuad[this.l] - 1) * tempVector[this.l];                        
                    }
                }

                // Подготовка параметров интерполяции
                for (j = 0; j < pointInQuad.Length; j++)
                    pointInQuad[j] = QunticCurve(pointInQuad[j]);

                // Линейная интерполяция между всеми противоположными точками/сторонами квадрата
                dotFindingStep = 1;
                l = 0;
                while (dotFindingStep < (1 << (dimension )))
                {
                    for (j = 0; j < dotProduct.Length; j += dotFindingStep << 1)
                        dotProduct[j] = Lerp(pointInQuad[l], dotProduct[j], dotProduct[j + dotFindingStep]);
                    dotFindingStep <<= 1;
                    l++;
                }

                // Применение шума в текущей октаве, подготовка к вычислению следующей октавы
                res += dotProduct[0] * amplitude;
                amplitude *= persistence;
                for (j = 0; j < dimension; j++)
                    coords[j] /= persistence;

            }
            return res / octaveFactor / maxNoiseValue;
        }

    }
}