import axios from "axios";

export const hoardApi = axios.create({
    baseURL: import.meta.env.VITE_HOARD_API_BASE_URL,
    timeout: 10_000
});