# Form Editor scheduled jobs
*Don't worry! Form Editor works right out of the box without any scheduled jobs running. At the time of writing, the only feature that requires a scheduled job to work is the [automatic deletion of expired submissions](install.md#automatic-deletion-of-old-submissions).*

## Authenticating
When running scheduled jobs with the scheduler that's built into Umbraco, you need publicly available endpoints to do the work. To add a layer of safety, Form Editor scheduled jobs require you to pass an authentication token in the querystring parameter `authToken`.

The authentication token is configured in the `<Jobs>` section of */Config/FormEditor.config*:

```xml
<FormEditor>
  <Jobs authToken="something-very-secret"></Jobs>
  
  <!-- ... -->
</FormEditor>
```

## Invoking jobs
You can invoke the job that handles deletion of expired submissions by adding it to the Umbraco scheduler. This is done in the `<scheduledTasks>` section of */Config/umbracoSettings.config*:

```xml
<settings>
  <!-- ... -->
  
  <scheduledTasks>
    <!-- run the PurgeExpiredSubmissions job every 6 hours (21600 seconds) -->
    <task log="true" alias="FormEditorPurgeExpiredSubmissions" interval="21600" url="[your site host]/umbraco/FormEditorApi/Jobs/PurgeExpiredSubmissions/?authToken=[your authentication token]"/>
  </scheduledTasks>
  
  <!-- ... -->
</settings>
```

You can read more about the Umbraco scheduler [here](https://our.umbraco.org/documentation/Reference/Config/umbracoSettings/#scheduledtasks).

## Alternative job schedulers
If you don't want to use the Umbraco scheduler, you can invoke the Form Editor jobs with any other job scheduler that can reach the */umbraco/* section of your site.

The Form Editor jobs can be invoked with both GET and POST requests (though you must provide the `authToken` parameter in the querystring even for POST requests).
