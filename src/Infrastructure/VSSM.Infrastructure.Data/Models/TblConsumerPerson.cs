namespace VSSM.Infrastructure.Data.Models;

public partial class TblConsumerPerson
{
    public int PkConsumerPersonId { get; set; }
    public int FkUid { get; set; }
    public int FkConsumerRecipientId { get; set; }
    public string? ConsumerPerson { get; set; }
    public string Vaccinator { get; set; }
    public int VaccinationRank { get; set; }
    public DateTime EntryDate { get; set; }

    public virtual TblRecipient Recipient { get; set; } = null!;
    public virtual TblUidInfo UidInfo { get; set; } = null!;

}
