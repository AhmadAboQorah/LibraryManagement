using Library.Application.Interfaces;
using Library.Infrastructure.Auth;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // DbContext is Scoped (one per HTTP request) - AddDbContext does this for us
        services.AddDbContext<LibraryDbContext>(o =>
            o.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // The DbContext is also our Unit of Work: same instance within one request
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LibraryDbContext>());

        // Repositories: Scoped, because they depend on the Scoped DbContext
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();

        // Stateless helpers: Singleton (one instance for the whole app is enough and cheaper)
        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
