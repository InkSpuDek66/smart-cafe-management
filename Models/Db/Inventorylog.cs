using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Inventorylog
{
    public int LogId { get; set; }

    public int? IngredientId { get; set; }

    public int? ReasonTypeId { get; set; }

    public int? RefOrderId { get; set; }

    public int? CreatedBy { get; set; }

    public float? QuantityChange { get; set; }

    public DateTime? CreatedAt { get; set; }

    // หมายเหตุ / สาเหตุการสูญเสีย — ต้องรัน SQL ก่อน:
    // ALTER TABLE inventorylogs ADD COLUMN Notes NVARCHAR(500) NULL;
    public string? Notes { get; set; }
}