import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { request, setToken } from '@/api/http'

export const useAuthStore = defineStore('auth', () => {
    const token = ref<string | null>(sessionStorage.getItem('token'))
    if (token.value) setToken(token.value)

    const isLoggedIn = computed(() => !!token.value)

    async function login(username: string, password: string) {
        const data = await request<{ token: string }>('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password }),
        })
        // TODO: set up getToken function instead.
        token.value = data.token
        // TODO: use httpOnly cookie
        setToken(data.token)
        sessionStorage.setItem('token', data.token)
    }

    function logout() {
        token.value = null
        setToken(null)
        sessionStorage.removeItem('token')
    }

    return { token, isLoggedIn, login, logout}
})