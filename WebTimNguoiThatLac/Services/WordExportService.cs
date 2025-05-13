//using DocumentFormat.OpenXml;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Wordprocessing;
//using HtmlToOpenXml;
//using System.IO;
//using WebTimNguoiThatLac.Models;


//namespace WebTimNguoiThatLac.Services
//{
//    public class WordExportService
//    {
//        public byte[] CreateOfficialReport(TimNguoi timNguoi, ApplicationUser user)
//        {
//            using (var memoryStream = new MemoryStream())
//            {
//                using (var wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
//                {
//                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
//                    mainPart.Document = new Document();
//                    Body body = mainPart.Document.AppendChild(new Body());

//                    // 1. QUỐC HIỆU TIÊU NGỮ
//                    AddQuocHieu(body);

//                    // 2. TIÊU ĐỀ ĐƠN
//                    AddTitle(body, "ĐƠN TRÌNH BÁO MẤT TÍCH",
//                        $"(V/v: Ông/Bà {timNguoi.HoTen} mất tích từ ngày {timNguoi.NgayMatTich?.ToString("dd/MM/yyyy")})");

//                    // 3. KÍNH GỬI
//                    AddRecipient(body);

//                    // 4. THÔNG TIN NGƯỜI LÀM ĐƠN
//                    AddPetitionerInfo(body, user);

//                    // 5. THÔNG TIN NGƯỜI MẤT TÍCH
//                    AddMissingPersonInfo(body, timNguoi);

//                    // 6. NỘI DUNG TRÌNH BÀY (HTML ĐƠN GIẢN)
//                    AddHtmlContent(mainPart, body, timNguoi);

//                    // 7. CAM KẾT
//                    AddCommitment(body);

//                    // 8. KÝ TÊN
//                    AddSignature(body, user);

//                    wordDoc.Save();
//                }
//                return memoryStream.ToArray();
//            }
//        }

//        private void AddHtmlContent(MainDocumentPart mainPart, Body body, TimNguoi timNguoi)
//        {
//            if (string.IsNullOrEmpty(timNguoi.MoTa))
//                return;

//            // Tiêu đề phần nội dung
//            body.AppendChild(new Paragraph(
//                new Run(new Text("Sau đây, tôi xin trình bày sự việc như sau:"))
//                { RunProperties = new RunProperties(new Bold()) }
//            ));

//            try
//            {
//                var converter = new HtmlConverter(mainPart);



//                // Cấu hình style cơ bản
//                var paragraphs = converter.Parse(timNguoi.MoTa);

//                foreach (var paragraph in paragraphs)
//                {
//                    if (paragraph != null) body.AppendChild(paragraph);
//                }
//            }
//            catch (Exception ex)
//            {
//                // Fallback: Thêm nội dung dạng text thuần
//                body.AppendChild(new Paragraph(new Run(new Text(timNguoi.MoTa))));
//                Console.WriteLine($"Lỗi chuyển đổi HTML: {ex.Message}");
//            }

//            // Kết luận
//            body.AppendChild(new Paragraph(
//                new Run(new Text($"Vì vậy, tôi làm đơn này kính đề nghị quý cơ quan tiến hành thủ tục tìm kiếm ông/bà {timNguoi.HoTen}."))
//            ));
//        }

//        private void AddQuocHieu(Body body)
//        {
//            body.AppendChild(new Paragraph(
//                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
//                new Run(
//                    new Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
//                    { Space = SpaceProcessingModeValues.Preserve }
//                )
//                { RunProperties = new RunProperties(new Bold()) },
//                new Run(new Break()),
//                new Run(
//                    new Text("Độc lập - Tự do - Hạnh phúc")
//                    { Space = SpaceProcessingModeValues.Preserve }
//                )
//                { RunProperties = new RunProperties(new Bold()) },
//                new Run(new Break()),
//                new Run(
//                    new Text($"..., ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}")
//                )
//            ));
//        }

