namespace APP.Eds.Services.Config;

public static class Configuration
{
    // ✅ API Base URLs
    public static string BaseUrl => "https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds";
    
    // ✅ Keycloak Authentication
    public static string KeycloakUrl => "https://keycloak-0yrtq1-u44828.vm.elestio.app/realms"; 
    public static string KeycloakCliendId => "application-eds";
    public static string KeycloakRealms => "AppEDS";
    
    // ✨ NUEVO: Electronic Billing API URLs
    public static string BillingApiUrl => "https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/billing";
    public static string PdfApiUrl => "https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/pdfinvoice/pdf";
    
    // ✨ NUEVO: Billing API Authentication
    public static string BillingApiToken => "59884a7d9bca1eb502186c76";
    public static string BillingApiEnvironment => "production-billing";
    
    // ✨ Feature Flags
    /// <summary>
    /// Controls whether automatic version checking is enabled on app startup.
    /// Set to false to disable automatic version checks (useful for testing before full integration).
    /// </summary>
    public static bool EnableAutoVersionCheck => false; // TODO: Set to true after testing
}
