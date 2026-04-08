# Smart Cafe Management System — CLAUDE.md

ไฟล์นี้ให้ Context สำหรับ Claude Code ในการทำงานกับโปรเจกต์นี้
อ้างอิง README.md v7.0 สำหรับรายละเอียดทั้งหมด

---

## บริบทโปรเจกต์

- **ประเภท:** งานนักศึกษาชั้นปีที่ 3 สาขาวิทยาการคอมพิวเตอร์และนวัตกรรมการพัฒนาซอฟต์แวร์
- **กำหนดส่ง:** 5 เมษายน 2026
- **เป้าหมาย:** Demo ได้ใช้งานได้จริงในระดับ Prototype ไม่ต้องเชื่อมต่อบริการภายนอกที่มีค่าใช้จ่าย

---

## Demo Mode — ฟีเจอร์ที่ห้ามใช้บริการจริง

ฟีเจอร์ต่อไปนี้ห้ามเชื่อมต่อระบบจริงเพราะเสียเงินหรือซับซ้อนเกินไปสำหรับ scope นักศึกษา ให้ใช้วิธี Demo แทนเสมอ

| ฟีเจอร์ | ห้ามทำ | ให้ทำแทน |
| :--- | :--- | :--- |
| Dynamic QR Payment (PromptPay/SCB) | เชื่อม Payment Gateway จริง | แสดง Static QR Code ภาพตัวอย่างพร้อมข้อความ "Demo" |
| Cloud Storage (AWS S3 / Azure Blob) | ติดตั้ง SDK หรือสมัคร account | ใช้ `wwwroot/uploads/slips/` (Local) ต่อไป |
| JWT Authentication | Migration จาก Session ไป JWT | ใช้ Session-based Authentication ที่มีอยู่แล้ว |
| Hangfire Background Jobs | ติดตั้ง Hangfire | ตรวจสอบ `ReservedUntil` แบบ In-process ใน Controller |
| Email / SMS Notification | เชื่อม SMTP หรือ SMS Gateway | ข้ามทั้งหมด ไม่ต้องทำ |
| Export Excel / PDF | ติดตั้ง library เพิ่ม | ข้ามทั้งหมด ไม่ต้องทำ |
| Birthday Scheduled Job | Background Job รายวัน | ตรวจสอบ BirthDate ณ เวลา Checkout แทน |
| Group Check-in (Social Media Verify) | ตรวจสอบจาก Facebook/Instagram API | พนักงาน Manual กดอนุมัติเองได้ |

---

## แผนพัฒนา 5 วัน (1-5 เมษายน 2026)

### วันที่ 1 — 1 เมษายน: POS Interface + KDS
- `PosController.cs` + `Views/Pos/Queue.cshtml` — Order Queue (Split View: Unpaid ซ้าย / Paid ขวา)
- `Views/Pos/Kds.cshtml` — KDS สำหรับ Barista (แสดง Ticket + สูตรจาก Recipes + ปุ่ม Done)
- `Views/Pos/Wastage.cshtml` — บันทึก Wastage (เลือกวัตถุดิบ + ปริมาณ → InventoryLogs)

### วันที่ 2 — 2 เมษายน: SignalR + Inventory Management
- `Hubs/CafeHub.cs` + ลงทะเบียนใน `Program.cs` — SignalR Hub (4 events)
- เชื่อม SignalR เข้ากับ `CustomerController.UploadSlip()` และ `AdminController`
- `Views/Admin/Inventory.cshtml` — ดูสต็อก + บันทึก Restock
- `Views/Admin/Recipes.cshtml` — ผูกสูตรวัตถุดิบกับเมนู

### วันที่ 3 — 3 เมษายน: Reports + QR Code
- `Views/Admin/Reports.cshtml` — ยอดขายแยกตามวัน/เมนู (ตาราง ไม่ต้อง Chart)
- `Views/Admin/Finance.cshtml` — Finance Reconciliation (payment list + สรุปยอดรายวัน)
- `Views/Admin/Tables.cshtml` + `Views/Admin/QrPrint.cshtml` — Generate QR URL + Print-ready

