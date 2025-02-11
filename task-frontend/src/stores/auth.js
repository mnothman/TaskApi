import { defineStore } from "pinia";
import { apiClient } from "@/services/api.js";
import { login as apiLogin, logout as apiLogout, checkAuth } from "@/services/api.js";

export const useAuthStore = defineStore("auth", {
  state: () => ({
    isAuthenticated: false, // Tracks login state
  }),

  actions: {
    // No manual JWT handling, rely on cookies
    async login(username, password) {
      try {
        await apiLogin(username, password);
        await this.checkAuthState(); // Ensure authentication state is updated **after login**
        
        if (this.isAuthenticated) {
          console.log("Login successful! Redirecting...");
        } else {
          console.error("Login failed: Token not stored properly.");
        }
      } catch (error) {
        console.error("Login failed:", error.response?.data || error);
        throw error;
      }
    },
    

    // Check Authentication State (No need to call fetchUser separately)
    async checkAuthState() {
      try {
        const response = await checkAuth();
        this.isAuthenticated = response; // Backend returns true/false
    
        console.log("Auth status:", this.isAuthenticated);
      } catch (error) {
        console.error("Auth check failed:", error.response?.data || error);
        this.isAuthenticated = false;
      }
    },
    
    // Clears backend session + resets state
    async logout() {
      try {
        await apiLogout();
      } catch (error) {
        console.error("Logout failed:", error.response?.data || error);
      }
      this.isAuthenticated = false;
    },
  },

  // Enable state persistence
  persist: {
    enabled: true,
    strategies: [
      { storage: sessionStorage, paths: ["isAuthenticated"] }, // Stores only `isAuthenticated`
    ],
  },
});

