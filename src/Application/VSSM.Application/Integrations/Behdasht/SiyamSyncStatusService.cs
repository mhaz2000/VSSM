namespace VSSM.Application.Integrations.Behdasht;

public class SiyamSyncStatus
{
    public string State { get; set; } = "Idle";
    public int Percent { get; set; }
    public int Total { get; set; }
    public int Processed { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public string? Message { get; set; }
}

public class SiyamSyncStatusService
{
    private readonly object _lock = new();
    private readonly SiyamSyncStatus _status = new();

    public SiyamSyncStatus Snapshot()
    {
        lock (_lock)
        {
            return new SiyamSyncStatus
            {
                State = _status.State,
                Percent = _status.Percent,
                Total = _status.Total,
                Processed = _status.Processed,
                LastSyncDate = _status.LastSyncDate,
                Message = _status.Message
            };
        }
    }

    public bool IsSyncing()
    {
        lock (_lock)
            return _status.State == "Syncing";
    }

    public void SetSyncing()
    {
        lock (_lock)
        {
            _status.State = "Syncing";
            _status.Percent = 0;
            _status.Total = 0;
            _status.Processed = 0;
            _status.Message = null;
        }
    }

    public void UpdateProgress(int processed, int total)
    {
        lock (_lock)
        {
            _status.Processed = processed;
            _status.Total = total;
            _status.Percent = total == 0 ? 0 : (int)((double)processed / total * 100);
        }
    }

    public void SetCompleted(DateTime when)
    {
        lock (_lock)
        {
            _status.State = "Completed";
            _status.Percent = 100;
            _status.Processed = _status.Total;
            _status.LastSyncDate = when;
        }
    }

    public void SetFailed(string message)
    {
        lock (_lock)
        {
            _status.State = "Failed";
            _status.Message = message;
        }
    }
}
