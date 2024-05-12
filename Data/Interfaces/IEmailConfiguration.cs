using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Interfaces
{
    public interface IEmailConfiguration
    {
		string SmtpServer { get; }
		int SmtpPort { get; }
		string SmtpUsername { get; set; }
		string SmtpPassword { get; set; }

		string IMAPServer { get; }
		int IMAPPort { get; }
		string IMAPUsername { get; }
		string IMAPPassword { get; }
	}
}
