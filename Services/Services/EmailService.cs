using Data.Interfaces;
using Data.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

		IMailFolder toTranslate;
		IMailFolder toTranslateProcessed;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
			_emailConfiguration.SmtpUsername = Environment.GetEnvironmentVariable("SmtpUsername");
			_emailConfiguration.SmtpPassword = Environment.GetEnvironmentVariable("SmtpPassword");
		}

        public List<EmailMessage> ReceiveEmail(string toTranslateFolderName)
        {
			/// Takes in a folder name, polls that folder for emails and if any exist it returns them as a list of EmailMessage objects
			// Client connection
			var client = new ImapClient();
			client.Connect(_emailConfiguration.IMAPServer, _emailConfiguration.IMAPPort, SecureSocketOptions.SslOnConnect);
			client.AuthenticationMechanisms.Remove("XOAUTH2");
			client.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

			// Get receiving folder
			toTranslate = client.GetFolder(toTranslateFolderName);
			using (client)
			{
				toTranslate.Open(FolderAccess.ReadWrite);
				List<EmailMessage> emails = new List<EmailMessage>();

				// fetch some useful metadata about each message in the folder...
				var items = toTranslate.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Body);

				// iterate over all of the messages and fetch them by UID
				foreach (var item in items)
				{
					var message = toTranslate.GetMessage(item.UniqueId);


					// Create email message
					var emailMessage = new EmailMessage
					{
						Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
						Subject = message.Subject,
						Attachments = message.Attachments.ToList(),
						UID = item.UniqueId
					};

					emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
					emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
					emailMessage.Content = PopulateInlineImages(message, emailMessage.Content);
					emails.Add(emailMessage);
				}
				return emails;
			}

		}

        public void Send(EmailMessage emailMessage)
        {
			/// Takes in email message and sends it
			/// 
			// Creates MineMessage and adds to and from addresses and subject from our emailMessage
			var message = new MimeMessage();
			message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
			message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
			message.Subject = emailMessage.Subject;

			var builder = new BodyBuilder();

            //// Adds all of the attachments to the new email
			if(emailMessage.Attachments != null && emailMessage.Attachments.Count() > 0)
            {
				foreach (var file in emailMessage.Attachments)
				{
					builder.Attachments.Add(file);
				}
			}

			// Uses the content from our emailMessage to build the MimeMessages HTML body
            builder.HtmlBody = emailMessage.Content;
			message.Body = builder.ToMessageBody();

			// Uses mailkits SmtpClient class to send the email
			using (var emailClient = new SmtpClient())
			{
				//The last parameter here is to use SSL
				emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.StartTls);

				//Remove any OAuth functionality as we won't be using it. 
				emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

				emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

				emailClient.Send(message);

				emailClient.Disconnect(true);
			}

		}

        public void Move(EmailMessage email, string toTranslateFolderName, string toTranslateProcessedFolderName)
        {
			/// Moves email from the first folder to the second
			
			// Creates instant of mailkits imap client and authenticates
			var client = new ImapClient();
			client.Connect(_emailConfiguration.IMAPServer, _emailConfiguration.IMAPPort, SecureSocketOptions.SslOnConnect);
			client.AuthenticationMechanisms.Remove("XOAUTH2");
			client.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

			// Gets both of the ofolders and open the first as we need to alter mail within that folder
			toTranslate = client.GetFolder(toTranslateFolderName);
			toTranslateProcessed = client.GetFolder(toTranslateProcessedFolderName);
			toTranslate.Open(FolderAccess.ReadWrite);

			// Moves the email that was passed into the method
			using (client)
            {
				toTranslate.MoveTo(email.UID, toTranslateProcessed);
            }
        }

        public string PopulateInlineImages(MimeMessage newMessage, string bodyHtml)
		{
			/// Adds the embedded images into the HTML
			if (bodyHtml != null)
			{
				foreach (MimePart att in newMessage.BodyParts)
				{
					if (att.ContentId != null && att.Content != null && att.ContentType.MediaType == "image" && (bodyHtml.IndexOf("cid:" + att.ContentId) > -1))
					{
						byte[] b;
						using (var mem = new MemoryStream())
						{
							att.Content.DecodeTo(mem);
							b = mem.ToArray();
						}
						string imageBase64 = "data:" + att.ContentType.MimeType + ";base64," + Convert.ToBase64String(b);
						bodyHtml = bodyHtml.Replace("cid:" + att.ContentId, imageBase64);
					}
				}
			}
			return bodyHtml;
		}
	}
}
