using System;
using System.Collections.Generic;

namespace WolfeReiter.Identity.DualStack.Security
{
    public static class Roles
    {
        //same roles need to be defined in both AzureAD And the database.
        public const string Administrator = "WolfeReiter.Identity.DualStack.Administrator";
        public const string User = "WolfeReiter.Identity.DualStack.User";
    }
}