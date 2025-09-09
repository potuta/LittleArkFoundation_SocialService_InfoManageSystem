function showConfirmModal(title, message, onConfirm) {
    document.getElementById("confirmModalTitle").innerText = title || "Confirm Action";
    document.getElementById("confirmModalBody").innerText = message || "Are you sure?";

    const yesBtn = document.getElementById("confirmModalYes");

    // Remove previous event listeners to avoid stacking
    const newYesBtn = yesBtn.cloneNode(true);
    yesBtn.parentNode.replaceChild(newYesBtn, yesBtn);

    newYesBtn.addEventListener("click", function () {
        if (onConfirm && typeof onConfirm === "function") {
            onConfirm();
        }
        const modalEl = bootstrap.Modal.getInstance(document.getElementById('confirmModal'));
        modalEl.hide();
    });

    const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
    modal.show();
}

// Special helper for forms
function confirmFormSubmit(formId, title = "Confirm", message = "Are you sure?") {
    const form = document.getElementById(formId);
    if (!form) return false;

    if (!form.reportValidity()) {
        // Form is invalid, browser will show validation errors
        return false;
    }

    showConfirmModal(title, message, () => {
        form.submit(); // safe to submit, validation already passed
    });

    return false; // stop immediate submit
}

// Special helper for url/links navigation
function confirmLinkClick(url, title = "Confirm", message = "Are you sure?") {
    showConfirmModal(title, message, () => {
        window.location.href = url;
    });
    return false; // prevents navigation until confirmed
}

function showAlertModal(message, title = "Alert") {
    document.getElementById("alertModalTitle").innerText = title;
    document.getElementById("alertModalMessage").innerText = message;

    const alertModal = new bootstrap.Modal(document.getElementById('alertModal'));
    alertModal.show();
}
