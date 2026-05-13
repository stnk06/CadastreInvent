import { state, getFilteredTasks, getTaskById } from './state.js';
import { openValidationPanel } from './validation-panel.js';

let map;
let markersGroup;
let polyGroup;

export function initMap() {
    const russiaBounds = [[41.1859, 19.6389], [81.8589, 190.0000]];
    map = L.map('dispatch-map', { maxBounds: russiaBounds, maxBoundsViscosity: 1.0, minZoom: 4 }).setView([55.75, 37.61], 5);
    L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', { maxZoom: 19 }).addTo(map);

    markersGroup = L.featureGroup().addTo(map);
    polyGroup = L.featureGroup().addTo(map);

    drawMarkers();
}

function getMarkerHtml(status) {
    const color = state.statusConfig[status]?.class.replace('status-', '') || 'cancelled';
    return `<div class="marker-pin ${color}"></div>`;
}

export function drawMarkers() {
    markersGroup.clearLayers();
    const filtered = getFilteredTasks();

    filtered.forEach(task => {
        const icon = L.divIcon({ className: 'custom-div-icon', html: getMarkerHtml(task.status), iconSize: [30, 42], iconAnchor: [15, 42] });
        const marker = L.marker([task.targetLat, task.targetLon], { icon: icon });

        marker.on('click', () => window.selectTask(task.id));
        markersGroup.addLayer(marker);
    });

    if (markersGroup.getLayers().length > 0 && !state.selectedTaskId) {
        map.fitBounds(markersGroup.getBounds(), { padding: [50, 50], maxZoom: 12 });
    }
}

function parseWkt(wkt) {
    if (!wkt) return [];
    const match = wkt.match(/\(\(([^)]+)\)\)/);
    if (!match) return [];

    return match[1].split(',').map(pair => {
        const parts = pair.trim().split(/\s+/);
        if (parts.length >= 2) return [parseFloat(parts[1]), parseFloat(parts[0])];
        return null;
    }).filter(c => c !== null);
}

export function focusMapOnTask(taskId) {
    const task = getTaskById(taskId);
    if (!task) return;

    polyGroup.clearLayers();

    if (task.polygonWkt) {
        const coords = parseWkt(task.polygonWkt);
        if (coords.length > 0) {
            L.polygon(coords, { color: '#3b82f6', weight: 2, fillOpacity: 0.1 }).addTo(polyGroup);
        }
    }

    map.flyTo([task.targetLat, task.targetLon], 16, { duration: 1 });
    openValidationPanel(task);
}

export function drawPhotoConnections(task) {
    if (!task || !task.photos) return;

    task.photos.forEach(p => {
        L.circleMarker([p.lat, p.lon], { radius: 6, color: '#ef4444', fillColor: '#ef4444', fillOpacity: 1 }).addTo(polyGroup);
        L.polyline([[task.targetLat, task.targetLon], [p.lat, p.lon]], { color: '#ef4444', weight: 2, dashArray: '5, 5' }).addTo(polyGroup);
    });
}

export function clearMapGeometries() {
    polyGroup.clearLayers();
    if (markersGroup.getLayers().length > 0) {
        map.flyToBounds(markersGroup.getBounds(), { padding: [50, 50], duration: 1 });
    }
}