<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const error = ref('')

async function logout() {
    error.value = ''
    try {
        await auth.logout()
        router.push('/login')
    } catch {
        error.value = 'Logout failed!'
    }
}

</script>


<template>
  <button v-if="auth.isLoggedIn" v-on:click="logout">Logout</button>
  <RouterView />
</template>

<style scoped></style>
