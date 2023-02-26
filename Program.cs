using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;


namespace AspClaim2App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            var app = builder.Build();

            app.UseAuthentication();

            app.MapGet("/login/{username}",
                async (string username, HttpContext context) => 
                {
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.MobilePhone, "+79001002030"),
                        new Claim("company", "Yandex")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await context.SignInAsync(claimsPrincipal);
                    return $"{claims[0].Issuer} sign in {username}";
                });

            app.MapGet("/phone", async (HttpContext context) =>
            {
                if(context.User.Identity is ClaimsIdentity claimsIdentity)
                {
                    var phoneClaim = claimsIdentity.FindFirst(ClaimTypes.MobilePhone);
                    if(claimsIdentity.TryRemoveClaim(phoneClaim))
                    {
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await context.SignInAsync(claimsPrincipal);
                    }
                }
            });

            app.Map("/", (HttpContext context) =>
            {
                var user = context.User;
                if (user is not null && user.Identity!.IsAuthenticated)
                {
                    return @$"User name: {user.FindFirst(ClaimTypes.Name)?.Value}
User phone: {user.FindFirst(ClaimTypes.MobilePhone)?.Value}
User Company: {user.FindFirst("company")?.Value}";
                }
                else
                    return $"User not auth";
            });

            app.MapGet("/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return "Log out";
            });

            app.Run();
        }
    }
}