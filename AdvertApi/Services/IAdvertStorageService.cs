using AdvertApi.Models;

namespace AdvertApi.Services
{
    public interface IAdvertStorageService
    {
        Task<string> Add(AdvertModel model);

        Task<bool> Confirm(ConfirmAdvertModel model);

        Task<bool> CheckHealthAsync();

        Task<AdvertModel> GetById(string id);

        Task<List<AdvertModel>> GetAll();
    }
}
