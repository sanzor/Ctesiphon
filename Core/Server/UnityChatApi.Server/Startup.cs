using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASPT.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ASPT.Server {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            Config config = this.Configuration.GetSection("config").Get<Config>();
            services.Configure<Config>(this.Configuration.GetSection("config"));
            services.AddControllers();
            services.AddSwaggerGen(x => {
                x.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
                    Title = config.Swagger.Title,
                    Version = config.Swagger.Version,
                    Description = config.Swagger.Description,
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact {
                        Name = "Bercovici Adrian",
                        Email = "bercovici.adrian.simon@gmail.com"
                    },
                });
                x.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement());

                x.AddSecurityDefinition("sec", new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                    Description = "logging system for the Leplace App",
                    Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows {
                        Implicit = new Microsoft.OpenApi.Models.OpenApiOAuthFlow(),

                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(x => {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "versions");
            });
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
