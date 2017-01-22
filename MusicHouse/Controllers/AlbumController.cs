using System;
using System.Collections.Generic;
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
    public class AlbumController : Controller
    {
        // GET: Album
        public ActionResult List()
        {
            try
            {
                var albums = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Album)")
                    .Return(a => a.As<Node<Album>>())
                    .Results;
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", albums);
                }
                else
                {
                    return View("List", albums);
                }
            }
            catch(Exception e)
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

        public ActionResult Index()
        {
            IEnumerable<Node<Album>> albums = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Album)")
                    .Return(a => a.As<Node<Album>>())
                    .Results;
            return View("Index", albums);
        }

        // GET: Album/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var album = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Album)")
                    .Where($"ID(a) = {id}")
                    .Return(a => a.As<Node<Album>>())
                    .Limit(1)
                    .Results;

                var albumstoRedis = album.First().Data;

                var songs = WebApiConfig.GraphClient.Cypher
                    .Match("(s:Song), (b:Album)")
                    .Where("(b)-[:HAS_SONG]->(s) AND ID(b) = {id}")
                    .WithParam("id", id)
                    .Return(s => s.As<Song>())
                    .Results;

                var group = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Album), (g:Group)")
                    .Where("(g)-[:HAVE_ALBUM]->(a) AND ID(a) = {id}")
                    .WithParam("id", id)
                    .Return(g => g.As<Node<Group>>())
                    .Results;

                AlbumSongsGroupViewModel albumSongsGroupVM = new AlbumSongsGroupViewModel
                {
                    Album = album.First(),
                    Group = group.First(),
                    Songs = songs
                };
                LeaderBoard l = new LeaderBoard();
                l.SetAlbumClicks(albumstoRedis.AlbumName, albumstoRedis.NumberOfCopies, albumstoRedis.Producer, albumstoRedis.Studio, albumstoRedis.RecordedFrom, albumstoRedis.RecordedTo, albumstoRedis.Songs, albumstoRedis.Singles, albumstoRedis.Length, albumstoRedis.Released);

                return View("Details", albumSongsGroupVM);

            }
            catch(Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Album/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: Album/Create
        [HttpPost]
        public ActionResult Create(Album album)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                    .Create("(a:Album{params})")
                    .WithParam("params", album)
                    .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Album" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Album/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var album = WebApiConfig.GraphClient.Cypher
                .Match("(a:Album)")
                .Where($"ID(a) = {id}")
                .Return(a => a.As<Album>())
                .Limit(1)
                .Results;
                var albumName = album.First().AlbumName;
                #region Load Connections
                var songs = WebApiConfig.GraphClient.Cypher
                   .Match("(s:Song),(a:Album{albumName:{albumName}})")
                    .Where("NOT (a)-[:HAS_SONG]->(s)")
                    .WithParam("albumName", albumName)
                   .Return(s => s.As<Node<Song>>())
                   .Results;
                Session["Song"] = songs;

                var awards = WebApiConfig.GraphClient.Cypher
                  .Match("(a:Award)")
                  .Return(a => a.As<Node<Award>>())
                  .Results;
                Session["Award"] = awards;

                var genres = WebApiConfig.GraphClient.Cypher
                  .Match("(g:Genre),(a:Album{albumName:{albumName}})")
                  .Where("NOT (a)-[:HAS_GENRE]->(g)")
                  .WithParam("albumName", albumName)
                  .Return(g => g.As<Node<Genre>>())
                  .Results;
                Session["Genre"] = genres;


                #endregion

                return View("Edit", album.First());
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // POST: Album/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Album album, FormCollection form)
        {
            try
            {
                var genreToAdd = form.Get("Genres");
                var songToAdd = form.Get("Songs");
                var awardToAdd = form.Get("Awards");

                WebApiConfig.GraphClient.Cypher
                    .Match("(a:Album)")
                    .Where($"ID(a) = {id}")
                    .Set("a = {params}")
                    .WithParam("params", album)
                    .ExecuteWithoutResults();

                if (genreToAdd != String.Empty && genreToAdd!=null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Album)", "(g:Genre)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={genreToAdd}")
                       .Create("(s)-[r:HAS_GENRE]->(g)")
                       .ExecuteWithoutResults();
                }
                if (awardToAdd != String.Empty && awardToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Album)", "(g:Award)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={awardToAdd}")
                       .Create("(s)-[r:HAS_AWARD]->(g)")
                       .ExecuteWithoutResults();
                }
                if (songToAdd != String.Empty && songToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Album)", "(g:Song)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={songToAdd}")
                       .Create("(s)-[r:HAS_SONG]->(g)")
                       .ExecuteWithoutResults();
                }

                return RedirectToAction("Admin", "Home", new { enty = "Album" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Album/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                    .Match("(a:Album)")
                    .Where($"ID(a) = {id}")
                    .DetachDelete("a")
                    .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Album" });
            }
            catch
            {
                return View("Error");
            }
        }

        // POST: Album/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return View("Error");
        }
        
        public ActionResult Songs(long id)
        {
            var k = WebApiConfig.GraphClient.Cypher
                .OptionalMatch("(a:Album)-[HAS_SONG]-(s:Song)")
                .Where($"ID(a) ={id}")
                .Return(s => s.As<Node<Song>>())
                .Results;

            return PartialView("~/Views/Song/List.cshtml", k);
        }

        public void WriteArtistToRedis(string AlbumName, long NumberOfCopies, string Producer, string Studio, DateTime recfrom, DateTime recto, byte song, byte singl, string lng, DateTime rel)
        {
            if (Request.IsAjaxRequest())
            {
                LeaderBoard l = new LeaderBoard();
                l.SetAlbumClicks(AlbumName, NumberOfCopies, Producer, Studio, recfrom, recto, song,singl,lng,rel);
                //var p= l.GetSongClicks(songName+"_"+writer);
            }

        }
    }
}
