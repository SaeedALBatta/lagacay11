// Micro-interactions and scale effects for Legacy Eleven Authentication
document.addEventListener('DOMContentLoaded', () => {
    const inputs = document.querySelectorAll('.auth-input');
    
    inputs.forEach(input => {
        const container = input.closest('.auth-input-container');
        if (container) {
            input.addEventListener('focus', () => {
                container.classList.add('scale-up');
            });
            input.addEventListener('blur', () => {
                container.classList.remove('scale-up');
            });
        }
    });

    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', () => {
            const btn = form.querySelector('.auth-btn');
            if (btn) {
                // Show loading spinner
                const originalText = btn.innerHTML;
                btn.innerHTML = '<span class="material-symbols-outlined animate-spin" style="animation: spin 1.5s linear infinite;">progress_activity</span> Authenticating...';
                btn.style.opacity = '0.85';
                btn.style.pointerEvents = 'none';

                // In case of postback redirect, it's fine. If there are validation errors, MVC page will reload and reset the button.
                // Just to prevent double submissions.
            }
        });
    }
});

// Spin animation keyframe helper
const style = document.createElement('style');
style.innerHTML = `
@keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}
`;
document.head.appendChild(style);
