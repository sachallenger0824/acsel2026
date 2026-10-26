(() => {
    const daysList = document.getElementById('days-list');
    const tracksList = document.getElementById('tracks-list');
    const notesList = document.getElementById('notes-list');
    const form = document.getElementById('programme-form');

    if (!daysList || !tracksList || !notesList || !form) return;

    const cloneTemplate = (id) => {
        const template = document.getElementById(id);
        return template?.content.firstElementChild?.cloneNode(true) ?? null;
    };

    const reindex = () => {
        daysList.querySelectorAll('[data-day]').forEach((day, dayIndex) => {
            day.querySelectorAll('[data-day-field]').forEach((field) => {
                field.name = `Programme.Days[${dayIndex}].${field.dataset.dayField}`;
            });

            day.querySelectorAll('[data-item]').forEach((item, itemIndex) => {
                item.querySelectorAll('[data-item-field]').forEach((field) => {
                    field.name = `Programme.Days[${dayIndex}].Items[${itemIndex}].${field.dataset.itemField}`;
                });
            });
        });

        tracksList.querySelectorAll('[data-track]').forEach((track, index) => {
            track.querySelectorAll('[data-track-field]').forEach((field) => {
                field.name = `Programme.Tracks[${index}].${field.dataset.trackField}`;
            });
        });

        notesList.querySelectorAll('[data-note]').forEach((note, index) => {
            const field = note.querySelector('[data-note-field]');
            if (field) field.name = `Programme.Notes[${index}]`;
        });
    };

    document.getElementById('add-day')?.addEventListener('click', () => {
        const day = cloneTemplate('day-template');
        if (!day) return;

        daysList.appendChild(day);
        const item = cloneTemplate('item-template');
        if (item) day.querySelector('.items-list')?.appendChild(item);
        reindex();
        day.querySelector('[data-day-field="Label"]')?.focus();
    });

    document.getElementById('add-track')?.addEventListener('click', () => {
        const track = cloneTemplate('track-template');
        if (!track) return;

        tracksList.appendChild(track);
        reindex();
        track.querySelector('[data-track-field="Name"]')?.focus();
    });

    document.getElementById('add-note')?.addEventListener('click', () => {
        const note = cloneTemplate('note-template');
        if (!note) return;

        notesList.appendChild(note);
        reindex();
        note.querySelector('[data-note-field]')?.focus();
    });

    document.addEventListener('click', (event) => {
        const button = event.target.closest('button');
        if (!button) return;

        if (button.matches('[data-add-item]')) {
            const item = cloneTemplate('item-template');
            if (!item) return;

            button.closest('[data-day]')?.querySelector('.items-list')?.appendChild(item);
            reindex();
            item.querySelector('[data-item-field="Time"]')?.focus();
            return;
        }

        if (button.matches('[data-remove-day]')) button.closest('[data-day]')?.remove();
        else if (button.matches('[data-remove-item]')) button.closest('[data-item]')?.remove();
        else if (button.matches('[data-remove-track]')) button.closest('[data-track]')?.remove();
        else if (button.matches('[data-remove-note]')) button.closest('[data-note]')?.remove();
        else return;

        reindex();
    });

    form.addEventListener('submit', reindex);
    reindex();
})();
