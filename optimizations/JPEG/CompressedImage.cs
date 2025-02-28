using System.Collections.Generic;
using System.IO;

namespace JPEG;

public class CompressedImage
{
	public int Width { get; set; }
	public int Height { get; set; }

	public int Quality { get; set; }
		
	public Dictionary<BitsWithLength, byte> DecodeTable { get; set; }

	public long BitsCount { get; set; }
	public byte[] CompressedBytes { get; set; }

    public void Save(string path)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(fs);

        writer.Write(Width);
        writer.Write(Height);
        writer.Write(Quality);

        writer.Write(DecodeTable.Count);
        foreach (var kvp in DecodeTable)
        {
            writer.Write(kvp.Key.Bits);
            writer.Write(kvp.Key.BitsCount);
            writer.Write(kvp.Value);
        }

        writer.Write(BitsCount);
        writer.Write(CompressedBytes.Length);
        writer.Write(CompressedBytes);
    }


    public static CompressedImage Load(string path)
    {
        var result = new CompressedImage();
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(fs);

        result.Width = reader.ReadInt32();
        result.Height = reader.ReadInt32();
        result.Quality = reader.ReadInt32();

        var decodeTableSize = reader.ReadInt32();
        result.DecodeTable = new Dictionary<BitsWithLength, byte>(decodeTableSize, new BitsWithLength.Comparer());

        for (var i = 0; i < decodeTableSize; i++)
        {
            var bits = reader.ReadInt32();
            var bitsCount = reader.ReadInt32();
            var mappedByte = reader.ReadByte();

            result.DecodeTable[new BitsWithLength { Bits = bits, BitsCount = bitsCount }] = mappedByte;
        }

        result.BitsCount = reader.ReadInt64();

        var compressedBytesCount = reader.ReadInt32();
        result.CompressedBytes = reader.ReadBytes(compressedBytesCount);

        return result;
    }

}