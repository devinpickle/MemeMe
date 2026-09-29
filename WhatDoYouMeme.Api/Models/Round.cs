namespace WhatDoYouMeme.Api.Models;

public class Round
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public int JudgeId { get; set; }
    public Player Judge { get; set; } = null!;
    public int ImageId { get; set; }
    public Image Image { get; set; } = null!;
    public RoundState State { get; set; } = RoundState.NotStarted;
    public int RoundNumber { get; set; }
    public ICollection<Caption> Captions { get; set; } = [];
    public int? WinnerId { get; set; }
    public Player? Winner { get; set; }
    public int? WinningCaptionId { get; set; }
    public Caption? WinningCaption { get; set; }
    public int? SelectedCaptionId { get; set; }
    public Caption? SelectedCaption { get; set; }
}

public enum RoundState
{
    NotStarted,
    WritingCaptions,
    Judging,
    ShowingWinner,
    Complete
}