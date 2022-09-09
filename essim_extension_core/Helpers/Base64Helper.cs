using System;
using System.Text;

namespace essim_extension_core.Helpers
{
    public static class Base64Helper
    {
        public static string ToBase64(this string content)
        {
            if (content.IsBase64()) return content; //Don't encode encoded content
            
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(contentBytes);
        }

        public static bool IsBase64(this string content)
        {
            Span<byte> buffer = new Span<byte>(new byte[content.Length]);
            return Convert.TryFromBase64String(content, buffer , out int _);
        }
    }
}
