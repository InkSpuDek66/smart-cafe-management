using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Staffshift
{
    public int ShiftId { get; set; }

    public int? StaffId { get; set; }

    public DateTime? ShiftStart { get; set; }

    public DateTime? ShiftEnd { get; set; }

    public DateTime? CreatedAt { get; set; }
}