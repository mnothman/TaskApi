import axios from "axios";

export const API_URL = import.meta.env.VITE_API_URL;

console.log("API URL:", API_URL);

export const apiClient = axios.create({
    baseURL: API_URL,
    headers: { "Content-Type": "application/json" },
  });


  export const getTasks = async (token) => {
    return apiClient.get("tasks", {
      headers: { Authorization: `Bearer ${token}` },
    });
  };