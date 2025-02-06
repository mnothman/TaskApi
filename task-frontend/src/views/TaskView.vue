<script setup>
import { ref, onMounted } from "vue";
import { apiClient, getTasks } from "../services/api.js";

const tasks = ref([]);
const authToken = ref(localStorage.getItem("token")); // Store JWT token locally

const fetchTasks = async () => {
  if (!authToken.value) {
    console.error("No authentication token found.");
    return;
  }
  try {
    tasks.value = await getTasks(authToken.value);
  } catch (error) {
    console.error("Failed to fetch tasks:", error);
  }
};

onMounted(fetchTasks);
</script>

<template>
  <div>
    <h2>Tasks</h2>
    <ul v-if="tasks.length">
      <li v-for="task in tasks" :key="task.id">{{ task.title }}</li>
    </ul>
    <p v-else>No tasks found.</p>
  </div>
</template>