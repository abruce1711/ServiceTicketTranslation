using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Interfaces
{
    public interface IEmailProcessingService
    {
        public void ProcessIncomingEmails();
        public void ProcessOutgoingEmails();
    }
}
