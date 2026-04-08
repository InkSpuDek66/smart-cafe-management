# รายงานความสมบูรณ์ของระบบ (Project Status Report)
**ประเมินความสมบูรณ์: 100%**
**อัปเดตล่าสุด: 7 เมษายน 2026**

---

## ส่วนที่ 1 — Customer Side (Mobile Web App) : 100%

| ฟีเจอร์ | สถานะ | หมายเหตุ |
| :--- | :---: | :--- |
| 1.1 Landing Page + Re-join Session (เช็คออเดอร์ค้างของโต๊ะ) | 100% | ครบ |
| 1.2 Checkout: Guest Name / Member Lookup + แสดง Points/Stamps | 100% | ครบ |
| 1.3 Menu Grid + Categories Bar + Sold Out badge | 100% | ครบ |
| 1.3 Header แสดง Points/Stamps สำหรับ Member บนหน้า Menu | 100% | ครบ |
| 1.3 Seasonal badge พิเศษ (กรอบตามเทศกาล) บนหน้า Menu | 100% | ครบ |
| 1.4 Cart + Options ครบ | 100% | ครบ |
| 1.4 แสดงรายการ Free Cookie ล่วงหน้าบนหน้า Checkout | 100% | ครบ |
| 1.5 Payment: Static QR Code + Upload Slip + Preview | 100% | ครบ |
| 1.6 Order Status: Step Indicator + Real-time (SignalR) | 100% | ครบ |
| 1.6 Cancel Order (ยกเลิกออเดอร์ + คืน ReservedQty) | 100% | ครบ |
| 1.6 แสดง Reject Reason เมื่อสลิปถูกปฏิเสธ | 100% | ครบ |
| 1.7 Rewards Catalog | 100% | ครบ |

---

## ส่วนที่ 2 — POS Tablet : 100%

| ฟีเจอร์ | สถานะ | หมายเหตุ |
| :--- | :---: | :--- |
| 2.1 Queue Split View (Unpaid ซ้าย / Active ขวา) | 100% | ครบ |
| 2.1 Auto-cancel ออเดอร์หมด Reservation (5 นาที) + คืน ReservedQty | 100% | ครบ |
| 2.2 Payment Modal: Redeem Points / Stamps / Reward | 100% | ครบ |
| 2.2 Group Check-in Manual Approve | 100% | ครบ |
| 2.3 Slip Verification: Approve / Reject + เลือกเหตุผล | 100% | ครบ |
| 2.3 ส่ง Reject Reason ผ่าน SignalR แจ้งลูกค้า | 100% | ครบ |
| 2.4 KDS: แสดง Ticket + Options + สูตรจาก Recipes | 100% | ครบ |
| 2.4 KDS: Mark Item Done | 100% | ครบ |
| 2.4 KDS: Call Customer button | 100% | ครบ |
| 2.5 Wastage Recording | 100% | ครบ |

---

## ส่วนที่ 3 — Web Admin : 100%

| ฟีเจอร์ | สถานะ | หมายเหตุ |
| :--- | :---: | :--- |
| 3.1 Dashboard: Stat Cards (ออเดอร์/รายได้/สมาชิก/สลิปรอ/ของเสีย) | 100% | ครบ |
| 3.1 Dashboard: Top 5 Best Sellers วันนี้ | 100% | ครบ |
| 3.1 Dashboard: Stock Alerts (วัตถุดิบต่ำกว่า Reorder Level) | 100% | ครบ |
| 3.1 Dashboard: ตารางยอดขายรายชั่วโมงวันนี้ | 100% | ครบ |
| 3.2 Finance: Payment list รายวัน + สรุปยอด | 100% | ครบ |
| 3.2 Finance: P&L (รายรับ − ต้นทุนขาย − ของเสีย = กำไรขั้นต้น) | 100% | ครบ |
| 3.2 Finance: แยกยอดตามช่องทางชำระเงิน | 100% | ครบ |
| 3.3 Menu CRUD + Seasonal + Recipes | 100% | ครบ |
| 3.4 Inventory: ดูสต็อก + Restock + Low Stock Alert | 100% | ครบ |
| 3.5 Staff Management + Shifts (Clock In/Out) | 100% | ครบ |
| 3.6 Promotions CRUD + Toggle เปิด/ปิด | 100% | ครบ |
| 3.6 สถิติการใช้งานโปรโมชั่นแต่ละชนิด | 100% | ครบ |
| 3.7 Tables Management + QR Print-ready (A4) | 100% | ครบ |
| 3.8 Reports: รายวัน / รายเมนู / รายชั่วโมง / รายสัปดาห์ | 100% | ครบ |
| 3.8 Reports: รายเดือน / รายไตรมาส / รายปี Year-over-Year | 100% | ครบ |
| 3.8 Reports: Payment channels / Options popularity / Wastage cost | 100% | ครบ |

