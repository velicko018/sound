using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RedisDataLayer
{
    public class Artists
    {
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string ArtistName { get; set; }

        public DateTime BirthDate { get; set; }

        public string Ancestry { get; set; }

        public DateTime? DeathDate { get; set; }

        public Artists(string FirstName, string MiddleName, string LastName, string ArtistName,DateTime birth,string ances,DateTime dth)
        {
            this.FirstName = FirstName;
            this.MiddleName = MiddleName;
            this.LastName = LastName;
            this.ArtistName = ArtistName;
            this.BirthDate = birth;
            this.Ancestry = ances;
            this.DeathDate = dth;
        }

        public Artists()
        {

        }

        public string ToJsonString()
        {
            return JsonSerializer.SerializeToString<Artists>(this);
        }
    }
}
