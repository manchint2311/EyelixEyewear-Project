// products.js - CHỈ XỬ LÝ GIAO DIỆN (Lọc, Sort, UI)

// 1. Biến toàn cục cho Filter/Sort
let currentFilter = 'all';
let currentSort = 'featured';

// 2. Hàm thay đổi ảnh khi chọn màu (UI)
function changeProductImage(clickedElement, newImageSrc) {
    const productItem = clickedElement.closest('.product-item');
    const imgElement = productItem.querySelector('.product-image img');

    imgElement.style.opacity = '0.5';
    setTimeout(() => {
        imgElement.src = newImageSrc;
        imgElement.style.opacity = '1';
    }, 150);

    const allColors = productItem.querySelectorAll('.color-option');
    allColors.forEach(c => c.classList.remove('active'));
    clickedElement.classList.add('active');
}

// 3. Logic Lọc & Sắp xếp (DOM Manipulation)
function applyFilterAndSort() {
    const allProducts = document.querySelectorAll('.product-item');
    const grid = document.getElementById('productsGrid');
    let visibleProducts = [];

    // Filter
    allProducts.forEach(product => {
        const filters = product.dataset.filter.split(' ');
        if (currentFilter === 'all' || filters.includes(currentFilter)) {
            product.style.display = 'block';
            visibleProducts.push(product);
        } else {
            product.style.display = 'none';
        }
    });

    // Sort
    visibleProducts.sort((a, b) => {
        const priceA = parseInt(a.dataset.price);
        const priceB = parseInt(b.dataset.price);
        const idA = parseInt(a.dataset.productId);
        const idB = parseInt(b.dataset.productId);

        if (currentSort === 'price-low') return priceA - priceB;
        if (currentSort === 'price-high') return priceB - priceA;
        if (currentSort === 'newest') return idB - idA;
        return 0;
    });

    visibleProducts.forEach(product => grid.appendChild(product));
}

// 4. Event Listeners
document.addEventListener('DOMContentLoaded', () => {
    // Filter Buttons
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
            e.target.classList.add('active');
            currentFilter = e.target.dataset.filter;
            applyFilterAndSort();
        });
    });

    // Sort Dropdown
    const sortSelect = document.getElementById('sortBy');
    if (sortSelect) {
        sortSelect.addEventListener('change', (e) => {
            currentSort = e.target.value;
            applyFilterAndSort();
        });
    }

    // Sidebar Accordion
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