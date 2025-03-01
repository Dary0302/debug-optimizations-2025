using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace JPEG;

public class DCT
{
    public static float[,] DCT2D(float[,] channel, float[,] cosTable, byte blockSize)
    {
        var beta = 1f / blockSize * 1.9f;
        var channelFreqs = new float[blockSize, blockSize];

        var temp = new float[blockSize, blockSize];

        Parallel.For(0, blockSize, y =>
        {
            for (byte u = 0; u < blockSize; u++)
            {
                var alphaU = Alpha(u);
                var sum = 0.0f;

                for (byte x = 0; x < blockSize; x++)
                {
                    sum += channel[x, y] * cosTable[x, u];
                }

                temp[u, y] = sum * alphaU;
            }
        });

        Parallel.For(0, blockSize, u =>
        {
            for (byte v = 0; v < blockSize; v++)
            {
                var alphaV = Alpha(v);

                var sum = 0.0f;

                for (byte y = 0; y < blockSize; y++)
                {
                    sum += temp[u, y] * cosTable[y, v];
                }

                channelFreqs[u, v] = sum * beta * alphaV;
            }
        });

        return channelFreqs;
    }


    public static float[,] PrecomputeCosTable(byte size)
    {
        var table = new float[size, size];
        for (byte x = 0; x < size; x++)
        {
            for (byte u = 0; u < size; u++)
            {
                table[x, u] = (float)Math.Cos((2.0 * x + 1.0) * u * Math.PI / (2.0 * size));
            }
        }
        return table;
    }

    public static void IDCT2D(float[,] input, float[,] output, float[,] cosTable, byte blockSize)
{
    var beta = 1f / blockSize * 1.4f; 
    var temp = new float[blockSize, blockSize];

    Parallel.For(0, blockSize, x =>
    {
        for (byte v = 0; v < blockSize; v++)
        {
            var sum = 0.0f;

            for (byte u = 0; u < blockSize; u++)
            {
                var alphaU = Alpha(u);
                sum += input[u, v] * alphaU * cosTable[x, u];
            }

            temp[x, v] = sum;
        }
    });

    Parallel.For(0, blockSize, x =>
    {
        for (byte y = 0; y < blockSize; y++)
        {
            var sum = 0.0f;

            for (byte v = 0; v < blockSize; v++)
            {
                sum += temp[x, v] * cosTable[y, v];
            }

            output[x, y] = sum * beta;
        }
    });
}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Alpha(int u)
    {
        if (u == 0)
            return 0.70710678118654752440084436210485f;
        return 1;
    }
}