using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProductAPI.Application.Interfaces;
using ProductAPI.Application.Services;

namespace ProductAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}