const circuit_box = document.getElementById("circuit-box"); // box that you make the circuit in

let selectedComponent = null; // component currently selected by user
let idNum = 0; // id for the component
let lineNum = 0; // id for the line
let canDraw = false;
let isDrawing = false;
let dragMode = true;
let startPoint; // the starting connector
let connectorConnector;
let showingValues = false;


function allowDrop(event) {
    event.preventDefault();
}

function drag(event) {
    select(event);
    event.dataTransfer.setData("text", event.target.id);
    event.dataTransfer.setData("source", event.currentTarget.parentNode.id);
    let rect = event.currentTarget.getBoundingClientRect();
    if (event.currentTarget.parentNode.id == "circuit") {
        rect = event.currentTarget.parentNode.getBoundingClientRect();
    }
    event.dataTransfer.setData("mousePosX", event.clientX - rect.left);
    event.dataTransfer.setData("mousePosY", event.clientY - rect.top);
}

function drop(event) {
    event.preventDefault();
    let data = event.dataTransfer.getData("text");
    let draggableElement = document.getElementById(data);
    if (draggableElement && draggableElement.draggable) {
        // Clone the draggable element
        let clone;
        if (event.dataTransfer.getData("source") === "circuit-box") {
            clone = draggableElement
        } else if (event.dataTransfer.getData("source") === "circuit") {
            clone = draggableElement.parentNode;
            console.log(draggableElement.parentNode);
        } else { // if the component came from the left panel a new component is created
            clone = draggableElement.cloneNode(true)
            clone.id = clone.id + idNum;
            idNum++;

            let connectorL = document.createElement("div"); // create left connector
            connectorL.className = "wireConnector";
            connectorL.id = "L";
            connectorL.style.top = '20px';
            connectorL.style.left = '-5px';
            clone.appendChild(connectorL);
            connectorL.addEventListener('mousedown', startDrawing);
            connectorL.addEventListener('mouseup', connectConnectors);

            let connectorR = document.createElement("div"); // create right connector
            connectorR.className = "wireConnector";
            connectorR.id = "R";
            connectorR.style.top = '20px';
            connectorR.style.left = '45px';
            clone.appendChild(connectorR);
            connectorR.addEventListener('mousedown', startDrawing);
            connectorR.addEventListener('mouseup', connectConnectors);

            let label = document.createElement("div"); // create label
            label.className = "label";
            label.style.top = '55px';
            label.style.left = '0px';
            clone.appendChild(label);
        }

        // Calculate the position relative to the right panel
        let rect = event.currentTarget.getBoundingClientRect();
        let offsetX = event.clientX - rect.left;
        let offsetY = event.clientY - rect.top;

        // Set the position of the clone relative to the right panel
        clone.style.position = "absolute";
        clone.style.left = offsetX - event.dataTransfer.getData("mousePosX") + "px";
        clone.style.top = offsetY - event.dataTransfer.getData("mousePosY") + "px";

        // Append the clone to the right panel
        circuit_box.appendChild(clone);
    }
}

function select(event){
    if (selectedComponent){
        selectedComponent.style.boxShadow = "none";
    }
    selectedComponent = event.currentTarget
    if(selectedComponent && selectedComponent.id.length > 10){
        selectedComponent.style.boxShadow = "1px 1px 1px 1px #888888";
        if (selectedComponent.id[9] == 'B' || selectedComponent.id[9] == 'R' || selectedComponent.id[9] == 'L'){
            document.getElementById("rotate").disabled = false;
            document.getElementById("switchState").hidden = true;
            let valueIn = document.getElementById("valueIn");
            valueIn.hidden = false;
            valueIn.value = selectedComponent.getAttribute('value');
            let valueLabel = document.getElementById("valueLabel");
            valueLabel.hidden = false;
            if(selectedComponent.id[9] == 'B'){
                valueLabel.innerHTML = "Potential Difference(V):";
            } else {
                valueLabel.innerHTML = "Resistance(Ω):";
            }
        }
        else if (selectedComponent.id[9] == 'V' || selectedComponent.id[9] == 'A'){
            document.getElementById("rotate").disabled = false;
            document.getElementById("switchState").hidden = true;
            let valueIn = document.getElementById("valueIn");
            valueIn.hidden = true;
            document.getElementById("valueLabel").hidden = true;
        }
        else if (selectedComponent.id[9] == 'S'){
            document.getElementById("rotate").disabled = false;
            document.getElementById("switchState").hidden = false;
            let valueIn = document.getElementById("valueIn");
            valueIn.hidden = true;
            document.getElementById("valueLabel").hidden = true;
        }
        else {
            selectedComponent.style.boxShadow = "none";
            document.getElementById("rotate").disabled = true;
            document.getElementById("switchState").hidden = true;
            let valueIn = document.getElementById("valueIn");
            valueIn.hidden = true;
            document.getElementById("valueLabel").hidden = true;
        }
    }
    else {
        document.getElementById("rotate").disabled = true;
        document.getElementById("switchState").hidden = true;
        let valueIn = document.getElementById("valueIn");
        valueIn.hidden = true;
        document.getElementById("valueLabel").hidden = true;
    }
}

