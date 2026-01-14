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
import { Play } from "./Component/Play"
import { AdminPanel } from "./Component/AdminPanel"
import { ThemeProvider, useTheme } from "./Context/ThemeContext"
import { CartProvider } from "./Context/CartContext"
import Cart from './Component/Cart'

function AppContent() {
  const { theme } = useTheme();
  
  return (
    <Router>
      <div className={`min-h-screen transition-colors duration-200 ${
        theme === 'dark' 
          ? 'bg-gray-900 text-gray-100' 
          : 'bg-gray-100 text-gray-900'
      }`}>
        <Navbar />
        <Routes>
          <Route path="/" element={<GameList />} />
          <Route path="/game/:id" element={<GameDetail />} />
          <Route path="/cart" element={<Cart />} />
          <Route path="/create-game" element={<CreateGame />} />
          <Route path="/edit-game/:id" element={<EditGame />} />
          <Route path="/register" element={<Register />} />
          <Route path="/login" element={<Login />} />
          <Route path="/my-library" element={<MyLibrary />} />
          <Route path="/user-info" element={<UserInfo />} />
          <Route path="/Play/:id" element={<Play />} />
          <Route path="/admin" element={<AdminPanel />} />
        </Routes>
      </div>
    </Router>
  );
}

function App() {
  return (
    <ThemeProvider>
      <CartProvider>
        <AppContent />
      </CartProvider>
    </ThemeProvider>
  );
}

export default App;
