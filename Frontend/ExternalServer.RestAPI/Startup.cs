using System;
using System.Text;
using ExternalServer.Common.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace ExternalServer.RestAPI {
    public class Startup {
        private IConfiguration _configuration;

        public Startup() {
            _configuration = ConfigurationContainer.Configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        LifetimeValidator = (notBefore, expires, securityToken, validationParameters) => {
                            if (expires == null || expires.Value > DateTime.UtcNow) {
                                return true;
                            }

                            return false;
                        },
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _configuration[ConfigurationVars.JWT_ISSUER],
                        ValidAudience = _configuration[ConfigurationVars.JWT_ISSUER],
                        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[ConfigurationVars.ISSUER_SIGNINGKEY]))
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestKey1asdfasdfasdfe3"))
                    };
                });

            services.AddControllers();
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        //public void ConfigureContainer(ContainerBuilder builder) {
        //    IoC.RegisterToContainerBuilder(ref builder);
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
