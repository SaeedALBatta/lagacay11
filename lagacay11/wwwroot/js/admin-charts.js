// Legacy Eleven - Admin Chart.js Graphs for Dark Luxury Theme

document.addEventListener("DOMContentLoaded", function () {
    // 1. Sales Trend Chart (Line Chart)
    const salesCanvas = document.getElementById("salesTrendChart");
    if (salesCanvas) {
        new Chart(salesCanvas, {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
                datasets: [{
                    label: 'Monthly Sales ($)',
                    data: [1200, 1900, 2400, 3100, 4200, 5600],
                    borderColor: '#f2ca50', // Trophy Gold
                    backgroundColor: 'rgba(242, 202, 80, 0.04)',
                    borderWidth: 2,
                    tension: 0.35,
                    fill: true,
                    pointBackgroundColor: '#f2ca50',
                    pointBorderColor: '#111318',
                    pointHoverBackgroundColor: '#ffffff',
                    pointHoverBorderColor: '#f2ca50',
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: '#1e2024',
                        titleColor: '#ffffff',
                        bodyColor: '#e2e2e8',
                        borderColor: 'rgba(255,255,255,0.1)',
                        borderWidth: 1,
                        padding: 10,
                        displayColors: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(255, 255, 255, 0.05)'
                        },
                        ticks: {
                            color: '#d0c5af',
                            font: {
                                family: "'Inter', sans-serif"
                            },
                            callback: function(value) {
                                return '$' + value;
                            }
                        }
                    },
                    x: {
                        grid: {
                            color: 'rgba(255, 255, 255, 0.05)'
                        },
                        ticks: {
                            color: '#d0c5af',
                            font: {
                                family: "'Inter', sans-serif"
                            }
                        }
                    }
                }
            }
        });
    }

    // 2. Order Status Chart (Doughnut Chart)
    const statusCanvas = document.getElementById("orderStatusChart");
    if (statusCanvas) {
        new Chart(statusCanvas, {
            type: 'doughnut',
            data: {
                labels: ['Processing', 'Completed', 'Cancelled'],
                datasets: [{
                    data: [4, 22, 2],
                    backgroundColor: [
                        '#f2ca50', // Processing Gold
                        '#4ae183', // Completed Green
                        '#ffb4ab'  // Cancelled Red
                    ],
                    borderColor: '#111318',
                    borderWidth: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            boxWidth: 12,
                            color: '#e2e2e8',
                            font: {
                                size: 11,
                                family: "'Inter', sans-serif"
                            },
                            padding: 15
                        }
                    },
                    tooltip: {
                        backgroundColor: '#1e2024',
                        titleColor: '#ffffff',
                        bodyColor: '#e2e2e8',
                        borderColor: 'rgba(255,255,255,0.1)',
                        borderWidth: 1,
                        padding: 10
                    }
                }
            }
        });
    }
});
