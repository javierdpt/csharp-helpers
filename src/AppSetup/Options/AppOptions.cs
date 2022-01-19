using Microsoft.IdentityModel.Tokens;
using System;

namespace RentersInsurance.Api.Infrastructure.Options
{
    /// <summary>
    /// App options
    /// </summary>
    public class AppOptions
    {
        /// <summary>
        /// ctor for AppOptions
        /// </summary>
        public AppOptions()
        {
            SecKeyString = Program.Configuration[".ServiceFabric.Settings:SecKeyString"];
            SecKey = new SymmetricSecurityKey(Convert.FromBase64String(SecKeyString));
        }

        /// <summary>Allowed sessions by IP</summary>
        public int AllowedSessionsByIp { get; set; }

        /// <summary>Cors allowed origins</summary>
        public string AllowedOrigins { get; set; }

        /// <summary>
        /// Token generator SymmetricSecurityKey string
        /// </summary>
        public string SecKeyString { get; set; }

        /// <summary>
        /// The SecurityKey based on the SecKeyString
        /// </summary>
        public SymmetricSecurityKey SecKey { get; set; }
    }
}