using Data.Models;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Interfaces
{
    public interface IEmailService
    {
        void Send(EmailMessage emailMessage);
        List<EmailMessage> ReceiveEmail(string toTranslateFolderName);
        string PopulateInlineImages(MimeMessage newMessage, string bodyHtml);
        public void Move(EmailMessage email, string toTranslateFolderName, string toTranslateProcessedFolderName);
    }
}
