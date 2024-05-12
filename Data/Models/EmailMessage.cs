using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public class EmailMessage
    {
		public EmailMessage()
		{
			ToAddresses = new List<EmailAddress>();
			FromAddresses = new List<EmailAddress>();
		}

		public List<EmailAddress> ToAddresses { get; set; }
		public List<EmailAddress> FromAddresses { get; set; }
		public List<MimeEntity> Attachments { get; set; }
		public string Subject { get; set; }
		public string Content { get; set; }
		public UniqueId UID { get; set; }
	}
}