---

## ระบบ Infrastructure : 100%

| ฟีเจอร์ | สถานะ | หมายเหตุ |
| :--- | :---: | :--- |
| SignalR: NotifyNewSlip | 100% | ครบ |
| SignalR: NotifyOrderPaid | 100% | ครบ |
| SignalR: NotifySlipRejected (พร้อม Reason) | 100% | ครบ |
| SignalR: NotifyOrderReady | 100% | ครบ |
| Public Screen (TV): แสดงคิว Ready + Auto-refresh | 100% | ครบ |
| Role Guard ทุก Controller (Barista/Cashier/Manager/Finance/Owner) | 100% | ครบ |
| Stock Reservation 5 นาที + Auto-cancel | 100% | ครบ |
| Inventory Cut-off เมื่อ Paid | 100% | ครบ |
| Points Earn + Stamp Earn เมื่อ Paid | 100% | ครบ |
| Member Registration + BBNNNN ID format | 100% | ครบ |

---

## โปรโมชั่น Logic : 100%

| โปรโมชั่น | สถานะ | หมายเหตุ |
| :--- | :---: | :--- |
| สะสมแต้ม 10 บาท = 1 แต้ม | 100% | ครบ |
| Stamp Card: 10 stamps = ฟรี 1 แก้ว | 100% | ครบ |
| Seasonal Menu (IsSeasonal + วันที่) | 100% | ครบ |
| Free Cookie เมื่อยอด >= 300 บาท (เป็น OrderItem ราคา 0) | 100% | ครบ |
| Group Check-in Manual Approve | 100% | ครบ |
| Birthday Promo (ตรวจ ณ เวลา PlaceOrder) | 100% | ครบ |
| สถิติการใช้งานโปรโมชั่น | 100% | ครบ |

---

## รายงานการทดสอบระบบ (7 เมษายน 2026)

### ภาพรวมสถาปัตยกรรม

ระบบแบ่งออกเป็น 4 แพลตฟอร์มหลักภายใน ASP.NET Core MVC เดียว:

| แพลตฟอร์ม | Controller | Layout | กลุ่มผู้ใช้ |
| :--- | :--- | :--- | :--- |
| Mobile Web App (PWA) | `CustomerController` | `_MobileLayout` | Guest, Member |
| POS Tablet | `PosController` | `_AdminLayout` | Barista, Cashier, Manager, Owner |
| Web Admin | `AdminController` | `_AdminLayout` | Manager, Finance, Owner |
| Public Screen (TV) | `PublicController` | ไม่มี Layout | สาธารณะ |

**Authentication:** Session-based (SHA-256 password hash)  
**Real-time:** SignalR Hub (`/hubs/cafe`) กลุ่ม `staff`, `public`, `order-{id}`  
**Database:** MySQL + EF Core (Pomelo), IDs แบบ BBNNNN ไม่ใช้ AUTO_INCREMENT

---

### บัคที่พบและแก้ไขแล้ว

