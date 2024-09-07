
using ECO.WebApi.Application.Common.Mailing;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ECO.WebApi.Infrastructure.Mailing;

public class SmtpMailService : IMailService
{
    private readonly SMTPEmailSettings _settings;
    private readonly ILogger<SmtpMailService> _logger;
    private readonly SmtpClient _smtpClient;
    public SmtpMailService(IOptions<SMTPEmailSettings> settings, ILogger<SmtpMailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _smtpClient = new SmtpClient();
    }

    public async Task SendAsync(MailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = new MimeMessage();

            // From
            email.From.Add(new MailboxAddress(_settings.DisplayName, request.From ?? _settings.From));

            // To
            foreach (string address in request.To)
                email.To.Add(MailboxAddress.Parse(address));

            // Reply To
            if (!string.IsNullOrEmpty(request.ReplyTo))
                email.ReplyTo.Add(new MailboxAddress(request.ReplyToName, request.ReplyTo));

            // Bcc
            if (request.Bcc != null)
            {
                foreach (string address in request.Bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                    email.Bcc.Add(MailboxAddress.Parse(address.Trim()));
            }

            // Cc
            if (request.Cc != null)
            {
                foreach (string? address in request.Cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                    email.Cc.Add(MailboxAddress.Parse(address.Trim()));
            }

            // Headers
            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                    email.Headers.Add(header.Key, header.Value);
            }

            // Content
            var builder = new BodyBuilder();
            email.Sender = new MailboxAddress(request.DisplayName ?? _settings.DisplayName, request.From ?? _settings.From);
            email.Subject = request.Subject;
            builder.HtmlBody = request.Body;

            // Create the file attachments for this e-mail message
            if (request.AttachmentData != null)
            {
                foreach (var attachmentInfo in request.AttachmentData)
                    builder.Attachments.Add(attachmentInfo.Key, attachmentInfo.Value);
            }

            email.Body = builder.ToMessageBody();

            await _smtpClient.ConnectAsync(_settings.SMTPServer, _settings.Port,
                  _settings.UseSsl, cancellationToken); // Task

            await _smtpClient.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            await _smtpClient.SendAsync(email, cancellationToken);

            await _smtpClient.DisconnectAsync(quit: true, cancellationToken);

            await _smtpClient.DisconnectAsync(quit: true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}
