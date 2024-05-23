using Microsoft.AspNetCore.Mvc;

public class ThemeController : Controller
{
    [HttpPost]
    public IActionResult SetTheme(string theme)
    {
        //append daje nowe ciasteczko do kolekcji, "Theme" to nazwa przesylanego ciasteczka, theme to jego wartosc,
        //CookieOptions to obiekt konfiguracyjny ktory okresla opcje, w naszym przypadk Expire (czyli nasze ciasteczko jest wazne rok)
        Response.Cookies.Append("Theme", theme, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

        //służy do przekierowania klienta na inny adres URL. W tym przypadku jest przekierowywany na
        //adres URL poprzedniej strony, z której zostało wykonane żądanie.
        //Referer zawiera adres URL poprzedniej strony na ktorym zostalo wykonane zadanie
        return Redirect(Request.Headers["Referer"].ToString());
    }
}
