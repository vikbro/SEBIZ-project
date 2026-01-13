import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import API from '../API/api';
import type { Game } from '../Interface/baseInterface';

type FormData = Omit<Game, 'id' | 'releaseDate' | 'tags' | 'createdById'> & {
    releaseDate: string;
    tags: string;
    imagePath?: string;

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
        tags: '',
        imagePath: ''
    });
    const [imageFile, setImageFile] = useState<File | null>(null);
    const [gameFile, setGameFile] = useState<File | null>(null);

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setImageFile(e.target.files[0]);
        }
    };

    const handleGameFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setGameFile(e.target.files[0]);
        }
    };

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
            let imagePath = formData.imagePath || '';
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

            if (gameFile) {
                const formData = new FormData();
                formData.append('file', gameFile);
                await API.post(`/GameUpload/upload/${id}`, formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                });
            }

            const gameData = {
                ...formData,
                price: Number(formData.price),
                releaseDate: new Date(formData.releaseDate),
                tags: formData.tags.split(',').map(tag => tag.trim()).filter(tag => tag),
                imagePath: imagePath
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
                    <label htmlFor="name">Name</label>
                    <input id="name" name="name" type="text" value={formData.name} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label htmlFor="description">Description</label>
                    <textarea id="description" name="description" value={formData.description} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" rows={4} required />
                </div>
                <div>
                    <label htmlFor="price">Price</label>
                    <input id="price" name="price" type="number" value={formData.price} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label htmlFor="genre">Genre</label>
                    <input id="genre" name="genre" type="text" value={formData.genre} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label htmlFor="developer">Developer</label>
                    <input id="developer" name="developer" type="text" value={formData.developer} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label htmlFor="releaseDate">Release Date</label>
                    <input id="releaseDate" name="releaseDate" type="date" value={formData.releaseDate} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" required />
                </div>
                <div>
                    <label htmlFor="tags">Tags (comma-separated)</label>
                    <input id="tags" name="tags" type="text" value={formData.tags} onChange={handleChange} className="w-full px-3 py-2 border rounded-md" />
                </div>
                <div>
                    <label htmlFor="image">Image</label>
                    <input id="image" type="file" onChange={handleImageChange} className="w-full px-3 py-2 border rounded-md" />
                </div>
                <div>
                    <label htmlFor="gameFile">Game File (.zip)</label>
                    <input id="gameFile" type="file" onChange={handleGameFileChange} className="w-full px-3 py-2 border rounded-md" accept=".zip" />
                </div>
                <div className="flex space-x-4">
                    <button type="submit" className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-md">Update Game</button>
                    <button type="button" onClick={() => navigate(`/game/${id}`)} className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-md">Cancel</button>
                </div>
            </form>
        </div>
    );
};
