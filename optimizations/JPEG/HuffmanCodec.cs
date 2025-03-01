using System.Linq;
using System.Collections.Generic;

namespace JPEG;

class HuffmanNode
{
    public byte? LeafLabel { get; init; }
    public int Frequency { get; init; }
    public HuffmanNode Left { get; init; }
    public HuffmanNode Right { get; init; }
}

public class BitsWithLength : IEqualityComparer<BitsWithLength>
{
    public int Bits { get; set; }
    public int BitsCount { get; set; }

    public bool Equals(BitsWithLength x, BitsWithLength y)
    {
        if (x == y)
            return true;
        if (x == null || y == null)
            return false;
        return x.BitsCount == y.BitsCount && x.Bits == y.Bits;
    }

    public int GetHashCode(BitsWithLength obj)
    {
        return ((397 * obj.Bits) << 5) ^ (17 * obj.BitsCount);
    }
}

class BitsBuffer
{
    private readonly List<byte> buffer = [];
    private int unfinishedBits;
    private int unfinishedBitsCount;

    public void Add(BitsWithLength bitsWithLength)
    {
        var bitsCount = bitsWithLength.BitsCount;
        var bits = bitsWithLength.Bits;

        var neededBits = 8 - unfinishedBitsCount;
        while (bitsCount >= neededBits)
        {
            bitsCount -= neededBits;
            buffer.Add((byte)((unfinishedBits << neededBits) + (bits >> bitsCount)));

            bits &= (1 << bitsCount) - 1;

            unfinishedBits = 0;
            unfinishedBitsCount = 0;

            neededBits = 8;
        }

        unfinishedBitsCount += bitsCount;
        unfinishedBits = (unfinishedBits << bitsCount) + bits;
    }

    public byte[] ToArray(out long bitsCount)
    {
        bitsCount = buffer.Count * 8L + unfinishedBitsCount;
        var result = new byte[(bitsCount + 7) / 8];
        buffer.CopyTo(result);
        if (unfinishedBitsCount > 0)
            result[buffer.Count] = (byte)(unfinishedBits << (8 - unfinishedBitsCount));
        return result;
    }
}

class HuffmanCodec
{
    public static byte[] Encode(
        IEnumerable<byte> data,
        out Dictionary<BitsWithLength, byte> decodeTable,
        out long bitsCount)
    {
        var frequences = CalcFrequences(data);

        var root = BuildHuffmanTree(frequences);

        var encodeTable = new BitsWithLength[byte.MaxValue + 1];
        FillEncodeTable(root, encodeTable);

        var bitsBuffer = new BitsBuffer();
        foreach (var b in data)
            bitsBuffer.Add(encodeTable[b]);

        decodeTable = CreateDecodeTable(encodeTable);

        return bitsBuffer.ToArray(out bitsCount);
    }

    public static byte[] Decode(byte[] encodedData, Dictionary<BitsWithLength, byte> decodeTable, long bitsCount)
    {
        var result = new List<byte>();

        byte decodedByte;
        var sample = new BitsWithLength { Bits = 0, BitsCount = 0 };
        for (var byteNum = 0; byteNum < encodedData.Length; byteNum++)
        {
            var b = encodedData[byteNum];
            for (var bitNum = 0; bitNum < 8 && byteNum * 8 + bitNum < bitsCount; bitNum++)
            {
                sample.Bits = (sample.Bits << 1) + ((b & (1 << (8 - bitNum - 1))) != 0 ? 1 : 0);
                sample.BitsCount++;

                if (decodeTable.TryGetValue(sample, out decodedByte))
                {
                    result.Add(decodedByte);

                    sample.BitsCount = 0;
                    sample.Bits = 0;
                }
            }
        }

        return result.ToArray();
    }

    private static Dictionary<BitsWithLength, byte> CreateDecodeTable(BitsWithLength[] encodeTable)
    {
        var result = new Dictionary<BitsWithLength, byte>(new BitsWithLength());
        for (var b = 0; b < encodeTable.Length; b++)
        {
            var bitsWithLength = encodeTable[b];
            if (bitsWithLength == null)
                continue;

            result[bitsWithLength] = (byte)b;
        }

        return result;
    }

    private static void FillEncodeTable(
        HuffmanNode node,
        BitsWithLength[] encodeSubstitutionTable,
        int bitvector = 0,
        int depth = 0)
    {
        if (node.LeafLabel != null)
            encodeSubstitutionTable[node.LeafLabel.Value] =
                new BitsWithLength { Bits = bitvector, BitsCount = depth };
        else
        {
            if (node.Left != null)
            {
                FillEncodeTable(node.Left, encodeSubstitutionTable, (bitvector << 1) + 1, depth + 1);
                FillEncodeTable(node.Right, encodeSubstitutionTable, (bitvector << 1) + 0, depth + 1);
            }
        }
    }

    private static HuffmanNode BuildHuffmanTree(int[] frequences)
    {
        var nodes = GetNodes(frequences);
        var priorityQueue = new SortedSet<HuffmanNode>(nodes, Comparer<HuffmanNode>.Create((a, b) =>
        {
            var result = a.Frequency.CompareTo(b.Frequency);
            return result == 0 ? a.GetHashCode().CompareTo(b.GetHashCode()) : result;
        }));

        while (priorityQueue.Count > 1)
        {
            var firstMin = priorityQueue.Min;
            priorityQueue.Remove(firstMin);
            var secondMin = priorityQueue.Min;
            priorityQueue.Remove(secondMin);
            priorityQueue.Add(new HuffmanNode
            {
                Frequency = firstMin.Frequency + secondMin.Frequency,
                Left = firstMin,
                Right = secondMin
            });
        }

        return priorityQueue.First();
    }

    private static IEnumerable<HuffmanNode> GetNodes(int[] frequences)
    {
        return Enumerable.Range(0, byte.MaxValue + 1)
            .Select(num => new HuffmanNode { Frequency = frequences[num], LeafLabel = (byte)num })
            .Where(node => node.Frequency > 0)
            .ToArray();
    }

    private static int[] CalcFrequences(IEnumerable<byte> data)
    {
        var result = new int[byte.MaxValue + 1];
        foreach (var b in data)
        {
            result[b]++;
        }
        return result;
    }
}