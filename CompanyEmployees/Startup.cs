using System.Collections.Generic;
using System.IO;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using CompanyEmployees.Utility;
using Contracts;
using Entities.DTOs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NLog;
using Repository;
using Repository.DataShaping;

namespace CompanyEmployees
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ValidationFilterAttribute>();
            services.AddScoped<ValidateCompanyExistsAttribute>();
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
            services.AddScoped <IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
            services.AddScoped<ValidateMediaTypeAttribute>();
            services.AddScoped<EmployeeLinks>();
            services.AddScoped<IAuthenticationManager, AuthenticationManager>();
            
            
            services.ConfigureCors();
            services.ConfigureIisIntegration();
            services.ConfigureLoggerService();
            
            services.ConfigureSqlContext(Configuration);

            services.ConfigureRepositoryManagement();

            services.AddAutoMapper(typeof(Startup));
            
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.ConfigureVersioning();
            
            services.ConfigureResponseCaching();
            
            services.AddAuthentication(); 
            services.ConfigureIdentity();
            services.ConfigureJWT(Configuration);

            services.AddControllers(config =>
            {
                config.RespectBrowserAcceptHeader = false;
                config.ReturnHttpNotAcceptable = false;
                config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
            }).AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters()
                .AddCustomCSVFormatter();
            
            services.AddCustomMediaTypes();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Code Maze API", Version = "v1"});
                c.SwaggerDoc("v2", new OpenApiInfo {Title = "Code Maze API", Version = "v2"});
                
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header, Description = "Place to add JWT with Bearer", 
                    Name = "Authorization", 
                    Type = SecuritySchemeType.ApiKey, Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme, Id = "Bearer"
                            },
                            Name = "Bearer"
                        },
                        new List<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Maze API v1"));
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "Code Maze API v2"));
            }
            else
            {
                app.UseHsts();
            }
            
            app.ConfigureExceptionHandler(logger);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("CorsPolicy");

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseResponseCaching();
            
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}