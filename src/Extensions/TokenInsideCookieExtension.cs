namespace Shipment.Extensions;

public static class TokenInsideCookieExtension
{
    public static void StoredTokenInCookie(this HttpContext httpContext, string cookieName, string token, DateTime expirations)
    {
        httpContext.Response.Cookies.Append(cookieName, token, new CookieOptions
        {
            Expires = expirations,
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
    }
}