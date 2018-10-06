using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ServerCore.Models;
using ServerCore.DataModel;
using ServerCore.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace ServerCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDbContext<PuzzleServerContext>
                (options => options.UseLazyLoadingProxies()
                    .UseSqlServer(Configuration.GetConnectionString("PuzzleServerContext")));

            //services.AddIdentity<GarbageUser, IdentityRole>()
            //    .AddEntityFrameworkStores<GarbageContext>()
            //    .AddDefaultTokenProviders();

            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ApplicationId"];
                microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:Password"];
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                PuzzleServerContext.UpdateDatabase(app);
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
