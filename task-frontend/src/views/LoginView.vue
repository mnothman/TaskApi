<script setup>
import { ref } from "vue";
import { useAuthStore } from "@/stores/auth";
import { useRouter } from "vue-router";

const authStore = useAuthStore();
const router = useRouter();
const username = ref("");
const password = ref("");
const errorMessage = ref("");

const handleLogin = async () => {
  try {
    await authStore.login(username.value, password.value);
    if (authStore.isAuthenticated) {
      router.push("/"); // Redirect to home after login
    }
  } catch (error) {
    errorMessage.value = "Invalid login credentials";
  }
};
</script>

<template>
  <div>
    <h2>Login</h2>
    <input v-model="username" placeholder="Username" />
    <input v-model="password" type="password" placeholder="Password" />
    <button @click="handleLogin">Login</button>
    <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
  </div>
</template>
