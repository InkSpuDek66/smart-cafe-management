// ViewModels/AdminMenuViewModel.cs
// ViewModels สำหรับการจัดการเมนูในหน้า Admin

using Project_CSI402_T2_Y3.Models.Db;

namespace Project_CSI402_T2_Y3.ViewModels;

// หน้ารายการเมนูทั้งหมด
public class AdminMenuListViewModel
{
    public List<Menuitem> Items     { get; set; } = new();
    public string? SuccessMessage   { get; set; }
    public string? ErrorMessage     { get; set; }
}

// ฟอร์มสร้าง / แก้ไขเมนู
public class MenuFormViewModel
{
    public int      MenuItemId          { get; set; }
    public string   MenuName            { get; set; } = "";
    public string?  MenuDescription     { get; set; }
    public decimal  Price               { get; set; }
    public string   Category            { get; set; } = "";
    public bool     IsAvailable         { get; set; } = true;
    public string?  ImageUrl            { get; set; }
    public bool     IsSeasonal          { get; set; }
    public string?  SeasonStartDate     { get; set; }
    public string?  SeasonEndDate       { get; set; }

    // กลุ่มท็อปปิ้ง: null=ค่าเริ่มต้น, "Soda"=ไม่มีนม+มีผลไม้, "NoMilk"=ไม่มีตัวเลือกนม
    public string?  ToppingGroup        { get; set; }

    // บอกว่าเป็นการแก้ไข (true) หรือสร้างใหม่ (false)
    public bool     IsEdit              { get; set; }

    public List<string> Categories { get; set; } = new()
    {
        "Coffee", "Non-Coffee", "Food", "Bakery"
    };

    // แปลงชื่อหมวดหมู่เป็นภาษาไทย (ค่าใน DB ยังคงเป็นภาษาอังกฤษ)
    public static readonly Dictionary<string, string> CategoryThaiNames = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Coffee",     "กาแฟ" },
        { "Non-Coffee", "เครื่องดื่มอื่น" },
        { "Food",       "อาหาร" },
        { "Bakery",     "เบเกอรี่" },
        { "Snack",      "ของทานเล่น" },
        { "Tea",        "ชา" },
        { "Seasonal",   "เมนูตามฤดูกาล" },
        { "Drink",      "เครื่องดื่ม" }
    };

    // แปลงชื่อหมวดหมู่เป็นภาษาไทย
    public static string GetThaiName(string? cat) =>
        cat != null && CategoryThaiNames.TryGetValue(cat, out var t) ? t : (cat ?? "");
}
