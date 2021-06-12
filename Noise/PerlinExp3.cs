using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Noise.Perlin
{
    public class PerlinExp3
    {
        Random rnd;
        int seed;
        int dimension;
        int octaves;
        double persistance;

        int[][] gradientVector;

        public PerlinExp3(int seed, int dimension, int octaves, double persistance = 0.5)
        {
            this.seed = seed;
            this.dimension = dimension;
            this.octaves = octaves;
            this.persistance = persistance;
            rnd = new Random(this.seed);

            CreateGradientVector();

        }

        void CreateGradientVector()
        {
            gradientVector = new int[1 << (dimension + 1)][];
            int zeroE = -1;
            int sign = 0;
            int signIndex;
            int v = 1 << (dimension - 1);
            double vecLenght = Math.Sqrt(1.0 / dimension);
            for (int i = 0; i < v * dimension; i++)
            {
                if ((i & (v - 1)) == 0) zeroE++;
                gradientVector[i] = new int[dimension];
                signIndex = 0;
                for (int j = 0; j < dimension; j++)
                {
                    if (j == zeroE) continue;
                    gradientVector[i][j] = (((sign >> signIndex) & 0b1) == 0) ? -1 : 1;
                    signIndex++;
                }
                sign++;
            }
            for (int i = v * dimension; i < gradientVector.Length; i++)
                gradientVector[i] = gradientVector[(i * 13631) & ((v * dimension) - 1)];
        }

        static double SmoothStep(double t) => t * t * (3 - 2 * t);
        static double QunticCurve(double t)=> t * t * t * (t * (t * 6 - 15) + 10);

    }
}
