// Models/UserActivityLog.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("UserActivityLog")] // Tên bảng trong cơ sở dữ liệu
    public class UserActivityLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(450)]  // Độ dài phù hợp cho UserId (Identity)
        public string? UserId { get; set; }  // Null nếu là khách (chưa đăng nhập)

        [MaxLength(256)]
        public string? UserName { get; set; } // Tên người dùng hoặc "Khách"

        [Required]
        [MaxLength(100)]
        public string? SessionId { get; set; } // Theo dõi phiên làm việc

        [MaxLength(100)]
        public string? AnonymousId { get; set; } // ID tạm thời cho khách

        [Required]
        [MaxLength(50)]
        public string? Action { get; set; } // GET, POST, PUT, DELETE

        [MaxLength(50)]
        public string? Controller { get; set; } // Tên controller (nếu có)

        [MaxLength(50)]
        public string? ActionMethod { get; set; } // Tên action (nếu có)

        [MaxLength(50)]
        public string? ActionName { get; set; } // Tên action (bổ sung cho middleware)

        [MaxLength(500)]
        public string? IpAddress { get; set; } // Địa chỉ IP người dùng

        [MaxLength(500)]
        public string? UserAgent { get; set; } // Trình duyệt/thiết bị

        [MaxLength(500)]
        public string? Url { get; set; } // Đường dẫn truy cập

        [MaxLength(500)]
        public string? RequestPath { get; set; } // Đường dẫn request (bổ sung cho middleware)

        [MaxLength(2000)]
        public string? QueryString { get; set; } // Query string (bổ sung cho middleware)

        public string? AdditionalData { get; set; } // JSON chứa QueryString, Headers

        [MaxLength(4000)]
        public string? Headers { get; set; } // Headers (bổ sung cho middleware)

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Thời gian ghi log
    }
}