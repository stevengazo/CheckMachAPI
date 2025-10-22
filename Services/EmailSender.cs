using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using CheckMachAPI.Settings;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(EmailSettings emailSettings)
    {
        _emailSettings = emailSettings;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}
