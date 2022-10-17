using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.Utilities;

public class EmailSender : IEmailSender
{
    // Used for getting values from user secrets
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Create email message
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse("welcome@bookheap.com"));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

        // Send email
        using (var emailClient = new SmtpClient())
        {
            emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            emailClient.Authenticate(_configuration["Email:Username"], _configuration["Email:Password"]);
            emailClient.Send(message);
            emailClient.Disconnect(true);
        }

        return Task.CompletedTask;
    }
}
