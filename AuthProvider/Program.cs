using AuthProvider.Data.Contexts;
using AuthProvider.Data.Entities;
using AuthProvider.Services;
using Azure.Messaging.ServiceBus;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<DataContext>(x => x.UseSqlServer(Environment.GetEnvironmentVariable("SqlServer")));
        services.AddSingleton<ServiceBusClient>(new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBus")));
        services.AddScoped<ISignUpService, SignUpService>();
        services.AddScoped<ISignInService, SignInService>();

        services.AddIdentity<UserEntity, IdentityRole>(x => {
            x.User.RequireUniqueEmail = true;
            x.SignIn.RequireConfirmedEmail = false;
            x.Password.RequiredLength = 8;
        }).AddEntityFrameworkStores<DataContext>()
          .AddDefaultTokenProviders();

        services.AddAuthentication().AddFacebook(facebookOptions =>
        {
            facebookOptions.AppId = configuration["FaceBookAppId"]!;
            facebookOptions.AppSecret = configuration["FaceBookAppSecret"]!;
        });
    })
    .Build();



host.Run(); 
