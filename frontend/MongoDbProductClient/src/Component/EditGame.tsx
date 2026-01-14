import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import API from '../API/api';
import { uploadGameFile, deleteGameFile } from '../API/gameFileApi';
import type { Game } from '../Interface/baseInterface';

const GENRES = ['Action', 'Adventure', 'RPG', 'Shooters', 'Strategy', 'Simulation', 'Sports', 'Puzzle'];

type FormData = Omit<Game, 'id' | 'releaseDate' | 'tags' | 'createdById'> & {
    releaseDate: string;
    tags: string;
    imagePath?: string;
    gameFilePath?: string;
    gameFileName?: string;
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
        imagePath: '',
        gameFilePath: '',
        gameFileName: ''
    });
    const [selectedGenres, setSelectedGenres] = useState<string[]>([]);
    const [imageFile, setImageFile] = useState<File | null>(null);
    const [gameFile, setGameFile] = useState<File | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setImageFile(e.target.files[0]);
        }
    };

    const handleGameFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const file = e.target.files[0];
            if (file.type !== 'application/zip' && !file.name.endsWith('.zip')) {
                alert('Please select a .zip file');
                return;
            }
            setGameFile(file);
        }
    };

    const handleGenreChange = (genre: string) => {
        if (selectedGenres.includes(genre)) {
            setSelectedGenres(selectedGenres.filter(g => g !== genre));
        } else {
            setSelectedGenres([...selectedGenres, genre]);
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
                // Parse genres from the genre field
                const genres = data.genre.split(',').map(g => g.trim()).filter(g => g);
                setSelectedGenres(genres);
            } catch (error) {
                console.error('Error fetching game:', error);
            }
        };
        fetchGame();
    }, [id]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!id) return;
        if (selectedGenres.length === 0) {
            alert('Please select at least one genre');
            return;
        }
        
        setIsLoading(true);
        try {
            let imagePath = formData.imagePath || '';
            let fileName = formData.fileName;
            if (imageFile) {
                const formDataImg = new FormData();
                formDataImg.append('file', imageFile);
                const response = await API.post('/Image/upload', formDataImg, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                });
                imagePath = response.data.imagePath;
                fileName = response.data.fileName;
            }

            // Handle game file upload/replacement
            let gameFilePath = formData.gameFilePath;
            let gameFileName = formData.gameFileName;
            if (gameFile) {
                const fileResponse = await uploadGameFile(gameFile);
                gameFilePath = fileResponse.gameFilePath;
                gameFileName = fileResponse.fileName;
                
                // Delete old game file if it exists and is different
                if (formData.gameFileName && formData.gameFileName !== gameFileName) {
                    try {
                        await deleteGameFile(formData.gameFileName);
                    } catch (error) {
                        console.error('Error deleting old game file:', error);
                    }
                }
            }

            const gameData = {
                name: formData.name,
                description: formData.description,
                price: Number(formData.price),
                genre: selectedGenres.join(', '),
                developer: formData.developer,
                releaseDate: new Date(formData.releaseDate),
                tags: formData.tags.split(',').map(tag => tag.trim()).filter(tag => tag),
                imagePath: imagePath,
                fileName: fileName,
                gameFilePath: gameFilePath,
                gameFileName: gameFileName
            };
            
            await API.put(`/Game/${id}`, gameData);
            navigate(`/game/${id}`);
        } catch (error) {
            console.error('Error updating game:', error);
            alert('Error updating game. Please try again.');
        } finally {
            setIsLoading(false);
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
                    <label className="block text-gray-700 mb-2 font-semibold">Genres (Select Multiple)</label>
                    <div className="grid grid-cols-2 gap-3">
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
                    <input id="image" type="file" onChange={handleImageChange} className="w-full px-3 py-2 border rounded-md" accept="image/*" />
                </div>
                <div>
                    <label htmlFor="gameFile" className="block text-gray-700 mb-2">Game File (*.zip) - Optional</label>
                    <input id="gameFile" type="file" onChange={handleGameFileChange} className="w-full px-3 py-2 border rounded-md" accept=".zip" />
                    {gameFile && (
                        <p className="text-sm text-gray-600 mt-2">New file selected: {gameFile.name}</p>
                    )}
                    {formData.gameFileName && !gameFile && (
                        <p className="text-sm text-gray-500 mt-2">Current file: {formData.gameFileName}</p>
                    )}
                </div>
                <div className="flex space-x-4">
                    <button type="submit" disabled={isLoading} className="bg-blue-500 hover:bg-blue-600 disabled:bg-gray-400 text-white px-4 py-2 rounded-md">
                        {isLoading ? 'Updating...' : 'Update Game'}
                    </button>
                    <button type="button" onClick={() => navigate(`/game/${id}`)} disabled={isLoading} className="bg-gray-500 hover:bg-gray-600 disabled:bg-gray-400 text-white px-4 py-2 rounded-md">Cancel</button>
                </div>
            </form>
        </div>
    );
};
