import { getCurrentRound, nextRound } from "../api/games";
import { useEffect, useRef, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { RoundSummaryDto } from "../types/game";
import { connection } from "../signalr/gameHub";
import * as signalR from "@microsoft/signalr";
import MemePreview from "./MemePreview";

const API_URL = import.meta.env.VITE_API_URL;

export default function Winner() {

    const navigate = useNavigate();
    
    const { joinCode } = useParams();

    const playerId = Number(sessionStorage.getItem("playerId"));

    const [isAdvancing, setIsAdvancing] = useState(false);

    const [currentRoundData, setRound] = useState<RoundSummaryDto | null>(null);
    const initialRoundNumber = useRef<number | null>(null);
    
    useEffect(() => {
        let cancelled = false;

        async function initialize() {
            if (!joinCode) {
                return;
            }

            const round = await getCurrentRound(joinCode);

            if (cancelled) {
                return;
            }

            if (round.gameFinished) {
                navigate(`/results/${joinCode}`);
                return;
            }

            setRound(round);

            if (connection.state === signalR.HubConnectionState.Disconnected) {
                await connection.start();
            }

            if (cancelled) {
                return;
            }

            await connection.invoke("JoinGame", joinCode);

            if (cancelled) {
                return;
            }

            if (initialRoundNumber.current === null) {
                initialRoundNumber.current = round.roundNumber;
            }

            connection.on("RoundUpdated", handleRoundUpdated);
            connection.on("GameFinished", handleGameFinished);
        }

        initialize();

        return () => {
            cancelled = true;

            connection.off("RoundUpdated", handleRoundUpdated);
            connection.off("GameFinished", handleGameFinished);

            if (joinCode) {
                connection.invoke("LeaveGame", joinCode).catch(() => {});
            }
        };
    }, [joinCode]);

    async function handleMoveToNextRound() {
        if (isAdvancing) {
            return;
        }

        setIsAdvancing(true);

        try {
            const result = await nextRound(joinCode!);

            if (result.gameFinished) {
                navigate(`/results/${joinCode}`);
            }
        } catch (error) {
            console.error("Failed to move to next round:", error);
            setIsAdvancing(false);
        }
    }

    async function handleRoundUpdated() {
        if (!joinCode) return;

        try {
            const round = await getCurrentRound(joinCode);
            setRound(round);

            if (
                initialRoundNumber.current !== null &&
                round.roundNumber > initialRoundNumber.current
            ) {
                navigate(`/game/${joinCode}`);
            }
        } catch (error) {
            console.error("Failed to update winner screen:", error);
        }
    }

    function handleGameFinished() {
        navigate(`/results/${joinCode}`);
    }

    if (!currentRoundData) {
        return <div>Loading...</div>;
    }

    const isJudge = currentRoundData.judgeId === playerId;

    return (
        <div className="min-h-screen bg-slate-100 flex justify-center p-6">
            <div className="w-full max-w-3xl">

                {/* Normal page heading */}
                <h1 className="text-3xl font-bold text-slate-800 text-center mb-4">
                    Round {currentRoundData.roundNumber} Winner!
                </h1>

                <h1 className="text-3xl font-bold text-slate-800 text-center mb-4">
                    {currentRoundData.winnerName} - Score = {currentRoundData.winnerScore}
                </h1>

                <div className="rounded-lg shadow-md overflow-hidden">
                    <MemePreview
                        imageUrl={`${API_URL}/images/${joinCode}/${currentRoundData.imageFileName}`}
                        caption={currentRoundData.selectedCaptionContent ?? ""}
                        memeStyle={currentRoundData.selectedCaptionStyle ?? "Classic"}
                    />
                </div>

                {/* Next round button */}
                {isJudge && (
                    <div className="flex justify-center mt-6">
                        <button
                            className="w-1/2 rounded-md bg-sky-600 py-2 font-medium text-white transition hover:bg-sky-700 disabled:cursor-not-allowed disabled:opacity-50"
                            onClick={handleMoveToNextRound}
                            disabled={isAdvancing}
                        >
                            {isAdvancing ? "Loading..." : "Next Round"}
                        </button>
                    </div>
                )}
            </div>
        </div>
    );


}