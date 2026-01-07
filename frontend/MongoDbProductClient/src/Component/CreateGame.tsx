import { useState } from 'react';
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
        tags: '',
        imagePath: ''
    });
    const [imageFile, setImageFile] = useState<File | null>(null);

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setImageFile(e.target.files[0]);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            let imagePath = '';
            if (imageFile) {
                const formData = new FormData();
                formData.append('file', imageFile);
                const response = await API.post('/Image/upload', formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                });
                imagePath = response.data.imagePath;
            }

            const gameData = {
                ...formData,
                price: Number(formData.price),
                releaseDate: new Date(formData.releaseDate),
                tags: formData.tags.split(',').map(tag => tag.trim()),
                imagePath: imagePath
            };
            const response = await API.post('/Game', gameData);
            const newGame = response.data;
            navigate(`/game/${newGame.id}`);
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
                    <label htmlFor="name" className="block text-gray-700 mb-2">Name</label>
                    <input id="name" type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Description */}
                <div>
                    <label htmlFor="description" className="block text-gray-700 mb-2">Description</label>
                    <textarea id="description" value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} className="w-full px-3 py-2 border rounded-md" rows={4} required />
                </div>
                {/* Price */}
                <div>
                    <label htmlFor="price" className="block text-gray-700 mb-2">Price</label>
                    <input id="price" type="number" value={formData.price} onChange={(e) => setFormData({ ...formData, price: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Genre */}
                <div>
                    <label htmlFor="genre" className="block text-gray-700 mb-2">Genre</label>
                    <input id="genre" type="text" value={formData.genre} onChange={(e) => setFormData({ ...formData, genre: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Developer */}
                <div>
                    <label htmlFor="developer" className="block text-gray-700 mb-2">Developer</label>
                    <input id="developer" type="text" value={formData.developer} onChange={(e) => setFormData({ ...formData, developer: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Release Date */}
                <div>
                    <label htmlFor="releaseDate" className="block text-gray-700 mb-2">Release Date</label>
                    <input id="releaseDate" type="date" value={formData.releaseDate} onChange={(e) => setFormData({ ...formData, releaseDate: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Tags */}
                <div>
                    <label htmlFor="tags" className="block text-gray-700 mb-2">Tags (comma-separated)</label>
                    <input id="tags" type="text" value={formData.tags} onChange={(e) => setFormData({ ...formData, tags: e.target.value })} className="w-full px-3 py-2 border rounded-md" />
                </div>
                {/* Image */}
                <div>
                    <label htmlFor="image" className="block text-gray-700 mb-2">Image</label>
                    <input id="image" type="file" onChange={handleImageChange} className="w-full px-3 py-2 border rounded-md" />
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
