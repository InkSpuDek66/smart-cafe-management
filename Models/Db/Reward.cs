using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Reward
{
    public int RewardId { get; set; }

    public string? RewardName { get; set; }

    public int? PointsRequired { get; set; }

    public int? StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public ulong? IsActive { get; set; }
}
