import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import API from '../API/api';
import type { Game } from '../Interface/baseInterface';

type FormData = Omit<Game, 'id' | 'releaseDate' | 'tags'> & {
    releaseDate: string;
    tags: string;
};

export const EditGame = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [formData, setFormData] = useState<FormData>({
        name: '',
        description: '',
        price: 0,
        genre: '',
        developer: '',
        releaseDate: '',
        tags: ''
    });

    useEffect(() => {
        const fetchGame = async () => {
            if (!id) return;
            try {
                const { data } = await API.get<Game>(`/Game/${id}`);
                setFormData({
                    ...data,
                    releaseDate: new Date(data.releaseDate).toISOString().split('T')[0],
                    tags: data.tags.join(', ')
                });
            } catch (error) {
                console.error('Error fetching game:', error);
            }
        };
        fetchGame();
    }, [id]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!id) return;
        try {
            const gameData = {
                ...formData,
                price: Number(formData.price),
                releaseDate: new Date(formData.releaseDate),
                tags: formData.tags.split(',').map(tag => tag.trim()).filter(tag => tag)
            };
            await API.put(`/Game/${id}`, gameData);
            navigate(`/game/${id}`);
        } catch (error) {
            console.error('Error updating game:', error);
        }
    };

    return (
        <div className="max-w-2xl mx-auto px-4 py-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-6">Edit Game</h2>
            <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-lg p-6 space-y-4">
                <div>
                    <label>Name</label>
                    <input name="name" type="text" value={formData.name} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label>Description</label>
                    <textarea name="description" value={formData.description} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" rows={4} required />
                </div>
                <div>
                    <label>Price</label>
                    <input name="price" type="number" value={formData.price} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label>Genre</label>
                    <input name="genre" type="text" value={formData.genre} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label>Developer</label>
                    <input name="developer" type="text" value={formData.developer} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label>Release Date</label>
                    <input name="releaseDate" type="date" value={formData.releaseDate} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label>Tags (comma-separated)</label>
                    <input name="tags" type="text" value={formData.tags} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" />
                </div>
                <div className="flex space-x-4">
                    <button type="submit" className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-md">Update Game</button>
                    <button type="button" onClick={() => navigate(`/game/${id}`)} className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-md">Cancel</button>
                </div>
            </form>
        </div>
    );
};
