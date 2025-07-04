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

window.addEventListener('DOMContentLoaded', () => {
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
window.onclick = function (event) {
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






