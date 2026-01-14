import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import API from '../API/api';
import { uploadGameFile } from '../API/gameFileApi';

const GENRES = ['Action', 'Adventure', 'RPG', 'Shooters', 'Strategy', 'Simulation', 'Sports', 'Puzzle'];

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

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (selectedGenres.length === 0) {
            alert('Please select at least one genre');
            return;
        }
        if (!gameFile) {
            alert('Please upload a game file (.zip)');
            return;
        }
        
        setIsLoading(true);
        try {
            let imagePath: string | null = null;
            let imageFileName: string | null = null;
            if (imageFile) {
                const imageData = new FormData();
                imageData.append('file', imageFile);
                const response = await API.post('/Image/upload', imageData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                });
                imagePath = response.data.imagePath;
                imageFileName = response.data.fileName;
            }

            // Upload game file
            let gameFilePath: string | null = null;
            let gameFileName: string | null = null;
            if (gameFile) {
                const fileResponse = await uploadGameFile(gameFile);
                gameFilePath = fileResponse.gameFilePath;
                gameFileName = fileResponse.fileName;
            }

            const gameData = {
                name: formData.name,
                description: formData.description,
                price: Number(formData.price),
                genre: selectedGenres.join(', '),
                developer: formData.developer,
                releaseDate: new Date(formData.releaseDate),
                tags: formData.tags.split(',').map(tag => tag.trim()).filter(tag => tag !== ''),
                imagePath: imagePath,
                fileName: imageFileName,
                gameFilePath: gameFilePath,
                gameFileName: gameFileName
            };
            const response = await API.post('/Game', gameData);
            const newGame = response.data;
            navigate(`/game/${newGame.id}`);
        } catch (error) {
            console.error('Error creating game:', error);
            alert('Error creating game. Please try again.');
        } finally {
            setIsLoading(false);
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
                {/* Genres */}
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
                    <input id="image" type="file" onChange={handleImageChange} className="w-full px-3 py-2 border rounded-md" accept="image/*" />
                </div>
                {/* Game File */}
                <div>
                    <label htmlFor="gameFile" className="block text-gray-700 mb-2 font-semibold">Game File (*.zip) *</label>
                    <input id="gameFile" type="file" onChange={handleGameFileChange} className="w-full px-3 py-2 border rounded-md border-blue-500" accept=".zip" required />
                    {gameFile && (
                        <p className="text-sm text-gray-600 mt-2">Selected: {gameFile.name}</p>
                    )}
                </div>
                {/* Buttons */}
                <div className="flex space-x-4">
                    <button type="submit" disabled={isLoading} className="bg-blue-500 hover:bg-blue-600 disabled:bg-gray-400 text-white px-4 py-2 rounded-md">
                        {isLoading ? 'Creating...' : 'Create Game'}
                    </button>
                    <button type="button" onClick={() => navigate('/')} disabled={isLoading} className="bg-gray-500 hover:bg-gray-600 disabled:bg-gray-400 text-white px-4 py-2 rounded-md">Cancel</button>
                </div>
            </form>
        </div>
    );
};
