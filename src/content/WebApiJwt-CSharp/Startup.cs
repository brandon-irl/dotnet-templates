using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if (JwtAuth)
using System.Text;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if (RequiresHttps)
using Microsoft.AspNetCore.HttpsPolicy;
#endif
using Microsoft.AspNetCore.Mvc;
#if (OrganizationalAuth || IndividualB2CAuth || JwtAuth)
using Microsoft.AspNetCore.Authentication.JwtBearer;
#if (JwtAuth)
using Microsoft.IdentityModel.Tokens;
#endif
#if (OrganizationalAuth || IndividualB2CAuth)
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
#endif
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if (GenerateGraph)
using Microsoft.Graph;
#endif
#if (EnableOpenAPI)
using Microsoft.OpenApi.Models;
#endif
#if (JwtAuth)
using Company.WebApplication1.Authentication;
#endif

namespace Company.WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if (OrganizationalAuth)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
#if (GenerateApiOrGraph)
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi()
#if (GenerateApi)
                        .AddDownstreamWebApi("DownstreamApi", Configuration.GetSection("DownstreamApi"))
#endif
#if (GenerateGraph)
                        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
#endif
                        .AddInMemoryTokenCaches();
#else
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));
#endif
#elif (IndividualB2CAuth)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
#if (GenerateApi)
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAdB2C"))
                    .EnableTokenAcquisitionToCallDownstreamApi()
                        .AddDownstreamWebApi("DownstreamApi", Configuration.GetSection("DownstreamApi"))
                        .AddInMemoryTokenCaches();
#else
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAdB2C"));
#endif
#endif

#if (JwtAuth)
            var section = Configuration.GetSection("AppSettings");
            var settings = section.Get<AppSettings>();
            var signingKey = Encoding.UTF8.GetBytes(settings.EncryptionKey);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtOptions =>
            {
                jwtOptions.SaveToken = true;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ValidateLifetime = true,
                    LifetimeValidator = LifetimeValidator
                };
            });
            bool LifetimeValidator(DateTime? notBefore,
                        DateTime? expires,
                        SecurityToken securityToken,
                        TokenValidationParameters validationParameters)
            {
                return expires != null && expires > DateTime.Now;
            }

            services.Configure<AppSettings>(section);
            services.AddTransient<AuthenticationService>();
            services.AddTransient<UserService>();
            services.AddTransient<TokenService>();
#endif
            services.AddControllers();
#if (EnableOpenAPI)
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Company.WebApplication1", Version = "v1" });
            });
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#if (EnableOpenAPI)
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Company.WebApplication1 v1"));
#endif
            }
#if (RequiresHttps)

            app.UseHttpsRedirection();
#endif

            app.UseRouting();

#if (OrganizationalAuth || IndividualAuth)
            app.UseAuthentication();
#endif
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
