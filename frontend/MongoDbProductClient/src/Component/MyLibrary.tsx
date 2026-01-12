import { useEffect, useState } from 'react';
import API from '../API/api';
import type { Game } from '../Interface/baseInterface';
import { Link } from 'react-router-dom';

export const MyLibrary = () => {
    const [games, setGames] = useState<Game[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchLibrary = async () => {
            try {
                const { data } = await API.get<Game[]>('/User/my-library');
                setGames(data);
            } catch (err) {
                setError('Failed to load your library.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchLibrary();
    }, []);

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
            <h1 className="text-3xl font-bold text-gray-800 mb-8">My Library</h1>
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
                            <Link to={`/Play/${game.id}`} className="mt-4 bg-green-500 hover:bg-green-600 text-white font-bold py-2 px-4 rounded text-center">
                                Play
                            </Link>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};
