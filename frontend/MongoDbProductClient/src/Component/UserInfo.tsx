import React, { useState, useEffect } from 'react';
import api from '../API/api';
import type { User, GameUsage, Transaction } from '../Interface/baseInterface';

const UserInfo = () => {
    const [user, setUser] = useState<User | null>(null);
    const [gameUsages, setGameUsages] = useState<GameUsage[]>([]);
    const [transactions, setTransactions] = useState<Transaction[]>([]);
    const [amount, setAmount] = useState(0);
    const [showModal, setShowModal] = useState(false);
    const [activeTab, setActiveTab] = useState<'playtime' | 'transactions'>('playtime');

    useEffect(() => {
        const fetchUserAndGameUsage = async () => {
            try {
                const userResponse = await api.get('/User/me');
                setUser(userResponse.data);

                if (userResponse.data) {
                    const usageResponse = await api.get(`/GameUsage/WithGameDetails/me`);
                    setGameUsages(usageResponse.data);

                    const transactionResponse = await api.get('/User/transactions');
                    setTransactions(transactionResponse.data);
                }

            } catch (error) {
                console.error('Error fetching data:', error);
            }
        };

        fetchUserAndGameUsage();
    }, []);

    const handleAddBalance = async () => {
        try {
            await api.post('/User/add-balance', { amount }, {
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            // Refresh user data
            const response = await api.get('/User/me');
            setUser(response.data);
            setShowModal(false);
        } catch (error) {
            console.error('Error adding balance:', error);
        }
    };

    if (!user) {
        return <div>Loading...</div>;
    }

    const formatDate = (date: Date) => {
        return new Date(date).toLocaleDateString() + ' ' + new Date(date).toLocaleTimeString();
    };

    return (
        <div className="container mx-auto mt-10">
            <div className="bg-white p-6 rounded-lg shadow-lg">
                <h1 className="text-2xl font-bold mb-4">User Information</h1>
                <p><strong>Username:</strong> {user.username}</p>
                <p><strong>Email:</strong> {user.email}</p>
                <p><strong>Balance:</strong> ${user.balance}</p>
                <button
                    onClick={() => setShowModal(true)}
                    className="mt-4 bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
                >
                    Add Balance
                </button>
            </div>

            {/* Tabs */}
            <div className="mt-6 bg-white rounded-lg shadow-lg">
                <div className="flex border-b">
                    <button
                        onClick={() => setActiveTab('playtime')}
                        className={`flex-1 px-6 py-3 font-semibold text-center ${activeTab === 'playtime' ? 'border-b-2 border-blue-500 text-blue-500' : 'text-gray-500'}`}
                    >
                        Playtime History
                    </button>
                    <button
                        onClick={() => setActiveTab('transactions')}
                        className={`flex-1 px-6 py-3 font-semibold text-center ${activeTab === 'transactions' ? 'border-b-2 border-blue-500 text-blue-500' : 'text-gray-500'}`}
                    >
                        Purchase History
                    </button>
                </div>

                {/* Playtime Tab */}
                {activeTab === 'playtime' && (
                    <div className="p-6">
                        <h2 className="text-xl font-bold mb-4">Game Playtime</h2>
                        {gameUsages.length === 0 ? (
                            <p className="text-gray-500">No playtime data available.</p>
                        ) : (
                            <table className="min-w-full">
                                <thead>
                                    <tr>
                                        <th className="px-6 py-3 border-b-2 border-gray-300 text-left text-xs leading-4 font-medium text-gray-500 uppercase tracking-wider">Game Title</th>
                                        <th className="px-6 py-3 border-b-2 border-gray-300 text-left text-xs leading-4 font-medium text-gray-500 uppercase tracking-wider">Playtime (minutes)</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {gameUsages.map((usage, index) => (
                                        <tr key={index}>
                                            <td className="px-6 py-4 whitespace-no-wrap border-b border-gray-500">{usage.gameTitle}</td>
                                            <td className="px-6 py-4 whitespace-no-wrap border-b border-gray-500">{usage.playTimeMinutes}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                )}

                {/* Transactions Tab */}
                {activeTab === 'transactions' && (
                    <div className="p-6">
                        <h2 className="text-xl font-bold mb-4">Purchase History</h2>
                        {transactions.length === 0 ? (
                            <p className="text-gray-500">No purchase history available.</p>
                        ) : (
                            <div className="overflow-x-auto">
                                <table className="min-w-full">
                                    <thead>
                                        <tr>
                                            <th className="px-6 py-3 border-b-2 border-gray-300 text-left text-xs leading-4 font-medium text-gray-500 uppercase tracking-wider">Game</th>
                                            <th className="px-6 py-3 border-b-2 border-gray-300 text-left text-xs leading-4 font-medium text-gray-500 uppercase tracking-wider">Amount</th>
                                            <th className="px-6 py-3 border-b-2 border-gray-300 text-left text-xs leading-4 font-medium text-gray-500 uppercase tracking-wider">From/To</th>
                                            <th className="px-6 py-3 border-b-2 border-gray-300 text-left text-xs leading-4 font-medium text-gray-500 uppercase tracking-wider">Date</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {transactions.map((transaction) => {
                                            const isBuyer = transaction.buyerId === user.id;
                                            return (
                                                <tr key={transaction.id}>
                                                    <td className="px-6 py-4 whitespace-no-wrap border-b border-gray-500 font-semibold">{transaction.gameTitle}</td>
                                                    <td className="px-6 py-4 whitespace-no-wrap border-b border-gray-500">${transaction.amount.toFixed(2)}</td>
                                                    <td className="px-6 py-4 whitespace-no-wrap border-b border-gray-500">
                                                        {isBuyer ? (
                                                            <span className="text-red-600">Paid to {transaction.sellerUsername}</span>
                                                        ) : (
                                                            <span className="text-green-600">Received from {transaction.buyerUsername}</span>
                                                        )}
                                                    </td>
                                                    <td className="px-6 py-4 whitespace-no-wrap border-b border-gray-500 text-sm">{formatDate(transaction.transactionDate)}</td>
                                                </tr>
                                            );
                                        })}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                )}
            </div>

            {showModal && (
                <div className="fixed z-10 inset-0 overflow-y-auto">
                    <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
                        <div className="fixed inset-0 transition-opacity" aria-hidden="true">
                            <div className="absolute inset-0 bg-gray-500 opacity-75"></div>
                        </div>
                        <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>
                        <div className="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
                            <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                                <div className="sm:flex sm:items-start">
                                    <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left">
                                        <h3 className="text-lg leading-6 font-medium text-gray-900" id="modal-title">
                                            Add Balance
                                        </h3>
                                        <div className="mt-2">
                                            <input
                                                type="number"
                                                className="w-full border-2 border-gray-300 p-2 rounded-lg"
                                                value={amount}
                                                onChange={(e) => setAmount(parseFloat(e.target.value))}
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                                <button
                                    type="button"
                                    className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:ml-3 sm:w-auto sm:text-sm"
                                    onClick={handleAddBalance}
                                >
                                    Add
                                </button>
                                <button
                                    type="button"
                                    className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                                    onClick={() => setShowModal(false)}
                                >
                                    Cancel
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default UserInfo;
