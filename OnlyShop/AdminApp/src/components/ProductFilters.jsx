import { Input, Select } from "./Field";
import { PRODUCT_STATUSES, PRODUCT_AVAILABILITY } from "../lib/enums";
import "./ProductFilters.css";

export default function ProductFilters({ filters, onChange, categories }) {
    function update(patch) {
        onChange({ ...filters, ...patch, page: 0 });
    }

    return (
        <div className="product-filters glass">
            <Input
                placeholder="Поиск по названию…"
                value={filters.search}
                onChange={(e) => update({ search: e.target.value })}
                className="product-filters__search"
            />

            <Select value={filters.categoryId} onChange={(e) => update({ categoryId: e.target.value })}>
                <option value="">Все категории</option>
                {categories.map((c) => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                ))}
            </Select>

            <Select value={filters.status} onChange={(e) => update({ status: e.target.value })}>
                <option value="">Любой статус</option>
                {PRODUCT_STATUSES.map((s) => (
                    <option key={s.value} value={s.value}>{s.label}</option>
                ))}
            </Select>

            <Select value={filters.availability} onChange={(e) => update({ availability: e.target.value })}>
                <option value="">Наличие и предзаказ</option>
                {PRODUCT_AVAILABILITY.map((a) => (
                    <option key={a.value} value={a.value}>{a.label}</option>
                ))}
            </Select>
        </div>
    );
}