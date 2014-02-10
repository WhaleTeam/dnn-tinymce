TinyMCE editor Provider for DotNetNuke
===========

Original code http://www.ventrian.com/resources/projects/tinymce-provider

Been modify to support Partial Render, TinyMCE 4 and DNN 7

Installations instructions
--------------------------

1. Copy assembly WhaleTeam.TinyMCE.dll to your DNN bin folder
2. Paste TinyMCE3 or TinyMCE4 folder into your ~/Providers/HtmlEditorProviders/
3. Add the following to your web.config (in the html editor provider area). Also make sure you set the defaultProvider attribute to the **WhaleTeam.TinyMCE**.

**for TinyMCE 4**
```    
    <add 
      name="WhaleTeam.TinyMCE" 
      type="WhaleTeam.TinyMCE.TinyMCEHtmlEditorProvider, WhaleTeam.TinyMCE" 
      providerPath="~/Providers/HtmlEditorProviders/TinyMCE4/" 
      tinymceVersion="4"
      mce_theme= "modern"
      mce_skin= "light"
      mce_plugins="advlist autolink link image lists charmap print preview hr anchor pagebreak spellchecker searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking save table contextmenu directionality emoticons template paste textcolor"
      mce_toolbar= "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | print preview media fullpage | forecolor backcolor emoticons"
      mce_relative_urls="false"
      mce_remove_script_host="false"
    />
```

**for TinyMCE 3**
```    
    <add 
      name="WhaleTeam.TinyMCE" 
      type="WhaleTeam.TinyMCE.TinyMCEHtmlEditorProvider, WhaleTeam.TinyMCE" 
      providerPath="~/Providers/HtmlEditorProviders/TinyMCE3/" 
      tinymceVersion="3"
      mce_mode="textareas"
	  mce_theme="simple"
      mce_relative_urls="false"
      mce_remove_script_host="false"
    />
```

**notes** : 
- providerPath is where you paste the TinyMCE sources.
- tinymceVersion for specific TinyMCE version (3 or 4)
- mce_[settings] is for all TinyMCE configuration, for detail visit http://www.tinymce.com/


Changelog
---------
####2014 Feb 11 - v1.0.1

Support TinyMCE 3 & TinyMCE 4

####2014 Feb 10 - v1.0.0

Release basic version of TinyMCE 4 Provider for DNN 7
