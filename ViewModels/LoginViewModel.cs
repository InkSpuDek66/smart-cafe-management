// ViewModels/LoginViewModel.cs
// ใช้รับข้อมูลจากฟอร์มเข้าสู่ระบบของพนักงาน

namespace Project_CSI402_T2_Y3.ViewModels;

public class LoginViewModel
{
    // รับได้ทั้ง StaffId (ตัวเลข 6 หลัก) หรือ Username
    public string Identifier { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
