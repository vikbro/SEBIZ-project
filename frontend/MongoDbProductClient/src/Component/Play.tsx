import { useParams, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import API, { BACKEND_URL } from '../API/api';
import type { Game } from '../Interface/baseInterface';

export const Play = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [game, setGame] = useState<Game | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchGame = async () => {
            try {
                if (!id) {
                    setError('Invalid game ID');
                    return;
                }
                const { data } = await API.get<Game>(`/Game/${id}`);
                setGame(data);
            } catch (err) {
                setError('Failed to load game.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchGame();
    }, [id]);

    if (loading) {
        return (
            <div className="flex justify-center items-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    if (error || !game) {
        return (
            <div className="container mx-auto px-4 py-8">
                <button
                    onClick={() => navigate(-1)}
                    className="mb-4 bg-gray-500 hover:bg-gray-600 text-white font-bold py-2 px-4 rounded"
                >
                    Back
                </button>
                <div className="text-red-500 text-center mt-10">{error || 'Game not found'}</div>
            </div>
        );
    }

    return (
        <div style={{ width: '100%', height: '100vh', display: 'flex', flexDirection: 'column', backgroundColor: '#000' }}>
            <button
                onClick={() => navigate(-1)}
                style={{
                    position: 'absolute',
                    top: '10px',
                    left: '10px',
                    zIndex: 1000,
                    backgroundColor: '#555',
                    color: 'white',
                    border: 'none',
                    padding: '10px 20px',
                    cursor: 'pointer',
                    borderRadius: '4px',
                }}
            >
                Back
            </button>

            {/* Embed Godot game via iframe */}
            <iframe
                src={`${BACKEND_URL}/games/web/index.html`}
                style={{
                    width: '100%',
                    height: '100%',
                    border: 'none',
                    display: 'block',
                }}
                title={`Play ${game.name}`}
            />
        </div>
    );
};
