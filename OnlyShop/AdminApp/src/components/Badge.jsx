import "./Badge.css";

const TONE_CLASS = {
    neutral: "badge--neutral",
    success: "badge--success",
    danger: "badge--danger",
    warning: "badge--warning",
    primary: "badge--primary",
};

export default function Badge({ children, tone = "neutral" }) {
    return <span className={`badge ${TONE_CLASS[tone] || TONE_CLASS.neutral}`}>{children}</span>;
}