namespace WorkFusionAPI.Interfaces
{
    public interface IForgotPasswordService
    {
        Task<bool> SendOTP(string email);
        Task<bool> ResetPassword(string email, string otp, string newPassword);
    }
}
