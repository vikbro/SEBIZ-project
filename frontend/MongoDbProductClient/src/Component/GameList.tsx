import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Game } from '../Interface/baseInterface';
import API from '../API/api';
import { Recommendations } from './Recommendations';

export const GameList = () => {
    const navigate = useNavigate();
    const [games, setGames] = useState<Game[]>([]);
    const [loading, setLoading] = useState(true);
    const [userId, setUserId] = useState('');

    useEffect(() => {
        const userString = localStorage.getItem('user');
        if (userString) {
            const user = JSON.parse(userString);
            setUserId(user.id);
        }
    }, []);

    const fetchGames = async () => {
        try {
            const { data } = await API.get('/Game');
            setGames(data);
            setLoading(false);
        } catch (error) {
            console.error('Error fetching games:', error);
            setLoading(false);
        }
    };

    const handleDelete = async (id: string) => {
        if (window.confirm('Are you sure you want to delete this game?')) {
            try {
                await API.delete(`/Game/${id}`);
                fetchGames();
            } catch (error) {
                console.error('Error deleting game:', error);
            }
        }
    };

    useEffect(() => {
        fetchGames();
    }, []);

    if (loading) {
        return (
            <div className="flex justify-center items-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8">
            <h1 className="text-3xl font-bold text-gray-800 mb-8">Our Games</h1>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {games.map((game) => (
                    <div
                        key={game.id}
                        className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow duration-300"
                    >
                        <img src={game.imagePath} alt={game.name} className="w-full h-48 object-cover" />
                        <div className="p-6">
                            <h2 className="text-xl font-semibold text-gray-800 mb-2">
                                {game.name || 'Unnamed Game'}
                            </h2>
                            <p className="text-gray-600 mb-4">
                                {game.description || 'No description available'}
                            </p>
                            <p className="text-gray-500 text-sm mb-2">Developer: {game.developer}</p>
                            <p className="text-gray-500 text-sm mb-4">Genre: {game.genre}</p>
                            <div className="flex justify-between items-center">
                                <span className="text-2xl font-bold text-blue-600">
                                    ${game.price?.toFixed(2) || 'N/A'}
                                </span>
                                <button
                                    onClick={() => navigate(`/game/${game.id}`)}
                                    className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-md"
                                >
                                    View Details
                                </button>
                                {userId === game.createdById && (
                                    <>
                                        <button
                                            onClick={() => navigate(`/edit-game/${game.id}`)}
                                            className="bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded-md"
                                        >
                                            Edit
                                        </button>
                                        <button
                                            onClick={() => handleDelete(game.id)}
                                            className="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-md"
                                        >
                                            Delete
                                        </button>
                                    </>
                                )}
                            </div>
                        </div>
                    </div>
                ))}
            </div>
            <Recommendations />
        </div>
    );
};
