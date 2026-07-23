using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using VSSM.Application.DTOs.UidHistory;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.UidHistory;

public class UidHistoryService(VSSMDbContext db) : IUidHistoryService
{
    public async Task<(bool Success, string Message, UidHistoryResultDto Data)>
        GetHistoryByUidAsync(string uid, int recipientId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(uid) || uid.Length < 20)
            return (false, "UID وارد شده معتبر نمی‌باشد (حداقل ۲۰ کاراکتر)", null);

        // 1. دریافت UidInfo با تمام ارتباطات مورد نیاز
        var uidRecord = await db.TblUidInfo
            .AsNoTracking()
            .Include(u => u.OrderInfo)
                .ThenInclude(o => o.Item)
            .Include(u => u.OrderInfo)
                .ThenInclude(o => o.DosePerVial)
            .Include(u => u.OrderInfo)
                .ThenInclude(o => o.Arrival)
                    .ThenInclude(a => a.Receiver)
            .Include(u => u.OrderInfo)
                .ThenInclude(o => o.Arrival)
                    .ThenInclude(a => a.Sender)
            .Include(u => u.OrderInfo)
                .ThenInclude(o => o.Arrival)
                    .ThenInclude(a => a.TblArrivalLines)
            .FirstOrDefaultAsync(
                u => u.SecLevelUid == uid ||
                     u.FirstLevelUid == uid ||
                     u.ParentUid == uid,
                token);

        if (uidRecord == null)
            return (false, "UID یافت نشد", null);

        var order = uidRecord.OrderInfo;
        if (order == null)
            return (false, "اطلاعات محموله برای این UID یافت نشد", null);

        // 2. دریافت نام کاربر سازنده
        string? createdByUserName = null;
        if (order.CreatedBy > 0)
        {
            var user = await db.TblUsers
                .AsNoTracking()
                .Where(u => u.UserId == order.CreatedBy)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstOrDefaultAsync(token);
            createdByUserName = user ?? order.CreatedBy.ToString();
        }

        // 3. دریافت اطلاعات مرتبط
        var arrival = order.Arrival;

        TblDispatch? dispatch = null;
        var dispatchUid = await db.TblDispatchUids
            .AsNoTracking()
            .Include(d => d.Dispatch)
                .ThenInclude(d => d.Receiver)
            .FirstOrDefaultAsync(d => d.FKUidInfo == uidRecord.PkUidInfo, token);
        if (dispatchUid != null)
            dispatch = dispatchUid.Dispatch;

