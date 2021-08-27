using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AdvertApi.Services;
using AdvertApi.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Amazon.Util;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;

namespace AdvertApi
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
     
            // services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            //});
    
            services.AddAutoMapper(options=> { 
            });
            services.AddTransient<IAdvertStorageService, DynamoDBAdvertStorage>();
            services.AddControllers();
           
            //To add health check 
            services.AddHealthChecks()
                .AddCheck<StorageHealthCheck>("Storage");
            services.AddCors(options =>
            {
                options.AddPolicy("AllOrigin", policy => policy.WithOrigins("*").AllowAnyHeader());
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Web Advertisement Apis",
                    Version = "version 1",
                    Contact = new OpenApiContact()
                    {
                        Name = "",
                        Email = ""
                    }
                });
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // For Linux Apache Deployment
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdvertApi v1"));
            }

            app.UseHealthChecks("/health");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();


            //await RegisterToCloudMap();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
   
        }

        private async Task RegisterToCloudMap()
        {
             string serviceId = Configuration.GetValue<string>("AwsCloudMapServiceId");
            var instanceId = EC2InstanceMetadata.InstanceId;
            if (!string.IsNullOrEmpty(instanceId))
            {
                var ipv4 = EC2InstanceMetadata.PrivateIpAddress;
                var client = new AmazonServiceDiscoveryClient();
               await client.RegisterInstanceAsync(new RegisterInstanceRequest
                {
                    InstanceId = instanceId,
                    ServiceId = serviceId,
                    Attributes = new Dictionary<string, string>()
                    {
                        {"AWS_INSTANCE_IPV4",ipv4 },
                        { "AWS_INSTANCE_PORT", "80"}
                    }
                });

            }
        }
    }
}
