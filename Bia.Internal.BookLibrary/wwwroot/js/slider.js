$(document).ready(function () {
    var swiper = new Swiper(".slider", {
        slidesPerView: "auto",
        spaceBetween: 16,
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
        autoplay: {
            delay: 5000,
            disableOnInteraction: false,
        },
    });
});
