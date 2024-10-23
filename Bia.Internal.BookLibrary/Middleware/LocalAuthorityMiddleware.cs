using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Bia.Internal.BookLibrary.Data;
using Microsoft.AspNetCore.Hosting;

namespace Bia.Internal.BookLibrary.Middleware
{
    public class LocalAuthorityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _log;
        private readonly IConfiguration _config;

        public LocalAuthorityMiddleware(RequestDelegate next, ILogger<LocalAuthorityMiddleware> log,  IConfiguration configuration)
        {
                _next = next;
                _log = log;
                _config = configuration;
        }

        public async Task Invoke(HttpContext context, BookDbContext ctx)
        {
            try
            {
                _log.LogDebug("LocalAuthorityMiddleware Invoked");

                if (context.Request.Path.StartsWithSegments("/images/covers"))
                {
                    await _next(context);
                    return;
                }

                if (context.User.Identity.IsAuthenticated)
                {
                    _log.LogDebug($"Local authority claims check for user [{context.User.Identity.Name}]");
                    //TODO check cookies
                    string sam;
                    try 
                    {
                        sam = context.User.Identity.Name.Split('\\')[1];
                    }
                    catch (IndexOutOfRangeException) 
                    { 
                        sam = context.User.Identity.Name;
                    }
                    var us = ctx.AppUsers.Where(res => res.LoginName == sam);
                    var dbUser = us.SingleOrDefault();
                    if (dbUser == null)
                    {
                        _log.LogWarning("LocalAuthorityMiddleware dbUser == null");
                        AppUser user;
                        AccessGroupId AccessGroup;
                        var claims = new List<Claim>();
                        if (_config["DefaultDevs"].Split(',').Any(dev => dev.Contains(sam)))
                        {
                            user = new Admin();
                            AccessGroup = AccessGroupId.Developer;
                            claims.Add(new Claim(Data.S.AccessLevelClaim, AccessGroupId.Developer.ToString()));
                        }
                        else if (_config["DefaultAdmins"].Split(',').Any(adm => adm.Contains(sam)))
                        {
                            user = new Admin();
                            AccessGroup = AccessGroupId.Admin;
                            claims.Add(new Claim(Data.S.AccessLevelClaim, AccessGroupId.Admin.ToString()));
                        }
                        else
                        {
                            AccessGroup = AccessGroupId.Common;
                            user = new AppUser();
                            claims.Add(new Claim(Data.S.AccessLevelClaim, AccessGroupId.Common.ToString()));
                        }

                        try
                        {
                            dbUser = user.NewUserFromActiveDirectory(sam);
                            //
                            context.Items["AppUser"] = dbUser;
                        }
                        catch (ActiveDirectoryUserNotFoundException ex)
                        {
                            _log.LogWarning(ex, $"User {context.User.Identity.Name} creation failed");
                            context.Items["originalPath"] = context.Request.Path.Value;
                            context.Request.Path = "/Account/AccessDenied";
                            await _next(context);
                            return;
                        }


                        dbUser.AccessGroup = AccessGroup;
                        foreach (var claim in claims)
                        {
                            ((ClaimsIdentity) context.User.Identity).AddClaim(claim);
                        }

                        ctx.AppUsers.Add(dbUser);
                        ctx.SaveChanges();
                        _log.LogTrace("LocalAuthorityMiddleware SaveChangesAsync");
                        await ctx.SaveChangesAsync();

                        await _next(context);
                    }
                    else
                    {
                        if (!dbUser.IsActive)
                        {
                            context.Items["originalPath"] = context.Request.Path.Value;
                            context.Request.Path = "/Account/AccessDenied";
                            await _next(context);
                        }
                        else if (dbUser.AccessGroup != 0)
                        {
                            var claims = (dbUser.AccessGroup).GetClaimsValues();
                            ((ClaimsIdentity)context.User.Identity).AddClaims(claims);
                            //
                            context.Items["AppUser"] = dbUser;
                            //
                            _log.LogWarning($"Local authority claims check for user [{context.User.Identity.Name}] [ok]");
                        }
                        await _next(context);
                    }
                }
                else
                {
                    _log.LogTrace($"LocalAuthorityMiddleware user not Authenticated {context.User.Identity.Name}");
                    await _next(context);
                }
            }
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
            {
                _log.LogError(ex, "LocalAuthorityMiddleware failed");
                await _next(context);
            }
        }
    }
}