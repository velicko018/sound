using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MusicHouse.Models;
using Neo4jClient;

namespace MusicHouse.ViewModels
{
    public class GroupViewModel
    {
        public IEnumerable<Node<Group>> Groups { get; set; }
        public IEnumerable<Node<Genre>> Genres { get; set; }
    }
}