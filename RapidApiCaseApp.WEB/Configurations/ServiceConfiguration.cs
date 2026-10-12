
using Microsoft.Extensions.Options;
using StayFinder.Web.Options;
using StayFinder.Web.Services;
using StayFinder.Web.Services.Interfaces;

namespace StayFinder.Web.Configurations;

public static class ServiceConfiguration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<BookingApiOptions>()
            .Bind(configuration.GetSection(
                BookingApiOptions.SectionName));

        services
            .AddOptions<RapidApiOptions>()
            .Bind(configuration.GetSection(
                RapidApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var useMock = configuration.GetValue<bool>(
            "BookingApi:UseMock");

        if (useMock)
        {
            services.AddScoped<
                IBookingService,
                MockBookingService>();
        }
        else
        {
            services.AddHttpClient<
                IBookingService,
                BookingService>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<
                            IOptions<RapidApiOptions>>()
                        .Value;

                    client.BaseAddress = new Uri(
                        options.BaseUrl);

                    client.DefaultRequestHeaders.Add(
                        "x-rapidapi-key",
                        options.Key);

                    client.DefaultRequestHeaders.Add(
                        "x-rapidapi-host",
                        options.Host);

                    client.Timeout =
                        TimeSpan.FromSeconds(30);
                });
        }

        return services;
    }
}
