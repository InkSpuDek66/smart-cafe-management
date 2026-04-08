using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Ingredient
{
    public int IngredientId { get; set; }

    public string? IngredientName { get; set; }

    public float? StockQuantity { get; set; }

    public float? ReservedQty { get; set; }

    public string? Unit { get; set; }

    public float? ReorderLevel { get; set; }

    public decimal? CostPerUnit { get; set; }
}
