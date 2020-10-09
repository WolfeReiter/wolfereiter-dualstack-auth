using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using WolfeReiter.Identity.Data;
using WolfeReiter.Identity.DualStack.Security;

namespace WolfeReiter.Identity.DualStack
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public readonly IConfiguration Configuration;
        public readonly IWebHostEnvironment Env;
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services = Configuration.GetValue<string>("EntityFramework:Driver") switch
            {
                "PostgreSql" => services.AddDbContext<PgSqlContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PgSqlConnection"))),
                "SqlServer"  => services.AddDbContext<SqlServerContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"))),
                _ => throw new InvalidOperationException("The EntityFramework:Driver configuration value must be set to \"PostgreSql\" or \"SqlServer\"."),
            };

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            //in production use DistributedSqlServerCache or Redis Cache
            services.AddDistributedMemoryCache();
            /*
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "host:4445";
            });
            */
            
            // Sign-in users with the Microsoft identity platform
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi(new string[] { "User.Read", "Directory.Read.All" })
                .AddDistributedTokenCaches();
            services.AddWolfeReiterAzureGroupsClaimsTransform(Configuration);

            //sign-in with forms and cookies
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            services.AddHealthChecks();
            services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();
            var mvcBuilder = services.AddRazorPages();
#if DEBUG
            //Razor runtime compilation only on DEBUG build.
            //https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1&tabs=visual-studio
            if (Env.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }
#endif
            services.AddSingleton<SmtpClientService>(new SmtpClientService(Configuration));
            services.AddSingleton<CryptoService>(new CryptoService(Configuration));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHealthChecks("/health");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStatusCodePages();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            using var scope         = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using DbContext context = (Configuration.GetValue<string>("EntityFramework:Driver")) switch
            {
                "PostgreSql" => scope.ServiceProvider.GetService<PgSqlContext>(),
                "SqlServer"  => scope.ServiceProvider.GetService<SqlServerContext>(),
                _ => throw new InvalidOperationException("The EntityFramework:Driver configuration value must be set to \"PostgreSql\" or \"SqlServer\"."),
            };
            context.Database.Migrate();
        }
    }
}
