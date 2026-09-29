namespace WhatDoYouMeme.Api.Models;

public class Image
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public ICollection<Round> Rounds { get; set; } = [];
    public string ContentType { get; set; } = "";
    public string Filename { get; set; } = "";
    public string OriginalFilename { get; set; } = "";
}