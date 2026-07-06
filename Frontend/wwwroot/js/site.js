document.addEventListener('click', async function (event) {
    const button = event.target.closest('[data-copy-event-id]');
    if (!button) {
        return;
    }

    const eventId = button.getAttribute('data-copy-event-id');
    if (!eventId) {
        return;
    }

    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(eventId);
        } else {
            const textarea = document.createElement('textarea');
            textarea.value = eventId;
            textarea.setAttribute('readonly', '');
            textarea.style.position = 'fixed';
            textarea.style.left = '-9999px';
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand('copy');
            document.body.removeChild(textarea);
        }
    } catch (error) {
        console.error('Failed to copy event id:', error);
    }
});
