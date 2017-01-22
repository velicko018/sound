using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Http;
using System.Web;
using RedisDataLayer;
using Microsoft.Ajax.Utilities;
using MusicHouse.Models;
using MusicHouse.ViewModels;
using Neo4jClient;

namespace MusicHouse.Controllersss
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                if (!System.Web.HttpContext.Current.Application.AllKeys.Contains("Exception"))
                {
                    if (Session["admin"] == null)
                    {
                        Session["admin"] = "false";

                    }

                    LeaderBoard l = new LeaderBoard();
                    var topsongs = l.GetTopSongs(20);
                    var reacentsongs = l.GetRecentSongs(20).DistinctBy(x => x.SongName).ToList();
                    List<Song> SongTopList = new List<Song>();
                    List<Song> SongReacentList = new List<Song>();
                    foreach (var item in topsongs)
                    {
                        Song song = new Song(item.SongName, item.Writer, item.Length, item.Number);
                        SongTopList.Add(song);
                    }
                    foreach (var item in reacentsongs)
                    {
                        Song song = new Song(item.SongName, item.Writer, item.Length, item.Number);
                        SongReacentList.Add(song);
                    }

                    var topArtist = l.GetTopArtists(3);
                    List<Artist> artistTopList = new List<Artist>();

                    foreach (var item in topArtist)
                    {
                        Artist artist = new Artist(
                            item.ArtistName,
                            item.FirstName,
                            item.LastName,
                            item.MiddleName,
                            item.BirthDate,
                            item.DeathDate.GetValueOrDefault(),
                            item.Ancestry,
                            item.Biography);
                        artistTopList.Add(artist);
                    }

                    var topAlbums = l.GetTopAlbums(3);
                    List<Album> albumTopList = new List<Album>();

                    foreach (var item in topAlbums)
                    {
                        Album album = new Album(
                            item.AlbumName,
                            item.Producer,
                            item.Studio,
                            item.NumberOfCopies,
                            item.Songs,
                            item.Singles,
                            item.RecordedFrom,
                            item.RecordedTo,
                            item.Length,
                            item.Released);
                        albumTopList.Add(album);
                    }

                    var topGroups = l.GetTopGroups(3);
                    List<Group> groupTopList = new List<Group>();

                    foreach (var item in topGroups)
                    {
                        Group group = new Group(
                            item.GroupName,
                            item.NumberOfMembers,
                            item.Origin,
                            item.Website,
                            item.Established,
                            item.YearOfDecay.GetValueOrDefault());
                        groupTopList.Add(group);
                    }

                    HomeViewModel homeViewModel = new HomeViewModel
                    {
                        Albums = (albumTopList.Count != 0) ? albumTopList : new List<Album>(),
                        Artists = (artistTopList.Count != 0) ? artistTopList : new List<Artist>(),
                        Groups = (groupTopList.Count != 0) ? groupTopList : new List<Group>(),
                        Songs = (SongTopList.Count != 0) ? SongTopList : new List<Song>(),
                    };

                    return View(homeViewModel);

                }
                return View("Error");


            }
            catch (Exception e)
            {
                return View("Error");
            }
            



           
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Admin(string enty)
        {
            Session["Entity"] = enty;
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Login()
        {
            var value1 = Request["email"];
            var value2 = Request["password"];
            if (value1=="admin@gmail.com" && value2=="admin")
            {
                Session["admin"] = "true";
            }
            else
            {
                Session["admin"] = "false";
            }
            return View("Index");
        }

        public ActionResult Logout()
        {
            Session["admin"] = "false";
            return View("Index");
        }
    }
}