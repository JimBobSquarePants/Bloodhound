namespace Our.Umbraco.Bloodhound
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Models;
    using global::Umbraco.Core.Models.PublishedContent;
    using global::Umbraco.Core.PropertyEditors;

    /// <summary>
    /// Converts an object stored with the <see cref="IPublishedContent"/> into the <see cref="List{BloodhoundUrlRewrite}"/>.
    /// </summary>
    public class BloodhoundUrlRewritePropertyValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        /// <inheritdoc/>
        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<BloodhoundUrlRewrite>>(source.ToString());
            }
            catch
            {
                return new List<BloodhoundUrlRewrite>();
            }
        }

        /// <inheritdoc/>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(BloodhoundPropertyEditor.PropertyEditorAlias);
        }

        /// <inheritdoc/>
        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(List<BloodhoundUrlRewrite>);
        }

        /// <inheritdoc/>
        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }
    }
}
