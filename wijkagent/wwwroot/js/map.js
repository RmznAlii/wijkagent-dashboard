window._crimeMap = null;
window._markers = [];
window.dotnetHelper = null;

window.initCrimeMap = () => {
    if (window._crimeMap) return;

    window._crimeMap = L.map('crimeMap').setView([52.1, 5.3], 7);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap'
    }).addTo(window._crimeMap);

    // Klik event
    window._crimeMap.on('click', async function (e) {
        const lat = e.latlng.lat;
        const lng = e.latlng.lng;
        console.log(`Map clicked: ${lat}, ${lng}`);

        let postcode = "";
        let city = "";
        let province = "";

        try {
            // Reverse geocoding via Nominatim
            const url = `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&addressdetails=1`;
            const response = await fetch(url);
            const data = await response.json();

            if (data.address) {
                postcode = data.address.postcode || "";
                city = data.address.city || data.address.town || data.address.village || "";
                province = data.address.state || "";
            }
        } catch (err) {
            console.error("Reverse geocoding failed:", err);
        }

        if (window.dotnetHelper) {
            window.dotnetHelper.invokeMethodAsync('MapClicked', lat, lng, postcode, city, province)
                .catch(err => console.error('JS Interop error:', err));
        }
    });
};

window.setDotNetHelper = (helper) => {
    window.dotnetHelper = helper;
    console.log("DotNetObjectReference set");
};

const markerIcon = L.Icon.extend({
    options: {
        iconSize: [37, 37],
        iconAnchor: [12, 37]
    }
});

const markerGreen = new markerIcon({ iconUrl: '/images/green_marker.png' });
const markerPink = new markerIcon({ iconUrl: '/images/pink_marker.png' });
const markerBlue = new markerIcon({ iconUrl: '/images/blue_marker.png' });



window.addCrime = (lat, lng, description, type) => {
    if (!window._crimeMap) return;

    let markerColor = markerBlue; // fallback

    if (type === "Diefstal") markerColor = markerPink;     // Diefstal
    if (type === "Vandalisme") markerColor = markerBlue;     // Vandalisme
    if (type === "Overlast") markerColor = markerGreen;    // Overlast

    const marker = L.marker([lat, lng], { icon: markerColor }).addTo(window._crimeMap)
        .bindPopup(description);

    window._markers.push(marker);
    console.log(`Marker added: ${lat}, ${lng},${type},${description}`);
};

window.clearCrimes = () => {
    if (!window._crimeMap) return;

    window._markers.forEach(marker => window._crimeMap.removeLayer(marker));
    window._markers = [];
    console.log("All markers cleared");
};
