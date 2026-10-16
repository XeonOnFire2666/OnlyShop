import { api } from "./client";

function buildQuery(params) {
    const search = new URLSearchParams();
    Object.entries(params || {}).forEach(([key, value]) => {
        if (value === undefined || value === null || value === "") return;
        search.append(key, value);
    });
    const qs = search.toString();
    return qs ? `?${qs}` : "";
}

export const ordersApi = {
    search: (query) => api.get(`/orders${buildQuery(query)}`),
    get: (id) => api.get(`/orders/${id}`),
    changeStatus: (id, payload) => api.post(`/orders/${id}/status`, payload),
};

export const staffApi = {
    list: () => api.get("/staff"),
    create: (payload) => api.post("/staff", payload),
    setActive: (id, isActive) => api.put(`/staff/${id}/active`, { isActive }),
};