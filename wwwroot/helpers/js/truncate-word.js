function truncateHtmlWords(htmlContent, wordLimit = 50) {
    // 1. Buat elemen dummy untuk menghilangkan tag HTML
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = htmlContent;

    // 2. Ambil teks tanpa tag
    const text = tempDiv.textContent || tempDiv.innerText || '';

    // 3. Potong berdasarkan kata
    const words = text.trim().split(/\s+/);

    if (words.length <= wordLimit) {
        return text;
    }

    const truncated = words.slice(0, wordLimit).join(' ') + '...';
    return truncated;
}
