import { useParams, useNavigate } from 'react-router-dom';
import { useEffect, useState, useRef } from 'react';
import API, { BACKEND_URL } from '../API/api';
import type { Game } from '../Interface/baseInterface';

export const Play = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [game, setGame] = useState<Game | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    
    const playtimeIntervalRef = useRef<NodeJS.Timeout | null>(null);
    const accumulatedSecondsRef = useRef<number>(0);

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

    // Start playtime tracking when game loads
    useEffect(() => {
        if (game && !loading) {
            console.log(`Starting playtime tracking for game: ${game.id}`);
            accumulatedSecondsRef.current = 0;

            // Send playtime update every 60 seconds
            playtimeIntervalRef.current = setInterval(() => {
                sendPlaytimeUpdate(60);
            }, 60000);
        }

        return () => {
            // Cleanup: stop tracking
            if (playtimeIntervalRef.current) {
                clearInterval(playtimeIntervalRef.current);
            }
        };
    }, [game, loading]);

    // Handle page unload (user closes tab/navigates away)
    useEffect(() => {
        const handleBeforeUnload = () => {
            if (accumulatedSecondsRef.current > 0 && game) {
                // Send final playtime update
                const seconds = accumulatedSecondsRef.current;
                const data = JSON.stringify({ gameId: game.id, secondsPlayed: seconds });
                const headers = { 'Content-Type': 'application/json' };
                const token = localStorage.getItem('user') 
                    ? JSON.parse(localStorage.getItem('user')!).token 
                    : null;
                
                if (token) {
                    headers['Authorization'] = `Bearer ${token}`;
                }

                // Use sendBeacon for reliability
                navigator.sendBeacon(
                    `${BACKEND_URL}/api/Play/playtime/update`,
                    data
                );
            }
        };

        window.addEventListener('beforeunload', handleBeforeUnload);
        return () => window.removeEventListener('beforeunload', handleBeforeUnload);
    }, [game]);

    const sendPlaytimeUpdate = async (seconds: number) => {
        if (!game) return;

        try {
            accumulatedSecondsRef.current += seconds;
            console.log(`Sending playtime update: ${seconds}s (total: ${accumulatedSecondsRef.current}s)`);

            await API.post('/Play/playtime/update', {
                gameId: game.id,
                secondsPlayed: seconds,
            });

            console.log(`Playtime updated successfully`);
        } catch (err) {
            console.error('Error updating playtime:', err);
        }
    };

    const handleBackClick = async () => {
        // Send final update before navigating away
        if (playtimeIntervalRef.current) {
            clearInterval(playtimeIntervalRef.current);
        }
        
        if (accumulatedSecondsRef.current > 0 && game) {
            try {
                await sendPlaytimeUpdate(accumulatedSecondsRef.current);
            } catch (err) {
                console.error('Error sending final playtime update:', err);
            }
        }

        navigate(-1);
    };

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
                onClick={handleBackClick}
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
