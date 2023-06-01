using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OzSapkaTShirt.Data;
using Microsoft.AspNetCore.Identity;
using OzSapkaTShirt.Models;
namespace OzSapkaTShirt

{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ApplicationContext context;
            UserManager<ApplicationUser> userManager;
            RoleManager<IdentityRole> roleManager;
            IdentityRole role;
            ApplicationUser applicationUser;

            builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationContext") ?? throw new InvalidOperationException("Connection string 'ApplicationContext' not found.")));
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireNonAlphanumeric = false;
            })
             .AddEntityFrameworkStores<ApplicationContext>();
               
              

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication(); ;

            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
              name: "areas",
              pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            context = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<ApplicationContext>();
            context.Database.Migrate();
            userManager = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<UserManager<ApplicationUser>>();
            roleManager = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<RoleManager<IdentityRole>>();
            if (roleManager.FindByNameAsync("Administrator").Result == null)
            {
                role = new IdentityRole("Administrator");
                roleManager.CreateAsync(role).Wait();
                applicationUser = new ApplicationUser();
                applicationUser.Name = "Test";
                applicationUser.SurName = "53";
                applicationUser.Corporate = true;
                applicationUser.Address = "Kadýköy";
                applicationUser.Gender = 0;
                applicationUser.BirthDate = DateTime.Now;
                applicationUser.Email = "admin@ozsapka.com";             
                applicationUser.PhoneNumber = "1234567890";                                        
                applicationUser.CityCode = 34;
           
                applicationUser.UserName = "Administrator";
                applicationUser.PassWord = "Abcd.1234";
                applicationUser.ConfirmPassWord = "Abcd.1234";
            


                userManager.CreateAsync(applicationUser, "Abcd.1234").Wait();
                userManager.AddToRoleAsync(applicationUser, "Administrator").Wait();
            }
            app.Run();
        }
    }
}