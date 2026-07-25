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

// Configuración local no versionada para credenciales de desarrollo y pruebas.
// Los proveedores estándar (variables de entorno, secretos, etc.) siguen disponibles.
builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["SQLCONNSTR_DefaultConnection"]
    ?? builder.Configuration["CUSTOMCONNSTR_DefaultConnection"];

if (!string.IsNullOrWhiteSpace(defaultConnection))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = defaultConnection;
}

var mimConnection = builder.Configuration.GetConnectionString("MIMConnection")
    ?? builder.Configuration["SQLCONNSTR_MIMConnection"]
    ?? builder.Configuration["CUSTOMCONNSTR_MIMConnection"];

if (!string.IsNullOrWhiteSpace(mimConnection))
{
    builder.Configuration["ConnectionStrings:MIMConnection"] = mimConnection;
}

Console.WriteLine($"DefaultConnection configurada: {!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection"))}");

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
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new NSIE.Filters.InvariantDecimalModelBinderProvider());
});
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
builder.Services.AddTransient<IRepositorioPODECOBIS, RepositorioPODECOBIS>();
builder.Services.AddTransient<IRepositorioPODECOBIPolos, RepositorioPODECOBIPolos>();
builder.Services.AddTransient<IRepositorioInformePormenorizado, RepositorioInformePormenorizado>();
builder.Services.AddTransient<IPamrntProyectosIdentificadosService, PamrntProyectosIdentificadosService>();
builder.Services.AddTransient<IPamActualizacionService, PamActualizacionService>();
builder.Services.AddScoped<IPamFuenteExtractionService, PamFuenteExtractionService>();
builder.Services.AddScoped<IPamAnalisisService, PamAnalisisService>();
builder.Services.AddHostedService<PamAnalisisWorker>();
builder.Services.AddTransient<IRepositorioInversionDesarrolloEnergetico, RepositorioInversionDesarrolloEnergetico>();
builder.Services.AddTransient<PvirseImportService>();
builder.Services.AddTransient<IngestionService>();
builder.Services.AddTransient<IRepositorioReuniones, RepositorioReuniones>();
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
builder.Services.AddScoped<IRepositorioPermisosEnergeticos, RepositorioPermisosEnergeticos>();
builder.Services.AddScoped<IServicioPermisosEnergeticos, ServicioPermisosEnergeticos>();
builder.Services.AddTransient<FacturaExtractorService>();
builder.Services.AddTransient<IRepositorioInscripcion, RepositorioInscripcion>();
builder.Services.AddScoped<IRepositorioFinanzas, RepositorioFinanzas>();
builder.Services.AddScoped<IRepositorioFuentesdeInformacion, RepositorioFuentesdeInformacion>();
builder.Services.AddTransient<IRepositorioSIIL, RepositorioSIIL>();
builder.Services.AddTransient<ManualSharePointImportService>();
builder.Services.AddTransient<InformePormenorizadoImportService>();
builder.Services.AddTransient<IRepositorioGestor, RepositorioGestor>();
builder.Services.AddTransient<IRepositorioProyectosPrivados, RepositorioProyectosPrivados>();
builder.Services.AddTransient<IRepositorioGruposInteres, RepositorioGruposInteres>();
builder.Services.AddMemoryCache();
builder.Services.Configure<InegiOptions>(builder.Configuration.GetSection(InegiOptions.SectionName));
builder.Services.AddHttpClient<IInegiTerritorialService, InegiTerritorialService>(client =>
{
    client.BaseAddress = new Uri("https://www.inegi.org.mx/");
    client.Timeout = TimeSpan.FromSeconds(25);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("DGMESNIE-Territorial/1.0");
});



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
    options.IdleTimeout = TimeSpan.FromMinutes(120); // Tiempo de inactividad antes de expirar la sesión
});

// IP
// Configura el middleware de reenvío de encabezados
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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

app.UseForwardedHeaders();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
    });
}
else
{
    app.UseStaticFiles();
}
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();
