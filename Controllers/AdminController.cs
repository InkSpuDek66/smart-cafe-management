// Controllers/AdminController.cs
// Controller ฝั่ง Admin — Web Admin สำหรับ Manager, Finance, Owner, Cashier, Barista
// ครอบคลุม: Dashboard, จัดการเมนู, ออเดอร์, Verify สลิป

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Project_CSI402_T2_Y3.Hubs;
using Project_CSI402_T2_Y3.Models.Db;
using Project_CSI402_T2_Y3.ViewModels;

namespace Project_CSI402_T2_Y3.Controllers;

public class AdminController : Controller
{
    private readonly Csi402dbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IHubContext<CafeHub> _hub;

    public AdminController(Csi402dbContext db, IWebHostEnvironment env, IHubContext<CafeHub> hub)
    {
        _db  = db;
        _env = env;
        _hub = hub;
    }

    // ตรวจสอบว่า Login แล้วหรือยัง — ใช้ทุก Action
    private bool IsLoggedIn() => HttpContext.Session.GetInt32("StaffRoleId").HasValue;

    // ตรวจสอบว่า Role มีสิทธิ์หรือไม่
    private bool HasRole(params int[] allowedRoles)
    {
        var roleId = HttpContext.Session.GetInt32("StaffRoleId");
        return roleId.HasValue && allowedRoles.Contains(roleId.Value);
    }

    private IActionResult ForbiddenRedirect()
    {
        TempData["Error"] = "คุณไม่มีสิทธิ์เข้าถึงหน้านี้";
        return RedirectToAction("Index");
    }

