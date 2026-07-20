using System.Net;
using System.Net.Mail;
using AgriVision.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AgriVision.Infrastructure.Identity;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"]!;
        var port = int.Parse(_configuration["EmailSettings:Port"]!);
        var senderName = _configuration["EmailSettings:SenderName"]!;
        var senderEmail = _configuration["EmailSettings:SenderEmail"]!;
        var senderPassword = _configuration["EmailSettings:SenderPassword"]!;

        using var client = new SmtpClient(smtpServer, port)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}