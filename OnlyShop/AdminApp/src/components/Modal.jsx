import { useEffect } from "react";
import { createPortal } from "react-dom";
import "./Modal.css";

export default function Modal({ title, onClose, children, width = 520 }) {
    useEffect(() => {
        function handleKey(e) {
            if (e.key === "Escape") onClose();
        }
        document.addEventListener("keydown", handleKey);
        return () => document.removeEventListener("keydown", handleKey);
    }, [onClose]);

    return createPortal(
        <div className="modal-backdrop" onMouseDown={onClose}>
            <div
                className="modal glass"
                style={{ maxWidth: width }}
                onMouseDown={(e) => e.stopPropagation()}
                role="dialog"
                aria-modal="true"
                aria-label={title}
            >
                <div className="modal__header">
                    <h3>{title}</h3>
                    <button className="modal__close" onClick={onClose} aria-label="Закрыть">✕</button>
                </div>
                <div className="modal__body">{children}</div>
            </div>
        </div>,
        document.body
    );
}