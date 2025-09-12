function toggleDropdown() {
    document.getElementById("dropdown-profile-menu").classList.toggle("show");
    document.getElementById("profileDropdownToggle").classList.toggle("open");
}

function toggleSidebar() {
    const sidebar = document.querySelector('.navbar-left');
    const mainContent = document.querySelector('.main-content');

    const isCollapsed = sidebar.classList.toggle('collapsed');
    mainContent.classList.toggle('collapsed', isCollapsed); // sync both

    // Save the state to localStorage
    localStorage.setItem('sidebar-collapsed', isCollapsed);

    // For small screens, toggle 'active' class to show/hide sidebar
    sidebar.classList.toggle('active');
}

document.addEventListener('DOMContentLoaded', () => {
    const sidebar = document.querySelector('.navbar-left');
    const mainContent = document.querySelector('.main-content');

    // Temporarily disable transitions to prevent jitter
    sidebar.classList.add('no-transition');
    mainContent.classList.add('no-transition');

    const isCollapsed = localStorage.getItem('sidebar-collapsed') === 'true';

    if (isCollapsed) {
        sidebar.classList.add('collapsed');
        mainContent.classList.add('collapsed');
    }

    // Force reflow so classes are applied immediately
    void sidebar.offsetWidth;

    // Re-enable transitions
    sidebar.classList.remove('no-transition');
    mainContent.classList.remove('no-transition');

    // Attach the toggle button
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

document.addEventListener("DOMContentLoaded", async function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/usersHub")
        .build();

    connection.on("ActiveUsersUpdated", function(users) {
        let list = document.getElementById("activeUsersList");
        list.innerHTML = "";
        users.forEach(u => {
            let li = document.createElement("li");
            li.textContent = u;
            li.className = "users";
            list.appendChild(li);
        });

        // Update count
        const usersCountElement = document.querySelector(".container-users p");
        usersCountElement.innerHTML = '<span class="live-dot"></span> Active Users: ' + users.length;

    });

    connection.start().catch(err => console.error(err));
});

function enableUnsavedChangesWarning(options = {}) {
    const formSelector = options.formSelector || "#editForm";
    const linkSelector = options.linkSelector ||
        "a.btn-navbar, .dropdown-profile-content a, a.btn-left, a.btn-left-sub, a.btn-back";

    const form = document.querySelector(formSelector);
    if (!form) return;

    // Prevent duplicate attachment if called multiple times
    if (form.dataset.unsavedWarningAttached) return;
    form.dataset.unsavedWarningAttached = "true";

    function shouldSkip(el) {
        return !el.name || el.disabled || el.type === "hidden" || el.readOnly;
    }

    // Capture initial values
    const initialData = {};
    Array.from(form.elements).forEach(el => {
        if (shouldSkip(el)) return;
        const tag = el.tagName.toUpperCase();

        if (el.type === "radio") {
            if (!(el.name in initialData)) {
                const checked = form.querySelector(`input[name="${el.name}"]:checked`);
                initialData[el.name] = checked ? checked.value : null;
            }
        } else if (el.type === "checkbox") {
            const checkboxes = form.querySelectorAll(`input[name="${el.name}"]`);
            if (checkboxes.length > 1) {
                initialData[el.name] = Array.from(checkboxes)
                    .filter(c => c.checked)
                    .map(c => c.value);
            } else {
                initialData[el.name] = el.checked;
            }
        } else if (tag === "SELECT" && el.multiple) {
            initialData[el.name] = Array.from(el.selectedOptions).map(opt => opt.value);
        } else {
            initialData[el.name] = el.value;
        }
    });

    let changedCount = 0;

    function getChangedCount() {
        let count = 0;
        const checkedNames = new Set();

        Array.from(form.elements).forEach(el => {
            if (shouldSkip(el) || checkedNames.has(el.name)) return;
            const tag = el.tagName.toUpperCase();
            let changed = false;

            if (el.type === "radio") {
                const current = form.querySelector(`input[name="${el.name}"]:checked`);
                const currentValue = current ? current.value : null;
                if (currentValue !== (initialData[el.name] ?? null)) changed = true;
            } else if (el.type === "checkbox") {
                const checkboxes = form.querySelectorAll(`input[name="${el.name}"]`);
                if (checkboxes.length > 1) {
                    const currentValues = Array.from(checkboxes).filter(c => c.checked).map(c => c.value);
                    const initialValues = initialData[el.name] || [];
                    if (currentValues.length !== initialValues.length ||
                        currentValues.some(v => !initialValues.includes(v))) {
                        changed = true;
                    }
                } else {
                    if (el.checked !== initialData[el.name]) changed = true;
                }
            } else if (tag === "SELECT" && el.multiple) {
                const currentValues = Array.from(el.selectedOptions).map(opt => opt.value);
                const initialValues = initialData[el.name] || [];
                if (currentValues.length !== initialValues.length ||
                    currentValues.some(v => !initialValues.includes(v))) {
                    changed = true;
                }
            } else {
                if (el.value !== initialData[el.name]) changed = true;
            }

            if (changed) count++;
            checkedNames.add(el.name);
        });

        return count;
    }

    // Detect changes
    form.addEventListener("input", () => changedCount = getChangedCount());
    form.addEventListener("change", () => changedCount = getChangedCount());

    // Warn on tab close/refresh
    form.dataset.submitting = "false";
    function beforeUnloadHandler(e) {
        if (form.dataset.submitting == "false" && changedCount > 0) {
            e.preventDefault();
            e.returnValue = `You have ${changedCount} unsaved change(s). Are you sure you want to leave?`;
        }
    }

    window.addEventListener("beforeunload", beforeUnloadHandler);

    // Capture submit button clicks BEFORE validation
    form.querySelectorAll("[type=submit]").forEach(btn => {
        btn.addEventListener("click", () => {
            form.dataset.submitting = "true";
            console.log("Submit button clicked, submitting set to true");
        });
    });

    // Reset if form submit is blocked
    const confirmModalEl = document.getElementById("confirmModal");
    if (confirmModalEl) {
        confirmModalEl.setAttribute("data-bs-backdrop", "static"); // prevent closing by clicking outside
    }

    const cancelBtn = document.querySelector("#confirmModalNo");
    if (cancelBtn) {
        cancelBtn.addEventListener("click", () => {
            if (form.dataset.submitting == "true") {
                form.dataset.submitting = "false";
                console.log("Submit cancelled, submitting reset to false");
            }
        });
    }

    const closeBtn = document.querySelector("#confirmModal .btn-close");
    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            if (form.dataset.submitting == "true") {
                form.dataset.submitting = "false";
                console.log("Submit modal closed, submitting reset to false");
            }
        });
    }

    // hide.bs.modal
    //if (confirmModalEl) {
    //    confirmModalEl.addEventListener("hide.bs.modal", () => {
    //        if (submitting) {
    //            submitting = false;
    //            console.log("Submit modal dismissed, submitting reset to false");
    //        }
    //    });
    //}

    // Handle confirmed navigation
    function continueNavigation(href) {
        window.removeEventListener("beforeunload", beforeUnloadHandler);
        setTimeout(() => { window.location.href = href; }, 0);
    }

    // Intercept navigation links
    document.querySelectorAll(linkSelector).forEach(link => {
        link.addEventListener("click", function (e) {
            if (
                link.getAttribute("href") === "javascript:void(0);" ||
                link.hasAttribute("aria-controls")
            ) {
                return;
            }

            if (changedCount > 0) {
                e.preventDefault();
                showConfirmModal(
                    "Unsaved Changes",
                    `You have ${changedCount} unsaved change(s). Do you really want to leave?`,
                    () => continueNavigation(link.href)
                );
            }
        });
    });
}







