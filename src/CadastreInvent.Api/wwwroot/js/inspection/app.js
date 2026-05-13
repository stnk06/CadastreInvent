import { initMap } from './map-engine.js';
import { renderSidebar } from './task-sidebar.js';
import { initSignalR } from './signalr-client.js';

document.addEventListener("DOMContentLoaded", () => {
    // 1. Инициализация Leaflet Карты
    initMap();

    // 2. Рендеринг боковой панели задач
    renderSidebar();

    // 3. Подключение WebSockets (SignalR)
    initSignalR();

    // 4. Инициализация иконок Lucide
    if (window.lucide) {
        window.lucide.createIcons();
    }

    // Скрытие системных алертов через 4 секунды
    setTimeout(() => {
        document.querySelectorAll('.alert').forEach(a => a.style.display = 'none');
    }, 4000);
});