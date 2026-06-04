/**
 * CartoBureau - Application Logic
 * Interactive dashboard mapping French departments, DITIB addresses, and road routing.
 */

// Application State
const state = {
    map: null,
    tileLayer: null,
    geojsonLayer: null,
    addressMarkers: [],
    selectedDept: 'all',
    searchQuery: '',
    activeAddressId: null,
    
    // Routing state
    routeLine: null,
    routeStartMarker: null,
    routeEndMarker: null
};

// Target Departments Mapping for quick reference
const DEPARTMENTS_INFO = {
    '25': { name: 'Doubs', color: '#3b82f6' },
    '52': { name: 'Haute-Marne', color: '#10b981' },
    '54': { name: 'Meurthe-et-Moselle', color: '#f59e0b' },
    '55': { name: 'Meuse', color: '#ef4444' },
    '57': { name: 'Moselle', color: '#8b5cf6' },
    '67': { name: 'Bas-Rhin', color: '#ec4899' },
    '68': { name: 'Haut-Rhin', color: '#06b6d4' },
    '70': { name: 'Haute-Saône', color: '#14b8a6' },
    '88': { name: 'Vosges', color: '#f97316' },
    '90': { name: 'Territoire de Belfort', color: '#6366f1' }
};

// Map Tile Layers Configuration
const TILE_URLS = {
    dark: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
    light: 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png',
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
};

/* ==========================================================================
   Initialization
   ========================================================================== */
document.addEventListener('DOMContentLoaded', () => {
    initTheme();
    initMap();
    loadDepartments();
    loadAddresses();
    populateRouteSelects();
    setupEventListeners();
});

/**
 * Theme Setup (Dark / Light)
 */
function initTheme() {
    const savedTheme = localStorage.getItem('theme') || 'dark';
    document.documentElement.setAttribute('data-theme', savedTheme);
    updateThemeToggleIcon(savedTheme);
}

function updateThemeToggleIcon(theme) {
    const btn = document.getElementById('theme-toggle');
    if (!btn) return;
    const icon = btn.querySelector('i');
    if (theme === 'dark') {
        icon.className = 'fa-solid fa-sun';
        btn.title = 'Passer au mode clair';
    } else {
        icon.className = 'fa-solid fa-moon';
        btn.title = 'Passer au mode sombre';
    }
}

function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute('data-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    updateThemeToggleIcon(newTheme);
    
    // Smoothly swap map tile layer
    if (state.tileLayer && state.map) {
        state.map.removeLayer(state.tileLayer);
        state.tileLayer = L.tileLayer(TILE_URLS[newTheme], {
            attribution: TILE_URLS.attribution,
            subdomains: 'abcd',
            maxZoom: 20
        }).addTo(state.map);
    }
}

/**
 * Leaflet Map Setup
 */
function initMap() {
    // Center of the 10 target departments (approx Epinal / Vosges area)
    const centerPoint = [48.15, 6.6]; 
    const initialZoom = 8;
    
    state.map = L.map('map', {
        zoomControl: true,
        attributionControl: true
    }).setView(centerPoint, initialZoom);
    
    // Set Tile Layer based on active theme
    const theme = document.documentElement.getAttribute('data-theme') || 'dark';
    state.tileLayer = L.tileLayer(TILE_URLS[theme], {
        attribution: TILE_URLS.attribution,
        subdomains: 'abcd',
        maxZoom: 20
    }).addTo(state.map);
}

/* ==========================================================================
   Data Loading and Polygon Rendering
   ========================================================================== */

/**
 * Renders department boundaries from GeoJSON variable
 */
