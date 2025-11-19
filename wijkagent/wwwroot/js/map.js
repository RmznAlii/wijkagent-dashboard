window._crimeMap = null;
window._markers = [];

window.initCrimeMap = () => {
    if (window._crimeMap) return; // al geïnitialiseerd

    window._crimeMap = L.map('crimeMap').setView([52.1, 5.3], 7);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19
    }).addTo(window._crimeMap);
};

// Voeg nieuw delict marker toe
window.addCrime = (lat, lng, description) => {
    if (!window._crimeMap) return;

    const marker = L.marker([lat, lng]).addTo(window._crimeMap)
        .bindPopup(description);

    window._markers.push(marker);
};

// Optioneel: verwijder alle markers
window.clearCrimes = () => {
    if (!window._crimeMap) return;

    window._markers.forEach(m => window._crimeMap.removeLayer(m));
    window._markers = [];
};
