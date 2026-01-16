// Coffee Shop - Search functionality

(function() {
    'use strict';

    let searchTimeout = null;
    const DEBOUNCE_DELAY = 300;

    // Initialize search when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        initSearchBox();
        initMobileSearch();
    });

    function initSearchBox() {
        const searchInput = document.getElementById('searchInput');
        const searchDropdown = document.getElementById('searchDropdown');
        const searchForm = document.getElementById('searchForm');

        if (!searchInput || !searchDropdown) return;

        // Handle input with debounce
        searchInput.addEventListener('input', function(e) {
            const query = e.target.value.trim();
            
            if (searchTimeout) {
                clearTimeout(searchTimeout);
            }

            if (query.length < 2) {
                hideDropdown();
                return;
            }

            showLoading();
            
            searchTimeout = setTimeout(function() {
                performSearch(query);
            }, DEBOUNCE_DELAY);
        });

        // Handle focus
        searchInput.addEventListener('focus', function() {
            if (searchInput.value.trim().length >= 2) {
                showDropdown();
            }
        });

        // Handle form submit
        if (searchForm) {
            searchForm.addEventListener('submit', function(e) {
                const query = searchInput.value.trim();
                if (query.length > 0) {
                    window.location.href = '/Products?search=' + encodeURIComponent(query);
                }
                e.preventDefault();
            });
        }

        // Close dropdown when clicking outside
        document.addEventListener('click', function(e) {
            if (!e.target.closest('#searchContainer')) {
                hideDropdown();
            }
        });

        // Keyboard navigation
        searchInput.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                hideDropdown();
                searchInput.blur();
            }
        });
    }

    function initMobileSearch() {
        const mobileSearchBtn = document.getElementById('mobileSearchBtn');
        const mobileSearchOverlay = document.getElementById('mobileSearchOverlay');
        const closeMobileSearch = document.getElementById('closeMobileSearch');
        const mobileSearchInput = document.getElementById('mobileSearchInput');

        if (mobileSearchBtn && mobileSearchOverlay) {
            mobileSearchBtn.addEventListener('click', function() {
                mobileSearchOverlay.classList.remove('hidden');
                mobileSearchOverlay.classList.add('flex');
                setTimeout(() => mobileSearchInput.focus(), 100);
            });

            closeMobileSearch.addEventListener('click', function() {
                mobileSearchOverlay.classList.add('hidden');
                mobileSearchOverlay.classList.remove('flex');
            });

            mobileSearchInput.addEventListener('keydown', function(e) {
                if (e.key === 'Enter') {
                    const query = mobileSearchInput.value.trim();
                    if (query.length > 0) {
                        window.location.href = '/Products?search=' + encodeURIComponent(query);
                    }
                    e.preventDefault();
                }
            });
        }
    }

    function performSearch(query) {
        fetch('/Products/Search?q=' + encodeURIComponent(query))
            .then(response => response.json())
            .then(data => {
                renderResults(data);
            })
            .catch(error => {
                console.error('Search error:', error);
                hideDropdown();
            });
    }

    function renderResults(results) {
        const searchDropdown = document.getElementById('searchDropdown');
        const searchResults = document.getElementById('searchResults');
        
        if (!searchDropdown || !searchResults) return;

        if (results.length === 0) {
            searchResults.innerHTML = `
                <div class="p-4 text-center text-gray-500 dark:text-gray-400">
                    <span class="text-3xl mb-2 block">🔍</span>
                    Không tìm thấy sản phẩm
                </div>
            `;
        } else {
            searchResults.innerHTML = results.map(product => `
                <a href="/Products/Details/${product.id}" 
                   class="flex items-center gap-3 p-3 hover:bg-amber-50 dark:hover:bg-gray-700 transition-colors">
                    <div class="w-12 h-12 flex-shrink-0 bg-gradient-to-br from-amber-100 to-orange-100 dark:from-amber-900/20 dark:to-orange-900/20 rounded-lg flex items-center justify-center">
                        <span class="text-xl">☕</span>
                    </div>
                    <div class="flex-1 min-w-0">
                        <p class="font-semibold text-gray-900 dark:text-white truncate">${product.name}</p>
                        <p class="text-amber-600 dark:text-amber-400 font-medium">${product.priceFormatted}</p>
                    </div>
                </a>
            `).join('');
        }

        showDropdown();
    }

    function showLoading() {
        const searchResults = document.getElementById('searchResults');
        if (searchResults) {
            searchResults.innerHTML = `
                <div class="p-4 text-center">
                    <div class="inline-block w-6 h-6 border-2 border-amber-600 border-t-transparent rounded-full animate-spin"></div>
                </div>
            `;
        }
        showDropdown();
    }

    function showDropdown() {
        const searchDropdown = document.getElementById('searchDropdown');
        if (searchDropdown) {
            searchDropdown.classList.remove('hidden', 'opacity-0');
            searchDropdown.classList.add('opacity-100');
        }
    }

    function hideDropdown() {
        const searchDropdown = document.getElementById('searchDropdown');
        if (searchDropdown) {
            searchDropdown.classList.add('hidden', 'opacity-0');
            searchDropdown.classList.remove('opacity-100');
        }
    }

})();
