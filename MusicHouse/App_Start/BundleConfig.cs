using System.Web;
using System.Web.Optimization;

namespace MusicHouse
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/jquery.unobtrusive-ajax.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/theme").Include(
                      "~/Content/theme/js/utilcarousel-files/bootstrap.js",
                      "~/Content/theme/js/utilcarousel-files/jquery.*",
                      "~/Content/theme/js/utilcarousel-files/soundtheme-*",
                      "~/Content/theme/js/utilcarousel-files/soundmanager2-nodebug-jsmin.js",
                      "~/Content/theme/js/utilcarousel-files/magnific-popup/jquery.magnific-popup.js",
                      "~/Content/theme/js/utilcarousel-files/utilcarousel/jquery.utilcarousel.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Login.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/theme").Include(
                      "~/Content/theme/css/bootstrap.css",
                      "~/Content/theme/css/font-awsome*",
                      "~/Content/theme/css/soundtheme-*",
                      "~/Content/theme/js/utilcarousel-files/magnific-popup/magnific-popup.css",
                      "~/Content/theme/js/utilcarousel-files/utilcarousel/util.*"));
        }
    }
}
