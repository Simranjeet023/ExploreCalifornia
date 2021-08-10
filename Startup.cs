using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExploreCalifornia.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExploreCalifornia
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<FormattingService>();

            //adding dependency injection using AddTransient method
            services.AddTransient<FeatureToggles>(x => new FeatureToggles
            {
                DeveloperExcaptions = configuration.GetValue<bool>("FeatureToggles:DeveloperExceptions")
            });

            services.AddDbContext<BlogDataContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("BlogDataContext");
                options.UseSqlServer(connectionString);
            });

            services.AddDbContext<IdentityDataContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("IdentityDataContext");
                options.UseSqlServer(connectionString);
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDataContext>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, FeatureToggles feature)
        {

            //to redirect to error html page if something goes bad
            app.UseExceptionHandler("/error.html");

            if (feature.DeveloperExcaptions)
            {
                //help to see more details about an error
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.Contains("invalid"))
                    throw new Exception("ERROR!");

                await next();
            });

            app.UseIdentity();

            //register mvc pattern
            app.UseMvc(routes =>
            {
                //represent the URL
                routes.MapRoute("Default",
                    "{controller=Home}/{action=Index}/{id?}"
                    );
            });

            // to call static files
            //default call to index.html file
            app.UseFileServer();

            ////register multiple middleware and call them using defined sequence
            //app.Use(async (context, next) =>
            //{
            //    if(context.Request.Path.Value.StartsWith("/hello"))
            //    await context.Response.WriteAsync("Hello World!");
            //    //call to next method
            //    await next();
            //});

            //// app.Run method to register middleware
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World1!");
            //});

        }
    }
}
