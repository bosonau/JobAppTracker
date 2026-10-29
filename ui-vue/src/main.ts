import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router/index.ts'
import { setOnUnauthorized } from './api/http.ts'
import { useAuthStore } from './stores/auth.ts'

const app = createApp(App)
app.use(createPinia())
app.use(router)

const auth = useAuthStore()
setOnUnauthorized(() => { 
    auth.logout()
    const current = router.currentRoute.value
    if (current.meta.guestOnly) return
    router.push({ path: '/login', query: {redirect: current.fullPath}}) 
})

app.mount('#app')
