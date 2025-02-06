<script setup>
import { ref, onMounted } from "vue";
import { apiClient } from "../services/api.js";

const tasks = ref([]);

const fetchTasks = async () => {
  try {
    const response = await apiClient.get("tasks"); // Auto-uses API_URL
    console.log("API Response:", response.data);
    tasks.value = response.data;
  } catch (error) {
    console.error("Error fetching tasks:", error.response?.data || error.message);
  }
};

onMounted(fetchTasks);
</script>

<template>
  <div>
    <h2>Tasks</h2>
    <ul>
      <li v-for="task in tasks" :key="task.id">{{ task.title }}</li>
    </ul>
  </div>
</template>
