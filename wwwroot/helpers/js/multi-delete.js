function hapusmulti(deleteUrl) {
  var id = [];

  $(":checkbox:checked").each(function (i) {
    id[i] = $(this).val();
  });

  if (id.length === 0) {
  } else {
    $("#datahapus").val(id);

    $("#formHapusMulti").attr("action", deleteUrl);
    $("#modal-multidelete").modal("show");
  }
}
