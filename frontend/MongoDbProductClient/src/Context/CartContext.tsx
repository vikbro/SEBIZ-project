import React, { createContext, useContext, useEffect, useState } from 'react';
import type { Game } from '../Interface/baseInterface';

type CartItem = Game;

type CartContextType = {
    items: CartItem[];
    addToCart: (game: CartItem) => void;
    removeFromCart: (id: string) => void;
    clearCart: () => void;
    total: number;
    count: number;
};

const CartContext = createContext<CartContextType | undefined>(undefined);

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [items, setItems] = useState<CartItem[]>(() => {
        try {
            const raw = localStorage.getItem('cart');
            return raw ? JSON.parse(raw) : [];
        } catch {
            return [];
        }
    });

    useEffect(() => {
        try {
            localStorage.setItem('cart', JSON.stringify(items));
        } catch (e) {
            // ignore
        }
    }, [items]);

    const addToCart = (game: CartItem) => {
        setItems((prev) => {
            if (prev.find((g) => g.id === game.id)) return prev;
            return [...prev, game];
        });
    };

    const removeFromCart = (id: string) => {
        setItems((prev) => prev.filter((g) => g.id !== id));
    };

    const clearCart = () => setItems([]);

    const total = items.reduce((s, g) => s + (g.price || 0), 0);
    const count = items.length;

    return (
        <CartContext.Provider value={{ items, addToCart, removeFromCart, clearCart, total, count }}>
            {children}
        </CartContext.Provider>
    );
};

export const useCart = () => {
    const ctx = useContext(CartContext);
    if (!ctx) throw new Error('useCart must be used within CartProvider');
    return ctx;
};

export default CartContext;
