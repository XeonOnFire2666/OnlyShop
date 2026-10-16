import { useCallback, useEffect, useRef, useState } from "react";
import PageHeader from "../components/PageHeader";
import Button from "../components/Button";
import Modal from "../components/Modal";
import Badge from "../components/Badge";
import { Field, Input, Select, Textarea } from "../components/Field";
import { EmptyState, Spinner } from "../components/Feedback";
import { ordersApi } from "../api/orders";
import { useToast } from "../components/Toast";
import {
    ORDER_STATUSES,
    ORDER_TRANSITIONS,
    labelFor,
    toneFor,
    formatDate,
} from "../lib/enums";
import "./OrdersPage.css";

const EMPTY = { status: "", search: "", page: 0, pageSize: 20 };

export default function OrdersPage() {
    const toast = useToast();
    const [filters, setFilters] = useState(EMPTY);
    const [debounced, setDebounced] = useState("");
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [activeId, setActiveId] = useState(null);
    const reqRef = useRef(0);

    useEffect(() => {
        const t = setTimeout(() => setDebounced(filters.search), 300);
        return () => clearTimeout(t);
    }, [filters.search]);

    const load = useCallback(async () => {
        const id = ++reqRef.current;
        setLoading(true);
        try {
            const data = await ordersApi.search({
                status: filters.status || undefined,
                search: debounced || undefined,
                page: filters.page + 1,
                pageSize: filters.pageSize,
            });
            if (id !== reqRef.current) return;
            setItems(Array.isArray(data) ? data : data.items || []);
        } catch (err) {
            toast.error(err.message || "Не удалось загрузить заказы");
        } finally {
            if (id === reqRef.current) setLoading(false);
        }
    }, [debounced, filters.status, filters.page, filters.pageSize, toast]);

    useEffect(() => {
        load();
    }, [load]);

    return (
        <div>
            <PageHeader title="Заказы" description="Статусы, клиенты, состав заказа" />

            <div className="orders-filters glass">
                <Input
                    placeholder="Поиск по имени, телефону…"
                    value={filters.search}
                    onChange={(e) => setFilters({ ...filters, search: e.target.value, page: 0 })}
                />
                <Select
                    value={filters.status}
                    onChange={(e) => setFilters({ ...filters, status: e.target.value, page: 0 })}
                >
                    <option value="">Все статусы</option>
                    {ORDER_STATUSES.map((s) => (
                        <option key={s.value} value={s.value}>{s.label}</option>
                    ))}
                </Select>
            </div>

            {loading ? (
                <Spinner label="Загрузка заказов…" />
            ) : items.length === 0 ? (
                <EmptyState
                    icon="▤"
                    title="Заказов нет"
                    description="Как только клиент оформит заказ, он появится здесь."
                />
            ) : (
                <div className="orders-table glass">
                    <div className="orders-table__head">
                        <span>Клиент</span>
                        <span>Статус</span>
                        <span>Сумма</span>
                        <span>Создан</span>
                        <span />
                    </div>
                    <div className="orders-table__body">
                        {items.map((o) => (
                            <button
                                key={o.id}
                                className="orders-table__row"
                                onClick={() => setActiveId(o.id)}
                                type="button"
                            >
                                <span className="orders-table__cell orders-table__cell--strong">
                                    {o.customerName || "—"}
                                    {o.customerPhone && (
                                        <small className="orders-table__sub">{o.customerPhone}</small>
                                    )}
                                </span>
                                <span className="orders-table__cell">
                                    <Badge tone={toneFor(ORDER_STATUSES, o.status)}>
                                        {labelFor(ORDER_STATUSES, o.status)}
                                    </Badge>
                                </span>
                                <span className="orders-table__cell orders-table__cell--mono">
                                    {o.total != null
                                        ? `${o.total} ${o.currency === "Rub" ? "RUB" : "BYN"}`
                                        : "—"}
                                </span>
                                <span className="orders-table__cell orders-table__cell--muted">
                                    {formatDate(o.createdAt)}
                                </span>
                                <span className="orders-table__cell orders-table__cell--arrow">→</span>
                            </button>
                        ))}
                    </div>
                </div>
            )}

            {activeId && (
                <OrderDetailModal
                    id={activeId}
                    onClose={() => setActiveId(null)}
                    onChanged={load}
                />
            )}
        </div>
    );
}

