using System.Drawing;
using System.Drawing.Imaging;

namespace JPEG.Images;

class Matrix
{
    public readonly Pixel[,] Pixels;
    public readonly int Height;
    public readonly int Width;

    public Matrix(int height, int width)
    {
        Height = height;
        Width = width;

        Pixels = new Pixel[height, width];
        for (var i = 0; i < height; ++i)
            for (var j = 0; j < width; ++j)
                Pixels[i, j] = new Pixel(0, 0, 0, PixelFormat.RGB);
    }

    public static explicit operator Matrix(Bitmap bmp)
    {
        var height = bmp.Height - bmp.Height % 8;
        var width = bmp.Width - bmp.Width % 8;
        var matrix = new Matrix(height, width);

        var rec = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rec, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

        unsafe
        {
            var dataPtr = (byte*)data.Scan0;

            for (var i = 0; i < height; i++)
            {
                var row = dataPtr + i * data.Stride;
                for (var j = 0; j < width; j++)
                {
                    var offset = j * 3;
                    matrix.Pixels[i, j] = new Pixel(row[offset + 2], row[offset + 1], row[offset], PixelFormat.RGB);
                }
            }
        }

        bmp.UnlockBits(data);

        return matrix;
    }

    public static explicit operator Bitmap(Matrix matrix)
    {
        var bmp = new Bitmap(matrix.Width, matrix.Height);
        var rec = new Rectangle(0, 0, matrix.Width, matrix.Height);
        var bitData = bmp.LockBits(rec, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

        unsafe
        {
            var dataPtr = (byte*)bitData.Scan0;

            for (var i = 0; i < bmp.Height; i++)
            {
                var row = dataPtr + i * bitData.Stride;
                for (var j = 0; j < bmp.Width; j++)
                {
                    var offset = j * 3;
                    var pixel = matrix.Pixels[i, j];
                    row[offset] = (byte)pixel.B;
                    row[offset + 1] = (byte)pixel.G;
                    row[offset + 2] = (byte)pixel.R;
                }
            }
        }

        bmp.UnlockBits(bitData);
        return bmp;
    }

    public static int ToByte(double d)
    {
        var val = (int)d;
        if (val > byte.MaxValue)
            return byte.MaxValue;
        if (val < byte.MinValue)
            return byte.MinValue;
        return val;
    }
}