function loadDepartments() {
    if (typeof DEPARTEMENTS_GEOJSON === 'undefined') {
        console.error('GeoJSON des départements introuvable. Assurez-vous que departements.js est chargé.');
        return;
    }
    
    // Style configuration for polygons
    const defaultStyle = {
        fillColor: 'rgba(59, 130, 246, 0.05)',
        weight: 1.5,
        opacity: 0.7,
        color: 'var(--accent-color)',
        dashArray: '4, 4',
        fillOpacity: 0.1
    };
    
    state.geojsonLayer = L.geoJSON(DEPARTEMENTS_GEOJSON, {
        style: (feature) => {
            const code = feature.properties.code;
            const deptInfo = DEPARTMENTS_INFO[code];
            if (deptInfo) {
                return {
                    ...defaultStyle,
                    color: deptInfo.color,
                    fillColor: deptInfo.color
                };
            }
            return defaultStyle;
        },
        onEachFeature: (feature, layer) => {
            const code = feature.properties.code;
            const name = feature.properties.nom || DEPARTMENTS_INFO[code]?.name || `Département ${code}`;
            
            // Text Tooltip (shows on hover)
            layer.bindTooltip(`${code} - ${name}`, {
                sticky: true,
                className: 'dept-tooltip',
                direction: 'top'
            });
            
            // Interaction listeners
            layer.on({
                mouseover: (e) => {
                    const l = e.target;
                    l.setStyle({
                        weight: 3,
                        dashArray: '',
                        fillOpacity: 0.25
                    });
                    if (!L.Browser.ie && !L.Browser.opera && !L.Browser.edge) {
                        l.bringToFront();
                    }
                },
                mouseout: (e) => {
                    const l = e.target;
                    state.geojsonLayer.resetStyle(l);
                    // Keep active markers on top
                    state.addressMarkers.forEach(m => m.bringToFront());
                },
                click: (e) => {
                    // Zoom to clicked department
                    state.map.fitBounds(e.target.getBounds(), { padding: [20, 20] });
                    // Set department filter
                    setDepartmentFilter(code);
                }
            });
        }
    }).addTo(state.map);
}

/**
 * Loads precise addresses and places them on the map
 */
function loadAddresses() {
    const listContainer = document.getElementById('cities-list');
    if (!listContainer) return;

    // Check if ADRESSES_DATA exists (global from adresses.js)
    if (typeof ADRESSES_DATA === 'undefined' || !Array.isArray(ADRESSES_DATA) || ADRESSES_DATA.length === 0) {
        listContainer.innerHTML = `
            <div class="empty-state">
                <i class="fa-solid fa-location-dot"></i>
                <p>Aucune adresse enregistrée.<br>Veuillez coller vos adresses dans le fichier Excel et exécuter le script.</p>
            </div>
        `;
        return;
    }
    
    // Clean up previous markers if any
    state.addressMarkers.forEach(m => state.map.removeLayer(m));
    state.addressMarkers = [];
    
    // Create markers and map info
    ADRESSES_DATA.forEach((addr, index) => {
        addr.id = `addr-${index}`;
        
        const lat = parseFloat(addr.lat);
        const lon = parseFloat(addr.lon);
        if (isNaN(lat) || isNaN(lon)) {
            console.warn(`Coordonnées invalides pour l'adresse : ${addr.name}`);
            return;
        }
        
        // Define Custom HTML Glowing Marker (Orange)
        const customIcon = L.divIcon({
            className: 'address-marker',
            iconSize: [14, 14],
            iconAnchor: [7, 7],
            html: `<div class="address-dot"></div>`
        });
        
        // Create Marker
        const marker = L.marker([lat, lon], { icon: customIcon });
        
        // Custom interactive HTML Popup for precise addresses
        const popupContent = `
            <div class="custom-popup">
                <div class="popup-header" style="background: linear-gradient(135deg, rgba(249, 115, 22, 0.2), rgba(239, 68, 68, 0.2))">
                    <h4 style="color: #f97316"><i class="fa-solid fa-map-pin"></i> ${addr.name}</h4>
                </div>
                <div class="popup-body">
                    <div class="popup-row">
                        <i class="fa-solid fa-location-dot" style="color: #f97316"></i>
                        <span>Adresse : <strong>${addr.resolvedAddress}</strong></span>
                    </div>
                    <div class="popup-row">
                        <i class="fa-solid fa-building-flag" style="color: #f97316"></i>
                        <span>Département : <strong>${addr.departmentName}</strong></span>
                        <span class="popup-badge" style="background-color: rgba(249, 115, 22, 0.15); color: #f97316">${addr.departmentCode}</span>
                    </div>
                    <div class="popup-coords">
                        Lat: ${addr.lat.toFixed(5)} | Lon: ${addr.lon.toFixed(5)}
                    </div>
                </div>
            </div>
        `;
        
        marker.bindPopup(popupContent, {
            maxWidth: 260,
            offset: [0, -5]
        });
        
        // Strip DITIB prefix from permanent tooltip label for cleaner map presentation
        const cleanLabelName = addr.name.replace(/^DITIB\s+/i, '');
        
        // Bind permanent label directly above the point
        marker.bindTooltip(cleanLabelName, {
            permanent: true,
            direction: 'top',
            className: 'addr-permanent-tooltip',
            offset: [0, -8]
        });
        
        marker.on('click', () => {
            focusOnItem(addr.id);
        });
        
        marker.on('popupclose', () => {
            removeSidebarHighlight();
        });
        
        marker.addressId = addr.id;
        marker.addressData = addr;
        
        marker.addTo(state.map);
        state.addressMarkers.push(marker);
    });
    
    // Zoom out map to show ALL markers immediately on load ("carte fixe")
    try {
        if (state.addressMarkers.length > 0) {
            const group = L.featureGroup(state.addressMarkers);
            state.map.fitBounds(group.getBounds(), { padding: [50, 50] });
        }
    } catch (e) {
        console.warn("Erreur lors de l'ajustement initial de la carte", e);
    }
    
    // Render UI list
    renderSidebarList();
}

