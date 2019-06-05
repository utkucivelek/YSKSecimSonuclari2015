var layer = L.tileLayer('http://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, &copy; <a href="http://cartodb.com/attributions">CartoDB</a>, <a href="http://onikimaymun.org">12maymun</a>'
});

//center turkey
var map = L.map('map', { center: [39.523252, 34.703039], zoom: 6 });
map.addLayer(layer);

// create heat maps
var heatAKP = L.heatLayer([], { radius: 15, blur: 25 });
var heatCHP = L.heatLayer([], { radius: 15, blur: 25 });
var heatMHP = L.heatLayer([], { radius: 15, blur: 25 });
var heatHDP = L.heatLayer([], { radius: 15, blur: 25 });
var heatSP = L.heatLayer([], { radius: 15, blur: 25 });
var heatLDP = L.heatLayer([], { radius: 15, blur: 25 });
var heatVATAN = L.heatLayer([], { radius: 15, blur: 25 });

var overlayMaps = {
    "AKP": heatAKP,
    "CHP": heatCHP,
    "MHP": heatMHP,
    "HDP": heatHDP,
    "SAADETx10": heatSP,
    "VATANx100": heatVATAN,
    "LDPx100": heatLDP
};

var dataSources = {
    "AKP": "js/votesAKP.js",
    "CHP": "js/votesCHP.js",
    "MHP": "js/votesMHP.js",
    "HDP": "js/votesHDP.js",
    "SAADETx10": "js/votesSAADETx10.js",
    "VATANx100": "js/votesVATANx100.js",
    "LDPx100": "js/votesLDPx100.js"
};
var loadedData = {};

map.addControl(new L.Control.Layers({}, overlayMaps, { collapsed: false }));

// handle user layer elections
var currentPartyname = "";
map.on('overlayadd', function (e) {
    currentPartyname = e.name;
    if (loadedData[currentPartyname] != true) {
        loadScript(dataSources[e.name], loadCallback);
        loadedData[currentPartyname] = true;
    }
    // console.log(e);
});

var loadCallback = function () {
    overlayMaps[currentPartyname].setLatLngs(values);
};

function loadScript(url, callback) {
    // Adding the script tag to the head as suggested before
    var head = document.getElementsByTagName('head')[0];
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;

    // Then bind the event to the callback function.
    // There are several events for cross browser compatibility.
    script.onreadystatechange = callback;
    script.onload = callback;

    // Fire the loading
    head.appendChild(script);
}