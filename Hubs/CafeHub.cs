// Hubs/CafeHub.cs
// SignalR Hub สำหรับส่งข้อความ Real-time ระหว่าง Server กับ Client
// 4 Events: NotifyNewSlip, NotifyOrderPaid, NotifySlipRejected, NotifyOrderReady

using Microsoft.AspNetCore.SignalR;

namespace Project_CSI402_T2_Y3.Hubs;

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
