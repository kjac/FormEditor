using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormEditor.Interfaces
{
    public interface IFormModel
    {

        string EmailNotificationRecipients { get; set; }
        string EmailNotificationSubject { get; set; }
        string EmailNotificationFromAddress { get; set; }
        bool EmailNotificationAttachments { get; set; }
        string EmailConfirmationRecipientsField { get; set; }
        string EmailConfirmationSubject { get; set; }
        string EmailConfirmationBody { get; set; }
        string EmailConfirmationFromAddress { get; set; }
        int SuccessPageId { get; set; }
        string ReceiptHeader { get; set; }
        string ReceiptBody { get; set; }
        int? MaxSubmissions { get; set; }
        string MaxSubmissionsExceededHeader { get; set; }
        string MaxSubmissionsExceededText { get; set; }
        bool DisallowMultipleSubmissionsPerUser { get; set; }
        string MaxSubmissionsForCurrentUserExceededHeader { get; set; }
        string MaxSubmissionsForCurrentUserExceededText { get; set; }
    }
}
