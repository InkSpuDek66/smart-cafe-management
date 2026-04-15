-- ============================================================
-- Migration Script — รันหลังจาก SQLQuery_project.sql แล้ว
-- ใช้สำหรับอัปเดต Schema ที่เพิ่มเติมในระหว่างการพัฒนา
-- ============================================================

USE CSI402DB;


-- ============================================================
-- [2026-04-14] Promotions seed ทั้ง 5 + SeasonEndDate สำหรับ Sakura Latte
-- รัน safe ซ้ำได้ทุกครั้งเพราะใช้ ON DUPLICATE KEY UPDATE
-- ============================================================

-- ตั้งวันหมดอายุ Sakura Latte ให้ใช้งานได้ถึงปี 2027
UPDATE MenuItems
SET SeasonStartDate = '2025-03-01',
    SeasonEndDate   = '2027-12-31'
WHERE MenuItemId = 17;

-- Promotions ทั้ง 5 — ลบก่อนแล้ว INSERT ใหม่ เพื่อให้รันซ้ำได้โดยไม่ error
DELETE FROM Promotions WHERE PromotionId IN (1, 2, 3, 4, 5);

INSERT INTO Promotions (PromotionId, PromotionName, ConditionType, ConditionValue, RewardType, RewardValue, IsActive, StartDate, EndDate)
VALUES
    (1, 'สะสมแต้ม 10 บาท = 1 แต้ม',
        NULL,           NULL,   'PointEarn',    '1',   1, NULL,         NULL),
    (2, 'เมนู Seasonal ตามเทศกาล',
        'Seasonal',     NULL,   'SeasonalMenu', '',    1, '2025-03-01', '2027-12-31'),
    (3, 'ยอดครบ 300 บาท รับ Chocolate Chip Cookie ฟรี 1 ชิ้น',
        'MinAmount',    '300',  'FreeItem',     '18',  1, NULL,         NULL),
    (4, 'Stamp Card — ครบ 10 ดวง รับเครื่องดื่มฟรี 1 แก้ว',
        'StampCount',   '10',   'FreeDrink',    '1',   1, NULL,         NULL),
    (5, 'Group Check-in 5 คนขึ้นไป รับเครื่องดื่มฟรี 1 แก้ว',
        'GroupCheckin', '5',    'Discount',     '75',  1, NULL,         NULL);