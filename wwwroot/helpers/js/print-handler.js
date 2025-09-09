function printDokumen(divSelector) {
	const table = document.getElementById(divSelector);
	const printWindow = window.open('', '', 'height=600,width=800');
	printWindow.document.write('<html><head><title>Print Dokumen</title>');
	
	// Ambil semua tag <style> dan <link rel="stylesheet">
	document.querySelectorAll('style, link[rel="stylesheet"]').forEach((node) => {
		printWindow.document.write(node.outerHTML);
	});

	printWindow.document.write('</head><body>');
	printWindow.document.write(table.outerHTML);
	printWindow.document.write('</body></html>');
	printWindow.document.close();
	printWindow.focus();
	printWindow.print();
	printWindow.close();
}
