using System.Threading.Tasks;

namespace WorkFusionAPI.Interfaces
{
    public interface IImageService
    {
        Task<string> GetImageByUserIdAndRoleIdAsync(int userId, int roleId);
    }
}
