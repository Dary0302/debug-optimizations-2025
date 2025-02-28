using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace JPEG;

public class DCT
{
    public static double[,] DCT2D(float[,] channel, float[,] cosTable, int blockSize)
    {
        var channelFreqs = new double[blockSize, blockSize];

        for (var u = 0; u < blockSize; u++)
        {
            for (var v = 0; v < blockSize; v++)
            {
                var sum = 0f;

                for (var x = 0; x < blockSize; x++)
                {
                    for (var y = 0; y < blockSize; y++)
                    {
                        var pixel = channel[x, y];
                        sum += pixel *
                            cosTable[x, u] *
                            cosTable[y, v];
                    }
                }

                var alphaU = Alpha(u);
                var alphaV = Alpha(v);

                channelFreqs[u, v] = 0.25 * alphaU * alphaV * sum; // 0.25 это (1/4), сокращает коэффициенты сразу
            }
        }

        return channelFreqs;
    }

    public static float[,] PrecomputeCosTable(int size)
    {
        var table = new float[size, size];
        for (var x = 0; x < size; x++)
        {
            for (var u = 0; u < size; u++)
            {
                table[x, u] = (float)Math.Cos((2.0 * x + 1.0) * u * Math.PI / (2.0 * size));
            }
        }
        return table;
    }

    public static void IDCT2D(float[,] input, float[,] output, float[,] cosTable, int blockSize)
    {
        var width = input.GetLength(0);
        var height = input.GetLength(1);

        Parallel.For(0, width / blockSize, blockX =>
        {
            for (var blockY = 0; blockY < height / blockSize; blockY++)
            {
                ProcessBlock(input, output, blockX * blockSize, blockY * blockSize, blockSize, cosTable);
            }
        });
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessBlock(
        float[,] input,
        float[,] output,
        int offsetX,
        int offsetY,
        int blockSize,
        float[,] cosTable)
    {
        var beta = 2f * (1f / blockSize);

        for (var x = 0; x < blockSize; x++)
        {
            for (var y = 0; y < blockSize; y++)
            {
                var sum = 0f;
                for (var u = 0; u < blockSize; u++)
                {
                    var alphaU = Alpha(u);
                    for (var v = 0; v < blockSize; v++)
                    {
                        var alphaV = Alpha(v);
                        sum += alphaU *
                            alphaV *
                            input[offsetX + u, offsetY + v] *
                            cosTable[x, u] *
                            cosTable[y, v];
                    }
                }
                output[offsetX + x, offsetY + y] = sum * beta;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Alpha(int u)
    {
        if (u == 0)
            return 0.70710678118654752440084436210485f;
        return 1;
    }
}