function changeValue(){
    selectedComponent.setAttribute('value', document.getElementById("valueIn").value);
    showingValues = !showingValues;
    showValues();
}

function switchState(){
    if (selectedComponent.id[9] == 'S'){
        if (selectedComponent.hasAttribute('closed')){
            selectedComponent.removeAttribute('closed');
            selectedComponent.style.backgroundImage = "url('../ComponentSprites/SwitchOpen.png')";
            document.getElementById("switchState").innerHTML = "close switch"
        } else {
            selectedComponent.setAttribute('closed', '')
            selectedComponent.style.backgroundImage = "url('../ComponentSprites/SwitchClosed.png')";
            document.getElementById("switchState").innerHTML = "open switch"
        }
        
        console.log(!selectedComponent.getAttribute('closed'));
    }
}

function rotate(){
    let angle = Number(selectedComponent.getAttribute("angle"));
    angle += 90;
    selectedComponent.style.transform = "rotate("+angle+"deg)";
    selectedComponent.setAttribute("angle", angle);
    for (childNode of selectedComponent.childNodes){
        childNode.style.transform = "rotate(-"+angle+"deg)";
    }

}

function showValues(){
    allComponents = getComponents();
    for (component of allComponents){
        label = component.getElementsByClassName("label")[0]
        if (label != null){
            if (!showingValues){
                if (component.className == "battery"){
                    label.style.visibility = "visible";
                    label.innerHTML = component.getAttribute("value") + "V";
                }
                if (component.className == "resistor" || component.className == "bulb"){
                    label.style.visibility = "visible";
                    label.innerHTML = component.getAttribute("value") + "Ω";
                }
            } else {
                if (component.className == "battery" || component.className == "resistor" || component.className == "bulb"){
                    label.style.visibility = "hidden";
                }
            }
        }
    }
    showingValues = !showingValues;
}

function toggleWire(event) {
    dragMode = !dragMode
    let components = getComponents().concat(Array.from(document.getElementsByClassName("wire")));
    for (component of components) {
        if (component.className != "wire") {
            component.draggable = dragMode;
        }
        if (component.draggable) {
            component.style.cursor = "move";
        } else {
            component.style.cursor = "auto";
        }

        connectors = component.getElementsByClassName("wireConnector");
        for (connector of connectors) {
            if (dragMode) {
                connector.style.visibility = "hidden";
            } else {
                connector.style.visibility = "visible";
            }
        }
    }
    canDraw = !canDraw;
}

function startDrawing(event) {
    event.preventDefault();
    console.log("start");
    if (canDraw) {
        isDrawing = true;
    }

    startPoint = event.target;
    event.stopPropagation();
}

// Event listener for mouse move event 
circuit_box.addEventListener('mousemove', (event) => {
    if (isDrawing) {
        drawWire(event.clientX - startPoint.getBoundingClientRect().left, event.clientY - startPoint.getBoundingClientRect().top);
    }
});

// Event listener for mouse up event 
circuit_box.addEventListener('mouseup', (e) => {
    select(e);
    console.log('stop');
    if (typeof connectorConnector !== 'undefined') {
        connectorConnector.style.visibility = 'visible';
    }
    isDrawing = false;
    lineNum++;
});

function connectConnectors(event) {
    console.log("stop on connector");
    console.log(connectorConnector);
    if(event.target == circuit_box){
        connectorConnector.style.visibility = 'visible';
    }
    else if (startPoint != event.target && connectorConnector != event.target && event.target.className == "wireConnector" && isDrawing) {
        console.log("!");
        let finalPosAdjx = 3;
        let finalPosAdjy = 3;
        if (event.target.getBoundingClientRect().left - startPoint.getBoundingClientRect().left > 0) {
            finalPosAdjx = 5;
        }
        if (event.target.getBoundingClientRect().top - startPoint.getBoundingClientRect().top > 0) {
            finalPosAdjy = 5;
        }
        drawWire(event.target.getBoundingClientRect().left + finalPosAdjx - startPoint.getBoundingClientRect().left, event.target.getBoundingClientRect().top + finalPosAdjy - startPoint.getBoundingClientRect().top);
        connectorConnector.style.visibility = 'visible';
        lineNum++;
        startPoint = connectorConnector;
        drawWire(event.target.getBoundingClientRect().left + finalPosAdjx - startPoint.getBoundingClientRect().left, event.target.getBoundingClientRect().top + finalPosAdjy - startPoint.getBoundingClientRect().top);
        connectorConnector.remove();
        addToCircuit(startPoint, event.target);
    }
    else if (typeof connectorConnector !== 'undefined') {
        connectorConnector.style.visibility = 'visible';
    }
    isDrawing = false;
    lineNum++;
    event.stopPropagation();
}


