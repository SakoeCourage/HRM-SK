using Carter;
using FluentValidation;
using Hangfire;
using HRM_SK.Database;
using HRM_SK.Extensions;
using HRM_SK.Providers;
using HRM_SK.Serivices.ImageKit;
using HRM_SK.Serivices.Mail_Service;
using HRM_SK.Serivices.Notification_Service;
using HRM_SK.Services.SMS_Service;
using HRM_SK.Shared;
using HRM_SK.Utilities;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
var AppKey = builder.Configuration.GetValue<string>("SiteSettings:AppKey");

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<DatabaseContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging();
    ;

});

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));



builder.Services.AddScoped<ImageKit>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    return new ImageKit(configuration);
});

builder.Services.AddTransient<SMSService>();
builder.Services.AddTransient<MailService>();
builder.Services.AddAntiforgery();

builder.Services.AddScoped<Authprovider>(services =>
{
    var scope = services.GetRequiredService<IServiceScopeFactory>();
    return new Authprovider(scope);
});

builder.Services.AddScoped(typeof(Notify<>));

JWTStartupConfig.ConfigureJWt(builder.Services, builder.Configuration);
builder.Services.AddSingleton<JWTProvider>(new JWTProvider(AppKey));
Paginator.SetHttpContextAccessor(builder.Services.BuildServiceProvider().GetService<IHttpContextAccessor>());

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGenNewtonsoftSupport();

builder.Services.AddSwaggerGen(option => SwaggerDoc.OpenAuthentication(option));

builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(assembly));

builder.Services.AddCarter();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddEndpointsApiExplorer();

var MyAllowSpecificOrigins = "_allowedOrgins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5181", "http://localhost:5180")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                      });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(MyAllowSpecificOrigins);
app.UseRateLimiter();


app.UseHttpsRedirection();
app.MapCarter();

app.Use(async (context, next) =>
{
    var exemptPath = "/api/auth/user/login";

    if (context.Request.Path.Value.Equals(exemptPath, StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    if (!context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        var response = new Error("Unauthorized", StatusCodes.Status401Unauthorized.ToString());
        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(jsonResponse);

        return;
    }

    await next();
});
app.UseHangfireDashboard("/hangfire");

app.Run();


