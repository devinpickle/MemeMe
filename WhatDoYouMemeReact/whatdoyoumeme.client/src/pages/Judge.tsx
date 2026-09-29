import { getCurrentRoundCaptions, getCurrentRound, updateSelectedCaption, judgeCaption } from "../api/games";
import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { RoundCaptionsDto, RoundSummaryDto, CaptionSummaryDto } from "../types/game";
import { connection } from "../signalr/gameHub";
import * as signalR from "@microsoft/signalr";
import MemePreview from "./MemePreview";

const API_URL = import.meta.env.VITE_API_URL;

export default function Judge() {

    const navigate = useNavigate();
    
    const { joinCode } = useParams();

    const playerId = Number(sessionStorage.getItem("playerId"));

    const [currentRoundData, setRound] = useState<RoundSummaryDto | null>(null);
    const [roundDataCaps, setRoundCaptions] = useState<RoundCaptionsDto | null>(null);
    const [selectedCaption, setSelectedCaption] = useState<CaptionSummaryDto | null>(null);
    const [isSubmittingWinner, setIsSubmittingWinner] = useState(false);

    useEffect(() => {
        async function initialize() {
            if (!joinCode) {
                return;
            }

            await refreshJudge();

            if (connection.state === signalR.HubConnectionState.Disconnected) {
                await connection.start();
            }

            await connection.invoke("JoinGame", joinCode);

            connection.on("RoundUpdated", refreshJudge);
        }

        initialize();

        return () => {
            connection.off("RoundUpdated", refreshJudge);

            if (joinCode) {
                connection.invoke("LeaveGame", joinCode).catch(() => {});
            }
        };

        async function refreshJudge() {
            if (!joinCode) {
                return;
            }

            const round = await getCurrentRound(joinCode);

            setRound(round);

            if (round.state.valueOf() == "ShowingWinner") {
                navigate(`/winner/${joinCode}`);
                return;
            }

            const captions = await getCurrentRoundCaptions(joinCode);
            setRoundCaptions(captions);
        }
    }, [joinCode]);
    
    // Update the selected caption
    async function handleSelectCaption(caption: CaptionSummaryDto) {
        setSelectedCaption(caption);

        await updateSelectedCaption(joinCode!, caption.id);
    }

    async function handlePickWinner(caption: CaptionSummaryDto) {
        if (isSubmittingWinner) {
            return;
        }

        setIsSubmittingWinner(true);

        console.log("Picking winner");
        console.log({
            joinCode,
            playerId,
            captionId: caption.id
        });

        try {
            await judgeCaption(joinCode!, playerId, caption.id);
        } catch (error) {
            console.error("Failed to pick winner:", error);
            setIsSubmittingWinner(false);
        }
    }

    if (!currentRoundData || !roundDataCaps) {
        return <div>Loading...</div>;
    }

    const isJudge = currentRoundData.judgeId === playerId;
    return (
        <div className="min-h-screen bg-slate-100 flex justify-center p-6">
            <div className="w-full max-w-3xl bg-white rounded-lg shadow-md p-6">
                <h1 className="text-3xl font-bold text-slate-800 text-center mb-4">
                    Round {currentRoundData.roundNumber}
                </h1>

                {currentRoundData.selectedCaptionContent ? (
                                <div className="mt-6">
                                    <MemePreview
                                        imageUrl={`${API_URL}/images/${joinCode}/${currentRoundData.imageFileName}`}
                                        caption={currentRoundData.selectedCaptionContent}
                                        memeStyle={currentRoundData.selectedCaptionStyle ?? "Classic"}
                                    />
                                </div>
                            ) : (
                                <h2 className="text-2xl font-semibold text-slate-700">
                                    Waiting for the judge to select a caption...
                                </h2>
                            )}

                <div className="text-center max-w-md mx-auto">
                    {isJudge ? (
                        <div className="flex flex-col gap-3 mt-4">

                            {roundDataCaps.captions.map((captionSummary) => {
                                return (
                                    <div
                                        key={captionSummary.id}
                                        className={`cursor-pointer rounded-lg border-2 p-3 transition ${
                                            selectedCaption?.id === captionSummary.id
                                                ? "border-sky-500 bg-sky-50"
                                                : "border-slate-200 bg-white hover:border-slate-300"
                                        }`}
                                        onClick={() => handleSelectCaption(captionSummary)}
                                    >
                                        <p className="mt-2 text-sm text-slate-500">
                                            {captionSummary.content}
                                        </p>
                                    </div>
                                )})
                            }

                            <button
                                className={`rounded border p-2 ${
                                    selectedCaption && !isSubmittingWinner
                                        ? "bg-red-200 hover:bg-red-300"
                                        : "bg-slate-200 text-slate-400 cursor-not-allowed"
                                }`}
                                disabled={!selectedCaption || isSubmittingWinner}
                                onClick={() => handlePickWinner(selectedCaption!)}
                            >
                                {isSubmittingWinner ? "Choosing Winner..." : "Choose Winner"}
                            </button>

                        </div>
                    ) : (
                        <div className="flex flex-col gap-4">
                            
                            

                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}