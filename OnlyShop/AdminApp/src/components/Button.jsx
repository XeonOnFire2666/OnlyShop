import "./Button.css";

const VARIANT_CLASS = {
    glass: "btn btn--glass glass glass-btn",
    solid: "btn btn--solid",
    ghost: "btn btn--ghost",
    danger: "btn btn--danger",
    subtle: "btn btn--subtle",
};

export default function Button({
    children,
    variant = "glass",
    size = "md",
    icon,
    loading = false,
    className = "",
    ...rest
}) {
    const classes = [VARIANT_CLASS[variant] || VARIANT_CLASS.glass, `btn--${size}`, className]
        .filter(Boolean)
        .join(" ");

    return (
        <button className={classes} disabled={loading || rest.disabled} {...rest}>
            {loading ? <span className="btn__spinner" aria-hidden="true" /> : icon}
            <span>{children}</span>
        </button>
    );
}