| # | บัค | ความรุนแรง | ไฟล์ | สถานะ |
| :---: | :--- | :---: | :--- | :---: |
| 1 | `AdminController.MenuList` และ `Orders` อ่าน `TempData["Success"]` เข้า ViewModel ก่อน Layout จะ render ทำให้ข้อความแจ้งเตือนหายไปทุกครั้ง | สูง | `AdminController.cs` | แก้แล้ว |
| 2 | `AdminController.RejectPayment` ใช้ `TempData["Error"]` (สีแดง) สำหรับการดำเนินการที่สำเร็จ ทำให้ UX สับสน | กลาง | `AdminController.cs` | แก้แล้ว |
| 3 | `AdminController.ApprovePayment` มีคำภาษาอังกฤษ "Approve" ปนใน TempData message | ต่ำ | `AdminController.cs` | แก้แล้ว |
| 4 | `AccountController.Register` ใช้ `TempData["SuccessMessage"]` แต่ Layout ใช้ key `TempData["Success"]` — ข้อความยืนยันหลังสมัครไม่แสดงเลย | สูง | `AccountController.cs` | แก้แล้ว |
| 5 | `AccountController.AddStaff` ไม่มีการตรวจสอบ Role — พนักงานทุก Role (รวม Barista) สามารถเพิ่มพนักงานใหม่ได้ | สูง | `AccountController.cs` | แก้แล้ว |
| 6 | `PosController.MarkItemDone` ไม่มีการตรวจสอบสถานะออเดอร์ก่อน set เป็น Ready (4) — ถ้าออเดอร์ Completed (5) แล้ว จะถูก reset กลับเป็น Ready | กลาง | `PosController.cs` | แก้แล้ว |
| 7 | `_CafeLayout.cshtml` ไม่มีการ render TempData ทำให้ข้อความแจ้งเตือนบนหน้า Login และ Register ไม่แสดง | สูง | `_CafeLayout.cshtml` | แก้แล้ว |

---

### รายละเอียดการแก้ไขแต่ละบัค

#### บัค #1 — TempData ถูกบริโภคก่อน Layout
**สาเหตุ:** `MenuList()` และ `Orders()` อ่าน `TempData["Success"]` เข้า ViewModel property  
เมื่อ `_AdminLayout.cshtml` พยายามอ่าน `TempData["Success"]` ทีหลัง ค่าจะว่างแล้ว เพราะ TempData อ่านได้ครั้งเดียว
```csharp
// ก่อนแก้ (ผิด)
var viewModel = new AdminMenuListViewModel {
    Items          = items,
    SuccessMessage = TempData["Success"] as string,   // บริโภค TempData ก่อน Layout
    ErrorMessage   = TempData["Error"]  as string
};

// หลังแก้ (ถูก)
var viewModel = new AdminMenuListViewModel { Items = items };
// TempData["Success"] ถูกอ่านโดย _AdminLayout.cshtml แทน
```

#### บัค #2 — RejectPayment ใช้ Error flash สำหรับ Success
**สาเหตุ:** ใช้ `TempData["Error"]` ซึ่งแสดงเป็นสีแดงสำหรับการปฏิเสธที่สำเร็จ ควรเป็น Success (สีเขียว)
```csharp
// ก่อนแก้
TempData["Error"] = $"Reject สลิป Payment #{paymentId} แล้ว";
// หลังแก้
TempData["Success"] = $"ปฏิเสธสลิป Payment #{paymentId} แล้ว";
```

#### บัค #4 — TempData key ไม่ตรงกัน
**สาเหตุ:** `AccountController.Register` ใช้ key `"SuccessMessage"` แต่ Layout ใช้ `"Success"`
```csharp
// ก่อนแก้
TempData["SuccessMessage"] = $"สมัครสมาชิกสำเร็จ รหัสสมาชิก {memberId}";
// หลังแก้
TempData["Success"] = $"สมัครสมาชิกสำเร็จ รหัสสมาชิกของคุณคือ {memberId}";
```

