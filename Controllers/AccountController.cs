// Controllers/AccountController.cs
// จัดการระบบบัญชีผู้ใช้: Login พนักงาน, สมัครสมาชิก (ลูกค้าเท่านั้น), เพิ่มพนักงาน, ดูรายชื่อสมาชิก

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_CSI402_T2_Y3.Models.Db;
using Project_CSI402_T2_Y3.ViewModels;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Project_CSI402_T2_Y3.Controllers;

public class AccountController : Controller
{
    private readonly Csi402dbContext _db;

    public AccountController(Csi402dbContext db)
    {
        _db = db;
    }

    // ตรวจสอบว่า Login แล้วหรือยัง
    private bool IsLoggedIn() => HttpContext.Session.GetInt32("StaffRoleId").HasValue;

    // ========== LOGIN พนักงาน ==========

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel data)
    {
        // normalize input เป็น lowercase ก่อนเปรียบเทียบ เพราะ username ในฐานข้อมูลเก็บเป็น lowercase เสมอ
        var username = data.Username?.Trim().ToLower() ?? "";

        var staff = _db.Staff.FirstOrDefault(s => s.Username == username);

        // ตรวจสอบ: ต้องพบพนักงาน, ยังทำงานอยู่, และรหัสผ่านถูกต้อง
        if (staff == null || staff.IsActive != (ulong)1)
        {
            ViewBag.ErrorMessage = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
            return View(data);
        }

        if (staff.PasswordHash != HashPassword(data.Password))
        {
            ViewBag.ErrorMessage = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
            return View(data);
        }

        // บันทึกข้อมูลพนักงานลง Session หลังจาก Login สำเร็จ
        HttpContext.Session.SetInt32("StaffId", staff.StaffId);
        HttpContext.Session.SetString("Username", staff.Username ?? "");
        HttpContext.Session.SetInt32("StaffRoleId", staff.StaffRoleId ?? 0);
        HttpContext.Session.SetString("FullName", $"{staff.FirstName} {staff.LastName}");

        return RedirectToAction("Index", "Home");
    }

    // ล้าง Session และออกจากระบบ
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }

    // ========== สมัครสมาชิก (ลูกค้าเท่านั้น) ==========

    [HttpGet]
    public IActionResult Register()
    {
        // ถ้า Staff Login อยู่แล้ว ห้ามเข้าหน้านี้ เพราะ Staff ไม่มีสิทธิ์เพิ่ม Member
        if (IsLoggedIn())
        {
            TempData["Error"] = "พนักงานไม่มีสิทธิ์เพิ่มสมาชิกผ่านช่องทางนี้";
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public IActionResult Register(MemberViewModel data)
    {
        // ป้องกัน Staff ใช้งาน Endpoint นี้ผ่าน POST ตรง
        if (IsLoggedIn())
            return Forbid();

        // Generate MemberId อัตโนมัติในรูปแบบ BBNNNN
        int memberId;
        using (var tx = _db.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            memberId = GenerateId(_db.Members.Select(m => m.MemberId));
            tx.Commit();
        }

        var member = new Member
        {
            MemberId   = memberId,
            FirstName  = data.FirstName,
            LastName   = data.LastName,
            Phone      = data.Phone,
            BirthDate  = DateOnly.TryParse(data.BirthDate, out DateOnly bd) ? bd : null,
            Points     = 0,
            StampBalance = 0
        };

        _db.Members.Add(member);
        _db.SaveChanges();

        // หลังสมัครสำเร็จ ไปหน้า Login เพื่อให้ลูกค้าเข้าสู่ระบบ
        TempData["Success"] = $"สมัครสมาชิกสำเร็จ รหัสสมาชิกของคุณคือ {memberId}";
        return RedirectToAction("Login", "Account");
    }

    // ========== เพิ่มพนักงาน (เฉพาะ Staff ที่ Login แล้ว) ==========

    // ตรวจสอบ Role — เฉพาะ Store Manager (3) และ Owner (5) เพิ่มพนักงานได้
    private bool CanManageStaff()
    {
        var roleId = HttpContext.Session.GetInt32("StaffRoleId");
        return roleId == 3 || roleId == 5;
    }

    [HttpGet]
    public IActionResult AddStaff()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!CanManageStaff())
        {
            TempData["Error"] = "คุณไม่มีสิทธิ์เพิ่มพนักงาน";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Roles = _db.Staffroles.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult AddStaff(StaffViewModel data)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!CanManageStaff())
        {
            TempData["Error"] = "คุณไม่มีสิทธิ์เพิ่มพนักงาน";
            return RedirectToAction("Index", "Home");
        }

        // ตรวจสอบ Username ไม่ให้ซ้ำ
        if (_db.Staff.Any(s => s.Username == data.Username))
        {
            ViewBag.ErrorMessage = "Username นี้มีอยู่แล้ว กรุณาใช้ชื่ออื่น";
            ViewBag.Roles = _db.Staffroles.ToList();
            return View(data);
        }

        // Generate StaffId อัตโนมัติในรูปแบบ BBNNNN
        int staffId;
        using (var tx = _db.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            staffId = GenerateId(_db.Staff.Select(s => s.StaffId));
            tx.Commit();
        }

        var staff = new Staff
        {
            StaffId      = staffId,
            FirstName    = data.FirstName,
            LastName     = data.LastName,
            Username     = data.Username,
            PasswordHash = HashPassword(data.Password),
            StaffRoleId  = int.TryParse(data.StaffRoleId, out int roleId) ? roleId : null,
            IsActive     = (ulong)1
        };

        _db.Staff.Add(staff);
        _db.SaveChanges();

        ViewBag.SuccessMessage = $"เพิ่มพนักงานสำเร็จ รหัสพนักงาน: {staffId}";
        ViewBag.Roles = _db.Staffroles.ToList();
        return View();
    }

    // ========== รายชื่อสมาชิก (อ่านอย่างเดียว) ==========

    public IActionResult MemberList()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

        var members = _db.Members.OrderBy(m => m.MemberId).ToList();
        return View(members);
    }

    // ========== Helper ==========

    // แปลงรหัสผ่านเป็น SHA-256 Hash ก่อนบันทึกลงฐานข้อมูล
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    // Generate ID อัตโนมัติในรูปแบบ BBNNNN (6 หลัก)
    // BB   = 2 หลักสุดท้ายของปี พ.ศ. (AD + 543) เช่น ปี 2569 → BB = 69
    // NNNN = Running Number 4 หลัก เริ่มจาก 0001 ถึง 9999
    // ตัวอย่าง: ID แรกของปี 2569 = 690001, ID สุดท้าย = 699999
    // ค่าสูงสุดที่เป็นไปได้คือ 999999 ซึ่งอยู่ในช่วง INT (max ~2.1 พันล้าน) สบาย
    // ต้องเรียกใช้ภายใน Serializable Transaction เท่านั้น เพื่อป้องกัน ID ซ้ำ
    private static int GenerateId(IQueryable<int> existingIds)
    {
        int buddhistYear = DateTime.Now.Year + 543;
        int prefix       = (buddhistYear % 100) * 10_000; // เช่น 69 * 10000 = 690000
        int prefixEnd    = prefix + 10_000;               // ค่าสูงสุดของปีนี้ = 699999

        // หา ID สูงสุดในปีปัจจุบัน แล้วเพิ่ม 1
        int maxId = existingIds
            .Where(id => id >= prefix && id < prefixEnd)
            .Max(id => (int?)id) ?? prefix;

        int nextId = maxId + 1;

        if (nextId >= prefixEnd)
            throw new InvalidOperationException($"Running Number เต็มสำหรับปี พ.ศ. {buddhistYear}");

        return nextId;
    }
}
