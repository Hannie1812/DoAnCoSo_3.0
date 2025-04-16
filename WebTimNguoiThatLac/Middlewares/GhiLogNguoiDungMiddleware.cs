namespace WebTimNguoiThatLac.Middlewares
{
    public class GhiLogNguoiDungMiddleware // Ghi log người dùng
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GhiLogNguoiDungMiddleware> _logger;

        public GhiLogNguoiDungMiddleware(RequestDelegate next, ILogger<GhiLogNguoiDungMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var tenNguoiDung = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Khách";
            var duongDan = context.Request.Path;
            var phuongThuc = context.Request.Method;
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var thoiGian = DateTime.UtcNow;

            _logger.LogInformation($"[{thoiGian}] Người dùng: {tenNguoiDung}, Phương thức: {phuongThuc}, Đường dẫn: {duongDan}, IP: {ip}");

            await _next(context);
        }
    }
}
