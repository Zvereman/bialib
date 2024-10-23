using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Hosting.Server;
using Passwork;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//
//builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
//   .AddNegotiate();
builder.Services.AddAuthentication();

//builder.Services.AddAuthorization(options =>
//{
//    // By default, all incoming requests will be authorized according to the default policy.
//    options.FallbackPolicy = options.DefaultPolicy;
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthentication();
//app.UseAuthorization();

//
app.MapGet("/get-connection-string", async () => 
{
    var client = new Client("https://onepass.bia-tech.ru/api/v4");
    var ctx = await client.LoginAsync("TTpFPQM6zCo79xc4pqxcsU5NB7c2U5d3O3Rt6XmqNEo36RjfqsZQLhLIUiOv");
    var vault = await ctx.GetVaultByName("LibraryTest");
    var passes = await vault.GetPasswords();
    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
    foreach (var pass in passes)
    {
        await pass.Unlock();
        keyValuePairs.Add(pass.Name, pass.Pass);
    }
    return new GetConnectionString(keyValuePairs.GetValueOrDefault("Server"), keyValuePairs.GetValueOrDefault("Port"), keyValuePairs.GetValueOrDefault("Database"), 
        keyValuePairs.GetValueOrDefault("Uid"), keyValuePairs.GetValueOrDefault("Pwd"));

}).WithName("GetConnectionString");

app.Run();

internal record GetConnectionString(string? server, string? port, string? database, string? uid, string? pwd)
{
    public string dbConnection => $"Server={server};Port={port};Database={database};Uid={uid};Pwd={pwd}";
}