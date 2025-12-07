using Anamel.Api.Extensions;
using Anamel.Api.Middleware;
using Anamel.BL.Services;
using Anamel.Core.Interfaces.IServices;
using Anamel.Core.IRepositories;
using Anamel.Core.IRepositories.Services;
using Anamel.DL;
using Anamel.DL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Anamel
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            try
            {
                // Add services to the container.
                builder.Services.AddControllers();

                // Configure database
                builder.Services.ConfigureDatabase(builder.Configuration);
                // Configure Identity
                builder.Services.ConfigureIdentity();

                // Configure JWT
                builder.Services.ConfigureJWT(builder.Configuration);

                // Register Repositories
                builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
                builder.Services.AddScoped<ICategoryService, CategoryService>();
                builder.Services.AddScoped<IAuthService, AuthService>();
                builder.Services.AddScoped<IEmailService, EmailService>();
                // Configure CORS
                builder.Services.ConfigureCors();

                // Configure business services
                builder.Services.ConfigureServices();

                // Authorization policies
                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("RequireAdminRole", policy =>
                        policy.RequireRole("Admin"));
                    options.AddPolicy("RequireCustomerRole", policy =>
                        policy.RequireRole("Customer", "Admin"));
                    options.AddPolicy("RequireManagerRole", policy =>
                        policy.RequireRole("Manager", "Admin"));
                });

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "E-Commerce API",
                        Version = "v1",
                        Description = "A comprehensive e-commerce API built with layered architecture",
                        Contact = new OpenApiContact
                        {
                            Name = "Development Team",
                            Email = "hanaanabilzedan@gmail.com"
                        }
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                builder.Services.AddLogging();

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    try
                    {
                        var context = services.GetRequiredService<ApplicationDbContext>();

                        // Check if we can connect to the database
                        logger.LogInformation("Testing database connection...");
                        if (await context.Database.CanConnectAsync())
                        {
                            logger.LogInformation("Database connection successful.");

                            // Only run migrations if configured to do so
                            if (builder.Configuration.GetValue<bool>("RunMigrationsOnStartup", false))
                            {
                                logger.LogInformation("Applying database migrations...");
                                await context.Database.MigrateAsync();
                                logger.LogInformation("Database migrations applied successfully.");
                            }

                            // Seed roles and admin user
                            logger.LogInformation("Seeding roles and admin user...");
                            await ServiceExtensions.SeedRolesAndAdmin(services);
                            logger.LogInformation("Roles and admin seeded successfully.");
                        }
                        else
                        {
                            logger.LogWarning("Could not connect to database. Skipping migrations and seeding.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred during database initialization. The app will start anyway.");
                    }
                }
                app.UseDeveloperExceptionPage();
                //app.UseMiddleware<GlobalExceptionMiddleware>();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API V1");
                    c.RoutePrefix = string.Empty;
                });

                // Comment out HTTPS redirection for now
                // app.UseHttpsRedirection();

                app.UseMiddleware<GlobalExceptionMiddleware>();

                app.UseCors("AllowAll");

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
                app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error during startup: {ex}");
                throw;
            }
        }
    }
}