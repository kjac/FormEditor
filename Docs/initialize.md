# Create a default form
If you'd like to have a default form added whenever your editors create content, you can create the form programmatically by hooking into Umbracos `ContentService.Created` event. 

The following sample creates a default contact form for all newly created "myFormContentType" content.

```cs
public class UmbracoEventsHandler : Umbraco.Core.ApplicationEventHandler
{
  protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
  {
    // hook into the Created event
    ContentService.Created += ContentServiceOnCreated;
  }

  private void ContentServiceOnCreated(IContentService sender, NewEventArgs<IContent> newEventArgs)
  {
    // if creating content of type "myFormContentType", add a default form
    if (newEventArgs.Alias == "myFormContentType")
    {
      // create a one page form containing two rows:
      // - one two-column row with a name and an email field
      // - one one-column row with a message field and a submit button
      var form = new FormModel
      {
        Pages = new List<Page>
        {
          new Page
          {
            Rows = new List<Row>
            {
              // the row and cell aliases used below are the default aliases from the Form Editor data type - replace them with your own
              new Row
              {
                Alias = "two-column",
                Cells = new List<Cell>
                {
                  new Cell
                  {
                    Alias = "col-md-6",
                    Fields = new List<Field>
                    {
                      new TextBoxField
                      {
                        Name = "Name",
                        Label = "Your name",
                        ErrorMessage = "Please enter your name",
                        Placeholder = "Enter your name",
                        Mandatory = true
                      }
                    }
                  },
                  new Cell
                  {
                    Alias = "col-md-6",
                    Fields = new List<Field>
                    {
                      new EmailField
                      {
                        Name = "Email",
                        Label = "Your email",
                        ErrorMessage = "Please enter your email",
                        Placeholder = "Enter your email",
                        Mandatory = false,
                        HelpText = "Your email will not be displayed on the site."
                      }
                    }
                  }

                }
              },
              new Row
              {
                Alias = "one-column",
                Cells = new List<Cell>
                {
                  new Cell
                  {
                    Alias = "col-md-12",
                    Fields = new List<Field>
                    {
                      new TextAreaField
                      {
                        Name = "Message",
                        Label = "Your message",
                        ErrorMessage = "Please enter your message",
                        Placeholder = "Enter your message",
                        Mandatory = true
                      },
                      new SubmitButtonField
                      {
                        Text = "Send your message"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      };

      // use the Form Editor serialization helper to serialize the form to JSON
      newEventArgs.Entity.SetValue("form", SerializationHelper.SerializeFormModel(form));
    }
  }
}
```