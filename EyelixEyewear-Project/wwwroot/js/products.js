// =====================================================
// FILE: wwwroot/js/products.js
// THAY THẾ TOÀN BỘ FILE CŨ BẰNG CODE NÀY
// =====================================================

// ==========================================
// GLOBAL VARIABLES
// ==========================================
let currentFilter = 'all';
let currentSort = 'featured';
let activeFilters = {
    material: [],
    shape: [],
    gender: [],
    color: []
};

// ==========================================
// TOGGLE FILTER SECTION (Accordion - Mở/Đóng)
// ==========================================
function toggleFilterSection(header) {
    console.log('Toggle section'); // Debug

    const section = header.parentElement;
    const content = section.querySelector('.filter-content');
    const icon = header.querySelector('.toggle-icon');

    if (!content || !icon) return;

    if (section.classList.contains('open')) {
        // Đóng
        section.classList.remove('open');
        content.classList.add('collapsed');
        icon.textContent = '▼';
    } else {
        // Mở
        section.classList.add('open');
        content.classList.remove('collapsed');
        icon.textContent = '▲';
    }
}

// ==========================================
// TOGGLE FILTER OPTION (Click vào checkbox)
// ==========================================
function toggleFilter(element) {
    console.log('Toggle filter'); // Debug

    const filterType = element.dataset.filterType;
    const filterValue = element.dataset.value;

    if (!filterType || !filterValue) {
        console.error('Missing filter data');
        return;
    }

    const checkbox = element.querySelector('.filter-checkbox');
    if (!checkbox) return;

    const isChecked = checkbox.textContent === '☑' || element.classList.contains('active');

    if (isChecked) {
        // Uncheck
        checkbox.textContent = '☐';
        element.classList.remove('active');

        // Remove from activeFilters
        const index = activeFilters[filterType].indexOf(filterValue);
        if (index > -1) {
            activeFilters[filterType].splice(index, 1);
        }
    } else {
        // Check
        checkbox.textContent = '☑';
        element.classList.add('active');

        // Add to activeFilters
        if (!activeFilters[filterType].includes(filterValue)) {
            activeFilters[filterType].push(filterValue);
        }
    }

    console.log('Active filters:', activeFilters); // Debug
    applyFilters();
}

// ==========================================
// APPLY FILTERS (Lọc sản phẩm theo sidebar)
// ==========================================
function applyFilters() {
    console.log('Applying filters'); // Debug

    const products = document.querySelectorAll('.product-item');
    let visibleCount = 0;

    products.forEach(product => {
        let shouldShow = true;

        // Check Material filter
        if (activeFilters.material.length > 0) {
            const productMaterial = product.dataset.material || '';
            if (!activeFilters.material.includes(productMaterial)) {
                shouldShow = false;
            }
        }

        // Check Shape filter
        if (activeFilters.shape.length > 0) {
            const productShape = product.dataset.shape || '';
            if (!activeFilters.shape.includes(productShape)) {
                shouldShow = false;
            }
        }

        // Check Gender filter
        if (activeFilters.gender.length > 0) {
            const productGender = product.dataset.gender || '';
            if (!activeFilters.gender.includes(productGender)) {
                shouldShow = false;
            }
        }

        // Check Color filter
        if (activeFilters.color.length > 0) {
            const productColor = product.dataset.color || '';
            if (!activeFilters.color.includes(productColor)) {
                shouldShow = false;
            }
        }

        // Show/Hide product
        if (shouldShow) {
            product.style.display = 'block';
            visibleCount++;
        } else {
            product.style.display = 'none';
        }
    });

    console.log('Visible:', visibleCount); // Debug
    updateResultCount(visibleCount);
}

// ==========================================
// RESET ALL FILTERS
// ==========================================
function resetAllFilters() {
    console.log('Reset filters'); // Debug

    activeFilters = {
        material: [],
        shape: [],
        gender: [],
        color: []
    };

    document.querySelectorAll('.filter-option').forEach(option => {
        const checkbox = option.querySelector('.filter-checkbox');
        if (checkbox) {
            checkbox.textContent = '☐';
        }
        option.classList.remove('active');
    });

    document.querySelectorAll('.product-item').forEach(product => {
        product.style.display = 'block';
    });

    const totalProducts = document.querySelectorAll('.product-item').length;
    updateResultCount(totalProducts);
}

// ==========================================
// UPDATE RESULT COUNT
// ==========================================
function updateResultCount(count) {
    const totalProducts = document.querySelectorAll('.product-item').length;

    let resultElement = document.querySelector('.filter-result-count');

    if (!resultElement) {
        resultElement = document.createElement('div');
        resultElement.className = 'filter-result-count';

        const filterSection = document.querySelector('.filter-section');
        if (filterSection) {
            filterSection.insertAdjacentElement('afterend', resultElement);
        }
    }

    resultElement.textContent = `Showing ${count} of ${totalProducts} products`;
}

// ==========================================
// FILTER BY TYPE (All, Bestsellers, New, Sale)
// ==========================================
function applyFilterAndSort() {
    const allProducts = document.querySelectorAll('.product-item');
    const grid = document.getElementById('productsGrid');
    let visibleProducts = [];

    // Filter
    allProducts.forEach(product => {
        const filters = product.dataset.filter ? product.dataset.filter.split(' ') : [];
        if (currentFilter === 'all' || filters.includes(currentFilter)) {
            product.style.display = 'block';
            visibleProducts.push(product);
        } else {
            product.style.display = 'none';
        }
    });

    // Sort
    visibleProducts.sort((a, b) => {
        const priceA = parseInt(a.dataset.price) || 0;
        const priceB = parseInt(b.dataset.price) || 0;
        const idA = parseInt(a.dataset.productId) || 0;
        const idB = parseInt(b.dataset.productId) || 0;

        if (currentSort === 'price-low') return priceA - priceB;
        if (currentSort === 'price-high') return priceB - priceA;
        if (currentSort === 'newest') return idB - idA;
        return 0;
    });

    visibleProducts.forEach(product => grid.appendChild(product));
}

// ==========================================
// CHANGE PRODUCT IMAGE (Color variations)
// ==========================================
function changeProductImage(clickedElement, newImageSrc) {
    const productItem = clickedElement.closest('.product-item');
    if (!productItem) return;

    const imgElement = productItem.querySelector('.product-image img');
    if (!imgElement) return;

    imgElement.style.opacity = '0.5';
    setTimeout(() => {
        imgElement.src = newImageSrc;
        imgElement.style.opacity = '1';
    }, 150);

    const allColors = productItem.querySelectorAll('.color-option');
    allColors.forEach(c => c.classList.remove('active'));
    clickedElement.classList.add('active');
}

// ==========================================
// INITIALIZE ON PAGE LOAD
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    console.log('Products.js loaded'); // Debug

    // Mở section đầu tiên mặc định
    const firstSection = document.querySelector('.filter-section-sidebar');
    if (firstSection) {
        firstSection.classList.add('open');
        const content = firstSection.querySelector('.filter-content');
        if (content) {
            content.classList.remove('collapsed');
        }
        const icon = firstSection.querySelector('.toggle-icon');
        if (icon) {
            icon.textContent = '▲';
        }
    }

    // Filter Buttons (All Products, Bestsellers, New, Sale)
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

    // Show initial count
    const totalProducts = document.querySelectorAll('.product-item').length;
    updateResultCount(totalProducts);
});