import axios from "axios";

export const API_URL = import.meta.env.VITE_API_URL;

export const getTasks = async (token) => {
    return axios.get(`${API_URL}tasks`, {
        headers: { Authorization: `Bearer ${token}` },
    });
};
