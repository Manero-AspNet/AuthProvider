using AuthProvider.Data.Contexts;
using AuthProvider.Data.Entities;
using AuthProvider.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    })
    .Build();

host.Run();
