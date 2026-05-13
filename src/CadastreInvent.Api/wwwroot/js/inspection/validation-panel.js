import { state, getTaskById, updateTaskStatus } from './state.js';
import { drawPhotoConnections, clearMapGeometries, drawMarkers } from './map-engine.js';
import { renderSidebar } from './task-sidebar.js';

export function openValidationPanel(task) {
    const panel = document.getElementById('validation-panel');
    const content = document.getElementById('vp-content');
    const footer = document.getElementById('vp-footer');

    let html = '';
    const targetLatLng = L.latLng(task.targetLat, task.targetLon);

    // 1. Фотоматериалы
    if (task.photos && task.photos.length > 0) {
        html += `<div class="vp-section"><div class="vp-section-title"><i data-lucide="camera" style="width:18px;"></i> Фотофиксация объекта (${task.photos.length})</div>`;

        task.photos.forEach(p => {
            const photoLatLng = L.latLng(p.lat, p.lon);
            const distance = targetLatLng.distanceTo(photoLatLng);

            let gpsAlert = '';
            if (distance > 50) {
                gpsAlert = `<div class="vp-gps-warning"><i data-lucide="alert-octagon" style="width:16px;"></i> Отклонение координат: ${distance.toFixed(0)} м.</div>`;
            } else {
                gpsAlert = `<div class="vp-gps-success"><i data-lucide="check-circle-2" style="width:16px;"></i> Координаты верифицированы (${distance.toFixed(0)} м.)</div>`;
            }

            const dateStr = new Date(p.captureDate).toLocaleString('ru-RU');
            html += `
                <div class="vp-photo-card">
                    <img src="${p.photoUrl}" class="vp-photo-img" onerror="this.src='https://placehold.co/600x400?text=Ошибка+Загрузки'" />
                    <div class="vp-photo-meta">
                        <div style="font-weight:600; color:#0f172a;">Метка времени: ${dateStr}</div>
                        ${gpsAlert}
                    </div>
                </div>
            `;
        });
        html += `</div>`;
        drawPhotoConnections(task);
    } else {
        html += `<div class="vp-section" style="color: #94a3b8; font-size: 0.95rem; margin-bottom: 24px; text-align: center; padding: 20px; border: 1px dashed #cbd5e1; border-radius: 12px;"><i data-lucide="image-off" style="width:24px; height:24px; margin-bottom: 8px;"></i><br/>Фотоматериалы отсутствуют</div>`;
    }

    // 2. Акты наблюдений (Сравнительный анализ характеристик)
    if (task.observations && task.observations.length > 0) {
        html += `<div class="vp-section"><div class="vp-section-title"><i data-lucide="clipboard-list" style="width:18px;"></i> Акты полевого обследования (${task.observations.length})</div>`;

        let oldProps = {};
        try {
            if (task.currentCharacteristicsJson) {
                const rawOld = JSON.parse(task.currentCharacteristicsJson);
                // Нормализация ключей для поиска независимо от регистра
                for (let k in rawOld) oldProps[k.toLowerCase()] = rawOld[k];
            }
        } catch (e) { console.error("Failed to parse old JSON", e); }

        task.observations.forEach(o => {
            const dateStr = new Date(o.observationDate).toLocaleString('ru-RU');
            const catLabel = state.catConfig[o.category] || state.catConfig[String(o.category)] || o.category;

            let parsedProps = '';
            try {
                // Пытаемся извлечь JSON.
                let rawNew = JSON.parse(o.remarksJson);

                // --- ЗАЩИТА ОТ КРИВОГО БЭКЕНДА ---
                // Если JSON завернут в свойство "notes", распаковываем его.
                if (rawNew.notes && typeof rawNew.notes === 'string' && rawNew.notes.startsWith('{')) {
                    try { rawNew = JSON.parse(rawNew.notes); } catch (e) { }
                }
                // ---------------------------------

                const newProps = {};
                for (let k in rawNew) newProps[k.toLowerCase()] = rawNew[k];

                const propMap = {
                    'area': 'Площадь (м²)',
                    'floor': 'Этажность',
                    'yearbuilt': 'Год постройки',
                    'distancetocenterkm': 'Удаленность от центра (км)',
                    'roomscount': 'Кол-во комнат',
                    'condition': 'Состояние',
                    'discrepancytype': 'Характер нарушения'
                };

                let rowsHtml = '';

                for (let key in propMap) {
                    const label = propMap[key];
                    const oldVal = oldProps[key];
                    const newVal = newProps[key];

                    if (newVal !== undefined && newVal !== "") {
                        if (oldVal !== undefined && String(oldVal) !== String(newVal)) {
                            rowsHtml += `<div class="vp-diff-row">
                                <span class="vp-diff-label">${label}</span>
                                <div class="vp-diff-values">
                                    <span class="vp-diff-old">${oldVal}</span>
                                    <i data-lucide="arrow-right" style="width:14px; color:#94a3b8;"></i>
                                    <span class="vp-diff-new">${newVal}</span>
                                </div>
                            </div>`;
                        } else {
                            rowsHtml += `<div class="vp-diff-row">
                                <span class="vp-diff-label">${label}</span>
                                <div class="vp-diff-values">
                                    <span class="vp-diff-unchanged">${newVal}</span>
                                </div>
                            </div>`;
                        }
                    }
                }

                let notesHtml = '';
                if (newProps['notes'] && typeof newProps['notes'] === 'string') {
                    notesHtml = `<div class="vp-obs-notes"><strong><i data-lucide="message-square" style="width:14px; margin-right:4px;"></i> Примечание инспектора:</strong><div style="margin-top: 4px;">${newProps['notes']}</div></div>`;
                }

                if (rowsHtml) {
                    parsedProps = `<div class="vp-diff-container">${rowsHtml}</div>${notesHtml}`;
                } else if (notesHtml) {
                    parsedProps = notesHtml;
                } else {
                    parsedProps = `<div class="vp-obs-json">Характеристики не изменены</div>`;
                }

            } catch (e) {
                // Фолбек: просто текст
                parsedProps = `<div class="vp-obs-json">${o.remarksJson}</div>`;
            }

            html += `
                <div class="vp-obs-card">
                    <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom: 12px;">
                        <div>
                            <div class="vp-obs-cat">${catLabel}</div>
                            <div style="font-size: 0.8rem; color: #64748b; margin-top: 2px;">Дата: ${dateStr}</div>
                        </div>
                    </div>
                    ${parsedProps}
                </div>
            `;
        });
        html += `</div>`;
    } else {
        html += `<div class="vp-section" style="color: #94a3b8; font-size: 0.95rem; text-align: center; padding: 20px;"><i data-lucide="file-x-2" style="width:24px; height:24px; margin-bottom:8px;"></i><br/>Акты отсутствуют</div>`;
    }

    content.innerHTML = html;

    if (task.status === 'Completed') {
        const mlWarning = `
            <div class="vp-action-warning">
                <i data-lucide="cpu" style="width: 24px; height: 24px; flex-shrink: 0; color: #2563eb;"></i>
                <div>
                    <strong>Интеграция с CAMA Engine</strong>
                    <div style="margin-top: 4px;">Утверждение обновит характеристики в ЕГРН и автоматически запустит пересчет стоимости нейросетью.</div>
                </div>
            </div>`;

        footer.innerHTML = mlWarning + `
            <div style="display: flex; gap: 12px; width: 100%;">
                <button type="button" onclick="window.processVerification('rework')" class="vp-btn vp-btn-reject"><i data-lucide="refresh-cw" style="width: 18px;"></i> На доработку</button>
                <button type="button" onclick="window.processVerification('approve')" class="vp-btn vp-btn-approve"><i data-lucide="check-circle" style="width: 18px;"></i> Утвердить</button>
            </div>
        `;
        footer.style.display = 'block';
    } else {
        footer.style.display = 'none';
    }

    panel.classList.add('open');
    if (window.lucide) window.lucide.createIcons();
}

