using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WhatDoYouMeme.Api.Models;

public class Game
{
    public int Id { get; set; }

    [StringLength(4)]
    public string JoinCode { get; set; } = "";

    public ICollection<Image> Images { get; set; } = [];

    public GameStatus Status { get; set; } = GameStatus.Lobby;

    public ICollection<Player> Players { get; set; } = [];

    public int NumberOfRounds { get; set; } = 1;

    public ICollection<Round> Rounds { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum GameStatus
{
    Lobby,
    InProgress,
    Finished,
    Cancelled
}