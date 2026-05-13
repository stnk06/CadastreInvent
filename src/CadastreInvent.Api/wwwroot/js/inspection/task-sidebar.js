import { state, getFilteredTasks } from './state.js';
import { drawMarkers, focusMapOnTask, clearMapGeometries } from './map-engine.js';
import { closeValidationPanel } from './validation-panel.js';

export function renderSidebar() {
    const container = document.getElementById('task-list-container');
    container.innerHTML = '';

    const filtered = getFilteredTasks();

    if (filtered.length === 0) {
        container.innerHTML = '<div style="text-align: center; padding: 20px; color: #94a3b8;">Задач не найдено</div>';
        return;
    }

    filtered.forEach(task => {
        const conf = state.statusConfig[task.status] || state.statusConfig['Cancelled'];
        const dateStr = new Date(task.deadline).toLocaleDateString('ru-RU');

        const card = document.createElement('div');
        card.className = `disp-task-card ${conf.class}`;
        if (state.selectedTaskId === task.id) {
            card.classList.add('active');
        }
        card.id = `sidebar-card-${task.id}`;

        card.onclick = () => window.selectTask(task.id);

        card.innerHTML = `
            <div style="display: flex; justify-content: space-between; align-items: flex-start;">
                <div class="disp-task-id">${task.id.substring(0, 8).toUpperCase()}</div>
                <div style="font-size: 0.75rem; font-weight: 700; color: ${conf.color}; padding: 2px 8px; background: ${conf.color}15; border-radius: 12px;">${conf.label}</div>
            </div>
            <div class="disp-task-su">Участок: ${task.spatialUnitReference}</div>
            <div class="disp-task-meta"><i data-lucide="user" style="width: 14px;"></i> ${task.inspectorName}</div>
            <div class="disp-task-meta"><i data-lucide="calendar" style="width: 14px;"></i> Дедлайн: ${dateStr}</div>
        `;

        container.appendChild(card);
    });

    if (window.lucide) window.lucide.createIcons();
}

window.filterTasks = function (status, btn) {
    state.currentFilter = status;
    document.querySelectorAll('.task-filter-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');

    state.selectedTaskId = null;
    closeValidationPanel();
    renderSidebar();
    drawMarkers();
};

window.selectTask = function (taskId) {
    state.selectedTaskId = taskId;
    document.querySelectorAll('.disp-task-card').forEach(c => c.classList.remove('active'));

    const card = document.getElementById(`sidebar-card-${taskId}`);
    if (card) {
        card.classList.add('active');
        card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    focusMapOnTask(taskId);
};