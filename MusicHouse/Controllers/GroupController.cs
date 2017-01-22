using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using MusicHouse.App_Start;
using MusicHouse.Models;
using MusicHouse.ViewModels;
using Neo4jClient;

namespace MusicHouse.Controllers
{
    public class GroupController : Controller
    {
        // GET: Group
        public ActionResult Index(string name)
        {
            try
            {
                IEnumerable<Node<Group>> groups = new List<Node<Group>>();
                if (name != null && name != "")
                {
                   groups = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Group), (b:Genre)")
                    .Where("(a)-[:HAS_GENRE]->(b) AND b.name = {name}")
                    .WithParam("name", name)
                    .Return(a => a.As<Node<Group>>())
                    .Results;

                }
                else
                {
                    groups = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Group)")
                    .Return(a => a.As<Node<Group>>())
                    .Results;
                }
                
                var genres = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Genre)")
                    .Return(a => a.As<Node<Genre>>())
                    .Results;

                GroupViewModel groupVM = new GroupViewModel
                {
                    Groups = groups,
                    Genres = genres
                };
                if (Request.IsAjaxRequest())
                {
                    return PartialView("Partial/GroupList", groupVM);
                }
                return View("Index", groupVM);
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        public ActionResult List()
        {
            try
            {
                var groups = WebApiConfig.GraphClient.Cypher
               .Match("(a:Group)")
               .Return(a => a.As<Node<Group>>())
               .Results;
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", groups);
                }
                else
                {
                    return View("List", groups);
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

        // GET: Group/Details/5
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

                var group = WebApiConfig.GraphClient.Cypher
                    .Match("(a:Group), (b:Album)")
                    .Where("(a)-[:HAVE_ALBUM]->(b) AND ID(a) = {id}")
                    .WithParam("id", id)
                    .Return(a => a.As<Node<Group>>())
                    .Results;

                var songs = WebApiConfig.GraphClient.Cypher
                    .Match("(s:Song), (b:Album)")
                    .Where("(b)-[:HAS_SONG]->(s) AND ID(a) = {id}")
                    .WithParam("id", id)
                    .Return(s => s.As<Song>())
                    .Results;

                AlbumSongsGroupViewModel groupAlbumSongVM = new AlbumSongsGroupViewModel
                {
                    Group = group.First(),
                    Album = album.First(),
                    Songs = songs
                };
                return View("Details", groupAlbumSongVM);
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
            
        }

        // GET: Group/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: Group/Create
        [HttpPost]
        public ActionResult Create(Group group)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                    .Create("(g:Group{params})")
                    .WithParam("params", group)
                    .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Group" });
            }
            catch(Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Group/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var group = WebApiConfig.GraphClient.Cypher
                .Match("(a:Group)")
                .Where($"ID(a) = {id}")
                .Return(a => a.As<Group>())
                .Limit(1)
                .Results;
                var groupName = group.First().GroupName;
                #region Load Connections       
                // muzicar je jedinstven za grupu
                var artists = WebApiConfig.GraphClient.Cypher
                  .Match("(a:Artist),(g:Group{groupName:{groupName}})")
                  .Where("NOT (a)-[:MEMBER]->(g)")
                  .WithParam("groupName", groupName)
                  .Return(a => a.As<Node<Artist>>())
                  .Results;
                Session["Artist"] = artists;

                //nagrade mogu vise puta da se dodeljuju istoj grupi
                var awards = WebApiConfig.GraphClient.Cypher
                  .Match("(a:Award)")
                  .Return(a => a.As<Node<Award>>())
                  .Results;
                Session["Award"] = awards;

                var albums = WebApiConfig.GraphClient.Cypher
                  .Match("(a:Album),(g:Group{groupName:{groupName}})")
                  .Where("NOT (g)-[:HAVE_ALBUM]->(a)")
                  .WithParam("groupName", groupName)
                  .Return(a => a.As<Node<Album>>())
                  .Results;
                Session["Album"] = albums;

                var genres = WebApiConfig.GraphClient.Cypher
                  .Match("(g:Genre),(gr:Group{groupName:{groupName}})")
                  .Where("NOT (gr)-[:HAS_GENRE]->(g)")
                  .WithParam("groupName", groupName)
                  .Return(g => g.As<Node<Genre>>())
                  .Results;
                Session["Genre"] = genres;

                #endregion

                return View("Edit", group.First());
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }

        }

        // POST: Group/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Group group, FormCollection form)
        {
            try
            {
                var artistToAdd = form.Get("Artists");
                var awardToAdd = form.Get("Awards");
                var albumToAdd = form.Get("Albums");
                var genreToAdd = form.Get("Genres");

                if (artistToAdd != String.Empty && artistToAdd!=null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Group)", "(g:Artist)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={artistToAdd}")
                       .Create("(g)-[r:MEMBER]->(s)")
                       .ExecuteWithoutResults();
                }
                if (awardToAdd != String.Empty && awardToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Group)", "(g:Award)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={awardToAdd}")
                       .Create("(s)-[r:HAS_AWARD]->(g)")
                       .ExecuteWithoutResults();
                }
                if (albumToAdd != String.Empty && albumToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Group)", "(g:Album)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={albumToAdd}")
                       .Create("(s)-[r:HAVE_ALBUM]->(g)")
                       .ExecuteWithoutResults();
                }
                if (genreToAdd != String.Empty && genreToAdd != null)
                {
                    WebApiConfig.GraphClient.Cypher
                       .Match("(s:Group)", "(g:Genre)")
                       .Where($"ID(s) = {id}")
                       .AndWhere($"ID(g)={genreToAdd}")
                       .Create("(s)-[r:HAS_GENRE]->(g)")
                       .ExecuteWithoutResults();
                }

                WebApiConfig.GraphClient.Cypher
                   .Match("(g:Group)")
                   .Where($"ID(g) = {id}")
                   .Set("g = {params}")
                   .WithParam("params", group)
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Group" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Group/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                   .Match("(g:Group)")
                   .Where($"ID(g) = {id}")
                   .DetachDelete("g")
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Group" });
            }
            catch
            {
                return View("Error");
            }
        }

        // POST: Group/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return View("Error");
        }
    }
}