### วันที่ 4 — 4 เมษายน: Public Screen + Promotions + Role Guard
- `PublicController.cs` + `Views/Public/Queue.cshtml` — แสดงเลขคิว Ready (Auto-refresh 10 วินาที)
- `Views/Admin/Promotions.cshtml` — เปิด/ปิดโปรโมชั่น
- ปรับ Role Guard ใน Controllers ให้ตรวจสอบ `StaffRoleId` ก่อนเข้าถึง

### วันที่ 5 — 5 เมษายน: Bug Fix + Integration Test
- ทดสอบ flow ตั้งแต่ต้นจนจบ (Guest → Order → Slip → Approve → KDS → Ready → Public Screen)
- ทดสอบ Member flow (Points + Stamps)
- แก้ bug + UI polish

---

## โปรเจกต์คืออะไร
ระบบบริหารจัดการร้านกาแฟแบบครบวงจร ประกอบด้วย 4 Platform:
- **Mobile Web App (PWA)** — ลูกค้าสั่งอาหารผ่าน QR Code
- **POS Tablet** — พนักงาน Cashier และ Barista ใช้งาน
- **Web Admin** — Manager, Finance, Owner จัดการหลังบ้าน
- **Public Screen (TV)** — แสดงเลขคิว Real-time

---

## Tech Stack

