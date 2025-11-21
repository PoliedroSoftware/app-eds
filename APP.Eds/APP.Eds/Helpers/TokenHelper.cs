using System;
using System.Text;
using Microsoft.Maui.Storage;

namespace APP.Eds.Helpers
{
    public static class TokenHelper
    {
        private const string TOKEN_KEY = "AUTH_ACCESS_TOKEN";
        private const string REFRESH_TOKEN_KEY = "AUTH_REFRESH_TOKEN";
        private const string TOKEN_PART_COUNT_KEY = "AUTH_TOKEN_PART_COUNT";
        private const string TOKEN_PART_PREFIX = "AUTH_TOKEN_PART_";

        /// <summary>
        /// Carga el token de acceso almacenado
        /// </summary>
        public static string LoadToken()
        {
            try
            {
                int chunkCount = Preferences.Get(TOKEN_PART_COUNT_KEY, 0);
                if (chunkCount == 0) return null;

                var tokenBuilder = new StringBuilder();
                for (int i = 0; i < chunkCount; i++)
                {
                    tokenBuilder.Append(Preferences.Get($"{TOKEN_PART_PREFIX}{i}", string.Empty));
                }
                return tokenBuilder.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading token: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Guarda el token de acceso
        /// </summary>
        public static void SaveToken(string token)
        {
            try
            {
                var tokenParts = SplitTokenIntoParts(token);
                Preferences.Set(TOKEN_PART_COUNT_KEY, tokenParts.Length);

                for (int i = 0; i < tokenParts.Length; i++)
                {
                    Preferences.Set($"{TOKEN_PART_PREFIX}{i}", tokenParts[i]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving token: {ex}");
            }
        }

        /// <summary>
        /// Carga el refresh token almacenado
        /// </summary>
        public static string LoadRefreshToken()
        {
            try
            {
                return Preferences.Get(REFRESH_TOKEN_KEY, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading refresh token: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Guarda el refresh token
        /// </summary>
        public static void SaveRefreshToken(string refreshToken)
        {
            try
            {
                Preferences.Set(REFRESH_TOKEN_KEY, refreshToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving refresh token: {ex}");
            }
        }

        /// <summary>
        /// Limpia todos los tokens almacenados
        /// </summary>
        public static void ClearTokens()
        {
            try
            {
                int chunkCount = Preferences.Get(TOKEN_PART_COUNT_KEY, 0);
                for (int i = 0; i < chunkCount; i++)
                {
                    Preferences.Remove($"{TOKEN_PART_PREFIX}{i}");
                }
                
                Preferences.Remove(TOKEN_PART_COUNT_KEY);
                Preferences.Remove(REFRESH_TOKEN_KEY);
                
                System.Diagnostics.Debug.WriteLine("All tokens cleared successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing tokens: {ex}");
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
