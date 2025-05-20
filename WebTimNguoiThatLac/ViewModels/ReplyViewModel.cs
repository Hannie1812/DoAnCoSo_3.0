namespace WebTimNguoiThatLac.ViewModels
{
    public class ReplyViewModel
    {
        public string? NoiDung { get; set; }

        public IFormFile? HinhAnh { get; set; }
        public int? ParentCommentId { get; set; }
        public int? PostId { get; set; }
    }
}
