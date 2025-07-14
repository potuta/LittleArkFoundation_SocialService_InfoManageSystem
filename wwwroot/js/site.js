function toggleDropdown() {
    document.getElementById("dropdown-profile-menu").classList.toggle("show");
}

//function toggleSidebar() {
//    document.body.classList.toggle('sidebar-collapsed');
//}

//function toggleSidebar() {
//    const sidebar = document.querySelector('.navbar-left');
//    const mainContent = document.querySelector('.main-content');
//    sidebar.classList.toggle('collapsed');
//    mainContent.classList.toggle('collapsed'); // Optional: shrink main content too
//}
function toggleSidebar() {
    const sidebar = document.querySelector('.navbar-left');
    const mainContent = document.querySelector('.main-content');

    const isCollapsed = sidebar.classList.toggle('collapsed');
    mainContent.classList.toggle('collapsed', isCollapsed); // sync both

    // Save the state to localStorage
    localStorage.setItem('sidebar-collapsed', isCollapsed);
}

document.addEventListener('DOMContentLoaded', () => {
    const isCollapsed = localStorage.getItem('sidebar-collapsed') === 'true';

    if (isCollapsed) {
        const sidebar = document.querySelector('.navbar-left');
        const mainContent = document.querySelector('.main-content');
        sidebar.classList.add('collapsed');
        mainContent.classList.add('collapsed');
    }

    // Attach the toggle function to your button
    const toggleBtn = document.getElementById('toggleSidebar');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', toggleSidebar);
    }
});


// Close the dropdown if the user clicks outside of it
document.onclick = function (event) {
    if (!event.target.matches('.profile-icon') && !event.target.matches('.profile-label')) {
        var dropdowns = document.getElementsByClassName("dropdown-profile-content");
        for (var i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
            }
        }
    }
}

document.addEventListener("DOMContentLoaded", function () {
    var loginError = document.getElementById("loginErrorMessage");
    var resetPasswordSuccess = document.getElementById("resetPasswordSuccessMessage");

    if (loginError) {
        var loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
        loginModal.show();
    }

    if (resetPasswordSuccess) {
        var loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
        loginModal.show();
    }
});

document.addEventListener("DOMContentLoaded", function () {
    const body = document.body;
    const controller = body.dataset.controller;
    const action = body.dataset.action;

    console.log("Controller:", controller);
    console.log("Action:", action);

    if (action === "Create" || action === "Edit" || action === "InterviewGeneral" || action === "AdmitOPD") {
        return;
    }

    if (controller === "Dashboard") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-house-door-fill";
            icon.style.marginRight = "8px"; // spacing between icon and text

            // Insert icon before the text
            h2.prepend(icon);
        }
    }
    else if (controller === "OPD") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-box-arrow-left";
            icon.style.marginRight = "8px"; // spacing between icon and text

            // Insert icon before the text
            h2.prepend(icon);
        }
    }
    else if (controller === "Form" || controller === "GeneralAdmission") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-box-arrow-in-right";
            icon.style.marginRight = "8px"; // spacing between icon and text

            // Insert icon before the text
            h2.prepend(icon);
        }
    }
    else if (controller === "Discharge") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-box-arrow-right";
            icon.style.marginRight = "8px"; // spacing between icon and text
            // Insert icon before the text
            h2.prepend(icon);
        }
    }
    else if (controller === "Users") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-person-gear";
            icon.style.marginRight = "8px"; // spacing between icon and text
            // Insert icon before the text
            h2.prepend(icon);
        }
    }
    else if (controller === "Roles") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-person-lock";
            icon.style.marginRight = "8px"; // spacing between icon and text
            // Insert icon before the text
            h2.prepend(icon);
        }
    }
    else if (controller === "Permissions") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-person-lock";
            icon.style.marginRight = "8px"; // spacing between icon and text
            // Insert icon before the text
            h2.prepend(icon);
        }
    }
    else if (controller === "Database") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-database-gear";
            icon.style.marginRight = "8px"; // spacing between icon and text
            // Insert icon before the text
            h2.prepend(icon);
        }
    }
    else if (controller === "SystemLogs") {
        const h2 = document.getElementsByTagName("h2")[0];
        if (h2) {
            // Create the icon element
            const icon = document.createElement("i");
            icon.className = "bi bi-tools";
            icon.style.marginRight = "8px"; // spacing between icon and text
            // Insert icon before the text
            h2.prepend(icon);
        }
    }
});







