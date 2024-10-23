#define AuthorizationSpecificUser

using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using WebApiBookLibrary;
using static Org.BouncyCastle.Math.EC.ECCurve;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IConfiguration configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .Build();
//
/*
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate(options =>
    {
        options.Events = new NegotiateEvents
        {
            OnAuthenticated = context =>
            {
                var username = context.Principal?.Identity?.Name;
                if (string.IsNullOrEmpty(username) || username != "DELLIN\\azachitaylov")
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    context.Fail("Access denied.");
                    return Task.CompletedTask;
                }

                return Task.CompletedTask;
            }
        };
    });
*/
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

/*
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AllowSpecificUser", policy =>
        policy.RequireAssertion(context =>
            context.User.Identity?.Name == "DELLIN\\azachitaylov"));
});
*/

#if AuthorizationSpecificUser
Dictionary<string, string> usersInfo = configuration.GetSection("UsersInfo").Get<Dictionary<string, string>>();
string domainName = configuration["DomainName"];
builder.Services.AddAuthorization(options =>
{ 
    options.AddPolicy("AllowSpecificUsers", policy =>
    {
        policy.RequireAssertion(context =>
           context.User != null &&
           context.User.Identity != null &&
           !string.IsNullOrEmpty(context.User.Identity.Name) &&
           usersInfo.Values.Contains(context.User.Identity.Name.Replace("DELLIN\\", "")));
    });
});
#else
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = options.DefaultPolicy);
#endif

var app = builder.Build();

string connectionString = configuration.GetConnectionString("dbConnection");
string libraryUrl = configuration.GetSection("LibraryUrl").Get<string>();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/GetUserInfo", async (string login) =>
{
    UserInfo userInfo = new UserInfo();
    return await userInfo.GetUserInfo(connectionString, login, libraryUrl);
})
#if AuthorizationSpecificUser
.WithName("GetUserInfo")
.RequireAuthorization("AllowSpecificUsers");
#else
.WithName("GetUserInfo");
#endif

//async (HttpContext context, string login) =>
app.MapGet("/GetAdminInfo", (HttpContext context, string login) =>
{
    AdminInfo? adminInfo = new AdminInfo();
    adminInfo = adminInfo.Load(connectionString, login);
    if (adminInfo != null)
    {
        return adminInfo;
    }
    else
    {
        context.Response.StatusCode = 400;
        //await context.Response.WriteAsync($"Пользователь с логином '{login}' не является администратором");
        return null;
    }
})
#if AuthorizationSpecificUser
.WithName("GetAdminInfo")
.RequireAuthorization("AllowSpecificUsers");
#else
.WithName("GetUserInfo");
#endif

app.Run();
