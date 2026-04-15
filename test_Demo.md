# Demo Script — Smart Cafe Management System
# วันที่ Present: 19 เมษายน 2569

---

## ข้อมูล Login พร้อม Demo

### พนักงาน (Staff)

| บทบาท | Username | Password | ใช้ Demo เมื่อ |
|---|---|---|---|
| Owner / Admin | `admin` | `admin123` | ภาพรวมทุกหน้า |
| Cashier | `cashier1` | `admin123` | Approve สลิป + POS Queue |
| Barista | `barista1` | `admin123` | KDS ดูออเดอร์ + สูตร |
| Manager | `manager1` | `admin123` | Inventory + Recipes |

URL Login: `/Account/Login`

### สมาชิก (Member) — ใช้เบอร์โทรกรอกในหน้า Checkout

| ชื่อ | เบอร์โทร | แต้ม | Stamp | จุดเด่น |
|---|---|---|---|---|
| สมชาย ใจดี | `0891234567` | 150 แต้ม | 4/10 | Demo แลกแต้ม |
| สมหญิง แก้วใส | `0812345678` | 80 แต้ม | 7/10 | Demo ใกล้ครบ Stamp |
| วิทย์ เก่งกาจ | `0856789012` | 0 แต้ม | 0/10 | Demo Guest / สมาชิกใหม่ |

---

## Checklist ก่อน Present (รันวันก่อน 19 เม.ย.)

```
[ ] รัน SQLQuery_project.sql ใหม่บน DB สะอาด (DROP + CREATE)
[ ] รัน SQLSeedDemo.sql ต่อทันที
[ ] ทดสอบ Login ทุก Account: admin / cashier1 / barista1 / manager1 (password: admin123)
[ ] เปิด Browser 3 หน้าพร้อมกัน: Customer | POS | Admin
[ ] ทดสอบ Flow ครบ 1 รอบ:
      สแกน QR → สั่ง → Upload Slip → Approve → KDS Done → Public Screen
[ ] ตรวจ Admin/Finance แสดงข้อมูลวันที่วันนี้ถูกต้อง
[ ] ตรวจ Admin/Reports แสดงตาราง salesByDay / salesByMenu
[ ] ตรวจ Public Screen /Public/Queue เปิดได้และแสดงคิว
[ ] ตรวจ Inventory สต็อกถูกหักหลัง Approve สลิป
[ ] เตรียมรูปสลิปตัวอย่าง (.jpg) ไว้อัปโหลด
```

---

## การเตรียม Browser ก่อน Present

เปิด 3 หน้าต่างแยกกัน (แนะนำ 3 Tabs หรือ 2 หน้าจอ)

```
Tab 1 — ลูกค้า (Mobile View / DevTools F12 → Toggle device)
  URL: /Customer/Menu?table=T01

Tab 2 — POS / Cashier / Barista
  URL: /Pos/Queue  (login เป็น cashier1)
  URL: /Pos/Kds    (login เป็น barista1 — เปิดแยก Tab)

Tab 3 — Admin / Public Screen
  URL: /Admin      (login เป็น admin)
  URL: /Public/Queue (เปิดบน TV / projector แยก)
```

---

## Flow Demo ที่แนะนำ

---

### Scene 1 — ลูกค้าสั่งอาหาร (~3 นาที)

**Browser Tab 1 (Mobile View)**

**ขั้นตอน:**

1. เปิด `/Customer/Menu?table=T01`
   - ชี้ให้เห็น: "ลูกค้าเข้าจาก QR Code ที่ติดอยู่หน้าโต๊ะ T01"
   - ชี้ Category Filter Bar: กาแฟ / เครื่องดื่มอื่น / เบเกอรี่ / อาหาร / Seasonal
   - ชี้ Badge **"Seasonal"** บน Sakura Latte — "เมนูพิเศษตามฤดูกาล"

2. กดเลือก **Latte (75฿)** → หน้า Detail
   - เลือก ความหวาน: "หวานน้อย"
   - เลือก ประเภทนม: "นมสด"
   - เลือก ขนาด: "L (+15฿)" → ราคาปรับเป็น 90฿ อัตโนมัติ
   - กด "เพิ่มลงตะกร้า"

3. กลับเมนู → เพิ่ม **Cappuccino (75฿)** + **Croissant (65฿)**
   - ยอดในตะกร้า = 90 + 75 + 65 = **230฿**

4. กดไปหน้า Cart
   - ชี้ปุ่มปรับจำนวน + ลบรายการ

5. กด Checkout
   - กรอกเบอร์ `0891234567` (สมาชิก สมชาย)
   - ระบบแสดง: "สมชาย มี 150 แต้ม / Stamp 4/10"
   - กด "ยืนยันออเดอร์"

6. หน้า Payment
   - แสดง Static QR PromptPay (Demo)
   - ชี้ปุ่ม "แลกแต้ม" (1 แต้ม = 1 บาท)
   - อัปโหลดรูปสลิปตัวอย่าง
   - กด "อัปโหลดสลิป" → Redirect ไปหน้า Tracking อัตโนมัติ

