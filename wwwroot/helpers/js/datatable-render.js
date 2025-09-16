async function dataTableRender(
  dtURL,
  columns,
  options,
  methodUrl,
  deleteDataUrl,
  numOrder,
  tableId
) {
  // Tunggu sampai DataTable selesai inisialisasi
  const table = await new Promise((resolve) => {
    const dt = $(`#${tableId}`).DataTable({
      autoWidth: false,
      processing: true,
      serverSide: true,
      responsive: true,
      lengthChange: true,
      searchDelay: 700,
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

  // Apply search per column
  table.columns().every(function () {
    var that = this;

    $("input", this.footer()).on("keyup change clear", function () {
      if (that.search() !== this.value) {
        that.search(this.value).draw();
      }
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
