// Keywords Analytics Page
const keywordId = window.location.pathname.split('/').pop();
let messageChart, conversionChart, hourlyChart, dayChart;

document.addEventListener('DOMContentLoaded', function() {
    loadAnalytics();
    setupEventListeners();
});

function setupEventListeners() {
    // Period change buttons
    const periodButtons = document.querySelectorAll('[data-period]');
    periodButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            changePeriod(this.dataset.period);
        });
    });

    // View contact buttons (delegated)
    const subscribersTable = document.getElementById('subscribersTable');
    if (subscribersTable) {
        subscribersTable.addEventListener('click', function(e) {
            const viewBtn = e.target.closest('[data-action="view-contact"]');
            if (viewBtn) {
                viewContact(viewBtn.dataset.phone);
            }
        });
    }
}

async function loadAnalytics() {
    try {
        // Mock data for demonstration
        const mockData = {
            keyword: 'JOIN',
            totalMessages: 1247,
            optIns: 1180,
            optOuts: 67,
            conversionRate: 94.6,
            messageVolume: [
                { date: '2024-03-14', messages: 145 },
                { date: '2024-03-15', messages: 168 },
                { date: '2024-03-16', messages: 192 },
                { date: '2024-03-17', messages: 156 },
                { date: '2024-03-18', messages: 178 },
                { date: '2024-03-19', messages: 203 },
                { date: '2024-03-20', messages: 205 }
            ],
            hourlyData: [12, 8, 5, 3, 2, 5, 15, 45, 67, 89, 92, 88, 95, 78, 72, 85, 93, 87, 65, 52, 38, 28, 20, 15],
            dayData: [156, 178, 192, 168, 203, 205, 145],
            recentSubscribers: [
                { phone: '+1234567890', status: 'Opted In', date: '2024-03-20 14:30', source: 'SMS' },
                { phone: '+1234567891', status: 'Opted In', date: '2024-03-20 13:15', source: 'SMS' },
                { phone: '+1234567892', status: 'Opted Out', date: '2024-03-20 11:45', source: 'SMS' },
                { phone: '+1234567893', status: 'Opted In', date: '2024-03-20 10:20', source: 'SMS' },
                { phone: '+1234567894', status: 'Opted In', date: '2024-03-19 16:30', source: 'SMS' }
            ]
        };
        
        updateStats(mockData);
        createCharts(mockData);
        renderSubscribers(mockData.recentSubscribers);
    } catch (error) {
        showNotification('Error loading analytics: ' + error.message, 'error');
    }
}

function updateStats(data) {
    document.getElementById('keywordName').textContent = data.keyword;
    document.getElementById('totalMessages').textContent = data.totalMessages.toLocaleString();
    document.getElementById('optIns').textContent = data.optIns.toLocaleString();
    document.getElementById('optOuts').textContent = data.optOuts.toLocaleString();
    document.getElementById('conversionRate').textContent = data.conversionRate.toFixed(1) + '%';
    
    const optInPercent = (data.optIns / data.totalMessages * 100).toFixed(1);
    const optOutPercent = (data.optOuts / data.totalMessages * 100).toFixed(1);
    document.getElementById('optInPercent').textContent = optInPercent + '%';
    document.getElementById('optOutPercent').textContent = optOutPercent + '%';
}

function createCharts(data) {
    // Message Volume Chart
    const messageCtx = document.getElementById('messageChart').getContext('2d');
    messageChart = new Chart(messageCtx, {
        type: 'line',
        data: {
            labels: data.messageVolume.map(d => d.date),
            datasets: [{
                label: 'Messages',
                data: data.messageVolume.map(d => d.messages),
                borderColor: 'rgb(13, 110, 253)',
                backgroundColor: 'rgba(13, 110, 253, 0.1)',
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            }
        }
    });

    // Conversion Pie Chart
    const conversionCtx = document.getElementById('conversionChart').getContext('2d');
    conversionChart = new Chart(conversionCtx, {
        type: 'doughnut',
        data: {
            labels: ['Opt-Ins', 'Opt-Outs'],
            datasets: [{
                data: [data.optIns, data.optOuts],
                backgroundColor: ['rgb(25, 135, 84)', 'rgb(220, 53, 69)']
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            }
        }
    });

    // Hourly Chart
    const hourlyCtx = document.getElementById('hourlyChart').getContext('2d');
    hourlyChart = new Chart(hourlyCtx, {
        type: 'bar',
        data: {
            labels: Array.from({length: 24}, (_, i) => `${i}:00`),
            datasets: [{
                label: 'Messages per Hour',
                data: data.hourlyData,
                backgroundColor: 'rgba(13, 110, 253, 0.7)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            }
        }
    });

    // Day of Week Chart
    const dayCtx = document.getElementById('dayChart').getContext('2d');
    dayChart = new Chart(dayCtx, {
        type: 'bar',
        data: {
            labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
            datasets: [{
                label: 'Messages per Day',
                data: data.dayData,
                backgroundColor: 'rgba(25, 135, 84, 0.7)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            }
        }
    });
}

function renderSubscribers(subscribers) {
    const tbody = document.querySelector('#subscribersTable tbody');
    
    if (!subscribers || subscribers.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No subscribers found</td></tr>';
        return;
    }
    
    let html = '';
    subscribers.forEach(sub => {
        const statusClass = sub.status === 'Opted In' ? 'success' : 'danger';
        html += `
            <tr>
                <td>${sub.phone}</td>
                <td><span class="badge bg-${statusClass}">${sub.status}</span></td>
                <td>${sub.date}</td>
                <td>${sub.source}</td>
                <td>
                    <button class="btn btn-sm btn-outline-primary" data-action="view-contact" data-phone="${sub.phone}">
                        <i class="bi bi-person"></i> View Contact
                    </button>
                </td>
            </tr>
        `;
    });
    tbody.innerHTML = html;
}

function changePeriod(period) {
    document.querySelectorAll('[data-period]').forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');
    showNotification(`Loading ${period} data... (Demo)`, 'info');
}

function viewContact(phone) {
    showNotification(`View contact details for ${phone} (Demo)`, 'info');
}
