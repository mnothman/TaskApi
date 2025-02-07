import axios from "axios";

export const API_URL = import.meta.env.VITE_API_URL;

console.log("API URL:", API_URL);

export const apiClient = axios.create({
    baseURL: API_URL,
    headers: { "Content-Type": "application/json" },
    withCredentials: true // Ensures JWT stored in cookies is sent with each request
  });

// Get tasks, no need to manually pass a token anymore
export const getTasks = async () => {
  try {
    const response = await apiClient.get("tasks"); // Axios will send JWT automatically
    return response.data;
  } catch (error) {
    console.error("Error fetching tasks:", error.response?.data || error);
    throw error;
  }
};

// Login and store JWT in HttpOnly cookie
export const login = async (username, password) => {
  try {
    const response = await apiClient.post("auth/login", { username, password });
    return response.data;
  } catch (error) {
    console.error("Login failed:", error.response?.data || error);
    throw error;
  }
};

// Logout (Clears JWT cookie)
export const logout = async () => {
  try {
    await apiClient.post("auth/logout");
  } catch (error) {
    console.error("Logout failed:", error.response?.data || error);
    throw error;
  }
};