// ViewModels/MemberViewModel.cs
// ใช้รับและส่งข้อมูลสมาชิกระหว่าง Controller และ View
// MemberId ถูกลบออก เพราะระบบ Generate อัตโนมัติ ผู้ใช้ไม่ต้องกรอก

namespace Project_CSI402_T2_Y3.ViewModels;

public class MemberViewModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? BirthDate { get; set; }

    // คะแนนและแสตมป์ใช้สำหรับแสดงผลเท่านั้น ไม่อนุญาตให้แก้ไขผ่านฟอร์ม
    public string Points { get; set; } = "0";
    public string StampBalance { get; set; } = "0";
}
