namespace Our.Umbraco.Bloodhound
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using Examine;
    using Examine.Providers;
    using Examine.SearchCriteria;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Core.Models;
    using global::Umbraco.Web;
    using global::Umbraco.Web.Routing;

    /// <summary>
    /// Attempts to assign an <see cref="IPublishedContent"/> instance to the request.
    /// </summary>
    public class BloodhoundRewriteContentFinder : IContentFinder
    {
        /// <inheritdoc/>
        public bool TryFindContent(PublishedContentRequest contentRequest)
        {
            if (!contentRequest.Is404)
            {
                // TODO: use contentRequest.Uri?
                string url = HttpContext.Current.Request.Url.ToString();
                string domain = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                try
                {
                    Tuple<IPublishedContent, int> result;
                    IPublishedContent rerouteContent = null;
                    int statusCode = 0;

                    // Check the cache first
                    object cached = UrlRewriteCache.GetItem(url);
                    if (cached != null)
                    {
                        result = (Tuple<IPublishedContent, int>)cached;
                        rerouteContent = result.Item1;
                        statusCode = result.Item2;
                    }
                    else
                    {
                        // Nothing in the cache so let's taverse through each level of the site until we find it.
                        IPublishedContent root = contentRequest.RoutingContext.UmbracoContext.ContentCache.GetAtRoot()
                                                               .FirstOrDefault(p => p.UrlAbsolute().StartsWith(domain));

                        // Start looking for a match, this is recursive.
                        result = this.TryFindRewriteNode(root, BloodhoundPropertyEditor.PropertyEditorAlias, url);

                        if (result != null)
                        {
                            rerouteContent = result.Item1;
                            statusCode = result.Item2;
                        }
                    }

                    // We have a result!
                    if (statusCode > 0 && rerouteContent != null)
                    {
                        if (statusCode == 301)
                        {
                            contentRequest.SetRedirectPermanent(rerouteContent.Url);
                        }
                        else
                        {
                            // 302
                            contentRequest.SetRedirect(rerouteContent.Url);
                        }

                        contentRequest.PublishedContent = rerouteContent;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<BloodhoundApplicationEvents>($"An error occured attempting to provide a rewrite for the following request {url}", ex);
                }
            }

            return contentRequest.PublishedContent != null;
        }

        /// <summary>
        /// Recursively loops through the published content cache one level at a time looking for a matching node.
        /// </summary>
        /// <param name="content">The <see cref="IPublishedContent"/> node currently being checked</param>
        /// <param name="alias">The property alias</param>
        /// <param name="url">The url to search for.</param>
        /// <returns></returns>
        private Tuple<IPublishedContent, int> TryFindRewriteNode(IPublishedContent content, string alias, string url)
        {
            // Check against the property editor alias since we can't enforce strict naming of the property.
            // Or can we?
            IPublishedProperty rewriteProperties = content
                .Properties
                .FirstOrDefault(p => content.ContentType.GetPropertyType(p.PropertyTypeAlias).PropertyEditorAlias.Equals(alias));

            if (rewriteProperties != null)
            {
                // Convert the properties and
                List<BloodhoundUrlRewrite> rewrites = rewriteProperties.GetValue<List<BloodhoundUrlRewrite>>();
                BloodhoundUrlRewrite rewrite = rewrites.FirstOrDefault(
                        r =>
                        r.RewriteUrl.InvariantEquals(url)
                        || (r.IsRegex && new Regex(r.RewriteUrl, RegexOptions.IgnoreCase).IsMatch(url)));

                // It's a result, cache it and return.
                if (rewrite != null)
                {
                    Tuple<IPublishedContent, int> result = new Tuple<IPublishedContent, int>(content, rewrite.StatusCode);
                    UrlRewriteCache.UpdateItem(rewrite.RewriteUrl, result);
                    return result;
                }
            }

            // Now check any children.
            IEnumerable<IPublishedContent> children = content.Children().AsQueryable();
            if (children.Any())
            {
                return children.Select(child => this.TryFindRewriteNode(child, alias, url))
                               .FirstOrDefault(result => result != null);
            }

            return null;
        }
    }
}
