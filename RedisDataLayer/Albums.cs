using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace RedisDataLayer
{
    public class Albums
    {
        public string AlbumName { get; set; }

        public long NumberOfCopies { get; set; }

        public string Producer { get; set; }

        public string Studio { get; set; }

        public DateTime RecordedFrom { get; set; }

        public DateTime RecordedTo { get; set; }

        public byte Songs { get; set; }

        public byte? Singles { get; set; }

        public string Length { get; set; }

        public DateTime Released { get; set; }

        public Albums(string AlbumName, long NumberOfCopies, string Producer, string Studio,DateTime recfrom,DateTime recto,byte song, byte singl, string lng, DateTime rel)
        {
            this.AlbumName = AlbumName;
            this.NumberOfCopies = NumberOfCopies;
            this.Producer = Producer;
            this.Studio = Studio;
            this.RecordedFrom = recfrom;
            this.RecordedTo = recto;
            this.Songs = song;
            this.Singles = singl;
            this.Length = lng;
            this.Released = rel;
        }

        public Albums()
        {

        }

        public string ToJsonString()
        {
            return JsonSerializer.SerializeToString<Albums>(this);
        }
    }
}
