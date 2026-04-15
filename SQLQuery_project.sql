-- อย่าลืมปิดประโยคด้วยเซมิโคลอน (;) ทุกครั้ง ไม่งั้นแตกกกก
USE CSI402DB;

--=========================================================================================
--  Smart Cafe Management System — Database Schema (Full Version)
--  เวอร์ชัน : v5.0
--  อัปเดต  : 26/03/2026
--  หมายเหตุ : Business logic ส่วนใหญ่อยู่ที่ Backend/Frontend
--             Database ทำหน้าที่เก็บข้อมูลเป็นหลัก ไม่ enforce constraint ซับซ้อน
--=========================================================================================


-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- REFERENCE TABLES
-- ตารางที่เก็บค่า Lookup / Enum แทนการใช้ ENUM type โดยตรง
-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

-- สถานะออเดอร์หลัก  1=Waiting_Payment  2=Paid  3=Preparing  4=Ready  5=Completed  6=Cancelled
CREATE TABLE OrderStatus (
    OrderStatusId   INT           PRIMARY KEY,
    StatusName      NVARCHAR(50)   -- ชื่อสถานะ เช่น 'Paid', 'Preparing'
);

-- สถานะของแต่ละรายการในออเดอร์ (ระดับ Item)  1=Pending  2=Preparing  3=Done
CREATE TABLE OrderItemStatus (
    OrderItemStatusId   INT           PRIMARY KEY,
    StatusName          NVARCHAR(50)   -- ชื่อสถานะ เช่น 'Pending', 'Done'
);

-- ประเภทเหตุผลที่ทำให้ยอดสต็อกเปลี่ยนแปลง  1=Sale  2=Wastage  3=Restock
CREATE TABLE InventoryReasonType (
    ReasonTypeId    INT           PRIMARY KEY,
    ReasonName      NVARCHAR(50)   -- ชื่อประเภทเหตุผล เช่น 'Sale', 'Wastage', 'Restock'
);

-- บทบาทของพนักงาน  1=Barista  2=Cashier  3=Store_Manager  4=Finance  5=Owner
CREATE TABLE StaffRole (
    StaffRoleId     INT           PRIMARY KEY,
    RoleName        NVARCHAR(50)   -- ชื่อบทบาท เช่น 'Barista', 'Owner'
);

-- ช่องทางการชำระเงิน  1=Slip_Upload  2=Dynamic_QR
CREATE TABLE PaymentMethod (
    PaymentMethodId     INT           PRIMARY KEY,
    MethodName          NVARCHAR(50)   -- ชื่อช่องทาง เช่น 'Slip_Upload', 'Dynamic_QR'
);

-- สถานะการชำระเงิน  1=Pending  2=Approved  3=Rejected
CREATE TABLE PaymentStatus (
    PaymentStatusId     INT           PRIMARY KEY,
    StatusName          NVARCHAR(50)   -- ชื่อสถานะ เช่น 'Pending', 'Approved', 'Rejected'
);

-- ประเภทการเคลื่อนไหวของแต้มและแสตมป์  1=Earn  2=Redeem  3=StampEarn  4=StampRedeem
CREATE TABLE PointTransactionType (
    PointTransactionTypeId  INT           PRIMARY KEY,
    TypeName                NVARCHAR(50)   -- ชื่อประเภท เช่น 'Earn', 'StampRedeem'
);


-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- CORE TABLES
-- ตารางหลักของระบบ เก็บข้อมูล Master ที่ใช้อ้างอิง
-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

-- โต๊ะทั้งหมดในร้าน แต่ละโต๊ะมี QR Code ของตัวเอง
CREATE TABLE Tables (
    TableId         INT             PRIMARY KEY,
    TableNumber     NVARCHAR(10),    -- เลขโต๊ะที่แสดงให้ลูกค้าเห็น เช่น 'T01', 'T02' ต้องตรงกับ QR Code
    QrCodeUrl       NVARCHAR(255),   -- URL ของ QR Code ที่ปริ้นติดโต๊ะ (URL มี Parameter ?table_no=X)
    IsActive        BIT              -- 1=พร้อมใช้งาน  0=ปิดใช้งาน [BIT คือประเภทข้อมูลที่เก็บค่า 0 หรือ 1 (Boolean)]
);

