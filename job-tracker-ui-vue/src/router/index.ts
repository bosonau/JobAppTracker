import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/applications' },
    { path: '/login', component: () => import('@/views/LoginView.vue')},
    { path: '/applications', component: () => import('@/views/ApplicationsView.vue'), meta: { requiresAuth: true }}
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isLoggedIn)  return '/login'
})

export default router
