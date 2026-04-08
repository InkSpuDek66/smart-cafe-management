// ViewModels/LoginViewModel.cs
// ใช้รับข้อมูลจากฟอร์มเข้าสู่ระบบของพนักงาน

namespace Project_CSI402_T2_Y3.ViewModels;

public class LoginViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
