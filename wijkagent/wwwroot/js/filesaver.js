window.saveFileFromBytes = async (filename, base64Data) => {
    const data = Uint8Array.from(atob(base64Data), c => c.charCodeAt(0));
    const blob = new Blob([data]);

    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = filename;
    a.click();

    URL.revokeObjectURL(a.href);
};
