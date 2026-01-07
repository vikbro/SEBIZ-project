import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import API from '../API/api';
import type { Game } from '../Interface/baseInterface';

type FormData = Omit<Game, 'id' | 'releaseDate' | 'tags'> & {
    releaseDate: string;
    tags: string;
    imageFile?: File | null;
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
        imageUrl: '',
        imageFile: null,
    });
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);

    useEffect(() => {
        const fetchGame = async () => {
            if (!id) return;
            try {
                const { data } = await API.get<Game>(`/Game/${id}`);
                setFormData({
                    ...data,
                    releaseDate: new Date(data.releaseDate).toISOString().split('T')[0],
                    tags: data.tags.join(', '),
                    imageFile: null,
                });
                if (data.imageUrl) {
                    setPreviewUrl(data.imageUrl);
                }
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

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            const file = e.target.files[0];
            setFormData(prev => ({ ...prev, imageFile: file }));
            setPreviewUrl(URL.createObjectURL(file));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!id) return;
        try {
            const postData = new FormData();
            postData.append('name', formData.name);
            postData.append('description', formData.description);
            postData.append('price', String(formData.price));
            postData.append('genre', formData.genre);
            postData.append('developer', formData.developer);
            postData.append('releaseDate', formData.releaseDate);
            postData.append('tags', formData.tags);
            if (formData.imageFile) {
                postData.append('imageFile', formData.imageFile);
            }

            await API.put(`/Game/${id}`, postData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });
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
                <div>
                    <label>Image</label>
                    <input type="file" onChange={handleFileChange} className="w-full" />
                    {previewUrl && <img src={previewUrl.startsWith('blob:') ? previewUrl : `http://localhost:5202${previewUrl}`} alt="Game preview" className="mt-4 w-full h-auto" />}
                </div>
                <div className="flex space-x-4">
                    <button type="submit" className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-md">Update Game</button>
                    <button type="button" onClick={() => navigate(`/game/${id}`)} className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-md">Cancel</button>
                </div>
            </form>
        </div>
    );
};
