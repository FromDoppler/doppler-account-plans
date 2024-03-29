using System;
using Doppler.AccountPlans.Encryption;
using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.TimeCollector;
using Doppler.AccountPlans.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Doppler.AccountPlans
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
            services.Configure<DopplerDatabaseSettings>(Configuration.GetSection(nameof(DopplerDatabaseSettings)));
            services.AddDopplerSecurity();
            services.AddRepositories();
            services.AddControllers();
            services.AddCors();
            services.AddSingleton<Weather.WeatherForecastService>();
            services.AddSingleton<Weather.DataService>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.Configure<EncryptionSettings>(Configuration.GetSection(nameof(EncryptionSettings)));
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddTimeCollector(Configuration);

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter the token into field as 'Bearer {token}'",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme },
                            },
                            Array.Empty<string>()
                        }
                    });

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Doppler.AccountPlans", Version = "v1" });

                var baseUrl = Configuration.GetValue<string>("BaseURL");
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    c.AddServer(new OpenApiServer() { Url = baseUrl });
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Doppler.AccountPlans v1"));

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(policy => policy
                .SetIsOriginAllowed(isOriginAllowed: _ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
