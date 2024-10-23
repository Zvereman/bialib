using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary
{
    public static class AdHelper
    {
        /// <summary>
        /// Searches a user in ActiveDirectory by provided username. Returns <see cref="AppUser"/> without AccessGroup property set.
        /// Throws an <see cref="ActiveDirectoryUserNotFoundException"/> if user not found in AD.
        /// </summary>
        /// <param name="username">SAM account name.</param>
        /// <returns></returns>
        /// <exception cref="ActiveDirectoryUserNotFoundException"/>
        public static AppUser NewUserFromActiveDirectory(this AppUser user,string username)
        {
            using (var context = new PrincipalContext(ContextType.Domain, Environment.GetEnvironmentVariable("USERDOMAIN") ?? Environment.GetEnvironmentVariable("HOSTNAME")))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    searcher.QueryFilter.SamAccountName = username;
                    var adInfo = (UserPrincipal)searcher.FindOne();

                    if (adInfo == null)
                    {
                        throw new ActiveDirectoryUserNotFoundException();
                    }

                    user.Uid = adInfo.Guid ?? Guid.NewGuid();
                    user.Email = adInfo.EmailAddress;
                    user.IsActive = adInfo.Enabled ?? true;
                    user.FirstName = adInfo.GivenName;
                    user.LastName = adInfo.Surname;
                    user.FullName = adInfo.DisplayName;
                    user.DateJoined = DateTime.Now;
                    user.LoginName = username;
                    
                    return user;
                }
            }
        }
    }
}
