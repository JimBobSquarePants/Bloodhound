namespace Our.Umbraco.Bloodhound
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Routing;

    using ClientDependency.Core;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Events;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Core.Models;
    using global::Umbraco.Core.Publishing;
    using global::Umbraco.Core.Services;
    using global::Umbraco.Web;
    using global::Umbraco.Web.Routing;

    using Newtonsoft.Json;

    using UmbracoConstants = global::Umbraco.Core.Constants;

    /// <summary>
    /// Runs initialization code for the plugin.
    /// </summary>
    public class BloodhoundApplicationEvents : ApplicationEventHandler
    {
        /// <inheritdoc/>
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            LogHelper.Info<BloodhoundApplicationEvents>($"Attaching {nameof(BloodhoundRewriteContentFinder)} as IContentFinder");
            ContentFinderResolver.Current.InsertTypeBefore<ContentFinderByNotFoundHandlers, BloodhoundRewriteContentFinder>();

            // Add the event handlers
            ContentService.Published += this.ContentServicePublished;
            ContentService.UnPublished += this.ContentServiceUnPublished;
            ContentService.Moving += this.ContentServiceMoving;
        }

        /// <inheritdoc/>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Register custom routes.
            RouteBuilder.RegisterRoutes(RouteTable.Routes);

            // Ensure client dependency can find the embedded resources.
            FileWriters.AddWriterForExtension(EmbeddedResourceConstants.ResourceExtension, new EmbeddedResourceWriter());
        }

        /// <summary>
        /// Handles the content service published event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PublishEventArgs{IContent}"/> containing information about the event.</param>
        private void ContentServicePublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            string alias = BloodhoundPropertyEditor.PropertyEditorAlias;
            UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);

            // Loop through the items and add the url, value pair to the cache.
            foreach (IContent content in e.PublishedEntities)
            {
                IPublishedContent publishedVersion = helper.TypedContent(content.Id);

                IPublishedProperty rewriteProperties =
                    publishedVersion
                    .Properties
                    .FirstOrDefault(p => publishedVersion.ContentType.GetPropertyType(p.PropertyTypeAlias).PropertyEditorAlias.Equals(alias));

                if (rewriteProperties != null)
                {
                    List<BloodhoundUrlRewrite> rewrites = rewriteProperties.GetValue<List<BloodhoundUrlRewrite>>();
                    foreach (BloodhoundUrlRewrite rewrite in rewrites)
                    {
                        UrlRewriteCache.UpdateItem(
                            rewrite.RewriteUrl,
                            new Tuple<IPublishedContent, int>(publishedVersion, rewrite.StatusCode));
                    }
                }
            }
        }

        /// <summary>
        /// Handles the content service unpublished event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e"> The <see cref="PublishEventArgs{IContent}"/> containing information about the event.</param>
        private void ContentServiceUnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            string alias = BloodhoundPropertyEditor.PropertyEditorAlias;
            UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);

            // Loop through the items and remove the url, value pair from the cache.
            foreach (IContent content in e.PublishedEntities)
            {
                IPublishedContent publishedVersion = helper.TypedContent(content.Id);

                IPublishedProperty rewriteProperties =
                    publishedVersion
                    .Properties
                    .FirstOrDefault(p => publishedVersion.ContentType.GetPropertyType(p.PropertyTypeAlias).PropertyEditorAlias.Equals(alias));

                if (rewriteProperties != null)
                {
                    List<BloodhoundUrlRewrite> rewrites = rewriteProperties.GetValue<List<BloodhoundUrlRewrite>>();
                    foreach (BloodhoundUrlRewrite rewrite in rewrites)
                    {
                        UrlRewriteCache.RemoveItem(rewrite.RewriteUrl);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the content service moving event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DeleteEventArgs{IContent}"/> containing information about the event.</param>
        private void ContentServiceMoving(IContentService sender, MoveEventArgs<IContent> e)
        {
            IContentService contentService = ApplicationContext.Current.Services.ContentService;
            string alias = BloodhoundPropertyEditor.PropertyEditorAlias;
            UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);

            foreach (MoveEventInfo<IContent> info in e.MoveInfoCollection)
            {
                this.AddRedirect(info.Entity, contentService, alias, helper);
            }
        }

        /// <summary>
        /// Attempts to add redirect information to the given content item. 
        /// </summary>
        /// <param name="content">The content that is being moved.</param>
        /// <param name="contentService">The content service responsible for operations involving <see cref="IContent"/></param>
        /// <param name="alias">The property editor alias.</param>
        /// <param name="helper">The <see cref="UmbracoHelper"/> that provides access to the published content cache.</param>
        private void AddRedirect(IContent content, IContentService contentService, string alias, UmbracoHelper helper)
        {
            // TODO: Probably don't need the second check for recycle bin.
            if (content.HasPublishedVersion && content.Id != UmbracoConstants.System.RecycleBinContent)
            {
                // The old url still exists in the published content cache.
                IPublishedContent publishedVersion = helper.TypedContent(content.Id);
                string url = publishedVersion.UrlAbsolute();
                PropertyCollection props = content.Properties;

                // Grab the first property that matches our property editor alias.
                if (props.Any(p => p.PropertyType.PropertyEditorAlias.Equals(alias)))
                {
                    Property prop = props.First(p => p.PropertyType.PropertyEditorAlias.Equals(alias));
                    List<BloodhoundUrlRewrite> rewrites;
                    try
                    {
                        rewrites = JsonConvert.DeserializeObject<List<BloodhoundUrlRewrite>>(prop.Value.ToString());
                    }
                    catch
                    {
                        rewrites = new List<BloodhoundUrlRewrite>();
                    }

                    // Add a new rewrite to track the change then save/publish.
                    if (!rewrites.Any(r => r.RewriteUrl.InvariantEquals(url)))
                    {
                        rewrites.Add(new BloodhoundUrlRewrite { RewriteUrl = url });
                    }

                    content.SetValue(prop.Alias, JsonConvert.SerializeObject(rewrites.Distinct()));
                    Attempt<PublishStatus> attempt = contentService.SaveAndPublishWithStatus(content);

                    if (attempt.Exception != null)
                    {
                        LogHelper.Error<BloodhoundApplicationEvents>(
                            $"Unable to update redirect url for content node with id {content.Id}",
                            attempt.Exception);
                    }

                    // Recursively check for any children
                    IEnumerable<IContent> children = content.Children().ToList();
                    if (children.Any())
                    {
                        foreach (IContent child in children)
                        {
                            this.AddRedirect(child, contentService, alias, helper);
                        }
                    }
                }
            }
        }
    }
}
