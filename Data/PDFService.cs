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
                        Margins = new MarginSettings { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                        DocumentTitle = documentTitle,
                        DPI = 300
                    },
                    Objects =
                    {
                        new ObjectSettings
                        {
                            HtmlContent = htmlContent,
                            WebSettings =
                            {
                                DefaultEncoding = "utf-8",
                                LoadImages = true,
                                PrintMediaType = true
                            },
                            UseExternalLinks = true,
                            LoadSettings =
                            {
                                ZoomFactor = 2
                            }
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
    }
}
