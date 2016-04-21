namespace Our.Umbraco.Bloodhound
{
    using ClientDependency.Core;

    using global::Umbraco.Core.PropertyEditors;
    using global::Umbraco.Web.PropertyEditors;

    /// <summary>
    /// The bloodhound property editor.
    /// </summary>
    [PropertyEditor(PropertyEditorAlias, "Bloodhound", EmbeddedResourceConstants.ResourceRoot + "bloodhound.html", ValueType = "JSON")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, EmbeddedResourceConstants.ResourceRoot + "bloodhound.controller.js" + EmbeddedResourceConstants.ResourceExtension)]
    [PropertyEditorAsset(ClientDependencyType.Css, EmbeddedResourceConstants.ResourceRoot + "bloodhound.css" + EmbeddedResourceConstants.ResourceExtension)]
    public class BloodhoundPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Gets or sets the alias uniquely identifying the property editor for the property type.
        /// </summary>
        public const string PropertyEditorAlias = "Our.Umbraco.Bloodhound";
    }
}
