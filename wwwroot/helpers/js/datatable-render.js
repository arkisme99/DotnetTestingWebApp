async function dataTableRender(
  dtURL,
  columns,
  options,
  methodUrl,
  deleteDataUrl,
  numOrder
) {
  // Tunggu sampai DataTable selesai inisialisasi
  const table = await new Promise((resolve) => {
    const dt = $("#product").DataTable({
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
      order: [numOrder, "desc"],
      columns: columns,
    });

    // DataTables punya event init
    dt.on("init", function () {
      resolve(dt);
    });
  });

  // Checkbox select all
  $("#checkAll").click(function () {
    $("input:checkbox").not(this).prop("checked", this.checked);
  });

  // Delete handler
  $(document).on("click", ".delete-record", function (e) {
    var id = $(this).attr("data-id");
    var title = $(this).attr("data-title");
    let deleteUrl = deleteDataUrl;
    deleteUrl = deleteUrl.replace(":id", id);

    $("#formHapus").attr("action", deleteUrl);
    $("#content").html(`${title}`);
    $("#modal-delete").modal("show");
  });

  // Form delete submit
  $(".frm-delete").submit(function (e) {
    let tpl = $("#tpl-loading-icon").html();
    $(".delete").html(tpl);
    $(".delete").prop("disabled", true);
  });

  return table; // biar bisa diakses di luar
}
