import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { Spinner } from "../components/Feedback";

export function ProtectedRoute({ children, adminOnly = false }) {
    const { isAuthenticated, isAdmin, isReady } = useAuth();
    const location = useLocation();

    if (!isReady) {
        return <Spinner label="Загрузка…" />;
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (adminOnly && !isAdmin) {
        return <Navigate to="/products" replace />;
    }

    return children;
}