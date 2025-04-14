using WebTimNguoiThatLac.Models;
namespace WebTimNguoiThatLac.BoTro
{
    public class ThongTinEmail
    {
        public static string TieuDeBinhLuanMoi = "Thông Báo Bài Viết Tìm Người Có Bình Luận Mới";
        
        public static string NoiDungBinhLuanMoi(DateTime? NgayBinhLuan, string? TenNguoiCanTim, string? TenNguoiBinhLuan, string? NoiDungBinhLuan)
        {
            string ngayBinhLuan = NgayBinhLuan.HasValue ? NgayBinhLuan.Value.ToString("dd/MM/yyyy") : "không xác định";
            return $"Kính gửi Quý độc giả,\n\n" +
                   $"Bạn có Bài đăng tìm người thất lạc {TenNguoiCanTim},\n\n"+
                   $"Chúng tôi xin thông báo rằng có một bình luận mới được đăng vào ngày {ngayBinhLuan} từ {TenNguoiBinhLuan}.\n" +
                   $"Nội dung bình luận: \"{NoiDungBinhLuan}\"\n\n" +
                   $"Trân trọng,\n" +
                   $"Đội ngũ quản lý";
        }

        public static string TieuDeDangBaiThanhCong = "Thông Báo Bài Viết Tìm Người Đã Đước Đăng Bài Thành Công";
        public static string NoiDungDangBaiThanhCong(TimNguoi x)
        {
            if (x == null)
            {
                return "Thông báo: Thông tin người tìm không hợp lệ. Vui lòng kiểm tra lại và thử lại.";
            }

            string hoTen = x.HoTen ?? "Không xác định";
            string tieuDe = x.TieuDe ?? "Không có tiêu đề";
            string moTa = x.MoTa ?? "Không có mô tả";
            string khuVuc = x.KhuVuc ?? "Không xác định khu vực";
            string trangThai = x.TrangThai ?? "Không có trạng thái";
            string ngayDang = x.NgayDang.ToString("dd/MM/yyyy HH:mm:ss");
            string gioiTinh = (x.GioiTinh == 1) ? "Nam" : (x.GioiTinh == 2) ? "Nữ" : "Không xác định";
            string sdt = x.ApplicationUser.PhoneNumber ?? "Chưa Cập Nhật";
            string email = x.ApplicationUser.Email ?? "Chưa Cập Nhật";

            return $"Kính gửi Quý Khách,\n\n" +
                   $"Chúng tôi xin thông báo rằng bài viết của bạn đã được đăng thành công trên hệ thống của chúng tôi.\n\n" +
                   $"--- Thông tin bài viết ---\n" +
                   $"- Người tìm: {hoTen}\n" +
                   $"- Tiêu đề bài viết: {tieuDe}\n" +
                   $"- Mô tả chi tiết: {moTa}\n" +
                   $"- Khu vực tìm kiếm: {khuVuc}\n" +
                   $"- Trạng thái hiện tại: {trangThai}\n" +
                   $"- Giới tính: {gioiTinh}\n" +
                   $"- Ngày giờ đăng: {ngayDang}\n\n" +
                   $"Chúng tôi xin chân thành cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi. " +
                   $"Chúng tôi hiểu rằng việc tìm kiếm người thân là một quá trình khó khăn và nhạy cảm, " +
                   $"và chúng tôi cam kết sẽ hỗ trợ bạn hết mình trong hành trình này.\n\n" +
                   $"Để giúp bạn có được trải nghiệm tốt nhất, chúng tôi khuyến khích bạn thường xuyên kiểm tra trạng thái của bài viết " +
                   $"cũng như phản hồi từ cộng đồng. Mỗi thông tin mà bạn cung cấp đều có thể là manh mối quý giá cho việc tìm kiếm.\n\n" +
                   $"Nếu bạn cần thêm thông tin hoặc có bất kỳ câu hỏi nào, " +
                   $"xin vui lòng liên hệ với chúng tôi qua các kênh hỗ trợ sau:\n" +
                   $"- Email: {email}\n" +
                   $"- Số điện thoại: {sdt}\n" +
                   $"- Website: www.webtimnguoi.com\n\n" +
                   $"Chúng tôi luôn sẵn sàng lắng nghe và hỗ trợ bạn. Hãy yên tâm rằng chúng tôi sẽ đồng hành cùng bạn trong từng bước của quá trình này.\n\n" +
                   $"Chúc bạn may mắn trong việc tìm kiếm và cảm ơn bạn đã tin tưởng chúng tôi!\n\n" +
                   $"Trân trọng,\n" +
                   $"Đội ngũ hỗ trợ Web Tìm Người\n" +
                   $"----------------------------------------------------\n" +
                   $"*Lưu ý: Để đảm bảo thông tin của bạn được bảo mật, xin vui lòng không chia sẻ thông tin cá nhân " +
                   $"cho bất kỳ ai không đáng tin cậy. Chúng tôi cam kết bảo vệ quyền riêng tư của bạn và sẽ không tiết lộ thông tin " +
                   $"của bạn cho bên thứ ba mà không có sự đồng ý của bạn.";
        }

