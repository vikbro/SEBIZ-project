import { useEffect, useState } from 'react';
import API from '../API/api';
import type { Game, User } from '../Interface/baseInterface';

export const MyLibrary = () => {
    const [games, setGames] = useState<Game[]>([]);
    const [user, setUser] = useState<User | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [playMessage, setPlayMessage] = useState('');

    useEffect(() => {
        const fetchLibrary = async () => {
            try {
                const userString = localStorage.getItem('user');
                if (!userString) {
                    setError('You must be logged in to view your library.');
                    setLoading(false);
                    return;
                }

                const userData: User = JSON.parse(userString);
                setUser(userData);
                const gameIds = userData.ownedGamesIds || [];

                if (gameIds.length === 0) {
                    setLoading(false);
                    return;
                }

                const gamePromises = gameIds.map((id: string) => API.get<Game>(`/Game/${id}`));
                const gameResponses = await Promise.all(gamePromises);
                const gamesData = gameResponses.map(response => response.data);

                setGames(gamesData);
            } catch (err) {
                setError('Failed to load your library.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchLibrary();
    }, []);

    const handlePlayGame = async (gameId: string) => {
        const userString = localStorage.getItem('user');
        if (!userString) {
            setPlayMessage('You must be logged in to play.');
            return;
        }
        const user = JSON.parse(userString);

        // Simulate playing for 1 minute
        const minutesPlayed = 1;

        try {
            await API.post('/GameUsage/update', {
                userId: user.id,
                gameId,
                minutesPlayed
            });
            setPlayMessage(`You "played" ${games.find(g => g.id === gameId)?.name} for ${minutesPlayed} minute.`);
        } catch (error) {
            setPlayMessage('Failed to record play time.');
            console.error(error);
        }
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    if (error) {
        return <div className="text-red-500 text-center mt-10">{error}</div>;
    }

    return (
        <div className="container mx-auto px-4 py-8">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-3xl font-bold text-gray-800">My Library</h1>
                {user && (
                    <div className="text-xl font-semibold text-green-600">
                        Balance: ${user.balance?.toFixed(2)}
                    </div>
                )}
            </div>
            {playMessage && <p className="mb-4 text-green-600">{playMessage}</p>}
            {games.length === 0 ? (
                <p>You don't own any games yet.</p>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {games.map((game) => (
                        <div key={game.id} className="bg-white rounded-lg shadow-md p-6 flex flex-col justify-between">
                            <div>
                                <h2 className="text-xl font-semibold">{game.name}</h2>
                                <p className="text-gray-600">{game.description}</p>
                            </div>
                            <button
                                onClick={() => handlePlayGame(game.id)}
                                className="mt-4 bg-green-500 hover:bg-green-600 text-white font-bold py-2 px-4 rounded"
                            >
                                Play (Simulate 1 min)
                            </button>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};
