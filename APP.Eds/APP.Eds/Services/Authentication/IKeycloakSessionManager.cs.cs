
namespace APP.Eds.Services.Authentication
{
    public class KeycloakSessionManager
    {
        public void ClearSession(string realm, string clientId)
        {

            Preferences.Remove($"AUTH_{realm}_{clientId}_TOKEN_PART_COUNT");

            int chunkCount = Preferences.Get($"AUTH_{realm}_{clientId}_TOKEN_PART_COUNT", 0);
            for (int i = 0; i < chunkCount; i++)
            {
                Preferences.Remove($"AUTH_{realm}_{clientId}_TOKEN_PART_{i}");
            }

            Preferences.Remove("CURRENT_AUTH_REALM");
            Preferences.Remove("CURRENT_AUTH_CLIENT_ID");

            Console.WriteLine($"Session cleared for realm={realm}, clientId={clientId}");
        }

        
        public void ClearCurrentSession()
        {
            string realm = "AppEDS";
            string clientId = "application-eds";

            ClearSession(realm, clientId);

            Console.WriteLine($"Current session cleared for realm={realm}, clientId={clientId}");
        }
    }
}
