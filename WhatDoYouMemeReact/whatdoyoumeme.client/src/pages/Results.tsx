import { getResults, cleanupGame } from "../api/games";
import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { GameResultsDto } from "../types/game";

export default function Winner() {
    const navigate = useNavigate();
    const { joinCode } = useParams();

    const [resultsData, setResults] = useState<GameResultsDto | null>(null);

    useEffect(() => {
        async function loadResults() {
            if (!joinCode) {
                return;
            }

            try {
                const results = await getResults(joinCode);
                setResults(results);

                await cleanupGame(joinCode);
            }
            catch (error) {
                console.error(error);
            }
        }

        loadResults();
    }, [joinCode]);

    function handleGoHome() {
        navigate("/");
    }
    

    if (!resultsData) {
        return <div>Loading...</div>;
    }

    //const isWinner = resultsData.winners.some(w => w.id === playerId);
    return (
        <div className="min-h-screen bg-slate-100 flex items-center justify-center p-6">
            <div className="w-full max-w-2xl rounded-xl bg-white shadow-lg p-8">

                <h1 className="text-4xl font-bold text-center text-slate-800 mb-2">
                    🎉 Game Over
                </h1>

                <p className="text-center text-slate-500 mb-8">
                    {resultsData.winners.length === 1
                        ? "We have a winner!"
                        : "It's a tie!"}
                </p>

                <div className="rounded-lg bg-amber-50 border border-amber-200 p-5 mb-8">
                    <h2 className="text-xl font-semibold text-center text-amber-700 mb-4">
                        🏆 {resultsData.winners.length === 1 ? "Winner" : "Winners"}
                    </h2>

                    {resultsData.winners.map((player) => (
                        <div
                            key={player.id}
                            className="flex justify-between items-center py-2 text-lg"
                        >
                            <span className="font-medium">{player.name}</span>
                            <span className="font-bold">{player.score} pts</span>
                        </div>
                    ))}
                </div>

                <div className="mb-8">
                    <h2 className="text-xl font-semibold text-slate-800 mb-3">
                        Final Scores
                    </h2>

                    <div className="rounded-lg border border-slate-200 divide-y">
                        {resultsData.playerSummaryDtos.map((player) => (
                            <div
                                key={player.id}
                                className="flex justify-between items-center px-4 py-3"
                            >
                                <span>{player.name}</span>
                                <span className="font-semibold">
                                    {player.score} pts
                                </span>
                            </div>
                        ))}
                    </div>
                </div>

                <button
                    onClick={handleGoHome}
                    className="w-full rounded-lg bg-sky-600 py-3 font-medium text-white transition hover:bg-sky-700"
                >
                    Back to Home
                </button>

            </div>
        </div>
    );
}