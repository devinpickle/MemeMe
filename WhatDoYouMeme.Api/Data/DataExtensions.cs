using WhatDoYouMeme.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace WhatDoYouMeme.Api.Data;

public static class DataExtensions
{
    public static void MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider
                            .GetRequiredService<GameContext>();

        dbContext.Database.Migrate();
    }

    public static void AddWhatDoYouMemeDb(this WebApplicationBuilder builder)
    {
        var connString = builder.Configuration.GetConnectionString("MemeGame");
        builder.Services.AddSqlite<GameContext>(
            connString,
            optionsAction: options => options.UseSeeding((context, _) =>
            {
                
            })    
        );
    }
}