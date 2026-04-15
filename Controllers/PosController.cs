using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Project_CSI402_T2_Y3.Hubs;
using Project_CSI402_T2_Y3.Models.Db;
using Project_CSI402_T2_Y3.ViewModels;

namespace Project_CSI402_T2_Y3.Controllers;

public class PosController : Controller
{
    private readonly Csi402dbContext _db;
    private readonly IHubContext<CafeHub> _hub;

    public PosController(Csi402dbContext db, IHubContext<CafeHub> hub)
    {
        _db  = db;
        _hub = hub;
    }

    private bool IsLoggedIn() => HttpContext.Session.GetInt32("StaffId") != null;
    private int? StaffId()    => HttpContext.Session.GetInt32("StaffId");
    private int? RoleId()     => HttpContext.Session.GetInt32("StaffRoleId");

    // ตรวจสอบว่า Role ปัจจุบันมีสิทธิ์หรือไม่
    private bool HasRole(params int[] allowedRoles)
    {
        var roleId = RoleId();
        return roleId.HasValue && allowedRoles.Contains(roleId.Value);
    }

    // คืนค่า Forbidden สำหรับ POS
    private IActionResult ForbiddenRedirect()
    {
        TempData["Error"] = "คุณไม่มีสิทธิ์เข้าถึงหน้านี้";
        return RedirectToAction("Queue");
    }

    // =====================================================================
    // Queue — แสดงออเดอร์แบบ Split View (รอชำระ ซ้าย / Active ขวา)
    // =====================================================================
    public IActionResult Queue()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        // Barista (1), Cashier (2), Manager (3), Owner (5) เข้าถึงได้
        if (!HasRole(1, 2, 3, 5)) return ForbiddenRedirect();

        // ตรวจสอบ Reservation หมดอายุ → Cancel เฉพาะออเดอร์ที่ไม่มีสลิปรอ Verify
        // ออเดอร์ที่ลูกค้าอัปโหลดสลิปแล้ว (PaymentStatusId=1) ต้องรอพนักงานตรวจก่อน ห้าม Cancel
        var ordersWithPendingSlip = _db.Payments
            .Where(p => p.PaymentStatusId == 1)
            .Select(p => p.OrderId)
            .Distinct()
            .ToList();

        var expiredOrders = _db.Orders
            .Where(o => o.OrderStatusId == 1
                     && o.ReservedUntil < DateTime.Now
                     && !ordersWithPendingSlip.Contains(o.OrderId))
            .ToList();

        if (expiredOrders.Any())
        {
            var expiredIds    = expiredOrders.Select(e => e.OrderId).ToList();
            var expiredItems  = _db.Orderitems.Where(i => expiredIds.Contains(i.OrderId ?? 0)).ToList();
            var allRecipes    = _db.Recipes.ToList();
            var allIngredients = _db.Ingredients.ToList();

            foreach (var exp in expiredOrders)
            {
                exp.OrderStatusId = 6; // Cancelled

                // คืน ReservedQty ของวัตถุดิบที่จองไว้
                var items = expiredItems.Where(i => i.OrderId == exp.OrderId).ToList();
                foreach (var ei in items)
                {
                    var recipes = allRecipes.Where(r => r.MenuItemId == ei.MenuItemId).ToList();
                    foreach (var recipe in recipes)
                    {
                        var ing = allIngredients.FirstOrDefault(i => i.IngredientId == recipe.IngredientId);
                        if (ing != null)
                        {
                            float release    = (recipe.QuantityRequired ?? 0) * (ei.Quantity ?? 1);
                            ing.ReservedQty  = Math.Max(0, (ing.ReservedQty ?? 0) - release);
                        }
                    }
                }
            }
            _db.SaveChanges();
        }

        // โหลด lookup tables
        var menuDict        = _db.Menuitems.ToDictionary(m => m.MenuItemId, m => new { m.MenuName, m.Category });
        var tableDict       = _db.Tables.ToDictionary(t => t.TableId, t => t.TableNumber);
        var statusDict      = _db.Orderstatuses.ToDictionary(s => s.OrderStatusId, s => s.StatusName);
        var itemStatusDict  = _db.Orderitemstatuses.ToDictionary(s => s.OrderItemStatusId, s => s.StatusName);

        // โหลดข้อมูลสมาชิก (ชื่อ + แต้ม + Stamp)
        var memberDict = _db.Members.ToDictionary(
            m => m.MemberId,
            m => new { Name = (m.FirstName + " " + m.LastName).Trim(), Points = m.Points ?? 0, Stamps = m.StampBalance ?? 0 }
        );

        // โหลด OrderItems + Options พร้อมกันทีเดียว
        var allItems   = _db.Orderitems.ToList();
        var allOptions = _db.Orderitemoptions.ToList();

        // โหลด Payment ของทุก Order ในหน้าจอนี้ เพื่อแสดงสถานะสลิป
        var allPayments = _db.Payments.ToList();