//        private void AddTitle(Body body, string title, string subtitle)
//        {
//            body.AppendChild(new Paragraph(
//                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
//                new Run(
//                    new Text(title)
//                )
//                { RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "28" }) },
//                new Run(new Break()),
//                new Run(
//                    new Text(subtitle)
//                )
//                { RunProperties = new RunProperties(new Italic()) }
//            ));
//        }

//        private void AddRecipient(Body body)
//        {
//            body.AppendChild(new Paragraph(
//                new Run(
//                    new Text("Kính gửi: CÔNG AN XÃ (PHƯỜNG, THỊ TRẤN)...")
//                )
//                { RunProperties = new RunProperties(new Bold()) }
//            ));
//        }

//        private void AddPetitionerInfo(Body body, ApplicationUser user)
//        {
//            string[] infoLines = {
//                $"Tên tôi là: {user.FullName}",
//                $"Sinh năm: {user.NgaySinh?.Year}",
//                $"CMND/CCCD số: {user.CCCD}",
//                $"Địa chỉ thường trú: {user.Address}",
//                $"Địa chỉ hiện tại: {user.Address}",
//                $"Số điện thoại: {user.PhoneNumber}"
//            };

//            foreach (var line in infoLines)
//            {
//                body.AppendChild(new Paragraph(new Run(new Text(line))));
//            }
//        }

//        private void AddMissingPersonInfo(Body body, TimNguoi timNguoi)
//        {
//            body.AppendChild(new Paragraph(
//                new Run(
//                    new Text($"Là: {timNguoi.MoiQuanHe} của ông/bà: {timNguoi.HoTen}")
//                )
//                { RunProperties = new RunProperties(new Bold()) }
//            ));

//            string[] infoLines = {
//                $"Sinh năm: {timNguoi.NgayMatTich?.Year}",
//                $"Đặc điểm nhận dạng: {timNguoi.DaciemNhanDang}",
//                $"Nơi thường trú: {"Cập nhật"}",
//                $"Khu vực mất tích: {timNguoi.KhuVuc}",
//                $"Ngày mất tích: {timNguoi.NgayMatTich?.ToString("dd/MM/yyyy")}"
//            };

//            foreach (var line in infoLines)
//            {
//                body.AppendChild(new Paragraph(new Run(new Text(line))));
//            }
//        }

//        private void AddCommitment(Body body)
//        {
//            body.AppendChild(new Paragraph(new Run(new Text(
//                "Tôi xin cam đoan những thông tin trên là đúng sự thật và chịu trách nhiệm trước pháp luật về tính chính xác của các thông tin đã cung cấp."
//            ))));

//            body.AppendChild(new Paragraph(new Run(new Text(
//                "Kính mong quý cơ quan xem xét, giải quyết. Tôi xin chân thành cảm ơn."
//            ))));
//        }

//        private void AddSignature(Body body, ApplicationUser user)
//        {
//            body.AppendChild(new Paragraph(
//                new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
//                new Run(
//                    new Text("NGƯỜI LÀM ĐƠN")
//                )
//                { RunProperties = new RunProperties(new Bold()) },
//                new Run(new Break()),
//                new Run(
//                    new Text("(Ký và ghi rõ họ tên)")
//                )
//                { RunProperties = new RunProperties(new Italic()) },
//                new Run(new Break()),
//                new Run(new Break()),
//                new Run(new Break()),
//                new Run(
//                    new Text(user.FullName)
//                )
//                { RunProperties = new RunProperties(new Bold()) }
//            ));
//        }


//        // Xác Nhận Tìm Thấy
//        public byte[] CreateFoundReport(TimThayNguoiThatLac timThay, ApplicationUser user)
//        {
//            using (var memoryStream = new MemoryStream())
//            {
//                using (var wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
//                {
//                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
//                    mainPart.Document = new Document();
//                    Body body = mainPart.Document.AppendChild(new Body());

