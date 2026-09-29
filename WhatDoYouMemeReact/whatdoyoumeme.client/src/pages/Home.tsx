import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createGame, joinGame } from "../api/games";

export default function Home(){

    const navigate = useNavigate();

    const [name, setName] = useState("");
    const [joinCode, setJoinCode] = useState("");
    const [numberOfRounds, setNumRounds] = useState(1);

    async function handleCreateGame() {
        if (numberOfRounds < 1) {
            return;
        }
        const game = await createGame(name, numberOfRounds);

        sessionStorage.setItem("playerId", game.playerId.toString());
        sessionStorage.setItem("accessToken", game.accessToken);
        sessionStorage.setItem("joinCode", game.joinCode);

        navigate(`/lobby/${game.joinCode}`);
    }

    async function handleJoinGame() {
        const player = await joinGame(joinCode, name);

        sessionStorage.setItem("playerId", player.playerId.toString());
        sessionStorage.setItem("accessToken", player.accessToken);
        sessionStorage.setItem("joinCode", player.joinCode);

        navigate(`/lobby/${player.joinCode}`);
    }

    return (
        <div className="min-h-screen flex items-center justify-center bg-slate-100">
            <div className="w-full max-w-md rounded-xl bg-white p-8 shadow-md">
                <h1 className="mb-6 text-center text-3xl font-bold text-slate-800">
                    Meme Me
                </h1>

                <div className="space-y-4">
                    <div>
                        <label className="mb-1 block text-sm font-medium text-slate-700">
                            Your Name
                        </label>
                        <input
                            className="w-full rounded-md border border-slate-300 px-3 py-2 outline-none focus:border-sky-500 focus:ring-1 focus:ring-sky-500"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                        />
                    </div>

                    <label className="mb-1 block text-sm font-medium text-slate-700">
                        Number of Rounds
                    </label>
                    <input
                        type="number"
                        min="1"
                        className="w-full rounded-md border border-slate-300 px-3 py-2 uppercase outline-none focus:border-sky-500 focus:ring-1 focus:ring-sky-500"
                        value={numberOfRounds}
                        onChange={(e) => setNumRounds(Number(e.target.value))}
                    />

                    <button
                        className="w-full rounded-md bg-sky-600 py-2 font-medium text-white transition hover:bg-sky-700"
                        onClick={handleCreateGame}
                    >
                        Create Game
                    </button>

                    <div className="flex items-center gap-3 py-2">
                        <div className="h-px flex-1 bg-slate-300" />
                        <span className="text-sm text-slate-500">OR</span>
                        <div className="h-px flex-1 bg-slate-300" />
                    </div>

                    <div>
                        <label className="mb-1 block text-sm font-medium text-slate-700">
                            Join Code
                        </label>
                        <input
                            className="w-full rounded-md border border-slate-300 px-3 py-2 uppercase outline-none focus:border-sky-500 focus:ring-1 focus:ring-sky-500"
                            value={joinCode}
                            onChange={(e) => setJoinCode(e.target.value)}
                        />
                    </div>

                    <button
                        className="w-full rounded-md bg-slate-800 py-2 font-medium text-white transition hover:bg-slate-900"
                        onClick={handleJoinGame}
                    >
                        Join Game
                    </button>
                </div>
            </div>
        </div>
    );
}