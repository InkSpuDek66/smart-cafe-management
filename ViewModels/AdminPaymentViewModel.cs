// ViewModels/AdminPaymentViewModel.cs
// ViewModel สำหรับหน้า Verify สลิปในระบบ Admin

namespace Project_CSI402_T2_Y3.ViewModels;

// แสดงข้อมูล Payment แต่ละแถว
public class AdminPaymentRowViewModel
{
    public int      PaymentId           { get; set; }
    public int      OrderId             { get; set; }
    public string?  QueueNumber         { get; set; }
    public string?  TableNumber         { get; set; }
    public string?  CustomerName        { get; set; }
    public decimal  Amount              { get; set; }
    public string?  SlipUrl             { get; set; }
    public int      PaymentStatusId     { get; set; }
    public string?  StatusName          { get; set; }
    public string?  PaymentMethodName   { get; set; }
    public DateTime? CreatedAt          { get; set; }

    // ชื่อสถานะภาษาไทย
    public string StatusNameTh => PaymentStatusId switch
    {
        1 => "รอตรวจสอบ",
        2 => "อนุมัติแล้ว",
        3 => "ปฏิเสธ",
        _ => StatusName ?? "ไม่ทราบ"
    };
}

// ViewModel หน้า Verify Payment
public class AdminPaymentViewModel
{
    public List<AdminPaymentRowViewModel> Payments  { get; set; } = new();
    public string? SuccessMessage                   { get; set; }
    public string? ErrorMessage                     { get; set; }
}