        TblReceiveDispatches? receiveDispatch = null;
        if (uidRecord.FkReceiveDispatchId.HasValue)
        {
            receiveDispatch = await db.TblReceiveDispatches
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PkReceiveDispatchesId == uidRecord.FkReceiveDispatchId, token);
        }

        TblConsumerPerson? consumer = null;
        if (uidRecord.PkUidInfo > 0)
        {
            consumer = await db.TblConsumerPeople
                .AsNoTracking()
                .Include(c => c.Recipient)
                .FirstOrDefaultAsync(c => c.FkUid == uidRecord.PkUidInfo, token);
        }

        TblDestroyUid? destroy = null;
        if (uidRecord.PkUidInfo > 0)
        {
            destroy = await db.TblDestroyUids
                .AsNoTracking()
                .Include(d => d.DestroyReason)
                .Include(d => d.Location)
                .Include(d => d.CreatedByUser)
                .FirstOrDefaultAsync(d => d.FkUid == uidRecord.PkUidInfo, token);
        }

        // 4. ساخت ردیف‌های تاریخچه
        var rows = new List<UidHistoryRowDto>();

        // ─── ردیف ۱: بارگذاری فایل ───
        string madeDesc =
            $"سری ساخت: {order.BatchNumber ?? "-"} - " +
            $"نام کالا: {order.Item?.FullName ?? "-"} - " +
            $"تاریخ تولید: {order.ManufactureDate.ToShamsi()} - " +
            $"تاریخ انقضا: {(order.ExpirationDate.HasValue ? order.ExpirationDate.Value.ToShamsi() : "-")} - " +
            $"تعداد دوز: {order.DosePerVial?.Title ?? "-"} - " +
            $"کاربر: {createdByUserName ?? order.CreatedBy.ToString()} - " +
            $"مصرف شده: {(uidRecord.Used ? "بله" : "خیر")}";

        rows.Add(new UidHistoryRowDto
        {
            RowNumber = 0,
            EventTitle = "تاریخ بارگذاری فایل :",
            EventDate = order.CreationTime.ToShamsi(),
            Description = madeDesc,
            SortOrder = 1
        });

       string storeName = "-";
        
        if (arrival != null)
        {
            string reportNumber = arrival.ReportNumber ?? "-";

            var arrivalLine = arrival.TblArrivalLines?.FirstOrDefault();
            if (arrivalLine?.StoreId.HasValue == true)
            {
                var store = await db.TblStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StoreId == arrivalLine.StoreId.Value, token);
                storeName = store?.Name ?? "-";
            }

            string arrivalDesc =
                $"طی شماره گزارش: {reportNumber} - " +
                $"وارد انبار: {storeName} - " +
                $"کاربر: {createdByUserName ?? order.CreatedBy.ToString()}";

            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventTitle = "تاریخ ورود به انبار مرکز تهران :",
                EventDate = arrival.Date?.ToShamsi() ?? "-",
                Description = arrivalDesc,
                SortOrder = 2
            });
        }

        if (dispatch != null)
        {
            string receiverName = dispatch.Receiver?.FullName ?? "-";
            string transportMode = dispatch.TransportMode?.ToString() ?? "-";
            string requestNum = dispatch.RequestNum ?? "-";

            string dispatchDesc =
                $"انتقال برای: {receiverName} - " +
                $"طی شماره فاکتور: {dispatch.DispatchVoucherNumber ?? "-"} - " +
                $"از طریق انبار: {storeName ?? "-"} - " +
                $"روش انتقال: {transportMode} - " +
                $"شماره درخواست: {requestNum}";

            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventTitle = "تاریخ ارسال :",
                EventDate = dispatch.Date?.ToShamsi() ?? "-",
                Description = dispatchDesc,
                SortOrder = 3
            });
        }

        if (receiveDispatch != null)
        {
            string receiverName = dispatch?.Receiver?.FullName ?? "-";

            string receiveDesc =
                $"محل دریافت: {receiverName} - " +
                $"شماره رسید: {receiveDispatch.ReceiveNumber} - " +
                $"وضعیت دریافت: دریافت شده";

            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventTitle = "تاریخ ورود به انبار مراکز :",
                EventDate = receiveDispatch.ReceiveDate.ToShamsi(),
                Description = receiveDesc,
                SortOrder = 4
            });
        }

        if (consumer != null)
        {
            string consumerPerson = consumer.ConsumerPerson ?? "-";
            string vaccinator = consumer.Vaccinator ?? "-";
            string recipientName = consumer.Recipient?.FullName ?? "-";

            string consumerDesc =
                $"واکسن مورد نظر در تاریخ: {consumer.EntryDate.ToShamsi()} - " +
                $"توسط کد ملی: {consumerPerson} - " +
                $"در محل: {recipientName} - " +
                $"تزریق کننده: {vaccinator}";

            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventTitle = "تاریخ مصرف :",
                EventDate = consumer.EntryDate.ToShamsi(),
                Description = consumerDesc,
                SortOrder = 5
            });
        }

        if (destroy != null)
        {
            string location = destroy.Location?.FullName ?? "-";
            string reason = destroy.DestroyReason?.Title ?? "-";
            string destroyedBy = (destroy.CreatedByUser != null)
                ? $"{destroy.CreatedByUser.FirstName} {destroy.CreatedByUser.LastName}"
                : "-";

            string destroyDesc =
                $"واکسن مورد نظر در تاریخ: {destroy.DestroyDate} - " +
                $"در محل: {location} - " +
                $"به دلیل: {reason} - " +
                $"امحا گردید - " +
                $"امحا کننده: {destroyedBy}";

            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventTitle = "تاریخ امحا :",
                EventDate = destroy.DestroyDate,
                Description = destroyDesc,
                SortOrder = 6
            });
        }

        rows = rows.OrderBy(r => r.SortOrder).ToList();
        for (int i = 0; i < rows.Count; i++)
            rows[i].RowNumber = i + 1;

        return (true, string.Empty, new UidHistoryResultDto
        {
            SearchedUid = uid,
            Rows = rows
        });
    }
}