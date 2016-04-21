﻿namespace Our.Umbraco.Bloodhound
{
    using ClientDependency.Core.CompositeFiles;

    using global::Umbraco.Core;

    /// <summary>
    /// The embedded resource virtual path provider.
    /// </summary>
    public sealed class EmbeddedResourceVirtualPathProvider : IVirtualFileProvider
    {
        /// <summary>
        /// Returns a value indicating whether the virtual file exists for the given path.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool FileExists(string virtualPath)
        {
            if (!virtualPath.InvariantEndsWith(EmbeddedResourceConstants.ResourceExtension))
            {
                return false;
            }

            return EmbeddedResourceHelper.ResourceExists(virtualPath);
        }

        /// <summary>
        /// Returns the virtual file for the given path.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path.
        /// </param>
        /// <returns>
        /// The <see cref="IVirtualFile"/>.
        /// </returns>
        public IVirtualFile GetFile(string virtualPath)
        {
            if (!this.FileExists(virtualPath))
            {
                return null;
            }

            return new EmbeddedResourceVirtualFile(virtualPath);
        }
    }
}
