import { useCallback, useEffect, useRef, useState } from "react";
import PageHeader from "../components/PageHeader";
import Button from "../components/Button";
import Modal from "../components/Modal";
import Badge from "../components/Badge";
import { Field, Input, Select, Textarea } from "../components/Field";
import { ConfirmDialog, EmptyState, Spinner } from "../components/Feedback";
import ProductCard from "../components/ProductCard";
import ProductFilters from "../components/ProductFilters";
import VariantEditor from "../components/VariantEditor";
import { productsApi, categoriesApi, presaleLocationsApi } from "../api/products";
import { useToast } from "../components/Toast";
import {
    PRODUCT_CONDITIONS,
    PRODUCT_STATUSES,
    formatDate,
    labelFor,
    toneFor,
} from "../lib/enums";
import "./ProductsPage.css";

const EMPTY_FILTERS = {
    search: "",
    categoryId: "",
    status: "",
    availability: "",
    page: 0,
    pageSize: 24,
};

export default function ProductsPage() {
    const toast = useToast();
    const [filters, setFilters] = useState(EMPTY_FILTERS);
    const [debouncedSearch, setDebouncedSearch] = useState("");
    const [categories, setCategories] = useState([]);
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [createOpen, setCreateOpen] = useState(false);
    const [activeId, setActiveId] = useState(null);
    const reqRef = useRef(0);

    useEffect(() => {
        const t = setTimeout(() => setDebouncedSearch(filters.search), 300);
        return () => clearTimeout(t);
    }, [filters.search]);

    useEffect(() => {
        categoriesApi.list().then(setCategories).catch(() => { });
    }, []);

    const load = useCallback(async () => {
        const id = ++reqRef.current;
        setLoading(true);
        try {
            const data = await productsApi.search({
                search: debouncedSearch || undefined,
                categoryId: filters.categoryId || undefined,
                status: filters.status || undefined,
                availability: filters.availability || undefined,
                page: filters.page + 1,
                pageSize: filters.pageSize,
            });
            if (id !== reqRef.current) return;
            setItems(Array.isArray(data) ? data : data.items || []);
        } catch (err) {
            toast.error(err.message || "Не удалось загрузить товары");
        } finally {
            if (id === reqRef.current) setLoading(false);
        }
    }, [
        debouncedSearch,
        filters.categoryId,
        filters.status,
        filters.availability,
        filters.page,
        filters.pageSize,
        toast,
    ]);

    useEffect(() => {
        load();
    }, [load]);

    function mapForCard(p) {
        return {
            id: p.id,
            name: p.name,
            brand: p.brand,
            status: p.status,
            availability: p.availability,
            price: { byn: p.priceByn, rub: p.priceRub },
            photoUrl: p.coverUrl,
            totalStock: p.totalStock,
        };
    }

    return (
        <div>
            <PageHeader
                title="Товары"
                description="Каталог, наличие размеров, цены и публикация"
                actions={
                    <Button variant="solid" onClick={() => setCreateOpen(true)}>
                        + Новый товар
                    </Button>
                }
            />

            <ProductFilters filters={filters} onChange={setFilters} categories={categories} />

            {loading ? (
                <Spinner label="Загрузка товаров…" />
            ) : items.length === 0 ? (
                <EmptyState
                    icon="◧"
                    title="Товаров нет"
                    description="Добавьте первый товар — он сразу появится в списке."
                    action={
                        <Button variant="solid" onClick={() => setCreateOpen(true)}>
                            Добавить товар
                        </Button>
                    }
                />
            ) : (
                <div className="product-grid">
                    {items.map((p) => (
                        <ProductCard
                            key={p.id}
                            product={mapForCard(p)}
                            onOpen={() => setActiveId(p.id)}
                        />
                    ))}
                </div>
            )}

            {createOpen && (
                <ProductCreateModal
                    categories={categories}
                    onClose={() => setCreateOpen(false)}
                    onCreated={() => {
                        setCreateOpen(false);
                        load();
                        toast.success("Товар создан");
                    }}
                />
            )}

            {activeId && (
                <ProductDetailModal
                    id={activeId}
                    categories={categories}
                    onClose={() => setActiveId(null)}
                    onChanged={load}
                />
            )}
        </div>
    );
}

