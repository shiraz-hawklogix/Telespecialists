using System.Collections.Generic;
using System.Web;
using System.Web.Optimization;
using TeleSpecialists.BLL;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            var jQueryBundles = new List<string>();
            jQueryBundles.Add("~/Scripts/jquery-{version}.js");
            if (Settings.RPCMode == RPCMode.SignalR)
                jQueryBundles.Add("~/Scripts/jquery.signalR-{version}.js");
            jQueryBundles.Add("~/Scripts/spin.min.js");
            jQueryBundles.Add("~/Scripts/Custom/jqueryUI.js");
            jQueryBundles.Add("~/Scripts/Custom/BootSideMenu.js");
            jQueryBundles.Add("~/Scripts/jquery.multiselect.js");
            

            jQueryBundles.Add("~/Scripts/moment.min.js");
            jQueryBundles.Add("~/Scripts/moment-timezone-with-data.min.js");

            jQueryBundles.Add("~/Scripts/base64.min.js");
            jQueryBundles.Add("~/Scripts/Custom/jsExtensions.js");            
            jQueryBundles.Add("~/Scripts/Custom/Shared.js");

            jQueryBundles.Add("~/Scripts/jszip.min.js");
            jQueryBundles.Add("~/Scripts/pako_deflate.min.js");


            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                            jQueryBundles.ToArray()
                        ));



            if (Settings.RPCMode != RPCMode.Disabled)
            {
                var bundleFiles = new List<string>();
                bundleFiles.Add("~/Scripts/Hubs/physician_case_popup_hub_code.js");

                if (Settings.RPCMode == RPCMode.SignalR)
                {
                    bundleFiles.Add("~/Scripts/Hubs/physician_case_popup_hub.js");
                }
                else if (Settings.RPCMode == RPCMode.WebSocket)
                    bundleFiles.Add("~/Scripts/Hubs/tc_socket_hub.js");

                bundles.Add(new ScriptBundle("~/bundles/hubs").Include(bundleFiles.ToArray()));
            }


            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryvalidate").Include(
                        "~/Scripts/jquery.validate.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/five9").Include(
                        "~/Scripts/Five9/iframe-extension.js",
                        "~/Scripts/Five9/five9_events.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/custom_popper.js",
                      "~/Scripts/bootstrap.js",                  
                       "~/Scripts/combodate.js"

                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/jquery.multiselect.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/Custom/BootMenuCss.css"));

            bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
                "~/Scripts/kendo/kendo.all.min.js",
                // "~/Scripts/kendo/kendo.timezones.min.js", // uncomment if using the Scheduler
                "~/Scripts/kendo/kendo.aspnetmvc.min.js"));


            bundles.Add(new StyleBundle("~/Content/kendo/css").Include(
               "~/Content/kendo/kendo.common-bootstrap.min.css",
               "~/Content/kendo/kendo.bootstrap.min.css"));

          //  bundles.Add(new ScriptBundle("~/bundles/firebase").Include(
          //     "~/Scripts/firebase/js/init.js",
          //     "~/Scripts/firebase/js/call.js"
          //));
        }
    }
}
