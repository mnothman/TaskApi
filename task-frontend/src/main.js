import { createApp } from 'vue'
import { createPinia } from "pinia";
import App from './App.vue'
import router from './router';
import { useAuthStore } from "@/stores/auth";
import piniaPersist from "pinia-plugin-persistedstate";

const pinia = createPinia();
pinia.use(piniaPersist); // Enable state persistence

const app = createApp(App);

app.use(pinia);
app.use(router);

const authStore = useAuthStore();
authStore.checkAuthState().then(() => {
  app.mount("#app");
});