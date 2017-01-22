using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Http;
using System.Web;
using RedisDataLayer;
using Microsoft.Ajax.Utilities;
using MusicHouse.Models;
using Neo4jClient;

namespace MusicHouse.Controllersss
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!System.Web.HttpContext.Current.Application.AllKeys.Contains("Exception"))
            {
                if(Session["admin"]==null)
                {
                    Session["admin"] = "false";
                }
                return View();

            }

            LeaderBoard l = new LeaderBoard();
            var topsongs = l.GetTopSongs(20);
            var reacentsongs = l.GetRecentSongs(20).DistinctBy(x => x.SongName).ToList();
            IList<Song> SongTopList = new List<Song>();
            IList<Song> SongReacentList = new List<Song>();
            foreach (var item in topsongs)
            {
             Song song = new Song(item.SongName,item.Writer,item.Length,item.Number);
                SongTopList.Add(song);
            }
            foreach (var item in reacentsongs)
            {
                Song song = new Song(item.SongName, item.Writer, item.Length, item.Number);
                SongReacentList.Add(song);
            }

            return View("Error");
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