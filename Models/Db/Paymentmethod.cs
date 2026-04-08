using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Paymentmethod
{
    public int PaymentMethodId { get; set; }

    public string? MethodName { get; set; }
}
