using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPurchaseNotificationAsync(string sellerEmail, string sellerUsername, string gameTitle, double amount)
        {
            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);
    //this is a hardcoded sollution!!!
                var from = new EmailAddress("gevoge4672@ixospace.com", "SEBIZ");
                var to = new EmailAddress(sellerEmail, sellerUsername);
                var subject = "Game Purchase Notification";
                var htmlContent = $@"
<p>Dear {sellerUsername},</p>
<p>Great news! Your game '<strong>{gameTitle}</strong>' has been purchased!</p>
<p><strong>Amount Earned:</strong> ${amount:F2}</p>
<p>This amount has been added to your account balance and is now available for you to use or withdraw.</p>
<p>Thank you for sharing your game with our community!</p>
<p>Best regards,<br>SEBIZ Team</p>
";

                var msg = new SendGridMessage()
                {
                    From = from,
                    Subject = subject,
                    HtmlContent = htmlContent
                };
                msg.AddTo(to);

                var response = await client.SendEmailAsync(msg);
                _logger.LogInformation($"Purchase notification sent to {sellerEmail} - Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {sellerEmail}: {ex.Message}");
            }
        }

        public async Task SendBuyerPurchaseNotificationAsync(string buyerEmail, string buyerUsername, string gameTitle, double amount)
        {
            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("gevoge4672@ixospace.com", "SEBIZ");
                var to = new EmailAddress(buyerEmail, buyerUsername);
                var subject = "Purchase Confirmation - SEBIZ";
                var htmlContent = $@"
<p>Dear {buyerUsername},</p>
<p>Thank you for your purchase!</p>
<p><strong>Game:</strong> {gameTitle}</p>
<p><strong>Amount Spent:</strong> ${amount:F2}</p>
<p>Your purchase has been completed successfully. You can now access and play this game in your library.</p>
<p>Enjoy playing!</p>
<p>Best regards,<br>SEBIZ Team</p>
";

                var msg = new SendGridMessage()
                {
                    From = from,
                    Subject = subject,
                    HtmlContent = htmlContent
                };
                msg.AddTo(to);

                var response = await client.SendEmailAsync(msg);
                _logger.LogInformation($"Purchase confirmation sent to {buyerEmail} - Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {buyerEmail}: {ex.Message}");
            }
        }
    }
}
