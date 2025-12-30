import axios from "axios";

const API = axios.create({
    baseURL: "http://localhost:5182/api",
    headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
    },
});

API.interceptors.request.use((config) => {
    const user = localStorage.getItem('user');
    if (user) {
        const { token } = JSON.parse(user);
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
    }
    return config;
});


export default API;
