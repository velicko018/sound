using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MusicHouse.Models
{
    public class Instrument
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "type")]
        [Display(Name = "Type of Instrument")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "manufactureYear")]
        [Display(Name = "Manufacture year")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true)]
        public DateTime? ManufactureYear { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "category")]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "name")]
        [Display(Name = "Name of Instrument")]
        public string Name { get; set; }
    }
}