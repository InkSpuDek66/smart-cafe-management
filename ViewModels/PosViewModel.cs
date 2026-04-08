namespace Project_CSI402_T2_Y3.ViewModels;

// ViewModel สำหรับแสดงแถวออเดอร์ในหน้า Queue และ KDS
public class PosOrderRow
{
    public int    OrderId       { get; set; }
    public string? QueueNumber  { get; set; }
    public string? TableNumber  { get; set; }
    public string? CustomerName { get; set; }   // GuestName หรือ Member FullName
    public decimal? TotalAmount { get; set; }
    public decimal? NetAmount   { get; set; }
    public int    StatusId      { get; set; }
    public string? StatusName   { get; set; }
    public DateTime? CreatedAt  { get; set; }
    // ข้อมูลสมาชิก — ใช้สำหรับแสดง Redemption UI ใน Queue
    public int?   MemberId      { get; set; }
    public int?   MemberPoints  { get; set; }
    public int?   MemberStamps  { get; set; }
    // ข้อมูล Payment ของออเดอร์นี้ — ใช้แสดงสถานะสลิปบน Queue
    public int?   PaymentId       { get; set; }
    public int?   PaymentStatusId { get; set; }  // 1=Pending, 2=Approved, 3=Rejected
    public string? SlipUrl        { get; set; }
    // ข้อมูลโปรโมชัน Group Check-in
    public bool   GroupCheckinApplied { get; set; }
    public List<PosOrderItemRow> Items { get; set; } = new();
}

// ViewModel สำหรับแต่ละ OrderItem ในหน้า Queue
public class PosOrderItemRow
{
    public int    OrderItemId   { get; set; }
    public int    MenuItemId    { get; set; }
    public string? MenuName     { get; set; }
    public int?   Quantity      { get; set; }
    public int?   StatusId      { get; set; }
    public string? StatusName   { get; set; }
    public List<string> Options { get; set; } = new();   // "OptionName: OptionValue"
    public List<RecipeRow> Recipes { get; set; } = new(); // สูตรวัตถุดิบ (ใช้ใน KDS)
}

// ViewModel สำหรับสูตรวัตถุดิบ (แสดงใน KDS)
public class RecipeRow
{
    public string? IngredientName   { get; set; }
    public float?  QuantityRequired { get; set; }
    public string? Unit             { get; set; }
}

// ViewModel หลักสำหรับหน้า Queue (Split View)
public class PosQueueViewModel
{
    public List<PosOrderRow> UnpaidOrders { get; set; } = new(); // StatusId=1 รอชำระ
    public List<PosOrderRow> ActiveOrders { get; set; } = new(); // StatusId=2,3,4
}

// ViewModel สำหรับหน้าออก Reward ให้สมาชิกที่ POS
public class PosRewardRow
{
    public int    RewardId       { get; set; }
    public string? RewardName    { get; set; }
    public int?   PointsRequired { get; set; }
    public int?   StockQuantity  { get; set; }
    public string? ImageUrl      { get; set; }
}

// ViewModel หลักสำหรับหน้า KDS
public class PosKdsViewModel
{
    public List<PosOrderRow> Orders { get; set; } = new(); // StatusId=2,3
}

// ViewModel สำหรับหน้าบันทึก Wastage
public class PosWastageViewModel
{
    public int    IngredientId   { get; set; }
    public string? IngredientName { get; set; }
    public float?  StockQuantity  { get; set; }
    public string? Unit           { get; set; }
}