-- ข้อมูลสมาชิก ลูกค้าที่สมัครเพื่อสะสมแต้มและใช้โปรโมชั่น
-- MemberId ใช้ format BBNNNN (6 หลัก) สร้างอัตโนมัติโดย Backend
--   BB   = 2 หลักท้ายของปี พ.ศ. เช่น ปี 2569 → 69
--   NNNN = Running Number 4 หลัก เริ่ม 0001 ถึง 9999 (รีเซ็ตทุกปี)
--   ตัวอย่าง: สมาชิกคนแรกปี 2569 = 690001
CREATE TABLE Members (
    MemberId        INT             PRIMARY KEY,
    Phone           NVARCHAR(20),    -- เบอร์โทรศัพท์ ใช้ Login และค้นหาสมาชิก เป็น Candidate Key ที่ไม่ซ้ำกัน
    FirstName       NVARCHAR(50),    -- ชื่อจริง
    LastName        NVARCHAR(50),    -- นามสกุล
    BirthDate       DATE,            -- วันเกิด ใช้คำนวณโปรโมชั่นวันเกิดในอนาคต
    Points          INT,             -- แต้มสะสมคงเหลือ คำนวณจาก 10 บาท = 1 แต้ม
    StampBalance    INT              -- จำนวนแสตมป์คงเหลือ ครบ 10 แสตมป์ = รับเครื่องดื่มฟรี 1 แก้ว
);

-- ข้อมูลพนักงานทุกคนในระบบ รวมถึง Owner และ Finance
-- StaffId ใช้ format BBNNNN (6 หลัก) สร้างอัตโนมัติโดย Backend (logic เดียวกับ MemberId)
CREATE TABLE Staff (
    StaffId         INT             PRIMARY KEY,
    StaffRoleId     INT,             -- FK → StaffRole  บทบาทของพนักงานคนนี้
    FirstName       NVARCHAR(50),    -- ชื่อจริง
    LastName        NVARCHAR(50),    -- นามสกุล
    Username        NVARCHAR(50),    -- ชื่อผู้ใช้สำหรับ Login เข้าระบบ POS และ Web Admin
    PasswordHash    NVARCHAR(255),   -- รหัสผ่านที่ผ่านการ Hash แล้ว ห้ามเก็บ Plain Text
    IsActive        BIT              -- 1=ยังทำงานอยู่  0=ออกแล้วหรือถูกระงับ
);

-- รายการเมนูทั้งหมด ทั้งเมนูปกติและเมนูตามฤดูกาล
CREATE TABLE MenuItems (
    MenuItemId      INT             PRIMARY KEY,
    MenuName        NVARCHAR(150),   -- ชื่อเมนูที่แสดงหน้าเว็บและ POS
    MenuDescription NVARCHAR(500),   -- รายละเอียดเมนู เช่น ส่วนผสม หรือ คำอธิบายรสชาติ
    Price           DECIMAL(10,2),   -- ราคาขายปกติ [DECIMAL(10,2) คือตัวเลขทศนิยม 2 หลัก เช่น 85.00]
    Category        NVARCHAR(50),    -- หมวดหมู่เมนู เช่น Coffee, Non-Coffee, Food, Bakery
    IsAvailable     BIT,             -- 1=มีขายอยู่  0=ปิดชั่วคราว (Sold Out) ระบบจะแสดง Badge หมด
    ImageUrl        NVARCHAR(255),   -- URL รูปภาพของเมนู ใช้แสดงหน้าเว็บลูกค้า
    IsSeasonal      BIT,             -- 1=เมนูตามฤดูกาล (Seasonal)  0=เมนูปกติ
    SeasonStartDate DATETIME,        -- วันเวลาที่เริ่มแสดงเมนู Seasonal (NULL ได้ ถ้าเป็นเมนูถาวร)
    SeasonEndDate   DATETIME         -- วันเวลาที่สิ้นสุดเมนู Seasonal (NULL ได้ ถ้าเป็นเมนูถาวร)
);

