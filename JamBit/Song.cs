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
    public class Song
    {
        [PrimaryKey, AutoIncrement]
        public int ID
        {
            get; set;
        }

        private string _fileName;
        [MaxLength(260)]
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                if (Data != null)
                    Data.Dispose();
                Data = TagLib.File.Create(_fileName);
                Title = Data.Tag.Title;
                Artist = Data.Tag.FirstPerformer;
                Album = Data.Tag.Album;
            }
        }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

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

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
                return false;

            Song s = obj as Song;

            return this.Artist == s.Artist && this.Title == s.Title && this.Album == s.Album;
        }

        /// <summary>
        /// Used to determine distinct artist names in the library
        /// </summary>
        public class ArtistComparator : IEqualityComparer<Song>
        {
            public bool Equals(Song x, Song y)
            {
                return x.Artist.ToLower() == y.Artist.ToLower();
            }

            public int GetHashCode(Song obj)
            {
                return obj.Artist.ToLower().GetHashCode();
            }
        }

        public class AlbumComparator : IEqualityComparer<Song>
        {
            public bool Equals(Song x, Song y)
            {
                return x.Album == y.Album;
            }

            public int GetHashCode(Song obj)
            {
                return obj.Album.ToLower().GetHashCode();
            }
        }
    }
}