**จุดพูด:**
> "ลูกค้าไม่ต้องสมัครสมาชิกก็สั่งได้ทันที ถ้าสมัครแล้วระบบจำแต้มและ Stamp ให้อัตโนมัติ"

---

### Scene 2 — Cashier Approve สลิป (~2 นาที)

**Browser Tab 2 (Login: cashier1)**

**ขั้นตอน:**

1. เปิด `/Pos/Queue`
   - ชี้ Panel ซ้าย "รอชำระ" → เห็นออเดอร์จาก Scene 1
   - ชี้ Badge สีแดง **"สลิปรอตรวจ"** ที่ขึ้นโดยไม่รีเฟรช (SignalR)

2. คลิกดูรูปสลิปใน Modal → ตรวจยอดเงิน

3. กด **"Approve"**
   - ระบบ Generate คิว **A001** ทันที
   - ออเดอร์ย้ายจากแถว "รอชำระ" → แถว "Active" ฝั่งขวา

4. สลับไปดู Tab 1 (ลูกค้า)
   - แสดง Pop-up: **"ชำระเงินสำเร็จ คิวของคุณ: A001"** (SignalR Real-time)

**จุดพูด:**
> "พนักงานเห็นสลิปทันทีผ่าน Real-time SignalR ไม่ต้องรีเฟรชหน้า อนุมัติเสร็จลูกค้าได้รับแจ้งทันที"

---

### Scene 3 — Barista KDS (~2 นาที)

**Browser Tab ใหม่ (Login: barista1)**

**ขั้นตอน:**

1. เปิด `/Pos/Kds`
   - ชี้ Ticket ออเดอร์ A001 พร้อมรายการเมนู
   - ชี้ **สูตรวัตถุดิบ** ใต้แต่ละเมนู:
     - Latte: เมล็ดกาแฟ Arabica 18g + นมสด 200ml
     - Cappuccino: เมล็ดกาแฟ 18g + นมสด 150ml + วิปครีม 50ml
   - ชี้ Options ที่ลูกค้าเลือก: ความหวาน / ประเภทนม / ขนาด

2. กด **"Done"** ที่ Latte → เปลี่ยนสีเป็นเขียว
3. กด **"Done"** ที่ Cappuccino
4. กด **"Done"** ที่ Croissant → ทุกรายการ Done

   - ออเดอร์เปลี่ยนเป็น **"พร้อมเสิร์ฟ"** อัตโนมัติ

5. สลับไปดู `/Public/Queue` (TV Screen)
   - A001 เปลี่ยนสีเป็นเขียว **"พร้อมเสิร์ฟ"** Real-time

6. สลับไปดู Tab ลูกค้า
   - แสดง Notification: **"ออเดอร์ของคุณพร้อมแล้ว กรุณารับที่เคาน์เตอร์"**

**จุดพูด:**
> "บาริสต้าเห็นสูตรชงทุกรายการพร้อมปริมาณที่ถูกต้อง เมื่อทำครบระบบแจ้งลูกค้าและอัปเดต Public Screen อัตโนมัติ"

---

### Scene 4 — โปรโมชั่นครบ 5 ข้อ (~2 นาที)

**Browser Tab 1 (ลูกค้า)**

**ขั้นตอน:**

1. ไป `/Admin/Promotions` (Admin) → ชี้ว่ามีโปรโมชั่นทั้ง 5 เปิดอยู่

2. **โปรโมชั่น 3 — ยอดครบ 300 ฟรีคุกกี้**
   - สั่ง: Avocado Toast (120) + Pasta Carbonara (145) + Cold Brew (85) = **350฿**
   - หน้า Checkout แสดง: **"คุณได้รับ Chocolate Chip Cookie ฟรี 1 ชิ้น"**
   - เมื่อ Approve → ออเดอร์มี Cookie เป็นรายการราคา 0฿ อัตโนมัติ

3. **โปรโมชั่น 4 — Stamp Card**
   - ใช้เบอร์ `0812345678` (สมหญิง Stamp 7/10)
   - ชี้ว่า Checkout แสดง Stamp Balance ปัจจุบัน
   - "ถ้าครบ 10 ดวง เลือกเมนูฟรีได้ทันที"

4. **โปรโมชั่น 1 — สะสมแต้ม**
   - หลัง Approve → ชี้ว่าระบบบวกแต้มให้อัตโนมัติ: `FLOOR(350 / 10) = 35 แต้ม`

5. **โปรโมชั่น 5 — Group Check-in**
   - ไป POS Queue → ชี้ปุ่ม "Group Check-in" บนออเดอร์
   - "พนักงาน Manual Approve เมื่อลูกค้ามา ≥ 5 คน"

6. **โปรโมชั่น 2 — Seasonal Menu**
   - ชี้ Sakura Latte มี Badge "Seasonal" บนหน้าเมนู
   - ไป Admin/MenuList → ชี้ว่าสามารถกำหนด Start/End Date ได้

