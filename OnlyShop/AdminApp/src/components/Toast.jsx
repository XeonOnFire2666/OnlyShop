import { createContext, useCallback, useContext, useMemo, useRef, useState } from "react";
import "./Toast.css";

const ToastContext = createContext(null);

export function ToastProvider({ children }) {
    const [toasts, setToasts] = useState([]);
    const idRef = useRef(0);

    const remove = useCallback((id) => {
        setToasts((prev) => prev.filter((t) => t.id !== id));
    }, []);

    const push = useCallback(
        (message, tone = "neutral") => {
            const id = ++idRef.current;
            setToasts((prev) => [...prev, { id, message, tone }]);
            setTimeout(() => remove(id), 4200);
        },
        [remove]
    );

    const api = useMemo(
        () => ({
            success: (msg) => push(msg, "success"),
            error: (msg) => push(msg, "danger"),
            info: (msg) => push(msg, "neutral"),
        }),
        [push]
    );

    return (
        <ToastContext.Provider value={api}>
            {children}
            <div className="toast-stack">
                {toasts.map((t) => (
                    <div key={t.id} className={`toast glass toast--${t.tone}`}>
                        {t.message}
                    </div>
                ))}
            </div>
        </ToastContext.Provider>
    );
}

export function useToast() {
    const ctx = useContext(ToastContext);
    if (!ctx) throw new Error("useToast must be used within ToastProvider");
    return ctx;
}