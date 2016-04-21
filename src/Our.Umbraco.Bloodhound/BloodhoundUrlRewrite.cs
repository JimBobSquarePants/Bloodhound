namespace Our.Umbraco.Bloodhound
{
    using System;

    using Newtonsoft.Json;

    /// <summary>
    /// Represents a url rewrite bound to a content node.
    /// </summary>
    public class BloodhoundUrlRewrite : IEquatable<BloodhoundUrlRewrite>
    {
        /// <summary>
        /// Gets or sets the rewrite url
        /// </summary>
        [JsonProperty("rewriteUrl")]
        public string RewriteUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this rewrite is a regex
        /// </summary>
        [JsonProperty("isRegex")]
        public bool IsRegex { get; set; }

        /// <summary>
        /// Gets or sets the status code
        /// </summary>
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; } = 301;

        /// <summary>
        /// Gets or sets the creation date in utc format
        /// </summary>
        [JsonProperty("createdDateUtc")]
        public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public bool Equals(BloodhoundUrlRewrite other)
        {
            return this.RewriteUrl.Equals(other.RewriteUrl)
                && this.IsRegex.Equals(other.IsRegex)
                && this.StatusCode.Equals(other.StatusCode)
                && this.CreatedDateUtc.Equals(other.CreatedDateUtc);
        }
    }
}
