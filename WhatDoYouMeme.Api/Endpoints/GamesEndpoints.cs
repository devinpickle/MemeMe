using WhatDoYouMeme.Api.Data;
using WhatDoYouMeme.Api.Dtos;
using WhatDoYouMeme.Api.Models;
using Microsoft.EntityFrameworkCore;
using WhatDoYouMeme.Api.Services;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WhatDoYouMeme.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Image = WhatDoYouMeme.Api.Models.Image;
using SkiaSharp;

namespace WhatDoYouMeme.Api.Endpoints;

public static class GamesEndpoints
{
    public const string GetGameEndpointName = "GetGame";
    public const string UploadRoot = "./Assets/Uploads";
    public const int CaptionLimit = 100;

    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games")
            .RequireRateLimiting("api");

        // Create a game
        // POST /games
        group.MapPost("/", async (CreateGameDto request, GameContext dbContext) => 
        {
            // validate host name
            if (string.IsNullOrWhiteSpace(request.HostName))
            {
                return Results.BadRequest("Host name is required.");
            }

            // validate number of rounds
            if (request.NumberOfRounds < 1)
            {
                return Results.BadRequest("Number of Rounds must be 1 or more.");
            }
            if (request.NumberOfRounds > 99)
            {
                return Results.BadRequest("Number of Rounds cannot be more than 99.");
            }

            string joinCode = "";
            do
            {
                joinCode = JoinCodeGenerator.Generate();
            }
            while (await dbContext.Games.AnyAsync(g => g.JoinCode == joinCode));

            Game game = new()
            {
                JoinCode = joinCode,
                Status = GameStatus.Lobby,
                CreatedAt = DateTime.UtcNow,
                NumberOfRounds = request.NumberOfRounds
            };

            // Create and add host
            game.Players.Add(new Player
            {
                Name = request.HostName,
                IsHost = true,
                AccessToken = Guid.NewGuid().ToString("N")
            });

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            CreateGameResponseDto responseDto = new(
                game.Id,
                game.JoinCode,
                game.Players.First().Id,
                game.Players.First().AccessToken
            );

            return Results.Created($"/games/{game.JoinCode}", responseDto);
        });

        // Join a Game
        // POST /games/{joinCode}/join
        group.MapPost("/{joinCode}/join", async (string joinCode, JoinGameDto request, GameContext dbContext, IHubContext<GameHub> hub) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode, true);

            if (game.Status != GameStatus.Lobby)
            {
                return Results.BadRequest("The game has already started.");
            }

            if (game.Players.Any(p => p.Name == request.Name))
            {
                return Results.BadRequest("That name is already taken.");
            }

            var player = new Player
            {
                Name = request.Name,
                GameId = game.Id,
                Score = 0,
                IsHost = false,
                AccessToken = Guid.NewGuid().ToString("N")
            };

            dbContext.Players.Add(player);
            await dbContext.SaveChangesAsync();

            await hub.Clients
                .Group(joinCode)
                .SendAsync("LobbyUpdated", player.Name);

