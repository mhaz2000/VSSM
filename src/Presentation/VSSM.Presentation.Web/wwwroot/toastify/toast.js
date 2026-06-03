function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showToast(message, type = "success") {
    const box = document.createElement("div");
    box.className = `toast-box toast-${type}`;

    const escapedMessage = escapeHtml(message);
    box.innerHTML = escapedMessage.replace(/\n/g, '<br>');

    document.body.appendChild(box);

    requestAnimationFrame(() => {
        box.style.display = "block";
        box.style.opacity = "1";
    });

    setTimeout(() => {
        box.style.opacity = "0";
        setTimeout(() => box.remove(), 300);
    }, 3000);
}