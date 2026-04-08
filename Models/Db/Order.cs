using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Order
{
    public int OrderId { get; set; }

    public int? MemberId { get; set; }

    public int? TableId { get; set; }

    public int? OrderStatusId { get; set; }

    public string? GuestName { get; set; }

    public string? QueueNumber { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public DateTime? ReservedUntil { get; set; }

    public DateTime? CreatedAt { get; set; }
}