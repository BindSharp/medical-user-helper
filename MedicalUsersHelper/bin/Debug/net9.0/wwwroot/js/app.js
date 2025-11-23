// App initialization
document.addEventListener('DOMContentLoaded', () => {
    initNavigation();
    initForms();
});

// Navigation
function initNavigation() {
    const navItems = document.querySelectorAll('.nav-item');
    const sections = document.querySelectorAll('.tool-section');
    const menuToggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');

    // Handle navigation clicks
    navItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const tool = item.getAttribute('data-tool');

            // Update active nav item
            navItems.forEach(nav => nav.classList.remove('active'));
            item.classList.add('active');

            // Show corresponding section
            sections.forEach(section => {
                section.classList.remove('active');
                if (section.id === `${tool}-section`) {
                    section.classList.add('active');
                }
            });

            // Close sidebar on mobile
            if (window.innerWidth <= 768) {
                sidebar.classList.remove('open');
            }
        });
    });

    // Mobile menu toggle
    if (menuToggle) {
        menuToggle.addEventListener('click', () => {
            sidebar.classList.toggle('open');
        });
    }

    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', (e) => {
        if (window.innerWidth <= 768) {
            if (!sidebar.contains(e.target) && !menuToggle.contains(e.target)) {
                sidebar.classList.remove('open');
            }
        }
    });
}

// Form Handlers
function initForms() {
    initDeaForm();
    initLicenseForm();
    initNpiGenerateForm();
    initNpiValidateForm();
}

// DEA Form
function initDeaForm() {
    const form = document.getElementById('dea-form');
    const resultCard = document.getElementById('dea-result');
    const errorCard = document.getElementById('dea-error');
    const numberDisplay = document.getElementById('dea-number');
    const errorText = document.getElementById('dea-error-text');
    const copyBtn = document.getElementById('dea-copy');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Hide previous results
        resultCard.style.display = 'none';
        errorCard.style.display = 'none';

        // Get form data
        const lastName = document.getElementById('dea-lastname').value.trim();
        const type = document.querySelector('input[name="dea-type"]:checked').value;
        const isNarcotic = type === 'narcotic';

        // Disable submit button
        const submitBtn = form.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<span>Generating...</span>';

        try {
            // Send request to C#
            const result = await photinoBridge.request('dea', {
                lastName: lastName,
                isNarcotic: isNarcotic,
                requestId: returnAndIncrementRequestCounter()
            });

            if (result.success) {
                // Show result
                numberDisplay.textContent = result.deaNumber;
                resultCard.style.display = 'block';
            } else {
                // Show error
                errorText.textContent = result.error || 'An error occurred while generating the DEA number.';
                errorCard.style.display = 'block';
            }
        } catch (error) {
            // Show error
            errorText.textContent = error.message || 'An error occurred while generating the DEA number.';
            errorCard.style.display = 'block';
        } finally {
            // Re-enable submit button
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        }
    });

    // Copy button
    copyBtn.addEventListener('click', () => {
        copyToClipboard(numberDisplay.textContent, 'DEA number copied!');
    });
}

// License Form
function initLicenseForm() {
    const form = document.getElementById('license-form');
    const resultCard = document.getElementById('license-result');
    const errorCard = document.getElementById('license-error');
    const numberDisplay = document.getElementById('license-number');
    const errorText = document.getElementById('license-error-text');
    const copyBtn = document.getElementById('license-copy');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Hide previous results
        resultCard.style.display = 'none';
        errorCard.style.display = 'none';

        // Get form data
        const stateCode = document.getElementById('license-state').value;
        const lastName = document.getElementById('license-lastname').value.trim();
        const licenseType = document.getElementById('license-type').value;

        // Disable submit button
        const submitBtn = form.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<span>Generating...</span>';

        try {
            // Send request to C#
            const result = await photinoBridge.request('license', {
                stateCode: stateCode,
                lastName: lastName,
                licenseType: licenseType,
                requestId: returnAndIncrementRequestCounter()
            });

            if (result.success) {
                // Show result
                numberDisplay.textContent = result.licenseNumber;
                resultCard.style.display = 'block';
            } else {
                // Show error
                errorText.textContent = result.error || 'An error occurred while generating the license number.';
                errorCard.style.display = 'block';
            }
        } catch (error) {
            // Show error
            errorText.textContent = error.message || 'An error occurred while generating the license number.';
            errorCard.style.display = 'block';
        } finally {
            // Re-enable submit button
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        }
    });

    // Copy button
    copyBtn.addEventListener('click', () => {
        copyToClipboard(numberDisplay.textContent, 'License number copied!');
    });
}

