export const PRODUCT_CONDITIONS = [
    { value: "New", label: "Новое" },
    { value: "Perfect", label: "Идеальное" },
    { value: "Excellent", label: "Отличное" },
    { value: "Good", label: "Хорошее" },
    { value: "Fair", label: "Удовлетворительное" },
];

export const PRODUCT_STATUSES = [
    { value: "Draft", label: "Черновик", tone: "neutral" },
    { value: "Published", label: "Опубликован", tone: "success" },
    { value: "Archived", label: "В архиве", tone: "danger" },
];

export const PRODUCT_AVAILABILITY = [
    { value: "InStock", label: "В наличии" },
    { value: "Presale", label: "Предзаказ" },
];

export const ORDER_STATUSES = [
    { value: "Pending", label: "Новый", tone: "warning" },
    { value: "Confirmed", label: "Подтверждён", tone: "primary" },
    { value: "Paid", label: "Оплачен", tone: "success" },
    { value: "Delivering", label: "Доставляется", tone: "primary" },
    { value: "Completed", label: "Завершён", tone: "success" },
    { value: "Canceled", label: "Отменён", tone: "danger" },
];

export const ORDER_TRANSITIONS = {
    Pending: ["Confirmed", "Canceled"],
    Confirmed: ["Paid", "Canceled"],
    Paid: ["Delivering", "Canceled"],
    Delivering: ["Completed"],
    Completed: [],
    Canceled: [],
};

export function labelFor(list, value) {
    return list.find((item) => item.value === value)?.label || value;
}

export function toneFor(list, value) {
    return list.find((item) => item.value === value)?.tone || "neutral";
}

export function formatPrice(byn, rub) {
    const parts = [];
    if (byn !== undefined && byn !== null) parts.push(`${byn} BYN`);
    if (rub !== undefined && rub !== null) parts.push(`${rub} RUB`);
    return parts.join(" / ") || "—";
}

export function formatDate(value) {
    if (!value) return "—";
    const date = new Date(value);
    return date.toLocaleString("ru-RU", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" });
}