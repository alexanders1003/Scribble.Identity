namespace Scribble.Identity.Models.Base;

public static class AppData
{
    public const string ServiceName = "Scribble Identity Service";
    public const string PolicyName = "CorsPolicy";
    public const string SystemAdministrator = "Administrator";
    public const string SystemModerator = "Moderator";
    public const string SystemUser = "User";

    public static IEnumerable<string> Roles
    {
        get
        {
            yield return SystemAdministrator;
            yield return SystemModerator;
            yield return SystemUser;
        }
    }
}