using Microsoft.Extensions.DependencyInjection;

namespace DocFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
