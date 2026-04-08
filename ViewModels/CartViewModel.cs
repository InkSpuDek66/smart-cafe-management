// ViewModels/CartViewModel.cs
// ViewModel สำหรับตะกร้าสินค้าของลูกค้า (เก็บใน Session)

namespace Project_CSI402_T2_Y3.ViewModels;

// รายการสินค้า 1 รายการในตะกร้า — Serialized เป็น JSON เก็บใน Session
public class CartItem
{
    public int MenuItemId       { get; set; }
    public string MenuName      { get; set; } = "";
    public decimal UnitPrice    { get; set; }
    public int Quantity         { get; set; }
    public string? ImageUrl     { get; set; }

    // Options ที่ลูกค้าเลือก
    public string? Sweetness    { get; set; }
    public string? MilkType     { get; set; }
    public string? Size         { get; set; }

    // ราคาปรับเพิ่ม/ลดจาก Options (เช่น +10 สำหรับนมโอ๊ต)
    public decimal OptionPriceAdjustment { get; set; }

    // ราคารวมของรายการนี้ = (ราคา + ปรับ) x จำนวน
    public decimal ItemTotal => (UnitPrice + OptionPriceAdjustment) * Quantity;
}

// ViewModel หน้าตะกร้าสินค้า
public class CartViewModel
{
    public List<CartItem> Items   { get; set; } = new();
    public string? TableNumber    { get; set; }
    public decimal TotalAmount    => Items.Sum(i => i.ItemTotal);
    public int TotalItemCount     => Items.Sum(i => i.Quantity);
}
