import axios from "axios";

export const API_URL = import.meta.env.VITE_API_URL;

console.log("API URL:", API_URL);

export const apiClient = axios.create({
    baseURL: API_URL,
    headers: { "Content-Type": "application/json" },
  });

// Get tasks with authentication token
  export const getTasks = async (token) => {
    try {
      const response = await apiClient.get("tasks", {
        headers: { Authorization: `Bearer ${token}` },
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching tasks:", error.response?.data || error);
      throw error;
    }
  };