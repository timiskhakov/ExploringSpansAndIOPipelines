using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using ExploringSpansAndIOPipelines.Core.Interfaces;
using ExploringSpansAndIOPipelines.Core.Models;

namespace ExploringSpansAndIOPipelines.Core.Parsers
{
    public class FileParserImproved : IFileParser
    {
        private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
        
        public async Task<List<Videogame>> Parse(string file)
        {
            var result = new List<Videogame>();
            await using var stream = File.OpenRead(file);
            var reader = PipeReader.Create(stream);
            while (true)
            {
                var read = await reader.ReadAsync();
                var buffer = read.Buffer;
                while (TryReadLine(ref buffer, out var sequence))
                {
                    var videogame = ProcessSequence(sequence);
                    result.Add(videogame);
                }
                    
                reader.AdvanceTo(buffer.Start, buffer.End);
                if (read.IsCompleted)
                {
                    break;
                }
            }

            return result;
        }
        
        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            var position = buffer.PositionOf((byte)'\n');
            if (position == null)
            {
                line = default;
                return false;
            }
            
            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            
            return true;
        }

        private static Videogame ProcessSequence(in ReadOnlySequence<byte> sequence)
        {
            var array = ArrayPool.Rent((int) sequence.Length);
            try
            {
                sequence.CopyTo(array);
                return LineParserImproved.Parse(array.AsSpan());
            }
            finally
            {
                ArrayPool.Return(array);
            }
        }
    }
}