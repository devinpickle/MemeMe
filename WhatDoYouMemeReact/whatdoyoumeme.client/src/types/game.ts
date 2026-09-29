import type { PlayerSummaryDto } from "./player";

export interface CreateGameResponseDto {
    gameId: number,
    joinCode: string,
    playerId: number,
    accessToken: string
}

export interface JoinGameResponseDto {
    playerId: number,
    playerName: string,
    gameId: number,
    joinCode: string,
    accessToken: string
}

export interface GameLobbySummaryDto {
    joinCode: string,
    status: GameStatus,
    players: PlayerSummaryDto[],
    imageCount: number
}

export interface GameRoundStartDto {
    joinCode: string,
    status: GameStatus,
    roundNumber: number,
    judgeId: number,
    imageId: number,
    state: RoundState,
    gameFinished: boolean
}

export interface SubmitCaptionResponseDto {
    submitted: number,
    needed: number
}

export interface RoundSummaryDto {
    roundNumber: number,
    state: RoundState,
    judgeId: number,
    imageId: number,
    imageFileName: string,
    submittedCaptionCount: number,
    captionsExpected: number,
    selectedCaptionId: number,
    selectedCaptionContent: string | null,
    selectedCaptionStyle: MemeStyle | null,
    winnerName: string | null,
    winnerId: number,
    winnerScore: number,
    gameFinished: boolean
}

export interface RoundFinishSummaryDto {
    state: RoundState,
    winnerId: number,
    winnerName: string,
    winnerScore: number,
    winningCaption: string,
    imageFileName: string,
    gameFinished: boolean
}

export interface GameResultsDto {
    winners: PlayerSummaryDto[],
    playerSummaryDtos: PlayerSummaryDto[],
}

export interface RoundCaptionsDto {
    roundId: number,
    captions: CaptionSummaryDto[],
}

export interface CaptionSummaryDto {
    id: number,
    content: string,
    memeStyle: MemeStyle
}

export interface SelectCaptionRequestDto {
    captionId: number
}

export type GameStatus =
    | "Lobby"
    | "InProgress"
    | "Finished"
    | "Cancelled";

export type RoundState =
    | "NotStarted"
    | "WritingCaptions"
    | "Judging"
    | "ShowingWinner"
    | "Complete";

export type MemeStyle =
| "Classic"
| "Impact"
| "BottomCaption";