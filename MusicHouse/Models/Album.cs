using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using Microsoft.ApplicationInsights.WindowsServer;
using Newtonsoft.Json;

namespace MusicHouse.Models
{
    public class Album
    {
        [JsonProperty(PropertyName = "albumName")]
        [Display(Name = "Album Name")]
        [Required]
        public string AlbumName { get; set; }

        [JsonProperty(PropertyName = "numberOfCopies")]
        [Display(Name = "Number of Copies")]
        [Range(0, 999)]
        public long NumberOfCopies { get; set; }

        [JsonProperty(PropertyName = "producer")]
        [Display(Name = "Producer Name")]
        [Required]
        public string Producer { get; set; }

        [JsonProperty(PropertyName = "studio")]
        [Display(Name = "Studio Name")]
        [Required]
        public string Studio { get; set; }

        [JsonProperty(PropertyName = "recordedFrom")]
        [Display(Name = "Recorded From")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime RecordedFrom { get; set; }

        [JsonProperty(PropertyName = "recordedTo")]
        [Display(Name = "Recorded To")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime RecordedTo { get; set; }

        [JsonProperty(PropertyName = "songs")]
        [Display(Name = "Songs")]
        [Range(0, 999)]
        public byte Songs { get; set; }

        [JsonProperty(PropertyName = "singles")]
        [Display(Name = "Singles")]
        [Range(0, 999)]
        public byte Singles { get; set; }

        [JsonProperty(PropertyName = "length")]
        [DataType(DataType.Time)]
        [Display(Name = "Length")]
        public string Length { get; set; }

        [JsonProperty(PropertyName = "released")]
        [Display(Name = "Release Time")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Released { get; set; }

        public Album(string albumname,string producer,string studio, long numberofcopies,byte songs,byte singles, DateTime recfrom, DateTime recto, string length, DateTime realese )
        {
            this.AlbumName = albumname;
            this.Producer = producer;
            this.Studio = studio;
            this.NumberOfCopies = numberofcopies;
            this.Songs = songs;
            this.Singles = singles;
            this.RecordedFrom = recfrom;
            this.RecordedTo = recto;
            this.Length = length;
            this.Released = realese;
        }

        public Album()
        {

        }

    }
}