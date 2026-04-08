// Controllers/HomeController.cs
// จัดการหน้าหลักและ Dashboard ภาพรวมระบบ

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project_CSI402_T2_Y3.Models;
using Project_CSI402_T2_Y3.Models.Db;
using Project_CSI402_T2_Y3.ViewModels;

namespace Project_CSI402_T2_Y3.Controllers;

public class HomeController : Controller
{
    private readonly Csi402dbContext _db;

    public HomeController(Csi402dbContext db)
    {
        _db = db;
    }

    // หน้า Landing — แสดงภาพรวมระบบและสถิติเบื้องต้น
    public IActionResult Index()
    {
        ViewBag.MemberCount = _db.Members.Count();
        ViewBag.MenuItemCount = _db.Menuitems.Count();
        ViewBag.StaffCount = _db.Staff.Count(s => s.IsActive == (ulong)1);
        return View();
    }

    // Dashboard สำหรับ Manager, Finance และ Owner
    public IActionResult Dashboard()
    {
        // ต้อง Login ก่อนเข้าใช้งาน Dashboard
        if (!HttpContext.Session.GetInt32("StaffRoleId").HasValue)
            return RedirectToAction("Login", "Account");

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var viewModel = new DashboardViewModel
        {
            TotalMembers = _db.Members.Count(),
            TotalMenuItems = _db.Menuitems.Count(),
            ActiveMenuItems = _db.Menuitems.Count(m => m.IsAvailable == (ulong)1),
            TotalStaff = _db.Staff.Count(s => s.IsActive == (ulong)1),

            // นับออเดอร์ที่สร้างวันนี้ทุกสถานะ
            TodayOrderCount = _db.Orders.Count(o => o.CreatedAt >= today && o.CreatedAt < tomorrow),

            // รวมยอดออเดอร์ที่ชำระแล้ว (Paid=2, Preparing=3, Ready=4, Completed=5) ยกเว้น Cancelled (6)
            TodayRevenue = _db.Orders
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow
                         && o.OrderStatusId >= 2 && o.OrderStatusId != 6)
                .Sum(o => (decimal?)o.NetAmount) ?? 0,

            // วัตถุดิบที่ StockQuantity ต่ำกว่า ReorderLevel
            LowStockAlerts = _db.Ingredients
                .Where(i => i.StockQuantity.HasValue && i.ReorderLevel.HasValue
                         && i.StockQuantity < i.ReorderLevel)
                .Select(i => new LowStockAlertItem
                {
                    IngredientName = i.IngredientName ?? "",
                    StockQuantity = i.StockQuantity ?? 0,
                    ReorderLevel = i.ReorderLevel ?? 0,
                    Unit = i.Unit ?? ""
                })
                .ToList()
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
