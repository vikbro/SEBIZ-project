using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public interface IEmailService
    {
        Task SendPurchaseNotificationAsync(string sellerEmail, string sellerUsername, string gameTitle, double amount);
        Task SendBuyerPurchaseNotificationAsync(string buyerEmail, string buyerUsername, string gameTitle, double amount);
    }
}
