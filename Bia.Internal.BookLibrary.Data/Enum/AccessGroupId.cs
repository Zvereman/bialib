using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// Used to define user's access level.
    /// </summary>
    public enum AccessGroupId
    {
        None = 0,
        Developer = 1,
        Admin = 2,
        Common = 4
    }

    public static class AccessGroupIdExtentions
    {
        public static IList<Claim> GetClaimsValues(this AccessGroupId access)
        {
            var claims = new List<Claim>();

            if (access == AccessGroupId.Common)
            {
                claims.Add(new Claim(S.AccessLevelClaim, AccessGroupId.Common.ToString()));
                return claims;
            }
            if (access == AccessGroupId.Developer)
            {
                claims.Add(new Claim(S.AccessLevelClaim, AccessGroupId.Developer.ToString()));
                claims.Add(new Claim(S.AccessLevelClaim, AccessGroupId.Admin.ToString()));
                claims.Add(new Claim(S.AccessLevelClaim, AccessGroupId.Common.ToString()));
                return claims;
            }
            if (access == AccessGroupId.Admin)
            {
                claims.Add(new Claim(S.AccessLevelClaim, AccessGroupId.Admin.ToString()));
                claims.Add(new Claim(S.AccessLevelClaim, AccessGroupId.Common.ToString()));
                return claims;
            }
            
            return claims;
        }
    }
}
