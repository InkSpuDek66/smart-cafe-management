// Helpers/SessionExtensions.cs
// Extension methods สำหรับ Session — รองรับการเก็บ/ดึง Object เป็น JSON

using System.Text.Json;

namespace Project_CSI402_T2_Y3.Helpers;

public static class SessionExtensions
{
    // บันทึก Object ลง Session โดย Serialize เป็น JSON ก่อน
    public static void SetJson<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    // ดึง Object จาก Session โดย Deserialize จาก JSON
    public static T? GetJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
