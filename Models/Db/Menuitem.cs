using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Menuitem
{
    public int MenuItemId { get; set; }

    public string? MenuName { get; set; }

    public string? MenuDescription { get; set; }

    public decimal? Price { get; set; }

    public string? Category { get; set; }

    public ulong? IsAvailable { get; set; }

    public string? ImageUrl { get; set; }

    public ulong? IsSeasonal { get; set; }

    public DateTime? SeasonStartDate { get; set; }

    public DateTime? SeasonEndDate { get; set; }

    // กลุ่มท็อปปิ้ง: null=ค่าเริ่มต้น(ทุกตัวเลือก), "Soda"=ไม่มีนม+มีผลไม้, "Coffee"=ค่าเริ่มต้น, "NoMilk"=ไม่มีตัวเลือกนม
    public string? ToppingGroup { get; set; }
}
