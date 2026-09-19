using ClosedXML.Excel;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.Reports;

namespace VSSM.Application.Services.ExcelGenerators
{
    public class ReportExcelGenerator : IReportExcelGenerator
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

        public MemoryStream GetTotalLossExcel(List<TotalLossReportDto> data)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("گزارش اتلاف کل");

                // Right to Left sheet
                ws.RightToLeft = true;

                // Headers
                ws.Cell(1, 1).Value = "کالا";
                ws.Cell(1, 2).Value = "دلیل امحا";
                ws.Cell(1, 3).Value = "امحا شده توسط";
                ws.Cell(1, 4).Value = "تاریخ";

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
                    ws.Cell(row, 1).Value = item.Item;
                    ws.Cell(row, 2).Value = item.DestroyReason;
                    ws.Cell(row, 3).Value = item.DestroyBy;
                    ws.Cell(row, 4).Value = item.Date;

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

        public MemoryStream ExpiredItemsReportExcel(List<GroupedExpiredItemDto> data)
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
                    ws.Cell(1, 4).Value = "دوز بر ویال";
                    ws.Cell(1, 5).Value = "تعداد";
                    ws.Cell(1, 6).Value = "تعداد ویال";
                    ws.Cell(1, 7).Value = "انبار";
                    ws.Cell(1, 8).Value = "مرحله VVM";

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

