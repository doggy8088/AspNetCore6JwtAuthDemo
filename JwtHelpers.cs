using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public class JwtHelpers
{
    private readonly IConfiguration Configuration;

    public JwtHelpers(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    public string GenerateToken(string userName, int expireMinutes = 30)
    {
        var issuer = Configuration.GetValue<string>("JwtSettings:Issuer");
        var signKey = Configuration.GetValue<string>("JwtSettings:SignKey");

        // Configuring "Claims" to your JWT Token
        var claims = new List<Claim>();

        // In RFC 7519 (Section#4), there are defined 7 built-in Claims, but we mostly use 2 of them.
        //claims.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));
        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName)); // User.Identity.Name
        //claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "The Audience"));
        //claims.Add(new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds().ToString()));
        //claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())); // 必須為數字
        //claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())); // 必須為數字
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // JWT ID

        // The "NameId" claim is usually unnecessary.
        //claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userName));

        // This Claim can be replaced by JwtRegisteredClaimNames.Sub, so it's redundant.
        //claims.Add(new Claim(ClaimTypes.Name, userName));

        // TODO: You can define your "roles" to your Claims.
        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        claims.Add(new Claim(ClaimTypes.Role, "Users"));

        var userClaimsIdentity = new ClaimsIdentity(claims);

        // Create a SymmetricSecurityKey for JWT Token signatures
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));

        // HmacSha256 MUST be larger than 128 bits, so the key can't be too short. At least 16 and more characters.
        // https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        // Create SecurityTokenDescriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            //Audience = issuer, // Sometimes you don't have to define Audience.
            //NotBefore = DateTime.Now, // Default is DateTime.Now
            //IssuedAt = DateTime.Now, // Default is DateTime.Now
            Subject = userClaimsIdentity,
            Expires = DateTime.Now.AddMinutes(expireMinutes),
            SigningCredentials = signingCredentials
        };

        // Generate a JWT, than get the serialized Token result (string)
        var tokenHandler = new JsonWebTokenHandler();
        var serializeToken = tokenHandler.CreateToken(tokenDescriptor);

        return serializeToken;
    }
}
