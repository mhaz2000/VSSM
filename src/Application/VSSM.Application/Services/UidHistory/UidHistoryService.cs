using Microsoft.EntityFrameworkCore;
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

        // 🔹 دریافت نام کاربر سازنده
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

        // 2. دریافت اطلاعات مرتبط
        var arrival = order.Arrival;
        TblDispatch? dispatch = null;
        TblReceiveDispatches? receiveDispatch = null;
        TblConsumerPerson? consumer = null;
        TblDestroyUid? destroy = null;

        // دریافت Dispatch
        var dispatchUid = await db.TblDispatchUids
            .AsNoTracking()
            .Include(d => d.Dispatch)
                .ThenInclude(d => d.Receiver)
            .FirstOrDefaultAsync(d => d.FKUidInfo == uidRecord.PkUidInfo, token);
        if (dispatchUid != null)
            dispatch = dispatchUid.Dispatch;

        // دریافت ReceiveDispatch
        if (uidRecord.FkReceiveDispatchId.HasValue)
        {
            receiveDispatch = await db.TblReceiveDispatches
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PkReceiveDispatchesId == uidRecord.FkReceiveDispatchId, token);
        }

        // دریافت Consumer
        consumer = await db.TblConsumerPeople
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.FkUid == uidRecord.PkUidInfo, token);

        // دریافت Destroy
        destroy = await db.TblDestroyUids
            .AsNoTracking()
            .Include(d => d.DestroyReason)
            .Include(d => d.Location)
            .FirstOrDefaultAsync(d => d.FkUid == uidRecord.PkUidInfo, token);

        var rows = new List<UidHistoryRowDto>();

        var madeDesc = $"تاریخ تولید: {order.ManufactureDate.ToShamsi()}" +
                       $" - نام کالا: {order.Item?.FullName ?? "-"}" +
                       $" - شماره بچ: {order.BatchNumber ?? "-"}" +
                       $" - تعداد دوز: {order.DosePerVial?.Title ?? "-"}" +
                       $" - کاربر: {createdByUserName ?? order.CreatedBy.ToString()}";
        rows.Add(new UidHistoryRowDto
        {
            RowNumber = 0, 
            EventDate = order.CreationTime.ToShamsi(),
            Description = madeDesc,
            SortOrder = 1
        });

        if (arrival != null)
        {
            var reportNumber = arrival.ReportNumber ?? "-";


            var arrivalDesc =
                $"تاریخ ورود: {arrival.Date?.ToShamsi() ?? "-"}" +
                $" - شماره گزارش: \u2066{reportNumber}\u2069" +
                $" - فرستنده: {arrival.Sender?.FullName ?? "-"}" +
                $" - گیرنده: {arrival.Receiver?.FullName ?? "-"}" +
                $" - تاریخ انقضا: {order.ExpirationDate?.ToShamsi() ?? "-"}";

            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventDate = arrival.Date.Value.ToShamsi(),
                Description = arrivalDesc,
                SortOrder = 2
            });
        }

        if (dispatch != null)
        {
            var dispatchDesc = $"تاریخ ارسال: {dispatch.Date.Value.ToShamsi()}" +
                               $" - شماره رسید: {dispatch.ReceiverId}" +
                               $" - دریافت کننده: {dispatch.Receiver?.FullName ?? "-"}";
            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventDate = dispatch.Date.Value.ToShamsi(),
                Description = dispatchDesc,
                SortOrder = 3
            });
        }

        if (receiveDispatch != null)
        {
            var receiveDesc = $"تاریخ دریافت: {receiveDispatch.ReceiveDate.ToShamsi()}" +
                              $" - شماره دریافت: {receiveDispatch.ReceiveNumber}" +
                              $" - وضعیت دریافت: دریافت شده";
            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventDate = receiveDispatch.ReceiveDate.ToShamsi(),
                Description = receiveDesc,
                SortOrder = 4
            });
        }

        if (consumer != null)
        {
            var consumerDesc = $"تاریخ مصرف: {consumer.EntryDate.ToShamsi()}" +
                               $" - گیرنده: {consumer.ConsumerPerson ?? "-"}" +
                               $" - واکسیناتور: {consumer.Vaccinator ?? "-"}" +
                               $" - کد ملی: {consumer.FkConsumerRecipientId}";
            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventDate = consumer.EntryDate.ToShamsi(),
                Description = consumerDesc,
                SortOrder = 5
            });
        }

        if (destroy != null)
        {
            var destroyDesc = $"تاریخ امحا: {destroy.DestroyDate}" +
                              $" - محل امحا: {destroy.Location?.FullName ?? "-"}" +
                              $" - دلیل امحا: {destroy.DestroyReason?.Title ?? "-"}";
            rows.Add(new UidHistoryRowDto
            {
                RowNumber = 0,
                EventDate = destroy.DestroyDate,
                Description = destroyDesc,
                SortOrder = 6
            });
        }

        rows = rows.OrderBy(r => r.SortOrder).ToList();

        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].RowNumber = i + 1;
        }

        var result = new UidHistoryResultDto
        {
            SearchedUid = uid,
            Rows = rows
        };

        return (true, string.Empty, result);
    }
}