namespace JwtWebApiRolesProj2
{
    public class UserDto            //Would use this Dto (Data transfer object) for registration and logging into an actual app; Would also store this in the app database...
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;        // Normally would have a confirm password associated, but this is just a working reference example...


    }
}
