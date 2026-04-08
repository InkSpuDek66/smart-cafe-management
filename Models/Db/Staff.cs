using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Staff
{
    public int StaffId { get; set; }

    public int? StaffRoleId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public ulong? IsActive { get; set; }
}