-- วัตถุดิบทั้งหมดที่ใช้ในร้าน ระบบจะตัดสต็อกอัตโนมัติตาม Recipes เมื่อออเดอร์ถูก Paid
CREATE TABLE Ingredients (
    IngredientId    INT             PRIMARY KEY,
    IngredientName  NVARCHAR(150),   -- ชื่อวัตถุดิบ เช่น นมสด, เมล็ดกาแฟ, น้ำตาล
    StockQuantity   FLOAT,           -- ยอดสต็อกที่พร้อมใช้งานจริง (ไม่รวม ReservedQty)
    ReservedQty     FLOAT,           -- ยอดที่ถูก Reserve ชั่วคราวรอลูกค้าชำระเงิน ยังไม่ตัดสต็อกถาวร
    Unit            NVARCHAR(20),    -- หน่วยของวัตถุดิบ เช่น ml, g, piece
    ReorderLevel    FLOAT,           -- ระดับสต็อกขั้นต่ำ ถ้า StockQuantity ต่ำกว่านี้จะแจ้งเตือน Manager
    CostPerUnit     DECIMAL(10,4)    -- ต้นทุนต่อหน่วย ใช้คำนวณ COGS และกำไรสุทธิ [DECIMAL(10,4) ใช้ทศนิยม 4 หลักเพื่อความแม่นยำ]
);

-- สูตรเครื่องดื่มและอาหาร หัวใจของระบบตัดสต็อก
-- 1 เมนูมีได้หลาย Record (เพราะใช้หลายวัตถุดิบ) เช่น Latte = นมสด 150ml + เมล็ดกาแฟ 18g
CREATE TABLE Recipes (
    RecipeId            INT             PRIMARY KEY,
    MenuItemId          INT,             -- FK → MenuItems  เมนูที่สูตรนี้สังกัด
    IngredientId        INT,             -- FK → Ingredients  วัตถุดิบที่ใช้ใน Step นี้
    QuantityRequired    FLOAT,           -- ปริมาณที่ใช้ต่อ 1 หน่วย เช่น 150 (ml) หรือ 18 (g)
    Unit                NVARCHAR(20)     -- หน่วยของปริมาณนี้ เช่น ml, g
);

-- ของรางวัลที่สมาชิกสามารถแลกด้วยแต้มสะสม เช่น แก้วกาแฟ, เสื้อ
CREATE TABLE Rewards (
    RewardId        INT             PRIMARY KEY,
    RewardName      NVARCHAR(150),   -- ชื่อของรางวัล เช่น แก้วกาแฟ Limited Edition
    PointsRequired  INT,             -- จำนวนแต้มที่ต้องใช้แลก
    StockQuantity   INT,             -- จำนวนของรางวัลที่เหลืออยู่
    ImageUrl        NVARCHAR(255),   -- URL รูปภาพของรางวัล ใช้แสดงใน Rewards Catalog
    IsActive        BIT              -- 1=เปิดให้แลกได้  0=ปิดชั่วคราว
);

-- โปรโมชั่นทั้งหมดในระบบ ออกแบบให้ Manager และ Owner เปิด/ปิดได้ผ่าน Web Admin โดยไม่ต้องแก้ Code
CREATE TABLE Promotions (
    PromotionId     INT             PRIMARY KEY,
    PromotionName   NVARCHAR(150),   -- ชื่อโปรโมชั่น เช่น Free Cookie เมื่อยอดครบ 300
    ConditionType   NVARCHAR(50),    -- ประเภทเงื่อนไข เช่น MinAmount, StampCount, GroupCheckin
    ConditionValue  NVARCHAR(255),   -- ค่าเงื่อนไข เช่น '300' หรือ '10' (ตีความที่ Backend)
    RewardType      NVARCHAR(50),    -- ประเภทรางวัล เช่น FreeItem, Discount
    RewardValue     NVARCHAR(255),   -- ค่ารางวัล เช่น MenuItemId ของ Cookie หรือ DiscountAmount
    IsActive        BIT,             -- 1=โปรโมชั่นเปิดอยู่  0=ปิดชั่วคราว
    StartDate       DATE,            -- วันที่เริ่มใช้โปรโมชั่น (NULL = ไม่มีวันหมดอายุ)
    EndDate         DATE             -- วันที่สิ้นสุดโปรโมชั่น (NULL = ไม่มีวันหมดอายุ)
);


