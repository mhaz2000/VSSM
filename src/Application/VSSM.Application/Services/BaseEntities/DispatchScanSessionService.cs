using Microsoft.Extensions.Caching.Memory;
using VSSM.Application.DTOs.BaseEntities.Dispatches.Scan;

namespace VSSM.Application.Services.BaseEntities;

public class DispatchScanSessionService(IMemoryCache cache)
{
    private string GetKey(string sessionId)
    {
        return $"dispatch_scan_{sessionId}";
    }



    public DispatchScanSession GetSession(string sessionId)
    {
        return cache.Get<DispatchScanSession>(GetKey(sessionId))
               ??
               new DispatchScanSession
               {
                   SessionId = sessionId
               };
    }



    public void SaveSession(
        DispatchScanSession session)
    {
        cache.Set(
            GetKey(session.SessionId),
            session,
            TimeSpan.FromMinutes(60));
    }



    public bool AddPackage(
        string sessionId,
        ScannedPackage package)
    {
        var session = GetSession(sessionId);


        if (session.Items.Any(x => x.Uid == package.Uid))
        {
            return false;
        }


        session.Items.Add(package);

        SaveSession(session);

        return true;
    }



    public bool RemovePackage(
        string sessionId,
        string uid)
    {
        var session = GetSession(sessionId);


        var item = session.Items
            .FirstOrDefault(x => x.Uid == uid);


        if (item == null)
            return false;


        session.Items.Remove(item);


        session.RemovedUids.Add(uid);


        SaveSession(session);


        return true;
    }



    public void Clear(string sessionId)
    {
        cache.Remove(GetKey(sessionId));
    }
}
