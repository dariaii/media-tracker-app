let url = window.location.href;
$('#navigation-bar a.nav-item').each(function () {
    if (this.href === url.split('?')[0]) {
        $(this).addClass('active');
    }
});