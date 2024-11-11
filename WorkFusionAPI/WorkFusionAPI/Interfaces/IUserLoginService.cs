using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IUserLoginService
    {
        Task<string> Authenticate(UserLoginModel loginModel);
    }
}
