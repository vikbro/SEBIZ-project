import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import API from '../API/api';

export const CreateGame = () => {
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        price: '',
        genre: '',
        developer: '',
        releaseDate: '',
        coverImageUrl: ''
    });
    const [selectedTags, setSelectedTags] = useState<string[]>([]);

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (!token) {
            navigate('/login');
        }
    }, [navigate]);

    const handleTagChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const options = e.target.options;
        const value: string[] = [];
        for (let i = 0, l = options.length; i < l; i++) {
            if (options[i].selected) {
                value.push(options[i].value);
            }
        }
        setSelectedTags(value);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const gameData = {
                ...formData,
                price: Number(formData.price),
                releaseDate: new Date(formData.releaseDate),
                tags: selectedTags
            };
            await API.post('/Game', gameData);
            navigate('/');
        } catch (error) {
            console.error('Error creating game:', error);
        }
    };

    return (
        <div className="max-w-2xl mx-auto px-4 py-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-6">Add a New Game</h2>
            <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-lg p-6 space-y-4">
                {/* Name */}
                <div>
                    <label className="block text-gray-700 mb-2">Name</label>
                    <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Description */}
                <div>
                    <label className="block text-gray-700 mb-2">Description</label>
                    <textarea value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} className="w-full px-3 py-2 border rounded-md" rows={4} required />
                </div>
                {/* Price */}
                <div>
                    <label className="block text-gray-700 mb-2">Price</label>
                    <input type="number" value={formData.price} onChange={(e) => setFormData({ ...formData, price: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Genre */}
                <div>
                    <label className="block text-gray-700 mb-2">Genre</label>
                    <input type="text" value={formData.genre} onChange={(e) => setFormData({ ...formData, genre: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Developer */}
                <div>
                    <label className="block text-gray-700 mb-2">Developer</label>
                    <input type="text" value={formData.developer} onChange={(e) => setFormData({ ...formData, developer: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Release Date */}
                <div>
                    <label className="block text-gray-700 mb-2">Release Date</label>
                    <input type="date" value={formData.releaseDate} onChange={(e) => setFormData({ ...formData, releaseDate: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Cover Image URL */}
                <div>
                    <label className="block text-gray-700 mb-2">Cover Image URL</label>
                    <input type="text" value={formData.coverImageUrl} onChange={(e) => setFormData({ ...formData, coverImageUrl: e.target.value })} className="w-full px-3 py-2 border rounded-md" />
                </div>
                {/* Tags */}
                <div>
                    <label className="block text-gray-700 mb-2">Tags</label>
                    <select multiple value={selectedTags} onChange={handleTagChange} className="w-full px-3 py-2 border rounded-md">
                        <option value="Action">Action</option>
                        <option value="Adventure">Adventure</option>
                        <option value="RPG">RPG</option>
                        <option value="Strategy">Strategy</option>
                        <option value="Simulation">Simulation</option>
                        <option value="Sports">Sports</option>
                        <option value="Racing">Racing</option>
                    </select>
                </div>
                {/* Buttons */}
                <div className="flex space-x-4">
                    <button type="submit" className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-md">Create Game</button>
                    <button type="button" onClick={() => navigate('/')} className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-md">Cancel</button>
                </div>
            </form>
        </div>
    );
};
