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
        /// Limpia la sesión actual del usuario
        /// </summary>
        public void ClearSession()
        {
            // Limpiar tokens usando el TokenHelper actualizado
            TokenHelper.ClearTokens();

            // Limpiar preferencias de usuario
            Preferences.Remove("Usernamelogin");
            Preferences.Remove("userRole");

            // Destruir instancia del servicio de Court si existe
            CourtService.DestroyInstance();

            Console.WriteLine("Session cleared successfully");
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
