import Modal from "./Modal";
import Button from "./Button";
import "./Feedback.css";

export function ConfirmDialog({ title, message, confirmLabel = "Подтвердить", tone = "danger", onConfirm, onClose, loading }) {
    return (
        <Modal title={title} onClose={onClose} width={420}>
            <p className="confirm__message">{message}</p>
            <div className="confirm__actions">
                <Button variant="ghost" onClick={onClose} disabled={loading}>Отмена</Button>
                <Button variant={tone} onClick={onConfirm} loading={loading}>{confirmLabel}</Button>
            </div>
        </Modal>
    );
}

export function Spinner({ label }) {
    return (
        <div className="spinner-block">
            <span className="spinner-block__dot" />
            {label && <span>{label}</span>}
        </div>
    );
}

export function EmptyState({ icon = "□", title, description, action }) {
    return (
        <div className="empty-state">
            <div className="empty-state__icon">{icon}</div>
            <h3>{title}</h3>
            {description && <p>{description}</p>}
            {action}
        </div>
    );
}