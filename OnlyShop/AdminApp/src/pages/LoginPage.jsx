import { useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { Field, Input } from "../components/Field";
import Button from "../components/Button";
import "./LoginPage.css";

export default function LoginPage() {
    const { login } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    async function handleSubmit(e) {
        e.preventDefault();
        setError("");
        setLoading(true);
        try {
            await login(email, password);
            const redirectTo = location.state?.from?.pathname || "/products";
            navigate(redirectTo, { replace: true });
        } catch (err) {
            setError(err.message || "Не удалось войти");
        } finally {
            setLoading(false);
        }
    }

    return (
        <div className="login">
            <div className="login__card glass">
                <div className="login__brand">
                    <span className="login__brand-mark">OS</span>
                    <span>OnlyShop Admin</span>
                </div>

                <h1>Вход в панель</h1>
                <p className="login__subtitle">Введите рабочие учётные данные сотрудника</p>

                <form onSubmit={handleSubmit} className="login__form">
                    <Field label="Email" required>
                        <Input
                            type="email"
                            autoComplete="username"
                            placeholder="you@onlyshop.by"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                        />
                    </Field>

                    <Field label="Пароль" required>
                        <Input
                            type="password"
                            autoComplete="current-password"
                            placeholder="••••••••"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                        />
                    </Field>

                    {error && <div className="login__error">{error}</div>}

                    <Button type="submit" variant="solid" size="lg" loading={loading} className="login__submit">
                        Войти
                    </Button>
                </form>
            </div>
        </div>
    );
}