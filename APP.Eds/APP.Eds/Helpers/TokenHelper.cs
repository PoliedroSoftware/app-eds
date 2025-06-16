using System;
using System.Text;
using Microsoft.Maui.Storage;

namespace APP.Eds.Helpers
{
    public static class TokenHelper
    {
        public static string LoadToken(string clientId, string realm)
        {
            try
            {
                string prefix = $"AUTH_{realm}_{clientId}_";
                int chunkCount = Preferences.Get($"{prefix}TOKEN_PART_COUNT", 0);
                if (chunkCount == 0) return null;

                var tokenBuilder = new StringBuilder();
                for (int i = 0; i < chunkCount; i++)
                {
                    tokenBuilder.Append(Preferences.Get($"{prefix}TOKEN_PART_{i}", string.Empty));
                }
                return tokenBuilder.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading token: {ex}");
                return null;
            }
        }

        public static void SaveToken(string token, string clientId, string realm)
        {
            try
            {
                Preferences.Set("CURRENT_AUTH_REALM", realm);
                Preferences.Set("CURRENT_AUTH_CLIENT_ID", clientId);

                string prefix = $"AUTH_{realm}_{clientId}_";
                var tokenParts = SplitTokenIntoParts(token);
                Preferences.Set($"{prefix}TOKEN_PART_COUNT", tokenParts.Length);

                for (int i = 0; i < tokenParts.Length; i++)
                {
                    Preferences.Set($"{prefix}TOKEN_PART_{i}", tokenParts[i]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving token: {ex}");
            }
        }

        private static string[] SplitTokenIntoParts(string token)
        {
            const int chunkSize = 512;
            int partCount = (int)Math.Ceiling((double)token.Length / chunkSize);

            var parts = new string[partCount];
            for (int i = 0; i < partCount; i++)
            {
                int startIndex = i * chunkSize;
                int length = Math.Min(chunkSize, token.Length - startIndex);
                parts[i] = token.Substring(startIndex, length);
            }
            return parts;
        }
    }
}
