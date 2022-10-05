var dataTable;

$(document).ready(function () {
    // Grab search parameters from url and check status
    var url = window.location.search;
    if (url.includes("processing"))
        loadDataTable("processing");
    else if (url.includes("pending"))
        loadDataTable("pending");
    else if (url.includes("completed"))
        loadDataTable("completed");
    else if (url.includes("approved"))
        loadDataTable("approved");
    else
        loadDataTable("all");
});

function loadDataTable(status) {
    dataTable = $('#dataTbl').DataTable({
        "ajax": {
            // Append the passed in status argument to the url search parameters
            "url": "/Admin/Orders/GetAll?status=" + status
        },
        "columns": [
            { "data": "orderHeaderId", "width": "5%" },
            { "data": "name", "width": "25%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "25%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "orderHeaderId",
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