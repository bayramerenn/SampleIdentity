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
                    //username t�rk�e karakterlere izin verme
                    opt.User.AllowedUserNameCharacters = "abc�defg�h�ijklmno�pqrs�t�vwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
                    //Duplicate Email izin vermez
                    opt.User.RequireUniqueEmail = true;

                    opt.Password.RequiredLength = 4;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequireLowercase = false;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequireDigit = false;

                })
                .AddPasswordValidator<CustomPasswordValidator>()//password k�s�tlamalar� ve hatalar�
                .AddUserValidator<CustomUserValidator>()//User k�s�tlamalar� ve hata ekran�
                .AddErrorDescriber<CustomIdentityErrorDescriber>()//Hatalar�n t�rk�ele�tirilmesi
                .AddDefaultTokenProviders() //Kullan�c�lara �ifre s�f�rlama token g�nderilmesini sa�lar
                .AddEntityFrameworkStores<AppIdentityContext>();

            services.AddScoped<IPasswordReset, PasswordReset>();


            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "MyBlog";
                options.Cookie.HttpOnly = false; //client side kullan�c�lar cookie eri�ememsi i�in
                options.LogoutPath = new PathString("/Member/Logout");
                options.Cookie.SameSite = SameSiteMode.Lax;// siteler aras� istek h�rs�zl���
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;//En uygun mod

                options.ExpireTimeSpan = TimeSpan.FromDays(14);


                options.LoginPath = new PathString("/Home/login");
                options.AccessDeniedPath = new PathString("/Member/AccessDenied"); //Sayfalar� eri�im engeli oldu�unda y�nlendirelecek sayfa
                options.SlidingExpiration = true;//Expired olaca�� zaman diliminin yar�s�nda sonra sayfay� ziyaret ederse s�reyi tekrar uzatacak.


            });

            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();//hata hakk�nda bilgi verir
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
