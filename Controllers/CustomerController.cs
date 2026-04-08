// Controllers/CustomerController.cs
// Controller ฝั่งลูกค้า — Mobile Web App (PWA)
// ครอบคลุมการเลือกเมนู, ตะกร้า, สั่งออเดอร์, อัปโหลดสลิป, ติดตามออเดอร์

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Project_CSI402_T2_Y3.Helpers;
using Project_CSI402_T2_Y3.Hubs;
using Project_CSI402_T2_Y3.Models.Db;
using Project_CSI402_T2_Y3.ViewModels;

namespace Project_CSI402_T2_Y3.Controllers;

public class CustomerController : Controller
{
    private readonly Csi402dbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IHubContext<CafeHub> _hub;

    // Session Key ที่ใช้เก็บตะกร้าสินค้า
    private const string CartSessionKey = "CustomerCart";

    public CustomerController(Csi402dbContext db, IWebHostEnvironment env, IHubContext<CafeHub> hub)
    {
        _db  = db;
        _env = env;
        _hub = hub;
    }

    // ============================================================
    // GET /Customer/Menu?table=T01
    // หน้าเมนูหลักของลูกค้า — แสดง Grid รูปสินค้า กรองตาม Category
    // ============================================================
    public IActionResult Menu(string? table, string? category)
    {
        // บันทึกเลขโต๊ะลง Session เพื่อส่งต่อตลอด Flow
        if (!string.IsNullOrEmpty(table))
            HttpContext.Session.SetString("TableNumber", table);

        var tableNumber = HttpContext.Session.GetString("TableNumber");

        // ดึงเมนูที่เปิดใช้งาน กรองตาม Category ถ้ามี
        // เมนู seasonal ที่หมดอายุแล้ว (SeasonEndDate < Today) ให้ถือว่าปิดขายอัตโนมัติ
        var todayOnly = DateOnly.FromDateTime(DateTime.Today);
        var query = _db.Menuitems.Where(m =>
            m.IsAvailable == (ulong)1
            && !(m.IsSeasonal == (ulong)1 && m.SeasonEndDate.HasValue && m.SeasonEndDate.Value < todayOnly));

        if (!string.IsNullOrEmpty(category) && category != "All")
            query = query.Where(m => m.Category == category);

        var menuItems = query.OrderBy(m => m.Category).ThenBy(m => m.MenuName).ToList();

        // ดึง Category ที่มีในระบบ
        var categories = _db.Menuitems
            .Where(m => m.IsAvailable == (ulong)1 && m.Category != null)
            .Select(m => m.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        // นับของในตะกร้าสำหรับแสดงบน Badge
        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();

        // ตรวจสอบว่ามีออเดอร์ที่ยังดำเนินการอยู่สำหรับโต๊ะนี้หรือไม่
        int? activeOrderId = null;
        if (!string.IsNullOrEmpty(tableNumber))
        {
            var tableEntity = _db.Tables.FirstOrDefault(t => t.TableNumber == tableNumber);
            if (tableEntity != null)
            {
                var activeOrder = _db.Orders
                    .Where(o => o.TableId == tableEntity.TableId
                             && o.OrderStatusId >= 1
                             && o.OrderStatusId <= 4)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefault();
                activeOrderId = activeOrder?.OrderId;
            }
        }

        // ดึงข้อมูล Member จาก Session เพื่อแสดง Points/Stamps บน Header
        var memberPhone = HttpContext.Session.GetString("MemberPhone");
        if (!string.IsNullOrEmpty(memberPhone))
        {
            var member = _db.Members.FirstOrDefault(m => m.Phone == memberPhone);
            if (member != null)
            {
                ViewBag.MemberName   = $"{member.FirstName} {member.LastName}";
                ViewBag.MemberPoints = member.Points ?? 0;
                ViewBag.MemberStamps = member.StampBalance ?? 0;
            }
        }

        ViewBag.TableNumber       = tableNumber;
        ViewBag.SelectedCategory  = string.IsNullOrEmpty(category) ? "All" : category;
        ViewBag.Categories        = categories;
        ViewBag.CartCount         = cart.Sum(i => i.Quantity);
        ViewBag.ActiveOrderId     = activeOrderId;

        return View(menuItems);
    }

    // ============================================================
    // GET /Customer/Detail/5?table=T01
    // หน้ารายละเอียดสินค้า — แสดงรูป, คำอธิบาย, และ Options
    // ============================================================
    public IActionResult Detail(int id, string? table)
    {
        if (!string.IsNullOrEmpty(table))
            HttpContext.Session.SetString("TableNumber", table);

        var item = _db.Menuitems.FirstOrDefault(m => m.MenuItemId == id);
        if (item == null) return NotFound();

        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();

        ViewBag.CartCount = cart.Sum(i => i.Quantity);

        var viewModel = new MenuDetailViewModel
        {
            Item        = item,
            TableNumber = HttpContext.Session.GetString("TableNumber")
        };

        return View(viewModel);
    }

    // ============================================================
    // GET /Customer/OrderQueue
    // หน้าแสดงคิวออเดอร์ทั้งหมดของร้านวันนี้
    // ลูกค้าสามารถตรวจสอบลำดับและสถานะออเดอร์ของตนเองได้
    // ============================================================
    public IActionResult OrderQueue()
    {
        var today    = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var tableNumber = HttpContext.Session.GetString("TableNumber");

        // ออเดอร์ที่มีคิวแล้ววันนี้ (StatusId: 2=Paid, 3=Preparing, 4=Ready, 5=Completed)
        var orders = _db.Orders
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow
                     && o.QueueNumber != null
                     && o.OrderStatusId >= 2 && o.OrderStatusId <= 5)
            .OrderBy(o => o.QueueNumber)
            .ToList();

        var tableDict  = _db.Tables.ToDictionary(t => t.TableId, t => t.TableNumber ?? "?");
        var statusDict = _db.Orderstatuses.ToDictionary(s => s.OrderStatusId, s => s.StatusName ?? "?");

        var queueRows = orders.Select(o => new
        {
            o.OrderId,
            o.QueueNumber,
            o.OrderStatusId,
            StatusName  = statusDict.GetValueOrDefault(o.OrderStatusId ?? 0, "?"),
            TableNumber = o.TableId.HasValue ? tableDict.GetValueOrDefault(o.TableId.Value, "?") : "-",
            o.CreatedAt,
            IsMyTable   = o.TableId.HasValue
                          && tableDict.GetValueOrDefault(o.TableId.Value, "") == tableNumber
        }).ToList<dynamic>();

        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();
        ViewBag.CartCount   = cart.Sum(i => i.Quantity);
        ViewBag.TableNumber = tableNumber;
        ViewBag.QueueRows   = queueRows;

        return View();
    }

