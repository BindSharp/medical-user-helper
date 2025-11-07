/**
 * Main application logic
 * Medical Users Helper - Developer Tool
 */

// State management
const appState = {
    currentPage: 'dashboard',
    stats: {
        npiCount: 0,
        deaCount: 0,
        accountCount: 0
    },
    npiHistory: [],
    deaHistory: [],
    accounts: []
};

// Wait for DOM to be ready
document.addEventListener('DOMContentLoaded', () => {
    console.log('Medical Users Helper initialized');

    setupNavigation();
    setupListeners();
    setupUI();
    loadDashboardData();
});

// ========================================
// NAVIGATION
// ========================================

function setupNavigation() {
    // Handle sidebar navigation
    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const pageName = item.getAttribute('data-page');
            navigateToPage(pageName);
        });
    });

    // Handle sidebar toggle
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    sidebarToggle?.addEventListener('click', () => {
        sidebar.classList.toggle('collapsed');
    });

    // Handle quick action buttons
    document.querySelectorAll('[data-navigate]').forEach(btn => {
        btn.addEventListener('click', () => {
            const page = btn.getAttribute('data-navigate');
            navigateToPage(page);
        });
    });

    // Mobile responsive - close sidebar on navigation
    if (window.innerWidth <= 768) {
        navItems.forEach(item => {
            item.addEventListener('click', () => {
                sidebar.classList.remove('mobile-open');
            });
        });
    }
}

function navigateToPage(pageName) {
    // Update nav items
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });
    document.querySelector(`[data-page="${pageName}"]`)?.classList.add('active');

    // Update pages
    document.querySelectorAll('.page').forEach(page => {
        page.classList.remove('active');
    });
    document.getElementById(`page-${pageName}`)?.classList.add('active');

    appState.currentPage = pageName;

    // Load page-specific data
    switch(pageName) {
        case 'dashboard':
            loadDashboardData();
            break;
        case 'npi-generator':
            refreshNPIHistory();
            break;
        case 'dea-generator':
            refreshDEAHistory();
            break;
        case 'accounts':
            loadAccounts();
            break;
    }
}

// ========================================
// LISTENERS (C# -> JS)
// ========================================

function setupListeners() {
    // Dashboard stats
    photinoBridge.on('dashboard:stats', (data) => {
        appState.stats = data;
        updateDashboardStats();
    });

    photinoBridge.on('dashboard:activity', (data) => {
        updateRecentActivity(data.activities);
    });

    // NPI Generator
    photinoBridge.on('npi:generated', (data) => {
        displayNPIResult(data);
        appState.npiHistory.unshift(data);
        appState.stats.npiCount++;
        updateDashboardStats();
        addToActivity('NPI', `Generated ${data.type} NPI: ${data.npi}`);
    });

    photinoBridge.on('npi:history', (data) => {
        appState.npiHistory = data.history;
        renderNPIHistory();
    });

    // DEA Generator
    photinoBridge.on('dea:generated', (data) => {
        displayDEAResult(data);
        appState.deaHistory.unshift(data);
        appState.stats.deaCount++;
        updateDashboardStats();
        addToActivity('DEA', `Generated DEA: ${data.dea}`);
    });

    photinoBridge.on('dea:history', (data) => {
        appState.deaHistory = data.history;
        renderDEAHistory();
    });

    // Account Manager
    photinoBridge.on('accounts:list', (data) => {
        appState.accounts = data.accounts;
        appState.stats.accountCount = data.accounts.length;
        updateDashboardStats();
        renderAccounts();
    });

    photinoBridge.on('account:saved', (data) => {
        showToast('Account saved successfully!', 'success');
        clearAccountForm();
        loadAccounts();
    });

    photinoBridge.on('account:deleted', (data) => {
        showToast('Account deleted', 'success');
        loadAccounts();
    });

    photinoBridge.on('account:error', (data) => {
        showToast(data.message || 'An error occurred', 'error');
    });

    // General error handler
    photinoBridge.on('error', (errorMessage) => {
        showToast(errorMessage, 'error');
    });
}

// ========================================
// UI SETUP
// ========================================