-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- TRANSACTION TABLES
-- ตารางที่บันทึกการทำงานจริงของระบบในแต่ละวัน
-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

-- ออเดอร์หลัก 1 ออเดอร์ต่อ 1 ครั้งที่ลูกค้ากด Confirm Order
-- รองรับทั้ง Guest (MemberId = NULL) และ Member
CREATE TABLE Orders (
    OrderId         INT             PRIMARY KEY,
    MemberId        INT,             -- FK → Members  NULL ถ้าเป็น Guest ที่ไม่สมัครสมาชิก
    TableId         INT,             -- FK → Tables  โต๊ะที่ลูกค้านั่ง
    OrderStatusId   INT,             -- FK → OrderStatus  สถานะปัจจุบันของออเดอร์
    GuestName       NVARCHAR(100),   -- ชื่อเล่นที่ Guest กรอกเอง (NULL ถ้าเป็น Member)
    QueueNumber     NVARCHAR(10),    -- เลขคิว เช่น A001 สร้างเฉพาะเมื่อ OrderStatusId = 2 (Paid) เท่านั้น
    TotalAmount     DECIMAL(10,2),   -- ยอดรวมก่อนส่วนลด คำนวณจาก SUM(UnitPrice x Quantity) + SUM(PriceAdjustment)
    DiscountAmount  DECIMAL(10,2),   -- ยอดส่วนลดทั้งหมด รวมทุกโปรโมชั่น DECIMAL(10,2) คือตัวเลขทศนิยม 2 หลัก เช่น 50.00
    NetAmount       DECIMAL(10,2),   -- ยอดที่ต้องชำระจริง = TotalAmount - DiscountAmount
    ReservedUntil   DATETIME,        -- เวลาที่สต็อกจะถูกปล่อยคืนถ้าลูกค้าไม่จ่ายเงิน (ตั้งไว้ 5 นาทีหลัง Confirm)
    CreatedAt       DATETIME         -- เวลาที่ลูกค้ากด Confirm Order
);

-- รายการสินค้าแต่ละชิ้นภายในออเดอร์ 1 ออเดอร์มีได้หลาย OrderItems
CREATE TABLE OrderItems (
    OrderItemId         INT             PRIMARY KEY,
    OrderId             INT,             -- FK → Orders  ออเดอร์ที่รายการนี้สังกัด
    MenuItemId          INT,             -- FK → MenuItems  เมนูที่สั่ง
    OrderItemStatusId   INT,             -- FK → OrderItemStatus  สถานะการชงของรายการนี้
    Quantity            INT,             -- จำนวนที่สั่ง เช่น 2 แก้ว
    UnitPrice           DECIMAL(10,2),   -- ราคา ณ เวลาที่สั่ง (Price Snapshot) เพื่อป้องกันราคาเปลี่ยนภายหลัง
    IsStampReward       TINYINT(1)       -- 1 = เมนูฟรีจากการแลก Stamp Card, 0/NULL = รายการปกติ
);

-- ตัวเลือกเพิ่มเติมของแต่ละรายการ เช่น ระดับความหวาน ประเภทนม เพิ่ม Shot
-- 1 OrderItem มีได้หลาย Options
CREATE TABLE OrderItemOptions (
    OrderItemOptionId   INT             PRIMARY KEY,
    OrderItemId         INT,             -- FK → OrderItems  รายการที่ Option นี้สังกัด
    OptionName          NVARCHAR(100),   -- ชื่อประเภทตัวเลือก เช่น Sweetness, Milk Type
    OptionValue         NVARCHAR(100),   -- ค่าที่ลูกค้าเลือก เช่น 'Less Sugar 25%', 'Oat Milk'
    PriceAdjustment     DECIMAL(10,2)    -- ราคาที่บวกเพิ่มหรือหักออก เช่น +15.00 กรณีเพิ่ม Shot  0.00 ถ้าไม่มีค่าใช้จ่าย
);

