using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MusicHouse.Models
{
    public class Genre
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "name")]
        [Display(Name = "Name of Genre")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "origin")]
        [Display(Name = "Origin of Genre")]
        public string Origin { get; set; }

    }

}