<script setup lang="ts">
import { ref } from 'vue'
import { STATUSES, type ApplicationInput, type JobApplication } from '@/api/applications'

const props = defineProps<{ modelValue?: JobApplication }>()

const emit = defineEmits<{
    save: [ApplicationInput]
    cancel: []
}>()

const form = ref<ApplicationInput>({
    companyName: '',
    roleTitle: '',
    status: 0,
    dateApplied: new Date().toISOString().slice(0,10), // today, as 'YYYY-MM-DD'
    sourceUrl: '',
    notes: '',
    ...props.modelValue,
})

function submit() {
    emit('save', {
        ...form.value,
        sourceUrl: form.value.sourceUrl?.trim() || null,
        notes: form.value.notes?.trim() || null,
    })
}
</script>

<template>
    <form @submit.prevent="submit">
        <label>
            Company
            <input v-model="form.companyName" required maxlength="250" />
        </label>

        <label>
            Role
            <input v-model="form.roleTitle" required maxlength="250" />
        </label>

        <label>
            Status
            <select v-model="form.status">
                <option v-for="(name, i) in STATUSES" :key="i" :value="i">{{ name }}</option>
            </select>
        </label>
        <br />
        <label>
            Date applied
            <input v-model="form.dateApplied" type="date" required />
        </label>

        <label>
            Job posting URL
            <input v-model="form.sourceUrl" type="url" maxlength="2048" />
        </label>

        <label>
            Notes
            <textarea v-model="form.notes" maxlength="4000" />
        </label>
        <br />
        <button type="submit">Save</button>
        <button type="button" v-on:click="emit('cancel')">Cancel</button>
    </form>
</template>
