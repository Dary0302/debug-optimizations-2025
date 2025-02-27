using System;
using System.Linq;

namespace JPEG.Images;

public readonly struct Pixel
{
    // Переделать в byte

    public double R { get; }
    public double G { get; }
    public double B { get; }
    public double Y { get; }
    public double Cb { get; }
    public double Cr { get; }
    
    public Pixel(double firstComponent, double secondComponent, double thirdComponent, PixelFormat pixelFormat)
    {
        if (!new[] { PixelFormat.RGB, PixelFormat.YCbCr }.Contains(pixelFormat))
            throw new FormatException("Unknown pixel format: " + pixelFormat);
        
        if (pixelFormat == PixelFormat.RGB)
        {
            R = firstComponent;
            G = secondComponent;
            B = thirdComponent;

            Y = 16.0 + (65.738 * R + 129.057 * G + 24.064 * B) / 256.0;
            Cb = 128.0 + (-37.945 * R - 74.494 * G + 112.439 * B) / 256.0;
            Cr = 128.0 + (112.439 * R - 94.154 * G - 18.285 * B) / 256.0;
        }

        if (pixelFormat == PixelFormat.YCbCr)
        {
            Y = firstComponent;
            Cb = secondComponent;
            Cr = thirdComponent;

            R = (298.082 * Y + 408.583 * Cr) / 256.0 - 222.921;
            G = (298.082 * Y - 100.291 * Cb - 208.120 * Cr) / 256.0 + 135.576;
            B = (298.082 * Y + 516.412 * Cb) / 256.0 - 276.836;
        }
    }
}