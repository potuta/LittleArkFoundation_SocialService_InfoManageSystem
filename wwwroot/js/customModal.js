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

function showAlert(message, title = "Alert") {
    document.getElementById("alertModalTitle").innerText = title;
    document.getElementById("alertModalMessage").innerText = message;

    const alertModal = new bootstrap.Modal(document.getElementById('alertModal'));
    alertModal.show();
}
