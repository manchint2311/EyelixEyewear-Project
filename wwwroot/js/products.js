// =====================================
// PRODUCTS.JS - MVC Client-side Logic
// =====================================
// Chỉ xử lý tương tác UI, KHÔNG render HTML từ JS
// HTML đã được render sẵn từ Server (.cshtml)

// =====================================
// UTILITY FUNCTIONS
// =====================================
function formatPrice(price) {
    return price.toLocaleString('vi-VN') + ' VND';
}

// =====================================
// PRODUCT IMAGE COLOR VARIATIONS
// =====================================
function changeProductImage(productId, newImage, clickedElement) {
    const productCard = clickedElement.closest('.product-item');
    const productImage = productCard.querySelector('.product-image img');

    // Smooth transition khi đổi ảnh
    productImage.style.opacity = '0.5';
    setTimeout(() => {
        productImage.src = newImage;
        productImage.style.opacity = '1';
    }, 150);

    // Update active state
    const allColorOptions = productCard.querySelectorAll('.color-option');
    allColorOptions.forEach(option => option.classList.remove('active'));
    clickedElement.classList.add('active');
}

// =====================================
// FILTER & SORT (Client-side)
// =====================================
let currentFilter = 'all';
let currentSort = 'featured';

function applyFilterAndSort() {
    const allProducts = document.querySelectorAll('.product-item');
    let visibleProducts = [];

    // Filter
    allProducts.forEach(product => {
        const filterAttr = product.getAttribute('data-filter');
        if (currentFilter === 'all' || (filterAttr && filterAttr.includes(currentFilter))) {
            product.style.display = '';
            visibleProducts.push(product);
        } else {
            product.style.display = 'none';
        }
    });

    // Sort
    const grid = document.getElementById('productsGrid');
    if (!grid) return;

    visibleProducts.sort((a, b) => {
        const priceA = parseInt(a.getAttribute('data-price'));
        const priceB = parseInt(b.getAttribute('data-price'));

        switch (currentSort) {
            case 'price-low':
                return priceA - priceB;
            case 'price-high':
                return priceB - priceA;
            case 'newest':
                const idA = parseInt(a.getAttribute('data-product-id'));
                const idB = parseInt(b.getAttribute('data-product-id'));
                return idB - idA;
            default:
                return 0;
        }
    });

    // Re-append sorted elements
    visibleProducts.forEach(product => {
        grid.appendChild(product);
    });
}

// =====================================
// ADD TO CART (Submit form to server)
// =====================================
function addToCart(productId, productTitle) {
    // Tạo form và submit lên server
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Cart/AddToCart'; // Route đến CartController

    // Add CSRF token (nếu dùng)
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
    if (csrfToken) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = csrfToken.value;
        form.appendChild(tokenInput);
    }

    // Add product ID
    const productInput = document.createElement('input');
    productInput.type = 'hidden';
    productInput.name = 'productId';
    productInput.value = productId;
    form.appendChild(productInput);

    // Add quantity
    const quantityInput = document.createElement('input');
    quantityInput.type = 'hidden';
    quantityInput.name = 'quantity';
    quantityInput.value = 1;
    form.appendChild(quantityInput);

    document.body.appendChild(form);
    form.submit();
}

// Hoặc dùng AJAX nếu muốn không reload trang
function addToCartAjax(productId, productTitle) {
    const formData = new FormData();
    formData.append('productId', productId);
    formData.append('quantity', 1);

    // Lấy CSRF token
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
    if (csrfToken) {
        formData.append('__RequestVerificationToken', csrfToken.value);
    }

    fetch('/Cart/AddToCart', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification(`${productTitle} đã được thêm vào giỏ hàng!`);
                updateCartCount(data.cartCount);
            } else {
                showNotification('Có lỗi xảy ra. Vui lòng thử lại!', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('Có lỗi xảy ra. Vui lòng thử lại!', 'error');
        });
}

function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${type === 'success' ? '#28a745' : '#dc3545'};
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 4px;
        z-index: 9999;
        box-shadow: 0 2px 8px rgba(0,0,0,0.2);
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transition = 'opacity 0.3s';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

function updateCartCount(count) {
    const cartBadge = document.querySelector('.cart-count');
    if (cartBadge) {
        cartBadge.textContent = count;
    }
}

// =====================================
// WISHLIST TOGGLE
// =====================================
function toggleWishlist(btn, productId) {
    const isWishlisted = btn.textContent === '♥';

    // Submit to server
    fetch('/Wishlist/Toggle', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ productId: productId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                btn.textContent = data.isWishlisted ? '♥' : '♡';
                btn.style.color = data.isWishlisted ? '#e00d0d' : 'inherit';
            }
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

// =====================================
// EVENT LISTENERS
// =====================================
document.addEventListener('DOMContentLoaded', () => {

    // Filter buttons
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
            e.target.classList.add('active');
            currentFilter = e.target.dataset.filter;
            applyFilterAndSort();
        });
    });

    // Sort dropdown
    const sortSelect = document.getElementById('sortBy');
    if (sortSelect) {
        sortSelect.addEventListener('change', (e) => {
            currentSort = e.target.value;
            applyFilterAndSort();
        });
    }

    // Sidebar accordion
    document.querySelectorAll('.filter-section-sidebar').forEach((group, index) => {
        const content = group.querySelector('.filter-content');
        if (content) {
            if (index === 0) {
                group.classList.add('open');
                content.classList.remove('collapsed');
            } else {
                group.classList.remove('open');
                content.classList.add('collapsed');
            }
        }
    });
});

// =====================================
// SIDEBAR ACCORDION TOGGLE
// =====================================
function toggleFilterSection(headerElement) {
    const parent = headerElement.closest('.filter-section-sidebar');
    const content = parent.querySelector('.filter-content');

    parent.classList.toggle('open');

    if (parent.classList.contains('open')) {
        content.classList.remove('collapsed');
    } else {
        content.classList.add('collapsed');
    }
}