using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Bia.Internal.BookLibrary.Data;
using Bia.Internal.BookLibrary.Middleware;
using EmailSender;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wangkanai.Detection;

namespace Bia.Internal.BookLibrary
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            S.Initializate(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.AddDbContext<BookDbContext>(options => options.UseMySql(
            //         Configuration.GetConnectionString(Data.S.ConnectionStringName),
            //         optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(BookDbContext).Namespace))
            //         );
            services.AddDbContext<BookDbContext>(options => options.UseMySql(
                     Configuration.GetConnectionString(Data.S.ConnectionStringName),
                     optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(BookDbContext).Namespace))
                     );

            services.AddTransient(q => new EmailService(S.MailHost, S.MailUser, S.MailPassword));

            services.AddDetectionCore().AddBrowser();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Data.S.GroupDeveloper, policy => policy.RequireClaim(Data.S.AccessLevelClaim, new[] { Data.S.Developer }));
                options.AddPolicy(Data.S.GroupAdmin, policy => policy.RequireClaim(Data.S.AccessLevelClaim, new[] { Data.S.Admin }));
                options.AddPolicy(Data.S.GroupCommon, policy => policy.RequireClaim(Data.S.AccessLevelClaim, new[] { Data.S.Common }));

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/500");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            //app.MapWhen(context => context.Request.Path.StartsWithSegments("/images/covers"), appBuilder =>
            //{
            //    appBuilder.UseStaticFiles();
            //});

            //app.MapWhen(context => context.Request.Path.StartsWithSegments("/images/covers"), appBuilder =>
            //{
            //    appBuilder.UseStaticFiles();
            //});

            app.UseMiddleware<LocalAuthorityMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                //routes.MapRoute(
                //    name: "api",
                //    template: "ApiController/{action=Index}/{id?}",
                //    defaults: new { controller = "ApiController" });
            });

        }

        //private static void UpdateDatabase(IApplicationBuilder app)
        //{

        //    using (var serviceScope = app.ApplicationServices
        //        .GetRequiredService<IServiceScopeFactory>()
        //        .CreateScope())
        //    {
        //        using (var context = serviceScope.ServiceProvider.GetService<BookDbContext>())
        //        {
        //            context.Database.Migrate();
        //        }
        //    }
        //}
    }
}
