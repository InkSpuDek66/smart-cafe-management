using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Orderitemoption
{
    public int OrderItemOptionId { get; set; }

    public int? OrderItemId { get; set; }

    public string? OptionName { get; set; }

    public string? OptionValue { get; set; }

    public decimal? PriceAdjustment { get; set; }
}
