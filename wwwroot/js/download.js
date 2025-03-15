// Función para descargar archivos desde Blazor
window.downloadFile = function (contentBase64, contentType, fileName) {
    // Decodificar el contenido Base64
    const binaryContent = atob(contentBase64);

    // Convertir a array de bytes
    const byteArray = new Uint8Array(binaryContent.length);
    for (let i = 0; i < binaryContent.length; i++) {
        byteArray[i] = binaryContent.charCodeAt(i);
    }

    // Crear un Blob con el contenido
    const blob = new Blob([byteArray], { type: contentType });

    // Crear un enlace de descarga
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;

    // Agregar el enlace al documento (invisible)
    document.body.appendChild(link);

    // Hacer clic en el enlace para iniciar la descarga
    link.click();

    // Eliminar el enlace después de un breve retraso
    setTimeout(() => {
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
    }, 100);

    return true;
};