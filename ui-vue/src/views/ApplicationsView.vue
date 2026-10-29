<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { applicationsApi, type JobApplication, STATUSES, type ApplicationInput } from '@/api/applications'
import ApplicationForm from '@/components/ApplicationForm.vue'

const error = ref('')
const items = ref<JobApplication[]>([])
const loading = ref(false)
const showForm = ref(false)
const editing = ref<JobApplication | undefined>()

onMounted(async () => {
    await getApplications()
})

async function getApplications() {
    loading.value = true
    try {
        items.value = await applicationsApi.list()
    } catch {
        error.value = 'Loading app error'
    } finally {
        loading.value = false
    }
}

async function deleteApplication(application:JobApplication) {
    if (!window.confirm(`Delete "${application.roleTitle} at ${application.companyName}?"`)) return
    try {
        await applicationsApi.remove(application.id).then(
            getApplications
        )
    } catch {
        error.value = "error in deleting"
    }
}
function startAdd() {
    editing.value = undefined
    showForm.value = true
}   

function startEdit(application: JobApplication) {
    editing.value = application
    showForm.value = true
}

function closeForm() {
    editing.value = undefined
    showForm.value = false
}

function dateFormat(d:string) {
    var date = new Date(d)
    return date.toLocaleDateString()
}

async function saveApplication(input: ApplicationInput) {
    try {
        if(editing.value) {
            await applicationsApi.update(editing.value.id, input)
        } else {
            await applicationsApi.create(input)
        }
        closeForm()
        await getApplications()
    } catch {
        error.value = 'Could not save the application.'
    }
}

</script>

<template>
    <h1>Applications</h1>
    <button v-if="!showForm" v-on:click="startAdd">Add Application</button>

    <ApplicationForm 
        v-if="showForm"
        :key="editing?.id ?? 'new'"
        :model-value="editing"
        @save="saveApplication"
        @cancel="closeForm"
    />

    <p v-if="loading">Loading...</p>
    <p v-else-if="error">{{ error }}</p>
    <p v-else-if="items.length == 0">No Applications Yet</p>
    <table v-else>
        <tr v-for="a in items" :key="a.id">
            <td>{{ a.companyName }}</td>
            <td>{{ a.roleTitle }}</td>
            <td>{{ dateFormat(a.dateApplied) }}</td>
            <td>{{ STATUSES[a.status] }}</td>
            <td><a :href="a.sourceUrl" target="_blank" rel="noopener" v-if="a.sourceUrl">link</a></td>
            <td>{{ a.notes }}</td>
            <td>
                <button v-on:click="startEdit(a)">Edit</button>
                <button v-on:click="deleteApplication(a)">Delete</button>
            </td>            
        </tr>
    </table>
</template>