function dataTableRender(dtURL, columns, options, methodUrl, deleteDataUrl) {
  $("#product").DataTable({
    autoWidth: false,
    processing: true,
    serverSide: true,
    responsive: true,
    lengthChange: true,
    colResize: options,
    ajax: {
      url: dtURL,
      method: methodUrl,
      data: function (d) {
        return d;
      },
    },
    createdRow: function (row, data, dataIndex, cells) {
      $(row).attr("data-id", row.id);
    },
    order: [5, "desc"],
    columns: columns,
    // buttons: ["copy", "csv", "excel", "pdf", "print", "colvis"]
  });

  $("#checkAll").click(function () {
    $("input:checkbox").not(this).prop("checked", this.checked);
  });

  $(document).on("click", ".delete-record", function (e) {
    var id = $(this).attr("data-id");
    var title = $(this).attr("data-title");
    let deleteUrl = deleteDataUrl;
    deleteUrl = deleteUrl.replace(":id", id);

    $("#formHapus").attr("action", deleteUrl);
    $("#content").html(`${title}`);
    $("#modal-delete").modal("show");
  });

  $(".frm-delete").submit(function (e) {
    //e.preventDefault();
    let tpl = $("#tpl-loading-icon").html();
    $(".delete").html(tpl);
    $(".delete").prop("disabled", true);
  });
}
