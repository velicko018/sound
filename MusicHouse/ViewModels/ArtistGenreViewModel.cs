using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MusicHouse.Models;
using Neo4jClient;

namespace MusicHouse.ViewModels
{
    public class ArtistGenreViewModel
    {
        public IEnumerable<Node<Artist>> Artists { get; set; }
        public IEnumerable<Node<Genre>> Genres { get; set; }
    }
}