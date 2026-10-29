//API helper
const BASE = import.meta.env.VITE_API_BASE_URL

let token: string | null = null
let onUnauthorized: () => void = () => {}

export function setToken(t: string | null) { token = t }
export function setOnUnauthorized(fn: () => void) { onUnauthorized = fn }

export class ApiError extends Error {
    constructor(public status: number, message: string) { super(message) }
}

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
    const headers = new Headers(options.headers)
    headers.set('Content-Type', 'application/json')
    if (token) headers.set('Authorization', `Bearer ${token}` )
    
    const res = await fetch(`${BASE}${path}`, { ...options, headers})

    if (res.status === 401) {
       onUnauthorized()
       throw new ApiError(401, 'Not Authorized') 
    }

    if (!res.ok) throw new ApiError(res.status, await res.text())
    if (res.status === 204) return undefined as T
    return (await res.json()) as T
}