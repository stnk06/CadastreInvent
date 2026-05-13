export const state = {
    tasks: window.INITIAL_DISPATCHER_DATA || [],
    currentFilter: 'all',
    selectedTaskId: null,
    statusConfig: {
        'Created': { label: 'Новый ордер', color: '#64748b', class: 'status-assigned' },
        'Assigned': { label: 'Назначено', color: '#f59e0b', class: 'status-assigned' },
        'InProgress': { label: 'В работе', color: '#3b82f6', class: 'status-inprogress' },
        'Completed': { label: 'Ожидает проверки', color: '#10b981', class: 'status-completed' },
        'Verified': { label: 'Утверждено', color: '#059669', class: 'status-completed' },
        'Rejected': { label: 'Отклонено', color: '#94a3b8', class: 'status-cancelled' },
        'Cancelled': { label: 'Отменено', color: '#94a3b8', class: 'status-cancelled' },
        'RequiresRework': { label: 'На доработке', color: '#ef4444', class: 'status-inprogress' }
    },
    catConfig: {
        'BoundaryVerification': 'Обычное обследование',
        'ConditionAssessment': 'Оценка физического состояния',
        'DiscrepancyFound': 'Выявлено расхождение',
        'IllegalConstruction': 'Самозастрой / Нарушение',
        '0': 'Обычное обследование',
        '1': 'Оценка физического состояния',
        '2': 'Выявлено расхождение',
        '3': 'Самозастрой / Нарушение'
    }
};

export function getFilteredTasks() {
    if (state.currentFilter === 'all') {
        return state.tasks;
    }
    return state.tasks.filter(t => t.status === state.currentFilter);
}

export function getTaskById(id) {
    return state.tasks.find(t => t.id === id);
}

export function updateTaskStatus(taskId, newStatus) {
    const task = getTaskById(taskId);
    if (task) {
        task.status = newStatus;
    }
}

export function addPhotoToTask(taskId, photo) {
    const task = getTaskById(taskId);
    if (task) {
        if (!task.photos) task.photos = [];
        task.photos.push(photo);
    }
}

export function addObservationToTask(taskId, observation) {
    const task = getTaskById(taskId);
    if (task) {
        if (!task.observations) task.observations = [];
        task.observations.push(observation);
    }
}