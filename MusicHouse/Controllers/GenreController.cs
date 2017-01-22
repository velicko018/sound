using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicHouse.App_Start;
using MusicHouse.Models;
using Neo4jClient;

namespace MusicHouse.Controllers
{
    public class GenreController : Controller
    {
        // GET: Genre
        public ActionResult List()
        {
            try
            {
                var Genres = WebApiConfig.GraphClient.Cypher
               .Match("(a:Genre)")
               .Return(a => a.As<Node<Genre>>())
               .Results;
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", Genres);
                }
                else
                {
                    return View("List", Genres);
                }
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }

        }

        // GET: Genre/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: Genre/Create
        [HttpPost]
        public ActionResult Create(Genre Genre)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                    .Create("(g:Genre{params})")
                    .WithParam("params", Genre)
                    .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Genre" });
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

        // GET: Genre/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var Genre = WebApiConfig.GraphClient.Cypher
                .Match("(a:Genre)")
                .Where($"ID(a) = {id}")
                .Return(a => a.As<Genre>())
                .Limit(1)
                .Results;

                return View("Edit", Genre.First());
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }

        }

        // POST: Genre/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Genre Genre)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                   .Match("(g:Genre)")
                   .Where($"ID(g) = {id}")
                   .Set("g = {params}")
                   .WithParam("params", Genre)
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Genre" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Genre/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                   .Match("(g:Genre)")
                   .Where($"ID(g) = {id}")
                   .DetachDelete("g")
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Genre" });
            }
            catch
            {
                return View("Error");
            }
        }

        // POST: Genre/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return View("Error");
        }
    }
}