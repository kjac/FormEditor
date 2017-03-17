# Setting up web service integration
Form Editor can send form data automatically to an external web service upon a successful form submission. This is configured per Form Editor data type in the **Web service** section of the data type configuration:

![Form Editor web service integration](img/web service integration.png)

Whenever a form is submitted to a page that contains this data type, Form Editor will attempt to perform a POST request to this URL with the submitted data and a few other bits of useful information (see below). 

You can optionally enter a user name and a password, which will then be used to perform basic authentication when calling the web service.

*Note:* The submitted data will still be saved to the storage index and made available in the Umbraco back office. 

*Also note:* The POST request is a one-shot action. There is no retry policy - if the web service is not responding, the request will silently fail (the error will be logged to the Umbraco log). 

## The data format
The data is sent to the web service as a JSON object structured as follows:

```json
{
  "umbracoContentName": "A name",
  "umbracoContentId": 1234,
  "indexRowId": "fa7f92ff-2b64-4b44-a5e3-2323a431a3a9",
  "formData": [
    {
      "name": "Name",
      "formSafeName": "_Name",
      "type": "core.textbox",
      "submittedValue": "It's my name"
    },
    {
      "name": "Date",
      "formSafeName": "_Date",
      "type": "core.date",
      "submittedValue": "2017-04-15"
    },
    {
      "name": "Email",
      "formSafeName": "_Email",
      "type": "core.email",
      "submittedValue": "its@my.email"
    }
  ]
}
```

* `umbracoContentName` and `umbracoContentId` contain the name and ID of the page that the form was submitted to.
* `indexRowId` is the ID of the form submission in the storage index.
* `formData` is an array of the submitted form data fields. Each entry contains the name and "form safe name" of the field, the field type and of course the submitted field value.
