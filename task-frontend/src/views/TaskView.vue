<script setup>
import { ref, watchEffect } from "vue";
import { getTasks } from "@/services/api.js";
import { useAuthStore } from "@/stores/auth";
import { useRouter } from "vue-router";

const authStore = useAuthStore();
const router = useRouter();
const tasks = ref([]);
const errorMessage = ref("");

const fetchTasks = async () => {
  try {
    tasks.value = await getTasks();
  } catch (error) {
    errorMessage.value = "Failed to fetch tasks.";
  }
};

const handleLogout = async () => {
  await authStore.logout();
  tasks.value = []; // clear tasks after logout
  router.push("/login");
};

// Wait for authentication state before fetching tasks
watchEffect(() => {
  if (authStore.isAuthenticated) {
    fetchTasks();
  } else {
    router.push("/login");
  }
});
</script>

<template>
  <div>
    <h2>Tasks</h2>
    <button @click="handleLogout">Logout</button>
    <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
    <ul>
      <li v-for="task in tasks" :key="task.id">{{ task.title }}</li>
    </ul>
  </div>
</template>

<style scoped>
.error {
  color: red;
}
button {
  margin-bottom: 10px;
  padding: 8px;
  background-color: #ff4d4d;
  color: white;
  border: none;
  cursor: pointer;
}
button:hover {
  background-color: #cc0000;
}
</style>