    // ============================================================
    // POST /Customer/AddToCart
    // เพิ่มสินค้าลงตะกร้า Session
    // ============================================================
    [HttpPost]
    public IActionResult AddToCart(int menuItemId, int quantity, string? sweetness, string? milkType, string? size, decimal optionPriceAdjustment)
    {
        var item = _db.Menuitems.FirstOrDefault(m => m.MenuItemId == menuItemId);
        if (item == null) return NotFound();

        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();

        cart.Add(new CartItem
        {
            MenuItemId              = item.MenuItemId,
            MenuName                = item.MenuName ?? "",
            UnitPrice               = item.Price ?? 0,
            Quantity                = quantity <= 0 ? 1 : quantity,
            ImageUrl                = item.ImageUrl,
            Sweetness               = sweetness,
            MilkType                = milkType,
            Size                    = size,
            OptionPriceAdjustment   = optionPriceAdjustment
        });

        HttpContext.Session.SetJson(CartSessionKey, cart);

        TempData["Success"] = $"เพิ่ม {item.MenuName} ลงตะกร้าแล้ว";

        return RedirectToAction("Menu");
    }

    // ============================================================
    // POST /Customer/RemoveFromCart
    // ลบรายการออกจากตะกร้าตาม Index
    // ============================================================
    [HttpPost]
    public IActionResult RemoveFromCart(int index)
    {
        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();
        if (index >= 0 && index < cart.Count)
            cart.RemoveAt(index);

        HttpContext.Session.SetJson(CartSessionKey, cart);
        return RedirectToAction("Cart");
    }

    // ============================================================
    // POST /Customer/UpdateCartQty
    // อัปเดตจำนวนสินค้าในตะกร้า
    // ============================================================
    [HttpPost]
    public IActionResult UpdateCartQty(int index, int quantity)
    {
        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();
        if (index >= 0 && index < cart.Count)
        {
            if (quantity <= 0)
                cart.RemoveAt(index);
            else
                cart[index].Quantity = quantity;
        }

        HttpContext.Session.SetJson(CartSessionKey, cart);
        return RedirectToAction("Cart");
    }

    // ============================================================
    // GET /Customer/Cart
    // หน้าตะกร้าสินค้า
    // ============================================================
    public IActionResult Cart()
    {
        var cart       = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();
        var tableNumber = HttpContext.Session.GetString("TableNumber");

        var viewModel = new CartViewModel
        {
            Items       = cart,
            TableNumber = tableNumber
        };

        return View(viewModel);
    }

