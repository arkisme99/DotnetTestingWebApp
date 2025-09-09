function formatDateTimeYMDHIS(inputDate) {
    const date = new Date(inputDate);

    if (isNaN(date.getTime())) return ''; // cek jika input tidak valid

    const pad = (n) => n.toString().padStart(2, '0');

    const year   = date.getFullYear();
    const month  = pad(date.getMonth() + 1);
    const day    = pad(date.getDate());
    const hour   = pad(date.getHours());
    const minute = pad(date.getMinutes());
    const second = pad(date.getSeconds());

    return `${year}-${month}-${day} ${hour}:${minute}:${second}`;
}
