using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using Data.Models;
using EmailTranslation.Hangfire;
using Hangfire;
using Hangfire.MemoryStorage;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Interfaces;
using Services.Services;

namespace EmailTranslation
{
    public class Startup
    {
        private object recurringJobManager;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IEmailProcessingService, EmailProcessingService>();
            services.AddHangfireServer();

            services.AddHangfire(config =>
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseDefaultTypeSerializer()
            .UseMemoryStorage());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IEmailService emailService,
            IEmailProcessingService emailProcessingService,
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseRouter(builder =>
            {
                builder.MapGet("", context =>
                {
                    context.Response.Redirect("/hangfire", permanent: false);
                    return Task.FromResult(0);
                });
            });

            var options = new DashboardOptions
            {
                Authorization = new[] {
                    new HangfireAuthorizationFilter (new[]
                    {
                        new HangfireUserCredentials
                        {
                            Username = Environment.GetEnvironmentVariable("HangfireUsername"),
                            Password = Environment.GetEnvironmentVariable("HangfirePassword")
                        }
                    })
                }
            };

            app.UseHangfireDashboard("/hangfire", options);


            recurringJobManager.AddOrUpdate(
                "Process incoming emails every minute",
                () => serviceProvider.GetService<IEmailProcessingService>().ProcessIncomingEmails(),
                "0 * * ? * *"
                );

            recurringJobManager.AddOrUpdate(
            "Process outgoing emails every minute",
            () => serviceProvider.GetService<IEmailProcessingService>().ProcessOutgoingEmails(),
            "0 * * ? * *"
            );
        }
    }
}
