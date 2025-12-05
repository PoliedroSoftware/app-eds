using APP.Eds.Helpers;
using APP.Eds.Services.Court;

namespace APP.Eds.Services.Authentication
{
    /// <summary>
    /// Gestor de sesión para el backend (reemplaza KeycloakSessionManager)
    /// </summary>
    public class BackendSessionManager
    {
        /// <summary>
        /// Limpia la sesión actual del usuario y todos los datos de la aplicación.
        /// Este método garantiza que todos los formularios, servicios y estado
        /// de la aplicación se reinicien completamente, previniendo que datos
        /// de sesiones previas permanezcan en caché cuando un nuevo usuario inicia sesión.
        /// </summary>
        public void ClearSession()
        {
            // Limpiar tokens usando el TokenHelper actualizado
            TokenHelper.ClearTokens();

            // Limpiar preferencias de usuario
            Preferences.Remove("Usernamelogin");
            Preferences.Remove("userRole");

            // Reiniciar y destruir servicios singleton con estado
            // Esto asegura que los datos de transacciones, ventas, y formularios
            // no persistan entre sesiones de diferentes usuarios
            CourtService.ResetInstanceFields();
            CourtService.DestroyInstance();

            // Limpiar cualquier dato en caché de Preferences que pueda persistir
            // entre sesiones (excepto configuraciones de sistema)
            ClearApplicationCache();

            Console.WriteLine("Session and application data cleared successfully");
        }

        /// <summary>
        /// Limpia datos en caché de la aplicación que no deberían persistir
        /// entre sesiones de diferentes usuarios.
        /// </summary>
        private void ClearApplicationCache()
        {
            // Lista de claves de Preferences que deben limpiarse en logout
            var keysToRemove = new[]
            {
                "LastSelectedBusiness",
                "LastSelectedEds",
                "CachedFormData",
                // Agregar más claves según sea necesario
            };

            foreach (var key in keysToRemove)
            {
                if (Preferences.ContainsKey(key))
                {
                    Preferences.Remove(key);
                }
            }
        }

        /// <summary>
        /// Verifica si hay una sesión activa
        /// </summary>
        public bool HasActiveSession()
        {
            var token = TokenHelper.LoadToken();
            return !string.IsNullOrEmpty(token);
        }
    }
}