// NPI Generate Form
function initNpiGenerateForm() {
    const form = document.getElementById('npi-generate-form');
    const resultCard = document.getElementById('npi-generate-result');
    const errorCard = document.getElementById('npi-generate-error');
    const numberDisplay = document.getElementById('npi-generate-number');
    const errorText = document.getElementById('npi-generate-error-text');
    const copyBtn = document.getElementById('npi-generate-copy');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Hide previous results
        resultCard.style.display = 'none';
        errorCard.style.display = 'none';

        // Get form data
        const providerType = document.querySelector('input[name="npi-provider-type"]:checked').value;
        const isOrganization = providerType === 'organization';

        // Disable submit button
        const submitBtn = form.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<span>Generating...</span>';

        debugger;
        try {
            // Send request to C#
            const result = await photinoBridge.request('npi', {
                action: 'generate',
                isOrganization: isOrganization,
                requestId: returnAndIncrementRequestCounter()
            });

            if (result.success) {
                // Show result
                numberDisplay.textContent = result.npi;
                resultCard.style.display = 'block';
            } else {
                // Show error
                errorText.textContent = result.error || 'An error occurred while generating the NPI.';
                errorCard.style.display = 'block';
            }
        } catch (error) {
            // Show error
            errorText.textContent = error.message || 'An error occurred while generating the NPI.';
            errorCard.style.display = 'block';
        } finally {
            // Re-enable submit button
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        }
    });

    // Copy button
    copyBtn.addEventListener('click', () => {
        copyToClipboard(numberDisplay.textContent, 'NPI copied!');
    });
}

// NPI Validate Form
function initNpiValidateForm() {
    const form = document.getElementById('npi-validate-form');
    const resultDiv = document.getElementById('npi-validate-result');
    const errorCard = document.getElementById('npi-validate-error');
    const badge = document.getElementById('npi-validation-badge');
    const errorText = document.getElementById('npi-validate-error-text');
    const input = document.getElementById('npi-validate-input');

    // Only allow numbers
    input.addEventListener('input', (e) => {
        e.target.value = e.target.value.replace(/[^0-9]/g, '');
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Hide previous results
        resultDiv.style.display = 'none';
        errorCard.style.display = 'none';

        // Get form data
        const npi = input.value.trim();

        // Client-side validation
        if (npi.length !== 10) {
            errorText.textContent = 'NPI must be exactly 10 digits.';
            errorCard.style.display = 'block';
            return;
        }

        // Disable submit button
        const submitBtn = form.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<span>Validating...</span>';

        try {
            // Send request to C#
            const result = await photinoBridge.request('npi', {
                action: 'validate',
                npi: npi,
                requestId: returnAndIncrementRequestCounter()
            });

            if (result.success) {
                // Show result
                if (result.isValid) {
                    badge.textContent = 'Valid NPI';
                    badge.className = 'validation-badge valid';
                } else {
                    badge.textContent = 'Invalid NPI';
                    badge.className = 'validation-badge invalid';
                }
                resultDiv.style.display = 'block';
            } else {
                // Show error
                errorText.textContent = result.error || 'An error occurred while validating the NPI.';
                errorCard.style.display = 'block';
            }
        } catch (error) {
            // Show error
            errorText.textContent = error.message || 'An error occurred while validating the NPI.';
            errorCard.style.display = 'block';
        } finally {
            // Re-enable submit button
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        }
    });
}

// Utility Functions
function copyToClipboard(text, message = 'Copied!') {
    // Try to use modern clipboard API
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).then(() => {
            showToast(message);
        }).catch(() => {
            // Fallback
            fallbackCopy(text, message);
        });
    } else {
        // Fallback for older browsers
        fallbackCopy(text, message);
    }
}

function fallbackCopy(text, message) {
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.style.position = 'fixed';
    textarea.style.opacity = '0';
    document.body.appendChild(textarea);
    textarea.select();

    try {
        document.execCommand('copy');
        showToast(message);
    } catch (err) {
        showToast('Failed to copy');
    }

    document.body.removeChild(textarea);
}

function showToast(message) {
    const toast = document.getElementById('toast');
    toast.textContent = message;
    toast.classList.add('show');

    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

function returnAndIncrementRequestCounter() {
    photinoBridge.requestId += 1;

    return photinoBridge.requestId;
}