using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using UAB.DAL.LoginDTO;
using UAB.DAL.Models;

namespace UAB
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
            services.AddSingleton<IAuthenticationService1, AuthenticationService>();
            services.AddSingleton<IPasswordAlgorithmFactory, PasswordAlgorithmFactory>();
            services.AddSingleton<IClock, Clock>();
            services.AddSingleton<IPasswordAlgorithm, PasswordAlgorithm>();
            services.AddSingleton<AuthenticationService, AuthenticationService>();
            services.AddMvc();
            services.AddSession();
            services.AddHttpContextAccessor();
           
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(
                        options =>
                        {
                            Configuration.Bind("AzureAd", options);
                            options.Events = new OpenIdConnectEvents
                            {
                                OnTokenValidated = async ctx =>
                                {
                                    var context = ctx.HttpContext.RequestServices.GetRequiredService<AuthenticationService>();
                                    var userInfo = context.GetUserInfoByEmail(ctx.Principal.Identity.Name);
                                    if (userInfo != null)
                                    {
                                        var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.Sid, userInfo.UserId.ToString()),
                                        new Claim(ClaimTypes.Email, userInfo.Email),
                                        new Claim(ClaimTypes.Role, userInfo.RoleName)
                                    };
                                        var appIdentity = new ClaimsIdentity(claims);
                                        ctx.Principal.AddIdentity(appIdentity);
                                    }
                                }
                            };
                        }
                    );

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddRazorPages()
                .AddMicrosoftIdentityUI();
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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "SignIn",
                    pattern: "{controller=Account}/{action=SignIn}");
            });
        }
    }
}
