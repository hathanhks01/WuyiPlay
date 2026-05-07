using WuyiPlay_BLL.Services;

namespace WuyiPlay_BLL.IServices
{
    public interface ISePayService
    {
        Dictionary<string, string> CreatePaymentFields(SePayPaymentRequest request);
        string GetCheckoutUrl();
    }
}