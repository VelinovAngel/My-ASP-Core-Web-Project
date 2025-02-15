﻿namespace BikesBooking.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    using BikeBooking.Service.CronJobs;
    using BikesBooking.Data;
    using BikesBooking.Data.Common;
    using BikesBooking.Data.Common.Repositories;
    using BikesBooking.Data.Models;
    using BikesBooking.Data.Repositories;
    using BikesBooking.Data.Seeding;
    using BikesBooking.Services.Data.Client;
    using BikesBooking.Services.Data.Contact;
    using BikesBooking.Services.Data.Dealer;
    using BikesBooking.Services.Data.Home;
    using BikesBooking.Services.Data.Motorcycle;
    using BikesBooking.Services.Data.User;
    using BikesBooking.Services.Data.Votes;
    using BikesBooking.Services.Mapping;
    using BikesBooking.Services.Messaging;
    using BikesBooking.Services.Services;
    using BikesBooking.Web.Infrastructure.Filters.Hangfire;
    using BikesBooking.Web.ViewModels;
    using CloudinaryDotNet;
    using Hangfire;
    using Hangfire.SqlServer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using static BikesBooking.Common.GlobalConstants;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options
                .UseSqlServer(this.configuration.GetConnectionString("DefaultConnection"))
                .UseLazyLoadingProxies());

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            // Add Hangfire
            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSqlServerStorage(
                this.configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true,
                });
            });

            services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = this.configuration[FacebookLogin.AppId];
                    options.AppSecret = this.configuration[FacebookLogin.AppSecret];
                })
                .AddGoogle(option =>
                {
                    option.ClientId = this.configuration[GoogleLogin.AppId];
                    option.ClientSecret = this.configuration[GoogleLogin.AppSecret];
                });

            // Add clodinary
            var cloudinary = new Cloudinary(new Account()
            {
                Cloud = this.configuration["Cloudinary:CloudName"],
                ApiKey = this.configuration["Cloudinary:APIKey"],
                ApiSecret = this.configuration["Cloudinary:APISecret"],
            });
            services.AddSingleton(cloudinary);

            services.Configure<CookiePolicyOptions>(
                options =>
                    {
                        options.CheckConsentNeeded = context => true;
                        options.MinimumSameSitePolicy = SameSiteMode.None;
                    });
            services.AddMemoryCache();
            services.AddResponseCaching();
            services.AddAntiforgery(option =>
            {
                option.HeaderName = "X-CSRF-TOKEN";
            });

            services.AddControllersWithViews(
                options =>
                    {
                        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    }).AddRazorRuntimeCompilation();
            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddSingleton(this.configuration);

            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // Application services
            services.AddTransient<IHomeService, HomeService>();
            services.AddTransient<ICloudinaryService, CloudinaryService>();
            services.AddTransient<IMotorcycleService, MotorcycleService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient<IDealersService, DealersService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<IEmailSenderService, EmailSenderService>();
            services.AddTransient<IVoteService, VoteService>();

            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IRecurringJobManager recurringJobManager)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Database.Migrate();

                new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();

                SeedHangfireJobs(recurringJobManager);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            RequestLocalizationOptions localizationOptions = new()
            {
                SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US") },
                SupportedUICultures = new List<CultureInfo> { new CultureInfo("en-US") },
                DefaultRequestCulture = new RequestCulture("en-US"),
            };

            app.UseRequestLocalization(localizationOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseResponseCaching();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/administrator/statistic", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
            });
            app.UseHangfireServer();

            app.UseEndpoints(
                endpoints =>
                    {
                        endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                        endpoints.MapControllerRoute(
                            name: "Motocycle Details",
                            pattern: "/Client/Details/{id}/{information}",
                            defaults: new { controller = "Client", action = "Details" });

                        endpoints.MapControllerRoute(
                           name: "Motocycle Details",
                           pattern: "/Client/BookThisModel/{id}/{information}",
                           defaults: new { controller = "Client", action = "BookThisModel" });

                        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapRazorPages();
                    });
        }

        private static void SeedHangfireJobs(IRecurringJobManager recurringJobManager)
        {
            recurringJobManager.AddOrUpdate<DeletePastOffers>("DeletePastOffers", x => x.Work(), Cron.Daily);
        }
    }
}
