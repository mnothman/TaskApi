import axios from "axios";

export const API_URL = import.meta.env.VITE_API_URL;

// console.log("API URL:", API_URL);

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: { "Content-Type": "application/json" },
  withCredentials: true, // Ensures JWT stored in cookies is sent with each request
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

// API function for logging in
export const login = async (username, password) => {
  try {
    const response = await apiClient.post("auth/login", { username, password });
    return response.data;
  } catch (error) {
    console.error("Login failed:", error.response?.data || error);
    throw error;
  }
};

// API function for checking auth status
export const checkAuth = async () => {
  try {
    const response = await apiClient.get("auth/check");
    return response.data.isAuthenticated;
  } catch (error) {
    console.error("Auth check failed:", error.response?.data || error);
    return false;
  }
};

// API function for logging out
export const logout = async () => {
  try {
    await apiClient.post("auth/logout");
  } catch (error) {
    console.error("Logout failed:", error.response?.data || error);
    throw error;
  }
};