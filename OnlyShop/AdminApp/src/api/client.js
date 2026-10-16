const BASE_URL = import.meta.env.VITE_API_BASE_URL || "/api/admin";

let onUnauthorized = () => { };

export function setUnauthorizedHandler(handler) {
    onUnauthorized = handler;
}

function getToken() {
    return localStorage.getItem("onlyshop_token");
}

export function setToken(token) {
    if (token) {
        localStorage.setItem("onlyshop_token", token);
    } else {
        localStorage.removeItem("onlyshop_token");
    }
}

export class ApiError extends Error {
    constructor(message, status, data) {
        super(message);
        this.status = status;
        this.data = data;
    }
}

async function request(path, { method = "GET", body, isForm = false, signal } = {}) {
    const headers = {};
    const token = getToken();
    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    let payload = body;
    if (body && !isForm) {
        headers["Content-Type"] = "application/json";
        payload = JSON.stringify(body);
    }

    const res = await fetch(`${BASE_URL}${path}`, {
        method,
        headers,
        body: payload,
        signal,
    });

    if (res.status === 401) {
        onUnauthorized();
        throw new ApiError("Сессия истекла, войдите снова", 401);
    }

    if (res.status === 204) {
        return null;
    }

    const contentType = res.headers.get("content-type") || "";
    const data = contentType.includes("application/json") ? await res.json() : await res.text();

    if (!res.ok) {
        const message = (data && data.message) || (typeof data === "string" ? data : "Ошибка запроса");
        throw new ApiError(message, res.status, data);
    }

    return data;
}

export const api = {
    get: (path, opts) => request(path, { ...opts, method: "GET" }),
    post: (path, body, opts) => request(path, { ...opts, method: "POST", body }),
    put: (path, body, opts) => request(path, { ...opts, method: "PUT", body }),
    patch: (path, body, opts) => request(path, { ...opts, method: "PATCH", body }),
    delete: (path, opts) => request(path, { ...opts, method: "DELETE" }),
    postForm: (path, formData, opts) => request(path, { ...opts, method: "POST", body: formData, isForm: true }),
    putForm: (path, formData, opts) => request(path, { ...opts, method: "PUT", body: formData, isForm: true }),
};

export { BASE_URL };