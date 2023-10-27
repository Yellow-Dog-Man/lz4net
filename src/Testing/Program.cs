using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LZ4;

namespace Testing
{
    class Allocator : IBufferAllocator
    {
        List<byte[]> buffers = new List<byte[]>();

        public byte[] GetBuffer(int minSize)
        {
            Console.WriteLine("Getting buffer: " + minSize);

            for(int i = 0; i < buffers.Count; i++)
            {
                if(buffers[i].Length >= minSize)
                {
                    var buffer = buffers[i];
                    buffers.RemoveAt(i);
                    return buffer;
                }
            }

            return new byte[minSize];
        }

        public void ReturnBuffer(byte[] buffer)
        {
            buffers.Add(buffer);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var allocator = new Allocator();
            var r = new Random();

            for (int c = 0; c < 10; c++)
            {
                var stream = new MemoryStream();

                var lz4stream = new LZ4Stream(stream, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression,
                    bufferAllocator: allocator);

                var data = new byte[100 + r.Next() % 10000];

                r.NextBytes(data);

                lz4stream.Write(data, 0, data.Length);
                lz4stream.Dispose();

                var compressed = stream.ToArray();

                stream = new MemoryStream(compressed);
                lz4stream = new LZ4Stream(stream, LZ4StreamMode.Decompress,
                    bufferAllocator: allocator);

                var decoded = new byte[data.Length];

                lz4stream.Read(decoded, 0, decoded.Length);

                for (int i = 0; i < decoded.Length; i++)
                    if (decoded[i] != data[i])
                        Console.WriteLine("MISMATCH!");

                lz4stream.Dispose();

                Console.WriteLine("Test");
            }

            Console.Read();
        }
    }
}
