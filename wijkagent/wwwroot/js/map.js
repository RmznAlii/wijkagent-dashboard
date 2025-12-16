window._crimeMap = null;
window._markers = [];
window._tempMarker = null;
window.dotnetHelper = null;

window.setDotNetHelper = (helper) => {
    window.dotnetHelper = helper;
};

window.initCrimeMap = () => {
    const mapDiv = document.getElementById('crimeMap');
    if (!mapDiv) return;

    if (window._crimeMap) {
        window._crimeMap.remove();
        window._crimeMap = null;
        window._markers = [];
    }

    window._crimeMap = L.map('crimeMap').setView([52.1, 5.3], 7);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap'
    }).addTo(window._crimeMap);

    window._crimeMap.on('click', async function (e) {
        const lat = e.latlng.lat;
        const lng = e.latlng.lng;
        let postcode = "", city = "", province = "", street = "", houseNumber = "";

        try {
            const url = `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&addressdetails=1`;
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

        if (window.dotnetHelper) {
            window.dotnetHelper.invokeMethodAsync('MapClicked', lat, lng, postcode, city, province, street, houseNumber)
                .catch(err => console.error(err));
        }
    });
};

function getCrimeIcon(type) {
    let markerIcon = L.Icon.extend({ options: { iconSize: [37, 37], iconAnchor: [12, 37], popupAnchor: [1, -34] } });
    if (type === "Diefstal") return new markerIcon({ iconUrl: '/images/pink_marker.png' });
    if (type === "Vandalisme") return new markerIcon({ iconUrl: '/images/blue_marker.png' });
    if (type === "Overlast") return new markerIcon({ iconUrl: '/images/green_marker.png' });
    return new markerIcon({ iconUrl: '/images/blue_marker.png' });
}

// Nieuw: addCrime ontvangt nu ook het Crime ID zodat marker-click het juiste delict kan selecteren in Blazor
window.addCrime = (id, lat, lng, description, type) => {
    if (!window._crimeMap) return;

    const icon = getCrimeIcon(type);
    const marker = L.marker([lat, lng], { icon: icon })
        .addTo(window._crimeMap)
        .bindPopup(`<b>${type}</b><br/>${description}`);

    marker.on('click', () => {
        if (window.dotnetHelper) {
            window.dotnetHelper.invokeMethodAsync('MarkerClicked', id)
                .catch(err => console.error(err));
        }
    });

    window._markers.push(marker);
};

window.clearCrimes = () => {
    if (!window._crimeMap) return;
    window._markers.forEach(m => window._crimeMap.removeLayer(m));
    window._markers = [];
};

window.showCrimesFiltered = (crimeListJson) => {
    if (!window._crimeMap) return;

    window.clearCrimes();
    const crimes = JSON.parse(crimeListJson);

    crimes.forEach(crime => {
        const icon = getCrimeIcon(crime.Type);

        const marker = L.marker([crime.Lat, crime.Lng], { icon: icon })
            .addTo(window._crimeMap)
            .bindPopup(`<b>${crime.Type}</b><br/>${crime.Description}<br/>${crime.Street} ${crime.HouseNumber}, ${crime.Postcode} ${crime.City}`);

        marker.on('click', () => {
            if (window.dotnetHelper) {
                window.dotnetHelper.invokeMethodAsync('MarkerClicked', crime.Id)
                    .catch(err => console.error(err));
            }
        });

        window._markers.push(marker);
    });
};

window.placePinByAddress = async (street, houseNumber, postcode, city, province, type) => {
    if (!window._crimeMap) return;

    const parts = [];
    if (street) parts.push(street + (houseNumber ? ' ' + houseNumber : ''));
    if (postcode) parts.push(postcode);
    if (city) parts.push(city);
    if (province) parts.push(province);

    const query = encodeURIComponent(parts.join(', '));
    const url = `https://nominatim.openstreetmap.org/search?format=json&addressdetails=1&limit=1&q=${query}`;

    try {
        const response = await fetch(url, { headers: { 'User-Agent': 'wijkagent-app/1.0' } });
        const results = await response.json();

        if (results && results.length > 0) {
            const r = results[0];
            const lat = parseFloat(r.lat);
            const lng = parseFloat(r.lon);

            window._crimeMap.setView([lat, lng], 16);

            const icon = getCrimeIcon(type);

            const tempMarker = L.marker([lat, lng], { icon: icon })
                .addTo(window._crimeMap)
                .bindPopup(`${street || ""} ${houseNumber || ""} <br/> ${postcode || ""} ${city || ""}`)
                .openPopup();

            if (window._tempMarker) window._crimeMap.removeLayer(window._tempMarker);
            window._tempMarker = tempMarker;

            const addr = r.address || {};
            if (window.dotnetHelper) {
                window.dotnetHelper.invokeMethodAsync(
                    'MapClicked',
                    lat,
                    lng,
                    addr.postcode || postcode,
                    addr.city || city,
                    addr.state || province,
                    addr.road || street,
                    addr.house_number || houseNumber
                ).catch(err => console.error(err));
            }
        } else {
            alert("Adres niet gevonden.");
        }
    } catch (err) {
        console.error(err);
        alert("Fout bij zoeken van adres.");
    }
};
