import { useEffect, useState } from "react";
import PageHeader from "../components/PageHeader";
import Button from "../components/Button";
import Modal from "../components/Modal";
import { Field, Input, Select } from "../components/Field";
import { ConfirmDialog, EmptyState, Spinner } from "../components/Feedback";
import { categoriesApi } from "../api/products";
import { useToast } from "../components/Toast";
import "./CategoriesPage.css";

export default function CategoriesPage() {
    const toast = useToast();
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [editing, setEditing] = useState(null);
    const [confirmRemove, setConfirmRemove] = useState(null);

    async function load() {
        setLoading(true);
        try {
            setItems(await categoriesApi.list());
        } catch (err) {
            toast.error(err.message || "Не удалось загрузить категории");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
    }, []);

    async function submit(form) {
        try {
            if (form.id) {
                await categoriesApi.update(form.id, {
                    name: form.name,
                    slug: form.slug,
                    parentId: form.parentId || null,
                    sortOrder: Number(form.sortOrder) || 0,
                });
            } else {
                await categoriesApi.create({
                    name: form.name,
                    slug: form.slug,
                    parentId: form.parentId || null,
                    sortOrder: Number(form.sortOrder) || 0,
                });
            }
            setEditing(null);
            await load();
            toast.success(form.id ? "Категория обновлена" : "Категория создана");
        } catch (err) {
            toast.error(err.message || "Ошибка сохранения");
        }
    }

    async function remove(id) {
        try {
            await categoriesApi.remove(id);
            setConfirmRemove(null);
            await load();
            toast.success("Категория удалена");
        } catch (err) {
            toast.error(err.message || "Не удалось удалить");
        }
    }

    return (
        <div>
            <PageHeader
                title="Категории"
                description="Дерево категорий и порядок отображения"
                actions={
                    <Button
                        variant="solid"
                        onClick={() =>
                            setEditing({ name: "", slug: "", parentId: "", sortOrder: 0 })
                        }
                    >
                        + Новая категория
                    </Button>
                }
            />

            {loading ? (
                <Spinner label="Загрузка…" />
            ) : items.length === 0 ? (
                <EmptyState
                    icon="◫"
                    title="Категорий нет"
                    description="Создайте первую категорию каталога."
                />
            ) : (
                <div className="cat-list glass">
                    {items.map((c) => (
                        <div key={c.id} className="cat-row">
                            <div className="cat-row__main">
                                <span className="cat-row__name">{c.name}</span>
                                <span className="cat-row__slug">/{c.slug}</span>
                            </div>
                            <span className="cat-row__order">#{c.sortOrder}</span>
                            <div className="cat-row__actions">
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() =>
                                        setEditing({
                                            id: c.id,
                                            name: c.name,
                                            slug: c.slug,
                                            parentId: c.parentId || "",
                                            sortOrder: c.sortOrder,
                                        })
                                    }
                                >
                                    Изменить
                                </Button>
                                <Button
                                    variant="danger"
                                    size="sm"
                                    onClick={() => setConfirmRemove(c)}
                                >
                                    Удалить
                                </Button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {editing && (
                <CategoryForm
                    initial={editing}
                    categories={items}
                    onClose={() => setEditing(null)}
                    onSubmit={submit}
                />
            )}

            {confirmRemove && (
                <ConfirmDialog
                    title="Удалить категорию?"
                    message={`Категория «${confirmRemove.name}» будет удалена. Товары в ней останутся без категории.`}
                    confirmLabel="Удалить"
                    onClose={() => setConfirmRemove(null)}
                    onConfirm={() => remove(confirmRemove.id)}
                />
            )}
        </div>
    );
}

function slugify(value) {
    return value
        .toLowerCase()
        .trim()
        .replace(/[а-яё]/g, (ch) => {
            const map = {
                а: "a", б: "b", в: "v", г: "g", д: "d", е: "e", ё: "e", ж: "zh",
                з: "z", и: "i", й: "y", к: "k", л: "l", м: "m", н: "n", о: "o",
                п: "p", р: "r", с: "s", т: "t", у: "u", ф: "f", х: "h", ц: "ts",
                ч: "ch", ш: "sh", щ: "sch", ъ: "", ы: "y", ь: "", э: "e", ю: "yu", я: "ya",
            };
            return map[ch] || "";
        })
        .replace(/[^a-z0-9]+/g, "-")
        .replace(/^-|-$/g, "");
}

function CategoryForm({ initial, categories, onClose, onSubmit }) {
    const [form, setForm] = useState(initial);
    const [busy, setBusy] = useState(false);
    const [autoSlug, setAutoSlug] = useState(!initial.id);

    function setName(v) {
        setForm((f) => ({
            ...f,
            name: v,
            slug: autoSlug && !f.id ? slugify(v) : f.slug,
        }));
    }

    async function handleSubmit(e) {
        e.preventDefault();
        setBusy(true);
        await onSubmit(form);
        setBusy(false);
    }

    const parents = categories.filter((c) => c.id !== form.id);

    return (
        <Modal title={form.id ? "Редактирование категории" : "Новая категория"} onClose={onClose}>
            <form className="pform" onSubmit={handleSubmit}>
                <Field label="Название" required>
                    <Input value={form.name} onChange={(e) => setName(e.target.value)} required autoFocus />
                </Field>
                <Field label="Slug" hint="URL-идентификатор, латиница и дефисы">
                    <Input
                        value={form.slug}
                        onChange={(e) => {
                            setAutoSlug(false);
                            setForm({ ...form, slug: e.target.value });
                        }}
                        required
                    />
                </Field>
                <div className="pform__row">
                    <Field label="Родитель">
                        <Select
                            value={form.parentId || ""}
                            onChange={(e) => setForm({ ...form, parentId: e.target.value })}
                        >
                            <option value="">— корневая —</option>
                            {parents.map((c) => (
                                <option key={c.id} value={c.id}>{c.name}</option>
                            ))}
                        </Select>
                    </Field>
                    <Field label="Порядок">
                        <Input
                            type="number"
                            value={form.sortOrder}
                            onChange={(e) => setForm({ ...form, sortOrder: e.target.value })}
                        />
                    </Field>
                </div>
                <div className="pform__actions">
                    <Button variant="ghost" type="button" onClick={onClose} disabled={busy}>
                        Отмена
                    </Button>
                    <Button variant="solid" type="submit" loading={busy}>
                        Сохранить
                    </Button>
                </div>
            </form>
        </Modal>
    );
}