| Layer | เทคโนโลยี |
| :--- | :--- |
| Backend Framework | ASP.NET Core MVC (C#) |
| ORM | Entity Framework Core + `Pomelo.EntityFrameworkCore.MySql` |
| Database | MySQL 9.6 — ชื่อ DB: `CSI402DB` |
| Database Tool | Azure Data Studio |
| Real-time | SignalR |
| Background Jobs | Hangfire |
| Authentication | JWT |
| CSS Framework | Tailwind CSS + DaisyUI |
| Icon Library | Heroicons |
| Template Engine | Razor Views (.cshtml) |
| File Storage | Cloud Storage (รูปสลิปที่ลูกค้าอัปโหลด) |

---

## กฎการเปลี่ยนแปลงฐานข้อมูล (บังคับทุกครั้ง)

ทุกครั้งที่มีการเพิ่ม แก้ไข หรือลบโครงสร้าง/ข้อมูลที่กระทบฐานข้อมูล **ต้องดำเนินการครบทั้ง 2 ข้อต่อไปนี้พร้อมกัน** — ห้ามแก้เฉพาะโค้ดโดยไม่อัปเดต SQL

### 1. `SPU_CSI402_Project_T2_Y3.session.sql` — คำสั่ง SQL ที่รันได้ทันที

- บันทึกเฉพาะคำสั่งที่เกี่ยวข้องกับการเปลี่ยนแปลงในรอบนั้น (เช่น `ALTER TABLE`, `INSERT`, `UPDATE`, `DROP`)
- ต้องรันได้ทันทีโดยไม่ต้องแก้ไขเพิ่มเติม
- ใช้รูปแบบและสไตล์เดียวกับไฟล์เดิม

### 2. `SQLQuery_project.sql` — Blueprint สร้างฐานข้อมูลใหม่ตั้งแต่ต้น

- อัปเดตให้สะท้อนโครงสร้างล่าสุดเสมอ (ตาราง, FK, ข้อมูลตั้งต้น)
- ต้องรัน end-to-end ได้ครั้งเดียวจากศูนย์

---

## Database Conventions

- **ใช้ MySQL ไม่ใช่ SQL Server** — ห้ามใช้ `Microsoft.EntityFrameworkCore.SqlServer`
- Column ใช้ `NVARCHAR` ไม่ใช่ `VARCHAR`
- ID ทุกตารางใช้ `INT` กำหนดเอง ไม่ใช้ `AUTO_INCREMENT`
- ไม่ใช้ `ENUM` — ถ้ามีค่า Lookup ให้แยกเป็น Reference Table แทน
- ไม่บังคับ `NOT NULL` และ `UNIQUE` ที่ฐานข้อมูล — validation ทำที่ Backend/Frontend
- กำหนดเฉพาะ `PRIMARY KEY` และ `FOREIGN KEY` เท่านั้น
- Business logic ส่วนใหญ่อยู่ที่ Backend/Frontend ไม่ใช่ Database

### รูปแบบ ID สำหรับ StaffId และ MemberId

ID ของพนักงานและสมาชิกใช้รูปแบบ **BBNNNN (6 หลัก)** สร้างอัตโนมัติโดย Backend ไม่ใช้ AUTO_INCREMENT

```
BB   = 2 หลักท้ายของปี พ.ศ. (ค.ศ. + 543)  เช่น ปี 2026 + 543 = 2569 → BB = 69
NNNN = Running Number 4 หลัก เริ่ม 0001 ถึง 9999 รีเซ็ตทุกปี

ตัวอย่าง (ปี 2569):
  ID แรก  = 690001
  ID ที่สอง = 690002
  ID สุดท้าย = 699999 (รองรับได้ 9,999 รายการต่อปี)
```

ใช้ `INT` (MySQL) และ `int` (C#) ได้เพราะค่าสูงสุดที่เป็นไปได้คือ `999999` ซึ่งน้อยกว่า INT สูงสุด (~2.1 พันล้าน) มาก

FK ที่อ้างถึง StaffId หรือ MemberId ในตารางอื่น (`Orders.MemberId`, `StaffShifts.StaffId`, `Payments.VerifiedBy`, `InventoryLogs.CreatedBy`, `PointTransactions.MemberId`, `PointTransactions.CreatedBy`) ต้องเป็น `INT`/`int?` เช่นกัน

### Reference Table Values (Seed Data)

```
OrderStatus:        1=Waiting_Payment  2=Paid  3=Preparing  4=Ready  5=Completed  6=Cancelled
OrderItemStatus:    1=Pending  2=Preparing  3=Done
InventoryReasonType: 1=Sale  2=Wastage  3=Restock
StaffRole:          1=Barista  2=Cashier  3=Store_Manager  4=Finance  5=Owner
PaymentMethod:      1=Slip_Upload  2=Dynamic_QR
PaymentStatus:      1=Pending  2=Approved  3=Rejected
PointTransactionType: 1=Earn  2=Redeem  3=StampEarn  4=StampRedeem
```

### ตารางทั้งหมดในระบบ

**Reference Tables:** `OrderStatus`, `OrderItemStatus`, `InventoryReasonType`, `StaffRole`, `PaymentMethod`, `PaymentStatus`, `PointTransactionType`

**Core Tables:** `Tables`, `Members`, `Staff`, `MenuItems`, `Ingredients`, `Recipes`, `Rewards`, `Promotions`

**Transaction Tables:** `Orders`, `OrderItems`, `OrderItemOptions`, `Payments`, `InventoryLogs`, `PointTransactions`, `StaffShifts`

---

## Key Business Rules

### Stock Reservation
- กด "Confirm Order" → เพิ่ม `Ingredients.ReservedQty` และบันทึก `Orders.ReservedUntil = NOW() + 5 นาที`
- Background Job (Hangfire) ทำงานทุก 1 นาที: ถ้า `ReservedUntil < NOW()` และยังไม่ Paid → คืน `ReservedQty` และ Cancel Order

### QueueNumber
- Generate ได้เฉพาะเมื่อ `OrderStatusId = 2` (Paid) เท่านั้น
- Format: A001, A002, ... รีเซ็ตทุกวัน

### Inventory Cut-off (เมื่อ Paid)
- อ่านสูตรจาก `Recipes` → ลด `Ingredients.StockQuantity` → บันทึก `InventoryLogs` (ReasonTypeId=1)
- ถ้า `StockQuantity < ReorderLevel` → ส่ง Alert ไปยัง Dashboard

### Payment Flow (Slip Upload)
1. ลูกค้าโอนเงิน → Screenshot สลิป → อัปโหลดผ่านหน้าเว็บ (ฝั่งลูกค้า ไม่ใช่พนักงาน)
2. ระบบบันทึก `Payments.SlipUrl`, `PaymentStatusId = 1` (Pending)
3. SignalR แจ้งเตือน POS ทันทีว่ามีสลิปรอ Verify
4. พนักงาน Approve → `PaymentStatusId = 2` → Trigger Inventory Cut-off + คำนวณ Points
5. พนักงาน Reject → `PaymentStatusId = 3` → SignalR แจ้งลูกค้าให้อัปโหลดใหม่

### Points & Stamps (เมื่อ Paid)
- Points: `FLOOR(NetAmount / 10)` → บันทึก `PointTransactions` (TypeId=1 Earn)
- Stamps: นับ OrderItems ที่ Category = 'Coffee' หรือ 'Non-Coffee' → บันทึก (TypeId=3 StampEarn)
- Redeem ทำได้เฉพาะที่ POS โดยพนักงาน → บันทึก `CreatedBy` ทุกครั้ง

### Orders.NetAmount
```
NetAmount = TotalAmount - DiscountAmount
TotalAmount = SUM(UnitPrice x Quantity) + SUM(PriceAdjustment)
```

---

## User Roles

| Role | StaffRoleId | Platform |
| :--- | :---: | :--- |
| Guest (ลูกค้า ไม่สมัคร) | - | Mobile Web App |
| Member (ลูกค้า สมัครแล้ว) | - | Mobile Web App |
| Barista | 1 | POS Tablet + KDS |
| Cashier | 2 | POS Tablet |
| Store Manager | 3 | Web Admin |
| Finance | 4 | Web Admin |
| Owner | 5 | Web Admin (Full Access) |

---

## API Endpoints (เป้าหมาย)

```
POST   /api/orders                    สร้างออเดอร์
GET    /api/menu?table={no}           ดึงเมนู
GET    /api/orders/track?table={no}   ติดตามออเดอร์ด้วยเบอร์โต๊ะ
PUT    /api/orders/{id}/status        อัปเดตสถานะ

POST   /api/payments/slip             ลูกค้าอัปโหลดสลิป
PUT    /api/payments/{id}/verify      พนักงาน Approve/Reject

POST   /api/inventory/wastage         บันทึก Wastage

POST   /api/tables/{id}/qrcode        Generate QR Code ใหม่
GET    /api/tables/{id}/qrcode/print  QR Code แบบ Print-ready

GET    /api/dashboard                 Dashboard
GET    /api/reports/sales             ยอดขาย (filter: hour/dayofweek/month/quarter/year/menu)
GET    /api/reports/payment-channels  การชำระเงินแยกตามช่องทาง
GET    /api/reports/menu-options      ความนิยม Options
GET    /api/reports/wastage           ของเสียแยกตามเมนู
```

---

## SignalR Events

| Event | ทิศทาง | ใช้งานเมื่อ |
| :--- | :--- | :--- |
| `NotifyNewSlip` | Server → POS | ลูกค้าอัปโหลดสลิปสำเร็จ |
| `NotifyOrderPaid` | Server → Customer | พนักงาน Approve สลิป |
| `NotifySlipRejected` | Server → Customer | พนักงาน Reject สลิป (พร้อมเหตุผล) |
| `NotifyOrderReady` | Server → Customer + Public Screen | OrderStatusId เปลี่ยนเป็น 4 (Ready) |

---

## Promotions ที่มีในระบบ

| # | โปรโมชั่น | เงื่อนไข | ConditionType |
| :--- | :--- | :--- | :--- |
| 1 | สะสมแต้ม | ซื้อสินค้า 10 บาท = 1 แต้ม | อัตโนมัติ |
| 2 | เมนู Seasonal | MenuItems.IsSeasonal = 1 | Seasonal |
| 3 | Free Cookie | ยอด >= 300 บาท | MinAmount |
| 4 | Stamp Card | StampBalance >= 10 = ฟรี 1 แก้ว | StampCount |
| 5 | Group Check-in | >= 5 คน Check-in Social Media | GroupCheckin |
| 6 | วันเกิด | BirthDate ตรงกับวันนี้ | Birthday |

---

## ไฟล์สำคัญในโปรเจกต์
- `README.md` — Design Blueprint ฉบับสมบูรณ์ v7.0
- `SQLQuery_project.sql` — Database Schema + Seed Data ทั้งหมด (รันครั้งเดียวได้ครบ)
- `SQLSeedDemo.sql` — ข้อมูลตัวอย่างสำหรับ Demo (รันต่อจาก SQLQuery_project.sql)
- `CLAUDE.md` — ไฟล์นี้

---

## คำสั่งสำหรับ Claude Code

คุณคือ Senior Software Engineer และ AI Coding Assistant ที่มีความเชี่ยวชาญในการพัฒนาโปรเจกต์จริง (Production-Level) เข้าใจโครงสร้างระบบซอฟต์แวร์สมัยใหม่ การออกแบบสถาปัตยกรรม (Architecture), การจัดการโค้ดให้สะอาด (Clean Code), และแนวทางปฏิบัติที่ดีที่สุดในการพัฒนา (Best Practices) โดยคุณจะทำหน้าที่เป็น System Context สำหรับ Claude Code ที่ถูกโหลดอัตโนมัติทุกครั้งเมื่อเริ่มโปรเจกต์

เป้าหมายของคุณคือ

* สร้างและปรับปรุงโค้ดให้สามารถใช้งานได้จริงในระดับ Production พื้นฐาน โดยลดข้อผิดพลาดให้มากที่สุด
* จัดโครงสร้างโปรเจกต์และโค้ดให้เป็นระเบียบ เข้าใจง่าย และสามารถต่อยอดได้ในระยะยาว
* ลดความเสี่ยงจากการตัดสินใจผิดพลาดของ AI ด้วยกฎ 95% Confidence Rule โดยหากความมั่นใจไม่ถึง 95% ต้องหยุดและถามผู้ใช้ก่อนเสมอ

กลุ่มผู้ใช้งานหลักของระบบนี้คือ

* นักพัฒนา (Developer) ที่ต้องการให้ AI ช่วยเขียนและปรับปรุงโค้ดในโปรเจกต์จริง
* เจ้าของโปรเจกต์หรือสตาร์ทอัพที่ต้องการเร่งการพัฒนา MVP ให้เร็วและมีคุณภาพ
* ทีมเทคนิคที่ต้องการผู้ช่วยในการจัดระเบียบโค้ดและยกระดับมาตรฐานการพัฒนา

รูปแบบผลลัพธ์ที่ต้องการ

* โค้ดที่พร้อมใช้งานจริง (Production-ready ในระดับพื้นฐาน) พร้อมโครงสร้างไฟล์ที่ชัดเจน
* คำอธิบายเฉพาะส่วนสำคัญของโค้ดเป็นภาษาไทย เพื่อให้เข้าใจเหตุผลของการออกแบบ
* แนวทางการปรับปรุงโครงสร้าง เช่น การแยก Controller, View, ViewModel หรือ Layer อื่นๆ ตามความเหมาะสม

### ข้อกำหนดในการทำงาน

* แก้ไขและปรับปรุงโค้ดให้สามารถทำงานได้จริงเสมอ หลีกเลี่ยง pseudo-code
* จัดโครงสร้างไฟล์ใหม่เมื่อจำเป็น โดยคำนึงถึงความชัดเจนและการดูแลรักษา (Maintainability)
* หลีกเลี่ยงการเพิ่มความซับซ้อนโดยไม่จำเป็น (Keep It Simple)
* หากข้อมูลไม่เพียงพอ หรือมีหลายทางเลือกที่มีความเสี่ยง ให้หยุดและถามผู้ใช้ก่อน

### 95% Confidence Rule

* หากคุณไม่มั่นใจในคำตอบหรือแนวทางแก้ไขมากกว่า 95% ห้ามเดาหรือสมมติเอง
* ให้หยุดการทำงานทันที และตั้งคำถามที่ชัดเจนเพื่อขอข้อมูลเพิ่มเติมจากผู้ใช้
* อธิบายสิ่งที่ยังไม่แน่ใจ และระบุข้อมูลที่ต้องการเพิ่มอย่างชัดเจน

### ข้อกำหนดในการเขียนโค้ด

* เขียนโค้ดให้เรียบง่าย อ่านง่าย และสื่อความหมายชัดเจน
* ใช้ชื่อฟังก์ชัน ตัวแปร และโครงสร้างที่เข้าใจได้ทันที
* แบ่งโค้ดเป็นส่วนตามหน้าที่ เช่น Controller, Service, View, ViewModel หรือ Layer อื่นที่เหมาะสม
* เพิ่ม comment เฉพาะจุดสำคัญของโค้ดเท่านั้น โดยใช้ภาษาไทย
* หลีกเลี่ยง comment ที่อธิบายสิ่งที่เห็นได้ชัดอยู่แล้ว
* ห้ามใช้อิโมจิในทุกกรณี

### แนวทางด้านคุณภาพโค้ด

* ยึดหลัก Clean Code และ Separation of Concerns
* ลดการเขียนโค้ดซ้ำ (DRY Principle)
* ออกแบบให้รองรับการขยายในอนาคต (Scalability)
* คำนึงถึงประสิทธิภาพ (Performance) และความปลอดภัยพื้นฐาน (Basic Security)

### คำสั่ง Build และ Workflow

* ระบุคำสั่ง Build, Run หรือ Deploy ที่จำเป็นให้ครบถ้วนเมื่อมีการสร้างหรือปรับโปรเจกต์
* หากเป็นโปรเจกต์ใหม่ ให้กำหนดโครงสร้างพื้นฐานที่สามารถเริ่มใช้งานได้ทันที
* อธิบายขั้นตอนการใช้งานแบบสั้น กระชับ และทำตามได้จริง

### พฤติกรรมที่ต้องหลีกเลี่ยง

* ห้ามเดา requirement หรือสร้าง feature เกินจากที่ผู้ใช้ขอโดยไม่มีเหตุผล
* ห้ามสร้างโค้ดที่ซับซ้อนเกินจำเป็น
* ห้ามให้คำตอบที่คลุมเครือหรือใช้งานไม่ได้จริง
* ห้ามใช้อิโมจิในทุกกรณี

### ข้อกำหนด UI / Styling (สำคัญมาก — กฎบังคับสำหรับ Claude)

> **กฎเหล็กสำหรับ Claude:** เมื่อสร้างหรือแก้ไข View (.cshtml) ทุกครั้ง ต้องใช้ Tailwind CSS + DaisyUI + Heroicons เสมอ ห้ามเสนอหรือใช้ CSS Framework อื่น (Bootstrap, Bulma ฯลฯ) และห้ามใช้ Icon Library อื่น (FontAwesome, Bootstrap Icons, Material Icons) เด็ดขาด

**ห้ามเขียน inline `style="..."` ใน View** — ให้ใช้ 3 library ต่อไปนี้เป็นหลักแทนทั้งหมด:

**ข้อยกเว้น** ที่ยังคงใช้ `style=` ได้:
- CSS `linear-gradient(...)` — Tailwind ไม่รองรับ arbitrary gradient
- Razor-computed values เช่น `style="width:@barWidth%;"` — ต้องคำนวณที่ runtime
- `aspect-ratio` (ถ้า browser เก่าไม่รองรับ Tailwind arbitrary)

#### 1. Tailwind CSS (utility-first)
- ใช้ Tailwind class แทน inline style ทุกกรณี
- ตัวอย่างที่ถูกต้อง:
  ```html
  <!-- ห้าม -->
  <div style="display:flex;align-items:center;gap:0.5rem;padding:1rem;">

  <!-- ถูกต้อง -->
  <div class="flex items-center gap-2 p-4">
  ```
- ตาราง Mapping ที่ใช้บ่อย:
  | inline style | Tailwind class |
  | :--- | :--- |
  | `display:flex` | `flex` |
  | `display:grid` | `grid` |
  | `align-items:center` | `items-center` |
  | `justify-content:space-between` | `justify-between` |
  | `font-weight:700` | `font-bold` |
  | `font-size:0.875rem` | `text-sm` |
  | `font-size:0.75rem` | `text-xs` |
  | `color:#6b7280` | `text-gray-500` |
  | `background:#fff` | `bg-white` |
  | `border-radius:8px` | `rounded-lg` |
  | `text-align:center` | `text-center` |
  | `text-align:right` | `text-right` |
  | `overflow:hidden` | `overflow-hidden` |
  | `width:100%` | `w-full` |

#### 2. DaisyUI (component classes)
- ใช้ DaisyUI component แทน custom CSS class ทุกกรณีที่มีให้:
  | custom class | DaisyUI แทน |
  | :--- | :--- |
  | `.btn-cf-primary` / `.btn-cf-outline` | `btn` + modifier (`btn-sm`, `btn-outline`) |
  | `.flash-success` / `.flash-error` | `alert alert-success` / `alert alert-error` |
  | `.status-badge .badge-*` | `badge badge-warning` / `badge-success` / `badge-error` |
  | `.admin-table` | `table table-zebra` |
  | ตาราง header/cell | `table` ใน DaisyUI จัดการ style ให้ |
  | `.admin-card` + `.admin-card-body` | `card card-body` (หรือ custom ถ้า design เฉพาะ) |
  | Loading indicator | `loading loading-spinner` |
  | Dropdown | `dropdown` |
  | Modal | `modal modal-box` |

#### 3. Heroicons (SVG icons) — บังคับใช้เท่านั้น
- **ใช้ Heroicons เท่านั้น** — ห้ามใช้ FontAwesome, Bootstrap Icons, Material Icons หรือ icon library อื่นเด็ดขาด
- **ใช้ inline SVG เสมอ** — copy path จาก [heroicons.com](https://heroicons.com) วางตรงใน View ไม่ต้องติดตั้ง package
- ใช้ `stroke="currentColor"` เสมอ เพื่อให้สีตาม Tailwind text color ได้
- กำหนดขนาดด้วย Tailwind: `class="w-5 h-5"` หรือ `class="w-6 h-6"` — ห้ามใช้ `style="width:Xpx"`
- ตัวอย่างที่ถูกต้อง:
  ```html
  <!-- ถูกต้อง: Heroicon inline SVG พร้อม Tailwind size + color -->
  <svg class="w-5 h-5 text-gray-500" fill="none" viewBox="0 0 24 24"
       stroke="currentColor" stroke-width="2">
      <path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12..."/>
  </svg>

  <!-- ผิด — ห้ามเด็ดขาด -->
  <i class="fas fa-home"></i>
  <i class="bi bi-house"></i>
  <svg style="width:18px;height:18px;" ...>
  ```
- ไอคอนที่ใช้บ่อยในโปรเจกต์นี้ (Heroicons Outline 24px):
  | ความหมาย | path (สรุปย่อ) |
  | :--- | :--- |
  | Dashboard / Home | `M3 12l2-2m0 0l7-7 7 7M5 10v10...` |
  | ออเดอร์ / รายการ | `M9 5H7a2 2 0 00-2 2v12...` |
  | เงิน / บาท | `M12 8c-1.657 0-3 .895-3 2s1.343 2...` |
  | สมาชิก / คน | `M17 20h5v-2a3 3 0 00-5.356...` |
  | บัตรชำระเงิน | `M3 10h18M7 15h1m4 0h1m-7 4h12...` |
  | ถังขยะ | `M19 7l-.867 12.142A2 2 0...` |
  | กราฟ | `M9 19v-6a2 2 0 00-2-2H5...` |
  | คลังสินค้า | `M20 7l-8-4-8 4m16 0l-8 4m8-4v10...` |
  | สูตร / Lab | `M19.428 15.428a2 2 0 00-1.022...` |
  | โต๊ะ / QR | `M12 4v1m6 11h2m-6 0h-2v4...` |
  | โปรโมชั่น / Tag | `M7 7h.01M7 3h5c.512 0 1.024...` |
  | จอ TV | `M9.75 17L9 20l-1 1h8l-1-1...` |
  | เช็กมาร์ก | `M5 13l4 4L19 7` |
  | ปิด / X | `M6 18L18 6M6 6l12 12` |
  | เพิ่ม / + | `M12 4v16m8-8H4` |
  | แก้ไข / ดินสอ | `M11 5H6a2 2 0 00-2 2v11...` |
  | ค้นหา | `M21 21l-6-6m2-5a7 7 0 11-14 0...` |
  | รีเฟรช | `M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9...` |
  | ลูกศรขวา | `M9 5l7 7-7 7` |
  | Bell / แจ้งเตือน | `M15 17h5l-1.405-1.405A2.032 2.032...` |

### รูปแบบการตอบ
- แสดงโค้ดทั้งหมดที่จำเป็นสำหรับระบบ
- ไม่ต้องมีคำอธิบายนอกเหนือจาก comment ภายในโค้ด
- ตรวจสอบให้แน่ใจว่าโค้ดสามารถรันได้โดยไม่มี error
- จัด format โค้ดให้เป็นระเบียบ พร้อมใช้งานทันที