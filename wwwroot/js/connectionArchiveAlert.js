document.addEventListener("DOMContentLoaded", async function () {
    const response = await fetch('/Admin/Connection/IsConnectionArchived');
    const data = await response.json();

    if (data.isArchived && !data.isTemp && !data.isDefault) {
        const alertDiv = document.createElement('div');

        alertDiv.className = 'alert alert-warning text-center mb-3';
        alertDiv.role = 'alert';
        alertDiv.innerHTML = '<i class="bi bi-exclamation-triangle-fill"></i> Warning: The database connection is archived. No changes can be made.';

        const selection = document.querySelector('.main-content main hr');
        if (selection) {
            selection.after(alertDiv);
        }

        document.querySelectorAll('.hide-when-archived')
            .forEach(el => el.style.display = 'none');

    }
    else if (data.isArchived && data.isTemp && !data.isDefault) {
        const alertDiv = document.createElement('div');

        alertDiv.className = 'alert alert-info text-center mb-3';
        alertDiv.role = 'alert';
        alertDiv.innerHTML = '<i class="bi bi-info-circle-fill"></i> Note: The database connection is a template. Changes here will not affect the main database. You can restore this database to verify backup files.';

        const selection = document.querySelector('.main-content main hr');
        if (selection) {
            selection.after(alertDiv);
        }

        document.querySelectorAll('.profile-btn')
            .forEach(el => el.style.display = 'none');
    }
});