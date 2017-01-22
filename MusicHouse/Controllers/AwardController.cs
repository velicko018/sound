using MusicHouse.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicHouse.Models;
using Neo4jClient;

namespace MusicHouse.Controllers
{
    public class AwardController : Controller
    {
        // GET: Award
        public ActionResult List()
        {
            try
            {
                var Awards = WebApiConfig.GraphClient.Cypher
               .Match("(a:Award)")
               .Return(a => a.As<Node<Award>>())
               .Results;
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", Awards);
                }
                else
                {
                    return View("List", Awards);
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

        // GET: Award/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: Award/Create
        [HttpPost]
        public ActionResult Create(Award Award)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                    .Create("(g:Award{params})")
                    .WithParam("params", Award)
                    .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Award" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Award/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var Award = WebApiConfig.GraphClient.Cypher
                .Match("(a:Award)")
                .Where($"ID(a) = {id}")
                .Return(a => a.As<Award>())
                .Limit(1)
                .Results;

                return View("Edit", Award.First());
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }

        }

        // POST: Award/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Award Award)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                   .Match("(g:Award)")
                   .Where($"ID(g) = {id}")
                   .Set("g = {params}")
                   .WithParam("params", Award)
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Award" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Award/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                   .Match("(g:Award)")
                   .Where($"ID(g) = {id}")
                   .DetachDelete("g")
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Award" });
            }
            catch
            {
                return View("Error");
            }
        }

        // POST: Award/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return View("Error");
        }
    }
}