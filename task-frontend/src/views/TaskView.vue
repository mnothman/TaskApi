<script setup>
import { ref, onMounted } from "vue";
import { getTasks, login, logout } from "@/services/api.js";

const tasks = ref([]);
const username = ref("");
const password = ref("");
const isAuthenticated = ref(false); // Track authentication status
const errorMessage = ref("");

const fetchTasks = async () => {
  try {
    tasks.value = await getTasks();
    isAuthenticated.value = true; // If fetching tasks succeeds, user is authenticated
  } catch (error) {
    console.error("Failed to fetch tasks:", error);
    isAuthenticated.value = false; // If fetching tasks fails, user is not authenticated
  }
};

const handleLogin = async () => {
  try {
    await login(username.value, password.value);
    await fetchTasks(); // Fetch tasks after login
    errorMessage.value = "";
  } catch (error) {
    errorMessage.value = "Invalid username or password";
  }
};

const handleLogout = async () => {
  try {
    await logout();
    tasks.value = []; // Clear tasks after logout
    isAuthenticated.value = false;
  } catch (error) {
    console.error("Logout failed:", error);
  }
};

onMounted(fetchTasks);
</script>

<template>
  <div>
    <h2 v-if="!isAuthenticated">Login</h2>
    <div v-if="!isAuthenticated">
      <input v-model="username" placeholder="Username" />
      <input v-model="password" type="password" placeholder="Password" />
      <button @click="handleLogin">Login</button>
      <p v-if="errorMessage" style="color: red">{{ errorMessage }}</p>
    </div>

    <div v-else>
      <h2>Tasks</h2>
      <button @click="handleLogout">Logout</button>
      <ul>
        <li v-for="task in tasks" :key="task.id">{{ task.title }}</li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
input {
  margin: 5px;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}
button {
  margin: 5px;
  padding: 8px;
  background-color: #007bff;
  color: white;
  border: none;
  cursor: pointer;
}
button:hover {
  background-color: #0056b3;
}
</style>
