import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import API from '../API/api';

export const Register = () => {
    const navigate = useNavigate();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await API.post('/User/register', { username, password });
            navigate('/login');
        } catch (err) {
            setError('Registration failed. Please try again.');
            console.error(err);
        }
    };

    return (
        <div className="flex items-center justify-center h-screen">
            <form onSubmit={handleSubmit} className="p-8 bg-white rounded shadow-md w-80">
                <h2 className="text-2xl font-bold mb-6">Register</h2>
                {error && <p className="text-red-500 mb-4">{error}</p>}
                <div className="mb-4">
                    <label htmlFor="username" className="block text-gray-700">Username</label>
                    <input
                        id="username"
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        className="w-full px-3 py-2 border rounded"
                        required
                        autoComplete="username"
                    />
                </div>
                <div className="mb-6">
                    <label htmlFor="password"className="block text-gray-700">Password</label>
                    <input
                        id="password"
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="w-full px-3 py-2 border rounded"
                        required
                        autoComplete="new-password"
                    />
                </div>
                <button type="submit" className="w-full bg-blue-500 text-white py-2 rounded">
                    Register
                </button>
            </form>
        </div>
    );
};
