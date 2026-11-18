using ByteStore.Data;
using ByteStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONFIGURACION DE LA BASE DE DATOS
// =====================================================

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

// =====================================================
// ASP.NET CORE IDENTITY
// =====================================================

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();

// =====================================================
// SESIONES Y CARRITO
// =====================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Permite acceder a HttpContext desde los servicios.
builder.Services.AddHttpContextAccessor();

// Registrar el servicio del carrito.
builder.Services.AddScoped<CarritoService>();

// =====================================================
// CONSTRUCCION DE LA APLICACION
// =====================================================

var app = builder.Build();

// =====================================================
// CONFIGURACION DEL ENTORNO
// =====================================================

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

// =====================================================
// RUTAS MVC
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// =====================================================
// CREACION DEL ROL ADMINISTRADOR
// =====================================================

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

// =====================================================
// INICIAR BYTESTORE
// =====================================================

app.Run();
