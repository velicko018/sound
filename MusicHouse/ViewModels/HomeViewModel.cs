using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MusicHouse.Models;

namespace MusicHouse.ViewModels
{
    public class HomeViewModel
    {
        public List<Song> Songs { get; set; }
        public List<Artist> Artists { get; set; }
        public List<Group> Groups { get; set; }
        public List<Album> Albums { get; set; }
    }
}