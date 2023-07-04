using ContosoPizza.Models.Generated;
using ContosoPizza.Pages.Products;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace ContosoPizza.Pages.EmailFormat
{
    public class EmailService
    {
        public async Task<IActionResult> SendEmail(string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("rachael.corkery20@ethereal.email"));
            email.To.Add(MailboxAddress.Parse("changtraikhongbietghen15069@gmail.com"));
            email.Subject = "Test email subject";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = body
            };
            using var smtp = new SmtpClient();
            smtp.Connect("smtp.gmail.email", 587, SecureSocketOptions.StartTls);
            smtp.Send(email);
            smtp.Disconnect(true);

            return new RedirectResult("/Products");

        }
    }
}
