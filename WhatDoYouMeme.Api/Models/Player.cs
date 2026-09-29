namespace WhatDoYouMeme.Api.Models;

public class Player
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public string Name { get; set; } = "";
    public int Score { get; set; } = 0;
    public bool IsHost { get; set; }
    public string? ConnectionId { get; set; }
    public ICollection<Caption> Captions { get; set; } = [];
    public string AccessToken { get; set; } = "";
}