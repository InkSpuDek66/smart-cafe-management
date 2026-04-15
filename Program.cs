// Program.cs
// จุดเริ่มต้นของแอปพลิเคชัน — ลงทะเบียน Services และกำหนด Middleware Pipeline

using Project_CSI402_T2_Y3.Hubs;
using Project_CSI402_T2_Y3.Models.Db;

var builder = WebApplication.CreateBuilder(args);

// ลงทะเบียน MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// ลงทะเบียน Database Context — Connection String อยู่ใน OnConfiguring ของ Csi402dbContext
builder.Services.AddDbContext<Csi402dbContext>();

// ลงทะเบียน SignalR สำหรับ Real-time notification
builder.Services.AddSignalR();

// ลงทะเบียน In-Memory Cache สำหรับ Session (จำเป็นต้องมีก่อน AddSession)
builder.Services.AddDistributedMemoryCache();

// ลงทะเบียน Session สำหรับเก็บข้อมูลหลัง Login
builder.Services.AddSession(options =>
{
    // 20 นาที — ครอบ flow ทั้งหมด: เปิดเมนู → เลือก → ชำระ → รอรับสินค้า
    // การ logout จริงทำผ่าน JS countdown (NotifyOrderReady) ไม่ใช่ session timeout
    options.IdleTimeout        = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// เปิดใช้งาน Session ก่อน Authorization
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

// Map SignalR Hub ที่ path /hubs/cafe
app.MapHub<CafeHub>("/hubs/cafe");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
