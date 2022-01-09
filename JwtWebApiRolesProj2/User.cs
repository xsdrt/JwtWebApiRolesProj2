namespace JwtWebApiRolesProj2
{
    public class User       //This is the user model...
    {
        public string Username { get; set; } = string.Empty;        //Again you would want a user Id and store the user's name and Id in a database in a complete app.

        public byte[] PasswordHash { get; set; }            //So we won't store the password in plain text so we need a byte array with a PasswordHash and PasswordSalt
        public byte[] PasswordSalt { get; set; }
    }
}
