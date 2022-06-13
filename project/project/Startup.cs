using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using project.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using project.CustomTokenProviders;
using project.AuthData;
using Microsoft.AspNetCore.Authorization;
using Dal;
using Bal;
using Dal.Data;
using Dal.Models;
using Bal.Leaves;

namespace project
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddControllersWithViews(config =>
        config.Filters.Add(typeof(CustomExceptionFilter)));
            //services.AddSingleton<IAuthorizationPolicyProvider, AuthorizeAttribute>();
            services.AddDbContext<HelperlandContextData>(options => options.UseSqlServer(Configuration.GetConnectionString("Myconnection")));
            //services.AddDbContext<HelperLanddContext>(optionsBuilder =>
            // optionsBuilder.UseSqlServer(Configuration.GetConnectionString("Myconnection"), options => options.EnableRetryOnFailure()));

            //services.AddIdentity<ApplicationUser, IdentityRole<long>>(options =>
            //{
            //    options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(30);
            //    // This allows the username to have valid email characters.
            //    // See: https://en.wikipedia.org/wiki/Email_address
            //    options.User.AllowedUserNameCharacters = String.Empty;
            //})
            //    .AddEntityFrameworkStores<ApplicationDatasource, long>()
            //    .AddDefaultTokenProviders();
            services.AddIdentity<Users, IdentityRole<int>>(options =>
            {
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.SignIn.RequireConfirmedEmail = true; // checks if the mail is confirmed  or not
                options.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
            })              
              .AddEntityFrameworkStores<HelperlandContextData>()
              .AddDefaultTokenProviders()
              .AddTokenProvider<EmailConfirmationTokenProvider<Users>>("emailconfirmation");

            //services.AddDefaultIdentity<User>(options =>
            //  options.SignIn.RequireConfirmedAccount = true)
            //    .AddRoles<IdentityRole>() //Line that can help you
            //    .AddEntityFrameworkStores<HelperlandContext>();


            services.AddScoped<ILeaveServices, LeaveServices>();
            services.AddScoped<IDeleteTab, DeleteTab>();
            services.AddScoped<IUpdateLeave, UpdateLeave>();
            services.AddScoped<ILeavePageSearch, LeavePageSearch>();
            services.AddScoped<ILeavePageClear, LeavePageClear>();
            services.AddScoped<IAddSalary, AddSalary>();
            services.AddScoped<IGiveSalary, GiveSalary>();
            services.AddScoped<ISeeSalary, SeeSalary>();
            services.AddScoped<ISeeSalary, SeeSalary>();
            services.AddScoped<IAdminaddsEmp, AdminaddsEmp>();
            services.AddScoped<ILeavePageHome, LeavePageHome>();

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromMinutes(1));
            services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromDays(3));
            //===================== Instead I copied this options portion above in IdentityRole Section ===================
            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequiredLength = 7;
            //    options.Password.RequireNonAlphanumeric = false;
            //});

            //  *******************************  for Cookies ************************************* 
            // services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.ConsentCookie.IsEssential = true;//<--NOTE THIS
            //    options.CheckConsentNeeded = context => false;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options =>
            //    {
            //        options.Cookie.IsEssential = true;//<--NOTE THIS
            //        options.Cookie.HttpOnly = true;
            //        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //        options.Cookie.SameSite = SameSiteMode.None;
            //        options.LoginPath = "/Starting/Index";
            //        //options.LogoutPath = "/Account/Login";
            //    });


            //var policy = new AuthorizeAttribute("permission")
            //      .RequireAuthenticatedUser()
            //      .Build();
            services.AddMvc().AddXmlDataContractSerializerFormatters();
            //services.AddMvc(options =>
            //{
            //    options.Filters.Add(new AuthorizeAttribute());
            //    options.Filters.Add(typeof(AuthorizeAttribute));
            //});
            
            //services.AddScoped<IMyService>
            //    services.AddRazorPages()
            //.AddMvcOptions(options => options.Filters.Add(new AuthorizeAttribute()));
            //services.AddAuthorization(options =>
            //{
            //    options.DefaultPolicy = new AuthorizeAttribute()
            //      .RequireAuthenticatedUser()
            //      .Build();
            //});
            services.AddAuthorization();
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
            }
            app.UseStaticFiles();

            app.UseRouting();
            //app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Starting}/{action=Index}/{id?}");
            });
        }
    }
}
