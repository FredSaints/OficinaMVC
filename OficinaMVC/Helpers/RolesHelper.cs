namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Provides a list of predefined application roles.
    /// </summary>
    public static class RolesHelper
    {
        /// <summary>
        /// Gets the list of predefined roles in the application.
        /// </summary>
        public static List<string> Roles => new() { "Admin", "Receptionist", "Mechanic", "Client" };
    }
}
