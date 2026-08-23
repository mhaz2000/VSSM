#nullable disable

namespace VSSM.Infrastructure.Data.Models;

public partial class TblRecipient
{
    // Primary Key
    public int RecipientId { get; set; }

    // Basic Information
    public string Code { get; set; }
    public string FullName { get; set; }
    public DateTime? EntryDate { get; set; }
    public bool? IsActive { get; set; }
    public string Note { get; set; }

    // Hierarchy
    public int? ParentId { get; set; }
    public int? ChainLevel { get; set; }

    // Operational Information
    public int? TransportModeId { get; set; }
    public int? ReceivingModeId { get; set; }
    public int? Facility { get; set; }
    public int? LeadTime { get; set; }
    public int? ClimateId { get; set; }
    public int? OwnershipId { get; set; }
    public int? PowerSourceId { get; set; }
    public int? TypeOfRoadId { get; set; }

    // Statistical Information
    public long? Distance { get; set; }
    public long? Population { get; set; }
    public int? Year { get; set; }
    public long? DistanceTime { get; set; }
    public DateTime? StartUp { get; set; }

    // Contact & Address
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    // Capabilities
    public bool? HasLandLine { get; set; }
    public bool? HasInternet { get; set; }

    // Reference Navigation Properties
    public virtual TblBaseInfo Climate { get; set; }
    public virtual TblBaseInfo Ownership { get; set; }
    public virtual TblBaseInfo PowerSource { get; set; }
    public virtual TblBaseInfo ReceivingMode { get; set; }
    public virtual TblBaseInfo TransportMode { get; set; }
    public virtual TblBaseInfo TypeOfRoad { get; set; }

    // Arrival Relations
    public virtual ICollection<TblArrival> TblArrivalReceivers { get; set; }
        = new List<TblArrival>();

    public virtual ICollection<TblArrival> TblArrivalSenders { get; set; }
        = new List<TblArrival>();

    // Dispatch Relations
    public virtual ICollection<TblDispatch> TblDispatchReceivers { get; set; }
        = new List<TblDispatch>();

    public virtual ICollection<TblDispatch> TblDispatchSenders { get; set; }
        = new List<TblDispatch>();

    // Item Configuration Relations
    public virtual ICollection<TblMinMaxItem> TblMinMaxItems { get; set; }
        = new List<TblMinMaxItem>();

    public virtual ICollection<TblRecipientConfiguration> TblRecipientConfigurations { get; set; }
        = new List<TblRecipientConfiguration>();

    public virtual ICollection<TblRecipientMaxItem> TblRecipientMaxItemReceivers { get; set; }
        = new List<TblRecipientMaxItem>();

    public virtual ICollection<TblRecipientMaxItem> TblRecipientMaxItemRecipients { get; set; }
        = new List<TblRecipientMaxItem>();

    // Request Relations
    public virtual ICollection<TblRequest> TblRequestReceivers { get; set; }
        = new List<TblRequest>();

    public virtual ICollection<TblRequest> TblRequestSenders { get; set; }
        = new List<TblRequest>();

    // Store Relations
    public virtual ICollection<TblStore> TblStores { get; set; }
        = new List<TblStore>();

    // Other Relations
    public virtual ICollection<TblDestroyUid> DestroyUids { get; set; }
        = new List<TblDestroyUid>();
}