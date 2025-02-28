using BenchmarkDotNet.Attributes;

namespace JPEG.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class CompressedImageBenchmark
{
    private CompressedImage _image;
    private string _tempFilePath;

    [GlobalSetup]
    public void Setup()
    {
        _image = new CompressedImage
        {
            Width = 128,
            Height = 128,
            Quality = 90,
            DecodeTable = new Dictionary<BitsWithLength, byte>
            {
                [new BitsWithLength { Bits = 0b110, BitsCount = 3 }] = (byte)'A',
                [new BitsWithLength { Bits = 0b111, BitsCount = 3 }] = (byte)'B',
            },
            BitsCount = 1024,
            CompressedBytes = new byte[128]
        };

        _tempFilePath = Path.Combine(Path.GetTempPath(), "testimage.bin");
    }

    [Benchmark]
    public void SaveBenchmark()
    {
        _image.Save(_tempFilePath);
    }

    [Benchmark]
    public void LoadBenchmark()
    {
        CompressedImage.Load(_tempFilePath);
    }
}