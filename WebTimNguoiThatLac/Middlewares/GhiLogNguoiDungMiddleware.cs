using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebTimNguoiThatLac.Data;
using System.Security.Claims;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Middlewares
{
    public class GhiLogNguoiDungMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GhiLogNguoiDungMiddleware> _logger;

        public GhiLogNguoiDungMiddleware(
            RequestDelegate next,
            ILogger<GhiLogNguoiDungMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Bỏ qua nếu là request không cần ghi log
            if (ShouldSkipLogging(context))
            {
                await _next(context);
                return;
            }

            // Thu thập thông tin cơ bản trước
            var logInfo = new
            {
                UserId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName = context.User?.Identity?.Name ?? "Khách",
                SessionId = context.Session?.Id ?? await TaoSessionIdMoiAsync(context),
                AnonymousId = await LayHoacTaoAnonymousIdAsync(context),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                RequestMethod = context.Request.Method,
                RequestPath = context.Request.Path,
                ControllerName = context.Request.RouteValues["controller"]?.ToString(),
                ActionName = context.Request.RouteValues["action"]?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Ghi log console
            _logger.LogInformation(
                "[{ThoiGian}] User: {UserId}/{UserName}, Session: {SessionId}, IP: {IpAddress}, Action: {Method} {Path}",
                logInfo.Timestamp, logInfo.UserId, logInfo.UserName, logInfo.SessionId,
                logInfo.IpAddress, logInfo.RequestMethod, logInfo.RequestPath);

            // Lưu lại thông tin cần thiết trước khi vào Task.Run để tránh truy cập context đã dispose
            var queryStringValue = context.Request.QueryString.Value;
            var headersDict = context.Request.Headers
                .Where(h => !h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(h => h.Key, h => h.Value.ToString());
            var serviceProvider = context.RequestServices;

            // Ghi log database trong scope riêng (bất đồng bộ)
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var logEntry = new UserActivityLog
                        {
                            UserId = logInfo.UserId,
                            UserName = logInfo.UserName,
                            SessionId = logInfo.SessionId,
                            AnonymousId = logInfo.AnonymousId,
                            Action = logInfo.RequestMethod,
                            Controller = logInfo.ControllerName,
                            ActionMethod = logInfo.ActionName,
                            IpAddress = logInfo.IpAddress,
                            UserAgent = logInfo.UserAgent,
                            Url = $"{logInfo.RequestMethod} {logInfo.RequestPath}",
                            AdditionalData = JsonSerializer.Serialize(new
                            {
                                QueryString = queryStringValue,
                                Headers = headersDict
                            }),
                            Timestamp = logInfo.Timestamp
                        };

                        await dbContext.UserActivityLogs.AddAsync(logEntry);
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi ghi log vào database");
                }
            });

            await _next(context);
        }

        private bool ShouldSkipLogging(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            if (string.IsNullOrEmpty(path)) return true;

            return path.StartsWith("/_blazor") ||
                   path.StartsWith("/css/") ||
                   path.StartsWith("/js/") ||
                   path.StartsWith("/lib/") ||
                   path.EndsWith(".png") ||
                   path.EndsWith(".jpg") ||
                   path.EndsWith(".jpeg") ||
                   path.EndsWith(".gif") ||
                   path.EndsWith(".css") ||
                   path.EndsWith(".js") ||
                   path.EndsWith(".ico") ||
                   path.EndsWith(".svg");
        }

        private async Task<string> TaoSessionIdMoiAsync(HttpContext context)
        {
            var sessionId = Guid.NewGuid().ToString("N");

            // Khởi tạo session nếu chưa có
            if (context.Session != null && !context.Session.IsAvailable)
            {
                await context.Session.LoadAsync();
            }

            context.Session?.SetString("SessionId", sessionId);

            context.Response.Cookies.Append("SessionId", sessionId, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps
            });

            return sessionId;
        }

        private async Task<string> LayHoacTaoAnonymousIdAsync(HttpContext context)
        {
            var anonymousId = context.Request.Cookies["AnonymousId"];
            if (string.IsNullOrEmpty(anonymousId))
            {
                anonymousId = $"anon-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                context.Response.Cookies.Append("AnonymousId", anonymousId, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(365),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = context.Request.IsHttps
                });
            }
            return anonymousId;
        }
    }
}