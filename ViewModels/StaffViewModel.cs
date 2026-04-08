// ViewModels/StaffViewModel.cs
// ใช้รับข้อมูลพนักงานจากฟอร์ม AddStaff
// StaffId ถูกลบออก เพราะระบบ Generate อัตโนมัติ ผู้ใช้ไม่ต้องกรอก

namespace Project_CSI402_T2_Y3.ViewModels;

public class StaffViewModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string Password { get; set; } = string.Empty;

    // รหัสตำแหน่ง: 1=Barista 2=Cashier 3=Store_Manager 4=Finance 5=Owner
    public string? StaffRoleId { get; set; }
}
