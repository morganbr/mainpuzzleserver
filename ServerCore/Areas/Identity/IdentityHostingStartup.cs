using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.Areas.Identity.Data;

[assembly: HostingStartup(typeof(ServerCore.Areas.Identity.IdentityHostingStartup))]
namespace ServerCore.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<GarbageContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("GarbageContextConnection")));

                services.AddDefaultIdentity<GarbageUser>()
                    .AddEntityFrameworkStores<GarbageContext>();
            });
        }
    }
}