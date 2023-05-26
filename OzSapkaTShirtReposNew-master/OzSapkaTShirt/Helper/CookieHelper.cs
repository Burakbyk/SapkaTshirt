using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OzSapkaTShirt2.Helper
{
    public static class CookieHelper
    {
        public static void SetCookie<T>(HttpContext context, string key, T value)
        {
            context.Response.Cookies.Append(key, JsonSerializer.Serialize(value));
        }

        public static string GetObject(HttpContext context, string key)
        {
            var value = context.Request.Cookies[key];
            return value == null ? default : value;
        }
    }
}