function OrderDetailModal({ id, onClose, onChanged }) {
    const toast = useToast();
    const [order, setOrder] = useState(null);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [note, setNote] = useState("");

    useEffect(() => {
        let alive = true;
        ordersApi
            .get(id)
            .then((o) => alive && setOrder(o))
            .catch((err) => toast.error(err.message || "Не удалось загрузить заказ"))
            .finally(() => alive && setLoading(false));
        return () => {
            alive = false;
        };
    }, [id, toast]);

    async function changeStatus(status) {
        setBusy(true);
        try {
            await ordersApi.changeStatus(id, { status, note: note.trim() || null });
            const fresh = await ordersApi.get(id);
            setOrder(fresh);
            setNote("");
            onChanged?.();
            toast.success("Статус обновлён");
        } catch (err) {
            toast.error(err.message || "Не удалось изменить статус");
        } finally {
            setBusy(false);
        }
    }

    const nextStatuses = order ? ORDER_TRANSITIONS[order.status] || [] : [];

    return (
        <Modal title={`Заказ ${id.slice(0, 8)}`} onClose={onClose} width={680}>
            {loading || !order ? (
                <Spinner label="Загрузка…" />
            ) : (
                <div className="odetail">
                    <div className="odetail__grid">
                        <div>
                            <span className="odetail__label">Клиент</span>
                            <span className="odetail__value">{order.customerName}</span>
                        </div>
                        {order.customerPhone && (
                            <div>
                                <span className="odetail__label">Телефон</span>
                                <span className="odetail__value">{order.customerPhone}</span>
                            </div>
                        )}
                        <div>
                            <span className="odetail__label">Доставка</span>
                            <span className="odetail__value">
                                {order.deliveryMethod === "Pickup" ? "Самовывоз" : order.deliveryAddress || "—"}
                            </span>
                        </div>
                        <div>
                            <span className="odetail__label">Оплата</span>
                            <span className="odetail__value">{order.paymentMethod}</span>
                        </div>
                        <div>
                            <span className="odetail__label">Статус</span>
                            <Badge tone={toneFor(ORDER_STATUSES, order.status)}>
                                {labelFor(ORDER_STATUSES, order.status)}
                            </Badge>
                        </div>
                        <div>
                            <span className="odetail__label">Создан</span>
                            <span className="odetail__value">{formatDate(order.createdAt)}</span>
                        </div>
                    </div>

                    {order.comment && (
                        <div className="odetail__comment">
                            <span className="odetail__label">Комментарий</span>
                            <p>{order.comment}</p>
                        </div>
                    )}

                    <div className="odetail__items">
                        <h4>Состав заказа</h4>
                        <div className="odetail__items-list">
                            {(order.items || []).map((it) => (
                                <div key={it.id || `${it.productId}-${it.size}`} className="odetail__item">
                                    <span className="odetail__item-name">
                                        {it.productName}
                                        <small> · {it.size}</small>
                                    </span>
                                    <span className="odetail__item-qty">×{it.quantity}</span>
                                    <span className="odetail__item-price">
                                        {it.unitPrice} {order.currency === "Rub" ? "RUB" : "BYN"}
                                    </span>
                                </div>
                            ))}
                        </div>
                        <div className="odetail__total">
                            <span>Итого</span>
                            <span>
                                {order.total} {order.currency === "Rub" ? "RUB" : "BYN"}
                            </span>
                        </div>
                        {order.bonusesSpent > 0 && (
                            <div className="odetail__bonuses">Списано бонусов: {order.bonusesSpent}</div>
                        )}
                    </div>

                    {nextStatuses.length > 0 && (
                        <div className="odetail__actions">
                            <Field label="Заметка (необязательно)">
                                <Textarea
                                    rows={2}
                                    value={note}
                                    onChange={(e) => setNote(e.target.value)}
                                    placeholder="Комментарий к смене статуса"
                                />
                            </Field>
                            <div className="odetail__buttons">
                                {nextStatuses.map((s) => {
                                    const tone =
                                        s === "Canceled"
                                            ? "danger"
                                            : s === "Completed" || s === "Paid"
                                                ? "solid"
                                                : "subtle";
                                    return (
                                        <Button
                                            key={s}
                                            variant={tone}
                                            size="sm"
                                            loading={busy}
                                            onClick={() => changeStatus(s)}
                                        >
                                            {labelFor(ORDER_STATUSES, s)}
                                        </Button>
                                    );
                                })}
                            </div>
                        </div>
                    )}
                </div>
            )}
        </Modal>
    );
}