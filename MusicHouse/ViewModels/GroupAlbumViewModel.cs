using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MusicHouse.Models;
using Neo4jClient;

namespace MusicHouse.ViewModels
{
    public class GroupAlbumViewModel
    {
        public Node<Group> Group { get; set; }
        public IEnumerable<Node<Album>> Albums { get; set; }
    }
}