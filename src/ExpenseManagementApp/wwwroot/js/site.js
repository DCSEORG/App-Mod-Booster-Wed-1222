// Expense Management App - Client-side JavaScript

document.addEventListener('DOMContentLoaded', function() {
    console.log('Expense Management App loaded');
    
    // Highlight active nav link
    const currentPath = window.location.pathname;
    document.querySelectorAll('.nav-link').forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.style.color = 'var(--primary)';
            link.style.background = 'var(--light)';
        }
    });

    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 500);
        }, 5000);
    });

    // Form validation enhancement
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const requiredFields = form.querySelectorAll('[required]');
            let isValid = true;
            
            // Clear previous error messages
            form.querySelectorAll('.validation-error').forEach(el => el.remove());
            
            requiredFields.forEach(field => {
                if (!field.value.trim()) {
                    isValid = false;
                    field.style.borderColor = 'var(--danger)';
                    
                    // Add inline error message
                    const errorMsg = document.createElement('div');
                    errorMsg.className = 'validation-error text-danger';
                    errorMsg.style.fontSize = '0.875rem';
                    errorMsg.style.marginTop = '0.25rem';
                    errorMsg.textContent = 'This field is required';
                    field.parentElement.appendChild(errorMsg);
                } else {
                    field.style.borderColor = 'var(--border)';
                }
            });

            if (!isValid) {
                e.preventDefault();
                // Scroll to first error
                const firstError = form.querySelector('.validation-error');
                if (firstError) {
                    firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            }
        });
    });

    // Currency formatting
    const amountInputs = document.querySelectorAll('input[type="number"][step="0.01"]');
    amountInputs.forEach(input => {
        input.addEventListener('blur', function() {
            if (this.value) {
                this.value = parseFloat(this.value).toFixed(2);
            }
        });
    });

    // Confirm delete actions with better UX
    const deleteForms = document.querySelectorAll('form[action*="delete"]');
    deleteForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Create accessible confirmation dialog
            const confirmMsg = document.createElement('div');
            confirmMsg.className = 'alert alert-danger';
            confirmMsg.style.marginTop = '1rem';
            confirmMsg.innerHTML = `
                <strong>Confirm Deletion</strong><br>
                Are you sure you want to delete this expense? This action cannot be undone.
                <div style="margin-top: 1rem;">
                    <button type="button" class="btn btn-danger" id="confirmDelete">Yes, Delete</button>
                    <button type="button" class="btn btn-secondary" id="cancelDelete">Cancel</button>
                </div>
            `;
            
            form.insertAdjacentElement('beforebegin', confirmMsg);
            
            document.getElementById('confirmDelete').onclick = () => {
                confirmMsg.remove();
                form.submit();
            };
            
            document.getElementById('cancelDelete').onclick = () => {
                confirmMsg.remove();
            };
        });
    });

    // Table row click to expand details
    const tableRows = document.querySelectorAll('.table tbody tr');
    tableRows.forEach(row => {
        row.style.cursor = 'pointer';
    });
});

// Utility function to format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-GB', {
        style: 'currency',
        currency: 'GBP'
    }).format(amount);
}

// Utility function to format date
function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('en-GB', {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
    });
}
