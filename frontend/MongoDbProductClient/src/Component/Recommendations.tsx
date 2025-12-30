import { useEffect, useState } from 'react';
import API from '../API/api';
import type { Game } from '../Interface/baseInterface';
import { Link } from 'react-router-dom';

export const Recommendations = () => {
    const [recommendations, setRecommendations] = useState<Game[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchRecommendations = async () => {
            const userString = localStorage.getItem('user');
            if (!userString) {
                setLoading(false);
                return;
            }

            try {
                const user = JSON.parse(userString);
                const { data } = await API.get<Game[]>(`/Game/recommendations/${user.id}`);
                setRecommendations(data);
            } catch (error) {
                console.error('Error fetching recommendations:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchRecommendations();
    }, []);

    if (loading) {
        return <p>Loading recommendations...</p>;
    }

    if (recommendations.length === 0) {
        return null; // Don't render anything if there are no recommendations
    }

    return (
        <div className="mt-12">
            <h2 className="text-2xl font-bold text-gray-800 mb-6">Recommended For You</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {recommendations.map((game) => (
                    <div key={game.id} className="bg-white rounded-lg shadow-md overflow-hidden">
                        <img src={game.coverImageUrl || 'https://via.placeholder.com/300'} alt={game.name} className="w-full h-48 object-cover" />
                        <div className="p-4">
                            <h3 className="text-lg font-semibold">{game.name}</h3>
                            <p className="text-gray-600 text-sm">{game.genre}</p>
                            <Link to={`/game/${game.id}`} className="text-blue-500 hover:underline mt-2 inline-block">
                                View Details
                            </Link>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};
