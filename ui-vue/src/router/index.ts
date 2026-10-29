import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/applications' },
    { path: '/login', component: () => import('@/views/LoginView.vue'), meta: {guestOnly: true}},
    { path: '/applications', component: () => import('@/views/ApplicationsView.vue'), meta: { requiresAuth: true }}
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  //for logged out users
  if (to.meta.requiresAuth && !auth.isLoggedIn)  {
    return { path: '/login', query: {redirect: to.fullPath} }
  }
  //for logged in users
  if (to.meta.guestOnly && auth.isLoggedIn) {
        return '/applications'
  }
})

export default router
