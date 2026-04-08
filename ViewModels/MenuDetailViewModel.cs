// ViewModels/MenuDetailViewModel.cs
// ViewModel หน้ารายละเอียดเมนู — แสดงรูปสินค้าและตัวเลือก

using Project_CSI402_T2_Y3.Models.Db;

namespace Project_CSI402_T2_Y3.ViewModels;

public class MenuDetailViewModel
{
    public Menuitem Item            { get; set; } = null!;
    public string? TableNumber      { get; set; }

    // Options ระดับความหวาน
    public List<string> SweetnessOptions { get; set; } = new()
    {
        "หวานปกติ", "หวานน้อย", "หวานน้อยมาก", "ไม่หวาน"
    };

    // Options ประเภทนม — มีส่วนปรับราคาด้วย
    public List<(string Label, decimal PriceAdj)> MilkOptions { get; set; } = new()
    {
        ("นมสด", 0),
        ("นมข้นหวาน", 0),
        ("นมโอ๊ต (+10)", 10),
        ("นมอัลมอนด์ (+10)", 10),
        ("ไม่ใส่นม", 0)
    };

    // Options ขนาด
    public List<(string Label, decimal PriceAdj)> SizeOptions { get; set; } = new()
    {
        ("S — เล็ก (-10 บาท)", -10),
        ("M — กลาง (ปกติ)", 0),
        ("L — ใหญ่ (+15 บาท)", 15)
    };

    // แสดง Options ความหวาน/นม/ขนาด เฉพาะสินค้าประเภทเครื่องดื่ม
    // เบเกอรี่และของทานเล่นไม่ต้องเลือก
    public bool ShowDrinkOptions => !IsNonDrinkCategory(Item?.Category);

    private static bool IsNonDrinkCategory(string? category)
    {
        if (string.IsNullOrEmpty(category)) return false;
        var nonDrink = new[] { "Bakery", "เบเกอรี่", "Snack", "ของทานเล่น", "Food", "อาหาร", "ขนม" };
        return nonDrink.Any(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
    }
}
