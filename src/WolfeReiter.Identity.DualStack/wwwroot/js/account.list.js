import "../lib/jquery/jquery.min.js";
import "../lib/datatables/js/dataTables.min.js";

"use strict";

(function () {
    var pageSize = 10,
        data = $("table#account-datatable").attr("data"),
        detail = $("table#account-datatable").attr("detail");

    $(function () {
        var accountTable = $("table#account-datatable")
            .DataTable({
                "ajax": {
                    "url": data,
                    "type": "GET",
                    "data": function (options) {
                        options.length = $("table#account-datatable").DataTable().page.len();
                        options.page = $("table#account-datatable").DataTable().page;
                        options.username = $("#username").val();
                        options.email = $("#email").val();
                        options.active = $("#active").val();
                    }
                },
                "autoWidth": false,
                "columnDefs": [
                    { "data": "userId",                                             "targets": 0, "visible": false },
                    { "data": "name",       "title": "Username", "orderable": true, "targets": 1 },
                    { "data": "email",      "title": "Email",    "orderable": true, "targets": 2 },
                    { "data": "userNumber", "title": "User #",   "orderable": true, "targets": 3 },
                    { "data": "active",     "title": "Active",   "orderable": true, "targets": 4 }
                ],
                "language": {
                    "info": "Showing _START_ to _END_ of _TOTAL_ records",
                    "lengthMenu": "Show _MENU_ records",
                    "infoEmpty": "Showing 0 records",
                    "zeroRecords": "No records found"
                },
                "lengthChange": true,
                'order': [[1, 'asc']],
                "pageLength": pageSize,
                "pagingType": "simple_numbers",
                "processing": true,
                "serverSide": true,
                "searching": true
            });

        $('#account-datatable tbody').on('click', 'tr', function () {
            window.open(detail + "/" + accountTable.row(this).data().userId, "_blank");
        });

        $('#account-datatable tbody').hover(function () {
            $(this).css('cursor', 'pointer');
        });
    });
})();