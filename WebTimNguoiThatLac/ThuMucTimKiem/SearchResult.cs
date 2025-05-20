namespace WebTimNguoiThatLac.ThuMucTimKiem
{
    public class SearchResult
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string NhanDang { get; set; }
        public DateTime? NgayMatTich { get; set; }

        public int? GioiTinh { get; set; }
        public string Name { get; set; }
        public DateTime PostedDate { get; set; }
        public string ImageUrl { get; set; }
        public float Similarity { get; set; }

    }
}
