// ViewModels/DashboardViewModel.cs
// ใช้ส่งข้อมูลสรุปภาพรวมระบบไปยังหน้า Dashboard

namespace Project_CSI402_T2_Y3.ViewModels;

public class DashboardViewModel
{
    public int TotalMembers { get; set; }
    public int TotalMenuItems { get; set; }
    public int ActiveMenuItems { get; set; }
    public int TotalStaff { get; set; }
    public int TodayOrderCount { get; set; }
    public decimal TodayRevenue { get; set; }

    // Top 5 เมนูขายดีของวันนี้
    public List<BestSellerItem> Top5BestSellers { get; set; } = new();

    // ต้นทุน Wastage วันนี้ (สำหรับ widget บน Dashboard)
    public decimal TodayWastageCost { get; set; }

    public List<LowStockAlertItem> LowStockAlerts { get; set; } = new();
}

// ข้อมูลเมนูขายดี
public class BestSellerItem
{
    public string MenuName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

// ข้อมูลแจ้งเตือนวัตถุดิบต่ำกว่าระดับที่กำหนด
public class LowStockAlertItem
{
    public string IngredientName { get; set; } = string.Empty;
    public float StockQuantity { get; set; }
    public float ReorderLevel { get; set; }
    public string Unit { get; set; } = string.Empty;
}