function ProductCreateModal({ categories, onClose, onCreated }) {
    const toast = useToast();
    const [form, setForm] = useState({
        name: "",
        categoryId: categories[0]?.id || "",
        priceByn: "",
        priceRub: "",
        condition: "New",
        brand: "",
        description: "",
        variants: [],
    });
    const [draft, setDraft] = useState({ size: "", stockQuantity: "" });
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");

    const set = (patch) => setForm((f) => ({ ...f, ...patch }));

    function addVariant() {
        const size = draft.size.trim();
        if (!size) return;
        if (form.variants.some((v) => v.size.toLowerCase() === size.toLowerCase())) {
            toast.error("Такой размер уже есть");
            return;
        }
        set({
            variants: [
                ...form.variants,
                { size, stockQuantity: Number(draft.stockQuantity) || 0 },
            ],
        });
        setDraft({ size: "", stockQuantity: "" });
    }

    async function submit(e) {
        e.preventDefault();
        setError("");
        if (!form.categoryId) return setError("Выберите категорию");
        setSaving(true);
        try {
            await productsApi.create({
                name: form.name.trim(),
                categoryId: form.categoryId,
                priceByn: Number(form.priceByn),
                priceRub: Number(form.priceRub),
                condition: form.condition,
                brand: form.brand.trim() || null,
                description: form.description.trim() || null,
                variants: form.variants,
            });
            onCreated();
        } catch (err) {
            setError(err.message || "Не удалось создать товар");
        } finally {
            setSaving(false);
        }
    }

    return (
        <Modal title="Новый товар" onClose={onClose} width={640}>
            <form className="pform" onSubmit={submit}>
                <Field label="Название" required>
                    <Input
                        value={form.name}
                        onChange={(e) => set({ name: e.target.value })}
                        required
                        autoFocus
                    />
                </Field>

                <div className="pform__row">
                    <Field label="Категория" required>
                        <Select
                            value={form.categoryId}
                            onChange={(e) => set({ categoryId: e.target.value })}
                            required
                        >
                            <option value="">— выберите —</option>
                            {categories.map((c) => (
                                <option key={c.id} value={c.id}>
                                    {c.name}
                                </option>
                            ))}
                        </Select>
                    </Field>
                    <Field label="Состояние">
                        <Select
                            value={form.condition}
                            onChange={(e) => set({ condition: e.target.value })}
                        >
                            {PRODUCT_CONDITIONS.map((c) => (
                                <option key={c.value} value={c.value}>
                                    {c.label}
                                </option>
                            ))}
                        </Select>
                    </Field>
                </div>

                <div className="pform__row">
                    <Field label="Цена, BYN" required>
                        <Input
                            type="number"
                            step="0.01"
                            min="0"
                            value={form.priceByn}
                            onChange={(e) => set({ priceByn: e.target.value })}
                            required
                        />
                    </Field>
                    <Field label="Цена, RUB" required>
                        <Input
                            type="number"
                            step="0.01"
                            min="0"
                            value={form.priceRub}
                            onChange={(e) => set({ priceRub: e.target.value })}
                            required
                        />
                    </Field>
                </div>

                <Field label="Бренд">
                    <Input value={form.brand} onChange={(e) => set({ brand: e.target.value })} />
                </Field>

                <Field label="Описание">
                    <Textarea
                        value={form.description}
                        onChange={(e) => set({ description: e.target.value })}
                        rows={3}
                    />
                </Field>

                <Field label="Размеры и остатки">
                    <div className="pform__variants">
                        {form.variants.length > 0 && (
                            <ul className="pform__variant-list">
                                {form.variants.map((v, i) => (
                                    <li key={i} className="pform__variant">
                                        <span className="pform__variant-size">{v.size}</span>
                                        <span className="pform__variant-stock">{v.stockQuantity} шт.</span>
                                        <button
                                            type="button"
                                            className="pform__variant-remove"
                                            onClick={() =>
                                                set({ variants: form.variants.filter((_, idx) => idx !== i) })
                                            }
                                            aria-label="Удалить размер"
                                        >
                                            ✕
                                        </button>
                                    </li>
                                ))}
                            </ul>
                        )}
                        <div className="pform__variant-add">
                            <Input
                                placeholder="Размер, напр. M"
                                value={draft.size}
                                onChange={(e) => setDraft((d) => ({ ...d, size: e.target.value }))}
                            />
                            <Input
                                placeholder="Остаток"
                                type="number"
                                min="0"
                                value={draft.stockQuantity}
                                onChange={(e) =>
                                    setDraft((d) => ({ ...d, stockQuantity: e.target.value }))
                                }
                            />
                            <Button type="button" variant="subtle" size="sm" onClick={addVariant}>
                                Добавить
                            </Button>
                        </div>
                    </div>
                </Field>

                {error && <div className="pform__error">{error}</div>}

                <div className="pform__actions">
                    <Button variant="ghost" type="button" onClick={onClose} disabled={saving}>
                        Отмена
                    </Button>
                    <Button variant="solid" type="submit" loading={saving}>
                        Создать
                    </Button>
                </div>
            </form>
        </Modal>
    );
}

