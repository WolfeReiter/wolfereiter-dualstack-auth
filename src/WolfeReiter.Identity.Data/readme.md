# EF Core with migrations for PgSql and SqlServer

Use the `--context` and `--output-dir` arguments with `dotnet ef migrations`

```
[master] $ dotnet ef migrations add InitialCreate --context PgSqlContext --output-dir Migrations/PgSqlMigrations -v
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
[master] $ dotnet ef migrations add InitialCreate --context SqlServerContext --output-dir Migrations/SqlServerMigrations
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```