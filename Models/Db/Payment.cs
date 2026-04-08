using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public int? PaymentMethodId { get; set; }

    public int? PaymentStatusId { get; set; }

    public decimal? Amount { get; set; }

    public string? SlipUrl { get; set; }

    public int? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    // เหตุผลที่ Reject สลิป — บันทึกโดยพนักงาน ส่งแจ้งลูกค้าผ่าน SignalR
    public string? RejectReason { get; set; }
}