using Library.Application.Interfaces;
using Library.Application.Services;

namespace Library.API.Extensions;

public static class ServiceRegistration
{
    // Services hold business logic and use Scoped repositories -> they must be Scoped too
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ILoanService, LoanService>();
        return services;
    }
}