export function closeValidationPanel() {
    state.selectedTaskId = null;
    document.getElementById('validation-panel').classList.remove('open');
    document.querySelectorAll('.disp-task-card').forEach(c => c.classList.remove('active'));
    clearMapGeometries();
}

export function refreshValidationPanelIfOpen(taskId) {
    if (state.selectedTaskId === taskId) {
        const task = getTaskById(taskId);
        if (task) openValidationPanel(task);
    }
}

window.closeValidationPanel = closeValidationPanel;

window.processVerification = async function (action) {
    if (!state.selectedTaskId) return;

    const taskId = state.selectedTaskId;
    let url = '';
    let payload = null;

    if (action === 'approve') {
        url = `/api/inspection/tasks/${taskId}/approve`;
    } else if (action === 'rework') {
        const reason = prompt("Укажите причину отправки на доработку:");
        if (!reason) return;
        url = `/api/inspection/tasks/${taskId}/rework`;
        payload = { reason: reason };
    } else return;

    try {
        const response = await fetch(url, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: payload ? JSON.stringify(payload) : null
        });

        if (response.ok) {
            const newStatus = action === 'approve' ? 'Verified' : 'InProgress';
            updateTaskStatus(taskId, newStatus);
            renderSidebar();
            drawMarkers();
            closeValidationPanel();
        } else {
            alert('Ошибка верификации');
        }
    } catch (err) {
        console.error('Сетевая ошибка', err);
    }
};