/**
 * Populates the dropdown lists in the routing section
 */
function populateRouteSelects() {
    const startSelect = document.getElementById('route-start');
    const endSelect = document.getElementById('route-end');
    if (!startSelect || !endSelect) return;
    
    if (typeof ADRESSES_DATA === 'undefined' || !Array.isArray(ADRESSES_DATA)) return;
    
    // Sort addresses alphabetically by name
    const sorted = [...ADRESSES_DATA].sort((a, b) => a.name.localeCompare(b.name));
    
    // Create option items
    sorted.forEach(addr => {
        const optionStart = document.createElement('option');
        optionStart.value = addr.id;
        optionStart.textContent = addr.name.replace(/^DITIB\s+/i, '') + ` (${addr.postcode})`;
        
        const optionEnd = optionStart.cloneNode(true);
        
        startSelect.appendChild(optionStart);
        endSelect.appendChild(optionEnd);
    });
}

/* ==========================================================================
   UI Rendering & Synchronization
   ========================================================================== */

/**
 * Renders list items in the sidebar based on filters
 */
function renderSidebarList() {
    const listContainer = document.getElementById('cities-list');
    const countDisplay = document.getElementById('items-filtered-count');
    if (!listContainer) return;
    
    const hasAdresses = typeof ADRESSES_DATA !== 'undefined' && Array.isArray(ADRESSES_DATA);
    
    // Filter Adresses
    const filteredAdresses = hasAdresses ? ADRESSES_DATA.filter(addr => {
        const matchDept = state.selectedDept === 'all' || addr.departmentCode === state.selectedDept;
        const matchSearch = state.searchQuery === '' || 
                            addr.name.toLowerCase().includes(state.searchQuery.toLowerCase()) ||
                            addr.resolvedAddress.toLowerCase().includes(state.searchQuery.toLowerCase()) ||
                            addr.postcode.includes(state.searchQuery);
        return matchDept && matchSearch;
    }) : [];
    
    // Sync Adresses Markers visibility on the map
    state.addressMarkers.forEach(marker => {
        const data = marker.addressData;
        const matchDept = state.selectedDept === 'all' || data.departmentCode === state.selectedDept;
        const matchSearch = state.searchQuery === '' || 
                            data.name.toLowerCase().includes(state.searchQuery.toLowerCase()) ||
                            data.resolvedAddress.toLowerCase().includes(state.searchQuery.toLowerCase()) ||
                            data.postcode.includes(state.searchQuery);
        
        if (matchDept && matchSearch) {
            if (!state.map.hasLayer(marker)) {
                marker.addTo(state.map);
            }
        } else {
            if (state.map.hasLayer(marker)) {
                state.map.removeLayer(marker);
            }
        }
    });
    
    // Render list in sidebar
    countDisplay.textContent = `${filteredAdresses.length} adresse${filteredAdresses.length > 1 ? 's' : ''}`;
    
    if (filteredAdresses.length === 0) {
        listContainer.innerHTML = `
            <div class="empty-state">
                <i class="fa-solid fa-location-dot"></i>
                <p>Aucune adresse ne correspond à vos filtres.</p>
            </div>
        `;
        return;
    }
    
    listContainer.innerHTML = filteredAdresses.map(addr => {
        const color = '#f97316';
        const isActive = state.activeAddressId === addr.id ? 'active' : '';
        return `
            <div class="city-item ${isActive}" data-id="${addr.id}" role="listitem">
                <div class="city-item-info" style="max-width: 80%;">
                    <h4>${addr.name}</h4>
                    <p style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 100%;">
                        <i class="fa-solid fa-location-dot" style="color: ${color}"></i> ${addr.resolvedAddress}
                    </p>
                </div>
                <div class="city-item-badge" style="background: linear-gradient(135deg, ${color}, #ef4444)">
                    ${addr.departmentCode}
                </div>
            </div>
        `;
    }).join('');
    
    // Attach click events to items
    listContainer.querySelectorAll('.city-item').forEach(item => {
        item.addEventListener('click', () => {
            const id = item.getAttribute('data-id');
            focusOnItem(id);
        });
    });
    
    updateStatistics();
}

