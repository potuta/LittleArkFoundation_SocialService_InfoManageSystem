using ClosedXML.Excel;

namespace LittleArkFoundation.Areas.Admin.Services.Reports
{
    public static class ExcelReportStyler
    {
        public static void ApplyWorksheetDesign(
            IXLWorksheet worksheet,
            List<int> rowsList,
            List<int> headerRowsList,
            List<int> totalRowsList,
            int lastRowIndex,
            string userNameClaim,
            bool isCenterAligned,
            bool? hideTotalRows = false,
            bool? isStats = false)
        {
            if (isCenterAligned)
            {
                // Apply center alignment globally
                worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            int lastColumn = worksheet.LastColumnUsed().ColumnNumber();

            // Title rows
            foreach (var rowIndex in rowsList)
            {
                var titleRange = worksheet.Range(rowIndex, 1, rowIndex, lastColumn);
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Merge();
            }

            // Header rows
            foreach (var headerRowIndex in headerRowsList)
            {
                var headerRange = worksheet.Range(headerRowIndex, 1, headerRowIndex, lastColumn);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.Emerald;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Total rows
            if (hideTotalRows == false)
            {
                foreach (var totalRowIndex in totalRowsList)
                {
                    var totalRange = worksheet.Range(totalRowIndex, 1, totalRowIndex, lastColumn);
                    totalRange.Style.Font.Bold = true;
                    totalRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
                }
            }

            // Zebra stripes + borders only for rows with data
            var usedRange = worksheet.RangeUsed();
            int rowCounter = 1;

            // Exclude title rows, header rows, and total rows
            var excludedRows = rowsList
                .Concat(headerRowsList)
                .Concat(totalRowsList)
                .ToHashSet();

            foreach (var row in usedRange.Rows())
            {
                bool rowHasData = row.Cells().Any(c => !c.IsEmpty());

                if (rowHasData)
                {
                    // Apply borders only if row has data
                    row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Apply zebra stripes if not excluded
                    if (!excludedRows.Contains(row.RowNumber()) && row.RowNumber() > 2)
                    {
                        if (rowCounter % 2 == 0)
                            row.Style.Fill.BackgroundColor = XLColor.FromHtml("#daf2d0");
                    }

                    rowCounter++;
                }
            }

            // Signature block
            int signatureRowStart = lastRowIndex + 3;
            int lastCol = isStats.Value ? lastColumn - 1 : lastColumn;

            worksheet.Cell(signatureRowStart, 1).Value = $"Date Printed: {DateTime.Now:MM/dd/yyyy}";
            worksheet.Cell(signatureRowStart, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            worksheet.Cell(signatureRowStart, lastCol).Value = userNameClaim ?? "";
            worksheet.Cell(signatureRowStart + 1, lastCol).Value = "______________________________";
            worksheet.Cell(signatureRowStart + 2, lastCol).Value = "Prepared By";
            worksheet.Range(signatureRowStart, lastCol, signatureRowStart + 2, lastCol)
                     .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(signatureRowStart + 3, lastCol).Value = "______________________________";
            worksheet.Cell(signatureRowStart + 4, lastCol).Value = "Approved By";
            worksheet.Range(signatureRowStart + 3, lastCol, signatureRowStart + 4, lastCol)
                     .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Page setup
            worksheet.PageSetup.PrintAreas.Clear();
            worksheet.PageSetup.PrintAreas.Add($"A1:{worksheet.LastCellUsed().Address}");
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            worksheet.PageSetup.PagesWide = 1;
            worksheet.PageSetup.PagesTall = 0;
            //worksheet.PageSetup.Footer.Center.AddText("Page &P of &N");
            worksheet.PageSetup.Margins.Top = 0.5;
            worksheet.PageSetup.Margins.Bottom = 0.5;
            worksheet.PageSetup.Margins.Left = 0.5;
            worksheet.PageSetup.Margins.Right = 0.5;
            worksheet.PageSetup.CenterHorizontally = true;
            worksheet.Row(signatureRowStart - 1);

        }
    }

}
