// ViewModels/MenuDetailViewModel.cs
// ViewModel หน้ารายละเอียดเมนู — แสดงรูปสินค้าและตัวเลือก

using Project_CSI402_T2_Y3.Models.Db;

namespace Project_CSI402_T2_Y3.ViewModels;

public class MenuDetailViewModel
{
    public Menuitem Item            { get; set; } = null!;
    public string? TableNumber      { get; set; }

    // ===== Options ความหวาน (ใช้ร่วมกันทุก ToppingGroup) =====
    public List<string> SweetnessOptions { get; set; } = new()
    {
        "หวานปกติ", "หวานน้อย", "หวานน้อยมาก", "ไม่หวาน"
    };

    // ===== Options ประเภทนม (สำหรับ ToppingGroup ทั่วไป / Coffee) =====
    public static readonly List<(string Label, decimal PriceAdj)> DefaultMilkOptions = new()
    {
        ("นมสด", 0),
        ("นมข้นหวาน", 0),
        ("นมโอ๊ต (+10)", 10),
        ("นมอัลมอนด์ (+10)", 10),
        ("ไม่ใส่นม", 0)
    };

    // ===== Topping ผลไม้ (สำหรับ ToppingGroup = "Soda") =====
    // เมนูโซดาไม่มีตัวเลือกนม แต่มีท็อปปิ้งผลไม้แทน
    public static readonly List<(string Label, decimal PriceAdj)> SodaToppingOptions = new()
    {
        ("ผลไม้รวม (+10)", 10),
        ("สตรอเบอร์รี่ (+10)", 10),
        ("บลูเบอร์รี่ (+10)", 10),
        ("มะนาว (+5)", 5),
        ("ไม่เพิ่มท็อปปิ้ง", 0)
    };

    // ===== Options ขนาด (ใช้ร่วมกัน) =====
    public List<(string Label, decimal PriceAdj)> SizeOptions { get; set; } = new()
    {
        ("S — เล็ก (-10 บาท)", -10),
        ("M — กลาง (ปกติ)", 0),
        ("L — ใหญ่ (+15 บาท)", 15)
    };

    // Options นม/ท็อปปิ้งที่ใช้จริง — คำนวณตาม ToppingGroup ของเมนูนี้
    public List<(string Label, decimal PriceAdj)> MilkOptions =>
        (Item?.ToppingGroup ?? "").ToLowerInvariant() switch
        {
            "soda"   => new(), // โซดา: ไม่มีตัวเลือกนม (ใช้ FruitToppingOptions แทน)
            "nomilk" => new(), // เมนูที่ไม่มีนมเลย
            _        => DefaultMilkOptions
        };

    // Options ท็อปปิ้งผลไม้ (เฉพาะ Soda)
    public List<(string Label, decimal PriceAdj)> FruitToppingOptions =>
        (Item?.ToppingGroup ?? "").ToLowerInvariant() == "soda"
            ? SodaToppingOptions
            : new();

    public bool HasFruitToppings => FruitToppingOptions.Any();
    public bool HasMilkOptions   => MilkOptions.Any();

    // แสดง Options เฉพาะเครื่องดื่ม — เบเกอรี่และของทานเล่นไม่มีตัวเลือก
    public bool ShowDrinkOptions => !IsNonDrinkCategory(Item?.Category);

    private static bool IsNonDrinkCategory(string? category)
    {
        if (string.IsNullOrEmpty(category)) return false;
        var nonDrink = new[] { "Bakery", "เบเกอรี่", "Snack", "ของทานเล่น", "Food", "อาหาร", "ขนม" };
        return nonDrink.Any(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
    }
}