function setupUI() {
    // NPI Individual Form
    document.getElementById('npiIndividualForm')?.addEventListener('submit', (e) => {
        e.preventDefault();
        const firstName = document.getElementById('npiIndFirstName').value;
        const lastName = document.getElementById('npiIndLastName').value;

        photinoBridge.send('npi', `generate:individual:${JSON.stringify({ firstName, lastName })}`);
    });

    // NPI Organization Form
    document.getElementById('npiOrganizationForm')?.addEventListener('submit', (e) => {
        e.preventDefault();
        const orgName = document.getElementById('npiOrgName').value;

        photinoBridge.send('npi', `generate:organization:${JSON.stringify({ name: orgName })}`);
    });

    // DEA Form
    document.getElementById('deaForm')?.addEventListener('submit', (e) => {
        e.preventDefault();
        const firstName = document.getElementById('deaFirstName').value;
        const lastName = document.getElementById('deaLastName').value;
        const type = document.getElementById('deaType').value;

        if (!type) {
            showToast('Please select a registrant type', 'warning');
            return;
        }

        photinoBridge.send('dea', `generate:${JSON.stringify({ firstName, lastName, type })}`);
    });

    // Account Form
    document.getElementById('accountForm')?.addEventListener('submit', (e) => {
        e.preventDefault();
        const formData = {
            id: document.getElementById('accountId').value,
            platform: document.getElementById('accountPlatform').value,
            username: document.getElementById('accountUsername').value,
            password: document.getElementById('accountPassword').value,
            notes: document.getElementById('accountNotes').value
        };

        const action = formData.id ? 'update' : 'create';
        photinoBridge.send('account', `${action}:${JSON.stringify(formData)}`);
    });

    // Clear Account Form
    document.getElementById('clearAccountForm')?.addEventListener('click', clearAccountForm);

    // Refresh Accounts
    document.getElementById('refreshAccounts')?.addEventListener('click', loadAccounts);

    // Account Search
    document.getElementById('accountSearch')?.addEventListener('input', (e) => {
        filterAccounts(e.target.value);
    });

    // Password toggle buttons
    document.querySelectorAll('.toggle-password').forEach(btn => {
        btn.addEventListener('click', () => {
            const targetId = btn.getAttribute('data-target');
            const input = document.getElementById(targetId);
            if (input.type === 'password') {
                input.type = 'text';
                btn.textContent = '🙈';
            } else {
                input.type = 'password';
                btn.textContent = '👁️';
            }
        });
    });
}

// ========================================
// DASHBOARD
// ========================================

function loadDashboardData() {
    photinoBridge.send('dashboard', 'stats');
    photinoBridge.send('dashboard', 'activity');
}

function updateDashboardStats() {
    document.getElementById('npiCount').textContent = appState.stats.npiCount;
    document.getElementById('deaCount').textContent = appState.stats.deaCount;
    document.getElementById('accountCount').textContent = appState.stats.accountCount;
}

function updateRecentActivity(activities) {
    const container = document.getElementById('recentActivity');
    if (!activities || activities.length === 0) {
        container.innerHTML = '<p class="text-muted text-center">No recent activity</p>';
        return;
    }

    container.innerHTML = activities.map(activity => `
        <div class="activity-item">
            <div>
                <strong>${activity.type}</strong>
                <p class="text-small text-muted">${activity.description}</p>
            </div>
            <span class="text-small text-muted">${formatTimeAgo(activity.timestamp)}</span>
        </div>
    `).join('');
}

function addToActivity(type, description) {
    // This would be stored and synced with backend
    const activity = {
        type,
        description,
        timestamp: new Date().toISOString()
    };

    // For now, just log it
    console.log('Activity:', activity);
}

// ========================================
// NPI GENERATOR
// ========================================

function displayNPIResult(data) {
    const resultBox = data.type === 'individual'
        ? document.getElementById('npiIndividualResult')
        : document.getElementById('npiOrganizationResult');

    resultBox.className = 'result-box success';
    resultBox.innerHTML = `
        <div class="result-content">
            <span class="result-label">Generated NPI Number:</span>
            <span class="result-value">${data.npi}</span>
            <button class="btn-secondary copy-btn" onclick="copyToClipboard('${data.npi}')">
                📋 Copy to Clipboard
            </button>
        </div>
    `;
}

function refreshNPIHistory() {
    photinoBridge.send('npi', 'history');
}

function renderNPIHistory() {
    const tbody = document.getElementById('npiHistoryBody');

    if (appState.npiHistory.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No NPIs generated yet</td></tr>';
        return;
    }

    tbody.innerHTML = appState.npiHistory.map(item => `
        <tr>
            <td><span class="badge">${item.type}</span></td>
            <td>${item.name}</td>
            <td><code>${item.npi}</code></td>
            <td>${formatDate(item.timestamp)}</td>
            <td>
                <div class="table-actions">
                    <button class="icon-btn" onclick="copyToClipboard('${item.npi}')" title="Copy">📋</button>
                </div>
            </td>
        </tr>
    `).join('');
}

// ========================================
// DEA GENERATOR
// ========================================

function displayDEAResult(data) {
    const resultBox = document.getElementById('deaResult');

    resultBox.className = 'result-box success';
    resultBox.innerHTML = `
        <div class="result-content">
            <span class="result-label">Generated DEA Number:</span>
            <span class="result-value">${data.dea}</span>
            <button class="btn-secondary copy-btn" onclick="copyToClipboard('${data.dea}')">
                📋 Copy to Clipboard
            </button>
        </div>
    `;
}

function refreshDEAHistory() {
    photinoBridge.send('dea', 'history');
}

