function showToast(message, type = "success") {
    const box = document.createElement("div");
    box.className = `toast-box toast-${type}`;
    box.textContent = message; // هیچ HTML-encoding انجام نمیشه
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