document.querySelectorAll('.ss-wrapper').forEach(wrapper => {

    const display = wrapper.querySelector('.ss-display');
    const dropdown = wrapper.querySelector('.ss-dropdown');
    const search = wrapper.querySelector('.ss-search');
    const options = wrapper.querySelectorAll('.ss-option');
    const hidden = wrapper.querySelector('input[type="hidden"]');
    const text = wrapper.querySelector('.ss-text');

    search.addEventListener('input', () => {
        const term = search.value.toLowerCase().trim();

        wrapper.querySelectorAll('.ss-option').forEach(opt => {
            const text = opt.textContent.toLowerCase();

            if (text.includes(term)) {
                opt.style.display = 'block';
                if (opt.nextElementSibling?.tagName === 'HR')
                    opt.nextElementSibling.style.display = 'block';
            } else {
                opt.style.display = 'none';
                if (opt.nextElementSibling?.tagName === 'HR')
                    opt.nextElementSibling.style.display = 'none';
            }
        });
    });

    display.addEventListener('click', (e) => {
        if (wrapper.classList.contains('ss-disabled') || hidden.disabled) {
            e.preventDefault();
            e.stopPropagation();
            return;
        }

        const isOpen = wrapper.classList.contains('open');

        document.querySelectorAll('.ss-wrapper').forEach(w => {
            w.classList.remove('open');
            w.querySelector('.ss-dropdown').style.display = 'none';
        });

        if (!isOpen) {
            wrapper.classList.add('open');
            dropdown.style.display = 'block';
            search.focus();
        }
    });

    options.forEach(opt => {
        opt.addEventListener('click', () => {
            text.textContent = opt.textContent.trim();
            hidden.value = opt.dataset.value;
            $(hidden).trigger('change');
            wrapper.classList.remove('open');
            dropdown.style.display = 'none';
        });
    });

    document.addEventListener('click', e => {
        if (!wrapper.contains(e.target)) {
            wrapper.classList.remove('open');
            dropdown.style.display = 'none';
        }
    });
});

function setSearchableSelectDisabled(selectId, disabled = true) {
    const hidden = document.getElementById(selectId);
    if (!hidden) return;

    const wrapper = hidden.closest('.ss-wrapper');
    const dropdown = wrapper.querySelector('.ss-dropdown');

    hidden.disabled = disabled;

    if (disabled) {
        wrapper.classList.add('ss-disabled');
        wrapper.classList.remove('open');
        if (dropdown) dropdown.style.display = 'none';
    } else {
        wrapper.classList.remove('ss-disabled');
    }
}

function setSearchableSelectOptions(selectId, items, placeholder = 'انتخاب کنید') {

    const hidden = document.getElementById(selectId);
    if (!hidden) return;

    const wrapper = hidden.closest('.ss-wrapper');
    const optionsContainer = wrapper.querySelector('.ss-options');
    const text = wrapper.querySelector('.ss-text');

    // reset value
    hidden.value = '';
    text.textContent = placeholder;

    // clear options
    optionsContainer.innerHTML = '';

    items.forEach((i, index) => {

        const div = document.createElement('div');
        div.className = 'ss-option';
        div.dataset.value = i.value;
        div.textContent = i.text;

        div.addEventListener('click', () => {
            text.textContent = i.text;
            hidden.value = i.value;
            $(hidden).trigger('change');
            wrapper.classList.remove('open');
            wrapper.querySelector('.ss-dropdown').style.display = 'none';
        });

        optionsContainer.appendChild(div);

        if (index !== items.length - 1) {
            const hr = document.createElement('hr');
            hr.style.margin = '5px';
            optionsContainer.appendChild(hr);
        }
    });
}