/**
 * Focuses map on an address, activates marker, opens popup
 */
function focusOnItem(id) {
    state.activeAddressId = id;
    const marker = state.addressMarkers.find(m => m.addressId === id);
    if (!marker) return;
    
    // Select and highlight item in sidebar
    highlightItemInSidebar(id);
    
    // Zoom and pan
    const latlng = marker.getLatLng();
    state.map.setView(latlng, 14);
    
    // Temporarily add active class to marker element
    const iconElement = marker.getElement();
    if (iconElement) {
        iconElement.classList.add('address-marker-active');
    }
    
    // Open popup
    marker.openPopup();
    
    // Scroll active item into view
    const activeItem = document.querySelector(`.city-item[data-id="${id}"]`);
    if (activeItem) {
        activeItem.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
}

/**
 * Highlights selected sidebar item
 */
function highlightItemInSidebar(id) {
    document.querySelectorAll('.city-item').forEach(item => {
        item.classList.remove('active');
        if (item.getAttribute('data-id') === id) {
            item.classList.add('active');
        }
    });
    
    state.addressMarkers.forEach(m => {
        const el = m.getElement();
        if (el) {
            if (m.addressId === id) {
                el.classList.add('address-marker-active');
            } else {
                el.classList.remove('address-marker-active');
            }
        }
    });
}

function removeSidebarHighlight() {
    state.activeAddressId = null;
    
    document.querySelectorAll('.city-item').forEach(item => {
        item.classList.remove('active');
    });
    
    state.addressMarkers.forEach(m => {
        const el = m.getElement();
        if (el) {
            el.classList.remove('address-marker-active');
        }
    });
}

/**
 * Sets active filters and handles UI synchronization
 */
function setDepartmentFilter(deptCode) {
    state.selectedDept = deptCode;
    
    // Update select dropdown
    const select = document.getElementById('dept-select');
    if (select) {
        select.value = deptCode;
    }
    
    // Update floating panel tags
    document.querySelectorAll('.tags-container .tag').forEach(tag => {
        if (tag.getAttribute('data-dept') === deptCode) {
            tag.classList.add('active');
        } else {
            tag.classList.remove('active');
        }
    });
    
    renderSidebarList();
}

/**
 * Updates Top Stats cards
 */
function updateStatistics() {
    const totalCountCard = document.getElementById('stat-total-count');
    if (totalCountCard) {
        const countAdresses = (typeof ADRESSES_DATA !== 'undefined' && Array.isArray(ADRESSES_DATA)) ? ADRESSES_DATA.length : 0;
        totalCountCard.textContent = countAdresses.toString();
    }
}

/* ==========================================================================
   Routing Logic (OSRM Integration)
   ========================================================================== */

/**
 * Calls OSRM Driving API and draws the route line
 */
async function calculateRoute() {
    const startId = document.getElementById('route-start').value;
    const endId = document.getElementById('route-end').value;
    
    if (!startId || !endId) {
        alert("Veuillez sélectionner un départ et une arrivée.");
        return;
    }
    
    if (startId === endId) {
        alert("Le point de départ et d'arrivée doivent être différents.");
        return;
    }
    
    // Get start & end data
    const startData = ADRESSES_DATA.find(a => a.id === startId);
    const endData = ADRESSES_DATA.find(a => a.id === endId);
    
    if (!startData || !endData) return;
    
    // Clear previous route
    clearRoute();
    
    // Show loading indicator
    const calcBtn = document.getElementById('btn-route-calculate');
    const originalText = calcBtn.innerHTML;
    calcBtn.disabled = true;
    calcBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Calcul...';
    
    // OSRM Public API URL
    // Format: driving/lon1,lat1;lon2,lat2
    const url = `https://router.project-osrm.org/route/v1/driving/${startData.lon},${startData.lat};${endData.lon},${endData.lat}?overview=full&geometries=geojson`;
    
    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error("Erreur de communication avec l'API OSRM");
        
        const data = await response.json();
        
        if (!data.routes || data.routes.length === 0) {
            alert("Aucun itinéraire routier trouvé entre ces deux adresses.");
            return;
        }
        
        const route = data.routes[0];
        const routeGeoJson = route.geometry;
        
        // 1. Draw Route Line (Bold glowing polyline)
        state.routeLine = L.geoJSON(routeGeoJson, {
            style: {
                color: '#f97316', // Glowing Orange route line
                weight: 6,
                opacity: 0.85,
                lineCap: 'round',
                lineJoin: 'round'
            }
        }).addTo(state.map);
        
        // 2. Add Start & End Markers highlights
        const greenIcon = L.divIcon({
            className: 'route-point-marker',
            iconSize: [16, 16],
            html: '<div style="background-color: #10b981; border: 2.5px solid #ffffff; width: 16px; height: 16px; border-radius: 50%; box-shadow: 0 0 10px rgba(16,185,129,0.8);"></div>'
        });
        const redIcon = L.divIcon({
            className: 'route-point-marker',
            iconSize: [16, 16],
            html: '<div style="background-color: #ef4444; border: 2.5px solid #ffffff; width: 16px; height: 16px; border-radius: 50%; box-shadow: 0 0 10px rgba(239,68,68,0.8);"></div>'
        });
        
        state.routeStartMarker = L.marker([startData.lat, startData.lon], { icon: greenIcon }).addTo(state.map);
        state.routeEndMarker = L.marker([endData.lat, endData.lon], { icon: redIcon }).addTo(state.map);
        
        // 3. Format results
        const distanceKm = (route.distance / 1000).toFixed(1); // distance in meters
        const durationSec = route.duration; // duration in seconds
        
        // Convert seconds to hours/minutes
        let durationText = '';
        const hours = Math.floor(durationSec / 3600);
        const minutes = Math.floor((durationSec % 3600) / 60);
        if (hours > 0) {
            durationText = `${hours} h ${minutes} min`;
        } else {
            durationText = `${minutes} min`;
        }
        
        // Update Results UI
        document.getElementById('route-distance').textContent = `${distanceKm} km`;
        document.getElementById('route-duration').textContent = durationText;
        
        // Show results panel
        const resultsBox = document.getElementById('routing-results');
        resultsBox.classList.remove('hidden');
        
        // Enable Clear button
        document.getElementById('btn-route-clear').disabled = false;
        
        // 4. Fit map bounds to show full route
        state.map.fitBounds(state.routeLine.getBounds(), { padding: [60, 60] });
        
    } catch (e) {
        console.error(e);
        alert("Une erreur s'est produite lors du calcul du trajet par la route.");
    } finally {
        calcBtn.disabled = false;
        calcBtn.innerHTML = originalText;
    }
}

