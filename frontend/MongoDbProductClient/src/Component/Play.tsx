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
    const [hasOwned, setHasOwned] = useState(false);
    
    const playtimeIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
    const accumulatedSecondsRef = useRef<number>(0);
    const hasBeenUnloadedRef = useRef<boolean>(false);

    useEffect(() => {
        const fetchGame = async () => {
            try {
                if (!id) {
                    setError('Invalid game ID');
                    return;
                }
                const { data } = await API.get<Game>(`/Game/${id}`);
                setGame(data);
                
                // Check if user owns the game by fetching user library
                try {
                    const userResponse = await API.get('/User/me');
                    const ownedGames = userResponse.data.ownedGamesIds || [];
                    const owned = ownedGames.includes(id);
                    setHasOwned(owned);
                    
                    if (!owned) {
                        setError('You do not own this game. Purchase it to play.');
                    }
                } catch (err) {
                    setError('Failed to verify game ownership.');
                    console.error(err);
                }
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
        if (game && !loading && hasOwned) {
            console.log(`Starting playtime tracking for game: ${game.id}`);
            accumulatedSecondsRef.current = 0;
            hasBeenUnloadedRef.current = false;

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
    }, [game, loading, hasOwned]);

    // Handle page unload (user closes tab/navigates away)
    useEffect(() => {
        const handleBeforeUnload = () => {
            if (accumulatedSecondsRef.current > 0 && game && !hasBeenUnloadedRef.current) {
                // Mark as unloaded to prevent double-sending
                hasBeenUnloadedRef.current = true;
                
                // Send final playtime update
                const seconds = accumulatedSecondsRef.current;
                const data = JSON.stringify({ gameId: game.id, secondsPlayed: seconds });
                const headers: Record<string, string> = { 'Content-Type': 'application/json' };
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
        // Mark as unloaded to prevent beforeunload from sending again
        hasBeenUnloadedRef.current = true;
        
        // Send final update before navigating away
        if (playtimeIntervalRef.current) {
            clearInterval(playtimeIntervalRef.current);
        }
        
        if (accumulatedSecondsRef.current > 0 && game) {
            try {
                // Send only the accumulated time without adding to it
                const secondsToSend = accumulatedSecondsRef.current;
                accumulatedSecondsRef.current = 0; // Reset to prevent double-sending
                
                await API.post('/Play/playtime/update', {
                    gameId: game.id,
                    secondsPlayed: secondsToSend,
                });
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

    if (error || !game || !hasOwned) {
        return (
            <div className="container mx-auto px-4 py-8">
                <button
                    onClick={() => navigate(-1)}
                    className="mb-4 bg-gray-500 hover:bg-gray-600 text-white font-bold py-2 px-4 rounded"
                >
                    Back
                </button>
                <div className="text-red-500 text-center mt-10">{error || 'Game not found or not owned'}</div>
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

            {/* Embed game via iframe - all games use the same web build */}
            <iframe
                src={`${BACKEND_URL}/games/web/index.html`}
                style={{
                    width: '100%',
                    height: '100%',
                    border: 'none',
                    display: 'block',
                }}
                title={`Play ${game.name}`}
                sandbox="allow-same-origin allow-scripts allow-forms allow-popups"
            />
        </div>
    );
};
