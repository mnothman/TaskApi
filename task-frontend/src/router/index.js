import { createRouter, createWebHistory } from "vue-router";
import TaskView from "@/views/TaskView.vue";
import LoginView from "@/views/LoginView.vue";
import { useAuthStore } from "@/stores/auth";

const routes = [
  { path: "/", component: TaskView, meta: { requiresAuth: true } },
  { path: "/login", component: LoginView },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// Navigation Guard for Authentication
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore(); // Correct way to access authentication state
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next("/login");
  } else {
    next();
  }
});

export default router;
