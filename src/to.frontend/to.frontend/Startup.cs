﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using to.contracts;
using to.frontend.Constants;
using to.frontend.Factories;
using to.frontend.Helper;
using to.frontend.Models.Backlog;

namespace to.frontend
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
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => { options.LoginPath = "/Login"; });

            services.AddAuthorization(options =>
            {
                var permissions = Enum.GetValues(typeof(Permission));
                foreach (var permission in permissions)
                {
                    options.AddPolicy(permission.ToString(), policy => policy.RequireClaim(CustomClaims.Permission, permission.ToString()));
                }
            });

            services.AddSingleton<IRequestHandlerFactory, RequestHandlerFactory>();
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddSingleton<IApplicationState, ApplicationState>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            AutoMapper.Mapper.Initialize(config =>
            {
                config.CreateMap<BacklogCreationRequest, CreateBacklogViewModel>().ReverseMap();
                config.CreateMap<BacklogOrderQueryResult, BacklogEvalViewModel>().ReverseMap();
                config.CreateMap<BacklogEvalQueryResult, BacklogEvalViewModel>().ReverseMap();
                config.CreateMap<BacklogOrderRequest, BacklogOrderViewModel>().ReverseMap();
                config.CreateMap<BacklogOrderRequestViewModel, BacklogOrderRequest>()
                    .ForMember(d => d.UserStoryIndexes, s => s.MapFrom(str => str.UserStoryIndexes.Split(',', StringSplitOptions.None).Select(Int32.Parse).ToList()));
            });
        }
    }
}