        public static string TieuDeDangBaiThatBai = "Thông Báo Bài Viết Tìm Người Đã Đước Đăng Bài Thất Bại";
        public static string NoiDungDangBaiThatBai(TimNguoi x)
        {
            if (x == null)
            {
                return "Thông báo: Thông tin người tìm không hợp lệ. Vui lòng kiểm tra lại và thử lại.";
            }

            string hoTen = x.HoTen ?? "Không xác định";
            string tieuDe = x.TieuDe ?? "Không có tiêu đề";
            string ngayDang = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            string gioiTinh = (x.GioiTinh == 1) ? "Nam" : (x.GioiTinh == 2) ? "Nữ" : "Không xác định";
            string sdt = x.ApplicationUser.PhoneNumber ?? "Chưa Cập Nhật";
            string email = x.ApplicationUser.Email ?? "Chưa Cập Nhật";

            return $"Kính gửi Quý Khách,\n\n" +
                   $"Chúng tôi xin trân trọng thông báo rằng việc đăng bài viết của Quý Khách đã không thành công do một số lý do kỹ thuật.\n\n" +
                   $"--- Thông tin bài viết ---\n" +
                   $"- Người tìm: {hoTen}\n" +
                   $"- Tiêu đề bài viết: {tieuDe}\n" +
                   $"- Ngày giờ đăng: {ngayDang}\n" +
                   $"- Giới tính: {gioiTinh}\n\n" +
                   $"Chúng tôi rất tiếc về sự bất tiện này và khuyến khích Quý Khách thử lại. " +
                   $"Nếu Quý Khách gặp khó khăn trong quá trình đăng bài, xin vui lòng liên hệ với chúng tôi để được hỗ trợ tận tình.\n\n" +
                   $"Các kênh liên hệ:\n" +
                   $"- Email: {email}\n" +
                   $"- Số điện thoại: {sdt}\n\n" +
                   $"Chúng tôi luôn sẵn sàng lắng nghe và hỗ trợ Quý Khách. Xin chân thành cảm ơn Quý Khách đã tin tưởng và sử dụng dịch vụ của chúng tôi.\n\n" +
                   $"Trân trọng,\n" +
                   $"Đội ngũ hỗ trợ Web Tìm Người\n" +
                   $"----------------------------------------------------\n" +
                   $"*Lưu ý: Để đảm bảo thông tin của Quý Khách được bảo mật, xin vui lòng không chia sẻ thông tin cá nhân " +
                   $"cho bất kỳ ai không đáng tin cậy. Chúng tôi cam kết bảo vệ quyền riêng tư của Quý Khách và sẽ không tiết lộ thông tin " +
                   $"của Quý Khách cho bên thứ ba mà không có sự đồng ý của Quý Khách.";
        }


    }
}