//                    // 1. QUỐC HIỆU TIÊU NGỮ
//                    AddQuocHieu(body);

//                    // 2. TIÊU ĐỀ ĐƠN
//                    AddTitle(body, "ĐƠN XÁC NHẬN ĐÃ TÌM THẤY NGƯỜI MẤT TÍCH",
//                        $"(V/v: Ông/Bà {timThay.TimNguoi.HoTen} đã được tìm thấy)");

//                    // 3. KÍNH GỬI
//                    AddRecipient(body);

//                    // 4. THÔNG TIN NGƯỜI LÀM ĐƠN
//                    AddPetitionerInfo(body, user);

//                    // 5. THÔNG TIN NGƯỜI ĐÃ TÌM THẤY
//                    AddFoundPersonInfo(body, timThay);

//                    // 6. NỘI DUNG XÁC NHẬN (HỖ TRỢ HTML)
//                    AddFoundContent(mainPart, body, timThay);

//                    // 7. CAM KẾT
//                    AddFoundCommitment(body);

//                    // 8. KÝ TÊN
//                    AddSignature(body, user);

//                    wordDoc.Save();
//                }
//                return memoryStream.ToArray();
//            }
//        }


//        private void AddFoundPersonInfo(Body body, TimThayNguoiThatLac timThay)
//        {
//            body.AppendChild(new Paragraph(
//                new Run(
//                    new Text($"Là: {timThay.TimNguoi.MoiQuanHe} của ông/bà: {timThay.TimNguoi.HoTen}")
//                )
//                { RunProperties = new RunProperties(new Bold()) }
//            ));

//            string[] infoLines = {
//                $"Sinh năm: {timThay.TimNguoi.NgaySinh?.Year}",
//                $"Đặc điểm nhận dạng: {timThay.TimNguoi.DaciemNhanDang}",
//                $"Ngày mất tích: {timThay.TimNguoi.NgayMatTich?.ToString("dd/MM/yyyy")}",
//                $"Ngày tìm thấy: {timThay.NgayTimThay?.ToString("dd/MM/yyyy")}",
//                $"Địa điểm tìm thấy: {timThay.DiaDiemTimThay}",
//                $"Tình trạng sức khỏe: {timThay.TinhTrangSucKhoe}",
//                $"Người tìm thấy: {timThay.NguoiTimThay ?? "Không rõ"}"
//            };

//                foreach (var line in infoLines)
//                {
//                    body.AppendChild(new Paragraph(new Run(new Text(line))));
//                }
//        }

//        private void AddFoundContent(MainDocumentPart mainPart, Body body, TimThayNguoiThatLac timThay)
//        {
//            body.AppendChild(new Paragraph(
//                new Run(new Text("Sau đây, tôi xin xác nhận:"))
//                { RunProperties = new RunProperties(new Bold()) }
//            ));

//            // Thông tin cơ bản
//            body.AppendChild(new Paragraph(
//                new Run(new Text($"Ông/Bà {timThay.TimNguoi.HoTen} đã được tìm thấy vào ngày {timThay.NgayTimThay?.ToString("dd/MM/yyyy")}."))
//            ));

//            body.AppendChild(new Paragraph(
//                new Run(new Text($"Địa điểm tìm thấy: {timThay.DiaDiemTimThay}"))
//            ));

//            body.AppendChild(new Paragraph(
//                new Run(new Text($"Tình trạng sức khỏe: {timThay.TinhTrangSucKhoe}"))
//            ));

//            // Xử lý nội dung HTML nếu có
//            if (!string.IsNullOrEmpty(timThay.MoTaChiTiet))
//            {
//                body.AppendChild(new Paragraph(
//                    new Run(new Text("Chi tiết quá trình tìm thấy:"))
//                    { RunProperties = new RunProperties(new Bold()) }
//                ));

