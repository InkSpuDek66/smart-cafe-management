using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Pointtransaction
{
    public int TransId { get; set; }

    public int? MemberId { get; set; }

    public int? TypeId { get; set; }

    public int? Amount { get; set; }

    public int? RefOrderId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }
}