    // ============================================================
    // GET /Customer/Checkout
    // หน้ายืนยันออเดอร์ — กรอกชื่อ (ถ้าเป็น Guest)
    // ============================================================
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();
        if (!cart.Any()) return RedirectToAction("Menu");

        decimal totalAmount = cart.Sum(i => i.ItemTotal);

        // ตรวจสอบโปรโมชั่น MinAmount (Free Cookie) เพื่อแจ้งลูกค้าล่วงหน้า
        var freeItemPromos = _db.Promotions
            .Where(p => p.IsActive == (ulong)1 && p.ConditionType == "MinAmount" && p.RewardType == "FreeItem")
            .ToList();

        var pendingFreeItems = new List<string>();
        foreach (var promo in freeItemPromos)
        {
            if (decimal.TryParse(promo.ConditionValue, out decimal minAmt)
                && totalAmount >= minAmt
                && int.TryParse(promo.RewardValue, out int freeMenuId))
            {
                var freeMenu = _db.Menuitems.FirstOrDefault(m => m.MenuItemId == freeMenuId);
                if (freeMenu != null)
                    pendingFreeItems.Add(freeMenu.MenuName ?? "รายการฟรี");
            }
        }

        ViewBag.TableNumber      = HttpContext.Session.GetString("TableNumber");
        ViewBag.CartItems        = cart;
        ViewBag.TotalAmount      = totalAmount;
        ViewBag.PendingFreeItems = pendingFreeItems;

