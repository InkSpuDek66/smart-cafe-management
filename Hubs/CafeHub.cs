// Hubs/CafeHub.cs
// SignalR Hub สำหรับส่งข้อความ Real-time ระหว่าง Server กับ Client
// 5 Events: NotifyNewSlip, NotifyOrderPaid, NotifySlipRejected, NotifyOrderReady, NotifyOrderCancelled(Order ถูกยกเลิก)

using Microsoft.AspNetCore.SignalR; // ใช้สำหรับสร้าง Hub ที่ Client สามารถเชื่อมต่อและรับส่งข้อความแบบ Real-time

namespace Project_CSI402_T2_Y3.Hubs; // Hub นี้จะถูกใช้เพื่อส่งการแจ้งเตือนต่างๆ เช่น เมื่อมี Slip ใหม่, Order ถูกจ่ายเงิน, Slip ถูกปฏิเสธ, หรือ Order พร้อมเสิร์ฟ

public class CafeHub : Hub
{
    // Client เรียก JoinGroup เพื่อรับ Event เฉพาะกลุ่มของตัวเอง
    // เช่น พนักงาน Join "staff", ลูกค้าที่มีออเดอร์ Join "order-{orderId}"
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
