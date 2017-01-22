using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MusicHouse.Models
{
    public class Song
    {
        [JsonProperty(PropertyName = "songName")]
        [Display(Name = "Song Name")]
        public string SongName { get; set; }

        [JsonProperty(PropertyName = "writer")]
        [Display(Name = "Writer")]
        public string Writer { get; set; }

        [JsonProperty(PropertyName = "length")]
        [Display(Name = "Length")]
        public string Length { get; set; }

        [JsonProperty(PropertyName = "number")]
        [Display(Name = "Number")]
        [Range(1, 999)]
        public byte Number { get; set; }

        public Song(string songName, string songWriter, string length, byte number)
        {
            this.SongName = songName;
            this.Writer = songWriter;
            this.Length = length;
            this.Number = number;
        }

        public Song()
        {

        }
    }
}