using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicHouse.App_Start;
using MusicHouse.Models;
using MusicHouse.ViewModels;
using Neo4jClient;
using Newtonsoft.Json;
using RedisDataLayer;

namespace MusicHouse.Controllers
{
    public class ArtistController : Controller
    {
        public ActionResult Index(string name)
        {
            try
            {
                IEnumerable<Node<Artist>> artists = new List<Node<Artist>>();
                if (name != null && name != "")
                {
                    artists = WebApiConfig.GraphClient.Cypher
                     .Match("(a:Artist), (b:Genre)")
                     .Where("(a)-[:HAS_GENRE]->(b) AND b.name = {name}")
                     .WithParam("name", name)
                     .Return(a => a.As<Node<Artist>>())
                     .Results;

                }
                else
                {
                    artists = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Artist)")
                    .Return(a => a.As<Node<Artist>>())
                    .Results;
                }

                var genres = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Genre)")
                    .Return(a => a.As<Node<Genre>>())
                    .Results;

                ArtistGenreViewModel artistVM = new ArtistGenreViewModel
                {
                    Artists = artists,
                    Genres = genres
                };
                if (Request.IsAjaxRequest())
                {
                    return PartialView("Partial/ArtistList", artistVM);
                }
                return View("Index", artistVM);
            }
            catch (Exception e)
            {
                if (Request.IsAjaxRequest())
                {
                    Session["Exception"] = e;
                    return PartialView("Error");
                }
                Session["Exception"] = e;
                return View("Error");
            }

        }
    

        // GET: Artist
        public ActionResult List()
        {
            try
            {
                var artists = WebApiConfig.GraphClient.Cypher
               .Match("(a:Artist)")
               .Return(a => a.As<Node<Artist>>())
               .Results;
                if (Request.IsAjaxRequest())
                {
                    return PartialView(artists);
                }
                else
                {
                    return View("List", artists);
                }
            }
            catch (Exception e)
            {
                if(Request.IsAjaxRequest())
                {
                    Session["Exception"] = e;
                    return PartialView("Error");
                }
                Session["Exception"] = e;
                return View("Error");
            }
            
        }

        // GET: Artist/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var artist = WebApiConfig.GraphClient.Cypher
               .Match("(a:Artist)")
               .Where($"ID(a) = {id}")
               .Return(a => a.As<Node<Artist>>())
               .Limit(1)
               .Results;

                var albums = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Artist), (b:Album)")
                    .Where("(a)-[:HAVE_ALBUM]->(b) AND ID(a) = {id}")
                    .WithParam("id", id)
                    .Return(b => b.As<Node<Album>>())
                    .Results;

                var artToRedis = artist.First().Data;
                LeaderBoard l = new LeaderBoard();
                l.SetArtistClicks(artToRedis.FirstName, artToRedis.MiddleName, artToRedis.LastName, artToRedis.ArtistName, artToRedis.BirthDate, artToRedis.Ancestry, artToRedis.DeathDate, artToRedis.Biography);

                ArtistAlbumsViewModel groupAlbumVM = new ArtistAlbumsViewModel
                {
                    Artist = artist.First(),
                    Albums = albums,
                };
                return View("Details", groupAlbumVM);
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
            
        }

        // GET: Artist/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: Artist/Create
        [HttpPost]
        public ActionResult Create(Artist artist)
        {
            try
            {
                
                
                WebApiConfig.GraphClient.Cypher
                    .Create("(a:Artist{params})")
                    .WithParam("params", artist)
                    .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Artist" });
            }
            catch(Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Artist/Edit/5
        public ActionResult Edit(int id)
        {
            var artist = WebApiConfig.GraphClient.Cypher
                .Match("(a:Artist)")
                .Where($"ID(a) = {id}")
                .Return(a => a.As<Artist>())
                .Limit(1)
                .Results;

            var artistName = artist.First().ArtistName;
            #region Load Connections
            var songs = WebApiConfig.GraphClient.Cypher
               .Match("(s:Song),(a:Artist{artistName:{artistName}})")
               .Where("NOT (a)-[:HAS_SONG]->(s)")
               .WithParam("artistName", artistName)
               .Return(s => s.As<Node<Song>>())
               .Results;
            Session["Song"] = songs;

            var instruments = WebApiConfig.GraphClient.Cypher
              .Match("(a:Instrument)")
              .Return(a => a.As<Node<Instrument>>())
              .Results;
            Session["Instrument"] = instruments;

            var Awards = WebApiConfig.GraphClient.Cypher
              .Match("(a:Award)")
              .Return(a => a.As<Node<Award>>())
              .Results;
            Session["Award"] = Awards;

            var Albums = WebApiConfig.GraphClient.Cypher
              .Match("(al:Album),(a:Artist{artistName:{artistName}})")
              .Where("NOT (a)-[:HAVE_ALBUM]->(al)")
              .WithParam("artistName", artistName)
              .Return(al => al.As<Node<Album>>())
              .Results;
            Session["Album"] = Albums;

            var genres = WebApiConfig.GraphClient.Cypher
              .Match("(g:Genre),(a:Artist{artistName:{artistName}})")
              .Where("NOT (a)-[:HAS_GENRE]->(g)")
              .WithParam("artistName", artistName)
              .Return(g => g.As<Node<Genre>>())
              .Results;
            Session["Genre"] = genres;


            #endregion


            return View("Edit", artist.First());
        }

        // POST: Artist/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Artist artist, FormCollection form)
        {
            try
            {
                var instrumentToAdd = form.Get("Instruments");
                var songToAdd = form.Get("Songs");
                var awardToAdd = form.Get("Awards");
                var albumToAdd = form.Get("Albums");
                var genresToAdd = form.Get("Genres");

                WebApiConfig.GraphClient.Cypher
                    .Match("(a:Artist)")
                    .Where($"ID(a) = {id}")
                    .Set("a = {params}")
                    .WithParam("params", artist)
                    .ExecuteWithoutResults();
                if (instrumentToAdd != String.Empty && instrumentToAdd!=null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Artist)", "(g:Instrument)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={instrumentToAdd}")
                       .Create("(s)-[r:HAS_INSTRUMENT]->(g)")
                       .ExecuteWithoutResults();
                }
                if (awardToAdd != String.Empty && awardToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Artist)", "(g:Award)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={awardToAdd}")
                       .Create("(s)-[r:HAS_AWARD]->(g)")
                       .ExecuteWithoutResults();
                }
                if (songToAdd != String.Empty && songToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Artist)", "(g:Song)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={songToAdd}")
                       .Create("(s)-[r:HAS_SONG]->(g)")
                       .ExecuteWithoutResults();
                }
                if (albumToAdd != String.Empty && albumToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Artist)", "(g:Album)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={albumToAdd}")
                       .Create("(s)-[r:HAVE_ALBUM]->(g)")
                       .ExecuteWithoutResults();
                }
                if(genresToAdd!=String.Empty && genresToAdd!=null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Artist)", "(g:Genre)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={genresToAdd}")
                       .Create("(s)-[r:HAS_GENRE]->(g)")
                       .ExecuteWithoutResults();
                }

                return RedirectToAction("Admin", "Home", new { enty = "Artist" });
            }
            catch(Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Artist/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                 .Match("(a:Artist)")
                 .Where($"ID(a) = {id}")
                 .DetachDelete("a")
                 .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Artist" });
            }
            catch(Exception e)
            {
                return View("Error");

            }
        }

        // POST: Artist/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return View("Error");
        }

        public void WriteArtistToRedis(string FirstName, string MiddleName, string LastName, string ArtistName, DateTime birth, string ances, DateTime dth, string bio)
        {
            if (Request.IsAjaxRequest())
            {
                LeaderBoard l = new LeaderBoard();
                l.SetArtistClicks(FirstName, MiddleName, LastName, ArtistName, birth, ances,dth,bio);
                //var p= l.GetSongClicks(songName+"_"+writer);
            }

        }
    }
}
