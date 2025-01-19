

window.createLineChart = (canvasId, labels, data, annotationText, annotationIndex) => {
    const ctx = document.getElementById(canvasId).getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: '2024',
                data: data,
                fill: false,
                borderColor: 'rgba(75, 192, 192, 1)',
                
                tension: 0.4,
                pointBackgroundColor: 'rgba(75, 192, 192, 1)',
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                },
                tooltip: {
                    enabled: true,
                },
                annotation: {
                    annotations: {
                        label: {
                            type: 'label',
                            content: annotationText,
                            position: {
                                x: annotationIndex,
                                y: data[annotationIndex],
                            },
                            backgroundColor: '#ffffff',
                            color: 'rgb(24, 245, 245)',
                            padding: 4,
                            cornerRadius: 4,
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: false,
                    title: {
                        display: true,
                        text: 'Total Energy'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Months'
                    }
                }
            }
        }
    });
};
