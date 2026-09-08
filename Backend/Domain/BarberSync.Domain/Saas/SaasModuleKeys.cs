using System;
using System.Collections.Generic;

namespace BarberSync.Domain.Saas;

public static class SaasModuleKeys
{
    public const string Core = "CORE";
    public const string Scheduling = "SCHEDULING";
    public const string ServiceExecution = "SERVICE_EXECUTION";
    public const string PosCash = "POS_CASH";
    public const string Clients360 = "CLIENTS_360";
    public const string RelationshipCrm = "RELATIONSHIP_CRM";
    public const string QualityRetention = "QUALITY_RETENTION";
    public const string MarketingStudio = "MARKETING_STUDIO";
    public const string ClubSales = "CLUB_SALES";
    public const string CatalogPricing = "CATALOG_PRICING";
    public const string TeamHr360 = "TEAM_HR_360";
    public const string InventoryPurchasing360 = "INVENTORY_PURCHASING_360";
    public const string Finance360 = "FINANCE_360";
    public const string AiOperations = "AI_OPERATIONS";
    public const string ReportsBi = "REPORTS_BI";
    public const string Communication = "COMMUNICATION";
    public const string ClientPortal = "CLIENT_PORTAL";
    public const string PublicWeb = "PUBLIC_WEB";
    public const string Mobile = "MOBILE";
    public const string Totem = "TOTEM";
    public const string PartnersMarketplace = "PARTNERS_MARKETPLACE";
    public const string CommandCenter = "COMMAND_CENTER";
    public const string Integrations = "INTEGRATIONS";
}

public static class ModuleContractStatuses
{
    public const string Pending = "Pending";
    public const string PendingActivation = "PendingActivation";
    public const string Trial = "Trial";
    public const string Active = "Active";
    public const string GracePeriod = "GracePeriod";
    public const string PastDue = "PastDue";
    public const string Suspended = "Suspended";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";

    public static readonly IReadOnlySet<string> Entitled = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Trial, Active, GracePeriod
    };
}

public static class PlatformRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "PlatformAdmin";
    public const string Support = "PlatformSupport";
    public const string Billing = "PlatformBilling";
    public const string Auditor = "PlatformAuditor";
}

public static class ModuleAccessReasonCodes
{
    public const string Allowed = "ALLOWED";
    public const string Unauthenticated = "UNAUTHENTICATED";
    public const string TenantInactive = "TENANT_INACTIVE";
    public const string BranchInvalid = "BRANCH_INVALID";
    public const string SubscriptionInactive = "SUBSCRIPTION_INACTIVE";
    public const string ModuleNotEntitled = "MODULE_NOT_ENTITLED";
    public const string ModuleSuspended = "MODULE_SUSPENDED";
    public const string ModuleDisabled = "MODULE_DISABLED";
    public const string PermissionDenied = "PERMISSION_DENIED";
    public const string QuotaExceeded = "QUOTA_EXCEEDED";
    public const string FeatureDisabled = "FEATURE_DISABLED";
}
