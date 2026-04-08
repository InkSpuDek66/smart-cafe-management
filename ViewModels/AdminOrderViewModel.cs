// ViewModels/AdminOrderViewModel.cs
// ViewModels สำหรับการจัดการออเดอร์ในหน้า Admin

namespace Project_CSI402_T2_Y3.ViewModels;

// แสดงออเดอร์แต่ละแถวในตาราง
public class AdminOrderRowViewModel
{
    public int      OrderId         { get; set; }
    public string?  TableNumber     { get; set; }
    public string?  CustomerName    { get; set; }
    public string?  QueueNumber     { get; set; }
    public decimal  NetAmount       { get; set; }
    public int      OrderStatusId   { get; set; }
    public string?  StatusName      { get; set; }
    public DateTime? CreatedAt      { get; set; }
    public int      ItemCount       { get; set; }

    // CSS class ของ Badge ตามสถานะ
    public string StatusBadgeClass => OrderStatusId switch
    {
        1 => "badge-warning",
        2 => "badge-info",
        3 => "badge-primary",
        4 => "badge-success",
        5 => "badge-neutral",
        6 => "badge-error",
        _ => "badge-ghost"
    };

    // ชื่อสถานะภาษาไทย — ใช้แทน StatusName (English) จาก DB
    public string StatusNameTh => OrderStatusId switch
    {
        1 => "รอชำระ",
        2 => "ชำระแล้ว",
        3 => "กำลังทำ",
        4 => "พร้อมเสิร์ฟ",
        5 => "เสร็จสิ้น",
        6 => "ยกเลิก",
        _ => StatusName ?? "ไม่ทราบ"
    };
}

// ViewModel หน้ารายการออเดอร์
public class AdminOrderViewModel
{
    public List<AdminOrderRowViewModel> Orders  { get; set; } = new();
    public string?  FilterStatus                { get; set; }
    public string?  SuccessMessage              { get; set; }
}
