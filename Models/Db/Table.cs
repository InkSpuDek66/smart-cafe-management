using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Table
{
    public int TableId { get; set; }

    public string? TableNumber { get; set; }

    public string? QrCodeUrl { get; set; }

    public ulong? IsActive { get; set; }
}
