import {BrowserRouter as Router, Routes, Route } from "react-router-dom"
import Navbar from "./Component/NavBar"
import { GameList } from "./Component/GameList"
import { GameDetail } from "./Component/GameDetail"
import { CreateGame } from "./Component/CreateGame"
import { EditGame } from "./Component/EditGame"
import { Register } from "./Component/Register"
import { Login } from "./Component/Login"
import { MyLibrary } from "./Component/MyLibrary"
import UserInfo from "./Component/UserInfo"
import GamePage from "./Component/GamePage"

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-100">
        <Navbar />
        <Routes>
          <Route path="/" element={<GameList />} />
          <Route path="/game/:id" element={<GameDetail />} />
          <Route path="/create-game" element={<CreateGame />} />
          <Route path="/edit-game/:id" element={<EditGame />} />
          <Route path="/register" element={<Register />} />
          <Route path="/login" element={<Login />} />
          <Route path="/my-library" element={<MyLibrary />} />
          <Route path="/user-info" element={<UserInfo />} />
          <Route path="/game/play" element={<GamePage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