        return View();
    }

    // ============================================================
    // POST /Customer/PlaceOrder
    // สร้างออเดอร์ใน Database + Reserve Ingredient Stock
    // ============================================================
    [HttpPost]
    public IActionResult PlaceOrder(string? guestName, string? memberPhone)
    {
        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();
        if (!cart.Any()) return RedirectToAction("Menu");

        var tableNumber = HttpContext.Session.GetString("TableNumber");

        // ค้นหาโต๊ะจากเลขโต๊ะ — ถ้าไม่มีในระบบให้สร้างใหม่อัตโนมัติ (Demo Mode)
        Table? table = null;
        if (!string.IsNullOrEmpty(tableNumber))
        {
            table = _db.Tables.FirstOrDefault(t => t.TableNumber == tableNumber);
            if (table == null)
            {
                int newTableId = (_db.Tables.Any() ? _db.Tables.Max(t => t.TableId) : 0) + 1;
                table = new Table { TableId = newTableId, TableNumber = tableNumber };
                _db.Tables.Add(table);
            }
        }

        // ค้นหาสมาชิกถ้าลูกค้ากรอกเบอร์โทร
        Member? member = null;
        if (!string.IsNullOrEmpty(memberPhone))
        {
            member = _db.Members.FirstOrDefault(m => m.Phone == memberPhone);
            // บันทึกเบอร์โทรสมาชิกลง Session เพื่อแสดงข้อมูลบนหน้า Menu
            if (member != null)
                HttpContext.Session.SetString("MemberPhone", member.Phone ?? "");
        }

        // คำนวณยอดรวม
        decimal totalAmount = cart.Sum(i => i.ItemTotal);

        // คำนวณส่วนลดและรายการฟรีจากโปรโมชั่น (ตรวจสอบ ณ เวลา PlaceOrder)
        decimal discountAmount = 0;
        var activePromos = _db.Promotions.Where(p => p.IsActive == (ulong)1).ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);

        // รวบรวมรายการเมนูฟรีที่ต้องเพิ่มเข้า Order (จากโปรโมชั่น FreeItem)
        var freeMenuItemsToAdd = new List<int>();

        foreach (var promo in activePromos)
        {
            // โปรโมชั่น MinAmount + RewardType = FreeItem — เพิ่มรายการฟรีลง Order
            if (promo.ConditionType == "MinAmount"
                && promo.RewardType == "FreeItem"
                && decimal.TryParse(promo.ConditionValue, out decimal minAmt)
                && totalAmount >= minAmt
                && int.TryParse(promo.RewardValue, out int freeMenuId))
            {
                freeMenuItemsToAdd.Add(freeMenuId);
            }

            // โปรโมชั่น MinAmount + RewardType = Discount — ลดราคาตามปกติ
            if (promo.ConditionType == "MinAmount"
                && promo.RewardType == "Discount"
                && decimal.TryParse(promo.ConditionValue, out decimal minAmt2)
                && totalAmount >= minAmt2
                && decimal.TryParse(promo.RewardValue, out decimal discAmt))
            {
                discountAmount += discAmt;
            }

            // โปรโมชั่น Birthday — วันเกิดตรงกับวันนี้
            if (promo.ConditionType == "Birthday"
                && member != null
                && member.BirthDate.HasValue
                && member.BirthDate.Value.Month == today.Month
                && member.BirthDate.Value.Day   == today.Day
                && decimal.TryParse(promo.RewardValue, out decimal birthdayDiscount))
            {
                discountAmount += birthdayDiscount;
            }
        }

        // ยอดสุทธิหลังหักส่วนลด (ไม่ให้ติดลบ)
        decimal netAmount = Math.Max(0, totalAmount - discountAmount);

        // Generate OrderId (ไม่ใช้ AUTO_INCREMENT)
        int newOrderId = (_db.Orders.Any() ? _db.Orders.Max(o => o.OrderId) : 0) + 1;

        var order = new Order
        {
            OrderId         = newOrderId,
            MemberId        = member?.MemberId,
            TableId         = table?.TableId,
            OrderStatusId   = 1,                                    // Waiting_Payment
            GuestName       = member != null ? $"{member.FirstName} {member.LastName}" : guestName,
            TotalAmount     = totalAmount,
            DiscountAmount  = discountAmount,
            NetAmount       = netAmount,
            ReservedUntil   = DateTime.Now.AddMinutes(5),           // จอง 5 นาที
            CreatedAt       = DateTime.Now
        };

        _db.Orders.Add(order);

        // สร้าง OrderItems
        int nextItemId = (_db.Orderitems.Any() ? _db.Orderitems.Max(i => i.OrderItemId) : 0) + 1;
        int nextOptId  = (_db.Orderitemoptions.Any() ? _db.Orderitemoptions.Max(o => o.OrderItemOptionId) : 0) + 1;

        foreach (var cartItem in cart)
        {
            var orderItem = new Orderitem
            {
                OrderItemId         = nextItemId++,
                OrderId             = newOrderId,
                MenuItemId          = cartItem.MenuItemId,
                OrderItemStatusId   = 1,            // Pending
                Quantity            = cartItem.Quantity,
                UnitPrice           = cartItem.UnitPrice + cartItem.OptionPriceAdjustment
            };
            _db.Orderitems.Add(orderItem);

            // บันทึก Options ของสินค้า
            if (!string.IsNullOrEmpty(cartItem.Sweetness))
            {
                _db.Orderitemoptions.Add(new Orderitemoption
                {
                    OrderItemOptionId   = nextOptId++,
                    OrderItemId         = orderItem.OrderItemId,
                    OptionName          = "ความหวาน",
                    OptionValue         = cartItem.Sweetness,
                    PriceAdjustment     = 0
                });
            }
            if (!string.IsNullOrEmpty(cartItem.MilkType))
            {
                _db.Orderitemoptions.Add(new Orderitemoption
                {
                    OrderItemOptionId   = nextOptId++,
                    OrderItemId         = orderItem.OrderItemId,
                    OptionName          = "ประเภทนม",
                    OptionValue         = cartItem.MilkType,
                    PriceAdjustment     = 0
                });
            }
            if (!string.IsNullOrEmpty(cartItem.Size))
            {
                _db.Orderitemoptions.Add(new Orderitemoption
                {
                    OrderItemOptionId   = nextOptId++,
                    OrderItemId         = orderItem.OrderItemId,
                    OptionName          = "ขนาด",
                    OptionValue         = cartItem.Size,
                    PriceAdjustment     = cartItem.OptionPriceAdjustment
                });
            }
        }

        // เพิ่มรายการเมนูฟรี (จากโปรโมชั่น FreeItem) เป็น OrderItem ราคา 0
        foreach (var freeId in freeMenuItemsToAdd)
        {
            var freeMenuItem = _db.Menuitems.FirstOrDefault(m => m.MenuItemId == freeId);
            if (freeMenuItem == null) continue;

            _db.Orderitems.Add(new Orderitem
            {
                OrderItemId       = nextItemId++,
                OrderId           = newOrderId,
                MenuItemId        = freeId,
                OrderItemStatusId = 1,          // Pending
                Quantity          = 1,
                UnitPrice         = 0           // รายการฟรี — ราคา 0
            });

            // บันทึก Option พิเศษเพื่อระบุว่าเป็นรายการโปรโมชั่น
            _db.Orderitemoptions.Add(new Orderitemoption
            {
                OrderItemOptionId = nextOptId++,
                OrderItemId       = nextItemId - 1,
                OptionName        = "โปรโมชั่น",
                OptionValue       = "ฟรี",
                PriceAdjustment   = 0
            });
        }

        // Reserve Ingredient Stock ตาม Recipe
        foreach (var cartItem in cart)
        {
            var recipes = _db.Recipes
                .Where(r => r.MenuItemId == cartItem.MenuItemId)
                .ToList();

            foreach (var recipe in recipes)
            {
                var ingredient = _db.Ingredients.FirstOrDefault(i => i.IngredientId == recipe.IngredientId);
                if (ingredient != null)
                {
                    float needed = (recipe.QuantityRequired ?? 0) * cartItem.Quantity;
                    ingredient.ReservedQty = (ingredient.ReservedQty ?? 0) + needed;
                }
            }
        }

        _db.SaveChanges();

        // ล้างตะกร้า
        HttpContext.Session.Remove(CartSessionKey);

        return RedirectToAction("Payment", new { orderId = newOrderId });
    }

    // ============================================================
    // GET /Customer/Payment/5
    // หน้าชำระเงิน — แสดง QR และให้อัปโหลดสลิป
    // ============================================================
    public IActionResult Payment(int orderId)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null) return NotFound();

        // ดึงรายการ Items ใน Order
        var items = (from oi in _db.Orderitems
                     join mi in _db.Menuitems on oi.MenuItemId equals mi.MenuItemId
                     where oi.OrderId == orderId
                     select new { oi, mi }).ToList();

        ViewBag.Order   = order;
        ViewBag.Items   = items;

        // ดึง Payment ที่มีอยู่ (ถ้าเคยอัปโหลดแล้ว)
        var existingPayment = _db.Payments.FirstOrDefault(p => p.OrderId == orderId);
        ViewBag.ExistingPayment = existingPayment;

        // ดึงข้อมูลสมาชิกเพื่อแสดง UI แลกแต้ม/Stamp บนหน้าชำระเงิน
        if (order.MemberId.HasValue)
        {
            var member = _db.Members.Find(order.MemberId.Value);
            if (member != null)
            {
                ViewBag.MemberId     = member.MemberId;
                ViewBag.MemberName   = $"{member.FirstName} {member.LastName}".Trim();
                ViewBag.MemberPoints = member.Points ?? 0;
                ViewBag.MemberStamps = member.StampBalance ?? 0;
            }
        }

        return View();
    }

    // ============================================================
    // POST /Customer/RedeemPoints?orderId=5&points=10
    // ลูกค้าแลกแต้มเพื่อลดราคาออเดอร์ของตัวเอง (1 แต้ม = 1 บาท)
    // ตรวจสิทธิ์ผ่าน MemberPhone ใน Session
    // ============================================================
    [HttpPost]
    public IActionResult RedeemPoints(int orderId, int points)
    {
        var memberPhone = HttpContext.Session.GetString("MemberPhone");
        if (string.IsNullOrEmpty(memberPhone))
            return Json(new { ok = false, message = "กรุณาเข้าสู่ระบบสมาชิกก่อน" });

        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null || order.OrderStatusId != 1)
            return Json(new { ok = false, message = "ไม่พบออเดอร์หรือสถานะไม่ถูกต้อง" });

        var member = _db.Members.FirstOrDefault(m => m.Phone == memberPhone);
        if (member == null || member.MemberId != order.MemberId)
            return Json(new { ok = false, message = "ไม่มีสิทธิ์แลกแต้มของออเดอร์นี้" });

        if (points <= 0 || points > (member.Points ?? 0))
            return Json(new { ok = false, message = $"แต้มไม่พอ (มี {member.Points ?? 0} แต้ม)" });

        decimal discount     = points;
        order.DiscountAmount = (order.DiscountAmount ?? 0) + discount;
        order.NetAmount      = Math.Max(0, (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0));
        member.Points        = (member.Points ?? 0) - points;

        int nextTransId = (_db.Pointtransactions.Any() ? _db.Pointtransactions.Max(t => t.TransId) : 0) + 1;
        _db.Pointtransactions.Add(new Pointtransaction
        {
            TransId    = nextTransId,
            MemberId   = member.MemberId,
            TypeId     = 2,             // Redeem
            Amount     = points,
            RefOrderId = orderId,
            CreatedBy  = null,          // ลูกค้าทำเอง ไม่มี StaffId
            CreatedAt  = DateTime.Now
        });

        _db.SaveChanges();

        return Json(new { ok = true, newNetAmount = order.NetAmount, remainPoints = member.Points });
    }

    // ============================================================
    // POST /Customer/RedeemStamp?orderId=5
    // ลูกค้าใช้ 10 Stamp แลกเครื่องดื่มฟรี 1 แก้ว
    // ตรวจสิทธิ์ผ่าน MemberPhone ใน Session
    // ============================================================
    [HttpPost]
    public IActionResult RedeemStamp(int orderId)
    {
        var memberPhone = HttpContext.Session.GetString("MemberPhone");
        if (string.IsNullOrEmpty(memberPhone))
            return Json(new { ok = false, message = "กรุณาเข้าสู่ระบบสมาชิกก่อน" });

        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null || order.OrderStatusId != 1)
            return Json(new { ok = false, message = "ไม่พบออเดอร์หรือสถานะไม่ถูกต้อง" });

        var member = _db.Members.FirstOrDefault(m => m.Phone == memberPhone);
        if (member == null || member.MemberId != order.MemberId)
            return Json(new { ok = false, message = "ไม่มีสิทธิ์ใช้ Stamp ของออเดอร์นี้" });

        if ((member.StampBalance ?? 0) < 10)
            return Json(new { ok = false, message = $"Stamp ไม่พอ (มี {member.StampBalance ?? 0}/10)" });

        // มูลค่า Free Drink = ราคาต่ำสุดของ OrderItem ในออเดอร์นี้
        var orderItems = _db.Orderitems.Where(i => i.OrderId == orderId).ToList();
        decimal freePrice = orderItems.Any() ? orderItems.Min(i => i.UnitPrice ?? 0) : 0;

        member.StampBalance  = (member.StampBalance ?? 0) - 10;
        order.DiscountAmount = (order.DiscountAmount ?? 0) + freePrice;
        order.NetAmount      = Math.Max(0, (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0));

        int nextTransId = (_db.Pointtransactions.Any() ? _db.Pointtransactions.Max(t => t.TransId) : 0) + 1;
        _db.Pointtransactions.Add(new Pointtransaction
        {
            TransId    = nextTransId,
            MemberId   = member.MemberId,
            TypeId     = 4,             // StampRedeem
            Amount     = 10,
            RefOrderId = orderId,
            CreatedBy  = null,
            CreatedAt  = DateTime.Now
        });

        _db.SaveChanges();

        return Json(new { ok = true, newNetAmount = order.NetAmount, remainStamps = member.StampBalance, freePrice });
    }

    // ============================================================
    // POST /Customer/UploadSlip/5
    // ลูกค้าอัปโหลดสลิปโอนเงิน
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> UploadSlip(int orderId, IFormFile slip)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null) return NotFound();

        if (slip == null || slip.Length == 0)
        {
            TempData["Error"] = "กรุณาเลือกไฟล์สลิปก่อนอัปโหลด";
            return RedirectToAction("Payment", new { orderId });
        }

        // บันทึกไฟล์สลิปลงในโฟลเดอร์ wwwroot/uploads/slips/
        string uploadPath = Path.Combine(_env.WebRootPath, "uploads", "slips");
        Directory.CreateDirectory(uploadPath);

        string ext      = Path.GetExtension(slip.FileName);
        string fileName = $"order{orderId}_{Guid.NewGuid():N}{ext}";
        string filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await slip.CopyToAsync(stream);
        }

        string slipUrl = $"/uploads/slips/{fileName}";

        // ลบ Payment เดิม (ถ้ามี) แล้วสร้างใหม่
        var existing = _db.Payments.Where(p => p.OrderId == orderId).ToList();
        _db.Payments.RemoveRange(existing);

        int newPaymentId = (_db.Payments.Any() ? _db.Payments.Max(p => p.PaymentId) : 0) + 1;

        _db.Payments.Add(new Payment
        {
            PaymentId       = newPaymentId,
            OrderId         = orderId,
            PaymentMethodId = 1,            // Slip_Upload
            PaymentStatusId = 1,            // Pending — รอพนักงาน Verify
            Amount          = order.NetAmount ?? 0,
            SlipUrl         = slipUrl
        });

        _db.SaveChanges();

        // แจ้งเตือน POS ผ่าน SignalR ว่ามีสลิปใหม่รอ Verify
        await _hub.Clients.Group("staff").SendAsync("NotifyNewSlip", new
        {
            orderId,
            amount    = order.NetAmount,
            slipUrl
        });

        TempData["Success"] = "อัปโหลดสลิปสำเร็จ กรุณารอพนักงานตรวจสอบ";
        return RedirectToAction("Tracking", new { orderId });
    }

    // ============================================================
    // GET /Customer/MemberLookup?phone=0891234567
    // AJAX — คืนข้อมูลสมาชิกเป็น JSON สำหรับหน้า Checkout
    // ============================================================
    [HttpGet]
    public IActionResult MemberLookup(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Json(new { found = false });

        var member = _db.Members.FirstOrDefault(m => m.Phone == phone);
        if (member == null)
            return Json(new { found = false });

        return Json(new
        {
            found     = true,
            fullName  = $"{member.FirstName} {member.LastName}",
            points    = member.Points ?? 0,
            stamps    = member.StampBalance ?? 0
        });
    }

    // ============================================================
    // POST /Customer/CancelOrder/5
    // ลูกค้ายกเลิกออเดอร์ — คืน ReservedQty และเปลี่ยนสถานะเป็น Cancelled
    // ใช้ได้เฉพาะตอน OrderStatusId = 1 (ยังไม่ชำระ) หรือสลิปถูก Reject
    // ============================================================
    [HttpPost]
    public IActionResult CancelOrder(int orderId)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null) return NotFound();

        // อนุญาตให้ยกเลิกได้เฉพาะสถานะ Waiting_Payment (1)
        // หรือมีสลิปที่ถูก Reject
        bool canCancel = order.OrderStatusId == 1;
        if (!canCancel)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.OrderId == orderId);
            canCancel = payment?.PaymentStatusId == 3; // Rejected
        }

        if (!canCancel)
        {
            TempData["Error"] = "ไม่สามารถยกเลิกออเดอร์นี้ได้";
            return RedirectToAction("Tracking", new { orderId });
        }

        // คืน ReservedQty ของวัตถุดิบทั้งหมดใน Order
        var orderItems = _db.Orderitems.Where(i => i.OrderId == orderId).ToList();
        foreach (var oi in orderItems)
        {
            var recipes = _db.Recipes.Where(r => r.MenuItemId == oi.MenuItemId).ToList();
            foreach (var recipe in recipes)
            {
                var ingredient = _db.Ingredients.FirstOrDefault(i => i.IngredientId == recipe.IngredientId);
                if (ingredient != null)
                {
                    float toReturn = (recipe.QuantityRequired ?? 0) * (oi.Quantity ?? 1);
                    ingredient.ReservedQty = Math.Max(0, (ingredient.ReservedQty ?? 0) - toReturn);
                }
            }
        }

        order.OrderStatusId = 6; // Cancelled
        _db.SaveChanges();

        TempData["Success"] = "ยกเลิกออเดอร์สำเร็จ";
        return RedirectToAction("Menu");
    }

    // ============================================================
    // GET /Customer/Rewards
    // หน้าแสดงรายการรางวัลที่แลกได้ด้วยแต้ม
    // ============================================================
    public IActionResult Rewards()
    {
        var rewards = _db.Rewards
            .Where(r => r.IsActive == (ulong)1)
            .OrderBy(r => r.PointsRequired)
            .ToList();

        var cart = HttpContext.Session.GetJson<List<CartItem>>(CartSessionKey) ?? new();
        ViewBag.CartCount = cart.Sum(i => i.Quantity);

        return View(rewards);
    }

    // ============================================================
    // GET /Customer/MyOrders
    // หน้าคิวออเดอร์ปัจจุบันและประวัติการสั่งซื้อของลูกค้า
    // - สมาชิก: ดูจาก MemberPhone ใน Session
    // - Guest: ดูจาก TableNumber ใน Session (เฉพาะวันนี้)
    // ============================================================
    public IActionResult MyOrders()
    {
        var memberPhone = HttpContext.Session.GetString("MemberPhone");
        var tableNumber = HttpContext.Session.GetString("TableNumber");

        var activeOrders = new List<Order>();

        if (!string.IsNullOrEmpty(memberPhone))
        {
            // --- สมาชิก: ดูออเดอร์ทั้งหมดของตนเอง ---
            var member = _db.Members.FirstOrDefault(m => m.Phone == memberPhone);
            if (member != null)
            {
                activeOrders = _db.Orders
                    .Where(o => o.MemberId == member.MemberId
                             && o.OrderStatusId >= 1
                             && o.OrderStatusId <= 4)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                ViewBag.MemberName   = $"{member.FirstName} {member.LastName}";
                ViewBag.MemberPoints = member.Points ?? 0;
                ViewBag.MemberStamps = member.StampBalance ?? 0;
            }
        }
        else if (!string.IsNullOrEmpty(tableNumber))
        {
            // --- Guest: ดูออเดอร์ของโต๊ะเฉพาะวันนี้ ---
            var tableEntity = _db.Tables.FirstOrDefault(t => t.TableNumber == tableNumber);
            if (tableEntity != null)
            {
                activeOrders = _db.Orders
                    .Where(o => o.TableId == tableEntity.TableId
                             && o.OrderStatusId >= 1
                             && o.OrderStatusId <= 4)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();
            }
        }

        // ดึง OrderItems พร้อมชื่อเมนูสำหรับ Active Orders
        var allOrderIds = activeOrders.Select(o => o.OrderId).ToList();

        var orderItems = (from oi in _db.Orderitems
                          join mi in _db.Menuitems on oi.MenuItemId equals (int?)mi.MenuItemId
                          where oi.OrderId.HasValue && allOrderIds.Contains(oi.OrderId.Value)
                          select new
                          {
                              oi.OrderId,
                              mi.MenuName,
                              oi.Quantity,
                              oi.UnitPrice
                          }).ToList();

        var statusLabels = new Dictionary<int, string>
        {
            { 1, "รอตรวจสอบสลิป" },
            { 2, "ยืนยันแล้ว — รอคิวทำ" },
            { 3, "กำลังเตรียม" },
            { 4, "พร้อมรับ!" },
            { 5, "เสร็จสิ้น" },
            { 6, "ยกเลิก" }
        };

        ViewBag.ActiveOrders = activeOrders;
        ViewBag.OrderItems   = orderItems;
        ViewBag.StatusLabels  = statusLabels;
        ViewBag.TableNumber   = tableNumber;
        ViewBag.CartCount     = 0;

        return View();
    }

    // ============================================================
    // GET /Customer/TrackByTable?table=T01
    // ค้นหาออเดอร์ล่าสุดของโต๊ะ (Status 1-4) แล้ว Redirect ไปหน้าติดตาม
    // ============================================================
    public IActionResult TrackByTable(string? table)
    {
        var tableNumber = table ?? HttpContext.Session.GetString("TableNumber");
        if (string.IsNullOrEmpty(tableNumber))
            return RedirectToAction("Menu");

        // บันทึกเลขโต๊ะลง Session
        HttpContext.Session.SetString("TableNumber", tableNumber);

        // ค้นหาโต๊ะ
        var tableEntity = _db.Tables.FirstOrDefault(t => t.TableNumber == tableNumber);
        if (tableEntity == null)
            return RedirectToAction("Menu");

        // หาออเดอร์ล่าสุดของโต๊ะที่ยังไม่เสร็จ (Status 1-4)
        var activeOrder = _db.Orders
            .Where(o => o.TableId == tableEntity.TableId
                     && o.OrderStatusId >= 1
                     && o.OrderStatusId <= 4)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();

        if (activeOrder == null)
        {
            TempData["Error"] = "ไม่พบออเดอร์ที่กำลังดำเนินการสำหรับโต๊ะนี้";
            return RedirectToAction("Menu");
        }

        return RedirectToAction("Tracking", new { orderId = activeOrder.OrderId });
    }

    // ============================================================
    // GET /Customer/Tracking?orderId=5
    // หน้าติดตามสถานะออเดอร์
    // ============================================================
    public IActionResult Tracking(int orderId)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null) return NotFound();

        var payment = _db.Payments.FirstOrDefault(p => p.OrderId == orderId);

        // ดึงรายการ Items พร้อมชื่อเมนู
        var items = (from oi in _db.Orderitems
                     join mi in _db.Menuitems on oi.MenuItemId equals mi.MenuItemId
                     where oi.OrderId == orderId
                     select new
                     {
                         MenuName = mi.MenuName,
                         Quantity = oi.Quantity,
                         UnitPrice = oi.UnitPrice
                     }).ToList();

        // สถานะของ Order Status ภาษาไทย
        var statusLabels = new Dictionary<int, string>
        {
            { 1, "รอชำระเงิน" },
            { 2, "ชำระแล้ว — รอเตรียม" },
            { 3, "กำลังเตรียม" },
            { 4, "พร้อมรับ" },
            { 5, "เสร็จสิ้น" },
            { 6, "ยกเลิก" }
        };

        ViewBag.Order           = order;
        ViewBag.Payment         = payment;
        ViewBag.Items           = items;
        ViewBag.StatusLabels    = statusLabels;
        ViewBag.StatusLabel     = statusLabels.GetValueOrDefault(order.OrderStatusId ?? 0, "ไม่ทราบสถานะ");
        // บอก View ว่าลูกค้าคนนี้ Login เป็น Member อยู่หรือเปล่า
        ViewBag.IsMember        = !string.IsNullOrEmpty(HttpContext.Session.GetString("MemberPhone"));

        return View();
    }

    // ============================================================
    // POST /Customer/MemberLogout
    // ล้างข้อมูล Member ออกจาก Session — คง TableNumber ไว้เพื่อให้สั่งใหม่ได้
    // ============================================================
    [HttpPost]
    public IActionResult MemberLogout()
    {
        HttpContext.Session.Remove("MemberPhone");
        HttpContext.Session.Remove(CartSessionKey);
        return RedirectToAction("Menu");
    }
}
