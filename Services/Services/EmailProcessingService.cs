 using Data.Models;
using HtmlAgilityPack;
using MailKit.Net.Smtp;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Services
{
    public class EmailProcessingService : IEmailProcessingService
    {
        IEmailService _emailService;

        public EmailProcessingService(IEmailService emailService)
        {
            _emailService = emailService;
        }


        public void ProcessIncomingEmails()
        {
            /// Processes all emails in the incoming to translate folder.
            /// Translates the email subject and body then sends it back to itself to be picked up by mailbox rule
            
            string folder = "IncomingToTranslate";
            string processedFolder = "IncomingToTranslate - Processed";
            // Gets all of the emails from the incoming to translate folder
            List<EmailMessage> emailList = _emailService.ReceiveEmail(folder);
            if (emailList.Count > 0)
            {
                // Declare new translation service
                TranslationService t = new TranslationService();

                // Who we're sending the translated email to
                EmailAddress serviceDeskAddress = new EmailAddress()
                {
                    Name = "Service Desk France",
                    Address = ""
                };

                foreach (var email in emailList)
                {
                    // Gets the address the email is from
                    EmailAddress originalFromAddress = new EmailAddress()
                    {
                        Name = email.FromAddresses[0].Name,
                        Address = email.FromAddresses[0].Address
                    };

                    // Creates new email message
                    EmailMessage sendingEmail = new EmailMessage();
                    sendingEmail.ToAddresses.Add(serviceDeskAddress);
                    sendingEmail.FromAddresses.Add(serviceDeskAddress);
                    // Creates the subject appending the original from address in the format the rest of the application uses, plus the original subject and a translated one
                    sendingEmail.Subject = "OrgFrom[" + originalFromAddress.Address + "]end | " + t.Translate(email.Subject, "en") + " | " + email.Subject;
                    sendingEmail.Attachments = email.Attachments;

                    // Creates a new HTML body, loads in our HTML, and loads in nodes which contain plain text
                    HtmlDocument mainDoc = new HtmlDocument();
                    mainDoc.LoadHtml(email.Content);
                    var nodes = mainDoc.DocumentNode.SelectNodes("//body//text()[(normalize-space(.) != '') and not(parent::script) and not(*)]");

                    // Replaces the text within each node with translated text
                    foreach (HtmlNode htmlNode in nodes)
                    {
                        htmlNode.ParentNode.ReplaceChild(HtmlTextNode.CreateNode(t.Translate(htmlNode.InnerText, "en")), htmlNode);
                    }

                    // Sets our email content to the newly translated html body
                    sendingEmail.Content = mainDoc.DocumentNode.OuterHtml;

                    // Sends email
                    _emailService.Send(sendingEmail);
                    _emailService.Move(email, folder, processedFolder);
                }

            }
        }

        public void ProcessOutgoingEmails()
        {
            string folder = "OutgoingToTranslate";
            string processedFolder = "OutgoingToTranslate - Processed";
            // Gets all of the emails from the OutgoingToTranslate folder
            List<EmailMessage> emailList = _emailService.ReceiveEmail(folder);
            if (emailList.Count > 0)
            {
                // Declare new translation service
                TranslationService t = new TranslationService();

                // Who we're sending the translated email to
                EmailAddress serviceDeskAddress = new EmailAddress()
                {
                    Name = "Service Desk France",
                    Address = ""
                };

                // Loops through each email we retrieved and gets the address it's going to from either the subject or the body
                // Sends an error report if it can't be found
                foreach (var email in emailList)
                {
                    string orgFrom = String.Empty;
                    string subject = email.Subject;
                    string body = email.Content;
                    if (subject.Contains("OrgFrom[") && subject.Contains("]end"))
                    {
                        int pFrom = subject.IndexOf("OrgFrom[") + "OrgFrom[".Length;
                        int pTo = subject.LastIndexOf("]end");
                        orgFrom = subject.Substring(pFrom, pTo - pFrom);
                    } else if (body.Contains("OrgFrom[") && body.Contains("]end"))
                    {
                        int pFrom = body.IndexOf("OrgFrom[") + "OrgFrom[".Length;
                        int pTo = body.LastIndexOf("]end");
                        orgFrom = body.Substring(pFrom, pTo - pFrom);
                    } else
                    {
                        ErrorReport(email, "ProcessOutgoingEmails", "Email has no org from address in subject or body, unsure who to send it to");
                    }
                    
                    // Gets the address the email is from
                    EmailAddress originalFromAddress = new EmailAddress()
                    {
                        Address = orgFrom
                    };

                    // Creates and translates the new email message
                    EmailMessage sendingEmail = new EmailMessage();
                    sendingEmail.ToAddresses.Add(originalFromAddress);
                    sendingEmail.FromAddresses.Add(serviceDeskAddress);
                    sendingEmail.Subject = t.Translate(email.Subject, "fr");
                    sendingEmail.Content = email.Content;

                    HtmlDocument mainDoc = new HtmlDocument();
                    mainDoc.LoadHtml(email.Content);
                    var nodes = mainDoc.DocumentNode.SelectNodes("//body//text()[(normalize-space(.) != '') and not(parent::script) and not(*)]");
                    foreach (HtmlNode htmlNode in nodes)
                    {
                        htmlNode.ParentNode.ReplaceChild(HtmlTextNode.CreateNode(t.Translate(htmlNode.InnerText, "fr")), htmlNode);
                    }

                    sendingEmail.Content = mainDoc.DocumentNode.OuterHtml;

                    // Sends email
                    _emailService.Send(sendingEmail);
                    _emailService.Move(email, folder, processedFolder);
                }

            }
        }

        public void ErrorReport(EmailMessage email, string method, string exception)
        {
            EmailAddress errorAddress = new EmailAddress()
            {
                Address = ""
            };

            email.Subject = "TRANSLATION ERROR - " + email.Subject;
            email.Content = "The below email has failed or ran into a problem and may have failed to send.</br>" +
                "It failed in the following method - <strong>" + method + "</strong>.</br>" +
                "With the following error if applicable - " + exception + "</br>" +
                "Email Content - </br>" + email.Content;
            email.ToAddresses.Clear();
            email.ToAddresses.Add(errorAddress);
            _emailService.Send(email);
        }
    }
}
