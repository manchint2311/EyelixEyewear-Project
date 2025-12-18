const trendingProducts = [
    { id: 1, name: 'Maison', image: 'images/products/maison.png', price: '777.000 VND' },
    { id: 2, name: 'Nuit', image: 'images/products/nuit.png', price: '790.000 VND' },
    { id: 3, name: 'Dada', image: 'images/products/dada.png', price: '780.000 VND' },
    { id: 4, name: 'Luna', image: 'images/products/luna.png', price: '650.000 VND' },
    { id: 5, name: 'Nova', image: 'images/products/nova.png', price: '720.000 VND' }
];

const allProducts = [...trendingProducts];


// Open Search Overlay
function openSearch() {
    const overlay = document.getElementById('search-overlay');
    
    if (overlay) {
        overlay.classList.add('active'); 
        document.body.style.overflow = 'hidden'; 
        
        setTimeout(() => {
            const input = document.getElementById('searchInput');
            if(input) input.focus();
        }, 100);

        renderTrends(); 
        renderRecentlyViewed();
    } else {
        console.error("Không tìm thấy phần tử #search-overlay trong HTML");
    }
}
    
    setTimeout(() => {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.focus();
        }
    }, 300);


// Close Search Overlay
function closeSearch() {
    const overlay = document.getElementById('search-overlay');
    if (overlay) {
        overlay.classList.remove('active');
        document.body.style.overflow = ''; 
    }
}
    
// Clear search input
const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.value = '';
    }

// Clear History
function clearHistory() {
    if (confirm('Are you sure you want to clear your viewing history?')) {
        localStorage.removeItem('recentlyViewed');
        renderRecentlyViewed();
    }
}

// Render Trending Products
function renderTrends() {
    const grid = document.querySelector('.trends-grid');
    if (!grid) return;
    
    if (grid.innerHTML.trim() === '') {
        grid.innerHTML = trendingProducts.map(item => `
            <div class="trend-item" style="cursor: pointer; text-align: center;">
                <img src="${item.image}" alt="${item.name}" style="width: 100%; margin-bottom: 10px;">
                <div class="trend-name" style="font-size: 14px; font-weight: 500;">${item.name}</div>
            </div>
        `).join('');
    }
}

// Render Recently Viewed
function renderRecentlyViewed() {
}

document.addEventListener('DOMContentLoaded', () => {
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeSearch();
    });
});

// Add to Recently Viewed
function addToRecentlyViewed(product) {
    const stored = localStorage.getItem('recentlyViewed');
    let recentlyViewed = stored ? JSON.parse(stored) : [];
    
    // Remove if already exists
    recentlyViewed = recentlyViewed.filter(p => p.id !== product.id);
    
    // Add to beginning
    recentlyViewed.unshift(product);
    
    // Keep only last 8 items
    if (recentlyViewed.length > 8) {
        recentlyViewed = recentlyViewed.slice(0, 8);
    }
    
    localStorage.setItem('recentlyViewed', JSON.stringify(recentlyViewed));
    renderRecentlyViewed();
}

// View Product
function viewProduct(productId) {
    const product = allProducts.find(p => p.id === productId);
    if (product) {
        addToRecentlyViewed(product);
        closeSearch();
        
        // Redirect to product page (adjust URL as needed)
        // window.location.href = `/product.html?id=${productId}`;
        console.log('Viewing product:', product.name);
    }
}

// Search Functionality
function handleSearch(searchTerm) {
    searchTerm = searchTerm.toLowerCase().trim();
    
    const trendsGrid = document.querySelector('.trends-grid');
    const searchSection = document.querySelector('.search-section');
    const sectionHeader = searchSection ? searchSection.querySelector('.section-header h3') : null;
    
    if (!trendsGrid) return;
    
    if (!searchTerm) {
        // Reset to trends when search is empty
        if (sectionHeader) {
            sectionHeader.textContent = 'SEARCH TRENDS';
        }
        renderTrends();
        return;
    }
    
    // Update section title to show search results
    if (sectionHeader) {
        sectionHeader.textContent = 'SEARCH RESULTS';
    }
    
    // Filter products
    const results = allProducts.filter(product => 
        product.name.toLowerCase().includes(searchTerm)
    );
    
    if (results.length === 0) {
        trendsGrid.innerHTML = '<p class="no-history">No products found</p>';
        return;
    }
    
    trendsGrid.innerHTML = results.map(product => `
        <div class="trend-item" onclick="viewProduct(${product.id})">
            <img src="${product.image}" alt="${product.name}" class="trend-image">
            <div class="trend-name">${product.name}</div>
        </div>
    `).join('');
}

// Initialize Search
function initSearch() {
    // Render initial content
    renderTrends();
    renderRecentlyViewed();
    
    // Setup search input listener
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            handleSearch(e.target.value);
        });
    }
    
    // Close on ESC key
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeSearch();
        }
    });
    
    // Close on overlay click
    const overlay = document.getElementById('searchOverlay');
    if (overlay) {
        overlay.addEventListener('click', (e) => {
            if (e.target.id === 'searchOverlay') {
                closeSearch();
            }
        });
    }
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initSearch);
} else {
    initSearch();

}
