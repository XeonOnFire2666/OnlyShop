import { useEffect, useState } from "react";
import PageHeader from "../components/PageHeader";
import Button from "../components/Button";
import Modal from "../components/Modal";
import Badge from "../components/Badge";
import { Field, Input, Select } from "../components/Field";
import { EmptyState, Spinner } from "../components/Feedback";
import { staffApi } from "../api/orders";
import { useToast } from "../components/Toast";
import "./StaffPage.css";

const ROLES = [
    { value: "Manager", label: "Менеджер" },
    { value: "Admin", label: "Администратор" },
];

export default function StaffPage() {
    const toast = useToast();
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [creating, setCreating] = useState(false);

    async function load() {
        setLoading(true);
        try {
            setItems(await staffApi.list());
        } catch (err) {
            toast.error(err.message || "Не удалось загрузить сотрудников");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
    }, []);

    async function toggleActive(staff) {
        try {
            await staffApi.setActive(staff.id, !staff.isActive);
            await load();
            toast.success(staff.isActive ? "Доступ отключён" : "Доступ включён");
        } catch (err) {
            toast.error(err.message || "Не удалось изменить");
        }
    }

    return (
        <div>
            <PageHeader
                title="Сотрудники"
                description="Доступ к панели и роли"
                actions={
                    <Button variant="solid" onClick={() => setCreating(true)}>
                        + Добавить сотрудника
                    </Button>
                }
            />

            {loading ? (
                <Spinner label="Загрузка…" />
            ) : items.length === 0 ? (
                <EmptyState
                    icon="◍"
                    title="Сотрудников нет"
                    description="Добавьте первого сотрудника."
                />
            ) : (
                <div className="staff-list glass">
                    {items.map((s) => (
                        <div key={s.id} className="staff-row">
                            <div className="staff-row__main">
                                <span className="staff-row__name">{s.name}</span>
                                <span className="staff-row__email">{s.email}</span>
                            </div>
                            <Badge tone={s.role === "Admin" ? "primary" : "neutral"}>
                                {s.role === "Admin" ? "Администратор" : "Менеджер"}
                            </Badge>
                            <Badge tone={s.isActive ? "success" : "danger"}>
                                {s.isActive ? "Активен" : "Отключён"}
                            </Badge>
                            <div className="staff-row__actions">
                                <Button
                                    variant={s.isActive ? "ghost" : "subtle"}
                                    size="sm"
                                    onClick={() => toggleActive(s)}
                                >
                                    {s.isActive ? "Отключить" : "Включить"}
                                </Button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {creating && (
                <StaffCreateModal
                    onClose={() => setCreating(false)}
                    onCreated={async () => {
                        setCreating(false);
                        await load();
                        toast.success("Сотрудник добавлен");
                    }}
                />
            )}
        </div>
    );
}

function StaffCreateModal({ onClose, onCreated }) {
    const toast = useToast();
    const [form, setForm] = useState({
        name: "",
        email: "",
        password: "",
        role: "Manager",
    });
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState("");

    async function submit(e) {
        e.preventDefault();
        setError("");
        setBusy(true);
        try {
            await staffApi.create({
                name: form.name.trim(),
                email: form.email.trim(),
                password: form.password,
                role: form.role,
            });
            onCreated();
        } catch (err) {
            setError(err.message || "Не удалось создать");
        } finally {
            setBusy(false);
        }
    }

    return (
        <Modal title="Новый сотрудник" onClose={onClose}>
            <form className="pform" onSubmit={submit}>
                <Field label="Имя" required>
                    <Input
                        value={form.name}
                        onChange={(e) => setForm({ ...form, name: e.target.value })}
                        required
                        autoFocus
                    />
                </Field>
                <Field label="Email" required>
                    <Input
                        type="email"
                        value={form.email}
                        onChange={(e) => setForm({ ...form, email: e.target.value })}
                        required
                    />
                </Field>
                <Field label="Пароль" required hint="Минимум 8 символов">
                    <Input
                        type="password"
                        value={form.password}
                        onChange={(e) => setForm({ ...form, password: e.target.value })}
                        required
                        minLength={8}
                    />
                </Field>
                <Field label="Роль" required>
                    <Select
                        value={form.role}
                        onChange={(e) => setForm({ ...form, role: e.target.value })}
                    >
                        {ROLES.map((r) => (
                            <option key={r.value} value={r.value}>
                                {r.label}
                            </option>
                        ))}
                    </Select>
                </Field>

                {error && <div className="pform__error">{error}</div>}

                <div className="pform__actions">
                    <Button variant="ghost" type="button" onClick={onClose} disabled={busy}>
                        Отмена
                    </Button>
                    <Button variant="solid" type="submit" loading={busy}>
                        Создать
                    </Button>
                </div>
            </form>
        </Modal>
    );
}