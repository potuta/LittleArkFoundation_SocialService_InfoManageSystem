function toggleDropdown() {
    document.getElementById("dropdown-profile-menu").classList.toggle("show");
}

function toggleSidebar() {
    document.body.classList.toggle('sidebar-collapsed');
}

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