**จุดพูด:**
> "ทุกโปรโมชั่นทำงานอัตโนมัติไม่ต้องให้ลูกค้าพิมพ์โค้ด ตรวจสอบและปรับได้ผ่านหน้า Admin"

---

### Scene 5 — Finance + Reports (~2 นาที)

**Browser Tab 3 (Login: admin)**

**ขั้นตอน:**

1. ไป `/Admin` (Dashboard)
   - ชี้ KPI Cards: สมาชิกทั้งหมด / เมนูที่เปิดอยู่ / ยอดขายวันนี้ / พนักงาน
   - ชี้ **Low Stock Alert** ที่ขึ้นอัตโนมัติเมื่อสต็อกต่ำกว่า Reorder Level
   - ชี้ Top 5 เมนูขายดีวันนี้
   - ชี้ ยอดขายรายชั่วโมง

2. ไป `/Admin/Finance`
   - เลือกวันที่วันนี้ → กด "ดู"
   - ชี้ Summary Cards: ยอดรับจริง / รออนุมัติ / อนุมัติแล้ว / ปฏิเสธ
   - ชี้ตาราง **P&L: รายรับ - COGS = กำไรขั้นต้น**
   - ชี้ตาราง Payment รายการ พร้อมลิงก์ดูสลิปแต่ละใบ

3. ไป `/Admin/Reports`
   - เลือกช่วงวันที่ → กด "กรอง"
   - ชี้ตาราง: ยอดขายแยกตามวัน / แยกตามเมนู / แยกตามชั่วโมง

**จุดพูด:**
> "Finance เห็นยอดจริงทุกรายการ ตรวจสอบสลิปได้ทันที และดู P&L รายวันโดยไม่ต้องรอสิ้นเดือน"

---

### Scene 6 — Inventory + Wastage (~1 นาที)

**Login: manager1**

**ขั้นตอน:**

1. ไป `/Admin/Inventory`
   - ชี้ตารางสต็อก: Stock Qty vs Reorder Level
   - ชี้ว่า หลัง Approve ออเดอร์ใน Scene 2 สต็อกลดอัตโนมัติ:
     - เมล็ดกาแฟ: -18g (Latte) -18g (Cappuccino) = -36g
     - นมสด: -200ml (Latte) -150ml (Cappuccino) = -350ml
   - กด Restock เมล็ดกาแฟ +500g → ประวัติขึ้นทันที

2. ไป `/Pos/Wastage`
   - เลือก "เมล็ดกาแฟ Arabica" ปริมาณ 50 หน่วย g
   - หมายเหตุ: "เครื่องชงขัดข้อง"
   - กด "บันทึก Wastage" → ปรากฏในประวัติ + สต็อกลด

**จุดพูด:**
> "ทุกการเคลื่อนไหวของวัตถุดิบถูกบันทึกอัตโนมัติ ทั้งการตัดสต็อกจากการขายและการบันทึกของเสีย"

---

## จุดที่ต้องอธิบายล่วงหน้า (ก่อน Q&A)

| ประเด็น | คำอธิบาย |
|---|---|
| QR Payment เป็นรูป Static | "Demo Mode ใช้รูป QR ตัวอย่าง ระบบจริงต่อ Payment Gateway ได้" |
| ไม่มี Auto Background Job | "ตรวจออเดอร์หมดอายุเมื่อพนักงานเปิดหน้า POS Queue แทน Hangfire" |
| Group Check-in ไม่ต่อ Social | "พนักงาน Manual Approve แทนการ Verify จาก Facebook/Instagram API" |
| Edit/Delete Staff ไม่มี | "Add + List Staff มีแล้ว Edit/Delete อยู่ใน Backlog" |
| ReservedUntil = 15 นาที | "ตั้งใจให้นานขึ้นเพื่อความสะดวกใน Demo" |

---

## สรุป Requirement Coverage

| Requirement | สถานะ |
|---|---|
| สั่งผ่าน QR Code ประจำโต๊ะ | ✓ |
| ไม่บังคับสมัครสมาชิก | ✓ |
| บาริสต้าดูออเดอร์ + สูตรชง | ✓ |
| Approve/Reject สลิป + แจ้ง Real-time | ✓ |
| Finance เช็คยอดรายวัน + P&L | ✓ |
| บันทึก Wastage | ✓ |
| อัปเดตสถานะออเดอร์ | ✓ |
| Inventory + Low Stock Alert | ✓ |
| จัดการโปรโมชั่น | ✓ |
| Public Queue Screen Real-time | ✓ |
| โปรโมชั่น 1: สะสมแต้ม 10฿/แต้ม | ✓ |
| โปรโมชั่น 2: เมนู Seasonal | ✓ |
| โปรโมชั่น 3: ฟรีคุกกี้ครบ 300฿ | ✓ |
| โปรโมชั่น 4: Stamp Card 10 แก้ว | ✓ |
| โปรโมชั่น 5: Group Check-in | ✓ Demo |
| Edit/Delete Staff | Backlog |
