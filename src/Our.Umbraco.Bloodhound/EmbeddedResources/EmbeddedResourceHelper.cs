namespace Our.Umbraco.Bloodhound
{
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using global::Umbraco.Core;

    /// <summary>
    /// Provides methods for retrieving embedded resources.
    /// </summary>
    internal class EmbeddedResourceHelper
    {
        /// <summary>
        /// Returns a value indicating whether the given resource exists.
        /// </summary>
        /// <param name="resource">The resource name.</param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ResourceExists(string resource)
        {
            // Sanitize the resource request.
            resource = SanitizeResourceName(resource);

            // Check this assembly.
            Assembly assembly = typeof(BloodhoundEmbeddedResourceController).Assembly;

            // Find the resource name; not case sensitive.
            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(r => r.InvariantEndsWith(resource));

            return !string.IsNullOrWhiteSpace(resourceName);
        }

        /// <summary>
        /// Gets a stream containing the content of the embedded resource.
        /// </summary>
        /// <param name="assembly">The assembly containing the resource.</param>
        /// <param name="resource">The path to the resource.</param>
        /// <param name="resourceName">The resource name.</param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream GetResource(Assembly assembly, string resource, out string resourceName)
        {
            // Sanitize the resource request.
            resource = SanitizeResourceName(resource);

            // Find the resource name; not case sensitive.
            resourceName = assembly.GetManifestResourceNames().FirstOrDefault(r => r.InvariantEndsWith(resource));

            if (resourceName != null)
            {
                return assembly.GetManifestResourceStream(resourceName);
            }

            return null;
        }

        /// <summary>
        /// Gets a sanitized name for an embedded resource.
        /// </summary>
        /// <param name="resource">The path to the resource.</param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static string SanitizeResourceName(string resource)
        {
            string resourceRoot = EmbeddedResourceConstants.ResourceRoot;
            string extension = EmbeddedResourceConstants.ResourceExtension;

            if (resource.StartsWith(resourceRoot))
            {
                resource = resource.TrimStart(resourceRoot).Replace("/", ".").TrimEnd(extension);
            }
            else if (resource.EndsWith(extension))
            {
                resource = resource.TrimEnd(extension);
            }

            return resource;
        }
    }
}
