namespace Our.Umbraco.Bloodhound
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Handles the registration of custom routes.
    /// </summary>
    internal static class RouteBuilder
    {
        /// <summary>
        /// Registers a collection of routes.
        /// </summary>
        /// <param name="routes">The collection of routes for ASP.NET routing.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
            "BloodhoundEmbeddedResources",
            "App_Plugins/Bloodhound/GetResource/{fileName}",
            new
            {
                controller = "BloodhoundEmbeddedResource",
                action = "GetResource"
            },
            new[] { "Our.Umbraco.BloodHound" });
        }
    }
}