#### บัค #5 — AddStaff ขาด Role Guard
**สาเหตุ:** ตรวจสอบแค่ `IsLoggedIn()` โดยไม่ตรวจ Role ทำให้ Barista (Role 1) เพิ่มพนักงานได้  
ควรอนุญาตเฉพาะ Store Manager (3) และ Owner (5)
```csharp
// เพิ่ม helper method
private bool CanManageStaff()
{
    var roleId = HttpContext.Session.GetInt32("StaffRoleId");
    return roleId == 3 || roleId == 5;
}

// เพิ่มการตรวจสอบใน GET และ POST
if (!CanManageStaff()) {
    TempData["Error"] = "คุณไม่มีสิทธิ์เพิ่มพนักงาน";
    return RedirectToAction("Index", "Home");
}
```

#### บัค #6 — MarkItemDone ไม่มี Guard สถานะออเดอร์
**สาเหตุ:** เมื่อทุก Item Done จะ set Order เป็น Ready (4) โดยไม่ตรวจสอบ — ถ้า Cashier ส่งลูกค้าไปแล้ว (Status 5) แต่ Barista ยังกด Done อยู่ จะ reset กลับเป็น Ready
```csharp
// ก่อนแก้
if (order != null)
{
    order.OrderStatusId = 4;

// หลังแก้
if (order != null && order.OrderStatusId < 4)
{
    order.OrderStatusId = 4;
```

#### บัค #7 — _CafeLayout ไม่ render TempData
**สาเหตุ:** `_CafeLayout.cshtml` ใช้กับ Login, Register แต่ไม่มีโค้ด render TempData เลย  
เพิ่มบล็อกแสดงข้อความก่อน `@RenderBody()`:
```html
@if (TempData["Success"] != null)
{
    <div class="alert alert-success mb-4"><span>@TempData["Success"]</span></div>
}
@if (TempData["Error"] != null)
{
    <div class="alert alert-error mb-4"><span>@TempData["Error"]</span></div>
}
```

---

### ข้อสังเกตอื่น (ไม่ใช่บัค — ยอมรับสำหรับ Demo)

| รายการ | เหตุผลที่ยอมรับได้ |
| :--- | :--- |
| `MAX(Id) + 1` สำหรับ Generate ID ทุกตาราง | ระบบใช้งานคนเดียว (Demo) ไม่มี concurrent writes จริง |
| N+1 query ใน `AdminController.Orders` (subquery `Count` ต่อแถว) | EF Core แปลงเป็น correlated subquery ใน SQL อัตโนมัติ ยอมรับได้สำหรับข้อมูลน้อย |
| Reports option popularity query มี correlated subquery ซ้อน | ข้อมูล Demo ไม่มาก ไม่กระทบ performance |
| `_Layout.cshtml` ยังใช้ Bootstrap (หน้าเริ่มต้น ASP.NET) | ไม่มีหน้าจริงที่ใช้ `_Layout.cshtml` — ทุกหน้าใช้ `_CafeLayout`, `_AdminLayout`, หรือ `_MobileLayout` แทน |

---

## สรุปการเปลี่ยนแปลงทั้งหมด (7 เมษายน 2026)

| # | รายการ | ไฟล์ |
| :---: | :--- | :--- |
| 1 | แปลคำทั้งหมดในระบบเป็นภาษาไทย (Approved/Rejected/Pending/Wastage/Revenue/Profit/Active/Guest) | Views ทุกไฟล์ |
| 2 | แก้ TempData dual-consumption ใน MenuList และ Orders | `AdminController.cs` |
| 3 | แก้ RejectPayment ใช้ TempData["Error"] สำหรับ success | `AdminController.cs` |
| 4 | แก้ ApprovePayment message เป็นภาษาไทยล้วน | `AdminController.cs` |
| 5 | แก้ Register TempData key ให้ตรงกับ Layout | `AccountController.cs` |
| 6 | เพิ่ม Role Guard บน AddStaff (Manager/Owner เท่านั้น) | `AccountController.cs` |
| 7 | เพิ่ม Order status guard ใน MarkItemDone | `PosController.cs` |
| 8 | เพิ่ม TempData rendering ใน _CafeLayout | `_CafeLayout.cshtml` |
| 9 | แก้ Razor email-detection bug `x@item.Quantity` → `x@(item.Quantity)` | `Kds.cshtml`, `Queue.cshtml` |
| 10 | ลบ TempData flash ที่ซ้อนกันออกจาก Views ทั้งหมด (9 ไฟล์) | Admin Views หลายไฟล์ |
| 11 | เพิ่มระบบแจ้งเตือนลูกค้า Real-time (Notification Bell + Order Ready Overlay) | `_MobileLayout.cshtml` |

