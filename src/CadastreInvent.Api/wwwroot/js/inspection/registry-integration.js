

window.spawnInspectionTask = async function (spatialUnitId, inspectorId) {
    if (!spatialUnitId) {
        alert("Ошибка: Кадастровый ID участка отсутствует.");
        return;
    }

    if (!inspectorId) {
        const userInput = prompt("Введите ID инспектора (Guid) или оставьте пустым для назначения позже:");
        if (userInput === null) return; 
        inspectorId = userInput || '00000000-0000-0000-0000-000000000000';
    }

    const deadline = new Date();
    deadline.setDate(deadline.getDate() + 3);

    const payload = {
        SpatialUnitId: spatialUnitId,
        InspectorId: inspectorId,
        Deadline: deadline.toISOString()
    };

    try {
        const response = await fetch('/api/inspection/tasks', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            alert("Ордер инспекции успешно передан диспетчеру!");
        } else {
            const error = await response.json();
            alert(`Отклонено диспетчеризацией: ${error.message || 'Системная ошибка'}`);
        }
    } catch (err) {
        alert("Сетевая ошибка при связи с модулем Инспекций.");
        console.error(err);
    }
}