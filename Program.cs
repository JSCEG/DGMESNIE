using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using NSIE.Componentes;
using NSIE.Models;
using NSIE.Servicios;
using NSIE.Servicios.Interfaces;  // Actualizar este using
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;    // opcional, facilita tipos como GoogleDefaults
using Microsoft.AspNetCore.Authentication.Facebook;  // opcional



var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
Console.WriteLine("Cadena de conexión en Program.cs: " + builder.Configuration.GetConnectionString("DefaultConnection"));

// Configurar servicios
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader()
                     .WithExposedHeaders("Session-Expiry-Warning");
    });
});


// Configurar Kestrel para permitir grandes solicitudes
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 268435456; // 256 MB
});

// Add services to the container.
//builder.Services.AddSession();
//builder.Services.AddControllersWithViews();
// Configura la serialización JSON globalmente
builder.Services.AddControllersWithViews();
// .AddJsonOptions(options =>
// {
//     // Configura la serialización JSON aquítivar la conversión a camelCas
//     options.JsonSerializerOptions.PropertyNamingPolicy = null; // Desace
//     options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping; // Permite caracteres especiales
//     options.JsonSerializerOptions.WriteIndented = true; // Formatear JSON con indentación (opcional)
// });


builder.Services.AddTransient<IRepositorioIndicadores, RepositorioIndicadores>();
builder.Services.AddTransient<IRepositorioHidrocarburos, RepositorioHidrocarburos>();
builder.Services.AddTransient<IRepositorioProyectos, RepositorioProyectos>();
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddTransient<IRepositorioSecciones, RepositorioSecciones>();
builder.Services.AddTransient<IRepositorioAcceso, RepositorioAcceso>();
builder.Services.AddTransient<IRepositorioSNIER, RepositorioSNIER>();
builder.Services.AddTransient<VisitasViewComponent>();
builder.Services.AddTransient<IRepositorioHome, RepositorioHome>();
builder.Services.AddTransient<IRepositorioSankey, RepositorioSankey>();
builder.Services.AddTransient<IRepositorioSankeySener, RepositorioSankeySener>();
builder.Services.AddTransient<IRepositorioMIM, RepositorioMIM>();
builder.Services.AddTransient<IRepositorioAtlas, RepositorioAtlas>();
builder.Services.AddTransient<IRepositorioMap, RepositorioMap>();
builder.Services.AddTransient<IRepositorioLaboratoriosyUE, RepositorioLaboratoriosyUE>();
builder.Services.AddTransient<IRepositorioProyEstrategicos, RepositorioProyEstrategicos>();
builder.Services.AddTransient<IRepositorioPermisosPV, RepositorioPermisosPV>();
builder.Services.AddTransient<IRepositorioFacturas, RepositorioFacturas>();
builder.Services.AddTransient<IRepositorioEnergiasLimpias, RepositorioEnergiasLimpias>();
builder.Services.AddTransient<IRepositorioTarifas, RepositorioTarifas>();
builder.Services.AddTransient<FacturaExtractorService>();
builder.Services.AddTransient<IRepositorioInscripcion, RepositorioInscripcion>();
builder.Services.AddScoped<IRepositorioFinanzas, RepositorioFinanzas>();
builder.Services.AddScoped<IRepositorioFuentesdeInformacion, RepositorioFuentesdeInformacion>();
builder.Services.AddTransient<IRepositorioSIIL, RepositorioSIIL>();


builder.Services.AddTransient<IUserStore<UsuarioApp>, UsuarioStore>();
builder.Services.AddIdentityCore<UsuarioApp>();
builder.Services.AddScoped<AutorizacionFiltro>();
builder.Services.AddScoped<ValidacionInputFiltro>();

// builder.Services.AddTransient<IServicioEmail, ServicioEmailSendGrid>();
builder.Services.AddTransient<IServicioEmailSMTP, ServicioEmailSmtp>();

// Registrar el repositorio de bitácora
builder.Services.AddScoped<IRepositorioBitacora, RepositorioBitacora>();

// Esta línea es antes de otros servicios que dependan de IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configuración de límites de tamaño de archivo y tiempo de espera
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 268435456; // 256 MB, ajustar según necesidad
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

//Configurando la Sesión
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de inactividad antes de expirar la sesión
});

// IP
// Configura el middleware de reenvío de encabezados
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// IA
if (builder.Environment.IsDevelopment())
{
    // Configuración para ignorar errores de SSL en desarrollo
    builder.Services.AddHttpClient<IRepositorioChat, RepositorioChat>(client =>
    {
        client.BaseAddress = new Uri("https://api.openai.com/v1/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", builder.Configuration["OpenAI:ApiKey"]);
    }).ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });
}
else
{
    // Configuración para producción
    builder.Services.AddHttpClient<IRepositorioChat, RepositorioChat>(client =>
    {
        client.BaseAddress = new Uri("https://api.openai.com/v1/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", builder.Configuration["OpenAI:ApiKey"]);
    });
}


//Eventos
builder.Services.AddHttpClient();



// Configurar la localización
var supportedCultures = new[] { new CultureInfo("es-ES") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-ES");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Acceso/Login";
        options.LogoutPath = "/Acceso/Logout";
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.Scope.Add("profile");
        options.Scope.Add("email");
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        options.Scope.Add("email");
        options.Fields.Add("email");
        options.Fields.Add("name");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();
