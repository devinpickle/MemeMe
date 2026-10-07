import type { CreateGameResponseDto, GameLobbySummaryDto, GameResultsDto, GameRoundStartDto, JoinGameResponseDto, MemeStyle, RoundCaptionsDto, RoundFinishSummaryDto, RoundSummaryDto, SubmitCaptionResponseDto } from "../types/game";
import { apiFetch } from "./client";

export async function createGame(hostName: string, numberOfRounds: number): Promise<CreateGameResponseDto> {
    const response = await apiFetch("/games", {
        method: "POST",
        body: JSON.stringify({
            hostName,
            numberOfRounds
        }),
    });

    return response.json();
}

export async function joinGame(joinCode: string, name: string): Promise<JoinGameResponseDto> {
    const response = await apiFetch(`/games/${joinCode}/join`, {
        method: "POST",
        body: JSON.stringify({
            name
        }),
    });

    return response.json();
}

export async function getLobby(joinCode: string): Promise<GameLobbySummaryDto> {
    const response = await apiFetch(`/games/${joinCode}`, {
        method: "GET",
    });

    return response.json();
}

async function shrinkImage(file: File, maxSize = 1280, quality = 0.8): Promise<File> {
    // Already small, no need to re-encode
    if (file.size < 300 * 1024) {
        return file;
    }

    try {
        const bitmap = await createImageBitmap(file, { imageOrientation: "from-image" });

        const scale = Math.min(1, maxSize / Math.max(bitmap.width, bitmap.height));
        const width = Math.round(bitmap.width * scale);
        const height = Math.round(bitmap.height * scale);

        const canvas = document.createElement("canvas");
        canvas.width = width;
        canvas.height = height;

        const context = canvas.getContext("2d");
        if (!context) {
            bitmap.close();
            return file;
        }

        context.drawImage(bitmap, 0, 0, width, height);
        bitmap.close();

        const blob = await new Promise<Blob | null>(resolve =>
            canvas.toBlob(resolve, "image/jpeg", quality)
        );

        if (!blob) {
            return file;
        }

        return new File([blob], file.name.replace(/\.\w+$/, ".jpg"), { type: "image/jpeg" });
    } catch {
        // If the browser can't decode it, fall back to the original file
        return file;
    }
}

export async function uploadImages(joinCode: string, files: File[], accessToken: string): Promise<void> {
    const formData = new FormData();

    for (const file of files) {
        formData.append("files", await shrinkImage(file));
    }

    await apiFetch(`/games/${joinCode}/images`, {
        method: "POST",
        body: formData,
        accessToken: accessToken,
    });
}

export async function startGame(joinCode: string): Promise<GameRoundStartDto> {
    const response = await apiFetch(`/games/${joinCode}/start`, {
        method: "POST",
    });

    return response.json();
}

export async function submitCaption(joinCode: string, playerId: number, content: string, memeStyle: MemeStyle): Promise<SubmitCaptionResponseDto> {
    const response = await apiFetch(`/games/${joinCode}/captions`, {
        method: "POST",
        body: JSON.stringify({
            playerId,
            content,
            style: memeStyle
        }),
    });

    return response.json();
}

export async function getCurrentRound(joinCode: string): Promise<RoundSummaryDto> {
    const response = await apiFetch(`/games/${joinCode}/round`, {
        method: "GET",
    });

    return response.json();
}

export async function judgeCaption(joinCode: string, judgeId: number, captionId: number): Promise<RoundFinishSummaryDto> {
    const response = await apiFetch(`/games/${joinCode}/judge`, {
        method: "POST",
        body: JSON.stringify({
            judgeId,
            captionId
        }),
    });

    return response.json();
}

export async function getCurrentRoundCaptions(joinCode: string): Promise<RoundCaptionsDto> {
    const response = await apiFetch(`/games/${joinCode}/captions`, {
        method: "GET",
    });

    console.log(response);

    return response.json();
}

export async function updateSelectedCaption(joinCode: string, captionId: number): Promise<void> {
    await apiFetch(`/games/${joinCode}/selected-caption`, {
        method: "PUT",
        body: JSON.stringify({
            captionId
        }),
    });

}

export async function nextRound(joinCode: string): Promise<GameRoundStartDto> {
    console.log("Calling nextRound:", joinCode);

    const response = await apiFetch(`/games/${joinCode}/next-round`, {
        method: "POST",
    });

    console.log("nextRound response:", response.status, response.statusText);

    const data = await response.json();

    console.log("nextRound response body:", data);

    return data;
}

export async function getResults(joinCode: string): Promise<GameResultsDto> {
    const response = await apiFetch(`/games/${joinCode}/results`, {
        method: "GET",
    });

    return response.json();
}

export async function cleanupGame(joinCode: string) {
    const accessToken = sessionStorage.getItem("accessToken");

    if (!accessToken) {
        throw new Error("No player access token found.");
    }

    await apiFetch(`/games/${joinCode}/cleanup`, {
        method: "POST",
        accessToken,
    });
}