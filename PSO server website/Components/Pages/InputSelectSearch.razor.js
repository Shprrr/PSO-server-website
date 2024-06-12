function updateDropdownMenu(dropdown, dropdownMenu) {
    const htmlElement = dropdown.ownerDocument.documentElement;
    dropdownMenu.style = `width: ${dropdown.clientWidth}px; max-height: ${htmlElement.clientHeight - htmlElement.scrollTop}px;`;

    dropdown.mutationObserver = new MutationObserver(() => {
        const newStyle = getCopy(dropdownMenu.style);
        if (dropdownMenu.oldStyle?.transform !== newStyle.transform) {
            const matchTransform = newStyle.transform.match(/^translate\(\d+px, (\d+)px\)$/);
            if (matchTransform !== null) {
                const transformY = Number(matchTransform[1]);
                dropdownMenu.style.setProperty('max-height', `${htmlElement.clientHeight - htmlElement.scrollTop - transformY}px`);
            }
        }
        dropdownMenu.oldStyle = newStyle;
    });
    dropdown.mutationObserver.observe(dropdownMenu, {
        attributes: true,
        attributeFilter: ['style']
    });

    // Timeout to scrollIntoView in next cycle.
    setTimeout(() => dropdownMenu.querySelector('*[class~="active"]')?.scrollIntoView({ behavior: 'instant', block: 'center' }));
}

function stopUpdateDropdownMenu(dropdown) {
    dropdown.mutationObserver.disconnect();
}

const getCopy = (obj) => {
    let result = {}
    for (let key in obj) {
        if (obj.hasOwnProperty(key)) {
            result[key] = obj[key]
        }
    }
    return result
}
