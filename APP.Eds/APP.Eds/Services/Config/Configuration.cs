namespace APP.Eds.Services.Config;

public static class Configuration
{
    public static string BaseUrl => "https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds";
    public static string KeycloakUrl => "https://keycloak-0yrtq1-u44828.vm.elestio.app/realms"; 
    public static string KeycloakCliendId => "application-eds";
    public static string KeycloakRealms => "AppEDS";
}
