// Controllers/MenuController.cs
// จัดการการแสดงรายการเมนูและกรองตามหมวดหมู่

using Microsoft.AspNetCore.Mvc;
using Project_CSI402_T2_Y3.Models.Db;

namespace Project_CSI402_T2_Y3.Controllers;

public class MenuController : Controller
{
    private readonly Csi402dbContext _db;

    public MenuController(Csi402dbContext db)
    {
        _db = db;
    }

    // แสดงรายการเมนูทั้งหมด กรองตาม Category ได้ผ่าน Query String ?category=Coffee
    public IActionResult Index(string? category)
    {
        var query = _db.Menuitems.AsQueryable();

        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(m => m.Category == category);
        }

        var menuItems = query.OrderBy(m => m.Category).ThenBy(m => m.MenuName).ToList();

        // ดึง Category ที่มีในระบบเพื่อสร้างปุ่มกรอง
        var categories = _db.Menuitems
            .Where(m => m.Category != null)
            .Select(m => m.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        ViewBag.SelectedCategory = string.IsNullOrEmpty(category) ? "All" : category;
        ViewBag.Categories = categories;

        return View(menuItems);
    }
}
