using MyTutorAI.Api.Endpoints;
using MyTutorAI.Api.ErrorHandling;
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
builder.Services.AddPersistence();
builder.Services.AddAuthServices();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApiRateLimiting();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "MyTutorAI API"));
}

app.MapAuthEndpoints();
app.MapSessionEndpoints();
app.MapDemoEndpoints(app.Configuration);

app.Run();
