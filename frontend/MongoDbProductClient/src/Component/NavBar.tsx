import { Link, useNavigate } from 'react-router-dom';
import { useTheme } from '../Context/ThemeContext';

const Navbar = () => {
    const navigate = useNavigate();
    const { theme, toggleTheme } = useTheme();
    const user = localStorage.getItem('user');
    const userData = user ? JSON.parse(user) : null;

    const handleLogout = () => {
        localStorage.removeItem('user');
        navigate('/');
        // Consider a page reload or state update to refresh the navbar
        window.location.reload();
    };

    return (
        <nav className={`${
            theme === 'dark'
                ? 'bg-gray-900 border-b-2 border-blue-900 text-gray-100'
                : 'bg-blue-600 text-white'
        } p-4 shadow-md transition-colors duration-200`}>
            <div className="container mx-auto flex justify-between items-center">
                <Link to="/" className={`text-2xl font-bold ${
                    theme === 'dark'
                        ? 'text-blue-300 hover:text-blue-200'
                        : 'hover:text-blue-200'
                }`}>
                    GameStore
                </Link>
                <div className="flex items-center space-x-4">
                    <Link to="/" className={theme === 'dark' ? 'text-gray-200 hover:text-blue-300' : 'hover:text-blue-200'}>Home</Link>
                    <Link to="/create-game" className={theme === 'dark' ? 'text-gray-200 hover:text-blue-300' : 'hover:text-blue-200'}>Add Game</Link>

                    {user ? (
                        <>
                            <Link to="/my-library" className={theme === 'dark' ? 'text-gray-200 hover:text-blue-300' : 'hover:text-blue-200'}>My Library</Link>
                            <Link to="/user-info" className={theme === 'dark' ? 'text-gray-200 hover:text-blue-300' : 'hover:text-blue-200'}>User Info</Link>
                            {userData && userData.role === 'Admin' && (
                                <Link to="/admin" className={`${
                                    theme === 'dark'
                                        ? 'bg-yellow-600 hover:bg-yellow-700 text-gray-900'
                                        : 'bg-yellow-500 hover:bg-yellow-600'
                                } px-3 py-1 rounded font-semibold transition-colors`}>
                                    Admin Panel
                                </Link>
                            )}
                            <button onClick={handleLogout} className={`${
                                theme === 'dark'
                                    ? 'bg-red-700 hover:bg-red-600 text-gray-100'
                                    : 'bg-red-500 hover:bg-red-600 text-white'
                            } px-3 py-1 rounded transition-colors`}>
                                Logout
                            </button>
                        </>
                    ) : (
                        <>
                            <Link to="/login" className={theme === 'dark' ? 'text-gray-200 hover:text-blue-300' : 'hover:text-blue-200'}>Login</Link>
                            <Link to="/register" className={theme === 'dark' ? 'text-gray-200 hover:text-blue-300' : 'hover:text-blue-200'}>Register</Link>
                        </>
                    )}
                    
                    {/* Theme Toggle Button */}
                    <button
                        onClick={toggleTheme}
                        className={`ml-4 p-2 rounded-lg transition-all duration-200 ${
                            theme === 'dark'
                                ? 'bg-blue-900 hover:bg-blue-800 text-yellow-300'
                                : 'bg-blue-700 hover:bg-blue-800 text-yellow-200'
                        }`}
                        title={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}
                        aria-label="Toggle theme"
                    >
                        {theme === 'dark' ? (
                            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                {/* Sun icon */}
                                <path fillRule="evenodd" d="M10 2a1 1 0 011 1v2a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l-2.12-2.12a1 1 0 10-1.414 1.414l2.12 2.12a1 1 0 001.414-1.414zM2.05 6.464l2.12 2.12a1 1 0 101.414-1.414L3.464 5.05a1 1 0 10-1.414 1.414z" clipRule="evenodd" />
                            </svg>
                        ) : (
                            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                {/* Moon icon */}
                                <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z" />
                            </svg>
                        )}
                    </button>
                </div>
            </div>
        </nav>
    );
};

export default Navbar;
