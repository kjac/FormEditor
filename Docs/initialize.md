# Creating a default form
If you'd like to have a default form added whenever your editors create content, you have two options:

1. Use *Content Templates* (recommended) if your site is running Umbraco 7.7 or later.
2. Create the form programmatically. 

## Using Content Templates
*Content Templates* is a new feature in Umbraco 7.7. It's awesome. And it works with Form Editor too! 

This is not the place to start documenting Content Templates, but if you're running Umbraco 7.7 or later, you should definitively use them for default forms (and all other default content).

Remember that you can lock down the Form Editor features made available to the editors by changing the [tab order and availability](install.md#tab-order-and-availiability). In combination with Content Templates, this is pretty powerful. Consider a scenario where you want a form on every new instance of a certain content type, but you want to ensure that the entire form configuration (layout, receipts, validations etc) remains the same across all instances. With a Content Template you can create the form exactly as you want it, and then lock down the data type so only the *Submissions* tab remains visible to the editors.

## Creating the form programmatically
If you can't use *Content Templates*, you can create forms programmatically by hooking into the Umbraco `ContentService.Created` event.

The following sample creates a default contact form for all newly created "myFormContentType" content.

```cs
public class UmbracoEventsHandler : ApplicationEventHandler
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
              // the row and cell aliases used below are the default aliases from the 
              // Form Editor data type - replace them with your own
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

      // serialize the form to JSON using the Form Editor serialization helper
      // and store the JSON in the "form" property
      newEventArgs.Entity.SetValue("form", SerializationHelper.SerializeFormModel(form));
    }
  }
}
```