-- บันทึกการชำระเงินของแต่ละออเดอร์ รวมถึงการ Upload สลิปและผลการ Verify
CREATE TABLE Payments (
    PaymentId       INT             PRIMARY KEY,
    OrderId         INT,             -- FK → Orders  ออเดอร์ที่ชำระเงินนี้
    PaymentMethodId INT,             -- FK → PaymentMethod  วิธีชำระ เช่น โอนเงิน หรือ QR Code
    PaymentStatusId INT,             -- FK → PaymentStatus  สถานะ เช่น Pending รอ Verify หรือ Approved แล้ว
    Amount          DECIMAL(10,2),   -- ยอดที่ลูกค้าชำระมา ควรตรงกับ Orders.NetAmount
    SlipUrl         NVARCHAR(255),   -- URL รูปสลิปที่พนักงานถ่ายหรือลูกค้าอัปโหลด (NULL ถ้าจ่ายเงินสด)
    VerifiedBy      INT,             -- FK → Staff  รหัสพนักงานที่กด Approve หรือ Reject สลิป
    VerifiedAt      DATETIME,        -- เวลาที่พนักงานทำการ Verify
    RejectReason    NVARCHAR(255)    -- เหตุผลที่ Reject สลิป เช่น สลิปไม่ชัดเจน/ยอดไม่ตรง/สลิปซ้ำ (NULL ถ้า Approve)
);

-- บันทึกการเคลื่อนไหวของสต็อกทุกรายการ ไม่ว่าจะเป็นการขาย ของเสีย หรือเติมสต็อก
-- ใช้คำนวณต้นทุนจริงและสรุปรายงาน P&L
CREATE TABLE InventoryLogs (
    LogId           INT             PRIMARY KEY,
    IngredientId    INT,             -- FK → Ingredients  วัตถุดิบที่มีการเปลี่ยนแปลง
    ReasonTypeId    INT,             -- FK → InventoryReasonType  เหตุผล เช่น Sale, Wastage, Restock
    RefOrderId      INT,             -- FK → Orders  อ้างอิงออเดอร์ที่ทำให้สต็อกลด (เฉพาะกรณี ReasonType = Sale)
    CreatedBy       INT,             -- FK → Staff  พนักงานที่บันทึกรายการนี้
    QuantityChange  FLOAT,           -- ปริมาณที่เปลี่ยน ค่าลบ = ลดสต็อก  ค่าบวก = เพิ่มสต็อก
    Notes           NVARCHAR(500),   -- หมายเหตุ/สาเหตุ เช่น ของหมดอายุ ทำหก (ใช้กรณี Wastage เป็นหลัก)
    CreatedAt       DATETIME         -- เวลาที่บันทึก
);

-- บันทึกการเคลื่อนไหวของแต้มและแสตมป์สมาชิก เพื่อ Audit ย้อนหลังได้ว่าแต้มมาจากไหนและใช้ไปเมื่อไหร่
CREATE TABLE PointTransactions (
    TransId         INT             PRIMARY KEY,
    MemberId        INT,             -- FK → Members  สมาชิกเจ้าของแต้ม/แสตมป์
    TypeId          INT,             -- FK → PointTransactionType  ประเภทการเคลื่อนไหว เช่น Earn, StampRedeem
    Amount          INT,             -- จำนวนแต้มหรือแสตมป์ที่เปลี่ยน ค่าบวก = ได้รับ  ค่าลบ = ใช้ไป
    RefOrderId      INT,             -- FK → Orders  อ้างอิงออเดอร์ที่ทำให้เกิดแต้ม (NULL ถ้าเป็นการ Redeem)
    CreatedBy       INT,             -- FK → Staff  พนักงานที่กด Redeem ให้ลูกค้า (NULL ถ้าระบบ Earn อัตโนมัติ)
    CreatedAt       DATETIME         -- เวลาที่บันทึก
);

-- บันทึกกะการทำงานของพนักงาน ใช้ Track ว่าใครทำงานช่วงไหน และ Audit ธุรกรรมในแต่ละกะ
CREATE TABLE StaffShifts (
    ShiftId         INT             PRIMARY KEY,
    StaffId         INT,             -- FK → Staff  พนักงานที่เข้ากะ
    ShiftStart      DATETIME,        -- เวลาเริ่มกะ (Clock In)
    ShiftEnd        DATETIME,        -- เวลาสิ้นสุดกะ (Clock Out)  NULL = ยังอยู่ในกะ ยังไม่ Clock Out
    CreatedAt       DATETIME         -- เวลาที่บันทึก Record นี้
);


