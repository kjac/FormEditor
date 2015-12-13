# A note about storage
By default, Form Editor stores form submissions (including uploaded files) in a Lucene index under */App_Data/FormEditor/*. 

A new Lucene index is created per form in a sub directory named after the ID of the content item that contains the form. This means your editors can copy their forms in the Umbraco content tree and "start over" with a blank index.

## Changing the storage index
If you're hosting your site in a load balanced environment or in the cloud, the Lucene index might not be the best storage solution for you. Therefore the storage index is swappable in Form Editor. You can specify an alternative storage index in the `<Storage/>` section of [*/Config/FormEditor.config*](../Source/Umbraco/Config/FormEditor.config): 
```
<FormEditor>
  <!-- ... -->
  <Storage>
    <Index type="FormEditor.SqlIndex.Storage.Index, FormEditor.SqlIndex" />
  </Storage>
</FormEditor>
```

**Note**: You may have to restart the site to make Form Editor pick up the new configuration.

The storage index in the config sample above refers to the [sample implementation](../Source/Solution/FormEditor.SqlIndex) of an alternative storage index that stores form submissions in the database. You can create your own by implementing [`FormEditor.Storage.IIndex`](../Source/Solution/FormEditor/Storage/IIndex.cs).

