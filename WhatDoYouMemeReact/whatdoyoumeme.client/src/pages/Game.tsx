import { getCurrentRound, submitCaption } from "../api/games";
import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { MemeStyle, RoundSummaryDto } from "../types/game";
import { connection } from "../signalr/gameHub";
import * as signalR from "@microsoft/signalr";
import MemePreview from "./MemePreview";

const API_URL = import.meta.env.VITE_API_URL;

export default function Game() {

    const navigate = useNavigate();

    const { joinCode } = useParams();

    const playerId = Number(sessionStorage.getItem("playerId"));

    const [currentRoundData, setRound] = useState<RoundSummaryDto | null>(null);

    const [caption, setCaption] = useState("");
    const [submitted, setSubmitted] = useState(false);

    const [memeStyle, setMemeStyle] = useState<MemeStyle>("Classic");

    useEffect(() => {
        async function initialize() {
            if (!joinCode) return;

            const round = await getCurrentRound(joinCode);
            setRound(round);

            if (connection.state === signalR.HubConnectionState.Disconnected) {
                await connection.start();
            }

            await connection.invoke("JoinGame", joinCode);

            connection.on("RoundUpdated", handleRoundUpdated);
        }

        initialize();

        return () => {
            connection.off("RoundUpdated", handleRoundUpdated);

            if (joinCode) {
                connection.invoke("LeaveGame", joinCode);
            }
        };
    }, [joinCode]);

    useEffect(() => {
        if (!currentRoundData) {
            return;
        }

        if (currentRoundData.state.valueOf() == "Judging") {
            navigate(`/judge/${joinCode}`);
        }
    }, [currentRoundData, joinCode, navigate]);

    async function handleRoundUpdated() {
        if (!joinCode) return;

        const round = await getCurrentRound(joinCode);
        setRound(round);
    }

    async function handleSubmitCaption() {
        const trimmedCaption = caption.trim();

        if (trimmedCaption != "") {
            await submitCaption(joinCode!, playerId, caption, memeStyle);
            setSubmitted(true);
        }
    }

    if (!currentRoundData) {
        return <div>Loading...</div>;
    }

    const isJudge = currentRoundData.judgeId === playerId;
    return (
        <div className="min-h-screen bg-slate-100 flex justify-center p-6">
            <div className="w-full max-w-3xl bg-white rounded-lg shadow-md p-6">
                <h1 className="text-3xl font-bold text-slate-800 text-center mb-4">
                    Round {currentRoundData.roundNumber}
                </h1>

                <div className="text-center mb-6">
                    <p className="text-slate-600">
                        Captions Submitted
                    </p>
                    <p className="text-xl font-semibold text-slate-800">
                        {currentRoundData.submittedCaptionCount} / {currentRoundData.captionsExpected}
                    </p>
                </div>

                <MemePreview
                    imageUrl={`${API_URL}/images/${joinCode}/${currentRoundData.imageFileName}`}
                    caption={caption}
                    memeStyle={memeStyle}
                />

                <div className="text-center max-w-md mx-auto">
                    {isJudge ? (
                        <h2 className="text-2xl font-semibold text-sky-600">
                            You are the Judge
                        </h2>
                    ) : (
                        <div className="flex flex-col gap-4 mt-4">

                            <textarea
                                value={caption}
                                onChange={(e) => setCaption(e.target.value)}
                                placeholder="Enter a caption!"
                                rows={4}
                                className="w-full rounded-lg border border-slate-300 p-3 text-slate-700 placeholder:text-slate-400 focus:border-sky-500 focus:ring-2 focus:ring-sky-200 outline-none resize-none"
                            />

                            <div className="flex gap-2 justify-center">
                                <button
                                    onClick={() => setMemeStyle("Classic")}
                                    className={
                                        memeStyle === "Classic"
                                            ? "rounded bg-sky-600 px-4 py-2 text-white"
                                            : "rounded bg-slate-200 px-4 py-2"
                                    }
                                >
                                    Classic
                                </button>

                                <button
                                    onClick={() => setMemeStyle("Impact")}
                                    className={
                                        memeStyle === "Impact"
                                            ? "rounded bg-sky-600 px-4 py-2 text-white"
                                            : "rounded bg-slate-200 px-4 py-2"
                                    }
                                >
                                    Impact
                                </button>

                                <button
                                    onClick={() => setMemeStyle("BottomCaption")}
                                    className={
                                        memeStyle === "BottomCaption"
                                            ? "rounded bg-sky-600 px-4 py-2 text-white"
                                            : "rounded bg-slate-200 px-4 py-2"
                                    }
                                >
                                    Bottom Caption
                                </button>
                            </div>

                            <button
                                onClick={handleSubmitCaption}
                                disabled={submitted}
                                className={`rounded-lg px-4 py-2 font-medium transition ${
                                    submitted
                                        ? "bg-slate-300 text-slate-500 cursor-not-allowed"
                                        : "bg-sky-600 text-white hover:bg-sky-700"
                                }`}
                            >
                                {submitted ? "Caption Submitted" : "Submit Caption"}
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}