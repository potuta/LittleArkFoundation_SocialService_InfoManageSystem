using DinkToPdf;
using DinkToPdf.Contracts;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;

namespace LittleArkFoundation.Data
{
    public class PDFService
    {
        private readonly IConverter _pdfConverter;

        public PDFService(IConverter pdfConverter)
        {
            _pdfConverter = pdfConverter;
        }

        public async Task<byte[]> GeneratePdfAsync(string htmlContent, string documentTitle = "Generated PDF")
        {
            return await Task.Run(() =>
            {
                var pdfDocument = new HtmlToPdfDocument()
                {
                    GlobalSettings = new GlobalSettings
                    {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                        DocumentTitle = documentTitle,
                        DPI = 300,
                        ViewportSize = "595x842" // Set to A4 dimensions
                    },
                    Objects =
                    {
                        new ObjectSettings
                        {
                            HtmlContent = htmlContent,
                            WebSettings = new WebSettings
                            {
                                DefaultEncoding = "utf-8",
                                LoadImages = true,
                                PrintMediaType = true
                            },
                            UseExternalLinks = true,
                            UseLocalLinks = true,
                            LoadSettings = new LoadSettings
                            {
                                ZoomFactor = 1.5
                            },
                            HeaderSettings = { HtmUrl = "", Spacing = 0 },
                            FooterSettings = { HtmUrl = "", Spacing = 0 },
                        }
                    }
                };

                return _pdfConverter.Convert(pdfDocument);
            });
        }

        public async Task<byte[]> MergePdfsAsync(List<byte[]> pdfFiles)
        {
            return await Task.Run(() =>
            {
                using (var outputDocument = new PdfDocument())
                {
                    foreach (var pdfBytes in pdfFiles)
                    {
                        using (var stream = new MemoryStream(pdfBytes))
                        {
                            var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                            foreach (var page in inputDocument.Pages)
                            {
                                outputDocument.AddPage(page);
                            }
                        }
                    }

                    using (var outputStream = new MemoryStream())
                    {
                        outputDocument.Save(outputStream);
                        return outputStream.ToArray();
                    }
                }
            });
        }

        public async Task<byte[]> ConvertImageToPdfAsync(byte[] imageBytes, string imageFormat = "png")
        {
            return await Task.Run(() =>
            {
                // Convert image bytes to base64 string
                string base64 = Convert.ToBase64String(imageBytes);
                string imageSrc = $"data:image/{imageFormat};base64,{base64}";

                // Basic HTML that embeds the image
                string html = $@"
                    <html>
                        <head>
                            <style>
                                @page {{
                                    margin: 0;
                                }}
                                html, body {{
                                    margin: 0;
                                    padding: 0;
                                    height: 100%;
                                    width: 100%;
                                }}
                                img {{
                                    display: block;
                                    width: 100%;
                                    height: auto;
                                    max-width: 100%;
                                    max-height: 100%;
                                    object-fit: contain;
                                }}
                            </style>
                        </head>
                        <body>
                            <img src='{imageSrc}' alt='Image'/>
                        </body>
                    </html>";

                return GeneratePdfAsync(html, "Image Attachment");
            });
        }

    }
}
