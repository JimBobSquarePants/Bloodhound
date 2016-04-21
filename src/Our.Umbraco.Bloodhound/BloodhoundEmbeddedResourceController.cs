namespace Our.Umbraco.Bloodhound
{
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.Mvc;

    using global::Umbraco.Core;

    /// <summary>
    /// The bloodhound embedded resource controller. Handles the retrieval of embedded resources
    /// </summary>
    public class BloodhoundEmbeddedResourceController : Controller
    {
        /// <summary>
        /// Gets an embedded resource from the main assembly
        /// </summary>
        /// <param name="fileName">Name of resource</param>
        /// <returns>File stream of resource</returns>
        public ActionResult GetResource(string fileName)
        {
            Mandate.ParameterNotNullOrEmpty(fileName, "fileName");

            // Get this assembly.
            Assembly assembly = typeof(BloodhoundEmbeddedResourceController).Assembly;
            string resourceName;
            Stream resourceStream = EmbeddedResourceHelper.GetResource(assembly, fileName, out resourceName);

            if (resourceStream != null)
            {
                return new FileStreamResult(resourceStream, this.GetMimeType(resourceName));
            }

            return this.HttpNotFound();
        }

        /// <summary>
        /// Helper to set the MIME type for a given file name
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>MIME type for file</returns>
        private string GetMimeType(string fileName)
        {
            return MimeMapping.GetMimeMapping(fileName) ?? "text/plain";
        }
    }
}
