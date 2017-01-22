using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RedisDataLayer
{
   public class Groups
    {
        public string GroupName { get; set; }

        public string Origin { get; set; }

        public string Website { get; set; }

        public byte NumberOfMembers { get; set; }

        public DateTime Established { get; set; }

        public DateTime? YearOfDecay { get; set; }

        public Groups(string groupName, string origin, string website, byte numberOfMembers, DateTime est, DateTime? yod)
        {
            this.GroupName = groupName;
            this.Origin = origin;
            this.Website = website;
            this.NumberOfMembers = numberOfMembers;
            this.Established = est;
            this.YearOfDecay = yod;
        }

        public Groups()
        {

        }

        public string ToJsonString()
        {
            return JsonSerializer.SerializeToString<Groups>(this);
        }
    }
}