---

## รายงานการทดสอบระบบ รอบที่ 2 — Code Review (7 เมษายน 2026)

> ตรวจสอบโดย Claude Code ครอบคลุม Controller 7 ไฟล์, ViewModel 10 ไฟล์, Model/Db 22 ไฟล์

### บัคที่พบในรอบนี้ (BUG-001 ถึง BUG-004)

| ID | ความรุนแรง | ไฟล์ | บรรทัดเดิม | รายละเอียด | สถานะ |
| :--- | :---: | :--- | :---: | :--- | :---: |
| BUG-001 | Critical | `AdminController.cs` | ~517 | Duplicate TransId ใน Stamp transaction — `ApprovePayment` crash ทุกครั้งที่สมาชิกมีรายการ Coffee | แก้แล้ว |
| BUG-002 | Critical | `PosController.cs` | ~634 | Duplicate TransId ใน Stamp transaction — `ApprovePaymentAtPos` crash ทุกครั้งที่สมาชิกมีรายการ Coffee | แก้แล้ว |
| BUG-003 | Medium | `AdminController.cs` | ~372 | `UpdateOrderStatus` ไม่ Generate QueueNumber ที่ status=2 (Paid) ขัดกับ Business Rule | แก้แล้ว |
| BUG-004 | Medium | `HomeController.cs` | ~52 | `TodayRevenue` นับแค่ OrderStatusId==2 ขาดสถานะ 3,4,5 — ยอดขายบน Dashboard ต่ำกว่าจริง | แก้แล้ว |

---

### รายละเอียด BUG-001 และ BUG-002 — Duplicate Primary Key ใน PointTransactions

**สาเหตุ (เหมือนกันทั้ง 2 ไฟล์):**

ขั้นตอนใน `ApprovePayment` / `ApprovePaymentAtPos` เมื่อลูกค้าเป็นสมาชิกและมีรายการ Coffee:

1. คำนวณ `nextTransId = DB.Max(TransId) + 1`
2. เพิ่ม Points transaction (TypeId=1) ด้วย `TransId = nextTransId` → ยังไม่ `SaveChanges()`
3. คำนวณ `nextStampId = DB.Max(TransId) + 1` ← **ค้นหา DB อีกครั้ง**
4. เนื่องจาก step 2 ยังไม่ commit → DB ยังเห็น max เดิม → `nextStampId == nextTransId`
5. เพิ่ม Stamp transaction (TypeId=3) ด้วย `TransId = nextStampId` = ซ้ำ
6. `SaveChanges()` → **DbUpdateException: Duplicate entry for PRIMARY KEY**

**โค้ดก่อนแก้ (AdminController.cs ~517):**
```csharp
int nextStampId = (_db.Pointtransactions.Any()
    ? _db.Pointtransactions.Max(t => t.TransId) : 0) + 1;
// nextStampId == nextTransId เสมอ
```

**โค้ดก่อนแก้ (PosController.cs ~634):**
```csharp
int nextStampId = (_db.Pointtransactions.Max(t => t.TransId)) + 1;
// อันตรายกว่า: ไม่มี .Any() guard → NullReferenceException ถ้า table ว่างอยู่ด้วย
```

