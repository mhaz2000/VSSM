using Microsoft.Extensions.Caching.Memory;
using VSSM.Application.DTOs.BaseEntities.Dispatches.Scan;
using VSSM.Domain.Enums;


public class DispatchScanSessionService(IMemoryCache cache)
{

    private string GetKey(string sessionId)
    {
        return $"dispatch_scan_{sessionId}";
    }



    public DispatchScanSession GetSession(string sessionId)
    {

        return cache.Get<DispatchScanSession>(
            GetKey(sessionId))
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



        if (session.ScannedUids.Contains(package.Uid))
        {
            return false;
        }


        if (package.Level == UidLevel.Second)
        {

            int childId = package.ChildIds.First();


            if (session.SelectedChildIds.Contains(childId))
            {
                return false;
            }


            session.SelectedChildIds.Add(childId);

        }
        else
        {

            foreach (var child in package.ChildIds)
            {

                if (session.SelectedChildIds.Contains(child))
                {
                    return false;
                }

            }


            foreach (var child in package.ChildIds)
            {
                session.SelectedChildIds.Add(child);
            }

        }




        session.Items.Add(package);


        session.ScannedUids.Add(package.Uid);


        session.PackageIndex[package.Uid]
            = package;



        session.ItemId ??= package.ItemId;

        session.BatchNumber ??= package.BatchNumber;



        SaveSession(session);


        return true;
    }






    public bool RemovePackage(
        string sessionId,
        string uid)
    {

        var session = GetSession(sessionId);


        if (!session.PackageIndex.TryGetValue(uid, out var package))
        {
            return false;
        }

        session.Items.Remove(package);

        session.ScannedUids.Remove(uid);

        session.PackageIndex.Remove(uid);


        foreach (var child in package.ChildIds)
        {
            session.SelectedChildIds.Remove(child);
        }



        SaveSession(session);


        return true;
    }

    public void Clear(string sessionId)
    {
        cache.Remove(GetKey(sessionId));
    }

}