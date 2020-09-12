
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CraftopiaMerger.Helpers
{
    public static class StringExtensions
    {
        public static MemoryStream ToMemoryStream(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