                    foreach (var item in groupedItem.Details)
                    {
                        ws.Cell(row, 1).Value = item.BatchNumber;
                        ws.Cell(row, 2).Value = item.ManufacturerName;
                        ws.Cell(row, 3).Value = item.ExpiryDateText;
                        ws.Cell(row, 4).Value = item.DoesPerVial;
                        ws.Cell(row, 5).Value = item.CurrentStock;
                        ws.Cell(row, 6).Value = item.VialNumber;
                        ws.Cell(row, 7).Value = item.Stock;
                        ws.Cell(row, 8).Value = item.Vvmstage;

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


        public MemoryStream ArrivalByItemReportExcel(List<GroupedArrivalByItemDto> data)
        {
            var headers = new[]
            {
        "کالا", "نوع کالا",
        "تاریخ دریافت", "تاریخ ورود", "شماره گزارش",
        "منبع", "شماره سفارش", "Entered by",
        "سری ساخت", "سازنده",
        "دوز-مقدار", "تعداد ویال", "دوز بر ویال",
        "تاریخ انقضا", "تاریخ تولید",
        "انبار", "نام تجاری", "نوع VVM", "مرحله VVM", "ملاحظات"
    };

            var numericColumns = new[] { 11, 12 };

            var rows = new List<object[]>();

            foreach (var group in data)
            {
                foreach (var d in group.Details)
                {
                    rows.Add(new object[]
                    {
                group.Meta.ItemName,
                group.Meta.ItemClass,
                d.ReceiveDateText,
                d.ArrivalDateText,
                d.ReportNumber,
                d.Source,
                d.OrderNumber,
                d.EnteredBy,
                d.BatchNumber,
                d.Manufacturer,
                d.Quantity,
                d.NumVials,
                d.DosePerVial,
                d.ExpiryDateText,
                d.ProductionDateText,
                d.Store,
                d.CommercialName,
                d.VvmType,
                d.VvmStage,
                d.Note
                    });
                }
            }

            return BuildSheet(
                sheetName: "ورود بر اساس کالا",
                title: "گزارش ورودی بر اساس کالا",
                headers: headers,
                rows: rows,
                numericColumns: numericColumns);
        }

        public MemoryStream DispatchByRecipientReportExcel(List<DispatchReportDto> data)
        {
            var headers = new[]
            {
        "نوع کالا", "کالا", "کد کالا", "گیرنده", "تاریخ ارسال",
        "شماره حواله", "شماره درخواست", "سری ساخت", "تعداد",
        "دوز بر ویال", "تعداد ویال", "تاریخ انقضا", "انبار"
    };

            var numericColumns = new[] { 9, 11 };

            var rows = data.Select(item => new object[]
            {
        item.ItemClass, item.Item, item.Code, item.Recipient, item.DispatchDate,
        item.VoucherNumber, item.RequestNumber, item.BatchNumber, item.Quantity,
        item.DosePerVial, item.NumVials, item.ExpiryDate, item.Store
            }).ToList();

            return BuildSheet(
                sheetName: "ارسالات بر اساس گیرندگان",
                title: "گزارش ارسالات بر اساس گیرندگان",
                headers: headers,
                rows: rows,
                numericColumns: numericColumns);
        }

        public MemoryStream ArrivalSummaryReportExcel(List<ArrivalSummaryDto> data)
        {
            var headers = new[]
            {
        "نوع کالا", "کالا", "کد کالا", "سازنده", "منبع", "فعالیت",
        "تاریخ ورود", "شماره گزارش", "شماره سفارش",
        "تعداد", "سری ساخت", "دوز بر ویال", "تعداد ویال",
        "قیمت واحد", "مبلغ کل", "تاریخ انقضا"
    };

            var numericColumns = new[] { 10, 13, 14, 15 };

            var rows = data.Select(x => new object[]
            {
        x.ItemClass,
        x.Item,
        x.ItemCode,
        x.Manufacturer,
        x.Source,
        x.Activity,
        x.ArrivalDateText,
        x.ReportNumber,
        x.OrderNumber,
        x.Quantity,
        x.BatchNumber,
        x.DosePerVial,
        x.NumVials,
        x.UnitPrice,
        x.TotalPrice,
        x.ExpiryDateText
            }).ToList();

            return BuildSheet(
                sheetName: "خلاصه موارد رسیده",
                title: "گزارش خلاصه موارد رسیده",
                headers: headers,
                rows: rows,
                numericColumns: numericColumns);
        }

        private static MemoryStream BuildSheet(
    string sheetName,
    string title,
    string[] headers,
    List<object[]> rows,
    int[] numericColumns)
        {
            var stream = new MemoryStream();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(sheetName);

                ws.RightToLeft = true;
                ws.ShowGridLines = false;

                const int titleRow = 1;
                const int headerRow = 3;
                const int dataStartRow = 4;

                var titleRange = ws.Range(titleRow, 1, titleRow, headers.Length);
                titleRange.Merge();
                titleRange.Value = title;
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Font.FontColor = XLColor.FromHtml("#0F172A");
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(titleRow).Height = 30;

                for (var i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(headerRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.FromHtml("#1E3A8A");
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.BottomBorderColor = XLColor.FromHtml("#CBD5E1");
                }
                ws.Row(headerRow).Height = 24;

                for (var r = 0; r < rows.Count; r++)
                {
                    var excelRow = dataStartRow + r;
                    var rowData = rows[r];

                    for (var c = 0; c < headers.Length; c++)
                    {
                        var cell = ws.Cell(excelRow, c + 1);
                        var value = rowData[c];

                        if (value is null)
                        {
                            cell.Value = "-";
                        }
                        else if (value is long or int or decimal or double)
                        {
                            cell.Value = Convert.ToDecimal(value);
                            cell.Style.NumberFormat.Format = "#,##0";
                        }
                        else
                        {
                            cell.Value = value.ToString();
                        }

                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        cell.Style.Alignment.Horizontal = numericColumns.Contains(c + 1)
                            ? XLAlignmentHorizontalValues.Center
                            : XLAlignmentHorizontalValues.Right;
                    }

                    if (r % 2 == 1)
                    {
                        ws.Range(excelRow, 1, excelRow, headers.Length)
                          .Style.Fill.BackgroundColor = XLColor.FromHtml("#FAFBFC");
                    }
                }

                if (rows.Count > 0)
                {
                    var lastRow = dataStartRow + rows.Count - 1;
                    var lastRange = ws.Range(lastRow, 1, lastRow, headers.Length);
                    lastRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    lastRange.Style.Border.BottomBorderColor = XLColor.FromHtml("#CBD5E1");
                }

                ws.Columns(1, headers.Length).AdjustToContents();

                foreach (var col in ws.Columns(1, headers.Length))
                {
                    if (col.Width < 10) col.Width = 10;
                    if (col.Width > 40) col.Width = 40;
                }

                ws.SheetView.FreezeRows(headerRow);

                wb.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }
    }
}