function renderDEAHistory() {
    const tbody = document.getElementById('deaHistoryBody');

    if (appState.deaHistory.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No DEAs generated yet</td></tr>';
        return;
    }

    tbody.innerHTML = appState.deaHistory.map(item => `
        <tr>
            <td>${item.name}</td>
            <td><code>${item.dea}</code></td>
            <td><span class="badge">${item.type}</span></td>
            <td>${formatDate(item.timestamp)}</td>
            <td>
                <div class="table-actions">
                    <button class="icon-btn" onclick="copyToClipboard('${item.dea}')" title="Copy">📋</button>
                </div>
            </td>
        </tr>
    `).join('');
}

// ========================================
// ACCOUNT MANAGER
// ========================================

function loadAccounts() {
    photinoBridge.send('account', 'list');
}

function renderAccounts() {
    const tbody = document.getElementById('accountsTableBody');

    if (appState.accounts.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No accounts saved yet</td></tr>';
        return;
    }

    tbody.innerHTML = appState.accounts.map(account => `
        <tr>
            <td><strong>${account.platform}</strong></td>
            <td>${account.username}</td>
            <td>
                <div class="password-field">
                    <span class="password-hidden" id="pwd-${account.id}">••••••••</span>
                    <button class="icon-btn" onclick="togglePassword('${account.id}', '${escapeHtml(account.password)}')" title="Show">👁️</button>
                </div>
            </td>
            <td>${account.notes || '-'}</td>
            <td>
                <div class="table-actions">
                    <button class="icon-btn" onclick="copyToClipboard('${escapeHtml(account.password)}')" title="Copy Password">📋</button>
                    <button class="icon-btn" onclick="editAccount(${account.id})" title="Edit">✏️</button>
                    <button class="icon-btn danger" onclick="deleteAccount(${account.id})" title="Delete">🗑️</button>
                </div>
            </td>
        </tr>
    `).join('');
}

function filterAccounts(query) {
    const filtered = appState.accounts.filter(acc =>
        acc.platform.toLowerCase().includes(query.toLowerCase()) ||
        acc.username.toLowerCase().includes(query.toLowerCase())
    );

    // Temporarily replace accounts array for rendering
    const original = appState.accounts;
    appState.accounts = filtered;
    renderAccounts();
    appState.accounts = original;
}

function editAccount(id) {
    const account = appState.accounts.find(a => a.id === id);
    if (!account) return;

    document.getElementById('accountId').value = account.id;
    document.getElementById('accountPlatform').value = account.platform;
    document.getElementById('accountUsername').value = account.username;
    document.getElementById('accountPassword').value = account.password;
    document.getElementById('accountNotes').value = account.notes || '';
    document.getElementById('accountSubmitBtn').textContent = 'Update Account';

    // Scroll to form
    navigateToPage('accounts');
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function deleteAccount(id) {
    if (confirm('Are you sure you want to delete this account?')) {
        photinoBridge.send('account', `delete:${id}`);
    }
}

function clearAccountForm() {
    document.getElementById('accountForm').reset();
    document.getElementById('accountId').value = '';
    document.getElementById('accountSubmitBtn').textContent = 'Save Account';
}

function togglePassword(accountId, password) {
    const span = document.getElementById(`pwd-${accountId}`);
    if (span.classList.contains('password-hidden')) {
        span.textContent = password;
        span.classList.remove('password-hidden');
    } else {
        span.textContent = '••••••••';
        span.classList.add('password-hidden');
    }
}

// ========================================
// UTILITIES
// ========================================

function copyToClipboard(text) {
    // Try modern clipboard API first
    if (navigator.clipboard && window.isSecureContext) {
        navigator.clipboard.writeText(text).then(() => {
            showToast('Copied to clipboard!', 'success');
        }).catch(() => {
            fallbackCopy(text);
        });
    } else {
        fallbackCopy(text);
    }
}

function fallbackCopy(text) {
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.style.position = 'fixed';
    textarea.style.opacity = '0';
    document.body.appendChild(textarea);
    textarea.select();
    try {
        document.execCommand('copy');
        showToast('Copied to clipboard!', 'success');
    } catch (err) {
        showToast('Failed to copy', 'error');
    }
    document.body.removeChild(textarea);
}

function showToast(message, type = 'info') {
    const toast = document.getElementById('toast');
    toast.textContent = message;
    toast.className = `toast show ${type}`;

    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

function formatDate(timestamp) {
    const date = new Date(timestamp);
    return date.toLocaleString();
}

function formatTimeAgo(timestamp) {
    const seconds = Math.floor((new Date() - new Date(timestamp)) / 1000);

    if (seconds < 60) return 'just now';
    if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
    return `${Math.floor(seconds / 86400)}d ago`;
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Make some functions globally accessible
window.copyToClipboard = copyToClipboard;
window.editAccount = editAccount;
window.deleteAccount = deleteAccount;
window.togglePassword = togglePassword;