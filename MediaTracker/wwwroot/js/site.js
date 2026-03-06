let url = window.location.href;
$('#navigation-bar a.nav-item').each(function () {
    if (this.href === url.split('?')[0]) {
        $(this).addClass('active');
    }
});

document.addEventListener("DOMContentLoaded", function () {
    setTimeout(function () {
        var alerts = document.querySelectorAll('.alert-dismissible');
        alerts.forEach(function (alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});

// Lazy Image Loading functionality
document.addEventListener("DOMContentLoaded", function () {
    const lazyImages = document.querySelectorAll("img.lazy-image");

    function loadLazyImage(img) {
        let realSrc = img.getAttribute("data-src");
        if (realSrc) {
            let tempImg = new Image();
            tempImg.onload = function () {
                img.src = realSrc;
                img.classList.remove("lazy-image");
                img.classList.add("lazy-image-loaded");
            };
            tempImg.onerror = function () {
                img.style.display = 'none';
            };
            tempImg.src = realSrc;
        }
    }

    if ("IntersectionObserver" in window) {
        let lazyImageObserver = new IntersectionObserver(function (entries, observer) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    loadLazyImage(entry.target);
                    lazyImageObserver.unobserve(entry.target);
                }
            });
        });

        lazyImages.forEach(function (lazyImage) {
            lazyImageObserver.observe(lazyImage);
        });
    } else {
        lazyImages.forEach(loadLazyImage);
    }
});