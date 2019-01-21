using System.Collections.Generic;
using System.Linq;

namespace Titan.Util
{
    public static class ListUtil
    {
        
        // Credit to https://stackoverflow.com/a/24087164/10245934
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize) 
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        
    }
}