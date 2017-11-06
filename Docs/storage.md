# A note about storage
By default, Form Editor stores form submissions (including uploaded files) in a Lucene index under */App_Data/FormEditor/*. 

A new Lucene index is created per form in a sub directory named after the ID of the content item that contains the form. This means your editors can copy their forms in the Umbraco content tree and "start over" with a blank index.

## Cleaning up storage indexes
When deleting a content item (from the recycle bin) that contains a Form Editor, the storage index is deleted along with it, because the form submissions belong to the content item. 

If you want to change this behavior, add the application setting `FormEditor.PreserveIndexes` to `<appSettings>` and set its value to `true`:
```xml
  <appSettings>
    <!-- ... -->
    <add key="FormEditor.PreserveIndexes" value="true" />
  </appSettings>
```

*Note that features like the global search and deletion of old submissions ([read more](install.md#other-configuration-options)) only work with forms that exist within the content tree.*

## Changing the storage index
If you're hosting your site in a load balanced environment or in the cloud, the Lucene index might not be the best storage solution for you. Therefore the storage index is swappable in Form Editor. You can specify an alternative storage index in the `<Storage/>` section of [*/Config/FormEditor.config*](../Source/Umbraco/Config/FormEditor.config): 

```xml
<FormEditor>
  <!-- ... -->
  <Storage>
    <Index type="FormEditor.SqlIndex.Storage.Index, FormEditor.SqlIndex" />
  </Storage>
</FormEditor>
```

**Note**: You may have to restart the site to make Form Editor pick up the new configuration.

You can create your own storage index by implementing [`FormEditor.Storage.IIndex`](../Source/Solution/FormEditor/Storage/IIndex.cs). If you want your storage index to support full text search, you'll also need to implement [`FormEditor.Storage.IFullTextIndex`](../Source/Solution/FormEditor/Storage/IFullTextIndex.cs).

There are two sample implementations of storage indexes in the samples section:

### SQL storage index
The [SQL storage sample index](../Samples/SQL%20storage%20index/) stores form submissions in the Umbraco database. 

The necessary tables for this index are automatically created if they do not exist when the site starts up.

### Elastic storage index
The [Elastic storage sample index](../Samples/Elastic%20storage%20index/) stores form submissions in an Elastic index.

You must supply the Elastic connection string as `FormEditor.ElasticIndex` in the `<connectionStrings>` section of your `web.config` file - like this:

```xml
<connectionStrings>
  <!-- ... -->
  <add name="FormEditor.ElasticIndex" connectionString="[your Elastic connection string]" />
</connectionStrings>
```

## Supporting statistics
To make your index work with the build-in statistics it must implement the [`IStatisticsIndex`](../Source/Solution/FormEditor/Storage/Statistics/IStatisticsIndex.cs) interface. Nothing else is required - Form Editor will automatically enable statistics if it's backed by an index that implements this interface.

**Please note:** The `IStatisticsIndex` interface will most likely change over time, which might cause breaking changes for you when you upgrade Form Editor.

None of the sample indexes currently support statistics, but you can have a look at the [`default index implementation`](../Source/Solution/FormEditor/Storage/Index.cs) for inspiration.

## Supporting GDPR
The General Data Protection Regulation [introduces a few challenges](GDPR.md). To this end you should consider facilitating full text search and automation by implementing the [`IFullTextIndex`](../Source/Solution/FormEditor/Storage/IFullTextIndex.cs) and [`IFullTextIndex`](../Source/Solution/FormEditor/Storage/IAutomationIndex.cs) interfaces.

**Please note:** The `IAutomationIndex` interface will most likely change over time, which might cause breaking changes for you when you upgrade Form Editor.

The [SQL storage sample index](../Samples/SQL%20storage%20index/Storage/Index.cs) implements both interfaces, as does of course the [`default index implementation`](../Source/Solution/FormEditor/Storage/Index.cs) for inspiration.
