// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Toggle expandable search bar
function toggleSearchBar() {
    const expandableSearch = document.getElementById('expandableSearch');
    const searchToggleBtn = document.getElementById('searchToggleBtn');
    const searchInput = document.getElementById('searchInput');

    if (expandableSearch && searchToggleBtn) {
        const isHidden = expandableSearch.classList.contains('hidden');

        if (isHidden) {
            expandableSearch.classList.remove('hidden');
            searchToggleBtn.classList.add('hidden');
            if (searchInput) {
                searchInput.focus();
            }
        } else {
            expandableSearch.classList.add('hidden');
            searchToggleBtn.classList.remove('hidden');
        }
    }
}

// Close search when clicking outside
document.addEventListener('click', function (event) {
    const searchContainer = document.getElementById('searchContainer');
    const expandableSearch = document.getElementById('expandableSearch');
    const searchToggleBtn = document.getElementById('searchToggleBtn');

    if (searchContainer && expandableSearch && searchToggleBtn) {
        if (!searchContainer.contains(event.target) && !expandableSearch.classList.contains('hidden')) {
            expandableSearch.classList.add('hidden');
            searchToggleBtn.classList.remove('hidden');
        }
    }
});

// Toggle mobile menu
function toggleMobileMenu() {
    const mobileMenu = document.getElementById('mobileMenu');
    if (mobileMenu) {
        mobileMenu.classList.toggle('hidden');
    }
}

// Toggle user dropdown menu
function toggleUserMenu() {
    const userMenu = document.getElementById('userMenu');
    if (userMenu) {
        userMenu.classList.toggle('hidden');
    }
}

// Close user menu when clicking outside
document.addEventListener('click', function (event) {
    const userMenuContainer = document.getElementById('userMenuContainer');
    const userMenu = document.getElementById('userMenu');

    if (userMenuContainer && userMenu) {
        if (!userMenuContainer.contains(event.target)) {
            userMenu.classList.add('hidden');
        }
    }
});

// Toggle mobile search overlay
function toggleMobileSearch() {
    const mobileSearchOverlay = document.getElementById('mobileSearchOverlay');
    const mobileSearchInput = document.getElementById('mobileSearchInput');

    if (mobileSearchOverlay) {
        if (mobileSearchOverlay.classList.contains('hidden')) {
            mobileSearchOverlay.classList.remove('hidden');
            mobileSearchOverlay.classList.add('flex');
            if (mobileSearchInput) {
                mobileSearchInput.focus();
            }
        } else {
            mobileSearchOverlay.classList.add('hidden');
            mobileSearchOverlay.classList.remove('flex');
        }
    }
}

// Close mobile search overlay
document.addEventListener('DOMContentLoaded', function () {
    const closeMobileSearch = document.getElementById('closeMobileSearch');
    const mobileSearchInput = document.getElementById('mobileSearchInput');

    if (closeMobileSearch) {
        closeMobileSearch.addEventListener('click', toggleMobileSearch);
    }

    // Submit mobile search on Enter
    if (mobileSearchInput) {
        mobileSearchInput.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                window.location.href = '/Products?search=' + encodeURIComponent(mobileSearchInput.value);
            }
        });
    }
});
