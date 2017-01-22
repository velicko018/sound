using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MusicHouse.Models;
using Neo4jClient;

namespace MusicHouse.ViewModels
{
    public class AlbumSongsGroupViewModel
    {
        public Node<Group> Group { get; set; }
        public Node<Album> Album { get; set; }

        public IEnumerable<Song> Songs { get; set; }
    }
}