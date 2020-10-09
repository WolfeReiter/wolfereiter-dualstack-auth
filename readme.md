# Dualstack Authentication Demo

Web app that implements AzureAD OpenIdConnect authentication with Azure Groups mapped to Role claims and also forms authentication.

In addition, PostgreSQL And SQL Server database back ends are supported. The database backend is configured at runtime by setting `EntityFramework:Driver` in appsettings. The connection string for the corresponding driver must be set for it to work. The sample configuration uses Docker containers with connection accounts configured. See `start-pgsql.sh` and `start-mssqlserver.sh`. Obviously do not deploy into production with these accounts anywhere on the Internet.

```json
{
  "EntityFrameWork": {
    "Driver": "[set to PostgreSql or SqlServer] or set by Environment"
  },
  "ConnectionStrings": {
    "PgSqlConnection": "Host=localhost;Database=wr_dualstack;Username=wr_dualstack;Password=T2ie7yd1aHBw6MImg02e2tJdKmU9xeUGftaB5sN3ZGhMUdq2qI",
    "SqlServerConnection": "Server=localhost;Initial Catalog=WR_DUALSTACK;User Id=sa;Password=YkBkKCta6zNhlp6buYMAf6tmjjSl4WkIj0R8BfrCXA1zjjQ4PM"
  }
}
```

When the app starts up it generates an admin account in the local database:

| username | password |
|----------|----------|
| sysadmin | password |

The email address for this generated account is configured in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "Setup": {
    "SysadminEmail": "sysadmin@definitely-my-real-domain.fake"
  }
}
```
