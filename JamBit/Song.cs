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
        [PrimaryKey, MaxLength(32)]
        public string Checksum { get; set; }

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
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(_fileName))
                    {
                        byte[] ba = md5.ComputeHash(stream);
                        StringBuilder hex = new StringBuilder(ba.Length * 2);
                        foreach (byte b in ba)
                            hex.AppendFormat("{0:x2}", b);
                        Checksum = hex.ToString();
                    }
                }
            }
        }

        [Ignore]
        public int PlayCount { get; set; }

        [Ignore]
        public TagLib.File Data { get; set; }

        // TODO: Determine why length is off
        [Ignore]
        public int Length { get { return (int)(Data.Properties.Duration.TotalMilliseconds / 1000) - 1; } }

        public Song() { }

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
