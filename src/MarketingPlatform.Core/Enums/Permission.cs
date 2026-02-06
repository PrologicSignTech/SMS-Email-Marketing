namespace MarketingPlatform.Core.Enums
{
    [Flags]
    public enum Permission : long
    {
        // Campaign Permissions
        ViewCampaigns = 1L << 0,           // 1
        CreateCampaigns = 1L << 1,         // 2
        EditCampaigns = 1L << 2,           // 4
        DeleteCampaigns = 1L << 3,         // 8
        
        // Contact Permissions
        ViewContacts = 1L << 4,            // 16
        CreateContacts = 1L << 5,          // 32
        EditContacts = 1L << 6,            // 64
        DeleteContacts = 1L << 7,          // 128
        
        // Template Permissions
        ViewTemplates = 1L << 8,           // 256
        CreateTemplates = 1L << 9,         // 512
        EditTemplates = 1L << 10,          // 1024
        DeleteTemplates = 1L << 11,        // 2048
        
        // Analytics Permissions
        ViewAnalytics = 1L << 12,          // 4096
        ViewDetailedAnalytics = 1L << 13,  // 8192
        ExportAnalytics = 1L << 14,        // 16384
        
        // User Management Permissions
        ViewUsers = 1L << 15,              // 32768
        CreateUsers = 1L << 16,            // 65536
        EditUsers = 1L << 17,              // 131072
        DeleteUsers = 1L << 18,            // 262144
        
        // Role Management Permissions
        ViewRoles = 1L << 19,              // 524288
        ManageRoles = 1L << 20,            // 1048576
        
        // Settings Permissions
        ViewSettings = 1L << 21,           // 2097152
        ManageSettings = 1L << 22,         // 4194304
        
        // Compliance Permissions
        ViewCompliance = 1L << 23,         // 8388608
        ManageCompliance = 1L << 24,       // 16777216
        
        // Workflow Permissions
        ViewWorkflows = 1L << 25,          // 33554432
        CreateWorkflows = 1L << 26,        // 67108864
        EditWorkflows = 1L << 27,          // 134217728
        DeleteWorkflows = 1L << 28,        // 268435456
        
        // Audit Log Permissions
        ViewAuditLogs = 1L << 29,          // 536870912
        
        // Super Admin Permissions
        ViewSuperAdminLogs = 1L << 30,     // 1073741824
        ManageSuperAdmins = 1L << 31,      // 2147483648
        ViewPlatformConfig = 1L << 32,     // 4294967296
        ManagePlatformConfig = 1L << 33,   // 8589934592
        
        // All Permissions (SuperAdmin)
        All = ~0L
    }
}
