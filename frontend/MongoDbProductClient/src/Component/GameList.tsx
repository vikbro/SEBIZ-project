import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Game } from '../Interface/baseInterface';
import API from '../API/api';
import { Recommendations } from './Recommendations';
import { useTheme } from '../Context/ThemeContext';
import { useCart } from '../Context/CartContext';

const GENRES = ['Action', 'Adventure', 'RPG', 'Shooters', 'Strategy', 'Simulation', 'Sports', 'Puzzle'];

export const GameList = () => {
    const navigate = useNavigate();
    const { theme } = useTheme();
    const { addToCart } = useCart();
    const [games, setGames] = useState<Game[]>([]);
    const [filteredGames, setFilteredGames] = useState<Game[]>([]);
    const [loading, setLoading] = useState(true);
    const [userId, setUserId] = useState('');
    const [searchTerm, setSearchTerm] = useState('');
    const [selectedGenres, setSelectedGenres] = useState<string[]>([]);

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
            setFilteredGames(data);
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

    // Filter games based on search term and selected genres
    useEffect(() => {
        let filtered = games;

        // Filter by search term
        if (searchTerm.trim()) {
            const lowerSearch = searchTerm.toLowerCase();
            filtered = filtered.filter(game =>
                game.name.toLowerCase().includes(lowerSearch) ||
                game.description.toLowerCase().includes(lowerSearch) ||
                game.developer.toLowerCase().includes(lowerSearch)
            );
        }

        // Filter by genres
        if (selectedGenres.length > 0) {
            filtered = filtered.filter(game => {
                const gameGenres = game.genre.split(',').map(g => g.trim());
                return selectedGenres.some(genre => gameGenres.includes(genre));
            });
        }

        setFilteredGames(filtered);
    }, [searchTerm, selectedGenres, games]);

    const handleGenreChange = (genre: string) => {
        if (selectedGenres.includes(genre)) {
            setSelectedGenres(selectedGenres.filter(g => g !== genre));
        } else {
            setSelectedGenres([...selectedGenres, genre]);
        }
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8">
            {/* Recommendations Section - Displayed Above */}
            <Recommendations />
            
            <h1 className={`text-3xl font-bold mb-8 mt-12 ${
                theme === 'dark' ? 'text-gray-100' : 'text-gray-800'
            }`}>All Games</h1>

            {/* Search and Filter Section */}
            <div className={`rounded-lg shadow-md p-6 mb-8 transition-colors duration-200 ${
                theme === 'dark' 
                    ? 'bg-gray-800 border border-gray-700' 
                    : 'bg-white'
            }`}>
                {/* Search Bar */}
                <div className="mb-6">
                    <label htmlFor="search" className={`block font-semibold mb-2 ${
                        theme === 'dark' ? 'text-gray-200' : 'text-gray-700'
                    }`}>Search Games</label>
                    <input
                        id="search"
                        type="text"
                        placeholder="Search by name, description, or developer..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className={`w-full px-4 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors duration-200 ${
                            theme === 'dark'
                                ? 'bg-gray-700 border border-gray-600 text-gray-100 placeholder-gray-400'
                                : 'border border-gray-300 text-gray-900'
                        }`}
                    />
                </div>

                {/* Genre Filter */}
                <div>
                    <label className={`block font-semibold mb-2 ${
                        theme === 'dark' ? 'text-gray-200' : 'text-gray-700'
                    }`}>Filter by Genres</label>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                        {GENRES.map((genre) => (
                            <label key={genre} className="flex items-center">
                                <input
                                    type="checkbox"
                                    checked={selectedGenres.includes(genre)}
                                    onChange={() => handleGenreChange(genre)}
                                    className="w-4 h-4 text-blue-600 rounded"
                                />
                                <span className={`ml-2 ${
                                    theme === 'dark' ? 'text-gray-200' : 'text-gray-700'
                                }`}>{genre}</span>
                            </label>
                        ))}
                    </div>
                </div>

                {/* Clear Filters Button */}
                {(searchTerm || selectedGenres.length > 0) && (
                    <button
                        onClick={() => {
                            setSearchTerm('');
                            setSelectedGenres([]);
                        }}
                        className={`mt-4 px-4 py-2 rounded-md transition-colors ${
                            theme === 'dark'
                                ? 'bg-gray-700 hover:bg-gray-600 text-gray-100'
                                : 'bg-gray-400 hover:bg-gray-500 text-white'
                        }`}
                    >
                        Clear Filters
                    </button>
                )}

                {/* Results Count */}
                <p className={`mt-4 font-semibold ${
                    theme === 'dark' ? 'text-gray-300' : 'text-gray-600'
                }`}>
                    Showing {filteredGames.length} of {games.length} games
                </p>
            </div>

            {/* Games Grid */}
            {filteredGames.length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {filteredGames.map((game) => (
                        <div
                            key={game.id}
                            className={`rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-all duration-300 ${
                                theme === 'dark'
                                    ? 'bg-gray-800 border border-gray-700'
                                    : 'bg-white'
                            }`}
                        >
                            <img
                                src={game.imagePath || '/uploads/placeholder.png'}
                                alt={game.name}
                                className="w-full h-48 object-cover"
                                onError={(e) => {
                                    const target = e.target as HTMLImageElement;
                                    target.onerror = null;
                                    target.src = '/uploads/placeholder.png';
                                }}
                            />
                            <div className="p-6">
                                <h2 className={`text-xl font-semibold mb-2 ${
                                    theme === 'dark' ? 'text-gray-100' : 'text-gray-800'
                                }`}>
                                    {game.name || 'Unnamed Game'}
                                </h2>
                                <p className={`mb-4 ${
                                    theme === 'dark' ? 'text-gray-300' : 'text-gray-600'
                                }`}>
                                    {game.description || 'No description available'}
                                </p>
                                <p className={`text-sm mb-2 ${
                                    theme === 'dark' ? 'text-gray-400' : 'text-gray-500'
                                }`}>Developer: {game.developer}</p>
                                <p className={`text-sm mb-4 ${
                                    theme === 'dark' ? 'text-gray-400' : 'text-gray-500'
                                }`}>Genre: {game.genre}</p>
                                <div className="flex justify-between items-center gap-2 flex-wrap">
                                    <span className="text-2xl font-bold text-blue-600">
                                        ${game.price?.toFixed(2) || 'N/A'}
                                    </span>
                                    <button
                                        onClick={() => navigate(`/game/${game.id}`)}
                                        className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-md transition-colors"
                                    >
                                        View Details
                                    </button>
                                    <button
                                        onClick={() => addToCart(game)}
                                        className="ml-2 bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded-md transition-colors"
                                    >
                                        Add to Cart
                                    </button>
                                    {userId === game.createdById && (
                                        <>
                                            <button
                                                onClick={() => navigate(`/edit-game/${game.id}`)}
                                                className="bg-yellow-500 hover:bg-yellow-600 text-white px-4 py-2 rounded-md transition-colors text-sm"
                                            >
                                                Edit
                                            </button>
                                            <button
                                                onClick={() => handleDelete(game.id)}
                                                className="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-md transition-colors text-sm"
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
            ) : (
                <div className={`text-center py-12 rounded-lg shadow-md transition-colors duration-200 ${
                    theme === 'dark'
                        ? 'bg-gray-800 border border-gray-700'
                        : 'bg-white'
                }`}>
                    <p className={`text-lg ${
                        theme === 'dark' ? 'text-gray-300' : 'text-gray-600'
                    }`}>No games found matching your search or filters.</p>
                </div>
            )}
        </div>
    );
};
