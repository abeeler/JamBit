using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamBit
{
    class Song
    {
        public TagLib.File Data { get; private set; }
        public string FileName { get; private set; }
        public int PlayCount;

        // TODO: Determine why length is off
        public int Length { get { return (int)(Data.Properties.Duration.TotalMilliseconds / 1000) - 1; } }

        public Song(string fileName)
        {
            FileName = fileName;
            Data = TagLib.File.Create(fileName);
            PlayCount = 0;
        }
    }
}
