$(document).ready(function () {
    loadUserData();
});

function loadUserData() {
    dataTable = $('#myTable').DataTable({
        ordering: false,
        "ajax": {
            "url": '/UserManagement/GetTableData',
            "method": "GET",
            info: false,
            "datatype": "json",
            "async": true,
            "dataSrc": function (json) {
                console.log("Response Data:", json);  // Log the data returned from the server
                return json;  // Ensure this returns the user data array
            },
        },
        "columns": [
            {
                "render": function (data, type, row) {
                    return `<input type="checkbox" value="${row.email}"/>`;
                }
            },
            {
                data: 'name',
                name: "Name",
            },
            {
                data: 'email',
                name: "Email"
            },
            {
                data: 'isAdmin',
                name: "IsAdmin",
                render: function (data, type, row) {
                    if (data == true) {
                        return '<span class="text-danger">Admin</span>';
                    } else {
                        return '<span class="text-success">User</span>';
                    }
                }
            },
            {
                data: 'lockoutEnd',
                name: "Status",
                render: function (data, type, row) {
                    if (data && new Date(data) > new Date()) {
                        return '<span class="text-danger">Locked</span>';
                    } else {
                        return '<span class="text-success">Active</span>';
                    }
                }
            }
        ]
    });
}

function toggleSelectAll() {
    // Get the state of the "Select All" checkbox
    var isChecked = $('#select-all').prop('checked');
    // Apply the same state to all checkboxes in the table body
    $('#myTable tbody input[type="checkbox"]').each(function () {
        $(this).prop('checked', isChecked);
    });
}


$('#deleteBtn').on('click', function () {
    let val = [];

    $('#myTable tbody :checkbox:checked').each(function (i) {
        val[i] = $(this).val();
    });

    $.ajax({
        type: 'POST',
        url: '/usermanagement/DeleteUser',
        data: { 'email': val },
        success: function () {
            dataTable.ajax.reload();
        },
        error: function () {
            console.log('Error deleting users');
        }
    });
});
$('#MakeAdmin').on('click', function () {
    let val = [];

    $('#myTable tbody :checkbox:checked').each(function (i) {
        val[i] = $(this).val();
    });

    $.ajax({
        type: 'POST',
        url: '/usermanagement/MakeAdmin',
        data: { 'email': val },
        success: function () {
            dataTable.ajax.reload();
        },
        error: function () {
            console.log('Error deleting users');
        }
    });
});
$('#RemoveFromAdmin').on('click', function () {
    let val = [];

    $('#myTable tbody :checkbox:checked').each(function (i) {
        val[i] = $(this).val();
    });

    $.ajax({
        type: 'POST',
        url: '/usermanagement/RemoveFromAdmin',
        data: { 'email': val },
        success: function () {
            dataTable.ajax.reload();
            location.reload();
        },
        error: function () {
            console.log('Error deleting users');
        }
    });
});


$('#BlockBtn').on('click', function () {
    let val = [];

    // Collect selected checkbox values
    $('#myTable tbody :checkbox:checked').each(function (i) {
        val[i] = $(this).val();
    });

    // Perform AJAX request
    $.ajax({
        type: 'POST',
        url: '/usermanagement/BlockUser',
        data: { 'email': val },
        success: function () {
            // Reload the data table first
            dataTable.ajax.reload();

            // Reload the page after a short delay

        },
        error: function (xhr, status, error) {
            console.error("An error occurred:", error);
            /*alert("Failed to block users. Please try again.");*/
        }

    });
    dataTable.ajax.reload();
    setTimeout(function () {
        location.reload();
    }, 4000);
    /*datatype.ajax.reload();*/// Delay in milliseconds (e.g., 500ms)
});


$('#UnBlockBtn').on('click', function () {
    let val = [];

    $('#myTable tbody :checkbox:checked').each(function (i) {
        val[i] = $(this).val();
    });

    $.ajax({
        type: 'POST',
        url: '/usermanagement/UnBlockUser',
        data: { 'email': val },
        success: function () {
            dataTable.ajax.reload();
        },
        error: function () {
            console.log('Error unblocking users');
        }
    });
});