import { Link, useNavigate } from 'react-router-dom';

const Navbar = () => {
    const navigate = useNavigate();
    const user = localStorage.getItem('user');
    const userData = user ? JSON.parse(user) : null;

    const handleLogout = () => {
        localStorage.removeItem('user');
        navigate('/');
        // Consider a page reload or state update to refresh the navbar
        window.location.reload();
    };

    return (
        <nav className="bg-blue-600 text-white p-4 shadow-md">
            <div className="container mx-auto flex justify-between items-center">
                <Link to="/" className="text-2xl font-bold hover:text-blue-200">
                    GameStore
                </Link>
                <div className="flex items-center space-x-4">
                    <Link to="/" className="hover:text-blue-200">Home</Link>
                    <Link to="/create-game" className="hover:text-blue-200">Add Game</Link>

                    {user ? (
                        <>
                            <Link to="/my-library" className="hover:text-blue-200">My Library</Link>
                            <Link to="/user-info" className="hover:text-blue-200">User Info</Link>
                            {userData && userData.role === 'Admin' && (
                                <Link to="/admin" className="bg-yellow-500 hover:bg-yellow-600 px-3 py-1 rounded font-semibold">
                                    Admin Panel
                                </Link>
                            )}
                            <button onClick={handleLogout} className="bg-red-500 hover:bg-red-600 px-3 py-1 rounded">
                                Logout
                            </button>
                        </>
                    ) : (
                        <>
                            <Link to="/login" className="hover:text-blue-200">Login</Link>
                            <Link to="/register" className="hover:text-blue-200">Register</Link>
                        </>
                    )}
                </div>
            </div>
        </nav>
    );
};

export default Navbar;
