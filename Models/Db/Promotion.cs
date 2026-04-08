using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string? PromotionName { get; set; }

    public string? ConditionType { get; set; }

    public string? ConditionValue { get; set; }

    public string? RewardType { get; set; }

    public string? RewardValue { get; set; }

    public ulong? IsActive { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
