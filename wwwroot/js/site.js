window.getSelectedValues = function (selectElement) {
    return Array.from(selectElement.selectedOptions).map(o => parseInt(o.value));
};
