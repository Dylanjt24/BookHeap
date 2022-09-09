var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#dataTbl').DataTable({
        "ajax": {
            "url": "/Admin/Products/GetAll"
        },
        "columns": [
            { "data": "title", "width": "15%" },
            { "data": "isbn", "width": "15%" },
            { "data": "price", "width": "15%" },
            { "data": "author", "width": "15%" },
            { "data": "category.name", "width": "15%" },
            { "data": "coverType.name", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Products/Upsert?productId=${data}" class="btn btn-light mx-2 border rounded-pill"><i class="bi bi-pencil-square"></i> Edit</a>
                            <a class="btn btn-danger mx-2 border rounded-pill"><i class="bi bi-trash-fill"></i> Delete</a>
                        </div>
                        `
                },
                "width": "15%"
            }
        ]
    });
}