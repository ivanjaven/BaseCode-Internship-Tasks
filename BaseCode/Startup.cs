using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BaseCode.Models;

namespace BaseCode
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

            var db_host = Environment.GetEnvironmentVariable("DB_HOST");
            var db_port = Environment.GetEnvironmentVariable("DB_PORT");
            var db_name = Environment.GetEnvironmentVariable("DB_NAME");
            var db_user = Environment.GetEnvironmentVariable("DB_USER");
            var db_password = Environment.GetEnvironmentVariable("DB_PASS");

            var conn = "Server=" + db_host + ";Port=" + db_port + ";Database=" + db_name + ";Uid=" + db_user + ";Pwd=" + db_password + ";Convert Zero Datetime=True";
            services.Add(new ServiceDescriptor(typeof(DBContext), new DBContext(conn)));

            // NCM database connection
            var ncm_host = Environment.GetEnvironmentVariable("NCM_DB_HOST") ?? db_host;
            var ncm_port = Environment.GetEnvironmentVariable("NCM_DB_PORT") ?? db_port;
            var ncm_name = Environment.GetEnvironmentVariable("NCM_DB_NAME") ?? "ncm";
            var ncm_user = Environment.GetEnvironmentVariable("NCM_DB_USER") ?? db_user;
            var ncm_password = Environment.GetEnvironmentVariable("NCM_DB_PASS") ?? db_password;

            var ncmConn = "Server=" + ncm_host + ";Port=" + ncm_port + ";Database=" + ncm_name + ";Uid=" + ncm_user + ";Pwd=" + ncm_password + ";Convert Zero Datetime=True";

            var templateService = new TemplateService(ncmConn);
            services.AddSingleton<TemplateService>(templateService);
            services.AddSingleton<DBContext>(new DBContext(conn));

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;

            });
            services.AddHttpContextAccessor();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            //app.UseAuthentication();

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                //GetSettingResponse list = new GetSettingResponse();
                //var services = serviceScope.ServiceProvider;
                //var dbcon = services.GetService<DBContext>();
                //list = dbcon.GetSettingList("CORS");
                //foreach (Settings value in list.settings)
                //{

                //    }
            }


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
