import { state, updateTaskStatus, addPhotoToTask, addObservationToTask } from './state.js';
import { renderSidebar } from './task-sidebar.js';
import { drawMarkers } from './map-engine.js';
import { refreshValidationPanelIfOpen } from './validation-panel.js';

export function initSignalR() {
    const indicator = document.getElementById('socket-status');

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/inspection")
        .withAutomaticReconnect()
        .build();

    connection.on("TaskStatusChanged", (taskId, status) => {
        updateTaskStatus(taskId, status);
        renderSidebar();
        drawMarkers();
        refreshValidationPanelIfOpen(taskId);
        flashIndicator('#10b981');
    });

    connection.on("ObservationAdded", (taskId, observation) => {
        addObservationToTask(taskId, observation);
        refreshValidationPanelIfOpen(taskId);
        flashIndicator('#3b82f6');
    });

    connection.on("PhotoAdded", (taskId, photo) => {
        addPhotoToTask(taskId, photo);
        refreshValidationPanelIfOpen(taskId);
        flashIndicator('#f59e0b');
    });

    connection.onreconnecting(() => {
        if (indicator) indicator.style.background = '#f59e0b';
    });

    connection.start().then(() => {
        if (indicator) {
            indicator.style.background = '#10b981';
            indicator.title = "Real-Time соединение установлено";
        }
    }).catch(err => {
        console.error("SignalR Connection Error: ", err);
        if (indicator) indicator.style.background = '#ef4444';
    });

    function flashIndicator(color) {
        if (!indicator) return;
        const oldColor = indicator.style.background;
        indicator.style.background = color;
        indicator.style.boxShadow = `0 0 12px ${color}`;
        setTimeout(() => {
            indicator.style.background = '#10b981';
            indicator.style.boxShadow = '0 0 8px rgba(0,0,0,0.2)';
        }, 1000);
    }
}