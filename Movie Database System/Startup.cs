using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Movie_Database_System
{
    public class Startup
    {
        public static IWebHostEnvironment hostEnvironment;
        public static string databaseConnStr, blobStorageConnStr;
        public Startup(IConfiguration configuration, IWebHostEnvironment _hostEnvironment)
        {
            Configuration = configuration;
            hostEnvironment = _hostEnvironment;

            string decryptionKey = Configuration.GetValue<string>("ConnectionStrings:DerAlteWurfeltNicht").ToString();
            string encryptedDbConnStr = Configuration.GetValue<string>("ConnectionStrings:MovieAppDB").ToString();
            string encryptedBlobConnStr = Configuration.GetValue<string>("ConnectionStrings:MovieAppAzureBlobStorage").ToString();

            databaseConnStr = Movie_Database_System.ConnectionMgmt.DecryptString(decryptionKey, encryptedDbConnStr);
            blobStorageConnStr = Movie_Database_System.ConnectionMgmt.DecryptString(decryptionKey, encryptedBlobConnStr);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddControllers();
     
            services.AddWebEncoders(o => {
                o.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(UnicodeRanges.All);
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            }); 
        }
    }
}
