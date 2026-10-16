import { createContext, useContext, useEffect, useMemo, useState, useCallback } from "react";
import { login as loginRequest } from "../api/auth";
import { setToken, setUnauthorizedHandler } from "../api/client";

const AuthContext = createContext(null);

function decodeJwt(token) {
    try {
        const payload = token.split(".")[1];
        const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
        return JSON.parse(decodeURIComponent(escape(json)));
    } catch {
        return null;
    }
}

export function AuthProvider({ children }) {
    const [staff, setStaff] = useState(null);
    const [isReady, setIsReady] = useState(false);

    const logout = useCallback(() => {
        setToken(null);
        localStorage.removeItem("onlyshop_staff");
        setStaff(null);
    }, []);

    useEffect(() => {
        setUnauthorizedHandler(logout);

        const token = localStorage.getItem("onlyshop_token");
        const storedStaff = localStorage.getItem("onlyshop_staff");

        if (token && storedStaff) {
            const payload = decodeJwt(token);
            const notExpired = payload?.exp ? payload.exp * 1000 > Date.now() : true;

            if (notExpired) {
                try {
                    setStaff(JSON.parse(storedStaff));
                } catch {
                    localStorage.removeItem("onlyshop_staff");
                    localStorage.removeItem("onlyshop_token");
                }
            } else {
                localStorage.removeItem("onlyshop_staff");
                localStorage.removeItem("onlyshop_token");
            }
        } else {
            localStorage.removeItem("onlyshop_staff");
            localStorage.removeItem("onlyshop_token");
        }

        setIsReady(true);
    }, [logout]);

    const login = useCallback(async (email, password) => {
        const result = await loginRequest(email, password);
        setToken(result.token);
        localStorage.setItem("onlyshop_staff", JSON.stringify(result.staff));
        setStaff(result.staff);
        return result.staff;
    }, []);

    const value = useMemo(
        () => ({
            staff,
            isReady,
            isAuthenticated: Boolean(staff),
            isAdmin: staff?.role === "Admin",
            login,
            logout,
        }),
        [staff, isReady, login, logout]
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) {
        throw new Error("useAuth must be used within AuthProvider");
    }
    return ctx;
}

export { decodeJwt };