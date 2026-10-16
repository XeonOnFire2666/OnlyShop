import { useState } from "react";
import { Input } from "./Field";
import Button from "./Button";
import "./VariantEditor.css";

export default function VariantEditor({ variants, onAddVariant, onAdjustStock, disabled }) {
    const [size, setSize] = useState("");
    const [stock, setStock] = useState("");
    const [adjusting, setAdjusting] = useState(null);

    async function handleAdd(e) {
        e.preventDefault();
        if (!size.trim()) return;
        await onAddVariant({ size: size.trim(), stockQuantity: Number(stock) || 0 });
        setSize("");
        setStock("");
    }

    async function handleDelta(variantId, delta) {
        setAdjusting(variantId);
        try {
            await onAdjustStock(variantId, delta);
        } finally {
            setAdjusting(null);
        }
    }

    return (
        <div className="variant-editor">
            {variants.length > 0 && (
                <ul className="variant-editor__list">
                    {variants.map((v) => (
                        <li key={v.id} className="variant-editor__row">
                            <span className="variant-editor__size">{v.size}</span>
                            <div className="variant-editor__stock-control">
                                <button
                                    type="button"
                                    className="variant-editor__step"
                                    onClick={() => handleDelta(v.id, -1)}
                                    disabled={disabled || v.stockQuantity === 0 || adjusting === v.id}
                                    aria-label="Уменьшить количество"
                                >
                                    −
                                </button>
                                <span className={`variant-editor__qty${v.stockQuantity === 0 ? " variant-editor__qty--zero" : ""}`}>
                                    {v.stockQuantity}
                                </span>
                                <button
                                    type="button"
                                    className="variant-editor__step"
                                    onClick={() => handleDelta(v.id, 1)}
                                    disabled={disabled || adjusting === v.id}
                                    aria-label="Увеличить количество"
                                >
                                    +
                                </button>
                            </div>
                        </li>
                    ))}
                </ul>
            )}

            {!disabled && (
                <form className="variant-editor__add" onSubmit={handleAdd}>
                    <Input
                        placeholder="Размер, напр. M"
                        value={size}
                        onChange={(e) => setSize(e.target.value)}
                    />
                    <Input
                        placeholder="Остаток"
                        type="number"
                        min="0"
                        value={stock}
                        onChange={(e) => setStock(e.target.value)}
                    />
                    <Button type="submit" variant="subtle" size="sm">
                        Добавить размер
                    </Button>
                </form>
            )}
        </div>
    );
}