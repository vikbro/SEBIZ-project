import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import API from '../API/api';
import { useCart } from '../Context/CartContext';
import { useTheme } from '../Context/ThemeContext';

const Cart: React.FC = () => {
    const { items, removeFromCart, clearCart, total } = useCart();
    const { theme } = useTheme();
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');

    const handlePurchaseAll = async () => {
        const rawUser = localStorage.getItem('user');
        if (!rawUser) {
            setMessage('You need to be logged in to purchase items.');
            return;
        }

        const existingUser = JSON.parse(rawUser);
        const token = existingUser?.token;
        if (!token) {
            setMessage('Authentication token not found. Please log in again.');
            return;
        }

        setLoading(true);
        setMessage('');

        // Verify the user's current balance before attempting purchases
        try {
            const { data: me } = await API.get('/User/me');
            const currentBalance = me?.balance ?? 0;
            if (currentBalance < total) {
                const need = (total - currentBalance).toFixed(2);
                setMessage(`Insufficient balance. You need $${need} more to purchase all items.`);
                setLoading(false);
                return;
            }
        } catch (err) {
            console.error('Failed to verify user balance before purchase', err);
            setMessage('Failed to verify balance. Please try again.');
            setLoading(false);
            return;
        }
        const failed: string[] = [];
        try {
            for (const game of items) {
                try {
                    const res = await API.post(`/User/purchase/${game.id}`);
                    // API returns 204 NoContent on success; axios maps it to status 204
                    if (res.status !== 204 && res.status !== 200) {
                        failed.push(game.name || game.id);
                    }
                } catch (err: any) {
                    console.error('Failed to purchase', game.id, err?.response?.data || err.message || err);
                    failed.push(game.name || game.id);
                }
            }

            // Refresh user data but preserve the token
            try {
                const { data } = await API.get('/User/me');
                const merged = { ...data, token };
                localStorage.setItem('user', JSON.stringify(merged));
            } catch (err) {
                console.error('Failed to refresh user data', err);
            }

            if (failed.length === 0) {
                clearCart();
                setMessage('Purchase completed.');
                navigate('/my-library');
            } else {
                setMessage(`Purchase completed with failures for: ${failed.join(', ')}`);
            }
        } catch (err) {
            setMessage('Failed to complete purchase.');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    if (items.length === 0) {
        return (
            <div className="container mx-auto px-4 py-8">
                <div className={`rounded-lg p-6 ${theme === 'dark' ? 'bg-gray-800 text-gray-100' : 'bg-white text-gray-900'}`}>
                    <h2 className="text-2xl font-bold mb-4">Your Cart</h2>
                    <p>Your cart is empty. Continue shopping to add games.</p>
                    <button onClick={() => navigate('/')} className="mt-4 px-4 py-2 bg-blue-500 text-white rounded">Continue Shopping</button>
                </div>
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8">
            <div className={`rounded-lg p-6 ${theme === 'dark' ? 'bg-gray-800 text-gray-100' : 'bg-white text-gray-900'}`}>
                <h2 className="text-2xl font-bold mb-4">Your Cart</h2>
                <ul>
                    {items.map((g) => (
                        <li key={g.id} className="flex items-center justify-between border-b py-3">
                            <div className="flex items-center gap-4">
                                <img src={g.imagePath || '/uploads/placeholder.png'} alt={g.name} className="w-16 h-12 object-cover rounded" />
                                <div>
                                    <div className="font-semibold">{g.name}</div>
                                    <div className="text-sm text-gray-500">${g.price?.toFixed(2)}</div>
                                </div>
                            </div>
                            <div className="flex items-center gap-3">
                                <button onClick={() => removeFromCart(g.id)} className="px-3 py-1 bg-red-500 text-white rounded">Remove</button>
                                <button onClick={() => navigate(`/game/${g.id}`)} className="px-3 py-1 bg-blue-500 text-white rounded">View</button>
                            </div>
                        </li>
                    ))}
                </ul>

                <div className="mt-6 flex justify-between items-center">
                    <div className="text-xl font-bold">Total: ${total.toFixed(2)}</div>
                    <div className="flex items-center gap-3">
                        <button onClick={() => navigate('/')} className="px-4 py-2 bg-gray-400 text-white rounded">Continue Shopping</button>
                        <button onClick={handlePurchaseAll} disabled={loading} className="px-4 py-2 bg-green-600 text-white rounded">{loading ? 'Processing...' : 'Purchase All'}</button>
                    </div>
                </div>
                {message && <p className="mt-4 text-green-600">{message}</p>}
            </div>
        </div>
    );
};

export default Cart;
