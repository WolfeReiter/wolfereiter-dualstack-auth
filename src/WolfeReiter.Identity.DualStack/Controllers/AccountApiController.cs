using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WolfeReiter.Identity.Data;
using WolfeReiter.Identity.DualStack.Models;
using WolfeReiter.Identity.DualStack.Security;

namespace WolfeReiter.Identity.DualStack.Controllers
{
    [ApiController]
    [Route("/api/account")]
    [Produces(MediaTypeNames.Application.Json)]
    [Authorize(Policy = Policies.Administration)]
    public class AccountApiController : ControllerBase
    {
        readonly ILogger<AccountController> Logger;
        readonly SharedDbContext DbContext;
        readonly IConfiguration Configuration; 

        public AccountApiController(IConfiguration configuration, ILogger<AccountController> logger,
            PgSqlContext pgContext, SqlServerContext sqlContext)
        {
            Configuration = configuration;
            Logger        = logger;
            DbContext = (configuration.GetValue<string>("EntityFramework:Driver")) switch
            {
                "PostgreSql" => pgContext,
                "SqlServer" => sqlContext,
                _ => throw new InvalidOperationException("The EntityFramework:Driver configuration value must be set to \"PostgreSql\" or \"SqlServer\"."),
            };
        }

        [HttpGet("data")]
        public async Task<IActionResult> Data(int draw = 1, int length = 10, int start = 0)
        {
            /**crazy jagged array of dictionary argument encoding from datatables.net is impossible to bind to an argument
            
            Sample querystring decoded as a markdown table.

            | Name | Value  |
            |------|------- |
            | draw | 2 |
            | columns[0][data] | first_name |
            | columns[0][name] |  |
            | columns[0][searchable] | true |
            | columns[0][orderable] | true |
            | columns[0][search][value] |  |
            | columns[0][search][regex] | false |
            | columns[1][data] | last_name |
            | columns[1][name] |  |
            | columns[1][searchable] | true |
            | columns[1][orderable] | true |
            | columns[1][search][value] |  |
            | columns[1][search][regex] | false |
            | columns[2][data] | position |
            | columns[2][name] |  |
            | columns[2][searchable] | true |
            | columns[2][orderable] | true |
            | columns[2][search][value] |  |
            | columns[2][search][regex] | false |
            | columns[3][data] | office |
            | columns[3][name] |  |
            | columns[3][searchable] | true |
            | columns[3][orderable] | true |
            | columns[3][search][value] |  |
            | columns[3][search][regex] | false |
            | columns[4][data] | start_date |
            | columns[4][name] |  |
            | columns[4][searchable] | true |
            | columns[4][orderable] | true |
            | columns[4][search][value] |  |
            | columns[4][search][regex] | false |
            | columns[5][data] | salary |
            | columns[5][name] |  |
            | columns[5][searchable] | true |
            | columns[5][orderable] | true |
            | columns[5][search][value] |  |
            | columns[5][search][regex] | false |
            | order[0][column] | 0 |
            | order[0][dir] | asc |
            | start | 10 |
            | length | 10 |
            | search[value] |  |
            | search[regex] | false |
            | _ | 1582720018294 |
            
            **/

            //integer index of the column being sorted on as a string
            string index  = Request.Query["order[0][column]"].FirstOrDefault() ?? "1";
            //column name
            string column = Request.Query[$"columns[{index}][data]"].FirstOrDefault() ?? "name";
            //sort order of the sorted column
            string sort   = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";
            //search string from the search box enabled by the "searching" option
            string search = Request.Query["search[value]"].FirstOrDefault();

            int pageSize = length;
            int skip = start;

            IQueryable<Data.Models.User> projection = DbContext.Users;
            if (!string.IsNullOrEmpty(search)) 
            {
                projection = projection.Where(x => x.Name.Contains(search) || x.Email.Contains(search));
                if (int.TryParse(search, out int userNumber)) 
                { projection = projection.Union(DbContext.Users.Where(x => x.UserNumber == userNumber)); }
            }
            int totalRecords = await projection.CountAsync();
            var result = await projection
                .OrderBy($"{column} {sort}")
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var output = new
            {
                draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = result.Select(x => new
                {
                    UserId = x.UserId.ToString("N"),
                    x.UserNumber,
                    x.Name,
                    x.Email,
                    Active = x.Active ? "Enabled" : "Disabled"
                })
            };
            return Ok(output);
        }
    }
}