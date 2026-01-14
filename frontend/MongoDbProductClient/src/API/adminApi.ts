import API from './api';

// Transaction endpoints
export const getAdminTransactions = async () => {
    return await API.get('/User/admin/all-transactions');
};

// User endpoints
export const getAllUsers = async () => {
    return await API.get('/User/admin/all-users');
};

export const promoteToAdmin = async (userId: string) => {
    return await API.post(`/User/admin/promote/${userId}`);
};

export const demoteAdmin = async (userId: string) => {
    return await API.post(`/User/admin/demote/${userId}`);
};

// Game endpoints
export const deleteGameAsAdmin = async (gameId: string) => {
    return await API.delete(`/Game/admin/${gameId}`);
};

export const updateGameAsAdmin = async (gameId: string, gameData: any) => {
    return await API.put(`/Game/admin/${gameId}`, gameData);
};
