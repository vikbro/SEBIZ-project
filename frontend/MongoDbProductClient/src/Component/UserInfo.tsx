import React, { useState, useEffect } from 'react';
import api from '../API/api';
import type { User } from '../Interface/baseInterface';

const UserInfo = () => {
    const [user, setUser] = useState<User | null>(null);
    const [amount, setAmount] = useState(0);
    const [showModal, setShowModal] = useState(false);
    const [playtime, setPlaytime] = useState(0);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const userResponse = await api.get('/User/me');
                setUser(userResponse.data);

                const playtimeResponse = await api.get(`/User/${userResponse.data.id}/playtime`);
                setPlaytime(playtimeResponse.data);
            } catch (error) {
                console.error('Error fetching data:', error);
            }
        };

        fetchData();
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

    const formatPlaytime = (minutes: number) => {
        const hours = Math.floor(minutes / 60);
        const remainingMinutes = minutes % 60;
        return `${hours}h ${remainingMinutes}m`;
    };

    return (
        <div className="container mx-auto mt-10">
            <div className="bg-white p-6 rounded-lg shadow-lg">
                <h1 className="text-2xl font-bold mb-4">User Information</h1>
                <p><strong>Username:</strong> {user.username}</p>
                <p><strong>Balance:</strong> ${user.balance}</p>
                <p><strong>Total Playtime:</strong> {formatPlaytime(playtime)}</p>
                <button
                    onClick={() => setShowModal(true)}
                    className="mt-4 bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
                >
                    Add Balance
                </button>
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
