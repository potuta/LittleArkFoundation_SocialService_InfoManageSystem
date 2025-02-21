// modals.js

$('#detailsUserModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var userId = button.data('id');
    $.ajax({
        url: '/Users/Details/' + userId,
        type: 'GET',
        success: function (data) {
            $('#details-username').text(data.username);
            $('#details-email').text(data.email);
            $('#details-phoneNumber').text(data.phoneNumber);
            $('#details-passwordHash').text(data.passwordHash);
            $('#details-roleID').text(data.roleID);
            $('#details-createdAt').text(data.createdAt);
        }
    });
});

$('#editUserModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var userId = button.data('id');
    $.ajax({
        url: '/Users/Details/' + userId,
        type: 'GET',
        success: function (data) {
            $('#edit-username').val(data.username);
            $('#edit-email').val(data.email);
            $('#edit-phoneNumber').val(data.phoneNumber);
            $('#edit-passwordHash').val(data.passwordHash);
            $('#edit-roleID').val(data.roleID);
        }
    });
});

$('#deleteUserModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var userId = button.data('id');
    var form = $(this).find('form');
    form.attr('action', '/Users/Delete/' + userId);
});
