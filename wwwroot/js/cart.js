// =====================
// Helper Functions
// =====================
function formatPrice(price) {
    return price.toLocaleString('vi-VN');
}

function getCartImage(imagePath) {
    // ✅ Đảm bảo đường dẫn bắt đầu từ /wwwroot
    if (!imagePath) return '/images/products/default.png';
    return imagePath.startsWith('/') ? imagePath : '/' + imagePath;
}

// ==========================================
// UPDATE CART COUNT ICON (HEADER)
// ==========================================
window.updateCartCount = function () {
    // Lấy cart count từ server-rendered element hoặc từ attribute
    const cartCountElement = document.querySelector('.cart-count');
    if (!cartCountElement) return;

    const count = parseInt(cartCountElement.getAttribute('data-count') || '0');
    cartCountElement.textContent = count;
    cartCountElement.style.display = count > 0 ? 'flex' : 'none';
};

// ==========================================
// UPDATE QUANTITY (AJAX to Server)
// ==========================================
function updateQuantity(cartItemId, change) {
    const formData = new FormData();
    formData.append('cartItemId', cartItemId);
    formData.append('change', change);

    // Lấy CSRF token nếu có
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
    if (csrfToken) {
        formData.append('__RequestVerificationToken', csrfToken.value);
    }

    // Show loading state
    const cartItem = document.querySelector(`[data-cart-item-id="${cartItemId}"]`);
    if (cartItem) {
        cartItem.style.opacity = '0.5';
        cartItem.style.pointerEvents = 'none';
    }

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Reload page để cập nhật UI từ server
                window.location.reload();
            } else {
                alert(data.message || 'Failed to update quantity');
                if (cartItem) {
                    cartItem.style.opacity = '1';
                    cartItem.style.pointerEvents = 'auto';
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred. Please try again.');
            if (cartItem) {
                cartItem.style.opacity = '1';
                cartItem.style.pointerEvents = 'auto';
            }
        });
}

// ==========================================
// REMOVE ITEM (AJAX to Server)
// ==========================================
function removeFromCart(cartItemId) {
    if (!confirm('Remove this item from cart?')) return;

    const formData = new FormData();
    formData.append('cartItemId', cartItemId);

    // Lấy CSRF token nếu có
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
    if (csrfToken) {
        formData.append('__RequestVerificationToken', csrfToken.value);
    }

    // Show loading state
    const cartItem = document.querySelector(`[data-cart-item-id="${cartItemId}"]`);
    if (cartItem) {
        cartItem.style.opacity = '0.5';
        cartItem.style.pointerEvents = 'none';
    }

    fetch('/Cart/RemoveItem', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Reload page để cập nhật UI từ server
                window.location.reload();
            } else {
                alert(data.message || 'Failed to remove item');
                if (cartItem) {
                    cartItem.style.opacity = '1';
                    cartItem.style.pointerEvents = 'auto';
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred. Please try again.');
            if (cartItem) {
                cartItem.style.opacity = '1';
                cartItem.style.pointerEvents = 'auto';
            }
        });
}

// ==========================================
// UPDATE CART SUMMARY (Client-side calculation for shipping)
// ==========================================
function updateCartSummary() {
    const subtotalElement = document.getElementById('summary-subtotal');
    const totalElement = document.getElementById('summary-total');
    const shippingElement = document.getElementById('summary-shipping');

    if (!subtotalElement || !totalElement) return;

    // Lấy subtotal từ data attribute (đã được server render)
    const subtotal = parseFloat(subtotalElement.getAttribute('data-value') || '0');

    // ✅ Lấy giá ship từ data attribute của radio button đã chọn
    const selectedShipping = document.querySelector('input[name="shipping"]:checked');
    const shipping = subtotal === 0
        ? 0
        : parseFloat(selectedShipping?.getAttribute('data-price') || '0');

    const total = subtotal + shipping;

    // Update UI
    if (shippingElement) {
        shippingElement.textContent = formatPrice(shipping) + ' VND';
    }
    totalElement.textContent = formatPrice(total) + ' VND';
    totalElement.setAttribute('data-value', total);
}

