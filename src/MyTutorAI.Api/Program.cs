using MyTutorAI.Api.Auth;
using MyTutorAI.Api.Endpoints;
using MyTutorAI.Api.ErrorHandling;
using MyTutorAI.Api.OpenApi;
using MyTutorAI.Api.RateLimiting;
using MyTutorAI.Application;
using MyTutorAI.Application.Options;
using MyTutorAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<LlmOptions>()
    .BindConfiguration(LlmOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<FeaturesOptions>()
    .BindConfiguration(FeaturesOptions.SectionName);
builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddPromptRendering(useDiskTemplates: builder.Environment.IsDevelopment());
builder.Services.AddLlmClient();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddAuthServices(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApiRateLimiting();

builder.Services.AddJwtAuthentication();

builder.Services.AddOpenApi(o =>
{
    o.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    o.AddOperationTransformer<BearerSecuritySchemeTransformer>();
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));


var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "MyTutorAI API");
        o.EnablePersistAuthorization();
    });
}

app.MapAuthEndpoints();
app.MapSessionEndpoints();
app.MapDemoEndpoints(app.Configuration);
app.MapProfileEndpoints();
app.MapMemoryEndpoints();

app.Run();
