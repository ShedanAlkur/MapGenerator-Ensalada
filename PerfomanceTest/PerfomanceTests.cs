using Microsoft.VisualStudio.TestTools.UnitTesting;
using Noise.Perlin;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PerfomanceTest
{
    [TestClass]
    public class PerfomanceTests
    {
        [TestMethod]
        public void PerlinDifferentDimensionsComprasion()
        {
            const int numberOfTests = 1000 * 1000;
            Perlin perlin = new Perlin(0);
            Stopwatch sw = new Stopwatch();
            string res;

            sw.Restart();
            for (double x = 0; x < numberOfTests; x++)
                perlin.Noise(x, x);
            sw.Stop();
            res = $"На генерацию 2D шума в {numberOfTests} клетках потребовалось {sw.ElapsedMilliseconds}мс.";
            Console.WriteLine(res);

            sw.Restart();
            for (double x = 0; x < numberOfTests; x++)
                perlin.Noise(x, x, x);
            sw.Stop();
            res = $"На генерацию 3D шума в {numberOfTests} клетках потребовалось {sw.ElapsedMilliseconds}мс.";
            Console.WriteLine(res);

            sw.Restart();
            for (double x = 0; x < numberOfTests; x++)
                perlin.Noise(x, x, x, x);
            sw.Stop();
            res = $"На генерацию 4D шума в {numberOfTests} клетках потребовалось {sw.ElapsedMilliseconds}мс.";
            Console.WriteLine(res);
        }

        [TestMethod]        
        public void FloorComprasion()
        {
            const int numberOfTests = 100000;
            Random rnd = new Random(0);
            Stopwatch sw = new Stopwatch();
            double[] array1 = new double[numberOfTests];
            double[] array2 = new double[numberOfTests];
            for (int i = 0; i < numberOfTests; i++)
            {
                array1[i] = rnd.NextDouble() * 100;
                array2[i] = array1[i];
            }

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                array1[i] = Math.Floor(array1[i]);
            sw.Stop();
            Console.WriteLine($"Округлено с помощью Math.Floor() {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                array2[i] = (int)array2[i];
            sw.Stop();
            Console.WriteLine($"Округлено с помощью (int) {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            CollectionAssert.AreEquivalent(array1, array2);
        } // Лучше использовать (int)

        [TestMethod]
        public void Different2DPerlinComprasion()
        {
            const int numberOfTests = 10000 * 10000;
            Perlin perlin1 = new Perlin(0);
            PerlinExperimental1 perlin2 = new PerlinExperimental1(0);
            Stopwatch sw = new Stopwatch();
            string res;

            sw.Restart();
            for (double x = 0; x < numberOfTests; x++)
                perlin1.Noise(x, x);
            sw.Stop();
            res = $"Based Perlin в {numberOfTests} клетках потребовалось {sw.ElapsedMilliseconds}мс.";
            Console.WriteLine(res);

            sw.Restart();
            for (double x = 0; x < numberOfTests; x++)
                perlin2.Noise(x, x);
            sw.Stop();
            res = $"Exp1 Perlin в {numberOfTests} клетках потребовалось {sw.ElapsedMilliseconds}мс.";
            Console.WriteLine(res);
        }

        [TestMethod]
        public void LerpFunctionPerfomance()
        {
            const int numberOfTests = 1000000;
            Random rnd = new Random(0);
            Stopwatch sw = new Stopwatch();
            double[] arrayT = new double[numberOfTests];
            double[] arrayA = new double[numberOfTests];
            double[] arrayB = new double[numberOfTests];
            for (int i = 0; i < numberOfTests; i++)
            {
                arrayT[i] = rnd.NextDouble()*100;
                arrayA[i] = rnd.NextDouble()*100;
                arrayB[i] = rnd.NextDouble()*100;
            }
            double c;

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                c = Lerp1(arrayT[i], arrayA[i], arrayB[i]);
            sw.Stop();
            Console.WriteLine($"Lerp обыкновенная {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                c = Lerp2(arrayT[i], arrayA[i], arrayB[i]);
            sw.Stop();
            Console.WriteLine($"Lerp inline {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                c = arrayA[i] + arrayT[i] * (arrayB[i] - arrayA[i]);
            sw.Stop();
            Console.WriteLine($"Lerp буквально inline {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            double Lerp3(double t, double a, double b) => a + t * (b - a);
            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                c = Lerp3(arrayT[i], arrayA[i], arrayB[i]);
            sw.Stop();
            Console.WriteLine($"Lerp локальный{numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");
        } // Лучше писать вычисления вместо вызова функции lerp

    private static double Lerp1(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Lerp2(double t, double a, double b)
        {
            return a + t * (b - a);
        }
    }
}
