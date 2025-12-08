// Simple toast notification function
window.showToast = function (message, type = 'success') {
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `alert alert-${type} position-fixed top-0 end-0 m-3`;
    toast.style.zIndex = '9999';
    toast.innerHTML = message;
    
    // Add to document
    document.body.appendChild(toast);
    
    // Remove after 3 seconds
    setTimeout(() => {
        if (toast.parentNode) {
            toast.parentNode.removeChild(toast);
        }
    }, 3000);
};

// Cart-specific CSS
const cartStyles = `
.cart-badge {
    font-size: 0.7rem;
    min-width: 1.2rem;
    height: 1.2rem;
    display: flex;
    align-items: center;
    justify-content: center;
}

.offcanvas {
    width: 400px !important;
}

.cart-item-image {
    border-radius: 4px;
}

.offcanvas-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background-color: rgba(0, 0, 0, 0.5);
    z-index: 1040;
}
`;

// Add styles to document
const styleSheet = document.createElement('style');
styleSheet.type = 'text/css';
styleSheet.innerText = cartStyles;
document.head.appendChild(styleSheet);