function addToCircuit(connectorA, connectorB) {
    let parentComponentA = connectorA, parentComponentB = connectorB;
    while (parentComponentA.id.slice(0, 9) != "component") {
        parentComponentA = parentComponentA.parentNode;
    }
    while (parentComponentB.id.slice(0, 9) != "component") {
        parentComponentB = parentComponentB.parentNode;
    }
    let circuitDiv;
    if (parentComponentA.parentNode.id == "circuit-box") {
        circuitDiv = document.createElement("div");
        circuitDiv.appendChild(parentComponentA);
        circuit_box.appendChild(circuitDiv);
    } else {
        circuitDiv = parentComponentA.parentNode;
    }
    let oldCircuit = parentComponentB.parentNode;
    if (parentComponentB.parentNode.id == "circuit-box") {
        circuitDiv.appendChild(parentComponentB);
    } else if (circuitDiv === oldCircuit) { } else {
        let oldCircuit = parentComponentB.parentNode;
        while (oldCircuit.childNodes.length != 0) {
            circuitDiv.appendChild(oldCircuit.childNodes[0]);
        }
        oldCircuit.remove();
    }
    circuitDiv.id = "circuit";
    circuit_box.appendChild(circuitDiv);

    // make connection 
    while (connectorB.id != 'L' && connectorB.id != 'R') {
        connectorB = connectorB.parentNode;
    }
    connectorA.setAttribute('connection', parentComponentB.id + connectorB.id);
    console.log(parentComponentB.id + connectorB.id)
}

// Function to draw wires
function drawWire(x2, y2) {
    let x1 = 4, y1 = 4;
    // Create a new div element
    if (document.getElementById("wire" + lineNum)) {
        document.getElementById("wire" + lineNum).remove();
    }

    let element = document.createElement('div');
    let connector = document.createElement("div");
    element.id = "wire" + lineNum;
    element.className = "wire";

    // Set the position of the element based on the coordinates
    element.style.position = 'absolute';
    if (x1 < x2) {
        element.style.left = x1 + 'px';
        element.style.width = x2 - x1 + 1 + 'px';
        connector.style.left = x2 - x1 - 5 + 'px';
    } else {
        element.style.right = -2 - (x1 + startPoint.getBoundingClientRect().left - startPoint.getBoundingClientRect().right) + 'px';
        element.style.width = x1 - x2 + 1 + 'px';
        connector.style.right = x1 - x2 - 5 + 'px';
    }
    if (y1 < y2) {
        element.style.top = y1 + 'px';
        element.style.height = y2 - y1 + 1 + 'px';
        connector.style.top = y2 - y1 - 5 + 'px';
    } else {
        element.style.bottom = -2 - (y1 + startPoint.getBoundingClientRect().top - startPoint.getBoundingClientRect().bottom) + 'px';
        element.style.height = y1 - y2 + 1 + 'px';
        connector.style.bottom = y1 - y2 - 5 + 'px';
    }
    let width = parseFloat(element.style.width);
    let height = parseFloat(element.style.height);
    if (width > height) {
        element.style.height = '2px';
        connector.style.top = '-4px';

    } else {
        element.style.width = '2px';
        connector.style.left = '-4px';
    }

    // Append the element to the connector 
    startPoint.appendChild(element);
    element.addEventListener('mousedown', (e) => { e.stopPropagation(); });

    // setup new connector
    connector.className = "wireConnector";
    connector.id = "connector" + lineNum;
    element.appendChild(connector);
    connector.addEventListener('mousedown', startDrawing);
    connector.style.visibility = 'hidden';
    connector.addEventListener('mouseup', connectConnectors);
    connectorConnector = connector;
}

function getComponents() {
    let components = [];
    let allElements = Array.from(document.getElementsByTagName("div"));
    for (element of allElements) {
        if (element.id.slice(0, 9) === "component") {
            components.push(element);
        }
    }
    return (components);
}

function makeJSON() {
    let output = [];
    for (component of getComponents()) {
        if (component.id.length > 10){
            let componentObj = new Object();
            componentObj.Name = component.id;
            componentObj.Value = component.getAttribute('value');
            componentObj.switchClosed = component.hasAttribute('closed');
            componentObj.Connectors = [];
            for (connector of component.childNodes) {
                if (connector.id){
                    let connectionsObj = new Object();
                    connectionsObj.Name = connector.id;
                    connectionsObj.Connections = connSearch(connector);
                    componentObj.Connectors.push(connectionsObj);
                }
            }
            output.push(componentObj);
        }
    }
    console.log(JSON.stringify(output));
    SendJSON(JSON.stringify(output));
}


function connSearch(connector) {
    let conns = [];
    if (connector && connector.hasChildNodes()) {
        for (wire of connector.childNodes) {
            let nextConn = wire.childNodes[0];
            if (nextConn && nextConn.getAttribute("connection")) {
                conns.push(nextConn.getAttribute("connection"));
            }
            conns = conns.concat(connSearch(nextConn));
        }
    }
    return conns;
}


function updateValues(values){
    allComponents = getComponents();
    for (component of allComponents){
        label = component.getElementsByClassName("label")[0]
        if (label != null){
            let compID = Number(component.id.slice(10));
            if (component.className == "voltmeter"){
                label.style.visibility = "visible";
                label.innerHTML = values[compID] + "V";
            }
            if (component.className == "ammeter"){
                label.style.visibility = "visible";
                label.innerHTML = values[compID] + "A";
            }
        }
    }
}
