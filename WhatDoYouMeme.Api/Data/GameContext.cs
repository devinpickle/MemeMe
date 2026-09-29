using WhatDoYouMeme.Api.Models;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace WhatDoYouMeme.Api.Data;

public class GameContext(DbContextOptions<GameContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();

    public DbSet<Caption> Captions => Set<Caption>();

    public DbSet<Image> Images => Set<Image>();

    public DbSet<Player> Players => Set<Player>();

    public DbSet<Round> Rounds => Set<Round>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Game
        modelBuilder.Entity<Game>()
            .HasIndex(g => g.JoinCode)
            .IsUnique();

        // Caption
        modelBuilder.Entity<Caption>()
            .HasOne(c => c.Round)
            .WithMany(r => r.Captions)
            .HasForeignKey(c => c.RoundId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Caption>()
            .HasOne(c => c.Player)
            .WithMany(p => p.Captions)
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Caption>()
            .HasIndex(c => new { c.RoundId, c.PlayerId })
            .IsUnique();

        // Image
        modelBuilder.Entity<Image>()
            .HasOne(i => i.Game)
            .WithMany(g => g.Images)
            .HasForeignKey(i => i.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // Player
        modelBuilder.Entity<Player>()
            .HasOne(p => p.Game)
            .WithMany(g => g.Players)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // Round
        modelBuilder.Entity<Round>()
            .HasOne(r => r.Game)
            .WithMany(g => g.Rounds)
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Round>()
            .HasOne(r => r.Judge)
            .WithMany()
            .HasForeignKey(r => r.JudgeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Round>()
            .HasOne(r => r.Image)
            .WithMany(i => i.Rounds)
            .HasForeignKey(r => r.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Round>()
            .HasOne(r => r.Winner)
            .WithMany()
            .HasForeignKey(r => r.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Round>()
            .HasOne(r => r.WinningCaption)
            .WithMany()
            .HasForeignKey(r => r.WinningCaptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Round>()
            .HasOne(r => r.SelectedCaption)
            .WithMany()
            .HasForeignKey(r => r.SelectedCaptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public async Task<Game?> GetCurrentGameAsync(string joinCode, bool includePlayers = false)
    {
        if (includePlayers)
        {
            return await Games
                .Include(g => g.Players)
                .SingleOrDefaultAsync(g => g.JoinCode == joinCode);
        }

        return await Games
            .SingleOrDefaultAsync(g => g.JoinCode == joinCode);
    }

    public async Task<Round?> GetCurrentRoundAsync(int gameId)
    {
        var round = await Rounds
            .Include(r => r.Winner)
            .Include(r => r.Captions)
            .Include(r => r.Image)
            .SingleOrDefaultAsync(r =>
                r.GameId == gameId &&
                r.State != RoundState.NotStarted &&
                r.State != RoundState.Complete);

        Console.WriteLine("----- CURRENT ROUND DEBUG -----");

        if (round == null)
        {
            Console.WriteLine("NO ROUND FOUND");
        }
        else
        {
            Console.WriteLine($"Round Id: {round.Id}");
            Console.WriteLine($"Round Number: {round.RoundNumber}");
            Console.WriteLine($"State: {round.State}");
            Console.WriteLine($"ImageId: {round.ImageId}");
            Console.WriteLine($"JudgeId: {round.JudgeId}");
            Console.WriteLine($"WinnerId: {round.WinnerId}");
            Console.WriteLine($"Caption Count: {round.Captions.Count}");
        }

        Console.WriteLine("-------------------------------");

        return round;
    }
}