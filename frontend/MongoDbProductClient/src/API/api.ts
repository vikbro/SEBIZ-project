import axios from "axios";

const API = axios.create({
    baseURL: "https://localhost:7282/api",
    headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
    },
});

API.interceptors.request.use((config) => {
    const user = localStorage.getItem('user');
    if (user) {
        const token = JSON.parse(user).token;
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
    }
    return config;
}, (error) => {
    return Promise.reject(error);
});

export default API;
