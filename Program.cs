#region Usings
using Data;
using Filters;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;
#endregion

#region Services
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    options.Filters.Add<ExceptionHandlerFilterAttribute>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<SessionRepository>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(Configuration.Configurations.ConnectionStrings.Postgres));
#endregion

#region Middlewares
WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    DbContext context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
#endregion

app.Run();
