// Controllers/PublicController.cs
// Public Screen สำหรับจอ TV แสดงเลขคิวที่พร้อมเสิร์ฟ
// ไม่ต้อง Login — เปิดหน้าจอทิ้งไว้ที่ร้าน

using Microsoft.AspNetCore.Mvc;
using Project_CSI402_T2_Y3.Models.Db;

namespace Project_CSI402_T2_Y3.Controllers;

public class PublicController : Controller
{
    private readonly Csi402dbContext _db;

    public PublicController(Csi402dbContext db)
    {
        _db = db;
    }

    // ============================================================
    // GET /Public/Queue
    // แสดงออเดอร์ที่พร้อมเสิร์ฟ (StatusId=4) และออเดอร์ล่าสุดที่เสร็จ (StatusId=5)
    // Auto-refresh ทุก 10 วินาที และรับ SignalR event
    // ============================================================
    public IActionResult Queue()
    {
        // ออเดอร์พร้อมเสิร์ฟ (Ready = 4)
        var readyOrders = _db.Orders
            .Where(o => o.OrderStatusId == 4)
            .OrderBy(o => o.QueueNumber)
            .Select(o => new { o.OrderId, o.QueueNumber, o.TableId })
            .ToList();

        // ออเดอร์เสร็จแล้ว 5 ล่าสุด (เพิ่งเสิร์ฟ — StatusId=5)
        var recentCompleted = _db.Orders
            .Where(o => o.OrderStatusId == 5)
            .OrderByDescending(o => o.OrderId)
            .Take(5)
            .Select(o => new { o.OrderId, o.QueueNumber })
            .ToList();

        var tableDict = _db.Tables.ToDictionary(t => t.TableId, t => t.TableNumber);

        ViewBag.ReadyOrders      = readyOrders.Select(o => new
        {
            o.OrderId,
            o.QueueNumber,
            TableNumber = o.TableId.HasValue ? tableDict.GetValueOrDefault(o.TableId.Value, "") : ""
        }).ToList();
        ViewBag.RecentCompleted  = recentCompleted;

        return View();
    }
}
