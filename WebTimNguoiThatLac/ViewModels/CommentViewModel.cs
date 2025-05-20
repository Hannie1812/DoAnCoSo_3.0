using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.ViewModels
{
    public class CommentViewModel
    {
        public BinhLuan Comment { get; set; }
        public TimNguoi Model { get; set; }
        public string UserId { get; set; }
        public bool IsAuthor { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public int Depth { get; set; }
    }
}
