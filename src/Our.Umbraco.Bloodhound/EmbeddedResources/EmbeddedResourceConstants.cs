namespace Our.Umbraco.Bloodhound
{
    /// <summary>
    /// Provides a set of application wide constants for working with embedded resources.
    /// </summary>
    public static class EmbeddedResourceConstants
    {
        /// <summary>
        /// The root location for all embedded resources.
        /// </summary>
        public const string ResourceRoot = "/App_Plugins/Bloodhound/GetResource/";

        /// <summary>
        /// The root namespace for embedded resources.
        /// </summary>
        public const string ResourceRootNameSpace = "Our.Umbraco.Bloodhound.";

        /// <summary>
        /// The extension to add to embedded resources in order to ensure that the resource is not blocked
        /// by client dependency security constraints.
        /// </summary>
        public const string ResourceExtension = ".umb";
    }
}
