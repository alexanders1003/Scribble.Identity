using Calabonga.AspNetCore.AppDefinitions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefinitions(builder, typeof(Program));

var app = builder.Build();

app.UseDefinitions();

app.Run();
