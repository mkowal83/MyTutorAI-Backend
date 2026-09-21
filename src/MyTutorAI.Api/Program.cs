using MyTutorAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPromptRendering(useDiskTemplates: builder.Environment.IsDevelopment());

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
