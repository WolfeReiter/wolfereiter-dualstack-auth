using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WolfeReiter.Identity.Data;
using WolfeReiter.Identity.Data.Models;
using WolfeReiter.Identity.DualStack.Security;

namespace WolfeReiter.Identity.DualStack
{
    public class DbDataTransformer
    {
        readonly IConfiguration Configuration;
        readonly IWebHostEnvironment Env;
        readonly SharedDbContext DbContext;

        public DbDataTransformer(IConfiguration configuration, IWebHostEnvironment env, SharedDbContext dbContext)
        {
            Configuration = configuration;
            Env = env;
            DbContext = dbContext;
        }

        public async Task<int> ApplyStartupTransformAsync()
        {
            int rowsAffected = 0;

            rowsAffected += await AddRoles();
            rowsAffected += await AddSysadminUser();
            if (Env.IsDevelopment())
            {
                rowsAffected += await AddFakeAccounts("fake", 50);
            }
            return rowsAffected;
        }

        async Task<int> AddRoles()
        {
            const string transformId = "AddRoles";
            int rowsAffected = 0;

            if (!DbContext.DataTransformsHistory.Where(x => x.TransformId == transformId).Any())
            {
                DbContext.DataTransformsHistory.Add(new DataTransformsHistory { TransformId = transformId });
                DbContext.Roles.Add(new Role { Name = Roles.Administrator });
                DbContext.Roles.Add(new Role { Name = Roles.User });
                rowsAffected = await DbContext.SaveChangesAsync();
            }

            return rowsAffected;
        }

        async Task<int> AddSysadminUser()
        {
            const string transformId = "AddSysadminUser";
            int rowsAffected = 0;

            if (!DbContext.DataTransformsHistory.Where(x => x.TransformId == transformId).Any())
            {
                DbContext.DataTransformsHistory.Add(new DataTransformsHistory { TransformId = transformId });
                
                var administratorRoleId = DbContext.Roles
                    .Where(x => x.Name == Security.Roles.Administrator)
                    .Select(x => x.RoleId)
                    .Single();

                var sysadminUserId = Guid.NewGuid();

                /*
                [master] $ cd tools/pbkdf2/
                [master] $ dotnet run password
                Hash: HUl6Au5B3OeAbPkHXwwYfv3YSkMu6UecyHBGwDHghJI=
                Salt: MDatqlpx8UcfpXmzdS3H0NFEe/iBCgr2xxJkBbOZ7+Q=
                */

                //insert user record
                DbContext.Users.Add(new User()
                {
                    UserId = sysadminUserId,
                    Name = "sysadmin",
                    Email = Configuration.GetValue<string>("Setup:SysadminEmail") ?? string.Empty,
                    // initial password is "password"
                    Hash = Convert.FromBase64String("HUl6Au5B3OeAbPkHXwwYfv3YSkMu6UecyHBGwDHghJI="),
                    Salt = Convert.FromBase64String("MDatqlpx8UcfpXmzdS3H0NFEe/iBCgr2xxJkBbOZ7+Q=")
                });

                //insert join to Administrator Role record.
                DbContext.Add(new UserRole { UserId = sysadminUserId, RoleId = administratorRoleId });

                rowsAffected += await DbContext.SaveChangesAsync();
            }

            return rowsAffected;
        }

        async Task<int> AddFakeAccounts(string pattern, int number)
        {
            const string transformId = "AddFakeAccounts";
            int rowsAffected = 0;

            if (!DbContext.DataTransformsHistory.Where(x => x.TransformId == transformId).Any())
            {

                var fakeDomain = Configuration.GetValue<string>("Setup:FakeUserDomain");
                var fakePrefix = Configuration.GetValue<string>("Setup:FakeUserPrefix");
                
                DbContext.DataTransformsHistory.Add(new DataTransformsHistory { TransformId = transformId });

                for (int i = 0; i < number; i++)
                {
                    DbContext.Users.Add(new User()
                    {
                        Name = $"{pattern}-{i}",
                        Email = $"{fakePrefix}+{pattern}-{i}@{fakeDomain}",
                        //unusable password
                        Hash = new byte[0],
                        Salt = new byte[0]
                    });
                }
                rowsAffected += await DbContext.SaveChangesAsync();
            }
            return rowsAffected;
        }
    }
}