using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utilities
{
    public static class EmailHelper
    {
        public static byte[] SendMail(string from, string to, string subject, string BodyHtml, string attachmentFileName, byte[] attachment, bool html = true)
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add(to);
            message.Subject = subject;
            message.From = new System.Net.Mail.MailAddress(from);
            message.Body = BodyHtml;
            message.IsBodyHtml = html;
            if (attachmentFileName != null)
            {
                MemoryStream memStream = new MemoryStream(attachment);
                memStream.Position = 0;
                message.Attachments.Add(new System.Net.Mail.Attachment(memStream, attachmentFileName, null));
            }

            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
            try
            {
                smtp.Send(message);
            }
            catch
            {
                Thread.Sleep(500);
                smtp.Send(message);
            }
            Guid MailDir = Guid.NewGuid();
            smtp.PickupDirectoryLocation = Path.GetTempPath() + "/" + MailDir.ToString();
            Directory.CreateDirectory(smtp.PickupDirectoryLocation);
            smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory;
            smtp.Send(message);

            string[] files = Directory.GetFiles(smtp.PickupDirectoryLocation);
            byte[] emlToSave = File.ReadAllBytes(files[0]);
            Directory.Delete(smtp.PickupDirectoryLocation, true);
            return emlToSave;
        }
    }
}
