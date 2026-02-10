namespace MarketingPlatform.Web.Models;

/// <summary>
/// DataTables request model for server-side processing
/// </summary>
public class DataTablesRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public DataTablesSearch? Search { get; set; }
    public List<DataTablesOrder>? Order { get; set; }
    public int? Status { get; set; }
    public int? Channel { get; set; }
    public bool? IsActive { get; set; }

    // Additional filters for Users
    public int? RoleId { get; set; }

    // Additional filters for Contacts
    public int? GroupId { get; set; }

    // Additional filters for Suppression/Providers
    public string? Type { get; set; }
}

public class DataTablesSearch
{
    public string? Value { get; set; }
}

public class DataTablesOrder
{
    public string? Column { get; set; }
    public string? Dir { get; set; }
}
