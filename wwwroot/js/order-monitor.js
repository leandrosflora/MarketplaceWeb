(function () {
    const POLL_INTERVAL_MS = 4000;
    const container = document.querySelector('[data-jaeger-base-url]');
    const jaegerBaseUrl = container ? container.getAttribute('data-jaeger-base-url') : 'http://localhost:16686';
    const tbody = document.getElementById('journeys-tbody');
    const lastRefresh = document.getElementById('last-refresh');

    if (!tbody) {
        return;
    }

    function traceUrl(journey) {
        if (journey.rootTraceId) {
            return `${jaegerBaseUrl}/trace/${journey.rootTraceId}`;
        }
        const tags = encodeURIComponent(JSON.stringify({ 'correlation.id': journey.correlationId }));
        return `${jaegerBaseUrl}/search?tags=${tags}`;
    }

    function rowClass(journey) {
        if (journey.hasError) return 'table-danger';
        if (journey.secondsSinceLastEvent > 60) return 'table-warning';
        return '';
    }

    function escapeHtml(value) {
        if (value === null || value === undefined) return '';
        return String(value)
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;');
    }

    function renderRow(journey) {
        const orderCell = journey.orderId
            ? `<a href="/admin/operations/orders/detail/${journey.orderId}">${escapeHtml(journey.orderId)}</a>`
            : '<span class="text-muted">(sem orderId ainda)</span>';
        const errorCell = journey.hasError ? `⚠ ${escapeHtml(journey.errorReason)}` : '';
        const updatedAt = journey.updatedAt ? new Date(journey.updatedAt).toLocaleTimeString() : '';

        return `<tr data-journey-id="${escapeHtml(journey.id)}" class="${rowClass(journey)}">
            <td>${orderCell}</td>
            <td class="text-truncate" style="max-width: 140px;" title="${escapeHtml(journey.checkoutId)}">${escapeHtml(journey.checkoutId)}</td>
            <td><span class="badge text-bg-secondary">${escapeHtml(journey.currentStatus)}</span></td>
            <td>${escapeHtml(journey.currentStep)}</td>
            <td>${escapeHtml(journey.lastEventType)}</td>
            <td>${Math.round(journey.secondsSinceLastEvent)}s</td>
            <td>${errorCell}</td>
            <td class="text-truncate" style="max-width: 140px;" title="${escapeHtml(journey.correlationId)}">${escapeHtml(journey.correlationId)}</td>
            <td><a href="${traceUrl(journey)}" target="_blank" rel="noopener">Ver trace</a></td>
            <td>${updatedAt}</td>
        </tr>`;
    }

    async function poll() {
        try {
            const url = new URL(window.location.href);
            url.searchParams.set('handler', 'Poll');

            const response = await fetch(url.toString(), { headers: { Accept: 'application/json' } });
            if (!response.ok) return;

            const journeys = await response.json();
            tbody.innerHTML = journeys.map(renderRow).join('');
            if (lastRefresh) {
                lastRefresh.textContent = new Date().toLocaleTimeString();
            }
        } catch {
            // Transient network/poll failures are silently ignored; the next tick retries.
        }
    }

    setInterval(poll, POLL_INTERVAL_MS);
})();
