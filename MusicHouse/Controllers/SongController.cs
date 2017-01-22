using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicHouse.App_Start;
using MusicHouse.Models;
using Neo4jClient;
using Microsoft.Ajax.Utilities;

namespace MusicHouse.Controllers
{
    public class SongController : Controller
    {
        // GET: Song
        public ActionResult List()
        {
            try
            {
                var songs = WebApiConfig.GraphClient.Cypher
                   .Match("(s:Song)")
                   .Return(s => s.As<Node<Song>>())
                   .Results;
                if(Request.IsAjaxRequest())
                {
                    return PartialView("List", songs);
                }
                else
                {
                    return View("List", songs);
                }
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

        // GET: Song/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var song = WebApiConfig.GraphClient.Cypher
                .Match("(s:Song)")
                .Where($"ID(s) = {id}")
                .Return(s => s.As<Node<Song>>())
                .Limit(1)
                .Results;

                return View("Details", song.First());
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Song/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: Song/Create
        [HttpPost]
        public ActionResult Create(Song song)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                    .Create("(s:Song{params})")
                    .WithParam("params", song)
                    .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Song" });
            }
            catch(Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Song/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var song = WebApiConfig.GraphClient.Cypher
                    .Match("(s:Song)")
                    .Where($"ID(s) = {id}")
                    .Return(s => s.As<Song>())
                    .Limit(1)
                    .Results;

                var songName = song.First().SongName;

                #region Load Connections
                var awards = WebApiConfig.GraphClient.Cypher
                  .Match("(a:Award)")
                  .Return(a => a.As<Node<Award>>())
                  .Results;
                Session["Award"] = awards;

                var genres = WebApiConfig.GraphClient.Cypher
                  .Match("(g:Genre),(s:Song{songName:{songName}})")
                  .Where("NOT (s)-[:HAS_GENRE]->(g)")
                  .WithParam("songName", songName)
                  .Return(g => g.As<Node<Genre>>())
                  .Results;
                //IEnumerable<Node<Genre>> distGenres = genres.DistinctBy(x => x.Reference);
                Session["Genre"] = genres;
                #endregion

                return View("Edit", song.First());
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // POST: Song/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Song song, FormCollection form)
        {
            try
            {
                var genreToAdd = form.Get("Genres");
                var awardToAdd = form.Get("Awards");

                WebApiConfig.GraphClient.Cypher
                   .Match("(s:Song)")
                   .Where($"ID(s) = {id}")
                   .Set("s = {params}")
                   .WithParam("params", song)
                   .ExecuteWithoutResults();
                if (genreToAdd != String.Empty && genreToAdd!=null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Song)", "(g:Genre)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={genreToAdd}")
                       .Create("(s)-[r:HAS_GENRE]->(g)")
                       .ExecuteWithoutResults();
                }
                if (awardToAdd!=String.Empty && awardToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Song)", "(g:Award)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={awardToAdd}")
                       .Create("(s)-[r:HAS_AWARD]->(g)")
                       .ExecuteWithoutResults();
                }
                return RedirectToAction("Admin", "Home", new { enty = "Song" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Song/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                   .Match("(s:Song)")
                   .Where($"ID(s) = {id}")
                   .DetachDelete("s")
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Song" });
            }
            catch(Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // POST: Song/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return View("Error");
        }
    }
}
