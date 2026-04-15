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
        // แสดงออเดอร์ตั้งแต่ Paid (2) เป็นต้นไป: Paid=2, Preparing=3, Ready=4
        // เพื่อให้ลูกค้าทราบว่าออเดอร์ถูกรับแล้วและอยู่ในระบบแล้ว
        var activeOrders = _db.Orders
            .Where(o => o.OrderStatusId == 2 || o.OrderStatusId == 3 || o.OrderStatusId == 4)
            .OrderBy(o => o.QueueNumber)
            .Select(o => new { o.OrderId, o.QueueNumber, o.TableId, o.OrderStatusId })
            .ToList();

        // ออเดอร์เสร็จแล้ว 5 ล่าสุดเฉพาะวันนี้ (เพิ่งเสิร์ฟ — StatusId=5)
        var today = DateTime.Today;
        var recentCompleted = _db.Orders
            .Where(o => o.OrderStatusId == 5 && o.CreatedAt >= today)
            .OrderByDescending(o => o.OrderId)
            .Take(5)
            .Select(o => new { o.OrderId, o.QueueNumber })
            .ToList();

        var tableDict = _db.Tables.ToDictionary(t => t.TableId, t => t.TableNumber);

        // StatusId → label ภาษาไทยสำหรับ Public Screen
        ViewBag.ReadyOrders = activeOrders.Select(o => new
        {
            o.OrderId,
            o.QueueNumber,
            TableNumber  = o.TableId.HasValue ? tableDict.GetValueOrDefault(o.TableId.Value, "") : "",
            StatusId     = o.OrderStatusId,
            StatusLabel  = o.OrderStatusId switch
            {
                2 => "รับออเดอร์แล้ว",
                3 => "กำลังเตรียม",
                4 => "พร้อมรับ",
                _ => ""
            }
        }).ToList();
        ViewBag.RecentCompleted = recentCompleted;

        return View();
    }
}
