using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using static WuyiPlay_DAL.Common.DataType;

namespace WuyiPlay_Api.Configurations
{
    public static class AuthorizationPolicySetup
    {
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Dùng ClaimTypes.Role thay vì "role" để tránh remapping issue
                options.AddPolicy("RequireAdmin", policy =>
                    policy.RequireClaim(ClaimTypes.Role, ((int)role.Admin).ToString()));

                options.AddPolicy("RequireCollaborator", policy =>
                    policy.RequireClaim(ClaimTypes.Role, ((int)role.Collaborator).ToString()));

                options.AddPolicy("RequireCustomer", policy =>
                    policy.RequireClaim(ClaimTypes.Role, ((int)role.Customer).ToString()));

                options.AddPolicy("RequireAdminOrCollaborator", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == ClaimTypes.Role &&
                            (c.Value == ((int)role.Admin).ToString() ||
                             c.Value == ((int)role.Collaborator).ToString()))));
            });
        }

        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // KHÔNG clear map nữa — để "role" tự remap sang ClaimTypes.Role
            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // BỎ DÒNG NÀY

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],      // Nhất quán với appsettings.json
                    ValidAudience = configuration["Jwt:Audience"],  // Nhất quán với appsettings.json
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero  // Không cho phép trễ khi kiểm tra expire
                };
            });
        }
    }
}