        // ฟังก์ชันช่วยสร้าง PosOrderRow จาก Order
        PosOrderRow BuildRow(Order o)
        {
            var items = allItems.Where(i => i.OrderId == o.OrderId).Select(i =>
            {
                var opts = allOptions
                    .Where(op => op.OrderItemId == i.OrderItemId)
                    .Select(op => $"{op.OptionName}: {op.OptionValue}")
                    .ToList();

                var menuInfo = menuDict.GetValueOrDefault(i.MenuItemId ?? 0);
                return new PosOrderItemRow
                {
                    OrderItemId = i.OrderItemId,
                    MenuItemId  = i.MenuItemId ?? 0,
                    MenuName    = menuInfo?.MenuName,
                    Category    = menuInfo?.Category,
                    UnitPrice   = i.UnitPrice,
                    Quantity    = i.Quantity,
                    StatusId    = i.OrderItemStatusId,
                    StatusName  = itemStatusDict.GetValueOrDefault(i.OrderItemStatusId ?? 0),
                    Options     = opts
                };
            }).ToList();

            string? customerName;
            int?    memberPoints = null;
            int?    memberStamps = null;

            if (o.MemberId.HasValue && memberDict.TryGetValue(o.MemberId.Value, out var md))
            {
                customerName  = md.Name;
                memberPoints  = md.Points;
                memberStamps  = md.Stamps;
            }
            else
            {
                customerName = o.GuestName;
            }

            // ดึงข้อมูล Payment ล่าสุดของออเดอร์นี้
            var payment = allPayments
                .Where(p => p.OrderId == o.OrderId)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefault();

            return new PosOrderRow
            {
                OrderId            = o.OrderId,
                QueueNumber        = o.QueueNumber,
                TableNumber        = o.TableId.HasValue ? tableDict.GetValueOrDefault(o.TableId.Value) : null,
                CustomerName       = customerName,
                TotalAmount        = o.TotalAmount,
                NetAmount          = o.NetAmount,
                StatusId           = o.OrderStatusId ?? 0,
                StatusName         = statusDict.GetValueOrDefault(o.OrderStatusId ?? 0),
                CreatedAt          = o.CreatedAt,
                MemberId           = o.MemberId,
                MemberPoints       = memberPoints,
                MemberStamps       = memberStamps,
                PaymentId          = payment?.PaymentId,
                PaymentStatusId    = payment?.PaymentStatusId,
                SlipUrl            = payment?.SlipUrl,
                Items              = items
            };
        }

        // ออเดอร์รอชำระเงิน (StatusId=1)
        var unpaid = _db.Orders
            .Where(o => o.OrderStatusId == 1)
            .OrderBy(o => o.CreatedAt)
            .ToList()
            .Select(BuildRow)
            .ToList();

        // ออเดอร์ที่ชำระแล้ว / กำลังทำ / พร้อมเสิร์ฟ (StatusId=2,3,4)
        var active = _db.Orders
            .Where(o => o.OrderStatusId == 2 || o.OrderStatusId == 3 || o.OrderStatusId == 4)
            .OrderBy(o => o.QueueNumber)
            .ThenBy(o => o.CreatedAt)
            .ToList()
            .Select(BuildRow)
            .ToList();

        var vm = new PosQueueViewModel
        {
            UnpaidOrders = unpaid,
            ActiveOrders = active
        };

