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
        public void Different2DPerlinComprasion()
        {
            const int numberOfTests = 10000 * 10000;
            Perlin perlin1 = new Perlin(0);
            PerlinExp1 perlin2 = new PerlinExp1(0);
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
                arrayT[i] = rnd.NextDouble() * 100;
                arrayA[i] = rnd.NextDouble() * 100;
                arrayB[i] = rnd.NextDouble() * 100;
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

        [TestMethod]
        public void FloorPerfomanse()
        {
            const int numberOfTests = 30000000;
            Random rnd = new Random(0);
            Stopwatch sw = new Stopwatch();
            double[] array = new double[numberOfTests];
            int[] array1 = new int[numberOfTests];
            int[] array2 = new int[numberOfTests];
            int[] array3 = new int[numberOfTests];
            int[] array4 = new int[numberOfTests];
            int[] array5 = new int[numberOfTests];
            for (int i = 0; i < numberOfTests; i++)
            {
                array[i] = rnd.NextDouble() * 10000-5000;
            }

            int iv;
            double dv;

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                array1[i] = (int)Math.Floor(array[i]);
            sw.Stop();
            Console.WriteLine($"Math.Floor() {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
            {
                if (array[i] < 0 && array[i] % -1 != 0) array2[i] = (int)(array[i] - 1);
                else array2[i] = (int)array[i];
            }
            sw.Stop();
            Console.WriteLine($"Inline floor {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                array3[i] = ((array[i] < 0 && array[i] % -1 != 0)) ? (int)(array[i] - 1) : (int)array[i];
            sw.Stop();
            Console.WriteLine($"Inline ternary floor {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                array4[i] = Floor(array[i]);
            sw.Stop();
            Console.WriteLine($"MyFloor() {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            int MyLocalFloor(double x)
            {
                if (x < 0 && x % -1 != 0) return (int)(x - 1);
                else return (int)x;
            }

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                array5[i] = MyLocalFloor(array[i]);
            sw.Stop();
            Console.WriteLine($"MyLocalFloor() {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            CollectionAssert.AreEquivalent(array1, array2);
            CollectionAssert.AreEquivalent(array1, array3);
            CollectionAssert.AreEquivalent(array1, array4);
            CollectionAssert.AreEquivalent(array1, array5);
        }

        int Floor(double x) 
        {
            if (x < 0 && x % -1 != 0) return (int)(x - 1);
            else return (int)x;
        }

        [TestMethod]
        public void SmoothPerfomanse()
        {
            const int numberOfTests = 4000000;
            Random rnd = new Random(0);
            Stopwatch sw = new Stopwatch();
            double[] array = new double[numberOfTests];
            for (int i = 0; i < numberOfTests; i++)
            {
                array[i] = rnd.NextDouble();
            }

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                PerlinExp2.SmoothStep(array[i]);
            sw.Stop();
            Console.WriteLine($"SmoothStep {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");

            sw.Restart();
            for (int i = 0; i < numberOfTests; i++)
                PerlinExp2.QunticCurve(array[i]);
            sw.Stop();
            Console.WriteLine($"QunticCurve {numberOfTests} чисел за {sw.ElapsedMilliseconds}мс.");
        }
    }
}
