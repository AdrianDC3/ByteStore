using ByteStore.Data;
using ByteStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// BASE DE DATOS

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

if (builder.Environment.IsProduction() &&
    connectionString.Contains(
        "(localdb)",
        StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "ByteStore necesita una conexion SQL Server de produccion. " +
        "Configure ConnectionStrings__DefaultConnection en el hosting.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// IDENTITY

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// MVC

builder.Services.AddControllersWithViews();

// SESIONES

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SERVICIOS

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<CarritoService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<ProductoService>();

// CONSTRUIR APLICACION

var app = builder.Build();

// MIDDLEWARE

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// RUTAS

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// ROL ADMINISTRADOR

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    const string nombreRol = "Administrador";

    if (!await roleManager.RoleExistsAsync(nombreRol))
    {
        var resultado = await roleManager.CreateAsync(
            new IdentityRole(nombreRol));

        if (!resultado.Succeeded)
        {
            throw new InvalidOperationException(
                "No se pudo crear el rol Administrador.");
        }
    }
}

app.Run();