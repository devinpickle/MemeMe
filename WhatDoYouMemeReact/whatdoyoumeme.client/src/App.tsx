import { Link, Route, Routes } from "react-router-dom";
import Home from "./pages/Home";
import Lobby from "./pages/Lobby";
import Game from "./pages/Game";
import Judge from "./pages/Judge";
import Winner from "./pages/Winner";
import Results from "./pages/Results";

export default function App() {
    return <div>
      <Link to="/">Meme Me</Link>
      <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/lobby/:joinCode" element={<Lobby />} />
          <Route path="/game/:joinCode" element={<Game />} />
          <Route path="/judge/:joinCode" element={<Judge />} />
          <Route path="/winner/:joinCode" element={<Winner />} />
          <Route path="/results/:joinCode" element={<Results />} />
      </Routes>
    </div>;
}