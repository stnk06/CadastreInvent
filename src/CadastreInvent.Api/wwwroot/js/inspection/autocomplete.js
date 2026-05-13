export function initAutocomplete() {
    const input = document.getElementById('spatial-search-input');
    const hidden = document.getElementById('spatial-id-hidden');
    const dropdown = document.getElementById('autocomplete-dropdown');

    if (!input || !dropdown) return;

    let timeout = null;

    input.addEventListener('input', function (e) {
        const query = e.target.value.trim();

        hidden.value = '';

        if (query.length < 2) {
            dropdown.classList.remove('open');
            return;
        }

        dropdown.classList.add('open');
        dropdown.innerHTML = '<div class="autocomplete-loading">Поиск в PostGIS...</div>';

        clearTimeout(timeout);
        timeout = setTimeout(() => {
            fetch(`/api/inspection/spatial-units/search?q=${encodeURIComponent(query)}`)
                .then(response => response.json())
                .then(data => {
                    dropdown.innerHTML = '';
                    if (data.length === 0) {
                        dropdown.innerHTML = '<div class="autocomplete-loading">Участки с геометрией не найдены</div>';
                        return;
                    }

                    data.forEach(item => {
                        const div = document.createElement('div');
                        div.className = 'autocomplete-item';
                        div.innerHTML = `<strong>${item.referenceNumber}</strong> <span style="color:#64748b; font-size:0.8rem; margin-left:8px;">(S: ${item.area.toFixed(1)} кв.м.)</span>`;

                        div.addEventListener('click', () => {
                            input.value = item.referenceNumber;
                            hidden.value = item.id;
                            dropdown.classList.remove('open');
                        });

                        dropdown.appendChild(div);
                    });
                })
                .catch(err => {
                    console.error('Ошибка поиска', err);
                    dropdown.innerHTML = '<div class="autocomplete-loading" style="color:#ef4444;">Ошибка API</div>';
                });
        }, 500);
    });

    document.addEventListener('click', function (e) {
        if (!input.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.remove('open');
        }
    });
}