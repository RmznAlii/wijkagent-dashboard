// Globale variabelen
window._crimeMap = null;
window._markers = [];
window.dotnetHelper = null;

// Kaart initialiseren
window.initCrimeMap = () => {
    // Als er nog een oude map bestaat (bijv. na navigatie), eerst opruimen
    if (window._crimeMap) {
        window._crimeMap.remove();   // Leaflet cleanup
        window._crimeMap = null;
        window._markers = [];
    }

    // Nieuwe map koppelen aan de huidige <div id="crimeMap">
    window._crimeMap = L.map('crimeMap').setView([52.1, 5.3], 7);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap'
    }).addTo(window._crimeMap);

    // Klik-event op de kaart
    window._crimeMap.on('click', async function (e) {
        const lat = e.latlng.lat;
        const lng = e.latlng.lng;
        console.log(`Map clicked: ${lat}, ${lng}`);

        let postcode = "";
        let city = "";
        let province = "";
        let street = "";
        let houseNumber = "";

        try {
            // Reverse geocoding via Nominatim
            const url =
                `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&addressdetails=1`;
            const response = await fetch(url);
            const data = await response.json();

            if (data && data.address) {
                postcode = data.address.postcode || "";
                city = data.address.city || data.address.town || data.address.village || "";
                province = data.address.state || "";
                street = data.address.road || data.address.pedestrian || data.address.cycleway || "";
                houseNumber = data.address.house_number || "";
            }
        } catch (err) {
            console.error("Reverse geocoding failed:", err);
        }

        // Meld locatie terug naar Blazor inclusief street + houseNumber
        if (window.dotnetHelper) {
            window.dotnetHelper
                .invokeMethodAsync(
                    'MapClicked',
                    lat,
                    lng,
                    postcode,
                    city,
                    province,
                    street,
                    houseNumber
                )
                .catch(err => console.error('JS Interop error:', err));
        }
    });
};

// DotNet helper (voor JS → C# calls)
window.setDotNetHelper = (helper) => {
    window.dotnetHelper = helper;
    console.log("DotNetObjectReference set");
};

// Geocode adres (straat + huisnummer + postcode/plaats/provincie) en plaats pin
window.placePinByAddress = async (street, houseNumber, postcode, city, province) => {
    if (!window._crimeMap) return;

    const parts = [];
    if (street) parts.push(street + (houseNumber ? ' ' + houseNumber : ''));
    if (postcode) parts.push(postcode);
    if (city) parts.push(city);
    if (province) parts.push(province);
    const query = encodeURIComponent(parts.join(', '));
    const url =
        `https://nominatim.openstreetmap.org/search?format=json&addressdetails=1&limit=1&q=${query}`;

    try {
        const response = await fetch(url, {
            headers: {
                'User-Agent': 'wijkagent-app/1.0 (your-email@example.com)'
            }
        });
        const results = await response.json();

        if (results && results.length > 0) {
            const r = results[0];
            const lat = parseFloat(r.lat);
            const lng = parseFloat(r.lon);

            const desc =
                `${street || (r.display_name || "")} ${houseNumber || ""} <br/> ${postcode || ""} ${city || ""}`
                    .trim();

            // Blauwe marker voor handmatig geplaatste pin
            const marker = L
                .marker([lat, lng], { icon: markerBlue })
                .addTo(window._crimeMap)
                .bindPopup(desc);

            window._markers.push(marker);
            window._crimeMap.setView([lat, lng], 16);

            // Probeer adresgegevens uit response te halen als velden leeg zijn
            const addr = r.address || {};
            const foundPostcode = addr.postcode || postcode || "";
            const foundCity = addr.city || addr.town || addr.village || city || "";
            const foundProvince = addr.state || province || "";
            const foundStreet = addr.road || addr.pedestrian || addr.cycleway || street || "";
            const foundHouse = addr.house_number || houseNumber || "";

            // Meld locatie terug naar Blazor zodat lat/lng en adresvelden updaten
            if (window.dotnetHelper) {
                window.dotnetHelper
                    .invokeMethodAsync(
                        'MapClicked',
                        lat,
                        lng,
                        foundPostcode,
                        foundCity,
                        foundProvince,
                        foundStreet,
                        foundHouse
                    )
                    .catch(err => console.error('JS Interop error:', err));
            }
        } else {
            console.warn("Geen resultaten voor adres:", query);
            alert("Adres niet gevonden, probeer andere zoekgegevens.");
        }
    } catch (err) {
        console.error("Geocoding failed:", err);
        alert("Fout bij zoeken van adres. Kijk console voor details.");
    }
};

// Icon class en varianten
const markerIcon = L.Icon.extend({
    options: {
        iconSize: [37, 37],
        iconAnchor: [12, 37],
        popupAnchor: [1, -34]
    }
});

const markerGreen = new markerIcon({ iconUrl: '/images/green_marker.png' });
const markerPink = new markerIcon({ iconUrl: '/images/pink_marker.png' });
const markerBlue = new markerIcon({ iconUrl: '/images/blue_marker.png' });

// Voeg misdaad-marker toe (aangeroepen vanuit Blazor: addCrime)
window.addCrime = (lat, lng, description, type) => {
    if (!window._crimeMap) return;

    // fallback is blue
    let markerColor = markerBlue;

    if (type === "Diefstal") markerColor = markerPink;     // Diefstal -> pink
    if (type === "Vandalisme") markerColor = markerBlue;   // Vandalisme -> blue
    if (type === "Overlast") markerColor = markerGreen;    // Overlast -> green

    const marker = L
        .marker([lat, lng], { icon: markerColor })
        .addTo(window._crimeMap)
        .bindPopup(description);

    window._markers.push(marker);
    console.log(`Marker added: ${lat}, ${lng}, ${description}, ${type}`);
};

// Alle markers van de kaart verwijderen (maar map laten staan)
window.clearCrimes = () => {
    if (!window._crimeMap) return;

    window._markers.forEach(marker => window._crimeMap.removeLayer(marker));
    window._markers = [];
    console.log("All markers cleared");
};

// (optioneel) volledige cleanup, kun je vanuit Blazor.Dispose() aanroepen
window.disposeCrimeMap = () => {
    if (window._crimeMap) {
        window._crimeMap.remove();
        window._crimeMap = null;
    }
    window._markers = [];
    window.dotnetHelper = null;
    console.log("Crime map disposed");
};