        return View(vm);
    }

    // =====================================================================
    // KDS — Kitchen Display System สำหรับ Barista
    // =====================================================================
    public IActionResult Kds()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

        // KDS ใช้ได้เฉพาะ Barista (1), Store Manager (3), Owner (5)
        var roleId = RoleId();
        if (roleId != 1 && roleId != 3 && roleId != 5)
            return RedirectToAction("Queue");

        var menuDict       = _db.Menuitems.ToDictionary(m => m.MenuItemId, m => m.MenuName);
        var tableDict      = _db.Tables.ToDictionary(t => t.TableId, t => t.TableNumber);
        var itemStatusDict = _db.Orderitemstatuses.ToDictionary(s => s.OrderItemStatusId, s => s.StatusName);
        var allOptions     = _db.Orderitemoptions.ToList();

        // โหลดสูตรวัตถุดิบทั้งหมด
        var ingredientDict = _db.Ingredients.ToDictionary(i => i.IngredientId, i => i);
        var allRecipes     = _db.Recipes.ToList();

        // KDS แสดงเฉพาะออเดอร์วันนี้ที่ชำระแล้ว/กำลังทำ — ป้องกัน timer แสดงเวลาผิด
        var today    = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var orders = _db.Orders
            .Where(o => (o.OrderStatusId == 2 || o.OrderStatusId == 3)
                     && o.CreatedAt >= today && o.CreatedAt < tomorrow)
            .OrderBy(o => o.QueueNumber)
            .ThenBy(o => o.CreatedAt)
            .ToList();

        var orderIds = orders.Select(o => o.OrderId).ToList();
        var allItems = _db.Orderitems
            .Where(i => orderIds.Contains(i.OrderId ?? 0))
            .ToList();

        var rows = orders.Select(o =>
        {
            var items = allItems.Where(i => i.OrderId == o.OrderId).Select(i =>
            {
                var opts = allOptions
                    .Where(op => op.OrderItemId == i.OrderItemId)
                    .Select(op => $"{op.OptionName}: {op.OptionValue}")
                    .ToList();

                // สูตรวัตถุดิบของเมนูนี้
                var recipes = allRecipes
                    .Where(r => r.MenuItemId == i.MenuItemId)
                    .Select(r =>
                    {
                        var ing = ingredientDict.GetValueOrDefault(r.IngredientId ?? 0);
                        return new RecipeRow
                        {
                            IngredientName   = ing?.IngredientName,
                            QuantityRequired = r.QuantityRequired * (i.Quantity ?? 1),
                            Unit             = r.Unit ?? ing?.Unit
                        };
                    })
                    .ToList();

                return new PosOrderItemRow
                {
                    OrderItemId = i.OrderItemId,
                    MenuItemId  = i.MenuItemId ?? 0,
                    MenuName    = menuDict.GetValueOrDefault(i.MenuItemId ?? 0),
                    Quantity    = i.Quantity,
                    StatusId    = i.OrderItemStatusId,
                    StatusName  = itemStatusDict.GetValueOrDefault(i.OrderItemStatusId ?? 0),
                    Options     = opts,
                    Recipes     = recipes
                };
            }).ToList();

            return new PosOrderRow
            {
                OrderId     = o.OrderId,
                QueueNumber = o.QueueNumber,
                TableNumber = o.TableId.HasValue ? tableDict.GetValueOrDefault(o.TableId.Value) : null,
                CreatedAt   = o.CreatedAt,
                Items       = items
            };
        }).ToList();

        var vm = new PosKdsViewModel { Orders = rows };
        return View(vm);
    }

    // =====================================================================
    // MarkItemDone — Barista กด Done ที่แต่ละ OrderItem (AJAX POST)
    // เมื่อทุก Item ของ Order Done → อัปเดต Order เป็น Ready (StatusId=4)
    // =====================================================================
    [HttpPost]
    public async Task<IActionResult> MarkItemDone(int itemId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false, message = "ไม่ได้ Login" });
        // เฉพาะ Barista (1), Manager (3), Owner (5) กด Done ได้
        if (!HasRole(1, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var item = _db.Orderitems.Find(itemId);
        if (item == null) return Json(new { ok = false, message = "ไม่พบ Item" });

        // อัปเดตสถานะ Item เป็น Done (3)
        item.OrderItemStatusId = 3;
        _db.SaveChanges();

        // ตรวจสอบว่าทุก Item ของออเดอร์นี้ Done หมดแล้วหรือยัง
        // ต้องมี Item อย่างน้อย 1 รายการ ก่อนจะถือว่า allDone
        var allItems = _db.Orderitems.Where(i => i.OrderId == item.OrderId).ToList();
        bool allDone = allItems.Any() && allItems.All(i => i.OrderItemStatusId == 3);

        if (allDone)
        {
            // อัปเดตออเดอร์เป็น Ready (4) เฉพาะเมื่อยังไม่ถึงสถานะ Completed/Cancelled
            var order = _db.Orders.Find(item.OrderId);
            if (order != null && order.OrderStatusId < 4)
            {
                order.OrderStatusId = 4;
                _db.SaveChanges();

                // แจ้ง Customer + Public Screen ว่าออเดอร์พร้อมเสิร์ฟ
                await _hub.Clients.Group($"order-{order.OrderId}").SendAsync("NotifyOrderReady", new
                {
                    orderId     = order.OrderId,
                    queueNumber = order.QueueNumber
                });
                await _hub.Clients.Group("public").SendAsync("NotifyOrderReady", new
                {
                    orderId     = order.OrderId,
                    queueNumber = order.QueueNumber
                });
            }
        }

        return Json(new { ok = true, allDone });
    }

    // =====================================================================
    // CallCustomer — Barista กดปุ่ม "เรียกลูกค้า" บน KDS
    // ส่ง SignalR NotifyOrderReady ซ้ำเพื่อแจ้งเตือนลูกค้าอีกครั้ง
    // =====================================================================
    [HttpPost]
    public async Task<IActionResult> CallCustomer(int orderId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false, message = "ไม่ได้ Login" });
        if (!HasRole(1, 2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var order = _db.Orders.Find(orderId);
        if (order == null) return Json(new { ok = false, message = "ไม่พบออเดอร์" });

        // ส่งสัญญาณแจ้งลูกค้าและหน้า Public Screen ซ้ำ
        await _hub.Clients.Group($"order-{order.OrderId}").SendAsync("NotifyOrderReady", new
        {
            orderId     = order.OrderId,
            queueNumber = order.QueueNumber
        });
        await _hub.Clients.Group("public").SendAsync("NotifyOrderReady", new
        {
            orderId     = order.OrderId,
            queueNumber = order.QueueNumber
        });

        return Json(new { ok = true });
    }

    // =====================================================================
    // MarkOrderPreparing — Cashier/Barista กด "เริ่มทำ" บน Queue ฝั่งขวา
    // เปลี่ยน OrderStatus จาก Paid (2) → Preparing (3)
    // =====================================================================
    [HttpPost]
    public IActionResult MarkOrderPreparing(int orderId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false });
        if (!HasRole(1, 2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var order = _db.Orders.Find(orderId);
        if (order == null || order.OrderStatusId != 2)
            return Json(new { ok = false });

        order.OrderStatusId = 3;

        // อัปเดตทุก Item เป็น Preparing (2)
        var items = _db.Orderitems.Where(i => i.OrderId == orderId).ToList();
        foreach (var item in items)
            item.OrderItemStatusId = 2;

        _db.SaveChanges();
        return Json(new { ok = true });
    }

    // =====================================================================
    // MarkOrderCompleted — Cashier กด "ส่งลูกค้าแล้ว" บน Queue
    // เปลี่ยน OrderStatus จาก Ready (4) → Completed (5)
    // =====================================================================
    [HttpPost]
    public async Task<IActionResult> MarkOrderCompleted(int orderId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false });
        // เฉพาะ Cashier (2), Manager (3), Owner (5) ส่งลูกค้าได้
        if (!HasRole(2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var order = _db.Orders.Find(orderId);
        if (order == null || order.OrderStatusId != 4)
            return Json(new { ok = false });

        order.OrderStatusId = 5; // Completed
        _db.SaveChanges();

        // แจ้ง Public Screen ให้ลบ card ออกทันที
        await _hub.Clients.Group("public").SendAsync("NotifyOrderCompleted", new
        {
            orderId     = order.OrderId,
            queueNumber = order.QueueNumber
        });

        return Json(new { ok = true });
    }

    // =====================================================================
    // Wastage GET — หน้าบันทึก Wastage เลือกวัตถุดิบ + ปริมาณ
    // =====================================================================
    public IActionResult Wastage()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        // Barista (1), Cashier (2), Manager (3), Owner (5) บันทึก Wastage ได้
        if (!HasRole(1, 2, 3, 5)) return ForbiddenRedirect();

        var ingredients = _db.Ingredients
            .OrderBy(i => i.IngredientName)
            .Select(i => new PosWastageViewModel
            {
                IngredientId   = i.IngredientId,
                IngredientName = i.IngredientName,
                StockQuantity  = i.StockQuantity,
                Unit           = i.Unit
            })
            .ToList();

        ViewBag.Ingredients = ingredients;

        // ประวัติ Wastage 20 รายการล่าสุด
        var ingredientDict = _db.Ingredients.ToDictionary(i => i.IngredientId, i => i.IngredientName);
        var staffDict      = _db.Staff.ToDictionary(s => s.StaffId, s => (s.FirstName + " " + s.LastName).Trim());

        var ingredientFullDict = _db.Ingredients.ToDictionary(i => i.IngredientId, i => i);

        ViewBag.RecentLogs = _db.Inventorylogs
            .Where(l => l.ReasonTypeId == 2)
            .OrderByDescending(l => l.CreatedAt)
            .Take(20)
            .ToList()
            .Select(l => new
            {
                l.LogId,
                IngredientName = ingredientDict.GetValueOrDefault(l.IngredientId ?? 0, "?"),
                Unit           = ingredientFullDict.TryGetValue(l.IngredientId ?? 0, out var ing) ? ing.Unit : "—",
                l.QuantityChange,
                l.Notes,
                CreatedBy      = l.CreatedBy.HasValue ? staffDict.GetValueOrDefault(l.CreatedBy.Value, "?") : "?",
                l.CreatedAt
            })
            .ToList();

        return View();
    }

    // =====================================================================
    // RedeemPoints — แลกแต้มเพื่อลดราคาออเดอร์ (AJAX POST)
    // 1 แต้ม = ลดราคา 1 บาท
    // =====================================================================
    [HttpPost]
    public IActionResult RedeemPoints(int orderId, int points)
    {
        if (!IsLoggedIn()) return Json(new { ok = false, message = "ไม่ได้ Login" });
        // เฉพาะ Cashier (2), Manager (3), Owner (5) แลกแต้มให้ลูกค้าได้
        if (!HasRole(2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var order = _db.Orders.Find(orderId);
        if (order == null || order.OrderStatusId != 1)
            return Json(new { ok = false, message = "ไม่พบออเดอร์หรือสถานะไม่ถูกต้อง" });

        if (!order.MemberId.HasValue)
            return Json(new { ok = false, message = "ออเดอร์นี้ไม่มีข้อมูลสมาชิก" });

        var member = _db.Members.Find(order.MemberId.Value);
        if (member == null)
            return Json(new { ok = false, message = "ไม่พบสมาชิก" });

        if (points <= 0 || points > (member.Points ?? 0))
            return Json(new { ok = false, message = $"แต้มไม่พอ (มี {member.Points ?? 0} แต้ม)" });

        // อัปเดต Discount และ NetAmount ของออเดอร์ (1 แต้ม = 1 บาท)
        decimal discount     = points;
        order.DiscountAmount = (order.DiscountAmount ?? 0) + discount;
        order.NetAmount      = (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0);

        // ลดแต้มสมาชิก
        member.Points = (member.Points ?? 0) - points;

        // บันทึก PointTransaction TypeId=2 (Redeem)
        int nextTransId = (_db.Pointtransactions.Any() ? _db.Pointtransactions.Max(t => t.TransId) : 0) + 1;
        _db.Pointtransactions.Add(new Pointtransaction
        {
            TransId    = nextTransId,
            MemberId   = member.MemberId,
            TypeId     = 2,             // Redeem
            Amount     = points,
            RefOrderId = orderId,
            CreatedBy  = StaffId(),
            CreatedAt  = DateTime.Now
        });

        _db.SaveChanges();

        return Json(new { ok = true, newNetAmount = order.NetAmount, remainPoints = member.Points });
    }

    // =====================================================================
    // RedeemStamp — ใช้ 10 Stamp แลกเครื่องดื่มฟรี 1 แก้ว (AJAX POST)
    // =====================================================================
    [HttpPost]
    public IActionResult RedeemStamp(int orderId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false, message = "ไม่ได้ Login" });
        // เฉพาะ Cashier (2), Manager (3), Owner (5) ใช้ Stamp ให้ลูกค้าได้
        if (!HasRole(2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var order = _db.Orders.Find(orderId);
        if (order == null || order.OrderStatusId != 1)
            return Json(new { ok = false, message = "ไม่พบออเดอร์หรือสถานะไม่ถูกต้อง" });

        if (!order.MemberId.HasValue)
            return Json(new { ok = false, message = "ออเดอร์นี้ไม่มีข้อมูลสมาชิก" });

        var member = _db.Members.Find(order.MemberId.Value);
        if (member == null)
            return Json(new { ok = false, message = "ไม่พบสมาชิก" });

        if ((member.StampBalance ?? 0) < 10)
            return Json(new { ok = false, message = $"Stamp ไม่พอ (มี {member.StampBalance ?? 0}/10)" });

        // คำนวณมูลค่า Free Drink = ราคาต่ำสุดของ OrderItem ในออเดอร์นี้
        var orderItems = _db.Orderitems.Where(i => i.OrderId == orderId).ToList();
        decimal freePrice = orderItems.Any() ? orderItems.Min(i => i.UnitPrice ?? 0) : 0;

        // ลด Stamp 10 ดวง
        member.StampBalance  = (member.StampBalance ?? 0) - 10;

        // อัปเดต Discount และ NetAmount
        order.DiscountAmount = (order.DiscountAmount ?? 0) + freePrice;
        order.NetAmount      = Math.Max(0, (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0));

        // บันทึก PointTransaction TypeId=4 (StampRedeem)
        int nextTransId = (_db.Pointtransactions.Any() ? _db.Pointtransactions.Max(t => t.TransId) : 0) + 1;
        _db.Pointtransactions.Add(new Pointtransaction
        {
            TransId    = nextTransId,
            MemberId   = member.MemberId,
            TypeId     = 4,             // StampRedeem
            Amount     = 10,            // ใช้ 10 Stamp
            RefOrderId = orderId,
            CreatedBy  = StaffId(),
            CreatedAt  = DateTime.Now
        });

        _db.SaveChanges();

        return Json(new { ok = true, newNetAmount = order.NetAmount, remainStamps = member.StampBalance, freePrice });
    }

    // =====================================================================
    // ApprovePaymentAtPos — Cashier Approve สลิปจากหน้า POS Queue (AJAX POST)
    // ทำงานเหมือน AdminController.ApprovePayment แต่ Redirect กลับ Queue
    // =====================================================================
    [HttpPost]
    public async Task<IActionResult> ApprovePaymentAtPos(int paymentId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false, message = "ไม่ได้ Login" });
        // Cashier (2), Manager (3), Owner (5) Approve สลิปได้
        if (!HasRole(2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var staffId = StaffId();
        var payment = _db.Payments.Find(paymentId);
        if (payment == null) return Json(new { ok = false, message = "ไม่พบ Payment" });

        var order = _db.Orders.Find(payment.OrderId);
        if (order == null) return Json(new { ok = false, message = "ไม่พบออเดอร์" });

        // ตรวจสอบสถานะออเดอร์ก่อน Approve
        if (order.OrderStatusId == 6)
            return Json(new { ok = false, message = "ออเดอร์นี้ถูกยกเลิกอัตโนมัติแล้ว ไม่สามารถอนุมัติสลิปได้" });
        if (order.OrderStatusId != 1)
            return Json(new { ok = false, message = "สถานะออเดอร์ไม่ถูกต้อง (อนุมัติได้เฉพาะออเดอร์ที่รอชำระ)" });

        // อัปเดต Payment
        payment.PaymentStatusId = 2;    // Approved
        payment.VerifiedBy      = staffId;
        payment.VerifiedAt      = DateTime.Now;

        // อัปเดต Order เป็น Paid + Generate QueueNumber รูปแบบ A001
        order.OrderStatusId = 2;        // Paid
        order.QueueNumber   = GeneratePosQueueNumber();

        // ตัดสต็อก Ingredient ตาม Recipe
        var orderItems = _db.Orderitems.Where(i => i.OrderId == order.OrderId).ToList();
        int nextLogId  = (_db.Inventorylogs.Any() ? _db.Inventorylogs.Max(l => l.LogId) : 0) + 1;

        foreach (var oi in orderItems)
        {
            var recipes = _db.Recipes.Where(r => r.MenuItemId == oi.MenuItemId).ToList();
            foreach (var recipe in recipes)
            {
                var ingredient = _db.Ingredients.Find(recipe.IngredientId);
                if (ingredient == null) continue;

                float used = (recipe.QuantityRequired ?? 0) * (oi.Quantity ?? 1);

                ingredient.StockQuantity = (ingredient.StockQuantity ?? 0) - used;
                ingredient.ReservedQty   = Math.Max(0, (ingredient.ReservedQty ?? 0) - used);

                _db.Inventorylogs.Add(new Inventorylog
                {
                    LogId          = nextLogId++,
                    IngredientId   = ingredient.IngredientId,
                    ReasonTypeId   = 1,
                    RefOrderId     = order.OrderId,
                    CreatedBy      = staffId,
                    QuantityChange = -used,
                    CreatedAt      = DateTime.Now
                });
            }
        }

        // บันทึก Payment, Order, InventoryLogs ทั้งหมดก่อน
        _db.SaveChanges();

        // คำนวณ Points และ Stamp ให้สมาชิก (รันหลัง SaveChanges เพื่อให้ Order ถูก Commit แล้ว)
        if (order.MemberId.HasValue)
        {
            var member = _db.Members.Find(order.MemberId.Value);
            if (member != null)
            {
                int earnedPoints = (int)Math.Floor((order.NetAmount ?? 0) / 10);
                member.Points    = (member.Points ?? 0) + earnedPoints;

                int nextTransId = (_db.Pointtransactions.Any() ? _db.Pointtransactions.Max(t => t.TransId) : 0) + 1;

                // TypeId=1 (PointEarn) — เพิ่มเสมอแม้ earnedPoints = 0 เพื่อให้ประวัติครบ
                _db.Pointtransactions.Add(new Pointtransaction
                {
                    TransId    = nextTransId,
                    MemberId   = member.MemberId,
                    TypeId     = 1,
                    Amount     = earnedPoints,
                    RefOrderId = order.OrderId,
                    CreatedBy  = staffId,
                    CreatedAt  = DateTime.Now
                });

                // บันทึก PointEarn + member.Points ก่อน เพื่อให้ TransId ถูก Commit ก่อน query Max ครั้งถัดไป
                _db.SaveChanges();

                // TypeId=3 (StampEarn) — query Max ใหม่หลัง SaveChanges เพื่อป้องกัน PK ชน
                member.StampBalance = (member.StampBalance ?? 0) + 1;
                int nextStampTransId = _db.Pointtransactions.Max(t => t.TransId) + 1;
                _db.Pointtransactions.Add(new Pointtransaction
                {
                    TransId    = nextStampTransId,
                    MemberId   = member.MemberId,
                    TypeId     = 3,
                    Amount     = 1,
                    RefOrderId = order.OrderId,
                    CreatedBy  = staffId,
                    CreatedAt  = DateTime.Now
                });
                _db.SaveChanges();
            }
        }

        // แจ้ง Customer ว่า Payment ผ่านแล้ว
        await _hub.Clients.Group($"order-{order.OrderId}").SendAsync("NotifyOrderPaid", new
        {
            orderId     = order.OrderId,
            queueNumber = order.QueueNumber
        });

        return Json(new { ok = true, queueNumber = order.QueueNumber });
    }

    // =====================================================================
    // RejectPaymentAtPos — Cashier Reject สลิปจากหน้า POS Queue (AJAX POST)
    // =====================================================================
    [HttpPost]
    public async Task<IActionResult> RejectPaymentAtPos(int paymentId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false, message = "ไม่ได้ Login" });
        if (!HasRole(2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var payment = _db.Payments.Find(paymentId);
        if (payment == null) return Json(new { ok = false, message = "ไม่พบ Payment" });

        payment.PaymentStatusId = 3;    // Rejected
        payment.VerifiedBy      = StaffId();
        payment.VerifiedAt      = DateTime.Now;

        _db.SaveChanges();

        // แจ้ง Customer ให้อัปโหลดสลิปใหม่
        await _hub.Clients.Group($"order-{payment.OrderId}").SendAsync("NotifySlipRejected", new
        {
            orderId = payment.OrderId,
            message = "สลิปถูกปฏิเสธ กรุณาอัปโหลดใหม่"
        });

        return Json(new { ok = true });
    }

    // =====================================================================
    // ApproveGroupCheckin — พนักงาน Manual Approve โปรโมชัน Group Check-in (AJAX POST)
    // ส่วนลด = ราคาเครื่องดื่มราคาต่ำสุดในออเดอร์ (แทน "ฟรี 1 แก้ว")
    // หลังอนุมัติ → ส่ง SignalR NotifyGroupCheckinApproved ให้ลูกค้า reload หน้าชำระเงิน
    // =====================================================================
    [HttpPost]
    public async Task<IActionResult> ApproveGroupCheckin(int orderId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false, message = "ไม่ได้ Login" });
        if (!HasRole(2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var order = _db.Orders.Find(orderId);
        if (order == null || order.OrderStatusId != 1)
            return Json(new { ok = false, message = "ไม่พบออเดอร์หรือสถานะไม่ถูกต้อง (ต้องรออนุมัติก่อนลูกค้าชำระ)" });

        // ตรวจสอบว่าโปรโมชัน GroupCheckin เปิดอยู่
        var promoExists = _db.Promotions.Any(p =>
            p.ConditionType == "GroupCheckin" && p.IsActive == (ulong)1);
        if (!promoExists)
            return Json(new { ok = false, message = "ไม่มีโปรโมชัน Group Check-in ที่เปิดใช้งาน" });

        // คำนวณส่วนลด = ราคาเครื่องดื่ม (Coffee/Non-Coffee) ต่ำสุดในออเดอร์
        // ถ้าไม่มีเมนู Coffee/Non-Coffee ให้ใช้รายการราคาต่ำสุดแทน
        var orderItems = (from oi in _db.Orderitems
                          join mi in _db.Menuitems on oi.MenuItemId equals mi.MenuItemId
                          where oi.OrderId == orderId && (oi.UnitPrice ?? 0) > 0
                          select new { oi.UnitPrice, mi.Category }).ToList();

        if (!orderItems.Any())
            return Json(new { ok = false, message = "ไม่พบรายการในออเดอร์" });

        // โปรนี้คือ "รับเครื่องดื่มฟรี 1 แก้ว" — ต้องมี Coffee/Non-Coffee ในออเดอร์
        var drinkItems = orderItems.Where(i => i.Category == "Coffee" || i.Category == "Non-Coffee").ToList();
        if (!drinkItems.Any())
            return Json(new { ok = false, message = "ออเดอร์นี้ไม่มีเครื่องดื่ม ไม่สามารถใช้โปรโมชัน Group Check-in ได้" });

        decimal discount = drinkItems.Min(i => i.UnitPrice ?? 0);

        if (discount <= 0)
            return Json(new { ok = false, message = "ไม่สามารถคำนวณส่วนลดได้" });

        order.DiscountAmount = (order.DiscountAmount ?? 0) + discount;
        order.NetAmount      = Math.Max(0, (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0));

        _db.SaveChanges();

        // แจ้งลูกค้าว่าได้รับส่วนลด Group Check-in → ให้ reload หน้าชำระเงิน
        await _hub.Clients.Group($"order-{orderId}").SendAsync("NotifyGroupCheckinApproved", new
        {
            orderId,
            discount,
            newNetAmount = order.NetAmount
        });

        return Json(new { ok = true, newNetAmount = order.NetAmount, discount });
    }

    // =====================================================================
    // IssueReward — Staff ออก Reward ให้สมาชิกที่ POS (POST)
    // ตรวจสอบ Points, ตัดแต้ม, บันทึก PointTransaction, ลด Reward.StockQuantity
    // =====================================================================
    [HttpPost]
    public IActionResult IssueReward(int memberId, int rewardId)
    {
        if (!IsLoggedIn()) return Json(new { ok = false, message = "ไม่ได้ Login" });
        // Cashier (2), Manager (3), Owner (5) ออก Reward ได้
        if (!HasRole(2, 3, 5)) return Json(new { ok = false, message = "ไม่มีสิทธิ์" });

        var member = _db.Members.Find(memberId);
        if (member == null) return Json(new { ok = false, message = "ไม่พบสมาชิก" });

        var reward = _db.Rewards.Find(rewardId);
        if (reward == null || reward.IsActive != (ulong)1)
            return Json(new { ok = false, message = "ไม่พบ Reward หรือ Reward ปิดใช้งาน" });

        if ((reward.StockQuantity ?? 0) <= 0)
            return Json(new { ok = false, message = $"{reward.RewardName} สต็อกหมดแล้ว" });

        if ((member.Points ?? 0) < (reward.PointsRequired ?? 0))
            return Json(new { ok = false, message = $"แต้มไม่พอ (มี {member.Points ?? 0}, ต้องการ {reward.PointsRequired ?? 0})" });

        // ตัดแต้มสมาชิก
        member.Points = (member.Points ?? 0) - (reward.PointsRequired ?? 0);

        // ลดสต็อก Reward
        reward.StockQuantity = (reward.StockQuantity ?? 0) - 1;

        // บันทึก PointTransaction TypeId=2 (Redeem)
        int nextTransId = (_db.Pointtransactions.Any() ? _db.Pointtransactions.Max(t => t.TransId) : 0) + 1;
        _db.Pointtransactions.Add(new Pointtransaction
        {
            TransId   = nextTransId,
            MemberId  = member.MemberId,
            TypeId    = 2,              // Redeem
            Amount    = reward.PointsRequired ?? 0,
            CreatedBy = StaffId(),
            CreatedAt = DateTime.Now
        });

        _db.SaveChanges();

        return Json(new
        {
            ok           = true,
            rewardName   = reward.RewardName,
            remainPoints = member.Points
        });
    }

    // =====================================================================
    // GetRewards — AJAX GET รายการ Reward ที่ใช้ได้ สำหรับ Modal ที่ POS
    // =====================================================================
    [HttpGet]
    public IActionResult GetRewards()
    {
        if (!IsLoggedIn()) return Json(new { ok = false });

        var rewards = _db.Rewards
            .Where(r => r.IsActive == (ulong)1 && (r.StockQuantity ?? 0) > 0)
            .OrderBy(r => r.PointsRequired)
            .Select(r => new
            {
                r.RewardId,
                r.RewardName,
                r.PointsRequired,
                r.StockQuantity,
                r.ImageUrl
            })
            .ToList();

        return Json(rewards);
    }

    // =====================================================================
    // Private: Generate QueueNumber รูปแบบ A001 รีเซ็ตทุกวัน
    // =====================================================================
    private string GeneratePosQueueNumber()
    {
        var today    = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var lastQueue = _db.Orders
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && o.QueueNumber != null)
            .OrderByDescending(o => o.OrderId)
            .Select(o => o.QueueNumber)
            .FirstOrDefault();

        int nextNum = 1;
        if (lastQueue != null && lastQueue.Length > 1
            && int.TryParse(lastQueue.Substring(1), out int prev))
        {
            nextNum = prev + 1;
        }

        return $"A{nextNum:D3}";
    }

    // =====================================================================
    // Wastage POST — บันทึก Wastage → ลด StockQuantity + เพิ่ม InventoryLog
    // =====================================================================
    [HttpPost]
    public IActionResult Wastage(int ingredientId, float quantity, string? notes)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(1, 2, 3, 5)) return ForbiddenRedirect();

        if (quantity <= 0)
        {
            TempData["Error"] = "กรุณาระบุปริมาณมากกว่า 0";
            return RedirectToAction("Wastage");
        }

        var ingredient = _db.Ingredients.Find(ingredientId);
        if (ingredient == null)
        {
            TempData["Error"] = "ไม่พบวัตถุดิบที่เลือก";
            return RedirectToAction("Wastage");
        }

        // ตรวจสอบว่าปริมาณที่บันทึกไม่เกิน stock ที่มีอยู่จริง
        float availableStock = (ingredient.StockQuantity ?? 0) - (ingredient.ReservedQty ?? 0);
        if (quantity > availableStock)
        {
            TempData["Error"] = $"ปริมาณที่บันทึก ({quantity} {ingredient.Unit}) เกินกว่าสต็อกที่มี ({availableStock:N2} {ingredient.Unit}) กรุณาตรวจสอบอีกครั้ง";
            return RedirectToAction("Wastage");
        }

        // ลด StockQuantity (ไม่ให้ติดลบ)
        ingredient.StockQuantity = Math.Max(0, (ingredient.StockQuantity ?? 0) - quantity);

        // บันทึก InventoryLog ReasonTypeId=2 (Wastage) พร้อม Notes
        // หมายเหตุ: ต้องรัน SQL ก่อนใช้งาน:
        // ALTER TABLE inventorylogs ADD COLUMN Notes NVARCHAR(500) NULL;
        int newLogId = (_db.Inventorylogs.Any() ? _db.Inventorylogs.Max(l => l.LogId) : 0) + 1;
        _db.Inventorylogs.Add(new Inventorylog
        {
            LogId          = newLogId,
            IngredientId   = ingredientId,
            ReasonTypeId   = 2,
            QuantityChange = -quantity,
            CreatedBy      = StaffId(),
            CreatedAt      = DateTime.Now,
            Notes          = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        });

        _db.SaveChanges();

        var noteText = string.IsNullOrWhiteSpace(notes) ? "" : $" (หมายเหตุ: {notes.Trim()})";
        TempData["Success"] = $"บันทึก Wastage: {ingredient.IngredientName} {quantity} {ingredient.Unit} สำเร็จ{noteText}";
        return RedirectToAction("Wastage");
    }
}