    // ตั้งค่า ViewBag ที่ต้องการทุกหน้าก่อนที่ Action จะทำงาน
    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        ViewBag.PendingPaymentCount = _db.Payments.Count(p => p.PaymentStatusId == 1);
    }

    // ============================================================
    // GET /Admin
    // Dashboard ภาพรวมระบบ
    // ============================================================
    public IActionResult Index()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

        var today    = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var viewModel = new DashboardViewModel
        {
            TotalMembers    = _db.Members.Count(),
            TotalMenuItems  = _db.Menuitems.Count(),
            ActiveMenuItems = _db.Menuitems.Count(m => m.IsAvailable == (ulong)1),
            TotalStaff      = _db.Staff.Count(s => s.IsActive == (ulong)1),

            TodayOrderCount = _db.Orders.Count(o =>
                o.CreatedAt >= today && o.CreatedAt < tomorrow),

            TodayRevenue = _db.Orders
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow
                         && o.OrderStatusId >= 2 && o.OrderStatusId != 6)
                .Sum(o => (decimal?)o.NetAmount) ?? 0,

            LowStockAlerts = _db.Ingredients
                .Where(i => i.StockQuantity.HasValue && i.ReorderLevel.HasValue
                         && i.StockQuantity < i.ReorderLevel)
                .Select(i => new LowStockAlertItem
                {
                    IngredientName  = i.IngredientName ?? "",
                    StockQuantity   = i.StockQuantity ?? 0,
                    ReorderLevel    = i.ReorderLevel ?? 0,
                    Unit            = i.Unit ?? ""
                })
                .ToList()
        };

        // Top 5 เมนูขายดีวันนี้ (จำนวนชิ้นรวม)
        var todayOrderIds = _db.Orders
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow
                     && o.OrderStatusId >= 2 && o.OrderStatusId != 6)
            .Select(o => o.OrderId)
            .ToList();

        var menuDict = _db.Menuitems.ToDictionary(m => m.MenuItemId, m => m.MenuName ?? "?");

        viewModel.Top5BestSellers = _db.Orderitems
            .Where(i => todayOrderIds.Contains(i.OrderId ?? 0))
            .ToList()
            .GroupBy(i => i.MenuItemId ?? 0)
            .Select(g => new BestSellerItem
            {
                MenuName = menuDict.GetValueOrDefault(g.Key, "?"),
                Quantity = g.Sum(i => i.Quantity ?? 0),
                Revenue  = g.Sum(i => (i.UnitPrice ?? 0) * (i.Quantity ?? 0))
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToList();

        // ต้นทุน Wastage วันนี้
        var ingredCostDict = _db.Ingredients
            .Where(i => i.CostPerUnit.HasValue)
            .ToDictionary(i => i.IngredientId, i => i.CostPerUnit ?? 0);

        viewModel.TodayWastageCost = _db.Inventorylogs
            .Where(l => l.ReasonTypeId == 2 && l.CreatedAt >= today && l.CreatedAt < tomorrow)
            .ToList()
            .Sum(l => (decimal)Math.Abs(l.QuantityChange ?? 0) * ingredCostDict.GetValueOrDefault(l.IngredientId ?? 0, 0));

        // ยอดขายรายชั่วโมงของวันนี้ (สำหรับ widget บน Dashboard)
        var hourlySalesToday = _db.Orders
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow
                     && o.OrderStatusId >= 2 && o.OrderStatusId != 6)
            .ToList()
            .GroupBy(o => o.CreatedAt?.Hour ?? 0)
            .Select(g => new
            {
                Hour       = g.Key,
                OrderCount = g.Count(),
                Revenue    = g.Sum(o => o.NetAmount ?? 0)
            })
            .OrderBy(x => x.Hour)
            .ToList<object>();

        ViewBag.HourlySalesToday = hourlySalesToday;

        // ออเดอร์ล่าสุด 10 รายการ — ดึง ItemCount แยกเพื่อหลีกเลี่ยง N+1 query
        var recentOrdersBase = (from o in _db.Orders
                                join t in _db.Tables on o.TableId equals t.TableId into tj
                                from t in tj.DefaultIfEmpty()
                                join s in _db.Orderstatuses on o.OrderStatusId equals s.OrderStatusId into sj
                                from s in sj.DefaultIfEmpty()
                                orderby o.CreatedAt descending
                                select new
                                {
                                    o.OrderId,
                                    TableNumber   = t != null ? t.TableNumber : "—",
                                    CustomerName  = o.GuestName,
                                    o.QueueNumber,
                                    o.NetAmount,
                                    o.OrderStatusId,
                                    StatusName    = s != null ? s.StatusName : "—",
                                    o.CreatedAt
                                })
                               .Take(10)
                               .ToList();

        var recentOrderIds = recentOrdersBase.Select(o => o.OrderId).ToList();
        var itemCountMap   = _db.Orderitems
            .Where(i => recentOrderIds.Contains(i.OrderId ?? 0))
            .GroupBy(i => i.OrderId ?? 0)
            .ToDictionary(g => g.Key, g => g.Count());

        var recentOrders = recentOrdersBase.Select(o => new AdminOrderRowViewModel
        {
            OrderId       = o.OrderId,
            TableNumber   = o.TableNumber,
            CustomerName  = o.CustomerName,
            QueueNumber   = o.QueueNumber,
            NetAmount     = o.NetAmount ?? 0,
            OrderStatusId = o.OrderStatusId ?? 0,
            StatusName    = o.StatusName,
            CreatedAt     = o.CreatedAt,
            ItemCount     = itemCountMap.GetValueOrDefault(o.OrderId, 0)
        }).ToList();

        ViewBag.RecentOrders = recentOrders;

        // สลิปรอ Verify
        ViewBag.PendingPaymentCount = _db.Payments.Count(p => p.PaymentStatusId == 1);

        return View(viewModel);
    }

    // ============================================================
    // GET /Admin/MenuList
    // รายการเมนูทั้งหมด
    // ============================================================
    public IActionResult MenuList()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

        // auto-close เมนู seasonal ที่หมดอายุ — ปิด IsAvailable อัตโนมัติเมื่อเข้าหน้านี้
        var now = DateTime.Now;
        var expired = _db.Menuitems
            .Where(m => m.IsSeasonal == (ulong)1
                     && m.IsAvailable == (ulong)1
                     && m.SeasonEndDate.HasValue
                     && m.SeasonEndDate.Value < now)
            .ToList();

        if (expired.Any())
        {
            foreach (var m in expired)
                m.IsAvailable = (ulong)0;
            _db.SaveChanges();
        }

        var items = _db.Menuitems.OrderBy(m => m.MenuItemId).ToList();

        var viewModel = new AdminMenuListViewModel
        {
            Items = items
        };

        return View(viewModel);
    }

    // ============================================================
    // POST /Admin/ToggleMenuAvailability/{id}
    // เปิด/ปิดสถานะการขายเมนู (IsAvailable)
    // ============================================================
    [HttpPost]
    public IActionResult ToggleMenuAvailability(int id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var item = _db.Menuitems.Find(id);
        if (item == null)
        {
            TempData["Error"] = "ไม่พบเมนูที่ต้องการ";
            return RedirectToAction("MenuList");
        }

        // สลับสถานะ
        item.IsAvailable = item.IsAvailable == (ulong)1 ? (ulong)0 : (ulong)1;
        _db.SaveChanges();

        var statusText = item.IsAvailable == (ulong)1 ? "เปิดขาย" : "ปิดชั่วคราว";
        TempData["Success"] = $"เปลี่ยนสถานะ '{item.MenuName}' เป็น {statusText} สำเร็จ";
        return RedirectToAction("MenuList");
    }

    // ============================================================
    // GET /Admin/MenuCreate
    // ฟอร์มสร้างเมนูใหม่
    // ============================================================
    public IActionResult MenuCreate()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();
        return View("MenuForm", new MenuFormViewModel { IsEdit = false });
    }

    // ============================================================
    // POST /Admin/MenuCreate
    // บันทึกเมนูใหม่ — รองรับ Upload รูปภาพหรือกรอก URL
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> MenuCreate(MenuFormViewModel form, IFormFile? imageFile)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        int newId = (_db.Menuitems.Any() ? _db.Menuitems.Max(m => m.MenuItemId) : 0) + 1;

        // ถ้าอัปโหลดไฟล์รูปมา ให้บันทึกลง wwwroot/uploads/menus/ และใช้ URL นั้น
        string? imageUrl = string.IsNullOrWhiteSpace(form.ImageUrl) ? null : form.ImageUrl;
        if (imageFile != null && imageFile.Length > 0)
            imageUrl = await SaveMenuImage(imageFile, newId);

        _db.Menuitems.Add(new Menuitem
        {
            MenuItemId      = newId,
            MenuName        = form.MenuName,
            MenuDescription = form.MenuDescription,
            Price           = form.Price,
            Category        = form.Category,
            IsAvailable     = form.IsAvailable ? (ulong)1 : (ulong)0,
            ImageUrl        = imageUrl,
            IsSeasonal      = form.IsSeasonal ? (ulong)1 : (ulong)0,
            SeasonStartDate = ParseDateTime(form.SeasonStartDate),
            SeasonEndDate   = ParseDateTime(form.SeasonEndDate),
            ToppingGroup    = string.IsNullOrEmpty(form.ToppingGroup) ? null : form.ToppingGroup
        });

        _db.SaveChanges();
        TempData["Success"] = $"เพิ่มเมนู '{form.MenuName}' สำเร็จ";
        return RedirectToAction("MenuList");
    }

    // ============================================================
    // GET /Admin/MenuEdit/5
    // ฟอร์มแก้ไขเมนู
    // ============================================================
    public IActionResult MenuEdit(int id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var item = _db.Menuitems.FirstOrDefault(m => m.MenuItemId == id);
        if (item == null) return NotFound();

        var form = new MenuFormViewModel
        {
            IsEdit          = true,
            MenuItemId      = item.MenuItemId,
            MenuName        = item.MenuName ?? "",
            MenuDescription = item.MenuDescription,
            Price           = item.Price ?? 0,
            Category        = item.Category ?? "",
            IsAvailable     = item.IsAvailable == (ulong)1,
            ImageUrl        = item.ImageUrl,
            IsSeasonal      = item.IsSeasonal == (ulong)1,
            SeasonStartDate = item.SeasonStartDate?.ToString("yyyy-MM-ddTHH:mm"),
            SeasonEndDate   = item.SeasonEndDate?.ToString("yyyy-MM-ddTHH:mm"),
            ToppingGroup    = item.ToppingGroup
        };

        return View("MenuForm", form);
    }

    // ============================================================
    // POST /Admin/MenuEdit/5
    // บันทึกการแก้ไขเมนู — รองรับ Upload รูปใหม่หรือเปลี่ยน URL
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> MenuEdit(int id, MenuFormViewModel form, IFormFile? imageFile)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var item = _db.Menuitems.FirstOrDefault(m => m.MenuItemId == id);
        if (item == null) return NotFound();

        // ถ้าอัปโหลดไฟล์ใหม่มา ให้ใช้รูปใหม่ มิฉะนั้นคงรูปเดิม (ถ้า form.ImageUrl ว่างด้วย)
        string? imageUrl = string.IsNullOrWhiteSpace(form.ImageUrl) ? item.ImageUrl : form.ImageUrl;
        if (imageFile != null && imageFile.Length > 0)
            imageUrl = await SaveMenuImage(imageFile, id);

        item.MenuName        = form.MenuName;
        item.MenuDescription = form.MenuDescription;
        item.Price           = form.Price;
        item.Category        = form.Category;
        item.IsAvailable     = form.IsAvailable ? (ulong)1 : (ulong)0;
        item.ImageUrl        = imageUrl;
        item.IsSeasonal      = form.IsSeasonal ? (ulong)1 : (ulong)0;
        item.SeasonStartDate = ParseDateTime(form.SeasonStartDate);
        item.SeasonEndDate   = ParseDateTime(form.SeasonEndDate);
        item.ToppingGroup    = string.IsNullOrEmpty(form.ToppingGroup) ? null : form.ToppingGroup;

        _db.SaveChanges();
        TempData["Success"] = $"แก้ไขเมนู '{form.MenuName}' สำเร็จ";
        return RedirectToAction("MenuList");
    }

    // ============================================================
    // POST /Admin/MenuDelete/5
    // ลบเมนู
    // ============================================================
    [HttpPost]
    public IActionResult MenuDelete(int id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var item = _db.Menuitems.FirstOrDefault(m => m.MenuItemId == id);
        if (item != null)
        {
            _db.Menuitems.Remove(item);
            _db.SaveChanges();
            TempData["Success"] = $"ลบเมนู '{item.MenuName}' สำเร็จ";
        }

        return RedirectToAction("MenuList");
    }

    // ============================================================
    // GET /Admin/Orders?status=all
    // รายการออเดอร์ทั้งหมด กรองตามสถานะได้
    // ============================================================
    public IActionResult Orders(string? status)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

        var query = from o in _db.Orders
                    join t in _db.Tables on o.TableId equals t.TableId into tj
                    from t in tj.DefaultIfEmpty()
                    join s in _db.Orderstatuses on o.OrderStatusId equals s.OrderStatusId into sj
                    from s in sj.DefaultIfEmpty()
                    select new AdminOrderRowViewModel
                    {
                        OrderId         = o.OrderId,
                        TableNumber     = t != null ? t.TableNumber : "—",
                        CustomerName    = o.GuestName,
                        QueueNumber     = o.QueueNumber,
                        NetAmount       = o.NetAmount ?? 0,
                        OrderStatusId   = o.OrderStatusId ?? 0,
                        StatusName      = s != null ? s.StatusName : "—",
                        CreatedAt       = o.CreatedAt,
                        ItemCount       = _db.Orderitems.Count(i => i.OrderId == o.OrderId)
                    };

        // กรองตามสถานะ
        if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusId))
            query = query.Where(o => o.OrderStatusId == statusId);

        var viewModel = new AdminOrderViewModel
        {
            Orders       = query.OrderByDescending(o => o.CreatedAt).ToList(),
            FilterStatus = status
        };

        return View(viewModel);
    }

    // ============================================================
    // POST /Admin/UpdateOrderStatus
    // อัปเดตสถานะออเดอร์ (Barista/Cashier ใช้)
    // ============================================================
    [HttpPost]
    public IActionResult UpdateOrderStatus(int orderId, int newStatusId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        // Cashier (2), Manager (3), Owner (5) อัปเดตสถานะออเดอร์ได้
        if (!HasRole(2, 3, 5)) return ForbiddenRedirect();

        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null) return NotFound();

        order.OrderStatusId = newStatusId;

        // Generate QueueNumber เมื่อสถานะเปลี่ยนเป็น Paid (2) ตาม Business Rule
        // หรือเป็น fallback ที่ Ready (4) กรณีที่ผ่าน flow อื่นมาโดยไม่มี QueueNumber
        if ((newStatusId == 2 || newStatusId == 4) && string.IsNullOrEmpty(order.QueueNumber))
        {
            order.QueueNumber = GenerateQueueNumber();
        }

        _db.SaveChanges();
        TempData["Success"] = $"อัปเดตสถานะออเดอร์ #{orderId} สำเร็จ";
        return RedirectToAction("Orders");
    }

    // ============================================================
    // GET /Admin/Payments
    // รายการสลิปรอ Verify
    // ============================================================
    public IActionResult Payments()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        // Cashier (2), Manager (3), Finance (4), Owner (5) ตรวจสลิปได้
        if (!HasRole(2, 3, 4, 5)) return ForbiddenRedirect();

        var payments = (from p in _db.Payments
                        join o in _db.Orders on p.OrderId equals o.OrderId into oj
                        from o in oj.DefaultIfEmpty()
                        join t in _db.Tables on o.TableId equals t.TableId into tj
                        from t in tj.DefaultIfEmpty()
                        join pm in _db.Paymentmethods on p.PaymentMethodId equals pm.PaymentMethodId into pmj
                        from pm in pmj.DefaultIfEmpty()
                        join ps in _db.Paymentstatuses on p.PaymentStatusId equals ps.PaymentStatusId into psj
                        from ps in psj.DefaultIfEmpty()
                        orderby p.PaymentStatusId ascending, o.CreatedAt descending
                        select new AdminPaymentRowViewModel
                        {
                            PaymentId           = p.PaymentId,
                            OrderId             = p.OrderId ?? 0,
                            TableNumber         = t != null ? t.TableNumber : "—",
                            CustomerName        = o != null ? o.GuestName : "—",
                            QueueNumber         = o != null ? o.QueueNumber : null,
                            Amount              = p.Amount ?? 0,
                            SlipUrl             = p.SlipUrl,
                            PaymentStatusId     = p.PaymentStatusId ?? 0,
                            StatusName          = ps != null ? ps.StatusName : "—",
                            PaymentMethodName   = pm != null ? pm.MethodName : "—",
                            CreatedAt           = o != null ? o.CreatedAt : null
                        })
                       .ToList();

        var viewModel = new AdminPaymentViewModel
        {
            Payments = payments
        };

        return View(viewModel);
    }

    // ============================================================
    // POST /Admin/ApprovePayment/5
    // Approve สลิป → Paid → ตัดสต็อก → คำนวณ Points → Generate Queue
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> ApprovePayment(int paymentId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(2, 3, 4, 5)) return ForbiddenRedirect();

        var staffId = HttpContext.Session.GetInt32("StaffId");
        var payment = _db.Payments.FirstOrDefault(p => p.PaymentId == paymentId);
        if (payment == null) return NotFound();

        var order = _db.Orders.FirstOrDefault(o => o.OrderId == payment.OrderId);
        if (order == null) return NotFound();

        // ตรวจสอบสถานะออเดอร์ก่อน Approve
        if (order.OrderStatusId == 6)
        {
            TempData["Error"] = $"ออเดอร์ #{order.OrderId} ถูกยกเลิกอัตโนมัติแล้ว ไม่สามารถอนุมัติสลิปได้";
            return RedirectToAction("Payments");
        }
        if (order.OrderStatusId != 1)
        {
            TempData["Error"] = $"สถานะออเดอร์ #{order.OrderId} ไม่ถูกต้อง (อนุมัติได้เฉพาะออเดอร์ที่รอชำระ)";
            return RedirectToAction("Payments");
        }

        // อัปเดต Payment
        payment.PaymentStatusId = 2;    // Approved
        payment.VerifiedBy      = staffId;
        payment.VerifiedAt      = DateTime.Now;

        // อัปเดต Order เป็น Paid + Generate QueueNumber
        order.OrderStatusId = 2;        // Paid
        order.QueueNumber   = GenerateQueueNumber();

        // ตัดสต็อก Ingredient ตาม Recipe
        var orderItems = _db.Orderitems.Where(i => i.OrderId == order.OrderId).ToList();
        int nextLogId  = (_db.Inventorylogs.Any() ? _db.Inventorylogs.Max(l => l.LogId) : 0) + 1;

        foreach (var oi in orderItems)
        {
            var recipes = _db.Recipes.Where(r => r.MenuItemId == oi.MenuItemId).ToList();
            foreach (var recipe in recipes)
            {
                var ingredient = _db.Ingredients.FirstOrDefault(i => i.IngredientId == recipe.IngredientId);
                if (ingredient == null) continue;

                float used = (recipe.QuantityRequired ?? 0) * (oi.Quantity ?? 1);

                // ลด StockQuantity และคืน ReservedQty
                ingredient.StockQuantity    = (ingredient.StockQuantity ?? 0) - used;
                ingredient.ReservedQty      = Math.Max(0, (ingredient.ReservedQty ?? 0) - used);

                // บันทึก InventoryLog
                _db.Inventorylogs.Add(new Inventorylog
                {
                    LogId           = nextLogId++,
                    IngredientId    = ingredient.IngredientId,
                    ReasonTypeId    = 1,        // Sale
                    RefOrderId      = order.OrderId,
                    CreatedBy       = staffId,
                    QuantityChange  = -used,    // ค่าลบ = ลดสต็อก
                    CreatedAt       = DateTime.Now
                });
            }
        }

        // บันทึก Payment, Order, InventoryLogs ทั้งหมดก่อน
        _db.SaveChanges();

        // คำนวณ Points ถ้าเป็นสมาชิก (รันหลัง SaveChanges เพื่อให้ Order ถูก Commit แล้ว)
        if (order.MemberId.HasValue)
        {
            var member  = _db.Members.FirstOrDefault(m => m.MemberId == order.MemberId);
            if (member != null)
            {
                int earnedPoints = (int)Math.Floor((order.NetAmount ?? 0) / 10);
                member.Points = (member.Points ?? 0) + earnedPoints;

                int nextTransId = (_db.Pointtransactions.Any() ? _db.Pointtransactions.Max(t => t.TransId) : 0) + 1;

                _db.Pointtransactions.Add(new Pointtransaction
                {
                    TransId     = nextTransId,
                    MemberId    = member.MemberId,
                    TypeId      = 1,            // Earn
                    Amount      = earnedPoints,
                    RefOrderId  = order.OrderId,
                    CreatedBy   = staffId,
                    CreatedAt   = DateTime.Now
                });

                // บันทึก PointEarn + member.Points ก่อน เพื่อให้ TransId ถูก Commit ก่อน query Max ครั้งถัดไป
                _db.SaveChanges();

                // 1 ออเดอร์ = 1 Stamp เสมอ — query Max ใหม่หลัง SaveChanges เพื่อป้องกัน PK ชน
                member.StampBalance = (member.StampBalance ?? 0) + 1;
                int nextStampTransId = _db.Pointtransactions.Max(t => t.TransId) + 1;
                _db.Pointtransactions.Add(new Pointtransaction
                {
                    TransId     = nextStampTransId,
                    MemberId    = member.MemberId,
                    TypeId      = 3,            // StampEarn
                    Amount      = 1,
                    RefOrderId  = order.OrderId,
                    CreatedBy   = staffId,
                    CreatedAt   = DateTime.Now
                });
                _db.SaveChanges();
            }
        }

        // แจ้ง Customer ว่าชำระสำเร็จ + คิวหมายเลข
        await _hub.Clients.Group($"order-{order.OrderId}").SendAsync("NotifyOrderPaid", new
        {
            orderId     = order.OrderId,
            queueNumber = order.QueueNumber
        });

        TempData["Success"] = $"อนุมัติสลิปออเดอร์ #{order.OrderId} สำเร็จ — คิวหมายเลข {order.QueueNumber}";
        return RedirectToAction("Payments");
    }

    // ============================================================
    // POST /Admin/RejectPayment/5
    // Reject สลิป — รับเหตุผล แจ้งลูกค้าผ่าน SignalR พร้อมเหตุผล
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> RejectPayment(int paymentId, string? rejectReason)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(2, 3, 4, 5)) return ForbiddenRedirect();

        var staffId = HttpContext.Session.GetInt32("StaffId");
        var payment = _db.Payments.FirstOrDefault(p => p.PaymentId == paymentId);
        if (payment == null) return NotFound();

        payment.PaymentStatusId = 3;                            // Rejected
        payment.VerifiedBy      = staffId;
        payment.VerifiedAt      = DateTime.Now;
        payment.RejectReason    = rejectReason?.Trim();

        _db.SaveChanges();

        // แจ้ง Customer ให้อัปโหลดสลิปใหม่ พร้อมเหตุผล
        await _hub.Clients.Group($"order-{payment.OrderId}").SendAsync("NotifySlipRejected", new
        {
            orderId      = payment.OrderId,
            message      = "สลิปถูกปฏิเสธ กรุณาอัปโหลดใหม่",
            rejectReason = payment.RejectReason ?? "ไม่ระบุเหตุผล"
        });

        TempData["Success"] = $"ปฏิเสธสลิป Payment #{paymentId} แล้ว";
        return RedirectToAction("Payments");
    }

    // ============================================================
    // GET /Admin/Setup
    // สร้าง Admin Account ครั้งแรก — เฉพาะเมื่อยังไม่มี Staff ในระบบ
    // ============================================================
    public IActionResult Setup()
    {
        // ถ้ามี Staff แล้ว ปิดหน้านี้ทันที
        if (_db.Staff.Any())
            return Content("มีบัญชีพนักงานในระบบแล้ว Setup ไม่สามารถใช้งานได้");

        return View();
    }

    [HttpPost]
    public IActionResult Setup(string username, string password, string firstName, string lastName)
    {
        if (_db.Staff.Any())
            return Content("Setup ปิดใช้งานแล้ว");

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = Convert.ToHexString(
            sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))
        ).ToLower();

        // Generate StaffId ตาม format BBNNNN: BB=2 หลักท้ายปี พ.ศ., NNNN=0001 เสมอ (เพราะยังไม่มี Staff)
        int buddhistYear = DateTime.Now.Year + 543;
        int staffId      = (buddhistYear % 100) * 10_000 + 1;

        _db.Staff.Add(new Staff
        {
            StaffId     = staffId,
            StaffRoleId = 5,    // Owner
            FirstName   = firstName,
            LastName    = lastName,
            Username    = username,
            PasswordHash = hash,
            IsActive    = (ulong)1
        });
        _db.SaveChanges();

        return RedirectToAction("Login", "Account");
    }

    // ============================================================
    // Private: Generate QueueNumber รูปแบบ A001 รีเซ็ตทุกวัน
    // ============================================================
    private string GenerateQueueNumber()
    {
        var today    = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // หา Queue ล่าสุดของวันนี้
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

    // ============================================================
    // Private: บันทึกไฟล์รูปเมนูลง wwwroot/uploads/menus/ และคืน URL
    // ============================================================
    private async Task<string> SaveMenuImage(IFormFile file, int menuItemId)
    {
        string uploadPath = Path.Combine(_env.WebRootPath, "uploads", "menus");
        Directory.CreateDirectory(uploadPath);

        string ext      = Path.GetExtension(file.FileName);
        string fileName = $"menu_{menuItemId}_{Guid.NewGuid():N}{ext}";
        string filePath = Path.Combine(uploadPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/menus/{fileName}";
    }

    // ============================================================
    // Private: แปลง string เป็น DateOnly (ใช้กับ Promotion.StartDate / EndDate)
    // ============================================================
    private static DateOnly? ParseDate(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return null;
        return DateOnly.TryParse(dateStr, out var d) ? d : null;
    }

    // Private: แปลง string เป็น DateTime (ใช้กับ MenuItem.SeasonStartDate / SeasonEndDate — รองรับ datetime-local)
    private static DateTime? ParseDateTime(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return null;
        return DateTime.TryParse(dateStr, out var d) ? d : null;
    }

    // ============================================================
    // GET /Admin/Inventory
    // แสดงสต็อกวัตถุดิบทั้งหมด + แบบฟอร์ม Restock
    // ============================================================
    public IActionResult Inventory()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect(); // Manager, Owner

        var ingredients = _db.Ingredients.OrderBy(i => i.IngredientName).ToList();

        // แสดงประวัติ Restock 20 รายการล่าสุด
        var staffDict = _db.Staff.ToDictionary(s => s.StaffId, s => (s.FirstName + " " + s.LastName).Trim());
        ViewBag.RestockLogs = _db.Inventorylogs
            .Where(l => l.ReasonTypeId == 3)
            .OrderByDescending(l => l.CreatedAt)
            .Take(20)
            .ToList()
            .Select(l => new
            {
                l.LogId,
                IngredientName = _db.Ingredients
                    .Where(i => i.IngredientId == l.IngredientId)
                    .Select(i => i.IngredientName)
                    .FirstOrDefault() ?? "?",
                l.QuantityChange,
                CreatedBy = l.CreatedBy.HasValue ? staffDict.GetValueOrDefault(l.CreatedBy.Value, "?") : "?",
                l.CreatedAt
            })
            .ToList();

        return View(ingredients);
    }

    // ============================================================
    // POST /Admin/Restock
    // บันทึก Restock — เพิ่ม StockQuantity + InventoryLog ReasonTypeId=3
    // ============================================================
    [HttpPost]
    public IActionResult Restock(int ingredientId, float quantity)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        if (quantity <= 0)
        {
            TempData["Error"] = "กรุณาระบุปริมาณมากกว่า 0";
            return RedirectToAction("Inventory");
        }

        var ingredient = _db.Ingredients.Find(ingredientId);
        if (ingredient == null)
        {
            TempData["Error"] = "ไม่พบวัตถุดิบ";
            return RedirectToAction("Inventory");
        }

        ingredient.StockQuantity = (ingredient.StockQuantity ?? 0) + quantity;

        int newLogId = (_db.Inventorylogs.Any() ? _db.Inventorylogs.Max(l => l.LogId) : 0) + 1;
        _db.Inventorylogs.Add(new Inventorylog
        {
            LogId          = newLogId,
            IngredientId   = ingredientId,
            ReasonTypeId   = 3,         // Restock
            QuantityChange = quantity,  // ค่าบวก = เพิ่มสต็อก
            CreatedBy      = HttpContext.Session.GetInt32("StaffId"),
            CreatedAt      = DateTime.Now
        });

        _db.SaveChanges();
        TempData["Success"] = $"Restock {ingredient.IngredientName} +{quantity} {ingredient.Unit} สำเร็จ";
        return RedirectToAction("Inventory");
    }

    // ============================================================
    // POST /Admin/AddIngredient
    // เพิ่มวัตถุดิบใหม่
    // ============================================================
    [HttpPost]
    public IActionResult AddIngredient(string ingredientName, string unit, float reorderLevel, decimal costPerUnit)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            TempData["Error"] = "กรุณาระบุชื่อวัตถุดิบ";
            return RedirectToAction("Inventory");
        }

        int newId = (_db.Ingredients.Any() ? _db.Ingredients.Max(i => i.IngredientId) : 0) + 1;
        _db.Ingredients.Add(new Ingredient
        {
            IngredientId   = newId,
            IngredientName = ingredientName.Trim(),
            Unit           = unit,
            StockQuantity  = 0,
            ReservedQty    = 0,
            ReorderLevel   = reorderLevel,
            CostPerUnit    = costPerUnit
        });

        _db.SaveChanges();
        TempData["Success"] = $"เพิ่มวัตถุดิบ '{ingredientName}' สำเร็จ";
        return RedirectToAction("Inventory");
    }

    // ============================================================
    // GET /Admin/Recipes
    // แสดงสูตรวัตถุดิบของแต่ละเมนู + แบบฟอร์มเพิ่ม/ลบ
    // ============================================================
    public IActionResult Recipes()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        // จัดกลุ่มสูตรตามเมนู
        var menuItems   = _db.Menuitems.OrderBy(m => m.MenuName).ToList();
        var ingredients = _db.Ingredients.OrderBy(i => i.IngredientName).ToList();
        var recipes     = _db.Recipes.ToList();

        ViewBag.MenuItems   = menuItems;
        ViewBag.Ingredients = ingredients;
        ViewBag.Recipes     = recipes;

        return View();
    }

    // ============================================================
    // POST /Admin/AddRecipe
    // เพิ่มสูตรวัตถุดิบให้เมนู
    // ============================================================
    [HttpPost]
    public IActionResult AddRecipe(int menuItemId, int ingredientId, float quantityRequired, string unit)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        // ถ้ามีสูตรเมนู+วัตถุดิบนี้แล้ว อัปเดตปริมาณแทน
        var existing = _db.Recipes.FirstOrDefault(
            r => r.MenuItemId == menuItemId && r.IngredientId == ingredientId);

        if (existing != null)
        {
            existing.QuantityRequired = quantityRequired;
            existing.Unit             = unit;
        }
        else
        {
            int newId = (_db.Recipes.Any() ? _db.Recipes.Max(r => r.RecipeId) : 0) + 1;
            _db.Recipes.Add(new Recipe
            {
                RecipeId         = newId,
                MenuItemId       = menuItemId,
                IngredientId     = ingredientId,
                QuantityRequired = quantityRequired,
                Unit             = unit
            });
        }

        _db.SaveChanges();
        TempData["Success"] = "บันทึกสูตรสำเร็จ";
        return RedirectToAction("Recipes");
    }

    // ============================================================
    // POST /Admin/DeleteRecipe
    // ลบสูตรวัตถุดิบ
    // ============================================================
    [HttpPost]
    public IActionResult DeleteRecipe(int recipeId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var recipe = _db.Recipes.Find(recipeId);
        if (recipe != null)
        {
            _db.Recipes.Remove(recipe);
            _db.SaveChanges();
        }

        TempData["Success"] = "ลบสูตรสำเร็จ";
        return RedirectToAction("Recipes");
    }

    // ============================================================
    // GET /Admin/Promotions
    // แสดงโปรโมชั่นทั้งหมด + เปิด/ปิดแต่ละรายการ
    // ============================================================
    public IActionResult Promotions()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var promotions = _db.Promotions.OrderBy(p => p.PromotionId).ToList();

        // คำนวณสถิติการใช้งานโปรโมชั่นแต่ละชนิด (อ้างอิงจาก PointTransactions และ Orders)
        var promoUsage = new Dictionary<int, int>();
        foreach (var promo in promotions)
        {
            int usageCount = 0;
            switch (promo.ConditionType)
            {
                // สะสมแต้ม — นับจำนวนครั้งที่มีการ Earn Points
                case null:
                    usageCount = _db.Pointtransactions.Count(pt => pt.TypeId == 1);
                    break;
                // Stamp Card — นับจำนวนครั้งที่ใช้แลก Stamp
                case "StampCount":
                    usageCount = _db.Pointtransactions.Count(pt => pt.TypeId == 4);
                    break;
                // Free Cookie และ Discount — นับออเดอร์ที่มี DiscountAmount > 0
                case "MinAmount":
                    usageCount = _db.Orders.Count(o =>
                        o.DiscountAmount > 0
                        && (o.OrderStatusId == 2 || o.OrderStatusId == 3
                         || o.OrderStatusId == 4 || o.OrderStatusId == 5));
                    break;
                // Birthday — นับออเดอร์ที่ตรงกับวันเกิดสมาชิก
                case "Birthday":
                    var members = _db.Members
                        .Where(m => m.BirthDate.HasValue)
                        .ToList();
                    var paidOrders = _db.Orders
                        .Where(o => o.MemberId != null
                                 && (o.OrderStatusId == 2 || o.OrderStatusId == 3
                                  || o.OrderStatusId == 4 || o.OrderStatusId == 5))
                        .ToList();
                    usageCount = paidOrders.Count(o =>
                        members.Any(m => m.MemberId == o.MemberId
                                      && m.BirthDate.HasValue
                                      && m.BirthDate.Value.Month == (o.CreatedAt ?? DateTime.Today).Month
                                      && m.BirthDate.Value.Day   == (o.CreatedAt ?? DateTime.Today).Day));
                    break;
                // Group Check-in — ไม่มี Log โดยตรง ข้ามไป
                case "GroupCheckin":
                    usageCount = 0;
                    break;
                // Seasonal — นับ OrderItems ที่เป็นเมนู Seasonal
                case "Seasonal":
                    var seasonalMenuIds = _db.Menuitems
                        .Where(m => m.IsSeasonal == (ulong)1)
                        .Select(m => m.MenuItemId)
                        .ToList();
                    usageCount = _db.Orderitems
                        .Count(i => seasonalMenuIds.Contains(i.MenuItemId ?? 0));
                    break;
                default:
                    usageCount = 0;
                    break;
            }
            promoUsage[promo.PromotionId] = usageCount;
        }

        ViewBag.PromoUsage = promoUsage;
        return View(promotions);
    }

    // ============================================================
    // POST /Admin/TogglePromotion
    // สลับ IsActive ของโปรโมชั่น
    // ============================================================
    [HttpPost]
    public IActionResult TogglePromotion(int promotionId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var promo = _db.Promotions.Find(promotionId);
        if (promo != null)
        {
            promo.IsActive = promo.IsActive == (ulong)1 ? (ulong)0 : (ulong)1;
            _db.SaveChanges();
        }

        return RedirectToAction("Promotions");
    }

    // ============================================================
    // POST /Admin/AddPromotion
    // เพิ่มโปรโมชั่นใหม่
    // ============================================================
    [HttpPost]
    public IActionResult AddPromotion(string promotionName, string conditionType,
        string? conditionValue, string? rewardType, string? rewardValue,
        string? startDate, string? endDate)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        if (string.IsNullOrWhiteSpace(promotionName))
        {
            TempData["Error"] = "กรุณาระบุชื่อโปรโมชั่น";
            return RedirectToAction("Promotions");
        }

        int newId = (_db.Promotions.Any() ? _db.Promotions.Max(p => p.PromotionId) : 0) + 1;
        _db.Promotions.Add(new Promotion
        {
            PromotionId   = newId,
            PromotionName = promotionName.Trim(),
            ConditionType = conditionType,
            ConditionValue = conditionValue,
            RewardType    = rewardType,
            RewardValue   = rewardValue,
            IsActive      = (ulong)1,
            StartDate     = ParseDate(startDate),
            EndDate       = ParseDate(endDate)
        });

        _db.SaveChanges();
        TempData["Success"] = $"เพิ่มโปรโมชั่น '{promotionName}' สำเร็จ";
        return RedirectToAction("Promotions");
    }

    // ============================================================
    // POST /Admin/EditPromotion
    // แก้ไขข้อมูลโปรโมชั่นที่มีอยู่
    // ============================================================
    [HttpPost]
    public IActionResult EditPromotion(int promotionId, string promotionName,
        string? conditionType, string? conditionValue, string? rewardType,
        string? rewardValue, string? startDate, string? endDate)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        if (string.IsNullOrWhiteSpace(promotionName))
        {
            TempData["Error"] = "กรุณาระบุชื่อโปรโมชั่น";
            return RedirectToAction("Promotions");
        }

        var promo = _db.Promotions.Find(promotionId);
        if (promo == null)
        {
            TempData["Error"] = "ไม่พบโปรโมชั่นที่ต้องการแก้ไข";
            return RedirectToAction("Promotions");
        }

        promo.PromotionName  = promotionName.Trim();
        promo.ConditionType  = string.IsNullOrEmpty(conditionType) ? null : conditionType;
        promo.ConditionValue = conditionValue;
        promo.RewardType     = rewardType;
        promo.RewardValue    = rewardValue;
        promo.StartDate      = ParseDate(startDate);
        promo.EndDate        = ParseDate(endDate);

        _db.SaveChanges();
        TempData["Success"] = $"แก้ไขโปรโมชั่น '{promotionName}' สำเร็จ";
        return RedirectToAction("Promotions");
    }

    // ============================================================
    // POST /Admin/DeletePromotion
    // ลบโปรโมชั่น (ไม่ลบข้อมูล PointTransactions ที่อ้างอิงอยู่)
    // ============================================================
    [HttpPost]
    public IActionResult DeletePromotion(int promotionId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var promo = _db.Promotions.Find(promotionId);
        if (promo != null)
        {
            _db.Promotions.Remove(promo);
            _db.SaveChanges();
            TempData["Success"] = $"ลบโปรโมชั่น '{promo.PromotionName}' สำเร็จ";
        }

        return RedirectToAction("Promotions");
    }

    // ============================================================
    // GET /Admin/Reports
    // ยอดขายแยกตามวัน และแยกตามเมนู (เฉพาะ OrderStatusId=2,3,4,5)
    // ============================================================
    public IActionResult Reports(string? from, string? to)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 4, 5)) return ForbiddenRedirect(); // Manager, Finance, Owner

        var dateFrom = string.IsNullOrEmpty(from) ? DateTime.Today.AddDays(-6) : DateTime.Parse(from);
        var dateTo   = string.IsNullOrEmpty(to)   ? DateTime.Today             : DateTime.Parse(to);
        var dateToEnd = dateTo.AddDays(1);

        // ออเดอร์ที่ชำระแล้ว (Paid ขึ้นไป)
        var paidOrders = _db.Orders
            .Where(o => (o.OrderStatusId == 2 || o.OrderStatusId == 3
                      || o.OrderStatusId == 4 || o.OrderStatusId == 5)
                     && o.CreatedAt >= dateFrom && o.CreatedAt < dateToEnd)
            .ToList();

        // ยอดขายแยกตามวัน
        var salesByDay = paidOrders
            .GroupBy(o => o.CreatedAt?.Date ?? DateTime.Today)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Date       = g.Key,
                OrderCount = g.Count(),
                Revenue    = g.Sum(o => o.NetAmount ?? 0)
            })
            .ToList();

        // ยอดขายแยกตามเมนู
        var orderIds  = paidOrders.Select(o => o.OrderId).ToList();
        var menuDict  = _db.Menuitems.ToDictionary(m => m.MenuItemId, m => m.MenuName);

        var salesByMenu = _db.Orderitems
            .Where(i => orderIds.Contains(i.OrderId ?? 0))
            .ToList()
            .GroupBy(i => i.MenuItemId ?? 0)
            .Select(g => new
            {
                MenuName = menuDict.GetValueOrDefault(g.Key, "?"),
                Quantity = g.Sum(i => i.Quantity ?? 0),
                Revenue  = g.Sum(i => (i.UnitPrice ?? 0) * (i.Quantity ?? 0))
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        // ยอดขายแยกตามชั่วโมง
        var salesByHour = paidOrders
            .GroupBy(o => o.CreatedAt?.Hour ?? 0)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Hour       = g.Key,
                OrderCount = g.Count(),
                Revenue    = g.Sum(o => o.NetAmount ?? 0)
            })
            .ToList();

        // ยอดชำระแยกตาม Payment Channel (เฉพาะที่ Approved)
        var orderIdsForChannel = paidOrders.Select(o => o.OrderId).ToList();
        var methodDict = _db.Paymentmethods.ToDictionary(pm => pm.PaymentMethodId, pm => pm.MethodName);

        var paymentChannels = _db.Payments
            .Where(p => orderIdsForChannel.Contains(p.OrderId ?? 0) && p.PaymentStatusId == 2)
            .ToList()
            .GroupBy(p => p.PaymentMethodId ?? 0)
            .Select(g => new
            {
                MethodName = methodDict.GetValueOrDefault(g.Key, "?"),
                Count      = g.Count(),
                Total      = g.Sum(p => p.Amount ?? 0)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        // ความนิยมของ Options (ความหวาน, ประเภทนม, ขนาด) จาก OrderItemOptions
        var optionPopularity = _db.Orderitemoptions
            .Where(op => orderIds.Contains(
                _db.Orderitems.Where(i => i.OrderItemId == op.OrderItemId)
                              .Select(i => i.OrderId ?? 0)
                              .FirstOrDefault()))
            .ToList()
            .GroupBy(op => new { op.OptionName, op.OptionValue })
            .Select(g => new
            {
                OptionName  = g.Key.OptionName,
                OptionValue = g.Key.OptionValue,
                Count       = g.Count()
            })
            .OrderBy(x => x.OptionName)
            .ThenByDescending(x => x.Count)
            .ToList();

        // ต้นทุนการสูญเสีย (Wastage Cost Breakdown) ในช่วงวันที่เลือก
        var ingredientCostDict = _db.Ingredients
            .Where(i => i.CostPerUnit.HasValue)
            .ToDictionary(i => i.IngredientId, i => new { i.IngredientName, i.CostPerUnit, i.Unit });

        var wastageLogs = _db.Inventorylogs
            .Where(l => l.ReasonTypeId == 2
                     && l.CreatedAt >= dateFrom && l.CreatedAt < dateToEnd)
            .ToList();

        var wastageCost = wastageLogs
            .GroupBy(l => l.IngredientId ?? 0)
            .Select(g =>
            {
                var info      = ingredientCostDict.GetValueOrDefault(g.Key);
                float totalQty = g.Sum(l => Math.Abs(l.QuantityChange ?? 0));
                decimal cost   = (decimal)totalQty * (info?.CostPerUnit ?? 0);
                return new
                {
                    IngredientName = info?.IngredientName ?? "?",
                    Unit           = info?.Unit ?? "",
                    TotalQty       = totalQty,
                    CostPerUnit    = info?.CostPerUnit ?? 0,
                    TotalCost      = cost
                };
            })
            .OrderByDescending(x => x.TotalCost)
            .ToList();

        // ยอดขายแยกตามวันในสัปดาห์ (จันทร์-อาทิตย์)
        var dayNamesTh = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday,    "จันทร์"  },
            { DayOfWeek.Tuesday,   "อังคาร"  },
            { DayOfWeek.Wednesday, "พุธ"     },
            { DayOfWeek.Thursday,  "พฤหัส"  },
            { DayOfWeek.Friday,    "ศุกร์"   },
            { DayOfWeek.Saturday,  "เสาร์"   },
            { DayOfWeek.Sunday,    "อาทิตย์" }
        };
        var salesByWeekday = paidOrders
            .GroupBy(o => (o.CreatedAt ?? DateTime.Today).DayOfWeek)
            .Select(g => new
            {
                DayOfWeek  = g.Key,
                DayName    = dayNamesTh.GetValueOrDefault(g.Key, g.Key.ToString()),
                OrderCount = g.Count(),
                Revenue    = g.Sum(o => o.NetAmount ?? 0)
            })
            .OrderBy(x => x.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)x.DayOfWeek)
            .ToList();

        // ยอดขายแยกตามเดือน (ใช้ข้อมูลทั้งปีจาก Orders ที่ชำระแล้ว)
        var currentYear   = DateTime.Today.Year;
        var yearStart     = new DateTime(currentYear, 1, 1);
        var yearEnd       = new DateTime(currentYear + 1, 1, 1);

        var allYearOrders = _db.Orders
            .Where(o => (o.OrderStatusId == 2 || o.OrderStatusId == 3
                      || o.OrderStatusId == 4 || o.OrderStatusId == 5)
                     && o.CreatedAt >= yearStart && o.CreatedAt < yearEnd)
            .ToList();

        var thaiMonths = new[]
        {
            "", "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.",
            "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค."
        };

        var salesByMonth = allYearOrders
            .GroupBy(o => (o.CreatedAt ?? DateTime.Today).Month)
            .Select(g => new
            {
                Month      = g.Key,
                MonthName  = thaiMonths[g.Key],
                OrderCount = g.Count(),
                Revenue    = g.Sum(o => o.NetAmount ?? 0)
            })
            .OrderBy(x => x.Month)
            .ToList();

        // ยอดขายแยกตามไตรมาส
        var salesByQuarter = allYearOrders
            .GroupBy(o => ((o.CreatedAt ?? DateTime.Today).Month - 1) / 3 + 1)
            .Select(g => new
            {
                Quarter    = g.Key,
                QuarterName = $"Q{g.Key} ({currentYear})",
                OrderCount = g.Count(),
                Revenue    = g.Sum(o => o.NetAmount ?? 0)
            })
            .OrderBy(x => x.Quarter)
            .ToList();

        // ยอดขายรายปี Year-over-Year (เปรียบเทียบปีนี้ vs ปีที่แล้ว รายเดือน)
        var prevYear      = currentYear - 1;
        var prevYearStart = new DateTime(prevYear, 1, 1);
        var prevYearEnd   = new DateTime(currentYear, 1, 1);

        var prevYearOrders = _db.Orders
            .Where(o => (o.OrderStatusId == 2 || o.OrderStatusId == 3
                      || o.OrderStatusId == 4 || o.OrderStatusId == 5)
                     && o.CreatedAt >= prevYearStart && o.CreatedAt < prevYearEnd)
            .ToList();

        var salesByYearCurrent = allYearOrders
            .GroupBy(o => (o.CreatedAt ?? DateTime.Today).Month)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.NetAmount ?? 0));

        var salesByYearPrev = prevYearOrders
            .GroupBy(o => (o.CreatedAt ?? DateTime.Today).Month)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.NetAmount ?? 0));

        var yoyComparison = Enumerable.Range(1, 12)
            .Select(m => new
            {
                Month       = m,
                MonthName   = thaiMonths[m],
                CurrentYear = salesByYearCurrent.GetValueOrDefault(m, 0),
                PrevYear    = salesByYearPrev.GetValueOrDefault(m, 0)
            })
            .ToList();

        ViewBag.SalesByDay        = salesByDay;
        ViewBag.SalesByMenu       = salesByMenu;
        ViewBag.SalesByHour       = salesByHour;
        ViewBag.SalesByWeekday    = salesByWeekday;
        ViewBag.SalesByMonth      = salesByMonth;
        ViewBag.SalesByQuarter    = salesByQuarter;
        ViewBag.PaymentChannels   = paymentChannels;
        ViewBag.OptionPopularity  = optionPopularity;
        ViewBag.WastageCost       = wastageCost;
        ViewBag.TotalWastageCost  = wastageCost.Sum(x => x.TotalCost);
        ViewBag.DateFrom          = dateFrom.ToString("yyyy-MM-dd");
        ViewBag.DateTo            = dateTo.ToString("yyyy-MM-dd");
        ViewBag.TotalRevenue      = paidOrders.Sum(o => o.NetAmount ?? 0);
        ViewBag.TotalOrders       = paidOrders.Count;
        ViewBag.YoyComparison     = yoyComparison;
        ViewBag.CurrentYear       = currentYear;
        ViewBag.PrevYear          = prevYear;

        return View();
    }

    // ============================================================
    // GET /Admin/Finance
    // Finance Reconciliation — รายการ Payment + สรุปยอดรายวัน
    // ============================================================
    public IActionResult Finance(string? date)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 4, 5)) return ForbiddenRedirect(); // Manager, Finance, Owner

        var targetDate    = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);
        var targetDateEnd = targetDate.AddDays(1);

        // โหลด orderId ที่สร้างในวันนั้น สำหรับกรองสลิปที่ยังไม่ได้ Verify
        var orderIdsOnDay = _db.Orders
            .Where(o => o.CreatedAt >= targetDate && o.CreatedAt < targetDateEnd)
            .Select(o => o.OrderId)
            .ToList();

        // Payments ทั้งหมดในวันนั้น: Verified ในวันนี้ หรือ Pending สำหรับออเดอร์วันนี้
        var payments = _db.Payments
            .Where(p => (p.VerifiedAt >= targetDate && p.VerifiedAt < targetDateEnd)
                     || (p.VerifiedAt == null && orderIdsOnDay.Contains(p.OrderId ?? 0)))
            .ToList();

        var orderDict  = _db.Orders.ToDictionary(o => o.OrderId, o => o);
        var statusDict = new Dictionary<int, string>
        {
            {1,"Pending"}, {2,"Approved"}, {3,"Rejected"}
        };

        var paymentRows = payments.Select(p =>
        {
            var order = orderDict.GetValueOrDefault(p.OrderId ?? 0);
            return new
            {
                p.PaymentId,
                p.OrderId,
                QueueNumber  = order?.QueueNumber,
                Amount       = p.Amount,
                Status       = statusDict.GetValueOrDefault(p.PaymentStatusId ?? 0, "?"),
                p.SlipUrl,
                p.VerifiedAt
            };
        })
        .OrderByDescending(p => p.VerifiedAt)
        .ToList();

        decimal approvedTotal = payments
            .Where(p => p.PaymentStatusId == 2)
            .Sum(p => p.Amount ?? 0);

        // คำนวณ P&L: Revenue - COGS = Gross Profit
        // Revenue = ยอด Approved ในวันนี้
        decimal revenue = approvedTotal;

        // COGS ส่วนที่ 1: ต้นทุน Sale (ตัดสต็อกจาก InventoryLogs ReasonTypeId=1)
        var ingredientCostDict = _db.Ingredients
            .Where(i => i.CostPerUnit.HasValue)
            .ToDictionary(i => i.IngredientId, i => i.CostPerUnit ?? 0);

        decimal saleCogs = _db.Inventorylogs
            .Where(l => l.ReasonTypeId == 1 && l.CreatedAt >= targetDate && l.CreatedAt < targetDateEnd)
            .ToList()
            .Sum(l => (decimal)Math.Abs(l.QuantityChange ?? 0)
                      * ingredientCostDict.GetValueOrDefault(l.IngredientId ?? 0, 0));

        // COGS ส่วนที่ 2: ต้นทุน Wastage (บันทึก Wastage ในวันนั้น)
        decimal wastageCogs = _db.Inventorylogs
            .Where(l => l.ReasonTypeId == 2 && l.CreatedAt >= targetDate && l.CreatedAt < targetDateEnd)
            .ToList()
            .Sum(l => (decimal)Math.Abs(l.QuantityChange ?? 0)
                      * ingredientCostDict.GetValueOrDefault(l.IngredientId ?? 0, 0));

        decimal totalCogs     = saleCogs + wastageCogs;
        decimal grossProfit   = revenue - totalCogs;

        // แยกยอด Approved ตามช่องทางการชำระเงิน (Slip Upload vs Dynamic QR)
        var methodNameDict = _db.Paymentmethods.ToDictionary(pm => pm.PaymentMethodId, pm => pm.MethodName ?? "?");
        var approvedPayments = payments.Where(p => p.PaymentStatusId == 2).ToList();

        var cashVsTransfer = approvedPayments
            .GroupBy(p => p.PaymentMethodId ?? 0)
            .Select(g => new
            {
                MethodName = methodNameDict.GetValueOrDefault(g.Key, "?"),
                Count      = g.Count(),
                Total      = g.Sum(p => p.Amount ?? 0)
            })
            .OrderByDescending(x => x.Total)
            .ToList<object>();

        ViewBag.PaymentRows       = paymentRows;
        ViewBag.ApprovedTotal     = approvedTotal;
        ViewBag.TargetDate        = targetDate.ToString("yyyy-MM-dd");
        ViewBag.ApprovedCount     = payments.Count(p => p.PaymentStatusId == 2);
        ViewBag.PendingCount      = payments.Count(p => p.PaymentStatusId == 1);
        ViewBag.RejectedCount     = payments.Count(p => p.PaymentStatusId == 3);
        ViewBag.CashVsTransfer    = cashVsTransfer;

        // P&L
        ViewBag.Revenue      = revenue;
        ViewBag.SaleCogs     = saleCogs;
        ViewBag.WastageCogs  = wastageCogs;
        ViewBag.TotalCogs    = totalCogs;
        ViewBag.GrossProfit  = grossProfit;

        return View();
    }

    // ============================================================
    // GET /Admin/Tables
    // จัดการโต๊ะ + Generate QR Code URL
    // ============================================================
    public IActionResult Tables()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var tables = _db.Tables.OrderBy(t => t.TableNumber).ToList();

        // คำนวณ TableId ของโต๊ะที่มีหมายเลขสูงสุด (อนุญาตให้ลบได้เฉพาะโต๊ะนี้)
        int lastTableId = 0;
        if (tables.Any())
        {
            lastTableId = tables
                .OrderByDescending(t =>
                {
                    string digits = new string((t.TableNumber ?? "").Where(char.IsDigit).ToArray());
                    return int.TryParse(digits, out int n) ? n : 0;
                })
                .First().TableId;
        }
        ViewBag.LastTableId = lastTableId;

        return View(tables);
    }

    // ============================================================
    // POST /Admin/DeleteTable
    // ลบโต๊ะ — อนุญาตเฉพาะโต๊ะที่มีหมายเลขสูงสุดในระบบเท่านั้น
    // ============================================================
    [HttpPost]
    public IActionResult DeleteTable(int tableId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var table = _db.Tables.FirstOrDefault(t => t.TableId == tableId);
        if (table == null)
        {
            TempData["Error"] = "ไม่พบโต๊ะที่ต้องการลบ";
            return RedirectToAction("Tables");
        }

        // ตรวจสอบว่าเป็นโต๊ะที่มีหมายเลขสูงสุด
        var allNumbers = _db.Tables.ToList();
        int maxNum = allNumbers.Max(t =>
        {
            string digits = new string((t.TableNumber ?? "").Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int n) ? n : 0;
        });
        string tableDigits = new string((table.TableNumber ?? "").Where(char.IsDigit).ToArray());
        int.TryParse(tableDigits, out int thisNum);

        if (thisNum != maxNum)
        {
            TempData["Error"] = $"ลบได้เฉพาะโต๊ะหมายเลขสูงสุด (ปัจจุบันคือ T{maxNum:D2}) เท่านั้น";
            return RedirectToAction("Tables");
        }

        // ตรวจสอบว่าโต๊ะมีออเดอร์ที่ยังดำเนินการอยู่
        bool hasActiveOrder = _db.Orders.Any(o =>
            o.TableId == tableId && o.OrderStatusId >= 1 && o.OrderStatusId <= 4);
        if (hasActiveOrder)
        {
            TempData["Error"] = $"ไม่สามารถลบโต๊ะ {table.TableNumber} ได้ เนื่องจากมีออเดอร์ที่ยังดำเนินการอยู่";
            return RedirectToAction("Tables");
        }

        _db.Tables.Remove(table);
        _db.SaveChanges();
        TempData["Success"] = $"ลบโต๊ะ {table.TableNumber} สำเร็จ";
        return RedirectToAction("Tables");
    }

    // ============================================================
    // POST /Admin/ToggleTableStatus
    // เปิด/ปิดสถานะการใช้งานโต๊ะ
    // ============================================================
    [HttpPost]
    public IActionResult ToggleTableStatus(int tableId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var table = _db.Tables.FirstOrDefault(t => t.TableId == tableId);
        if (table == null)
        {
            TempData["Error"] = "ไม่พบโต๊ะ";
            return RedirectToAction("Tables");
        }

        table.IsActive = table.IsActive == (ulong)1 ? (ulong)0 : (ulong)1;
        _db.SaveChanges();

        string statusTh = table.IsActive == (ulong)1 ? "เปิดใช้งาน" : "ปิดใช้งาน";
        TempData["Success"] = $"เปลี่ยนสถานะโต๊ะ {table.TableNumber} เป็น \"{statusTh}\" แล้ว";
        return RedirectToAction("Tables");
    }

    // ============================================================
    // POST /Admin/AddTable
    // เพิ่มโต๊ะใหม่ — ดึงหมายเลขล่าสุดจาก DB แล้วเพิ่ม +1 อัตโนมัติ
    // ============================================================
    [HttpPost]
    public IActionResult AddTable()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        string tableNumber = GenerateNextTableNumber();

        int newId = (_db.Tables.Any() ? _db.Tables.Max(t => t.TableId) : 0) + 1;
        _db.Tables.Add(new Table
        {
            TableId     = newId,
            TableNumber = tableNumber,
            IsActive    = (ulong)1
        });

        _db.SaveChanges();
        TempData["Success"] = $"เพิ่มโต๊ะ {tableNumber} สำเร็จ";
        return RedirectToAction("Tables");
    }

    // สร้างหมายเลขโต๊ะถัดไปจากหมายเลขสูงสุดในระบบ (รูปแบบ T01, T02, ...)
    private string GenerateNextTableNumber()
    {
        var allNumbers = _db.Tables.Select(t => t.TableNumber).ToList();
        int maxNum = 0;
        foreach (var tn in allNumbers)
        {
            if (string.IsNullOrEmpty(tn)) continue;
            // รองรับรูปแบบ T01, T1, 1, 01
            string digits = new string(tn.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int n) && n > maxNum)
                maxNum = n;
        }
        return $"T{(maxNum + 1):D2}";
    }

    // ============================================================
    // GET /Admin/QrPrint?tableId=1
    // แสดง QR Code แบบ Print-ready (1 โต๊ะ หรือทั้งหมด)
    // ใช้ QRCoder library สร้าง QR Code เป็น base64 PNG โดยไม่ต้องพึ่ง API ภายนอก
    // ============================================================
    public IActionResult QrPrint(int? tableId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        List<Table> tables;
        if (tableId.HasValue)
            tables = _db.Tables.Where(t => t.TableId == tableId.Value).ToList();
        else
            tables = _db.Tables.Where(t => t.IsActive == (ulong)1).OrderBy(t => t.TableNumber).ToList();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        // สร้าง QR Code สำหรับแต่ละโต๊ะ → base64 data URL
        var qrDataUrls = new Dictionary<int, string>();
        using var qrGenerator = new QRCoder.QRCodeGenerator();
        foreach (var table in tables)
        {
            string menuUrl = $"{baseUrl}/Customer/Menu?table={table.TableNumber}";
            using var qrData = qrGenerator.CreateQrCode(menuUrl, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode  = new QRCoder.PngByteQRCode(qrData);
            byte[] png  = qrCode.GetGraphic(5);
            qrDataUrls[table.TableId] = "data:image/png;base64," + Convert.ToBase64String(png);
        }

        ViewBag.Tables     = tables;
        ViewBag.QrDataUrls = qrDataUrls;

        return View();
    }

    // ============================================================
    // GET /Admin/Shifts
    // รายการกะทำงานของพนักงานทั้งหมด พร้อมฟอร์มเพิ่ม/ปิดกะ
    // ============================================================
    public IActionResult Shifts()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        // Manager (3), Owner (5) จัดการกะได้ ส่วน Barista (1), Cashier (2) เช็คอินตัวเองได้
        if (!HasRole(1, 2, 3, 5)) return ForbiddenRedirect();

        var currentStaffId = HttpContext.Session.GetInt32("StaffId");
        var roleId         = HttpContext.Session.GetInt32("StaffRoleId");

        var staffDict = _db.Staff
            .Where(s => s.IsActive == (ulong)1)
            .ToDictionary(s => s.StaffId, s => (s.FirstName + " " + s.LastName).Trim());

        // Manager/Owner เห็นกะของทุกคน ส่วน Barista/Cashier เห็นแค่ของตัวเอง
        IQueryable<Staffshift> query = _db.Staffshifts;
        if (roleId == 1 || roleId == 2)
            query = query.Where(s => s.StaffId == currentStaffId);

        var shifts = query
            .OrderByDescending(s => s.ShiftStart)
            .Take(50)
            .ToList()
            .Select(s => new
            {
                s.ShiftId,
                StaffName  = staffDict.GetValueOrDefault(s.StaffId ?? 0, "?"),
                s.StaffId,
                s.ShiftStart,
                s.ShiftEnd,
                s.CreatedAt,
                // กะที่ยังไม่ปิด (ยังทำงานอยู่)
                IsOpen = s.ShiftEnd == null
            })
            .ToList();

        // กะที่ยังเปิดอยู่ของพนักงานคนปัจจุบัน (ใช้แสดงปุ่ม Clock Out)
        var openShift = _db.Staffshifts
            .Where(s => s.StaffId == currentStaffId && s.ShiftEnd == null)
            .OrderByDescending(s => s.ShiftStart)
            .FirstOrDefault();

        var allStaff = _db.Staff
            .Where(s => s.IsActive == (ulong)1)
            .OrderBy(s => s.FirstName)
            .ToList();

        ViewBag.Shifts         = shifts;
        ViewBag.AllStaff       = allStaff;
        ViewBag.OpenShift      = openShift;
        ViewBag.CurrentStaffId = currentStaffId;
        ViewBag.RoleId         = roleId;
        // เวลา Login ของ session ปัจจุบัน — ใช้สำหรับปุ่มเช็คอินอัตโนมัติ
        ViewBag.LoginTime = HttpContext.Session.GetString("LoginTime");

        return View();
    }

    // ============================================================
    // POST /Admin/AddShift
    // เพิ่มกะทำงานใหม่ — เช็คอินเข้างาน (ShiftEnd ยังว่าง)
    // ============================================================
    [HttpPost]
    public IActionResult AddShift(int? staffId, DateTime shiftStart)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(1, 2, 3, 5)) return ForbiddenRedirect();

        var currentStaffId = HttpContext.Session.GetInt32("StaffId");
        var roleId         = HttpContext.Session.GetInt32("StaffRoleId");

        // Barista/Cashier เพิ่มกะให้ตัวเองได้เท่านั้น
        int targetStaffId = (roleId == 1 || roleId == 2)
            ? (currentStaffId ?? 0)
            : (staffId ?? currentStaffId ?? 0);

        if (targetStaffId == 0)
        {
            TempData["Error"] = "กรุณาระบุพนักงาน";
            return RedirectToAction("Shifts");
        }

        // ตรวจสอบว่าพนักงานคนนี้มีกะที่ยังเปิดอยู่หรือไม่
        bool hasOpenShift = _db.Staffshifts.Any(s => s.StaffId == targetStaffId && s.ShiftEnd == null);
        if (hasOpenShift)
        {
            TempData["Error"] = "พนักงานคนนี้มีกะที่ยังไม่ปิดอยู่ กรุณาปิดกะก่อน";
            return RedirectToAction("Shifts");
        }

        int newId = (_db.Staffshifts.Any() ? _db.Staffshifts.Max(s => s.ShiftId) : 0) + 1;
        _db.Staffshifts.Add(new Staffshift
        {
            ShiftId    = newId,
            StaffId    = targetStaffId,
            ShiftStart = shiftStart,
            ShiftEnd   = null,      // ยังไม่ปิดกะ
            CreatedAt  = DateTime.Now
        });

        _db.SaveChanges();
        TempData["Success"] = "เช็คอินกะทำงานสำเร็จ";
        return RedirectToAction("Shifts");
    }

    // ============================================================
    // POST /Admin/ClockOut
    // ปิดกะทำงาน — บันทึกเวลาออก (ShiftEnd)
    // ============================================================
    [HttpPost]
    public IActionResult ClockOut(int shiftId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(1, 2, 3, 5)) return ForbiddenRedirect();

        var currentStaffId = HttpContext.Session.GetInt32("StaffId");
        var roleId         = HttpContext.Session.GetInt32("StaffRoleId");

        var shift = _db.Staffshifts.Find(shiftId);
        if (shift == null)
        {
            TempData["Error"] = "ไม่พบกะทำงาน";
            return RedirectToAction("Shifts");
        }

        // Barista/Cashier ปิดกะตัวเองได้เท่านั้น
        if ((roleId == 1 || roleId == 2) && shift.StaffId != currentStaffId)
        {
            TempData["Error"] = "ไม่มีสิทธิ์ปิดกะของพนักงานคนอื่น";
            return RedirectToAction("Shifts");
        }

        if (shift.ShiftEnd != null)
        {
            TempData["Error"] = "กะนี้ปิดแล้ว";
            return RedirectToAction("Shifts");
        }

        shift.ShiftEnd = DateTime.Now;
        _db.SaveChanges();

        TempData["Success"] = "ปิดกะทำงานสำเร็จ";
        return RedirectToAction("Shifts");
    }

    // ============================================================
    // POST /Admin/DeleteShift
    // ลบกะทำงาน (เฉพาะ Manager/Owner)
    // ============================================================
    [HttpPost]
    public IActionResult DeleteShift(int shiftId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!HasRole(3, 5)) return ForbiddenRedirect();

        var shift = _db.Staffshifts.Find(shiftId);
        if (shift != null)
        {
            _db.Staffshifts.Remove(shift);
            _db.SaveChanges();
        }

        TempData["Success"] = "ลบกะทำงานสำเร็จ";
        return RedirectToAction("Shifts");
    }
}
