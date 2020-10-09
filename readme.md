# Dualstack Authentication Demo

Web app that implements AzureAD OpenIdConnect authentication with Azure Groups mapped to Role claims and also forms authentication.

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