// ==========================================
// MANUAL QUANTITY INPUT CHANGE
// ==========================================
function handleQuantityInput(input) {
    const cartItemId = input.getAttribute('data-cart-item-id');
    const currentQty = parseInt(input.getAttribute('data-current-qty') || '1');
    const newQty = Math.max(parseInt(input.value) || 1, 1);

    if (newQty === currentQty) return;

    const change = newQty - currentQty;
    updateQuantity(cartItemId, change);
}

// ==========================================
// EVENT LISTENERS
// ==========================================
document.addEventListener('DOMContentLoaded', () => {

    // Update cart count on page load
    updateCartCount();

    // Quantity buttons: Minus
    document.querySelectorAll('.qty-btn.minus').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const cartItemId = btn.getAttribute('data-cart-item-id');
            updateQuantity(cartItemId, -1);
        });
    });

    // Quantity buttons: Plus
    document.querySelectorAll('.qty-btn.plus').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const cartItemId = btn.getAttribute('data-cart-item-id');
            updateQuantity(cartItemId, 1);
        });
    });

    // Manual quantity input
    document.querySelectorAll('.qty-input').forEach(input => {
        input.addEventListener('change', () => {
            handleQuantityInput(input);
        });

        // Prevent invalid input
        input.addEventListener('input', () => {
            if (input.value < 1) input.value = 1;
        });
    });

    // Remove item buttons
    document.querySelectorAll('.remove-item').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const cartItemId = btn.getAttribute('data-cart-item-id');
            removeFromCart(cartItemId);
        });
    });

    // Shipping method change
    document.querySelectorAll('input[name="shipping"]').forEach(radio => {
        radio.addEventListener('change', () => {
            updateCartSummary();
        });
    });

    // Checkout button
    const btnCheckout = document.getElementById('btn-checkout');
    if (btnCheckout) {
        btnCheckout.addEventListener('click', (e) => {
            const cartItems = document.querySelectorAll('.cart-item');
            if (cartItems.length === 0) {
                e.preventDefault();
                alert('Your cart is empty!');
                return;
            }
            // Form sẽ tự submit lên /Checkout/Index
        });
    }

    // Clear cart button (nếu có)
    const btnClearCart = document.getElementById('btn-clear-cart');
    if (btnClearCart) {
        btnClearCart.addEventListener('click', (e) => {
            e.preventDefault();
            if (!confirm('Are you sure you want to clear your cart?')) return;

            fetch('/Cart/ClearCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        window.location.reload();
                    } else {
                        alert('Failed to clear cart');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('An error occurred. Please try again.');
                });
        });
    }

    // Initial cart summary calculation
    updateCartSummary();
});

// ==========================================
// APPLY COUPON (AJAX to Server)
// ==========================================
function applyCoupon() {
    const couponInput = document.getElementById('coupon-code');
    if (!couponInput) return;

    const couponCode = couponInput.value.trim();
    if (!couponCode) {
        alert('Please enter a coupon code');
        return;
    }

    const formData = new FormData();
    formData.append('couponCode', couponCode);

    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
    if (csrfToken) {
        formData.append('__RequestVerificationToken', csrfToken.value);
    }

    // Show loading
    const applyBtn = document.getElementById('apply-coupon-btn');
    if (applyBtn) {
        applyBtn.disabled = true;
        applyBtn.textContent = 'Applying...';
    }

    fetch('/Cart/ApplyCoupon', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message || 'Coupon applied successfully!');
                window.location.reload();
            } else {
                alert(data.message || 'Invalid coupon code');
                if (applyBtn) {
                    applyBtn.disabled = false;
                    applyBtn.textContent = 'Apply';
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred. Please try again.');
            if (applyBtn) {
                applyBtn.disabled = false;
                applyBtn.textContent = 'Apply';
            }
        });
}

// Export function để có thể gọi từ HTML
window.applyCoupon = applyCoupon;