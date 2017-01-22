using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MusicHouse.Models
{
    public class Group
    {
        [JsonProperty(PropertyName = "groupName")]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "numberOfMembers")]
        [Display(Name = "Number of Members")]
        [Range(1, 999)]
        public byte NumberOfMembers { get; set; }

        [JsonProperty(PropertyName = "origin")]
        [Display(Name = "Origin")]
        public string Origin { get; set; }

        [JsonProperty(PropertyName = "website")]
        [Display(Name = "Website ")]
        public string Website { get; set; }

        [JsonProperty(PropertyName = "established")]
        [Display(Name = "Established")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Established { get; set; }

        [JsonProperty(PropertyName = "yearOfDecay")]
        [Display(Name = "Year Of Decay")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? YearOfDecay { get; set; }

        public Group(string groupname, byte numberofmembers, string origin, string website, DateTime established, DateTime yearOfDecay)
        {
            this.GroupName = groupname;
            this.NumberOfMembers = numberofmembers;
            this.Origin = origin;
            this.Website = website;
            this.Established = established;
            this.YearOfDecay = yearOfDecay;
        }

        public Group()
        {

        }
    }
}