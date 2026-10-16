import { ApiError, BASE_URL } from "./client";

const AUTH_URL = BASE_URL.replace(/\/admin$/, "/admin/auth");

export async function login(email, password) {
    const res = await fetch(`${AUTH_URL}/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
    });

    const data = await res.json().catch(() => null);

    if (!res.ok) {
        const message = (data && data.message) || "Неверный email или пароль";
        throw new ApiError(message, res.status, data);
    }

    return data;
}

export async function requestTelegramLinkCode(apiGet) {
    return apiGet("/me/telegram-link");
}