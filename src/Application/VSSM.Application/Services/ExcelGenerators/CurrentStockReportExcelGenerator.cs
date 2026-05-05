using ClosedXML.Excel;
using VSSM.Application.DTOs.BaseEntities.Reports;

namespace VSSM.Application.Services.ExcelGenerators
{
    public class CurrentStockReportExcelGenerator : ICurrentStockReportExcelGenerator
    {
        public MemoryStream AvailableQuantityDetailReportExcel(List<GroupedAvailableQuantityDetailDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {

                foreach (var groupedItem in data)
                {
                    var ws = wb.Worksheets.Add(groupedItem.Meta.ItemName);

                    // Right to Left sheet
                    ws.RightToLeft = true;

                    // Headers
                    ws.Cell(1, 1).Value = "سری ساخت";
                    ws.Cell(1, 2).Value = "سازنده";
                    ws.Cell(1, 3).Value = "تاریخ انقضا";
                    ws.Cell(1, 4).Value = "روز‌های مانده به انقضا";
                    ws.Cell(1, 5).Value = "تعداد";
                    ws.Cell(1, 6).Value = "دوز بر ویال";
                    ws.Cell(1, 7).Value = "انبار";
                    ws.Cell(1, 8).Value = "گیرنده";
                    ws.Cell(1, 9).Value = "تاریخ تولید";
                    ws.Cell(1, 10).Value = "نام تجاری";
                    ws.Cell(1, 11).Value = "نوع VVM";
                    ws.Cell(1, 12).Value = "مرحله VVM";

                    var header = ws.Range(1, 1, 1, 12);
                    header.Style.Font.Bold = true;
                    header.Style.Font.FontSize = 12;
                    header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                    header.Style.Font.FontColor = XLColor.White;
                    header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    int row = 2;

                    foreach (var item in groupedItem.Details)
                    {
                        ws.Cell(row, 1).Value = item.BatchNumber;
                        ws.Cell(row, 2).Value = item.ManufacturerName;
                        ws.Cell(row, 3).Value = item.ShamsiExpiryDate;
                        ws.Cell(row, 4).Value = item.DaysToBeExpired;
                        ws.Cell(row, 5).Value = item.CurrentStock;
                        ws.Cell(row, 6).Value = item.DoesPerVial;
                        ws.Cell(row, 7).Value = item.Stock;
                        ws.Cell(row, 8).Value = item.Recipient;
                        ws.Cell(row, 9).Value = item.ProductionDateText;
                        ws.Cell(row, 10).Value = item.CommercialName;
                        ws.Cell(row, 11).Value = item.Vvmtype;
                        ws.Cell(row, 12).Value = item.Vvmstage;

                        row++;
                    }


                    ws.SheetView.FreezeRows(1);

                    ws.Columns().AdjustToContents();

                    ws.Row(1).Height = 25;
                }

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream AvailableQuantityQuantitySummaryReportExcel(List<AvailableQuantityQuantitySummaryDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("مقدار موجود - خلاصه موجودی");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "نوع کالا";
                ws.Cell(1, 2).Value = "Item Code";
                ws.Cell(1, 3).Value = "کالا";
                ws.Cell(1, 4).Value = "سری ساخت";
                ws.Cell(1, 5).Value = "دوز بر ویال";
                ws.Cell(1, 6).Value = "تاریخ انقضا";
                ws.Cell(1, 7).Value = "تعداد";


                var header = ws.Range(1, 1, 1, 7);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.ItemClass;
                    ws.Cell(row, 2).Value = item.ItemCode;
                    ws.Cell(row, 3).Value = item.ItemName;
                    ws.Cell(row, 4).Value = item.BatchNumber;
                    ws.Cell(row, 5).Value = item.DoesPerVial;
                    ws.Cell(row, 6).Value = item.ExpiryDateText;
                    ws.Cell(row, 7).Value = item.CurrentStock;

                    row++;
                }


                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream AvailableQuantitySummaryReportExcel(List<AvailableQuantitySummaryDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("مقدار موجود - خلاصه گزارش");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "Item Code";
                ws.Cell(1, 2).Value = "کالا";
                ws.Cell(1, 3).Value = "تعداد";


                var header = ws.Range(1, 1, 1, 3);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.ItemCode;
                    ws.Cell(row, 2).Value = item.ItemName;
                    ws.Cell(row, 3).Value = item.CurrentStock;

                    row++;
                }

                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream AvailableQuantitySummaryWithDosePerVialReportExcel(List<AvailableQuantitySummaryWithDosePerVialDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("خلاصه گزارش با دوز بر ویال");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "Item Code";
                ws.Cell(1, 2).Value = "کالا";
                ws.Cell(1, 3).Value = "دوز بر ویال";
                ws.Cell(1, 4).Value = "تعداد";


                var header = ws.Range(1, 1, 1, 4);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.ItemCode;
                    ws.Cell(row, 2).Value = item.ItemName;
                    ws.Cell(row, 3).Value = item.DoesPerVial;
                    ws.Cell(row, 4).Value = item.CurrentStock;

                    row++;
                }


                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream InventoryHistoryExcel(List<InventoryHistoryDto> data)

        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("تاریخچه موجدی");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "مقدار در پایان ماه";
                ws.Cell(1, 2).Value = "سال";
                ws.Cell(1, 3).Value = "ماه";

                var header = ws.Range(1, 1, 1, 3);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.InStock;
                    ws.Cell(row, 2).Value = item.Year;
                    ws.Cell(row, 3).Value = item.Month;

                    row++;
                }


                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream ItemForRecipientInYearExcel(List<ItemForRecipientInYearDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("گزارش کالا برای گیرندگان در سال");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "گیرنده";
                ws.Cell(1, 2).Value = "ژانویه";
                ws.Cell(1, 3).Value = "فوریه";
                ws.Cell(1, 4).Value = "مارس";
                ws.Cell(1, 5).Value = "آپریل";
                ws.Cell(1, 6).Value = "می";
                ws.Cell(1, 7).Value = "ژوئن";
                ws.Cell(1, 8).Value = "جولای";
                ws.Cell(1, 9).Value = "آگوست";
                ws.Cell(1, 10).Value = "سپتامبر";
                ws.Cell(1, 11).Value = "اکتبر";
                ws.Cell(1, 12).Value = "نوامبر";
                ws.Cell(1, 13).Value = "دسامبر";
                ws.Cell(1, 14).Value = "Total Quotal";
                ws.Cell(1, 15).Value = "مقدار احتیاج سالانه";

                var header = ws.Range(1, 1, 1, 15);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.Reciever;
                    ws.Cell(row, 2).Value = item.Jan;
                    ws.Cell(row, 3).Value = item.Feb;
                    ws.Cell(row, 4).Value = item.Mar;
                    ws.Cell(row, 5).Value = item.Apr;
                    ws.Cell(row, 6).Value = item.May;
                    ws.Cell(row, 7).Value = item.Jun;
                    ws.Cell(row, 8).Value = item.Jul;
                    ws.Cell(row, 9).Value = item.Agu;
                    ws.Cell(row, 10).Value = item.Sep;
                    ws.Cell(row, 11).Value = item.Oct;
                    ws.Cell(row, 12).Value = item.Nov;
                    ws.Cell(row, 13).Value = item.Dec;
                    ws.Cell(row, 14).Value = item.TotalQuota;
                    ws.Cell(row, 15).Value = item.YearlyNeed;

                    row++;
                }


                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream ItemIndicatorExcel(List<ItemIndicatorDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Zero Dispatch Report");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "شماره حواله  شماره گزارش";
                ws.Cell(1, 2).Value = "شماره درخواست";
                ws.Cell(1, 3).Value = "تاریخ";
                ws.Cell(1, 4).Value = "از / به";
                ws.Cell(1, 5).Value = "ورودی (دریافتی)";
                ws.Cell(1, 6).Value = " خروجی (ارسالی)";
                ws.Cell(1, 7).Value = "مانده";
                ws.Cell(1, 8).Value = "توضیح";

                var header = ws.Range(1, 1, 1, 8);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.ReportNumber;
                    ws.Cell(row, 2).Value = item.RequestNum;
                    ws.Cell(row, 3).Value = item.DateText;
                    ws.Cell(row, 4).Value = item.Recipient;
                    ws.Cell(row, 5).Value = item.Arrival;
                    ws.Cell(row, 6).Value = item.Dispatch;
                    ws.Cell(row, 7).Value = item.CurrentStock;
                    ws.Cell(row, 8).Value = item.Description;

                    row++;
                }


                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream NotDispatchedToRecipientsExcel(List<NotDispatchedToRecipientsDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Zero Dispatch Report");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "گیرنده";

                var header = ws.Range(1, 1, 1, 1);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.FullName;

                    row++;
                }


                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream OutOfStockItemsExcel(List<OutOfStockItemsDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Zero Stock");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "نام کالا";

                var header = ws.Range(1, 1, 1, 1);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.ItemName;

                    row++;
                }


                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public MemoryStream PhysicalInventoryExcel(List<InventoryQuantityReportDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Physical Inventory");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "نوع کالا";
                ws.Cell(1, 2).Value = "کالا";
                ws.Cell(1, 3).Value = "سازنده";
                ws.Cell(1, 4).Value = "سری ساخت";
                ws.Cell(1, 5).Value = "تاریخ انقضا";
                ws.Cell(1, 6).Value = "نام انبار";
                ws.Cell(1, 7).Value = "تعداد";

                var header = ws.Range(1, 1, 1, 7);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;

                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.ItemClass;
                    ws.Cell(row, 2).Value = item.Item;
                    ws.Cell(row, 3).Value = item.Manufacturer;
                    ws.Cell(row, 4).Value = item.BatchNumber;
                    ws.Cell(row, 5).Value = item.ExpiryDate;
                    ws.Cell(row, 6).Value = item.Store;
                    ws.Cell(row, 7).Value = item.Quantity;

                    row++;
                }


                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                ws.Row(1).Height = 25;

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }
    }
}
