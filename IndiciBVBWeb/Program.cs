using IndiciBVBWeb.Data;
using IndiciBVBWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=IndiciBVBs}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        using (var driverBVB = new ChromeDriver())
        {
            driverBVB.Navigate().GoToUrl("https://m.bvb.ro/TradingAndStatistics/Trading/MarketsToday");
            var rowsBVB = driverBVB.FindElement(By.CssSelector("#gv.small-table"))
                                   .FindElements(By.CssSelector("tbody tr"));

            foreach (var row in rowsBVB)
            {
                var nume = row.FindElement(By.CssSelector("td[align='left'] a")).Text;
                var values = row.FindElements(By.CssSelector("td[align='right']"));

                if (values.Count >= 3)
                {
                    double valoareUnitara = Convert.ToDouble(values[0].Text.Replace(".", "").Replace(",", "."));
                    double crestereValoare = Convert.ToDouble(values[1].Text.Replace(".", "").Replace(",", "."));
                    double crestereProcent = Convert.ToDouble(values[2].Text.Replace("%", "").Replace(",", "."));

                    context.IndiciBVB.Add(new IndiciBVB
                    {
                        Nume = nume,
                        ValoareUnitara = valoareUnitara,
                        CastigValoare = crestereValoare,
                        CastigProcent = crestereProcent,
                        Data = DateTime.Now,
                        Sursa = "BVB"
                    });
                }
            }

            await context.SaveChangesAsync();
        }
        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Scraping error at startup: {ex.Message}");
    }
}
app.Run();


