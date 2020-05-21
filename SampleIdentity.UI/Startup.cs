using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleIdentity.UI.CustomValidation;
using SampleIdentity.UI.Helper;
using SampleIdentity.UI.Middleware;
using SampleIdentity.UI.Models.AppUser;

namespace SampleIdentity.UI
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandler>();

            services.AddDbContext<AppIdentityContext>(opts =>
            {
                opts.UseSqlServer(_configuration.GetConnectionString("Laptop"));
            });



            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("GiresunPolicy", policy =>
                 {
                     policy.RequireClaim("city", "Giresun");
                 });

                opts.AddPolicy("ViolencePolicy", policy =>
                 {
                     policy.RequireClaim("violence");
                 });

                opts.AddPolicy("ExchangePolicy", policy =>
                 {
                     policy.AddRequirements(new ExpireDateExchangeRequirement());
                 });

            });

            services.AddAuthentication().AddFacebook(opts =>
            {
                opts.AppId = _configuration["Authentication:Facebook:AppId"];
                opts.AppSecret = _configuration["Authentication:Facebook:AppSecret"];

            }).AddGoogle(opts =>
            {
                opts.ClientId = _configuration["Authentication:Google:ClientId"];
                opts.ClientSecret = _configuration["Authentication:Google:ClientSecret"];
            }).AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = _configuration["Authentication:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = _configuration["Authentication:Microsoft:ClientSecret"];
            });


            services.AddIdentity<AppUser, AppRole>(
                opt =>
                {
                    //username türkçe karakterlere izin verme
                    opt.User.AllowedUserNameCharacters = "abcçdefgðhýijklmnoöpqrsþtüvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
                    //Duplicate Email izin vermez
                    opt.User.RequireUniqueEmail = true;

                    opt.Password.RequiredLength = 4;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequireLowercase = false;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequireDigit = false;

                })
                .AddPasswordValidator<CustomPasswordValidator>()//password kýsýtlamalarý ve hatalarý
                .AddUserValidator<CustomUserValidator>()//User kýsýtlamalarý ve hata ekraný
                .AddErrorDescriber<CustomIdentityErrorDescriber>()//Hatalarýn türkçeleþtirilmesi
                .AddDefaultTokenProviders() //Kullanýcýlara þifre sýfýrlama token gönderilmesini saðlar
                .AddEntityFrameworkStores<AppIdentityContext>();

            services.AddScoped<IPasswordReset, PasswordReset>();


            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "MyBlog";
                options.Cookie.HttpOnly = false; //client side kullanýcýlar cookie eriþememsi için
                options.LogoutPath = new PathString("/Member/Logout");
                options.Cookie.SameSite = SameSiteMode.Lax;// siteler arasý istek hýrsýzlýðý
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;//En uygun mod

                options.ExpireTimeSpan = TimeSpan.FromDays(14);


                options.LoginPath = new PathString("/Home/login");
                options.AccessDeniedPath = new PathString("/Member/AccessDenied"); //Sayfalarý eriþim engeli olduðunda yönlendirelecek sayfa
                options.SlidingExpiration = true;//Expired olacaðý zaman diliminin yarýsýnda sonra sayfayý ziyaret ederse süreyi tekrar uzatacak.


            });

            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();//hata hakkýnda bilgi verir
            app.UseStatusCodePages();

            app.UseRouting();
            app.UseStaticFiles();
           // app.UseNodeModules();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{Controller=Home}/{Action=Index}/{id?}");
            });
        }
    }
}
