namespace WhaleTeam.TinyMCE.Web
{
    #region

using System;
using System.IO;
using System.Web.UI;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Text;
using DotNetNuke.Framework.Providers;
using Globals = DotNetNuke.Common.Globals;

    #endregion

    public class Textarea : WebControl, IPostBackDataHandler
    {

        #region "Constants"

        /// <summary>
        /// The provider type.
        /// </summary>
        private const string PROVIDER_TYPE = "htmlEditor";

        #endregion

        #region "Private Member"

        private bool _isMerged = false;
        private bool _isRendered = false;
        private static bool? _hasMsAjax;
        private NameValueCollection _settings;
        private string _providerPath;

        #endregion

        #region "Constructors"

        public Textarea() {
            _settings = new NameValueCollection();

            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(PROVIDER_TYPE);
            Provider objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            if ((objProvider != null))
            {
                foreach (string key in objProvider.Attributes)
                {
                    if ((key.ToLower().StartsWith("mce_")))
                    {
                        string adjustedKey = key.Substring(4, key.Length - 4).ToLower();
                        if ((!string.IsNullOrEmpty(adjustedKey)))
                        {
                            _settings[adjustedKey] = objProvider.Attributes[key];
                        }
                    }
                    else if ((key.ToLower() == "providerpath"))
                    {
                        _providerPath = objProvider.Attributes[key];
                    }
                }
            }
        }

        #endregion

        #region "Private Properties"

        private string ScriptURI {
            get
            {
                string outURI = null;

                outURI = ProviderPath + "tinymce.min.js";
                if (!File.Exists(this.Context.Server.MapPath(outURI)))
                {
                    throw new Exception("Could not locate TinyMCE by URI:" + outURI + ", Physical path:" + this.Context.Server.MapPath(outURI) + ". Make sure that you configured the installPath to a valid location in your web.config. This path should be an relative or site absolute URI to where TinyMCE is located.");
                }

                return Globals.ResolveUrl(outURI);
            }
        }

        /// <summary>
        /// Gets a value indicating whether Has Microsoft Ajax is installed.
        /// </summary>
        private static bool HasMsAjax {
            get {
                if (_hasMsAjax != null)
                    return _hasMsAjax.Value;

                _hasMsAjax = false;

                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Assembly asm in appAssemblies.Where(asm => asm.ManifestModule.Name == "System.Web.Extensions.dll")) {
                    try {
                        var scriptManager = asm.GetType("System.Web.UI.ScriptManager");

                        if (scriptManager != null)
                            _hasMsAjax = true;
                    }
                    catch {
                        _hasMsAjax = false;
                    }

                    break;
                }

                return _hasMsAjax.Value;
            }
        }

        #endregion

        #region "Public Properties"

        public bool HasRenderedTextArea(Control control)
        {
            if (control is Textarea && ((Textarea)control).IsRendered)
            {
                return true;
            }

            foreach (Control ctrl in control.Controls)
            {
                if (this.HasRenderedTextArea(ctrl))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsRendered
        {
            get { return _isRendered; }
        }

        public NameValueCollection Settings
        {
            get
            {
                if (!_isMerged)
                {
                    // Override local settings with attributes
                    foreach (string key in this.Attributes.Keys)
                    {
                        _settings[key] = this.Attributes[key];
                    }

                    _settings["elements"] = this.ClientID;
                    _settings["mode"] = "exact";

                    _isMerged = true;
                }

                return _settings;
            }
        }

        public string Value
        {
            get { return (string)ViewState["text"]; }
            set { ViewState["text"] = value; }
        }

        public string ProviderPath
        {
            get { return _providerPath; }
        }

        #endregion

        #region "Methods"

        /// <summary>
        /// Registers the on submit statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        /// <param name="script">The script.</param>
        private void RegisterOnSubmitStatement(Type type, string key, string script)
        {
            if (HasMsAjax)
            {
                ScriptManager.RegisterOnSubmitStatement(this, type, key, script);
            }
            else
            {
                Page.ClientScript.RegisterOnSubmitStatement(type, key, script);
            }
        }

        #endregion

        #region "Rendering"

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            /// Generates the editor load script.
            if (HasMsAjax)
            {
                StringBuilder postBackScript = new StringBuilder();
                postBackScript.Append("tinyMCE.triggerSave();");

                this.RegisterOnSubmitStatement(
                    this.GetType(), string.Format("TinyMCE_OnAjaxSubmit_{0}", this.ClientID), postBackScript.ToString());
            }
        }

        protected override void Render(HtmlTextWriter outWriter)
        {
            bool first = true;

            // Render HTML for TinyMCE instance
            if (!this.HasRenderedTextArea(this.Page))
            {
                DotNetNuke.UI.Utilities.ClientAPI.RegisterStartUpScript(Page, "tinymce", string.Format("<script type='text/javascript' src='{0}' ></script>", this.ScriptURI));
                _isRendered = true;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<script type='text/javascript'>");
            builder.AppendLine("//<![CDATA[");
            builder.AppendLine(";(function ($) {");
            builder.AppendLine("tinymce.init({");

            foreach (string key in this.Settings.Keys)
            {
                string val = this.Settings[key];

                if (!first)
                    builder.Append(",");
                else
                    first = false;

                // Is boolean state or string
                builder.AppendLine((val == "true" || val == "false") ? key + ":" + this.Settings[key] : key + ":\"" + this.Settings[key] + "\"");
            }
            builder.AppendLine("    });");
            // End TinyMCE

            builder.AppendLine("} (jQuery));");
            builder.AppendLine("//]]");
            builder.AppendLine("</script>");

            DotNetNuke.UI.Utilities.ClientAPI.RegisterStartUpScript(Page, string.Format("tinymce_{0}", this.ClientID), builder.ToString());

            // Write text area
            outWriter.AddAttribute("id", this.ClientID);
            outWriter.AddAttribute("name", this.UniqueID);

            if (this.CssClass.Length > 0)
                outWriter.AddAttribute("class", this.CssClass);

            if (this.Width.Value > 0)
                outWriter.AddStyleAttribute("width", this.Width.ToString());

            if (this.Height.Value > 0)
                outWriter.AddStyleAttribute("height", this.Height.ToString());

            outWriter.RenderBeginTag("textarea");
            outWriter.Write(this.Context.Server.HtmlEncode(this.Value));
            outWriter.WriteEndTag("textarea");
        }

        #endregion

        public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            string newContent = postCollection[postDataKey];

            if (newContent != this.Value)
            {
                this.Value = newContent;
                return true;
            }

            return false;
        }

        public void RaisePostDataChangedEvent()
        {
            // Do Nothing.
        }
    }
}
