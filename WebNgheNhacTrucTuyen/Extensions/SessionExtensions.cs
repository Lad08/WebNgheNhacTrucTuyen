using System.Text.Json;

namespace WebNgheNhacTrucTuyen.Extensions
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            var jsonData = JsonSerializer.Serialize(value);
            session.SetString(key, jsonData);
        }

        public static T Get<T>(this ISession session, string key)
        {
            var jsonData = session.GetString(key);
            return jsonData == null ? default : JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}
