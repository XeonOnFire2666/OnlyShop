import { api } from "./client";

function buildQuery(params) {
    const search = new URLSearchParams();
    Object.entries(params || {}).forEach(([key, value]) => {
        if (value === undefined || value === null || value === "") return;
        if (Array.isArray(value)) {
            value.forEach((v) => search.append(key, v));
        } else {
            search.append(key, value);
        }
    });
    const qs = search.toString();
    return qs ? `?${qs}` : "";
}

export const productsApi = {
    search: (query) => api.get(`/products${buildQuery(query)}`),
    get: (id) => api.get(`/products/${id}`),
    create: (payload) => api.post("/products", payload),
    updateDetails: (id, payload) => api.put(`/products/${id}`, payload),
    changePrice: (id, payload) => api.put(`/products/${id}/price`, payload),
    setPresale: (id, payload) => api.put(`/products/${id}/presale`, payload),
    setInStock: (id) => api.put(`/products/${id}/in-stock`),
    addVariant: (id, payload) => api.post(`/products/${id}/variants`, payload),
    adjustStock: (id, variantId, delta) => api.patch(`/products/${id}/variants/${variantId}/stock`, { delta }),
    addPhoto: (id, file) => {
        const form = new FormData();
        form.append("file", file);
        return api.postForm(`/products/${id}/photos`, form);
    },
    publish: (id) => api.post(`/products/${id}/publish`),
    archive: (id) => api.post(`/products/${id}/archive`),
};

export const categoriesApi = {
    list: () => api.get("/categories"),
    create: (payload) => api.post("/categories", payload),
    update: (id, payload) => api.put(`/categories/${id}`, payload),
    remove: (id) => api.delete(`/categories/${id}`),
};

export const presaleLocationsApi = {
    list: () => api.get("/presale-locations"),
    create: (formData) => api.postForm("/presale-locations", formData),
    update: (id, formData) => api.putForm(`/presale-locations/${id}`, formData),
    remove: (id) => api.delete(`/presale-locations/${id}`),
};