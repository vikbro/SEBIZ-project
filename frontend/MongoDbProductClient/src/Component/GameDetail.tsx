import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import type { Game } from '../Interface/baseInterface';
import API from '../API/api';

export const GameDetail = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [game, setGame] = useState<Game | null>(null);
    const [loading, setLoading] = useState(true);
    const [message, setMessage] = useState('');
    const [isOwner, setIsOwner] = useState(false);

    useEffect(() => {
        const fetchGame = async () => {
            if (!id) return;
            try {
                const { data } = await API.get(`/Game/${id}`);
                setGame(data);
            } catch (error) {
                console.error('Error fetching game:', error);
            } finally {
                setLoading(false);
            }
        };
        fetchGame();
    }, [id]);

    useEffect(() => {
        const userString = localStorage.getItem('user');
        if (userString && game) {
            const user = JSON.parse(userString);
            if (user.id === game.createdById) {
                setIsOwner(true);
            }
        }
    }, [game]);

    const handleAddToLibrary = async () => {
        if (!localStorage.getItem('user')) {
            setMessage('You need to be logged in to purchase games.');
            return;
        }
        if (!id) return;

        try {
            await API.post(`/User/purchase/${id}`);
            setMessage('Game purchased successfully!');
        } catch (error) {
            setMessage('Failed to purchase game. You may already own it.');
            console.error('Error purchasing game:', error);
        }
    };

    const handleDelete = async () => {
        if (!id) return;
        try {
            await API.delete(`/Game/${id}`);
            navigate('/');
        } catch (error) {
            setMessage('Failed to delete game.');
            console.error('Error deleting game:', error);
        }
    };

    if (loading) {
        return <div className="text-center mt-10">Loading...</div>;
    }

    if (!game) {
        return <div className="text-center mt-10">Game not found.</div>;
    }

    return (
        <div className="max-w-4xl mx-auto px-4 py-8">
            <div className="bg-white rounded-lg shadow-lg p-6">
                <h1 className="text-3xl font-bold text-gray-800 mb-2">{game.name}</h1>
                <p className="text-lg text-gray-500 mb-4">by {game.developer}</p>
                <p className="text-gray-600 mb-4">{game.description}</p>
                <p className="mb-2"><strong>Genre:</strong> {game.genre}</p>
                <p className="mb-4"><strong>Release Date:</strong> {new Date(game.releaseDate).toLocaleDateString()}</p>
                <div className="mb-6">
                    <strong>Tags:</strong>
                    {game.tags?.map(tag => <span key={tag} className="bg-gray-200 text-gray-800 text-xs font-semibold mr-2 px-2.5 py-0.5 rounded-full">{tag}</span>)}
                </div>
                <div className="text-2xl font-bold text-blue-600 mb-6">
                    ${game.price?.toFixed(2)}
                </div>
                <div className="flex space-x-4">
                    <button onClick={handleAddToLibrary} className="bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded-md">
                        Add to Library
                    </button>
                    {isOwner && (
                        <>
                            <button onClick={() => navigate(`/edit-game/${id}`)} className="bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded-md">
                                Edit Game
                            </button>
                            <button onClick={handleDelete} className="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-md">
                                Delete Game
                            </button>
                        </>
                    )}
                </div>
                {message && <p className="mt-4 text-green-600">{message}</p>}
            </div>
        </div>
    );
};
