using DocFlow.Application;
using DocFlow.Infrastructure;
using DocFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddApplication();

// Register infrastructure services and PostgreSQL DbContext
builder.Services.AddInfrastructure(builder.Configuration);

// Register controllers
builder.Services.AddControllers();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map controllers
app.MapControllers();

// Database health endpoint
app.MapGet("/health", async (
    DocFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        if (!canConnect)
        {
            return Results.Json(
                new
                {
                    status = "Unhealthy",
                    database = "Unavailable"
                },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Results.Ok(new
        {
            status = "Healthy",
            database = "Connected"
        });
    }
    catch
    {
        return Results.Json(
            new
            {
                status = "Unhealthy",
                database = "Unavailable"
            },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