-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- เพิ่มข้อมูลเริ่มต้น (Seed Data)
-- ต้อง INSERT Reference Tables ก่อนเสมอ เพราะตารางอื่นอ้างอิง FK มาที่นี่
-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

-- สถานะออเดอร์
INSERT INTO OrderStatus (OrderStatusId, StatusName) VALUES
    (1, 'Waiting_Payment'),
    (2, 'Paid'),
    (3, 'Preparing'),
    (4, 'Ready'),
    (5, 'Completed'),
    (6, 'Cancelled');

-- สถานะของรายการในออเดอร์ (ระดับ Item บนจอ KDS)
INSERT INTO OrderItemStatus (OrderItemStatusId, StatusName) VALUES
    (1, 'Pending'),
    (2, 'Preparing'),
    (3, 'Done');

-- ประเภทเหตุผลที่สต็อกเปลี่ยน
INSERT INTO InventoryReasonType (ReasonTypeId, ReasonName) VALUES
    (1, 'Sale'),
    (2, 'Wastage'),
    (3, 'Restock');

-- บทบาทพนักงาน  (5=Owner มีสิทธิ์สูงสุดในระบบ)
INSERT INTO StaffRole (StaffRoleId, RoleName) VALUES
    (1, 'Barista'),
    (2, 'Cashier'),
    (3, 'Store_Manager'),
    (4, 'Finance'),
    (5, 'Owner');

-- ช่องทางการชำระเงิน
INSERT INTO PaymentMethod (PaymentMethodId, MethodName) VALUES
    (1, 'Slip_Upload'),
    (2, 'Dynamic_QR');

-- สถานะการชำระเงิน
INSERT INTO PaymentStatus (PaymentStatusId, StatusName) VALUES
    (1, 'Pending'),
    (2, 'Approved'),
    (3, 'Rejected');

-- ประเภทการเคลื่อนไหวของแต้มและแสตมป์
-- Earn / Redeem = แต้มสะสม,  StampEarn / StampRedeem = แสตมป์บัตรสะสม
INSERT INTO PointTransactionType (PointTransactionTypeId, TypeName) VALUES
    (1, 'Earn'),
    (2, 'Redeem'),
    (3, 'StampEarn'),
    (4, 'StampRedeem');


-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- Promotions Seed — โปรโมชั่นทั้ง 5 สำหรับ Demo
-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
-- หมายเหตุ: PromotionId 1 (สะสมแต้ม) และ 4 (Stamp Card) มีไว้เพื่อแสดงใน Admin Dashboard
-- Logic ที่แท้จริงอยู่ใน ApprovePayment (Points) และ StampBalance (Stamp Card)
-- PromotionId 3 (MinAmount→FreeItem) ต้องการ MenuItemId 18 (Cookie) ใน SQLSeedDemo.sql
INSERT INTO Promotions (PromotionId, PromotionName, ConditionType, ConditionValue, RewardType, RewardValue, IsActive, StartDate, EndDate) VALUES
    (1, 'สะสมแต้ม 10 บาท = 1 แต้ม',
        NULL,           NULL,   'PointEarn',   '1',   1, NULL, NULL),
    (2, 'เมนู Seasonal ตามเทศกาล',
        'Seasonal',     NULL,   'SeasonalMenu','',    1, '2025-03-01', '2027-12-31'),
    (3, 'ยอดครบ 300 บาท รับ Chocolate Chip Cookie ฟรี 1 ชิ้น',
        'MinAmount',    '300',  'FreeItem',    '18',  1, NULL, NULL),
    (4, 'Stamp Card — ครบ 10 ดวง รับเครื่องดื่มฟรี 1 แก้ว',
        'StampCount',   '10',   'FreeDrink',   '1',   1, NULL, NULL),
    (5, 'Group Check-in ≥5 คน รับส่วนลด 1 แก้ว (ประมาณ 75 บาท)',
        'GroupCheckin', '5',    'Discount',    '75',  1, NULL, NULL);