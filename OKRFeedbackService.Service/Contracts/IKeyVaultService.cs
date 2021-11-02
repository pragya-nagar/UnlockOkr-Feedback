using OKRFeedbackService.ViewModel.Response;
using System.Threading.Tasks;

namespace OKRFeedbackService.Service.Contracts
{
    public interface IKeyVaultService
    {
        Task<BlobVaultResponse> GetAzureBlobKeysAsync();
        Task<ServiceSettingUrlResponse> GetSettingsAndUrlsAsync();
    }
}
