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
        imageFile: null as File | null,
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const postData = new FormData();
            postData.append('name', formData.name);
            postData.append('description', formData.description);
            postData.append('price', formData.price);
            postData.append('genre', formData.genre);
            postData.append('developer', formData.developer);
            postData.append('releaseDate', formData.releaseDate);
            postData.append('tags', formData.tags);
            if (formData.imageFile) {
                postData.append('imageFile', formData.imageFile);
            }

            await API.post('/Game', postData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });
            navigate('/');
        } catch (error) {
            console.error('Error creating game:', error);
        }
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setFormData({ ...formData, imageFile: e.target.files[0] });
        }
    };

    return (
        <div className="max-w-2xl mx-auto px-4 py-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-6">Add a New Game</h2>
            <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-lg p-6 space-y-4">
                {/* Name */}
                <div>
                    <label htmlFor="name" className="block text-gray-700 mb-2">Name</label>
                    <input id="name" name="name" type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Description */}
                <div>
                    <label htmlFor="description" className="block text-gray-700 mb-2">Description</label>
                    <textarea id="description" name="description" value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} className="w-full px-3 py-2 border rounded-md" rows={4} required />
                </div>
                {/* Price */}
                <div>
                    <label htmlFor="price" className="block text-gray-700 mb-2">Price</label>
                    <input id="price" name="price" type="number" value={formData.price} onChange={(e) => setFormData({ ...formData, price: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Genre */}
                <div>
                    <label htmlFor="genre" className="block text-gray-700 mb-2">Genre</label>
                    <input id="genre" name="genre" type="text" value={formData.genre} onChange={(e) => setFormData({ ...formData, genre: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Developer */}
                <div>
                    <label htmlFor="developer" className="block text-gray-700 mb-2">Developer</label>
                    <input id="developer" name="developer" type="text" value={formData.developer} onChange={(e) => setFormData({ ...formData, developer: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Release Date */}
                <div>
                    <label htmlFor="releaseDate" className="block text-gray-700 mb-2">Release Date</label>
                    <input id="releaseDate" name="releaseDate" type="date" value={formData.releaseDate} onChange={(e) => setFormData({ ...formData, releaseDate: e.target.value })} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                {/* Tags */}
                <div>
                    <label htmlFor="tags" className="block text-gray-700 mb-2">Tags (comma-separated)</label>
                    <input id="tags" name="tags" type="text" value={formData.tags} onChange={(e) => setFormData({ ...formData, tags: e.target.value })} className="w-full px-3 py-2 border rounded-md" />
                </div>
                {/* Image */}
                <div>
                    <label htmlFor="imageFile" className="block text-gray-700 mb-2">Image</label>
                    <input id="imageFile" name="imageFile" type="file" onChange={handleFileChange} className="w-full" />
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
