-- SQLSeedDemo.sql
-- ข้อมูลตัวอย่างสำหรับ Demo — รัน AFTER ที่ CREATE TABLE เสร็จแล้ว
-- รวม: Tables, Staff, MenuItems, Ingredients, Recipes
-- ============================================================

USE CSI402DB;

-- ============================================================
-- Tables — โต๊ะในร้าน
-- ============================================================
INSERT INTO Tables (TableId, TableNumber, QrCodeUrl, IsActive) VALUES
    (1, 'T01', '/customer/menu?table=T01', 1),
    (2, 'T02', '/customer/menu?table=T02', 1),
    (3, 'T03', '/customer/menu?table=T03', 1),
    (4, 'T04', '/customer/menu?table=T04', 1),
    (5, 'T05', '/customer/menu?table=T05', 0);  -- โต๊ะปิด


-- ============================================================
-- Staff — พนักงาน
-- PasswordHash = SHA256("admin123") ด้วย C# SHA256.Create() + Convert.ToHexString().ToLower()
-- ผลลัพธ์: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a1
--
-- หรือสร้าง Account แรกผ่านเว็บ: GET /Admin/Setup
-- (เฉพาะเมื่อยังไม่มี Staff ในระบบ)
-- ============================================================
-- StaffId ใช้ format BBNNNN: BB=69 (ปี พ.ศ. 2569), NNNN=Running Number
-- 690001=Admin/Owner, 690002=Cashier, 690003=Barista, 690004=Manager
INSERT INTO Staff (StaffId, StaffRoleId, FirstName, LastName, Username, PasswordHash, IsActive) VALUES
    (690001, 5, 'Admin',  'Owner',   'admin',    '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a1', 1),
    (690002, 2, 'สมชาย',  'แก้วใส',  'cashier1', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a1', 1),
    (690003, 1, 'สมหญิง', 'ดีงาม',   'barista1', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a1', 1),
    (690004, 3, 'วิชัย',  'จัดการ',  'manager1', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a1', 1);


-- ============================================================
-- MenuItems — เมนูทั้งหมด (ใช้รูปจาก placehold.co เป็น Demo)
-- ============================================================
INSERT INTO MenuItems (MenuItemId, MenuName, MenuDescription, Price, Category, IsAvailable, ImageUrl, IsSeasonal) VALUES

    -- Coffee
    (1,  'Espresso',          'กาแฟเข้มข้น กลิ่นหอม ทำจากเมล็ดกาแฟคัดพิเศษ',
         55.00, 'Coffee', 1, 'https://images.unsplash.com/photo-1510707577719-ae7c14805e3a?w=400&h=300&fit=crop', 0),

    (2,  'Latte',             'ลาเต้นมเนียน กลมกล่อม เข้ากันกับเอสเพรสโซ่',
         75.00, 'Coffee', 1, 'https://images.unsplash.com/photo-1570968915860-54d5c301fa9f?w=400&h=300&fit=crop', 0),

    (3,  'Cappuccino',        'คาปูชิโน่ฟองนมหนา กลิ่นหอม รสชาติกลมกล่อม',
         75.00, 'Coffee', 1, 'https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=400&h=300&fit=crop', 0),

    (4,  'Americano',         'กาแฟดำเจือน้ำร้อน รสเข้มอ่อนตามใจ',
         65.00, 'Coffee', 1, 'https://images.unsplash.com/photo-1551030173-122aabc4489c?w=400&h=300&fit=crop', 0),

    (5,  'Cold Brew',         'กาแฟหมักเย็น 12 ชั่วโมง รสเข้ม หวานตามธรรมชาติ',
         85.00, 'Coffee', 1, 'https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=400&h=300&fit=crop', 0),

    (6,  'Caramel Macchiato', 'ลาเต้ราดคาราเมล รสหวานหอม ชั้นสีสวย',
         90.00, 'Coffee', 1, 'https://images.unsplash.com/photo-1485808191679-5f86510bd9d3?w=400&h=300&fit=crop', 0),

    -- Non-Coffee
    (7,  'Matcha Latte',      'ชาเขียวมัทฉะนมเนียน รสชาติเข้มข้น หอมใบชา',
         80.00, 'Non-Coffee', 1, 'https://images.unsplash.com/photo-1589476993333-f55b84301219?w=400&h=300&fit=crop', 0),

    (8,  'Chocolate Frappe',  'ช็อกโกแลตปั่นเย็น ครีมเนื้อเนียน หวานหอม',
         85.00, 'Non-Coffee', 1, 'https://images.unsplash.com/photo-1553361371-9b22f78e8b1d?w=400&h=300&fit=crop', 0),

    (9,  'Thai Milk Tea',     'ชาไทยนมข้นหวาน สูตรต้นตำรับ หวานมัน',
         65.00, 'Non-Coffee', 1, 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=300&fit=crop', 0),

    (10, 'Strawberry Soda',   'โซดาสตรอเบอร์รี่สดชื่น รสหวานอมเปรี้ยว',
         70.00, 'Non-Coffee', 1, 'https://images.unsplash.com/photo-1497534446932-c925b458314e?w=400&h=300&fit=crop', 0),

    -- Food
    (11, 'Avocado Toast',     'ขนมปังซาวร์โดว์ทาอะโวคาโด ไข่ดาว โรยงา',
         120.00, 'Food', 1, 'https://images.unsplash.com/photo-1541519227354-08fa5d50c820?w=400&h=300&fit=crop', 0),

    (12, 'Pancake Stack',     'แพนเค้ก 3 ชั้น เนยสด น้ำผึ้ง ผลไม้สด',
         110.00, 'Food', 1, 'https://images.unsplash.com/photo-1528207776546-365bb710ee93?w=400&h=300&fit=crop', 0),

    (13, 'Pasta Carbonara',   'พาสต้าซอสครีม เบคอน ไข่แดง พาร์เมซาน',
         145.00, 'Food', 1, 'https://images.unsplash.com/photo-1612874742237-6526221588e3?w=400&h=300&fit=crop', 0),

    -- Bakery
    (14, 'Croissant',         'ครัวซองค์เนยแท้ กรอบนอกนุ่มใน อบสดใหม่ทุกวัน',
         65.00, 'Bakery', 1, 'https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=400&h=300&fit=crop', 0),

    (15, 'Blueberry Muffin',  'มัฟฟินบลูเบอร์รีรสเข้ม ชุ่มชื้น หอมอบ',
         70.00, 'Bakery', 1, 'https://images.unsplash.com/photo-1607958996333-41aef7caefaa?w=400&h=300&fit=crop', 0),

    (16, 'Cinnamon Roll',     'ซินนามอนโรลฟรอสติ้งครีมชีส หอมอ่อน หวานละมุน',
         80.00, 'Bakery', 1, 'https://images.unsplash.com/photo-1509365465985-25d11c17e812?w=400&h=300&fit=crop', 0),

    -- Seasonal (ตัวอย่าง)
    (17, 'Sakura Latte',      'ลาเต้ซากุระ รสหวานอ่อน กลิ่นดอกซากุระ Limited Edition',
         95.00, 'Coffee', 1, 'https://images.unsplash.com/photo-1566888596782-c7f41cc184c5?w=400&h=300&fit=crop', 1);


-- ============================================================
-- Ingredients — วัตถุดิบ
-- ============================================================
INSERT INTO Ingredients (IngredientId, IngredientName, StockQuantity, ReservedQty, Unit, ReorderLevel, CostPerUnit) VALUES
    (1,  'เมล็ดกาแฟ Arabica',   5000,  0,  'g',   500,  0.8000),
    (2,  'นมสด',                 20000, 0,  'ml',  2000, 0.0400),
    (3,  'น้ำตาลทราย',           10000, 0,  'g',   1000, 0.0200),
    (4,  'ผงมัทฉะ',              2000,  0,  'g',   200,  1.2000),
    (5,  'ผงโกโก้',              3000,  0,  'g',   300,  0.5000),
    (6,  'ไซรัปคาราเมล',         5000,  0,  'ml',  500,  0.0600),
    (7,  'วิปครีม',              8000,  0,  'ml',  800,  0.0500),
    (8,  'ขนมปังซาวร์โดว์',      50,    0,  'ชิ้น',10,   12.0000),
    (9,  'อะโวคาโด',             30,    0,  'ลูก', 5,    35.0000),
    (10, 'ไข่ไก่',               100,   0,  'ฟอง', 20,   4.0000),
    (11, 'แป้งสาลี',             10000, 0,  'g',   1000, 0.0300),
    (12, 'เนยสด',                5000,  0,  'g',   500,  0.1500),
    (13, 'ชาไทย',                3000,  0,  'g',   300,  0.2000),
    (14, 'น้ำมะนาว',             3000,  0,  'ml',  300,  0.0800),
    (15, 'น้ำเชื่อมกุหลาบ',     2000,  0,  'ml',  200,  0.0700);


-- ============================================================
-- Recipes — สูตรส่วนผสม (เฉพาะตัวอย่างหลัก)
-- ============================================================
INSERT INTO Recipes (RecipeId, MenuItemId, IngredientId, QuantityRequired, Unit) VALUES
    -- Espresso (1)
    (1,  1, 1, 18,  'g'),   -- เมล็ดกาแฟ 18g

    -- Latte (2)
    (2,  2, 1, 18,  'g'),   -- เมล็ดกาแฟ 18g
    (3,  2, 2, 200, 'ml'),  -- นมสด 200ml

    -- Cappuccino (3)
    (4,  3, 1, 18,  'g'),
    (5,  3, 2, 150, 'ml'),
    (6,  3, 7, 50,  'ml'),  -- วิปครีม 50ml

    -- Americano (4)
    (7,  4, 1, 18,  'g'),

    -- Cold Brew (5)
    (8,  5, 1, 30,  'g'),

    -- Caramel Macchiato (6)
    (9,  6, 1, 18,  'g'),
    (10, 6, 2, 200, 'ml'),
    (11, 6, 6, 20,  'ml'),  -- ไซรัปคาราเมล

    -- Matcha Latte (7)
    (12, 7, 4, 8,   'g'),   -- ผงมัทฉะ
    (13, 7, 2, 200, 'ml'),

    -- Chocolate Frappe (8)
    (14, 8, 5, 20,  'g'),   -- ผงโกโก้
    (15, 8, 2, 150, 'ml'),

    -- Thai Milk Tea (9)
    (16, 9, 13, 15, 'g'),   -- ชาไทย
    (17, 9, 2,  150,'ml'),

    -- Avocado Toast (11)
    (18, 11, 8, 2,  'ชิ้น'),
    (19, 11, 9, 0.5,'ลูก'),
    (20, 11, 10, 1, 'ฟอง');


-- ============================================================
-- Rewards — รางวัลแต้มสะสม
-- ============================================================
INSERT INTO Rewards (RewardId, RewardName, PointsRequired, StockQuantity, ImageUrl, IsActive) VALUES
    (1, 'เครื่องดื่มฟรี 1 แก้ว (ขนาด M)',       50, 20,
        'https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=400&h=300&fit=crop', 1),
    (2, 'เพิ่มขนาด Free (M → L)',                 20, 50,
        'https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=400&h=300&fit=crop', 1),
    (3, 'ขนมปัง Croissant ฟรี 1 ชิ้น',           30, 15,
        'https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=400&h=300&fit=crop', 1),
    (4, 'ส่วนลด ฿50 สำหรับออเดอร์ถัดไป',         40, NULL,
        'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=400&h=300&fit=crop', 1),
    (5, 'Matcha Latte ฟรี (มูลค่า ฿80)',          80, 10,
        'https://images.unsplash.com/photo-1589476993333-f55b84301219?w=400&h=300&fit=crop', 1);


-- ============================================================
-- Members ตัวอย่าง
-- ============================================================
-- MemberId ใช้ format BBNNNN: BB=69 (ปี พ.ศ. 2569), NNNN=Running Number
-- 690001=สมชาย, 690002=สมหญิง, 690003=วิทย์
INSERT INTO Members (MemberId, Phone, FirstName, LastName, BirthDate, Points, StampBalance) VALUES
    (690001, '0891234567', 'สมชาย', 'ใจดี',    '1990-05-15', 150, 4),
    (690002, '0812345678', 'สมหญิง','แก้วใส',  '1995-08-22', 80,  7),
    (690003, '0856789012', 'วิทย์', 'เก่งกาจ', '2000-12-01', 0,   0);
