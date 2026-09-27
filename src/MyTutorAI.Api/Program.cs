using MyTutorAI.Api.Endpoints;
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

builder.Services.AddApplication();
builder.Services.AddPromptRendering(useDiskTemplates: builder.Environment.IsDevelopment());
builder.Services.AddLlmClient();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "MyTutorAI API"));
}

app.MapDemoEndpoints(app.Configuration);

app.Run();
