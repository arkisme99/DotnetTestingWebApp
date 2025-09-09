function buildImageTag(routeTemplate, imageName, options = {}) {
    const {
        width = 70,
        height = 70,
        fallback = 'no-image.png',
        alt = '',
        className = ''
    } = options;

    const imageFile = imageName && imageName.trim() !== '' ? imageName : fallback;
    const url = routeTemplate.replace(':id', imageFile);

    return `<img src="${url}" width="${width}" height="${height}" alt="${alt}" class="${className}" />`;
}
