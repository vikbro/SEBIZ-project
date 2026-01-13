import axios from "axios";

const BACKEND_URL = "http://localhost:5182";

const API = axios.create({
    baseURL: `${BACKEND_URL}/api`,
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

export { BACKEND_URL };
export default API;
