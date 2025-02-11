import axios from "axios";

// export const API_URL = import.meta.env.VITE_API_URL;
// export const API_URL = "http://localhost:5000/api/";
export const API_URL = process.env.NODE_ENV === "production"
  ? "http://taskapi_backend:5000/api/" // Inside Docker
  : "http://localhost:5000/api/"; // For local dev

// console.log("API URL:", API_URL);

export const apiClient = axios.create({
  // baseURL: API_URL,
  baseURL: "http://localhost:5000/api/",
  credentials: "include",
  headers: { "Content-Type": "application/json" },
  withCredentials: true, // Ensures JWT stored in cookies is sent with each request
});

// Add Authorization header manually
const authToken = document.cookie.split("; ").find(row => row.startsWith("AuthToken"))?.split("=")[1];
if (authToken) {
  apiClient.defaults.headers.common["Authorization"] = `Bearer ${authToken}`;
}

apiClient.interceptors.request.use((config) => {
  console.log("🚀 Sending Request:", config);
  return config;
});

apiClient.interceptors.response.use(
  (response) => {
    console.log("✅ API Response:", response);
    return response;
  },
  (error) => {
    console.error("❌ API Error:", error.response || error);
    return Promise.reject(error);
  }
);

// Get tasks, no need to manually pass a token anymore
export const getTasks = async () => {
  try {
    const response = await apiClient.get("tasks", { withCredentials: true });
    console.log("Tasks fetched:", response.data);
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