import "./Field.css";

export function Field({ label, hint, error, children, required }) {
    return (
        <label className="field">
            {label && (
                <span className="field__label">
                    {label}
                    {required && <span className="field__required">*</span>}
                </span>
            )}
            {children}
            {hint && !error && <span className="field__hint">{hint}</span>}
            {error && <span className="field__error">{error}</span>}
        </label>
    );
}

export function Input(props) {
    return <input className="field__control" {...props} />;
}

export function Textarea(props) {
    return <textarea className="field__control field__control--area" {...props} />;
}

export function Select({ children, ...rest }) {
    return (
        <select className="field__control field__control--select" {...rest}>
            {children}
        </select>
    );
}