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

        public Albums(string AlbumName, long NumberOfCopies, string Producer, string Studio)
        {
            this.AlbumName = AlbumName;
            this.NumberOfCopies = NumberOfCopies;
            this.Producer = Producer;
            this.Studio = Studio;
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
