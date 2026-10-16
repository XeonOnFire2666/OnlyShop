import Badge from "./Badge";
import { labelFor, toneFor, formatPrice, PRODUCT_STATUSES, PRODUCT_AVAILABILITY } from "../lib/enums";
import "./ProductCard.css";

export default function ProductCard({ product, onOpen }) {
    const cover = product.photos?.[0]?.url || product.photoUrl;
    const totalStock = product.variants?.reduce((sum, v) => sum + v.stockQuantity, 0) ?? product.totalStock ?? 0;

    return (
        <button className="product-card" onClick={() => onOpen(product)}>
            <div className="product-card__media">
                {cover ? (
                    <img src={cover} alt={product.name} loading="lazy" />
                ) : (
                    <div className="product-card__placeholder">Нет фото</div>
                )}
                <div className="product-card__badges">
                    <Badge tone={toneFor(PRODUCT_STATUSES, product.status)}>{labelFor(PRODUCT_STATUSES, product.status)}</Badge>
                    {product.availability === "Presale" && <Badge tone="primary">Предзаказ</Badge>}
                </div>
            </div>

            <div className="product-card__body">
                <div className="product-card__top">
                    <h3>{product.name}</h3>
                    <span className="product-card__price">{formatPrice(product.price?.byn, product.price?.rub)}</span>
                </div>

                {product.brand && <p className="product-card__brand">{product.brand}</p>}

                <div className="product-card__meta">
                    <span>{labelFor(PRODUCT_AVAILABILITY, product.availability)}</span>
                    <span className={totalStock === 0 ? "product-card__stock product-card__stock--empty" : "product-card__stock"}>
                        {totalStock === 0 ? "Нет в наличии" : `${totalStock} шт.`}
                    </span>
                </div>
            </div>
        </button>
    );
}