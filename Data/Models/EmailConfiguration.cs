using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public class EmailConfiguration : IEmailConfiguration
    {
		public string SmtpServer { get; set; }
		public int SmtpPort { get; set; }
		public string SmtpUsername { get; set; }
		public string SmtpPassword { get; set; }

		public string IMAPServer { get; set; }
		public int IMAPPort { get; set; }
		public string IMAPUsername { get; set; }
		public string IMAPPassword { get; set; }
	}
}
