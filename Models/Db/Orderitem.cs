using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Orderitem
{
    public int OrderItemId { get; set; }

    public int? OrderId { get; set; }

    public int? MenuItemId { get; set; }

    public int? OrderItemStatusId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    // 1 = เมนูฟรีจากการแลก Stamp Card, 0/null = รายการปกติ
    public ulong? IsStampReward { get; set; }
}
