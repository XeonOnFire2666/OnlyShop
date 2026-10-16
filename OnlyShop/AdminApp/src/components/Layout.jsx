import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import "./Layout.css";

const NAV_ITEMS = [
    { to: "/products", label: "Товары", icon: "◧" },
    { to: "/orders", label: "Заказы", icon: "▤" },
    { to: "/categories", label: "Категории", icon: "◫" },
    { to: "/presale-locations", label: "Точки предзаказа", icon: "◎" },
];

const ADMIN_ONLY_ITEMS = [{ to: "/staff", label: "Сотрудники", icon: "◍" }];

export default function Layout() {
    const { staff, logout, isAdmin } = useAuth();

    return (
        <div className="shell">
            <aside className="shell__sidebar">
                <div className="shell__brand">
                    <span className="shell__brand-mark">OS</span>
                    <span className="shell__brand-name">OnlyShop</span>
                </div>

                <nav className="shell__nav">
                    {NAV_ITEMS.map((item) => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            className={({ isActive }) => `shell__nav-item${isActive ? " shell__nav-item--active" : ""}`}
                        >
                            <span className="shell__nav-icon">{item.icon}</span>
                            {item.label}
                        </NavLink>
                    ))}

                    {isAdmin && (
                        <>
                            <div className="shell__nav-divider" />
                            {ADMIN_ONLY_ITEMS.map((item) => (
                                <NavLink
                                    key={item.to}
                                    to={item.to}
                                    className={({ isActive }) => `shell__nav-item${isActive ? " shell__nav-item--active" : ""}`}
                                >
                                    <span className="shell__nav-icon">{item.icon}</span>
                                    {item.label}
                                </NavLink>
                            ))}
                        </>
                    )}
                </nav>

                <div className="shell__account glass">
                    <div className="shell__account-info">
                        <span className="shell__account-name">{staff?.name}</span>
                        <span className="shell__account-role">{staff?.role === "Admin" ? "Администратор" : "Менеджер"}</span>
                    </div>
                    <button className="shell__logout" onClick={logout} aria-label="Выйти" title="Выйти">⏻</button>
                </div>
            </aside>

            <main className="shell__content">
                <Outlet />
            </main>
        </div>
    );
}