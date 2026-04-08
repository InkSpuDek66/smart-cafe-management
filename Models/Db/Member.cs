using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Member
{
    public int MemberId { get; set; }

    public string? Phone { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public int? Points { get; set; }

    public int? StampBalance { get; set; }
}