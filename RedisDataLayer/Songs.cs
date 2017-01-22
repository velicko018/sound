using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace RedisDataLayer
{
    public class Songs
    {
        public string SongName { get; set; }

        public string Writer { get; set; }

        public string Length { get; set; }

        public byte Number { get; set; }

        public Songs(string SongName, string Writer, string Length, byte Number)
        {
            this.SongName = SongName;
            this.Writer = Writer;
            this.Length = Length;
            this.Number = Number;
        }
        public Songs()
        {

        }

        public string ToJsonString()
        {
            return JsonSerializer.SerializeToString<Songs>(this);
        }
    }
}
