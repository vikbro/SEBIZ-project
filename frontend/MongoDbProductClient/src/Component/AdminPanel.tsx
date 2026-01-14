import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import API from '../API/api';
import { 
    getAdminTransactions, 
    getAllUsers, 
    promoteToAdmin, 
    demoteAdmin, 
    deleteGameAsAdmin,
    updateGameAsAdmin 
} from '../API/adminApi';

const GENRES = ['Action', 'Adventure', 'RPG', 'Shooters', 'Strategy', 'Simulation', 'Sports', 'Puzzle'];

interface User {
    id: string;
    username: string;
    email: string;
    balance: number;
    role: string;
    ownedGamesIds: string[];
}

interface Transaction {
    id: string;
    buyerId: string;
    buyerUsername: string;
    sellerId: string;
    sellerUsername: string;
    gameId: string;
    gameTitle: string;
    amount: number;
    transactionDate: string;
}

interface Game {
    id: string;
    name: string;
    description: string;
    price: number;
    genre: string;
    developer: string;
    releaseDate: string;
    tags: string[];
    imagePath: string;
    createdById: string;
    fileName: string;
}

export const AdminPanel = () => {
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState<'transactions' | 'users' | 'games'>('transactions');
    const [transactions, setTransactions] = useState<Transaction[]>([]);
    const [users, setUsers] = useState<User[]>([]);
    const [games, setGames] = useState<Game[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [currentUser, setCurrentUser] = useState<any>(null);
    const [editingGame, setEditingGame] = useState<Game | null>(null);
    const [selectedGenres, setSelectedGenres] = useState<string[]>([]);

    useEffect(() => {
        const user = localStorage.getItem('user');
        if (!user) {
            navigate('/login');
            return;
        }
        
        const userData = JSON.parse(user);
        setCurrentUser(userData);
        
        if (userData.role !== 'Admin') {
            navigate('/');
            return;
        }

        fetchTransactions();
    }, [navigate]);

    const fetchTransactions = async () => {
        setLoading(true);
        setError('');
        try {
            const response = await getAdminTransactions();
            setTransactions(response.data);
        } catch (err: any) {
            setError('Failed to fetch transactions');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const fetchUsers = async () => {
        setLoading(true);
        setError('');
        try {
            const response = await getAllUsers();
            setUsers(response.data);
        } catch (err: any) {
            setError('Failed to fetch users');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const fetchGames = async () => {
        setLoading(true);
        setError('');
        try {
            const response = await API.get('/Game');
            setGames(response.data);
        } catch (err: any) {
            setError('Failed to fetch games');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handlePromoteUser = async (userId: string) => {
        try {
            await promoteToAdmin(userId);
            fetchUsers();
        } catch (err) {
            setError('Failed to promote user');
            console.error(err);
        }
    };

    const handleDemoteUser = async (userId: string) => {
        try {
            await demoteAdmin(userId);
            fetchUsers();
        } catch (err) {
            setError('Failed to demote user');
            console.error(err);
        }
    };

    const handleDeleteGame = async (gameId: string) => {
        if (window.confirm('Are you sure you want to delete this game?')) {
            try {
                await deleteGameAsAdmin(gameId);
                fetchGames();
            } catch (err) {
                setError('Failed to delete game');
                console.error(err);
            }
        }
    };

    const handleUpdateGame = async (gameId: string) => {
        const game = games.find(g => g.id === gameId);
        if (game) {
            setEditingGame({ ...game });
            // Parse genres from the genre field
            const genres = game.genre.split(',').map(g => g.trim()).filter(g => g);
            setSelectedGenres(genres);
        }
    };

    const handleSaveGame = async () => {
        if (!editingGame) return;
        if (selectedGenres.length === 0) {
            setError('Please select at least one genre');
            return;
        }
        try {
            const updatedGame = {
                ...editingGame,
                genre: selectedGenres.join(', ')
            };
            await updateGameAsAdmin(editingGame.id, updatedGame);
            setEditingGame(null);
            setSelectedGenres([]);
            fetchGames();
        } catch (err) {
            setError('Failed to update game');
            console.error(err);
        }
    };

    const handleGenreChange = (genre: string) => {
        if (selectedGenres.includes(genre)) {
            setSelectedGenres(selectedGenres.filter(g => g !== genre));
        } else {
            setSelectedGenres([...selectedGenres, genre]);
        }
    };

    const handleTabChange = (tab: 'transactions' | 'users' | 'games') => {
        setActiveTab(tab);
        if (tab === 'transactions') fetchTransactions();
        if (tab === 'users') fetchUsers();
        if (tab === 'games') fetchGames();
    };

    return (
        <div className="min-h-screen bg-gray-100 p-8">
            <div className="max-w-7xl mx-auto">
                <h1 className="text-4xl font-bold mb-8 text-gray-800">Admin Panel</h1>

                {error && <div className="mb-4 p-4 bg-red-100 text-red-700 rounded">{error}</div>}

                {/* Tab Navigation */}
                <div className="mb-8 flex space-x-4 border-b">
                    <button
                        onClick={() => handleTabChange('transactions')}
                        className={`py-2 px-4 font-semibold ${
                            activeTab === 'transactions'
                                ? 'border-b-2 border-blue-500 text-blue-500'
                                : 'text-gray-600 hover:text-gray-800'
                        }`}
                    >
                        All Transactions
                    </button>
                    <button
                        onClick={() => handleTabChange('users')}
                        className={`py-2 px-4 font-semibold ${
                            activeTab === 'users'
                                ? 'border-b-2 border-blue-500 text-blue-500'
                                : 'text-gray-600 hover:text-gray-800'
                        }`}
                    >
                        Users
                    </button>
                    <button
                        onClick={() => handleTabChange('games')}
                        className={`py-2 px-4 font-semibold ${
                            activeTab === 'games'
                                ? 'border-b-2 border-blue-500 text-blue-500'
                                : 'text-gray-600 hover:text-gray-800'
                        }`}
                    >
                        Games
                    </button>
                </div>

                {loading ? (
                    <div className="text-center py-8">Loading...</div>
                ) : (
                    <>
                        {/* Transactions Tab */}
                        {activeTab === 'transactions' && (
                            <div className="bg-white rounded shadow-md overflow-hidden">
                                <table className="w-full">
                                    <thead className="bg-gray-200">
                                        <tr>
                                            <th className="p-3 text-left">Buyer</th>
                                            <th className="p-3 text-left">Seller</th>
                                            <th className="p-3 text-left">Game</th>
                                            <th className="p-3 text-left">Amount</th>
                                            <th className="p-3 text-left">Date</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {transactions.map((transaction) => (
                                            <tr key={transaction.id} className="border-t hover:bg-gray-50">
                                                <td className="p-3">{transaction.buyerUsername}</td>
                                                <td className="p-3">{transaction.sellerUsername}</td>
                                                <td className="p-3">{transaction.gameTitle}</td>
                                                <td className="p-3">${transaction.amount.toFixed(2)}</td>
                                                <td className="p-3">
                                                    {new Date(transaction.transactionDate).toLocaleDateString()}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        )}

                        {/* Users Tab */}
                        {activeTab === 'users' && (
                            <div className="bg-white rounded shadow-md overflow-hidden">
                                <table className="w-full">
                                    <thead className="bg-gray-200">
                                        <tr>
                                            <th className="p-3 text-left">Username</th>
                                            <th className="p-3 text-left">Email</th>
                                            <th className="p-3 text-left">Balance</th>
                                            <th className="p-3 text-left">Role</th>
                                            <th className="p-3 text-left">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {users.map((user) => (
                                            <tr key={user.id} className="border-t hover:bg-gray-50">
                                                <td className="p-3">{user.username}</td>
                                                <td className="p-3">{user.email}</td>
                                                <td className="p-3">${user.balance.toFixed(2)}</td>
                                                <td className="p-3">
                                                    <span
                                                        className={`px-2 py-1 rounded text-sm font-semibold ${
                                                            user.role === 'Admin'
                                                                ? 'bg-red-100 text-red-700'
                                                                : 'bg-blue-100 text-blue-700'
                                                        }`}
                                                    >
                                                        {user.role}
                                                    </span>
                                                </td>
                                                <td className="p-3 space-x-2">
                                                    {user.id !== currentUser.id && (
                                                        <>
                                                            {user.role === 'User' ? (
                                                                <button
                                                                    onClick={() => handlePromoteUser(user.id)}
                                                                    className="bg-green-500 text-white px-3 py-1 rounded hover:bg-green-600"
                                                                >
                                                                    Promote
                                                                </button>
                                                            ) : (
                                                                <button
                                                                    onClick={() => handleDemoteUser(user.id)}
                                                                    className="bg-yellow-500 text-white px-3 py-1 rounded hover:bg-yellow-600"
                                                                >
                                                                    Demote
                                                                </button>
                                                            )}
                                                        </>
                                                    )}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        )}

                        {/* Games Tab */}
                        {activeTab === 'games' && (
                            <div>
                                {editingGame ? (
                                    <div className="bg-white rounded shadow-md p-6 mb-6">
                                        <h3 className="text-2xl font-bold mb-4">Edit Game</h3>
                                        <div className="space-y-4">
                                            <div>
                                                <label className="block text-gray-700 font-semibold mb-2">
                                                    Name
                                                </label>
                                                <input
                                                    type="text"
                                                    value={editingGame.name}
                                                    onChange={(e) =>
                                                        setEditingGame({
                                                            ...editingGame,
                                                            name: e.target.value,
                                                        })
                                                    }
                                                    className="w-full px-3 py-2 border rounded"
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-gray-700 font-semibold mb-2">
                                                    Description
                                                </label>
                                                <textarea
                                                    value={editingGame.description}
                                                    onChange={(e) =>
                                                        setEditingGame({
                                                            ...editingGame,
                                                            description: e.target.value,
                                                        })
                                                    }
                                                    className="w-full px-3 py-2 border rounded"
                                                    rows={4}
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-gray-700 font-semibold mb-2">
                                                    Price
                                                </label>
                                                <input
                                                    type="number"
                                                    value={editingGame.price}
                                                    onChange={(e) =>
                                                        setEditingGame({
                                                            ...editingGame,
                                                            price: parseFloat(e.target.value),
                                                        })
                                                    }
                                                    className="w-full px-3 py-2 border rounded"
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-gray-700 font-semibold mb-2">
                                                    Genre
                                                </label>
                                                <div className="space-y-2">
                                                    {GENRES.map((g) => (
                                                        <label key={g} className="flex items-center">
                                                            <input
                                                                type="checkbox"
                                                                checked={selectedGenres.includes(g)}
                                                                onChange={() => handleGenreChange(g)}
                                                                className="w-4 h-4 text-blue-600 rounded"
                                                            />
                                                            <span className="ml-2 text-gray-700">{g}</span>
                                                        </label>
                                                    ))}
                                                </div>
                                            </div>
                                            <div className="flex space-x-4">
                                                <button
                                                    onClick={handleSaveGame}
                                                    className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600"
                                                >
                                                    Save
                                                </button>
                                                <button
                                                    onClick={() => setEditingGame(null)}
                                                    className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
                                                >
                                                    Cancel
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                ) : (
                                    <div className="grid gap-4">
                                        {games.map((game) => (
                                            <div key={game.id} className="bg-white rounded shadow-md p-4">
                                                <div className="flex justify-between items-start">
                                                    <div className="flex-1">
                                                        <h3 className="text-xl font-bold text-gray-800">
                                                            {game.name}
                                                        </h3>
                                                        <p className="text-gray-600 mt-2">
                                                            {game.description.substring(0, 100)}...
                                                        </p>
                                                        <div className="mt-3 flex space-x-4 text-sm text-gray-500">
                                                            <span>Price: ${game.price}</span>
                                                            <span>Genre: {game.genre}</span>
                                                            <span>Developer: {game.developer}</span>
                                                        </div>
                                                    </div>
                                                    <div className="ml-4 space-y-2">
                                                        <button
                                                            onClick={() => handleUpdateGame(game.id)}
                                                            className="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
                                                        >
                                                            Edit
                                                        </button>
                                                        <button
                                                            onClick={() => handleDeleteGame(game.id)}
                                                            className="w-full bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
                                                        >
                                                            Delete
                                                        </button>
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
};
