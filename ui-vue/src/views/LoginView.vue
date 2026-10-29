<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth';

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const username = ref('')
const password = ref('')
const error = ref('')

//redirects, only to this app
function safeRedirect(value: unknown): string {
    return typeof value === 'string' && value.startsWith('/') && !value.startsWith('//')
        ? value
        : '/applications'
}


async function submit() {
    error.value = ''
    try {
        await auth.login(username.value, password.value)
        router.replace(safeRedirect(route.query.redirect))
    } catch {
        error.value = 'Login failed, check credentials.'
    }
}
</script>

<template>
    <form @submit.prevent="submit">
        <h1>Sign in</h1>
        <input v-model="username" placeholder="Username" autocomplete="username" />
        <input v-model="password" placeholder="Password" type="password" autocomplete="current-password" />
        <button type="submit">Sign in</button>
        <p v-if="error">{{ error }}</p>
    </form>
</template>