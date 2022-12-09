using Calabonga.AspNetCore.AppDefinitions;
using Microsoft.EntityFrameworkCore;
using Scribble.Identity.Infrastructure.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefinitions(builder, typeof(Program));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = app.Services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

app.UseDefinitions();

app.Run();