const TABS = [
    { id: "details", label: "Основное" },
    { id: "price", label: "Цена и наличие" },
    { id: "variants", label: "Размеры" },
    { id: "photos", label: "Фото" },
    { id: "presale", label: "Предзаказ" },
];

function ProductDetailModal({ id, categories, onClose, onChanged }) {
    const toast = useToast();
    const [product, setProduct] = useState(null);
    const [locations, setLocations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [tab, setTab] = useState("details");
    const [confirmArchive, setConfirmArchive] = useState(false);
    const [busy, setBusy] = useState(false);

    const reload = useCallback(async () => {
        try {
            const p = await productsApi.get(id);
            setProduct(p);
        } catch (err) {
            toast.error(err.message || "Не удалось загрузить товар");
        }
    }, [id, toast]);

    useEffect(() => {
        let alive = true;
        Promise.all([productsApi.get(id), presaleLocationsApi.list().catch(() => [])])
            .then(([p, l]) => {
                if (!alive) return;
                setProduct(p);
                setLocations(l);
            })
            .catch((err) => toast.error(err.message || "Ошибка загрузки"))
            .finally(() => alive && setLoading(false));
        return () => {
            alive = false;
        };
    }, [id, toast]);

    async function run(fn, okMessage) {
        setBusy(true);
        try {
            await fn();
            await reload();
            onChanged?.();
            if (okMessage) toast.success(okMessage);
        } catch (err) {
            toast.error(err.message || "Ошибка");
        } finally {
            setBusy(false);
        }
    }

    return (
        <>
            <Modal title={product?.name || "Товар"} onClose={onClose} width={720}>
                {loading || !product ? (
                    <Spinner label="Загрузка…" />
                ) : (
                    <div className="pdetail">
                        <div className="pdetail__tabs">
                            {TABS.map((t) => (
                                <button
                                    key={t.id}
                                    className={`pdetail__tab${tab === t.id ? " pdetail__tab--active" : ""}`}
                                    onClick={() => setTab(t.id)}
                                    type="button"
                                >
                                    {t.label}
                                </button>
                            ))}
                        </div>

                        {tab === "details" && (
                            <DetailsTab
                                product={product}
                                categories={categories}
                                busy={busy}
                                onSave={(payload) =>
                                    run(() => productsApi.updateDetails(id, payload), "Сохранено")
                                }
                            />
                        )}

                        {tab === "price" && (
                            <PriceTab
                                product={product}
                                busy={busy}
                                onSave={(payload) =>
                                    run(() => productsApi.changePrice(id, payload), "Цена обновлена")
                                }
                                onInStock={() => run(() => productsApi.setInStock(id), "Переведено в наличии")}
                            />
                        )}

                        {tab === "variants" && (
                            <VariantsTab
                                product={product}
                                busy={busy}
                                onAdd={(payload) =>
                                    run(() => productsApi.addVariant(id, payload), "Размер добавлен")
                                }
                                onAdjust={(variantId, delta) =>
                                    run(() => productsApi.adjustStock(id, variantId, delta))
                                }
                            />
                        )}

                        {tab === "photos" && (
                            <PhotosTab
                                product={product}
                                busy={busy}
                                onUpload={(file) =>
                                    run(() => productsApi.addPhoto(id, file), "Фото загружено")
                                }
                            />
                        )}

                        {tab === "presale" && (
                            <PresaleTab
                                product={product}
                                locations={locations}
                                busy={busy}
                                onSet={(payload) =>
                                    run(() => productsApi.setPresale(id, payload), "Предзаказ настроен")
                                }
                            />
                        )}

                        <div className="pdetail__footer">
                            <div className="pdetail__status">
                                <Badge tone={toneFor(PRODUCT_STATUSES, product.status)}>
                                    {labelFor(PRODUCT_STATUSES, product.status)}
                                </Badge>
                                <span className="pdetail__updated">
                                    Обновлён {formatDate(product.updatedAt)}
                                </span>
                            </div>

                            <div className="pdetail__actions">
                                {product.status !== "Published" && (
                                    <Button
                                        variant="solid"
                                        size="sm"
                                        disabled={busy}
                                        onClick={() =>
                                            run(() => productsApi.publish(id), "Опубликован")
                                        }
                                    >
                                        Опубликовать
                                    </Button>
                                )}
                                {product.status !== "Archived" && (
                                    <Button
                                        variant="danger"
                                        size="sm"
                                        disabled={busy}
                                        onClick={() => setConfirmArchive(true)}
                                    >
                                        В архив
                                    </Button>
                                )}
                            </div>
                        </div>
                    </div>
                )}
            </Modal>

            {confirmArchive && (
                <ConfirmDialog
                    title="В архив?"
                    message="Товар скроется из каталога, но останется в системе. Можно вернуть публикацией."
                    confirmLabel="В архив"
                    onClose={() => setConfirmArchive(false)}
                    loading={busy}
                    onConfirm={async () => {
                        await run(() => productsApi.archive(id), "В архиве");
                        setConfirmArchive(false);
                    }}
                />
            )}
        </>
    );
}

function DetailsTab({ product, busy, onSave }) {
    const [form, setForm] = useState({
        name: product.name || "",
        brand: product.brand || "",
        description: product.description || "",
        condition: product.condition || "New",
    });

    return (
        <form
            className="pform"
            onSubmit={(e) => {
                e.preventDefault();
                onSave({
                    name: form.name.trim(),
                    brand: form.brand.trim() || null,
                    description: form.description.trim() || null,
                    condition: form.condition,
                });
            }}
        >
            <Field label="Название" required>
                <Input
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    required
                />
            </Field>
            <div className="pform__row">
                <Field label="Бренд">
                    <Input
                        value={form.brand}
                        onChange={(e) => setForm({ ...form, brand: e.target.value })}
                    />
                </Field>
                <Field label="Состояние">
                    <Select
                        value={form.condition}
                        onChange={(e) => setForm({ ...form, condition: e.target.value })}
                    >
                        {PRODUCT_CONDITIONS.map((c) => (
                            <option key={c.value} value={c.value}>
                                {c.label}
                            </option>
                        ))}
                    </Select>
                </Field>
            </div>
            <Field label="Описание">
                <Textarea
                    rows={4}
                    value={form.description}
                    onChange={(e) => setForm({ ...form, description: e.target.value })}
                />
            </Field>
            <div className="pform__actions">
                <Button variant="solid" type="submit" loading={busy}>
                    Сохранить
                </Button>
            </div>
        </form>
    );
}

function PriceTab({ product, busy, onSave, onInStock }) {
    const [byn, setByn] = useState(product.priceByn ?? "");
    const [rub, setRub] = useState(product.priceRub ?? "");

    return (
        <form
            className="pform"
            onSubmit={(e) => {
                e.preventDefault();
                onSave({ priceByn: Number(byn), priceRub: Number(rub) });
            }}
        >
            <div className="pform__row">
                <Field label="Цена, BYN" required>
                    <Input
                        type="number"
                        step="0.01"
                        min="0"
                        value={byn}
                        onChange={(e) => setByn(e.target.value)}
                        required
                    />
                </Field>
                <Field label="Цена, RUB" required>
                    <Input
                        type="number"
                        step="0.01"
                        min="0"
                        value={rub}
                        onChange={(e) => setRub(e.target.value)}
                        required
                    />
                </Field>
            </div>

            <div className="pform__inline">
                <div className="pform__hint">
                    Текущее наличие:&nbsp;
                    <strong>
                        {product.availability === "InStock" ? "В наличии" : "Предзаказ"}
                    </strong>
                </div>
                {product.availability === "Presale" && (
                    <Button variant="ghost" size="sm" type="button" onClick={onInStock} disabled={busy}>
                        Вернуть в наличие
                    </Button>
                )}
            </div>

            <div className="pform__actions">
                <Button variant="solid" type="submit" loading={busy}>
                    Обновить цену
                </Button>
            </div>
        </form>
    );
}

function VariantsTab({ product, busy, onAdd, onAdjust }) {
    return (
        <div className="pform">
            <VariantEditor
                variants={product.variants || []}
                onAddVariant={onAdd}
                onAdjustStock={onAdjust}
                disabled={busy}
            />
        </div>
    );
}

function PhotosTab({ product, busy, onUpload }) {
    const fileRef = useRef(null);

    return (
        <div className="pform">
            {(product.photos || []).length === 0 ? (
                <div className="pform__hint">Фотографий пока нет.</div>
            ) : (
                <div className="pform__photos">
                    {product.photos.map((p) => (
                        <div key={p.id} className="pform__photo">
                            <img src={p.url} alt="" loading="lazy" />
                        </div>
                    ))}
                </div>
            )}

            <input
                ref={fileRef}
                type="file"
                accept="image/*"
                hidden
                onChange={(e) => {
                    const file = e.target.files?.[0];
                    if (file) onUpload(file);
                    e.target.value = "";
                }}
            />
            <div className="pform__actions">
                <Button
                    type="button"
                    variant="subtle"
                    loading={busy}
                    onClick={() => fileRef.current?.click()}
                >
                    Загрузить фото
                </Button>
            </div>
        </div>
    );
}

function PresaleTab({ product, locations, busy, onSet }) {
    const [locationId, setLocationId] = useState(product.presaleLocationId || "");
    const [closesAt, setClosesAt] = useState(
        product.presaleClosesAt ? product.presaleClosesAt.slice(0, 16) : ""
    );

    return (
        <form
            className="pform"
            onSubmit={(e) => {
                e.preventDefault();
                if (!locationId || !closesAt) return;
                onSet({ locationId, closesAt: new Date(closesAt).toISOString() });
            }}
        >
            <Field label="Точка предзаказа" required>
                <Select value={locationId} onChange={(e) => setLocationId(e.target.value)} required>
                    <option value="">— выберите точку —</option>
                    {locations.map((l) => (
                        <option key={l.id} value={l.id}>
                            {l.name}
                        </option>
                    ))}
                </Select>
            </Field>
            <Field label="Закрытие предзаказа" required>
                <Input
                    type="datetime-local"
                    value={closesAt}
                    onChange={(e) => setClosesAt(e.target.value)}
                    required
                />
            </Field>
            <div className="pform__actions">
                <Button variant="solid" type="submit" loading={busy}>
                    Сохранить предзаказ
                </Button>
            </div>
        </form>
    );
}