/**
 * Clears the route layers and hides results panel
 */
function clearRoute() {
    if (state.routeLine) {
        state.map.removeLayer(state.routeLine);
        state.routeLine = null;
    }
    
    if (state.routeStartMarker) {
        state.map.removeLayer(state.routeStartMarker);
        state.routeStartMarker = null;
    }
    
    if (state.routeEndMarker) {
        state.map.removeLayer(state.routeEndMarker);
        state.routeEndMarker = null;
    }
    
    // Hide results panel
    document.getElementById('routing-results').classList.add('hidden');
    
    // Disable Clear button
    document.getElementById('btn-route-clear').disabled = true;
}

/**
 * Swaps selected start and end select values
 */
function swapRoutePoints() {
    const startSelect = document.getElementById('route-start');
    const endSelect = document.getElementById('route-end');
    
    const temp = startSelect.value;
    startSelect.value = endSelect.value;
    endSelect.value = temp;
    
    // If a route was already plotted, recompute immediately
    if (state.routeLine) {
        calculateRoute();
    }
}

/* ==========================================================================
   Event Handling
   ========================================================================== */
function setupEventListeners() {
    // Theme Toggle click
    const themeBtn = document.getElementById('theme-toggle');
    if (themeBtn) {
        themeBtn.addEventListener('click', toggleTheme);
    }
    
    // Routing button clicks
    const calculateBtn = document.getElementById('btn-route-calculate');
    if (calculateBtn) {
        calculateBtn.addEventListener('click', calculateRoute);
    }
    
    const clearBtn = document.getElementById('btn-route-clear');
    if (clearBtn) {
        clearBtn.addEventListener('click', clearRoute);
    }
    
    const swapBtn = document.getElementById('route-swap');
    if (swapBtn) {
        swapBtn.addEventListener('click', swapRoutePoints);
    }
    
    // Search input typing
    const searchInput = document.getElementById('city-search');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            state.searchQuery = e.target.value;
            renderSidebarList();
        });
    }
    
    // Department dropdown filter
    const deptSelect = document.getElementById('dept-select');
    if (deptSelect) {
        deptSelect.addEventListener('change', (e) => {
            setDepartmentFilter(e.target.value);
        });
    }
    
    // Floating panel tag clicks
    document.querySelectorAll('.tags-container .tag').forEach(tag => {
        tag.addEventListener('click', () => {
            const dept = tag.getAttribute('data-dept');
            if (state.selectedDept === dept) {
                setDepartmentFilter('all');
            } else {
                setDepartmentFilter(dept);
                zoomToDepartment(dept);
            }
        });
    });
    
    // Collapse Floating Panel
    const closeBtn = document.getElementById('close-floating');
    const panel = document.querySelector('.floating-panel');
    if (closeBtn && panel) {
        closeBtn.addEventListener('click', () => {
            panel.classList.toggle('collapsed');
        });
    }
}

/**
 * Helper to zoom the map to a specific department boundaries
 */
function zoomToDepartment(deptCode) {
    if (!state.geojsonLayer) return;
    
    let targetLayer = null;
    state.geojsonLayer.eachLayer(layer => {
        if (layer.feature && layer.feature.properties.code === deptCode) {
            targetLayer = layer;
        }
    });
    
    if (targetLayer) {
        state.map.fitBounds(targetLayer.getBounds(), { padding: [25, 25] });
    }
}
