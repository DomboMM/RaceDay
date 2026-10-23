var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add HttpClient for communication with RaceDayAPI
builder.Services.AddHttpClient<RaceDayMVC.Services.RaceDayApiService>(
    client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["RaceDayApi:BaseUrl"]
            ?? "https://localhost:7136/");
    });

// Add session services
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Allow controllers to access HttpContext
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Enable session before authorization
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

// Configure default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();