            return Results.Ok(new JoinGameResponseDto
            (
                player.Id,
                player.Name,
                game.Id,
                game.JoinCode,
                player.AccessToken
            ));
        });

        // Get Lobby State
        // GET /games/{joinCode}
        group.MapGet("/{joinCode}", async (string joinCode, GameContext dbContext) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode, true);

            var imageCount = await dbContext.Images
                .CountAsync(i => i.GameId == game.Id);

            return Results.Ok(new GameLobbySummaryDto(
                game.JoinCode,
                game.Status,
                game.Players
                    .Select(p => new PlayerSummaryDto(
                        p.Id,
                        p.Name,
                        p.Score,
                        p.IsHost
                    )).ToArray(),
                imageCount
            ));
        });


        // Upload Images
        // POST /games/{joinCode}/images
        group.MapPost("/{joinCode}/images", async (
            string joinCode,
            HttpRequest request,
            IFormFileCollection files,
            GameContext dbContext,
            IHubContext<GameHub> hub) =>
        {
            if (!request.Headers.TryGetValue("X-Player-Token", out var accessToken))
            {
                return Results.StatusCode(403);
            }

            var game = await GetGameOrThrowAsync(dbContext, joinCode, true);

            var player = game.Players
                .FirstOrDefault(p => p.AccessToken == accessToken.ToString());

            if (player == null)
            {
                return Results.StatusCode(403);
            }

            if (game.Status != GameStatus.Lobby)
            {
                return Results.BadRequest("Cannot upload images after the game has started.");
            }

            if (files.Count == 0)
            {
                return Results.BadRequest("No files were uploaded.");
            }

            var uploadPath = Path.Combine(UploadRoot, game.JoinCode);

            Directory.CreateDirectory(uploadPath);

            const long maxFileSize = 5 * 1024 * 1024; // 5 MB
            const long maxTotalUploadSize = 25 * 1024 * 1024; // 25 MB per request

            var totalUploadSize = files.Sum(file => file.Length);

            if (totalUploadSize > maxTotalUploadSize)
            {
                return Results.BadRequest(
                    "The total upload size cannot exceed 25 MB.");
            }

            foreach (var file in files)
            {

                if (file.Length > maxFileSize)
                {
                    return Results.BadRequest(
                        $"{file.FileName} of size {file.Length} exceeds the 5MB limit.");
                }

                var extension = await GetImageExtensionAsync(file);

                if (extension == null)
                {
                    return Results.BadRequest(
                        $"{file.FileName} does not contain a valid image.");
                }

                var savedFilename = $"{Guid.NewGuid()}{extension}";

                var fullPath = Path.Combine(uploadPath, savedFilename);

                await using var stream = File.Create(fullPath);
                await file.CopyToAsync(stream);

                dbContext.Images.Add(new Image
                {
                    GameId = game.Id,
                    Filename = savedFilename,
                    OriginalFilename = file.FileName,
                    ContentType = extension switch
                    {
                        ".jpg" => "image/jpeg",
                        ".png" => "image/png",
                        ".webp" => "image/webp",
                        _ => "application/octet-stream"
                    }
                });
            }

            await dbContext.SaveChangesAsync();

            await hub.Clients.Group(joinCode)
                .SendAsync("LobbyUpdated");

            return Results.Ok();
        })
        .DisableAntiforgery()
        .RequireRateLimiting("image-upload");

        // Start the Game
        // POST /games/{joinCode}/start
        group.MapPost("/{joinCode}/start", async (string joinCode, GameContext dbContext, IHubContext<GameHub> hub) =>
        {
            // TODO: ensure that the game is started by the host

            var game = await dbContext.Games
                .Include(g => g.Players)
                .Include(g => g.Images)
                .SingleOrDefaultAsync(g => g.JoinCode == joinCode);

            if (game is null)
            {
                return Results.NotFound();
            }

            if (game.Status != GameStatus.Lobby)
            {
                return Results.BadRequest("Game Must be in the Lobby to start.");
            }

            if (game.Players.Count < 3)
            {
                return Results.BadRequest("You must have 3 or more players to start the game.");
            }

            if (game.Images.Count < 1)
            {
                return Results.BadRequest("You must have at least 1 image to start game.");
            }

            var shuffledPlayers = game.Players
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            var shuffledImages = game.Images
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            Round? firstRound = null;

            int numberOfRounds = game.NumberOfRounds;
            // Do 5 rounds or less - TODO: change this later to enter number of rounds by host
            for (int i = 0; i < numberOfRounds; i++)
            {
                var round = new Round
                {
                    Game = game,
                    Judge = shuffledPlayers[i % shuffledPlayers.Count],
                    Image = shuffledImages[i],
                    RoundNumber = i + 1
                };

                if (i == 0)
                {
                    round.State = RoundState.WritingCaptions;
                    firstRound = round;
                }

                dbContext.Rounds.Add(round);
            }

            game.Status = GameStatus.InProgress;
            await dbContext.SaveChangesAsync();

            await hub.Clients.Group(joinCode)
                .SendAsync("GameStarted");

            return Results.Ok(new GameRoundStartDto(
                game.JoinCode,
                game.Status,
                firstRound!.RoundNumber,
                firstRound.JudgeId,
                firstRound.ImageId,
                firstRound.State,
                game.Status == GameStatus.Finished
            ));
        });

        // Submit Captions
        // POST /games/{joinCode}/captions
        group.MapPost("/{joinCode}/captions", async (string joinCode, SubmitCaptionDto captionDto, GameContext dbContext, IHubContext<GameHub> hub) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode);

            if (game.Status != GameStatus.InProgress)
            {
                return Results.BadRequest("The Game has not started.");
            }

            // Find player
            var player = await dbContext.Players
                .SingleOrDefaultAsync(p =>
                    p.Id == captionDto.PlayerId &&
                    p.GameId == game.Id);

            if (player is null)
            {
                return Results.BadRequest("Player not found.");
            }

            var currentRound = await dbContext.Rounds
                .SingleOrDefaultAsync(r =>
                    r.GameId == game.Id &&
                    r.State == RoundState.WritingCaptions);

            if (currentRound == null)
            {
                return Results.BadRequest("Current Round not found.");
            }

            if (currentRound.JudgeId == player.Id)
            {
                return Results.BadRequest("You are the Judge, you cannot submit captions for this round.");
            }

            // has the player already submitted a caption for this round?
            var alreadySubmitted = await dbContext.Captions
                .AnyAsync(c =>
                    c.RoundId == currentRound.Id &&
                    c.PlayerId == player.Id);

            if (alreadySubmitted)
            {
                return Results.BadRequest("You have already submitted a caption.");
            }
            
            string? captionNotValid = invalidCaption(captionDto.Content);
            if (captionNotValid != null)
            {
                return Results.BadRequest(captionNotValid);
            }

            dbContext.Captions.Add(new Caption
            {
                RoundId = currentRound.Id,
                PlayerId = player.Id,
                Content = captionDto.Content,
                Style = captionDto.Style
            });

            await dbContext.SaveChangesAsync();

            var captionCount = await dbContext.Captions
                .CountAsync(c => c.RoundId == currentRound.Id);

            var playerCount = await dbContext.Players
                .CountAsync(p => p.GameId == game.Id);

            // all players have submitted a caption
            if (captionCount == playerCount - 1)
            {
                currentRound.State = RoundState.Judging;
                await dbContext.SaveChangesAsync();
            }

            await hub.Clients
                .Group(joinCode)
                .SendAsync("RoundUpdated", player.Name);

            return Results.Ok(new SubmitCaptionResponseDto(captionCount, playerCount));
        });

        // Get the current round state
        // GET /games/{joinCode}/round
        group.MapGet("/{joinCode}/round", async (string joinCode, GameContext dbContext) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode);

            var currentRound = await GetCurrentRound(dbContext, game.Id);

            var currentImage = await dbContext.Images
                .Where(i => i.GameId == game.Id && i.Id == currentRound.ImageId)
                .FirstOrDefaultAsync();

            if (currentImage == null)
            {
                return Results.NotFound("Image Not Found");
            }

            var submittedCaptionCount = await dbContext.Captions
                .CountAsync(c => c.RoundId == currentRound.Id);

            var playerCount = await dbContext.Players
                .CountAsync(p => p.GameId == game.Id);

            var captionsExpected = playerCount - 1; // # of players who still need to submit a caption. don't include the judge (-1)

            return Results.Ok(new RoundSummaryDto(
                currentRound.RoundNumber,
                currentRound.State,
                currentRound.JudgeId,
                currentRound.ImageId,
                currentImage.Filename,
                submittedCaptionCount,
                captionsExpected,
                currentRound.SelectedCaptionId,
                currentRound.SelectedCaption?.Content,
                currentRound.SelectedCaption?.Style,
                currentRound.Winner?.Name,
                currentRound.WinnerId,
                currentRound.Winner?.Score,
                game.Status == GameStatus.Finished
            ));

        });

        // Get Captions for a Round
        // GET /games/{joinCode}/captions
        group.MapGet("/{joinCode}/captions", async (string joinCode, GameContext dbContext) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode);

            var currentRound = await GetCurrentRound(dbContext, game.Id);

            if (currentRound == null)
            {
                return Results.NotFound("Round Not Found.");
            }

            if (currentRound.State != RoundState.Judging)
            {
                return Results.BadRequest("Round not in Judging state");
            }

            var captions = await dbContext.Captions
                .Where(c => c.RoundId == currentRound.Id)
                .Select(c => new CaptionSummaryDto(
                    c.Id,
                    c.Content,
                    c.Style
                ))
                .ToListAsync();

            return Results.Ok(new RoundCaptionsDto(
                currentRound.Id,
                captions,
                currentRound.SelectedCaptionId
            ));
        });

        // Update the Round's Selected Caption
        // PUT /games/{joinCode}/selected-caption
        group.MapPut("/{joinCode}/selected-caption", async (string joinCode,
            SelectCaptionRequestDto selectCaptionRequestDto,
            GameContext dbContext,
            IHubContext<GameHub> hub) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode);

            if (game.Status != GameStatus.InProgress)
            {
                return Results.BadRequest("Game is no longer In Progress.");
            }

            var currentRound = await GetCurrentRound(dbContext, game.Id);

            if (currentRound.State != RoundState.Judging)
            {
                return Results.BadRequest("Round is not in the Judging state.");
            }

            var caption = await dbContext.Captions
                .SingleOrDefaultAsync(c => c.Id == selectCaptionRequestDto.CaptionId &&
                    c.RoundId == currentRound.Id);

            if (caption == null)
            {
                return Results.NotFound("Caption Not Found");
            }

            currentRound.SelectedCaptionId = caption.Id;

            await dbContext.SaveChangesAsync();

            await hub.Clients
                .Group(joinCode)
                .SendAsync("RoundUpdated");

            return Results.Ok();
        });

        // Judge the Round
        // POST /games/{joinCode}/judge
        group.MapPost("/{joinCode}/judge", async (string joinCode,
            JudgeCaptionDto judgeCaptionDto,
            GameContext dbContext,
            IHubContext<GameHub> hub) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode);

            if (game.Status != GameStatus.InProgress)
            {
                return Results.BadRequest("Game is no longer In Progress.");
            }

            var currentRound = await GetCurrentRound(dbContext, game.Id);

            if (currentRound.State != RoundState.Judging)
            {
                return Results.BadRequest("Round is not in the Judging State");
            }

            if (currentRound.JudgeId != judgeCaptionDto.JudgeId)
            {
                return Results.BadRequest("Requesting player is not the judge of this round");
            }

            // find caption
            var selectedCaption = await dbContext.Captions
                .SingleOrDefaultAsync(c =>
                    c.Id == judgeCaptionDto.CaptionId &&
                    c.RoundId == currentRound.Id);
            
            if (selectedCaption == null)
            {
                 return Results.NotFound("Caption not found.");
            }

            currentRound.WinnerId = selectedCaption.PlayerId;
            currentRound.WinningCaptionId = selectedCaption.Id;

            var winner = await dbContext.Players
                .FindAsync(selectedCaption.PlayerId);

            if (winner == null)
            {
                return Results.NotFound("Winner not found.");
            }

            winner.Score++;

            currentRound.State = RoundState.ShowingWinner;

            var totalRounds = await dbContext.Rounds
                .CountAsync(r => r.GameId == game.Id);

            var finalImageFilename = currentRound.Image.Filename;

            await dbContext.SaveChangesAsync();

            await hub.Clients
                .Group(joinCode)
                .SendAsync("RoundUpdated");

            return Results.Ok(new RoundFinishSummaryDto(
                currentRound.State,
                winner.Id,
                winner.Name,
                winner.Score,
                selectedCaption.Content,
                finalImageFilename,
                game.Status == GameStatus.Finished
            ));
        });

        // Go to the next round
        // POST /games/{joinCode}/next-round
        group.MapPost("/{joinCode}/next-round", async (string joinCode, GameContext dbContext, IHubContext<GameHub> hub) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode);

            if (game.Status != GameStatus.InProgress)
            {
                return Results.BadRequest("Game is no longer In Progress.");
            }

            var currentRound = await GetCurrentRound(dbContext, game.Id);

            Console.WriteLine(currentRound.State);
            Console.WriteLine(game.Status);
            if (currentRound.State != RoundState.ShowingWinner)
            {
                return Results.BadRequest("Current round not in Showing Winner state");
            }

            currentRound.State = RoundState.Complete;

            // find next round
            var nextRound = await dbContext.Rounds
                .SingleOrDefaultAsync(r => r.GameId == game.Id &&
                r.RoundNumber == currentRound.RoundNumber + 1);

            if (nextRound == null)
            {
                // This was the final round.
                game.Status = GameStatus.Finished;

                await dbContext.SaveChangesAsync();

                await hub.Clients
                    .Group(joinCode)
                    .SendAsync("GameFinished");

                return Results.Ok(new GameRoundStartDto(
                    joinCode,
                    game.Status,
                    currentRound.RoundNumber,
                    currentRound.JudgeId,
                    currentRound.ImageId,
                    currentRound.State,
                    true
                ));
            }

            if (nextRound.State != RoundState.NotStarted)
            {
                return Results.BadRequest("Next round is not in correct state");
            }

            nextRound.State = RoundState.WritingCaptions;

            await dbContext.SaveChangesAsync();

            await hub.Clients
                .Group(joinCode)
                .SendAsync("RoundUpdated");

            Console.WriteLine("===== NEXT ROUND =====");
            Console.WriteLine($"Current Round: {currentRound.RoundNumber}");
            Console.WriteLine($"Game Status BEFORE: {game.Status}");

            return Results.Ok(new GameRoundStartDto(
                joinCode,
                game.Status,
                nextRound.RoundNumber,
                nextRound.JudgeId,
                nextRound.ImageId,
                nextRound.State,
                game.Status == GameStatus.Finished
            ));
        });

        // Get final results
        // GET /games/{joinCode}/results
        group.MapGet("/{joinCode}/results", async (string joinCode, GameContext dbContext) =>
        {
            var game = await GetGameOrThrowAsync(dbContext, joinCode);

            if (game.Status != GameStatus.Finished)
            {
                return Results.NotFound("Game is not finished.");
            }

            var players = await dbContext.Players
                .Where(p => p.GameId == game.Id)
                .OrderByDescending(p => p.Score)
                .Select(p => new PlayerSummaryDto(
                    p.Id,
                    p.Name,
                    p.Score,
                    p.IsHost
                ))
                .ToArrayAsync();

            if (players.Length == 0)
            {
                return Results.NotFound("No players found for this game.");
            }

            var topScore = players.Max(p => p.Score);

            var winners = players
                .Where(p => p.Score == topScore)
                .ToArray();

            return Results.Ok(new GameResultsDto(
                winners,
                players
            ));
        });

        // TODO: this should be in an image endpoint file
        // GET /images/{filename}
        app.MapGet("/images/{joinCode}/{filename}", async (
            string joinCode,
            string filename,
            GameContext dbContext,
            HttpRequest request) =>
        {
            if (!request.Headers.TryGetValue("X-Player-Token", out var accessToken))
            {
                return Results.StatusCode(403);
            }

            var game = await dbContext.Games
                .Include(g => g.Players)
                .SingleOrDefaultAsync(g => g.JoinCode == joinCode);

            if (game == null)
            {
                return Results.NotFound();
            }

            var player = game.Players
                .FirstOrDefault(p => p.AccessToken == accessToken.ToString());

            if (player == null)
            {
                return Results.StatusCode(403);
            }

            // Make sure this image belongs to this game.
            var image = await dbContext.Images
                .SingleOrDefaultAsync(i =>
                    i.GameId == game.Id &&
                    i.Filename == filename);

            if (image == null)
            {
                return Results.NotFound();
            }

            var uploadDirectory = Path.GetFullPath(
                Path.Combine("Assets", "Uploads", joinCode)
            );

            var path = Path.GetFullPath(
                Path.Combine(uploadDirectory, filename)
            );

            // Prevent path traversal.
            if (!path.StartsWith(
                    uploadDirectory + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Invalid filename.");
            }

            if (!File.Exists(path))
            {
                return Results.NotFound();
            }

            return Results.File(
                new FileStream(path, FileMode.Open, FileAccess.Read),
                image.ContentType
            );
        });

        group.MapPost("/{joinCode}/cleanup", async (
            string joinCode,
            GameContext dbContext,
            HttpRequest request) =>
        {
            if (!request.Headers.TryGetValue("X-Player-Token", out var accessToken))
            {
                return Results.StatusCode(403);
            }

            var game = await dbContext.Games
                .Include(g => g.Players)
                .SingleOrDefaultAsync(g => g.JoinCode == joinCode);

            if (game == null)
            {
                return Results.NotFound();
            }

            var player = game.Players
                .FirstOrDefault(p => p.AccessToken == accessToken.ToString());

            if (player == null)
            {
                return Results.StatusCode(403);
            }

            if (game.Status != GameStatus.Finished)
            {
                return Results.BadRequest("Game is not finished.");
            }

            await CleanupGameImagesAsync(dbContext, game);

            return Results.Ok();
        });

    }

    static async Task<string?> GetImageExtensionAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();

        try
        {
            using var data = SKData.Create(stream);
            using var codec = SKCodec.Create(data);

            if (codec == null)
            {
                return null;
            }

            var info = codec.Info;

            if (info.Width > 4096 || info.Height > 4096)
            {
                return null;
            }

            using var bitmap = SKBitmap.Decode(data);

            if (bitmap == null)
            {
                return null;
            }

            return codec.EncodedFormat switch
            {
                SKEncodedImageFormat.Jpeg => ".jpg",
                SKEncodedImageFormat.Png => ".png",
                SKEncodedImageFormat.Webp => ".webp",
                _ => null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Image validation failed for {file.FileName}: {ex}");
            return null;
        }
    }

    public static string? invalidCaption(string caption)
    {
        if (caption == "")
        {
            return "Your caption can't be empty.";
        }

        if (caption.Length > 100)
        {
            return "Your caption must be 100 characters or less.";
        }

        return null;
    }

    private static async Task<Game> GetGameOrThrowAsync(GameContext dbContext, string joinCode, bool includePlayers = false)
    {
        return await dbContext.GetCurrentGameAsync(joinCode, includePlayers)
            ?? throw new BadHttpRequestException("Game not found.");
    }

    private static async Task<Round> GetCurrentRound(GameContext dbContext, int gameId)
    {
        return await dbContext.GetCurrentRoundAsync(gameId)
            ?? throw new BadHttpRequestException("Round not found.");
    }

    private static async Task CleanupGameImagesAsync(
        GameContext dbContext,
        Game game)
    {
        // Remove the database records first.
        var images = await dbContext.Images
            .Where(i => i.GameId == game.Id)
            .ToListAsync();

        dbContext.Images.RemoveRange(images);
        await dbContext.SaveChangesAsync();

        // Then remove the files from disk.
        var uploadPath = Path.GetFullPath(
            Path.Combine("Assets", "Uploads", game.JoinCode)
        );

        if (Directory.Exists(uploadPath))
        {
            try
            {
                Directory.Delete(uploadPath, recursive: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to delete upload directory for game {game.JoinCode}: {ex.Message}");
            }
        }
    }
}