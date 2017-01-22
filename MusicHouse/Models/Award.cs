using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MusicHouse.Models
{
    public class Award
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "name")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "year")]
        [Display(Name = "Year")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime Year { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "festival")]
        [Display(Name = "Festival")]
        public string Festival { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "country")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "category")]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "numberOfAward")]
        [Display(Name = "Number of awards")]
        [Range(0,999)]
        public int NumberOfAward { get; set; }
    }
}