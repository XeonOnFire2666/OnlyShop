import { useEffect, useRef, useState } from "react";
import PageHeader from "../components/PageHeader";
import Button from "../components/Button";
import Modal from "../components/Modal";
import { Field, Input, Textarea } from "../components/Field";
import { ConfirmDialog, EmptyState, Spinner } from "../components/Feedback";
import { presaleLocationsApi } from "../api/products";
import { useToast } from "../components/Toast";
import "./PresaleLocationsPage.css";

export default function PresaleLocationsPage() {
    const toast = useToast();
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [editing, setEditing] = useState(null);
    const [confirmRemove, setConfirmRemove] = useState(null);

    async function load() {
        setLoading(true);
        try {
            setItems(await presaleLocationsApi.list());
        } catch (err) {
            toast.error(err.message || "Не удалось загрузить точки");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        load();
    }, []);

    return (
        <div>
            <PageHeader
                title="Точки предзаказа"
                description="Места, где клиент может забрать предзаказ"
                actions={
                    <Button variant="solid" onClick={() => setEditing({ isNew: true })}>
                        + Новая точка
                    </Button>
                }
            />

            {loading ? (
                <Spinner label="Загрузка…" />
            ) : items.length === 0 ? (
                <EmptyState
                    icon="◎"
                    title="Точек пока нет"
                    description="Добавьте первую точку, чтобы использовать её в предзаказах."
                />
            ) : (
                <div className="loc-grid">
                    {items.map((l) => (
                        <div key={l.id} className="loc-card glass">
                            <div className="loc-card__media">
                                {l.photoUrl ? (
                                    <img src={l.photoUrl} alt="" loading="lazy" />
                                ) : (
                                    <div className="loc-card__placeholder">Нет фото</div>
                                )}
                            </div>
                            <div className="loc-card__body">
                                <h3>{l.name}</h3>
                                {l.description && <p>{l.description}</p>}
                            </div>
                            <div className="loc-card__actions">
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() => setEditing({ ...l, isNew: false })}
                                >
                                    Изменить
                                </Button>
                                <Button
                                    variant="danger"
                                    size="sm"
                                    onClick={() => setConfirmRemove(l)}
                                >
                                    Удалить
                                </Button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {editing && (
                <LocationForm
                    initial={editing}
                    onClose={() => setEditing(null)}
                    onSaved={async () => {
                        setEditing(null);
                        await load();
                        toast.success("Сохранено");
                    }}
                />
            )}

            {confirmRemove && (
                <ConfirmDialog
                    title="Удалить точку?"
                    message={`«${confirmRemove.name}» будет удалена.`}
                    confirmLabel="Удалить"
                    onClose={() => setConfirmRemove(null)}
                    onConfirm={async () => {
                        try {
                            await presaleLocationsApi.remove(confirmRemove.id);
                            setConfirmRemove(null);
                            await load();
                            toast.success("Удалено");
                        } catch (err) {
                            toast.error(err.message || "Не удалось удалить");
                        }
                    }}
                />
            )}
        </div>
    );
}

function LocationForm({ initial, onClose, onSaved }) {
    const toast = useToast();
    const [name, setName] = useState(initial.name || "");
    const [description, setDescription] = useState(initial.description || "");
    const [file, setFile] = useState(null);
    const [preview, setPreview] = useState(initial.photoUrl || "");
    const [busy, setBusy] = useState(false);
    const fileRef = useRef(null);

    function pickFile(f) {
        if (!f) return;
        setFile(f);
        setPreview(URL.createObjectURL(f));
    }

    async function submit(e) {
        e.preventDefault();
        setBusy(true);
        try {
            const form = new FormData();
            form.append("name", name.trim());
            if (description.trim()) form.append("description", description.trim());
            if (file) form.append("file", file);

            if (initial.isNew) {
                await presaleLocationsApi.create(form);
            } else {
                await presaleLocationsApi.update(initial.id, form);
            }
            onSaved();
        } catch (err) {
            toast.error(err.message || "Не удалось сохранить");
        } finally {
            setBusy(false);
        }
    }

    return (
        <Modal
            title={initial.isNew ? "Новая точка предзаказа" : "Редактирование точки"}
            onClose={onClose}
        >
            <form className="pform" onSubmit={submit}>
                <Field label="Название" required>
                    <Input value={name} onChange={(e) => setName(e.target.value)} required autoFocus />
                </Field>
                <Field label="Описание">
                    <Textarea
                        rows={3}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                </Field>
                <Field label="Фото">
                    <div className="loc-form__photo">
                        {preview ? (
                            <img src={preview} alt="" />
                        ) : (
                            <div className="loc-form__placeholder">Нет изображения</div>
                        )}
                        <input
                            ref={fileRef}
                            type="file"
                            accept="image/*"
                            hidden
                            onChange={(e) => pickFile(e.target.files?.[0])}
                        />
                        <Button
                            type="button"
                            variant="subtle"
                            size="sm"
                            onClick={() => fileRef.current?.click()}
                        >
                            {preview ? "Заменить" : "Загрузить"}
                        </Button>
                    </div>
                </Field>

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