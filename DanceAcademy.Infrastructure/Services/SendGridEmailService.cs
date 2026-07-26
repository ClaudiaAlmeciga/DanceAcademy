#nullable enable
using DanceAcademy.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DanceAcademy.Infrastructure.Services;

public sealed class SendGridEmailService(
    IConfiguration configuration,
    ILogger<SendGridEmailService> logger) : IEmailService
{
    public async Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken ct = default)
    {
        var apiKey   = configuration["SendGrid:ApiKey"];
        var fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@danceacademy.local";
        var fromName  = configuration["SendGrid:FromName"]  ?? "Dance Academy";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("SendGrid:ApiKey no configurada. Email de reset no enviado a {Email}.", toEmail);
            return;
        }

        var client  = new SendGridClient(apiKey);
        var from    = new EmailAddress(fromEmail, fromName);
        var to      = new EmailAddress(toEmail);
        var subject = "Restablecer contraseña — Dance Academy";
        var html    = $"""
            <p>Recibimos una solicitud para restablecer tu contraseña.</p>
            <p><a href="{resetLink}" style="background:#4f46e5;color:#fff;padding:10px 20px;border-radius:6px;text-decoration:none;">Restablecer contraseña</a></p>
            <p>El enlace expira en <strong>1 hora</strong>.</p>
            <p>Si no solicitaste este cambio, ignora este mensaje.</p>
            """;

        var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, html);
        var response = await client.SendEmailAsync(message, ct);

        if (!response.IsSuccessStatusCode)
            logger.LogError("SendGrid respondió {StatusCode} al enviar reset a {Email}.", response.StatusCode, toEmail);
    }
}
