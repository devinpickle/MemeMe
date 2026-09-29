namespace WhatDoYouMeme.Api.Models;

public class Caption
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public Round Round { get; set; } = null!;
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public string Content { get; set; } = "";
    public MemeStyle Style { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}



public enum MemeStyle
{
    Classic,
    Impact,
    BottomCaption
}