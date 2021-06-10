using Microsoft.VisualStudio.TestTools.UnitTesting;
using Noise.Perlin;
using System;
using System.Diagnostics;

namespace PerfomanceTest
{
    [TestClass]
    public class PerfomanceTests
    {
        [TestMethod]
        public void PerlinDifferentDimensionsComprasion()
        {
            const int numberOfTests = 1000*1000;
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
        }

        [TestMethod]
        public void Different2DPerlinComprasion()
        {

        }
    }
}
