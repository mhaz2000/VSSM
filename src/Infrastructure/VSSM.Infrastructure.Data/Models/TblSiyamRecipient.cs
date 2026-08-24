using System.Collections.Generic;
using VSSM.Domain.ValueObjects;

namespace VSSM.Infrastructure.Data.Models;

public partial class TblSiyamRecipient
{
    public string Id { get; set; } = string.Empty;

    public OrgRef? OrganizationId { get; set; }

    public string? HierarchicalOrganizationId { get; set; }
    public virtual TblSiyamRecipient? HierarchicalOrganization { get; set; }
    public virtual ICollection<TblSiyamRecipient> ChildOrganizations { get; set; } = new List<TblSiyamRecipient>();

    public string? Name { get; set; }

    public CodedValue? Type { get; set; }
    public CodedValue? DependencyType { get; set; }
    public CodedValue? AllegianceType { get; set; }
    public CodedValue? OwnershipStatus { get; set; }
    public CodedValue? Specialty { get; set; }
    public CodedValue? ActivityStatus { get; set; }

    public OrgRef? OwnerOrganization { get; set; }
    public OrgRef? ObserverOrganization { get; set; }

    public CodedValue? NationalAreaCode { get; set; }
    public CodedValue? Country { get; set; }
    public CodedValue? Province { get; set; }
    public CodedValue? Town { get; set; }
    public CodedValue? District { get; set; }
    public CodedValue? RuralArea { get; set; }
    public CodedValue? City { get; set; }
    public CodedValue? Village { get; set; }

    public string? SpecialtyTypeJson { get; set; }
    public string? OtherIdentityInfoJson { get; set; }
    public string? ExtendedPropertyJson { get; set; }

    public string? PostalCode { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }

    public PartialDate? EstablishmentDate { get; set; }
    public PartialDate? ActivationDate { get; set; }
}
