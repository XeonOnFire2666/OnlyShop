import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

const BACKEND = "http://localhost:5201";     

export default defineConfig({
    base: "/admin/",                          
    plugins: [react()],
    build: {
        outDir: "../wwwroot/admin",     
        emptyOutDir: true,
    },
    server: {
        port: 5174,
        proxy: {
            "/api": { target: BACKEND, changeOrigin: true },
        },
    },
});