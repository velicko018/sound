using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MusicHouse.Models
{
    public class Artist
    {

        [JsonProperty(PropertyName = "firstName")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "middleName")]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "artistName")]
        [Display(Name = "Artist Name")]
        public string ArtistName { get; set; }

        [JsonProperty(PropertyName = "birthDate")]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime BirthDate { get; set; }

        [JsonProperty(PropertyName = "ancestry")]
        [Display(Name = "Ancestry ")]
        public string Ancestry { get; set; }

        [JsonProperty(PropertyName = "biography")]
        [Display(Name = "Biography ")]
        [DataType(DataType.MultilineText)]
        public string Biography { get; set; }

        [JsonProperty(PropertyName = "deathDate")]
        [Display(Name = "Death Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true)]
        public DateTime? DeathDate { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return LastName + " "+MiddleName+" " + FirstName;
            }
        }

        [Display(Name = "Lifespan")]
        public string Lifespan
        {
            get
            {
                return BirthDate.ToString() + "--" + DeathDate.ToString();
            }
        }

        public Artist(string artistname, string artistfirstname, string artistlastname, string artistmiddlename,DateTime birth, DateTime death, string ancestary, string bio)
        {
            this.FirstName = artistfirstname;
            this.ArtistName = artistname;
            this.MiddleName = artistmiddlename;
            this.LastName = artistlastname;
            this.Ancestry = ancestary;
            this.BirthDate = birth;
            this.DeathDate = death;
            this.Biography = bio;

        }

        public Artist()
        {

        }
    }
}