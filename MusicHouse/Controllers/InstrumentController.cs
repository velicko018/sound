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
    public class InstrumentController : Controller
    {
        // GET: Instrument
        public ActionResult List()
        {
            try
            {
                var Instruments = WebApiConfig.GraphClient.Cypher
               .Match("(a:Instrument)")
               .Return(a => a.As<Node<Instrument>>())
               .Results;
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", Instruments);
                }
                else
                {
                    return View("List", Instruments);
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

        // GET: Instrument/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: Instrument/Create
        [HttpPost]
        public ActionResult Create(Instrument Instrument)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                    .Create("(g:Instrument{params})")
                    .WithParam("params", Instrument)
                    .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Instrument" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Instrument/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var Instrument = WebApiConfig.GraphClient.Cypher
                .Match("(a:Instrument)")
                .Where($"ID(a) = {id}")
                .Return(a => a.As<Instrument>())
                .Limit(1)
                .Results;

                return View("Edit", Instrument.First());
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }

        }

        // POST: Instrument/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Instrument Instrument)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                   .Match("(g:Instrument)")
                   .Where($"ID(g) = {id}")
                   .Set("g = {params}")
                   .WithParam("params", Instrument)
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Instrument" });
            }
            catch (Exception e)
            {
                Session["Exception"] = e;
                return View("Error");
            }
        }

        // GET: Instrument/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                WebApiConfig.GraphClient.Cypher
                   .Match("(g:Instrument)")
                   .Where($"ID(g) = {id}")
                   .Delete("g")
                   .ExecuteWithoutResults();

                return RedirectToAction("Admin", "Home", new { enty = "Instrument" });
            }
            catch
            {
                return View("Error");
            }
        }

        // POST: Instrument/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeletePost(int id)
        {
            return View("Error");
        }
    }
}