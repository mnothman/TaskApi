<script setup>
import { useAuthStore } from "@/stores/auth";
import { useRouter } from "vue-router";
import { computed, onMounted } from "vue";
 
const authStore = useAuthStore();
const router = useRouter();

const isAuthenticated = computed(() => authStore.isAuthenticated); // Ensure reactivity

const handleLogout = async () => {
  await authStore.logout();
  router.push("/login");
};

// Ensure authentication state is checked when the app starts
onMounted(async () => {
  await authStore.checkAuthState();
});
</script>

<template>
  <nav>
    <router-link to="/">Home</router-link>
    <router-link v-if="!isAuthenticated" to="/login">Login</router-link>
    <button v-if="isAuthenticated" @click="handleLogout">Logout</button>
  </nav>

  <router-view></router-view> <!-- This dynamically swaps the views -->
</template>