**โค้ดหลังแก้ (ทั้ง 2 ไฟล์):**
```csharp
// ใช้ nextTransId + 1 แทนการ query DB ซ้ำ
// เพราะ Points transaction ถูกเพิ่มใน memory แต่ยังไม่ SaveChanges
int nextStampId = nextTransId + 1;
```

---

### รายละเอียด BUG-003 — UpdateOrderStatus ไม่ Generate QueueNumber ที่ Paid

**สาเหตุ:** ตาม Business Rule ใน CLAUDE.md: _"QueueNumber Generate ได้เฉพาะเมื่อ `OrderStatusId = 2` (Paid)"_
แต่ `UpdateOrderStatus` (Manual override โดย Admin) generate เฉพาะที่ status=4

ผล: ออเดอร์ที่ถูก Admin manual set เป็น Paid (2) จะไม่มี QueueNumber จนถึง Ready (4)
ขัดแย้งกับ `ApprovePayment` และ `ApprovePaymentAtPos` ที่ generate ที่ status=2

**โค้ดก่อนแก้:**
```csharp
if (newStatusId == 4 && string.IsNullOrEmpty(order.QueueNumber))
    order.QueueNumber = GenerateQueueNumber();
```

**โค้ดหลังแก้:**
```csharp
// Generate ที่ Paid (2) ตาม Business Rule + fallback ที่ Ready (4)
if ((newStatusId == 2 || newStatusId == 4) && string.IsNullOrEmpty(order.QueueNumber))
    order.QueueNumber = GenerateQueueNumber();
```

---

### รายละเอียด BUG-004 — TodayRevenue นับไม่ครบ

**สาเหตุ:** `HomeController.Dashboard` และ `AdminController.Index` คำนวณ `TodayRevenue` ด้วย Logic ต่างกัน:

| ที่ | Filter |
| :--- | :--- |
| `HomeController.Dashboard` (ผิด) | `OrderStatusId == 2` เท่านั้น |
| `AdminController.Index` (ถูก) | `OrderStatusId >= 2 && != 6` |

ออเดอร์ที่อยู่ใน Preparing (3), Ready (4), Completed (5) จะไม่ถูกนับใน Dashboard → ยอดขายแสดงน้อยกว่าจริง

**โค้ดหลังแก้:**
```csharp
TodayRevenue = _db.Orders
    .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow
             && o.OrderStatusId >= 2 && o.OrderStatusId != 6)
    .Sum(o => (decimal?)o.NetAmount) ?? 0,
```

---

### ข้อสังเกตจากรอบนี้ (ไม่ใช่บัค — ไม่แก้)

| รายการ | เหตุผลที่ยอมรับได้ |
| :--- | :--- |
| Connection String hardcode ใน `Csi402dbContext.OnConfiguring()` แทนที่จะอ่านจาก `appsettings.json` | มี `#warning` จาก EF Core Scaffold แล้ว ยอมรับสำหรับ Demo |
| `optionPopularity` ใน Reports ใช้ correlated subquery ซ้อน | ข้อมูล Demo ไม่มาก EF Core 9.0 translate ได้ |
| ID generation ไม่ใช้ Serializable Transaction (ยกเว้น Staff/Member) | Concurrent requests ต่ำในสภาพแวดล้อม Demo |

---

### สรุปรวมบัคทั้งหมดที่พบและแก้ไข (ทั้ง 2 รอบ)

| รอบ | จำนวนบัคพบ | Critical | Medium | Low | แก้แล้ว |
| :---: | :---: | :---: | :---: | :---: | :---: |
| รอบที่ 1 | 7 | 0 | 4 | 3 | 7 |
| รอบที่ 2 | 4 | 2 | 2 | 0 | 4 |
| **รวม** | **11** | **2** | **6** | **3** | **11** |

**สถานะโปรเจกต์: พร้อม Demo — บัคทุกรายการได้รับการแก้ไขครบถ้วน**
