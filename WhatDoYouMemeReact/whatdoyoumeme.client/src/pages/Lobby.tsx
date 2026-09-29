import { getLobby, uploadImages, startGame } from "../api/games";
import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { GameLobbySummaryDto } from "../types/game";
import { connection } from "../signalr/gameHub";
import * as signalR from "@microsoft/signalr";

export default function Lobby() {
    
    const navigate = useNavigate();

    const { joinCode } = useParams();

    const playerId = Number(sessionStorage.getItem("playerId"));
    const accessToken = sessionStorage.getItem("accessToken");

    const [lobbyData, setLobby] = useState<GameLobbySummaryDto | null>(null);

    useEffect(() => {
        async function initialize() {
            if (!joinCode) {
                return;
            }

            // Initial load
            const lobby = await getLobby(joinCode);
            setLobby(lobby);

            // Start SignalR
            if (connection.state === signalR.HubConnectionState.Disconnected) {
                await connection.start();
            }

            await connection.invoke("JoinGame", joinCode);

            // Register listeners
            connection.on("LobbyUpdated", async () => {
                const lobby = await getLobby(joinCode);
                setLobby(lobby);
            });

            connection.on("GameStarted", () => {
                navigate(`/game/${joinCode}`);
            });
        }

        initialize();

        return () => {
            if (joinCode) {
                connection.invoke("LeaveGame", joinCode);
            }

            connection.off("LobbyUpdated");
            connection.off("GameStarted");
        };
    }, [joinCode, navigate]);

    const [files, setFiles] = useState<File[]>([]);

    function handleFiles(e: React.ChangeEvent<HTMLInputElement>) {
        if (e.target.files) {
            setFiles(Array.from(e.target.files));
        }
    }
    
    async function handleUpload() {
        await uploadImages(joinCode!, files, accessToken!);
    }

    async function handleStartGame() {
        await startGame(joinCode!);
    }

    const currentPlayer = lobbyData?.players.find(p => p.id == playerId);
    
    if (!lobbyData) {
        return (
            <div className="min-h-screen bg-slate-100 flex items-center justify-center">
                <p className="text-lg text-slate-600">Loading...</p>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-slate-100 flex items-center justify-center p-6">
            <div className="w-full max-w-2xl rounded-xl bg-white p-8 shadow-lg">

                <h1 className="mb-8 text-center text-4xl font-bold text-slate-800">
                    Lobby
                </h1>

                <div className="mb-6 grid grid-cols-2 gap-4">
                    <div className="rounded-lg bg-slate-50 p-4 text-center">
                        <h2 className="text-sm font-medium uppercase tracking-wide text-slate-500">
                            Join Code
                        </h2>
                        <p className="mt-1 font-mono text-3xl font-bold tracking-widest text-sky-600">
                            {lobbyData.joinCode}
                        </p>
                    </div>

                    <div className="rounded-lg bg-slate-50 p-4 text-center">
                        <h2 className="text-sm font-medium uppercase tracking-wide text-slate-500">
                            Images
                        </h2>
                        <p className="mt-1 text-3xl font-bold text-slate-800">
                            {lobbyData.imageCount}
                        </p>
                    </div>
                </div>

                <div className="mb-6">
                    <div className="mb-3 flex items-center justify-between">
                        <h2 className="text-lg font-semibold text-slate-800">
                            Players
                        </h2>
                        <span className="rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-600">
                            {lobbyData.players.length} players
                        </span>
                    </div>

                    <div className="divide-y rounded-lg border border-slate-200">
                        {lobbyData.players.map((player) => (
                            <div
                                key={player.id}
                                className={`flex items-center justify-between px-4 py-3 ${
                                    currentPlayer?.id === player.id
                                        ? "bg-sky-50"
                                        : "bg-white"
                                }`}
                            >
                                <span
                                    className={
                                        currentPlayer?.id === player.id
                                            ? "font-semibold text-sky-600"
                                            : "text-slate-700"
                                    }
                                >
                                    {player.name}
                                    {currentPlayer?.id === player.id && " (You)"}
                                </span>

                                {player.isHost && (
                                    <span className="rounded-full bg-amber-100 px-2.5 py-1 text-xs font-medium text-amber-700">
                                        Host
                                    </span>
                                )}
                            </div>
                        ))}
                    </div>
                </div>

                <div className="mb-6 rounded-lg border border-slate-200 bg-slate-50 p-4">
                    <h2 className="mb-3 font-semibold text-slate-800">
                        Game Images
                    </h2>

                    <div className="flex flex-col gap-3 sm:flex-row">
                        <input
                            className="flex-1 rounded-md border border-slate-300 bg-white px-3 py-2 text-sm
                                file:mr-3 file:rounded file:border-0 file:bg-slate-200
                                file:px-3 file:py-1.5 file:text-sm file:font-medium
                                hover:file:bg-slate-300"
                            type="file"
                            multiple
                            onChange={handleFiles}
                        />

                        <button
                            className="rounded-md bg-sky-600 px-5 py-2 font-medium text-white
                                transition hover:bg-sky-700"
                            onClick={handleUpload}
                        >
                            Upload
                        </button>
                    </div>
                </div>

                {currentPlayer?.isHost && lobbyData.players.length >= 3 && (
                    <button
                        className="w-full rounded-md bg-green-600 py-3 font-medium text-white
                            transition hover:bg-green-700"
                        onClick={handleStartGame}
                    >
                        Start Game
                    </button>
                )}

                {currentPlayer?.isHost && lobbyData.players.length < 3 && (
                    <p className="text-center text-sm text-slate-500">
                        At least 3 players are needed to start the game.
                    </p>
                )}

            </div>
        </div>
    );

    
}