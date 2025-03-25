function createLineChart(canvasId, labels, data, annotationText, annotationIndex) {
    const ctx = document.getElementById(canvasId).getContext("2d");

    window[canvasId] = new Chart(ctx, {
        type: "line",
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Energy Consumption",
                    data: data,
                    borderColor: "teal",
                    borderWidth: 2,
                    pointBackgroundColor: "white",
                    pointBorderColor: "teal",
                    pointRadius: 4,
                    fill: false,
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true, // Automatically scale Y-axis
                }
            },
            plugins: {
                annotation: {
                    annotations: {
                        line1: {
                            type: "line",
                            scaleID: "x",
                            value: labels[annotationIndex],
                            borderColor: "red",
                            borderWidth: 2
                        }
                    }
                }
            }
        }
    });
}

function updateChart(canvasId, data) {
    if (window[canvasId]) {
        window[canvasId].data.datasets[0].data = data;
        window[canvasId].update();
    }
}
