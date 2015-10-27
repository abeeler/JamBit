using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JamBit
{
    class Song
    {
        [PrimaryKey, AutoIncrement]
        public int ID
        {
            get; set;
        }

        private string _fileName;
        [MaxLength(380)]
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                if (Data != null)
                    Data.Dispose();
                Data = TagLib.File.Create(_fileName);
            }
        }

        public int PlayCount { get; set; }

        [Ignore]
        public TagLib.File Data { get; set; }

        // TODO: Determine why length is off
        [Ignore]
        public double Length { get { return Data.Properties.Duration.TotalSeconds; } }

        public Song() {
            PlayCount = 0;
        }

        public Song(string fileName)
        {
            FileName = fileName;
            Data = TagLib.File.Create(fileName);
            PlayCount = 0;
        }

        public override string ToString()
        {
            return Data.Tag.Title;
        }
    }
}