//                try
//                {
//                    var converter = new HtmlConverter(mainPart);
//                    var paragraphs = converter.Parse(timThay.MoTaChiTiet);

//                    foreach (var paragraph in paragraphs)
//                    {
//                        if (paragraph != null) body.AppendChild(paragraph);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    // Fallback: Thêm nội dung dạng text thuần nếu có lỗi
//                    body.AppendChild(new Paragraph(new Run(new Text(timThay.MoTaChiTiet))));
//                    Console.WriteLine($"Lỗi chuyển đổi HTML: {ex.Message}");
//                }
//            }
//        }

//        private void AddFoundCommitment(Body body)
//        {
//            body.AppendChild(new Paragraph(new Run(new Text(
//                "Tôi xin cam đoan những thông tin trên là đúng sự thật và chịu trách nhiệm trước pháp luật về tính chính xác của các thông tin đã cung cấp."
//            ))));

//            body.AppendChild(new Paragraph(new Run(new Text(
//                "Xin chân thành cảm ơn sự hỗ trợ của quý cơ quan trong thời gian qua."
//            ))));
//        }
//    }
//}

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlToOpenXml;
using System;
using System.IO;
using System.Linq;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Services
{
    public class WordExportService
    {
        // Kích thước font (half-points)
        private const int DefaultFontSize = 22;  // ~11pt
        private const int TitleFontSize = 28;    // ~14pt
        private const int HeaderFontSize = 24;   // ~12pt
        private const int SmallFontSize = 20;    // ~10pt

        public byte[] CreateOfficialReport(TimNguoi timNguoi, ApplicationUser user)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                {
                    var (mainPart, body) = InitializeDocument(wordDoc);

                    // 1. QUỐC HIỆU TIÊU NGỮ
                    AddQuocHieu(body);

                    // 2. TIÊU ĐỀ ĐƠN
                    AddTitle(body, "ĐƠN TRÌNH BÁO MẤT TÍCH",
                        $"(V/v: Ông/Bà {timNguoi.HoTen} mất tích từ ngày {timNguoi.NgayMatTich?.ToString("dd/MM/yyyy")})");

                    // 3. KÍNH GỬI
                    AddRecipient(body);

                    // 4. THÔNG TIN NGƯỜI LÀM ĐƠN
                    AddPetitionerInfo(body, user);

                    // 5. THÔNG TIN NGƯỜI MẤT TÍCH
                    AddMissingPersonInfo(body, timNguoi);

                    // 6. NỘI DUNG TRÌNH BÀY
                    AddHtmlContent(mainPart, body, timNguoi.MoTa,
                        "Sau đây, tôi xin trình bày sự việc như sau:",
                        $"Vì vậy, tôi làm đơn này kính đề nghị quý cơ quan tiến hành thủ tục tìm kiếm ông/bà {timNguoi.HoTen}.");

                    // 7. CAM KẾT
                    AddCommitment(body,
                        "Tôi xin cam đoan những thông tin trên là đúng sự thật và chịu trách nhiệm trước pháp luật về tính chính xác của các thông tin đã cung cấp.",
                        "Kính mong quý cơ quan xem xét, giải quyết. Tôi xin chân thành cảm ơn.");

                    // 8. KÝ TÊN
                    AddSignature(body, user);

                    wordDoc.Save();
                }
                return memoryStream.ToArray();
            }
        }

        public byte[] CreateFoundReport(TimThayNguoiThatLac timThay, ApplicationUser user)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                {
                    var (mainPart, body) = InitializeDocument(wordDoc);

                    // 1. QUỐC HIỆU TIÊU NGỮ
                    AddQuocHieu(body);

                    // 2. TIÊU ĐỀ ĐƠN
                    AddTitle(body, "ĐƠN XÁC NHẬN ĐÃ TÌM THẤY NGƯỜI MẤT TÍCH",
                        $"(V/v: Ông/Bà {timThay.TimNguoi.HoTen} đã được tìm thấy)");

                    // 3. KÍNH GỬI
                    AddRecipient(body);

                    // 4. THÔNG TIN NGƯỜI LÀM ĐƠN
                    AddPetitionerInfo(body, user);

                    // 5. THÔNG TIN NGƯỜI ĐÃ TÌM THẤY
                    AddFoundPersonInfo(body, timThay);

                    // 6. NỘI DUNG XÁC NHẬN
                    AddHtmlContent(mainPart, body, timThay.MoTaChiTiet,
                        "Sau đây, tôi xin xác nhận:",
                        $"Ông/Bà {timThay.TimNguoi.HoTen} đã được tìm thấy vào ngày {timThay.NgayTimThay?.ToString("dd/MM/yyyy")}.",
                        $"Địa điểm tìm thấy: {timThay.DiaDiemTimThay}",
                        $"Tình trạng sức khỏe: {timThay.TinhTrangSucKhoe}",
                        "Chi tiết quá trình tìm thấy:");

                    // 7. CAM KẾT
                    AddCommitment(body,
                        "Tôi xin cam đoan những thông tin trên là đúng sự thật và chịu trách nhiệm trước pháp luật về tính chính xác của các thông tin đã cung cấp.",
                        "Xin chân thành cảm ơn sự hỗ trợ của quý cơ quan trong thời gian qua.");

                    // 8. KÝ TÊN
                    AddSignature(body, user);

                    wordDoc.Save();
                }
                return memoryStream.ToArray();
            }
        }

        private (MainDocumentPart, Body) InitializeDocument(WordprocessingDocument wordDoc)
        {
            var mainPart = wordDoc.AddMainDocumentPart();

            // Tạo styles với font Arial mặc định
            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = GenerateDefaultStyles();

            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            return (mainPart, body);
        }

        private Styles GenerateDefaultStyles()
        {
            var styles = new Styles();

            // Style mặc định
            var defaultStyle = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = "Normal",
                Default = true
            };
            defaultStyle.Append(new StyleName() { Val = "Normal" });
            defaultStyle.Append(new PrimaryStyle());
            defaultStyle.Append(new StyleRunProperties(
                new RunFonts()
                {
                    Ascii = "Arial",
                    HighAnsi = "Arial",
                    ComplexScript = "Arial",
                    EastAsia = "Arial"
                },
                new FontSize() { Val = DefaultFontSize.ToString() },
                new Color() { Val = "000000" } // Màu đen
            ));

            styles.Append(defaultStyle);
            return styles;
        }

        private void AddQuocHieu(Body body)
        {
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Center },
                    new SpacingBetweenLines() { After = "100" } // Khoảng cách dưới
                ),
                CreateRun("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM",
                    bold: true, fontSize: HeaderFontSize, preserveSpace: true),
                CreateBreak(),
                CreateRun("Độc lập - Tự do - Hạnh phúc",
                    bold: true, fontSize: HeaderFontSize, preserveSpace: true),
                CreateBreak(),
                CreateRun($"..., ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}",
                    fontSize: HeaderFontSize)
            );

            body.AppendChild(paragraph);
        }

        private void AddTitle(Body body, string title, string subtitle)
        {
            body.AppendChild(new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Center },
                    new SpacingBetweenLines() { After = "200" }
                ),
                CreateRun(title, bold: true, fontSize: TitleFontSize),
                CreateBreak(),
                CreateRun(subtitle, italic: true, fontSize: HeaderFontSize)
            ));
        }

        private void AddRecipient(Body body)
        {
            body.AppendChild(
                CreateParagraph("Kính gửi: CÔNG AN XÃ (PHƯỜNG, THỊ TRẤN)...",
                    bold: true, fontSize: HeaderFontSize,
                    spacingAfter: 200)
            );
        }

        private void AddPetitionerInfo(Body body, ApplicationUser user)
        {
            string[] infoLines = {
                $"Tên tôi là: {user.FullName}",
                $"Sinh năm: {user.NgaySinh?.Year}",
                $"CMND/CCCD số: {user.CCCD}",
                $"Địa chỉ thường trú: {user.Address}",
                $"Địa chỉ hiện tại: {user.Address}",
                $"Số điện thoại: {user.PhoneNumber}"
            };

            foreach (var line in infoLines)
            {
                body.AppendChild(CreateParagraph(line, fontSize: DefaultFontSize));
            }
            body.AppendChild(CreateParagraph(""));
        }

        private void AddMissingPersonInfo(Body body, TimNguoi timNguoi)
        {
            body.AppendChild(
                CreateParagraph($"Là: {timNguoi.MoiQuanHe} của ông/bà: {timNguoi.HoTen}",
                    bold: true, fontSize: DefaultFontSize)
            );

            string[] infoLines = {
                $"Sinh năm: {timNguoi.NgaySinh?.Year}",
                $"Đặc điểm nhận dạng: {timNguoi.DaciemNhanDang}",
                $"Nơi thường trú: {timNguoi.KhuVuc ?? "Cập nhật"}",
                $"Khu vực mất tích: {timNguoi.KhuVuc}",
                $"Ngày mất tích: {timNguoi.NgayMatTich?.ToString("dd/MM/yyyy")}"
            };

            foreach (var line in infoLines)
            {
                body.AppendChild(CreateParagraph(line, fontSize: DefaultFontSize));
            }
            body.AppendChild(CreateParagraph(""));
        }

        private void AddFoundPersonInfo(Body body, TimThayNguoiThatLac timThay)
        {
            body.AppendChild(
                CreateParagraph($"Là: {timThay.TimNguoi.MoiQuanHe} của ông/bà: {timThay.TimNguoi.HoTen}",
                    bold: true, fontSize: DefaultFontSize)
            );

            string[] infoLines = {
                $"Sinh năm: {timThay.TimNguoi.NgaySinh?.Year}",
                $"Đặc điểm nhận dạng: {timThay.TimNguoi.DaciemNhanDang}",
                $"Ngày mất tích: {timThay.TimNguoi.NgayMatTich?.ToString("dd/MM/yyyy")}",
                $"Ngày tìm thấy: {timThay.NgayTimThay?.ToString("dd/MM/yyyy")}",
                $"Địa điểm tìm thấy: {timThay.DiaDiemTimThay}",
                $"Tình trạng sức khỏe: {timThay.TinhTrangSucKhoe}",
                $"Người tìm thấy: {timThay.NguoiTimThay ?? "Không rõ"}"
            };

            foreach (var line in infoLines)
            {
                body.AppendChild(CreateParagraph(line, fontSize: DefaultFontSize));
            }
            body.AppendChild(CreateParagraph(""));
        }

        private void AddHtmlContent(MainDocumentPart mainPart, Body body, string htmlContent,
            string introText = null, params string[] additionalContents)
        {
            // Phần giới thiệu
            if (!string.IsNullOrEmpty(introText))
            {
                body.AppendChild(CreateParagraph(introText, bold: true, fontSize: DefaultFontSize));
            }

            // Nội dung HTML chính
            if (!string.IsNullOrEmpty(htmlContent))
            {
                try
                {
                    var converter = new HtmlConverter(mainPart);

                    // Cấu hình font mặc định
                    //converter.HtmlStyles.DefaultFont = "Arial";
                    //converter.HtmlStyles.DefaultFontSize = DefaultFontSize;

                    var style = new Style();
                    style.AddChild(new StyleRunProperties(
                        new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" },
                        new FontSize() { Val = DefaultFontSize.ToString() }
                    ));

                    // Parse HTML content
                    var paragraphs = converter.Parse("<div style='font-family:Arial;font-size:11pt'>" + htmlContent + "</div>");

                    foreach (var paragraph in paragraphs.OfType<Paragraph>())
                    {
                        EnsureArialFont(paragraph);
                        body.AppendChild(paragraph);
                    }
                }
                catch (Exception ex)
                {
                    // Fallback nếu có lỗi chuyển đổi HTML
                    Console.WriteLine($"Lỗi chuyển đổi HTML: {ex.Message}");
                    body.AppendChild(CreateParagraph(htmlContent, fontSize: DefaultFontSize));
                }
            }

            // Các nội dung bổ sung
            foreach (var content in additionalContents.Where(c => !string.IsNullOrEmpty(c)))
            {
                body.AppendChild(CreateParagraph(content, fontSize: DefaultFontSize));
            }
            body.AppendChild(CreateParagraph(""));
        }

        private void EnsureArialFont(Paragraph paragraph)
        {
            foreach (var run in paragraph.Descendants<Run>())
            {
                if (run.RunProperties == null)
                {
                    run.RunProperties = new RunProperties();
                }

                if (run.RunProperties.RunFonts == null)
                {
                    run.RunProperties.RunFonts = new RunFonts
                    {
                        Ascii = "Arial",
                        HighAnsi = "Arial",
                        ComplexScript = "Arial",
                        EastAsia = "Arial"
                    };
                }
            }
        }

        private void AddCommitment(Body body, params string[] commitmentTexts)
        {
            foreach (var text in commitmentTexts.Where(t => !string.IsNullOrEmpty(t)))
            {
                body.AppendChild(CreateParagraph(text, fontSize: DefaultFontSize));
            }
            body.AppendChild(CreateParagraph(""));
        }

        private void AddSignature(Body body, ApplicationUser user)
        {
            body.AppendChild(new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Right },
                    new SpacingBetweenLines() { Before = "200" }
                ),
                CreateRun("NGƯỜI LÀM ĐƠN", bold: true, fontSize: DefaultFontSize),
                CreateBreak(),
                CreateRun("(Ký và ghi rõ họ tên)", italic: true, fontSize: SmallFontSize),
                CreateBreak(),
                CreateBreak(),
                CreateBreak(),
                CreateRun(user.FullName, bold: true, fontSize: DefaultFontSize)
            ));
        }

        #region Helper Methods
        private Run CreateRun(string text, bool bold = false, int? fontSize = null,
            bool preserveSpace = false, bool italic = false)
        {
            var run = new Run();
            var props = new RunProperties();

            // Font Arial
            props.Append(new RunFonts
            {
                Ascii = "Arial",
                HighAnsi = "Arial",
                ComplexScript = "Arial",
                EastAsia = "Arial"
            });

            // Định dạng
            if (bold) props.Append(new Bold());
            if (italic) props.Append(new Italic());
            if (fontSize.HasValue) props.Append(new FontSize { Val = fontSize.Value.ToString() });

            run.RunProperties = props;

            // Nội dung text
            var textElement = new Text(text);
            if (preserveSpace) textElement.Space = SpaceProcessingModeValues.Preserve;

            run.Append(textElement);

            return run;
        }

        private Run CreateBreak()
        {
            return new Run(new Break());
        }

        private Paragraph CreateParagraph(string text, bool bold = false, int? fontSize = null,
            JustificationValues? justify = null, int? spacingAfter = null)
        {
            var paragraph = new Paragraph();
            var props = new ParagraphProperties();

            if (justify.HasValue)
            {
                props.Justification = new Justification() { Val = justify.Value };
            }

            if (spacingAfter.HasValue)
            {
                props.SpacingBetweenLines = new SpacingBetweenLines() { After = spacingAfter.Value.ToString() };
            }

            if (props.HasChildren)
            {
                paragraph.ParagraphProperties = props;
            }

            paragraph.Append(CreateRun(text, bold, fontSize));

            return paragraph;
        }
        #endregion
    }
}