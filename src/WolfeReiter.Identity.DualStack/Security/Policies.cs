using System;

namespace WolfeReiter.Identity.DualStack.Security
{
    public static class Policies
    {
        public const string Administration = "Administration";

        public static class RequiredRoles
        {
            public static readonly string[] Administration = new[] { Roles.Administrator };
        }
        
    }
}

