// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", function () {
    // Admin menu toggle logic
    const showAdminMenuBtn = document.getElementById("showAdminMenu");
    const backToStandardMenuBtn = document.getElementById("backToStandardMenu");
    const adminMenuContainer = document.getElementById("adminMenuContainer");
    const standardMenuItems = document.querySelectorAll(".standard-menu-item");
    const dernekGorevliCollapse = document.getElementById("dernekGorevliCollapse");

    if (showAdminMenuBtn && adminMenuContainer) {
        showAdminMenuBtn.addEventListener("click", function (e) {
            e.preventDefault();
            // Hide standard menu items
            standardMenuItems.forEach(item => item.classList.add("d-none"));
            // Collapse accordion if open
            if (dernekGorevliCollapse && dernekGorevliCollapse.classList.contains("show")) {
                let bsCollapse = bootstrap.Collapse.getInstance(dernekGorevliCollapse);
                if(bsCollapse) bsCollapse.hide();
            }
            // Show admin menu
            adminMenuContainer.classList.remove("d-none");
            // Hide the 'Admin' trigger item at bottom
            showAdminMenuBtn.closest('.mt-auto').classList.add("d-none");
        });
    }

    if (backToStandardMenuBtn && adminMenuContainer) {
        backToStandardMenuBtn.addEventListener("click", function (e) {
            e.preventDefault();
            // Hide admin menu
            adminMenuContainer.classList.add("d-none");
            // Show standard menu items
            standardMenuItems.forEach(item => item.classList.remove("d-none"));
            // Show the 'Admin' trigger item at bottom
            showAdminMenuBtn.closest('.mt-auto').classList.remove("d-none");
        });
    }
});
