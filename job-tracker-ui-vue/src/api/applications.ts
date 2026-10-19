import { request } from './http'

export interface JobApplication {
    id: number
    companyName: string
    roleTitle: string
    status: number
    dateApplied: string
    sourceUrl: string | null
    notes: string | null
}

export type ApplicationInput = Omit<JobApplication, 'id'>

export const applicationsApi = {
    //CRUD routes
    list: () => request<JobApplication[]>(`/api/applications`),
    get: (id:number) => request<JobApplication>(`/api/applications/${id}`),
    create: (a: ApplicationInput) => 
        request<JobApplication>('/api/applications', { method: 'POST', body: JSON.stringify(a) }),
    update: (id: number, a: ApplicationInput) =>
        request<void>(`/api/applications/${id}`, { method: 'PUT', body: JSON.stringify(a) }),
    remove: (id: number) => request<void>(`/api/applications/${id}`, { method: 'DELETE' }),
}

export const STATUSES = ['Applied', 'Screening', 'Interview', 'Offer', 'Rejected', 'Withdrawn']
