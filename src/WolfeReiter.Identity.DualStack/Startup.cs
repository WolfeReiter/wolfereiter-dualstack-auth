using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            services.AddDbContext<PgSqlContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PgSqlConnection")));
            services.AddDbContext<SqlServerContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection")));

            _ = (Configuration.GetValue<string>("EntityFramework:Driver")) switch
            {
                "PostgreSql" => services.AddDataProtection().PersistKeysToDbContext<PgSqlContext>(),
                "SqlServer" => services.AddDataProtection().PersistKeysToDbContext<SqlServerContext>(),
                _ => throw new InvalidOperationException("The EntityFramework:Driver configuration value must be set to \"PostgreSql\" or \"SqlServer\".")
            };

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            _ = (Configuration.GetValue<string>("DistributedCache:Driver")) switch
            {
                //don't use DistributedMemoryCache in production with multiple web workers, 
                //it is just a cache in the local process memory
                "Memory"    => services.AddDistributedMemoryCache(),
                "Redis"     => services.AddStackExchangeRedisCache(options => 
                            { 
                                options.Configuration = Configuration.GetConnectionString("RedisConnection"); 
                            }),
                "SqlServer" => services.AddDistributedSqlServerCache(options =>
                            {
                                options.ConnectionString = Configuration.GetConnectionString("SqlServerConnection");
                                options.SchemaName = "dbo";
                                options.TableName = "Cache";
                            }),
                _ => throw new InvalidOperationException("The DistributedCache:Driver configuration value must be set to \"Memory\", \"Redis\"or \"SqlServer\"."),
            };

            // Sign-in users with the Microsoft identity platform
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi(new string[] { "User.Read", "Directory.Read.All" })
                .AddDistributedTokenCaches();
            services.AddWolfeReiterAzureGroupsClaimsTransform(Configuration);

            //override AuthenticationOptions set by AddMicrosoftIdentityWebAppAuthentication() 
            //to allow sign-in with forms and cookies instead of as a side-effect of calling
            //services.AddAuthentication(Action<AuthenticationOptions>). 
            services.ConfigureAll<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            //IMvcBuilder.AddMicrosoftIdentityUI() sets AccessDeniedPath using ConfigureAll<T>.
            //Calling services.ConfigureApplicationCookie(Action<ConfigrationOptions>) doesn't work correctly because
            //AddMicrosoftIdentityUI() can't see that AccessDeniedPath was set through that mechanism.
            services.ConfigureAll<CookieAuthenticationOptions>(options =>
            {
                options.LoginPath = "/Account/SignInMethod"; //override default /Account/Login
                options.AccessDeniedPath = "/Account/Denied"; //override default /MicrosoftIdentity/Account/AccessDenied
                options.LogoutPath = "/Account/SignOut";
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Administration,
                    policy => policy.RequireRole(Policies.RequiredRoles.Administration));
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
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
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

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using SharedDbContext context = (Configuration.GetValue<string>("EntityFramework:Driver")) switch
            {
                "PostgreSql" => scope.ServiceProvider.GetService<PgSqlContext>(),
                "SqlServer" => scope.ServiceProvider.GetService<SqlServerContext>(),
                _ => throw new InvalidOperationException("The EntityFramework:Driver configuration value must be set to \"PostgreSql\" or \"SqlServer\"."),
            };
            context.Database.Migrate();
            var transformer = new DbDataTransformer(Configuration, env, context);
            int rowsAffected = await transformer.ApplyStartupTransformAsync();
            logger.Log(LogLevel.Information, $"DbDataTransformer.ApplyStartupTransformAsync() affected {rowsAffected} rows.");
        }
    }
}
