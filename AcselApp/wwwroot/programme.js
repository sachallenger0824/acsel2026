(() => {
    const root = document.getElementById('programme-card');
    if (!root) return;

    const make = (tag, className, text) => {
        const element = document.createElement(tag);
        if (className) element.className = className;
        if (text !== undefined && text !== null) element.textContent = text;
        return element;
    };

    const appendDetail = (list, detail) => {
        if (typeof detail !== 'string' || !detail.trim()) return;

        const item = document.createElement('li');
        const colon = detail.indexOf(':');
        if (colon > 0 && colon < 32) {
            const label = make('strong', '', `${detail.slice(0, colon)}:`);
            item.append(label, document.createTextNode(` ${detail.slice(colon + 1).trim()}`));
        } else {
            item.textContent = detail;
        }
        list.appendChild(item);
    };

    const renderProgramme = (data) => {
        root.replaceChildren();

        root.appendChild(make('p', 'programme-kicker data-text', 'ACSEL 2026 Preliminary Agenda'));
        root.appendChild(make('h2', 'card-title programme-title', data.conferenceName || 'ACSEL 2026 Conference Programme'));

        const meta = make('dl', 'programme-meta');
        [
            ['Theme', data.theme],
            ['Venue', data.venue]
        ].forEach(([label, value]) => {
            if (!value) return;
            const group = document.createElement('div');
            group.append(make('dt', '', label), make('dd', '', value));
            meta.appendChild(group);
        });
        if (meta.childElementCount) root.appendChild(meta);

        (Array.isArray(data.days) ? data.days : []).forEach((day, dayIndex) => {
            const section = make('section', 'programme-day');
            const headingId = `programme-day-${dayIndex + 1}`;
            const dayName = day.label || `Day ${dayIndex + 1}`;
            section.setAttribute('aria-labelledby', headingId);

            const heading = make('h3');
            heading.id = headingId;
            heading.append(make('span', '', dayName));
            if (day.date) heading.append(make('time', '', day.date));
            section.appendChild(heading);

            const wrap = make('div', 'programme-table-wrap');
            const table = make('table', 'programme-table');
            table.setAttribute('aria-label', `${dayName} conference programme`);

            const thead = document.createElement('thead');
            const headerRow = document.createElement('tr');
            ['Time', 'Programme'].forEach((label) => {
                const cell = make('th', '', label);
                cell.scope = 'col';
                headerRow.appendChild(cell);
            });
            thead.appendChild(headerRow);
            table.appendChild(thead);

            const tbody = document.createElement('tbody');
            (Array.isArray(day.items) ? day.items : []).forEach((event) => {
                const row = document.createElement('tr');
                row.appendChild(make('td', 'programme-time', event.time || ''));

                const description = document.createElement('td');
                description.appendChild(make('span', 'programme-session-title', event.program || ''));
                const details = Array.isArray(event.details) ? event.details.filter(value => typeof value === 'string' && value.trim()) : [];
                if (details.length) {
                    const list = make('ul', 'programme-details');
                    details.forEach((detail) => appendDetail(list, detail));
                    description.appendChild(list);
                }

                row.appendChild(description);
                tbody.appendChild(row);
            });
            table.appendChild(tbody);
            wrap.appendChild(table);
            section.appendChild(wrap);
            root.appendChild(section);
        });

        if (Array.isArray(data.tracks) && data.tracks.length) {
            const section = make('section', 'programme-tracks');
            section.setAttribute('aria-labelledby', 'oral-tracks-heading');
            const heading = make('h3', '', 'Oral Presentation Tracks');
            heading.id = 'oral-tracks-heading';
            section.appendChild(heading);

            const grid = make('div', 'programme-tracks-grid');
            data.tracks.forEach((track) => {
                const card = make('div', 'programme-track');
                card.append(make('span', 'programme-track-label', track.name || ''), make('p', '', track.theme || ''));
                grid.appendChild(card);
            });
            section.appendChild(grid);
            root.appendChild(section);
        }

        if (Array.isArray(data.notes) && data.notes.length) {
            const notes = make('aside', 'programme-notes');
            notes.setAttribute('aria-label', 'Programme notes');
            data.notes.forEach((note) => {
                if (typeof note !== 'string' || !note.trim()) return;
                const paragraph = make('p');
                const colon = note.indexOf(':');
                if (colon > 0 && colon < 32) {
                    paragraph.append(make('strong', '', `${note.slice(0, colon)}:`), document.createTextNode(` ${note.slice(colon + 1).trim()}`));
                } else {
                    paragraph.textContent = note;
                }
                notes.appendChild(paragraph);
            });
            if (notes.childElementCount) root.appendChild(notes);
        }
    };

    fetch(new URL('api/programme', window.location.href), { cache: 'no-store', headers: { Accept: 'application/json' } })
        .then((response) => {
            if (!response.ok) throw new Error('Programme request failed');
            return response.json();
        })
        .then(renderProgramme)
        .catch(() => {
            root.replaceChildren(make('p', 'programme-loading', 'The programme is temporarily unavailable. Please refresh the page in a moment.'));
        });
})();
