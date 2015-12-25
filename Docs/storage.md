# A note about storage
By default, Form Editor stores form submissions (including uploaded files) in a Lucene index under */App_Data/FormEditor/*. 

A new Lucene index is created per form in a sub directory named after the ID of the content item that contains the form. This means your editors can copy their forms in the Umbraco content tree and "start over" with a blank index.

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

You can create your own storage index by implementing [`FormEditor.Storage.IIndex`](../Source/Solution/FormEditor/Storage/IIndex.cs). There are two sample implementations of storage indexes in the samples section:

### SQL storage index
The [SQL storage index](../Samples/SQL storage index/) stores form submissions in the Umbraco database (with limited sorting options). 

The necessary tables for this index are automatically created if they do not exist when the site starts up.

### Elastic storage index
The [Elastic storage index](../Samples/Elastic storage index/) stores form submissions in an Elastic index. 

You must supply the Elastic connection string as `FormEditor.ElasticIndex` in the `<connectionStrings>` section of your `web.config` file - like this:

```xml
<connectionStrings>
  <!-- ... -->
  <add name="FormEditor.ElasticIndex" connectionString="[your Elastic connection string]" />
</connectionStrings>
```


