var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#dataTbl').DataTable({
        "ajax": {
            "url": "/Admin/Orders/GetAll"
        },
        "columns": [
            { "data": "orderHeaderId", "width": "5%" },
            { "data": "name", "width": "25%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "25%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Orders/Details?orderId=${data}" class="btn btn-primary mx-2 rounded-pill"><i class="bi bi-pencil-square"></i></a>
                        </div>
                        `
                },
                "width": "5%"
            }
        ]
    });
}