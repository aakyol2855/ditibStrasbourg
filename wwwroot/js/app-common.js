/**
 * DITIB Strasbourg Platform - Common JavaScript
 * Handles global UI initialization, modals, and generic event delegation.
 */

$(document).ready(function () {
    // 1. Sidebar Toggle Logic
    $("#sidebarToggle, #sidebar-backdrop").click(function (e) {
        e.preventDefault();
        
        if (window.innerWidth < 992) {
            // Mobile: Toggle Overlay
            $("body").toggleClass("sidebar-mobile-open");
            $("#sidebar-backdrop").toggleClass("show");
        } else {
            // Desktop: Toggle Mini-Sidebar
            $("body").toggleClass("sidebar-collapsed");
            
            // Save state to localStorage for persistence (Desktop only)
            var isCollapsed = $("body").hasClass("sidebar-collapsed");
            localStorage.setItem("sidebarState", isCollapsed ? "collapsed" : "expanded");
        }
    });

    // Restore Sidebar state on load (Desktop only)
    if (window.innerWidth >= 992 && localStorage.getItem("sidebarState") === "collapsed") {
        $("body").addClass("sidebar-collapsed");
    }

    // 2. Global Select2 Initialization
    // Any select with class 'select2' will be automatically initialized
    function initSelect2() {
        if ($.fn.select2) {
            $('.select2').each(function () {
                $(this).select2({
                    theme: 'bootstrap-5',
                    width: '100%',
                    placeholder: $(this).data('placeholder') || 'Seçiniz...'
                });
            });
        }
    }
    initSelect2();

    // 3. Global Modal System (Event Delegation)
    // Add class 'open-modal' to any link/button and data-url to load it in the global modal
    $(document).on('click', '.open-modal', function (e) {
        e.preventDefault();
        var url = $(this).data('url') || $(this).attr('href');
        var modalTitle = $(this).data('modal-title') || 'İşlem';

        if (!url) return;

        // Show spinner inside modal container
        var loadingHtml = `
            <div class="modal fade" id="platformModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content border-0 shadow">
                        <div class="modal-header bg-light border-bottom-0">
                            <h5 class="modal-title fw-bold text-dark">${modalTitle}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body text-center py-5">
                            <div class="spinner-border text-primary" role="status">
                                <span class="visually-hidden">Yükleniyor...</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;

        $('#globalModalContainer').html(loadingHtml);
        var modalInstance = new bootstrap.Modal(document.getElementById('platformModal'));
        modalInstance.show();

        // Load content
        $.get(url, function (data) {
            // Check if the response contains a full HTML document (e.g. redirect to login)
            if (data.indexOf('<html') !== -1) {
                window.location.reload();
                return;
            }

            var contentHtml = `
                <div class="modal fade" id="platformModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content border-0 shadow">
                            <div class="modal-header bg-light border-bottom-0">
                                <h5 class="modal-title fw-bold text-dark">${modalTitle}</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                ${data}
                            </div>
                        </div>
                    </div>
                </div>`;
                
            // Update modal HTML while keeping it open
            $('#platformModal').replaceWith($(contentHtml).find('.modal-dialog').parent());
            
            // Re-initialize select2 inside modal
            initSelect2();
        }).fail(function () {
            $('#platformModal .modal-body').html('<div class="alert alert-danger m-0">İçerik yüklenirken bir hata oluştu. Lütfen tekrar deneyin.</div>');
        });
    });

    // 4. Global Delete Confirmation (Event Delegation)
    // Add class 'confirm-delete' to any form submit button
    $(document).on('click', '.confirm-delete', function (e) {
        var form = $(this).closest('form');
        var message = $(this).data('confirm-message') || "Bu kaydı silmek istediğinize emin misiniz? Bu işlem geri alınamaz.";
        
        if (!confirm(message)) {
            e.preventDefault();
            return false;
        }
        
        // Disable button to prevent double submit and show loading state
        var btn = $(this);
        btn.prop('disabled', true);
        btn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Siliniyor...');
        form.submit();
    });

    // 5. Global Form Submit Loading State
    // Add class 'btn-loading' to submit buttons
    $(document).on('submit', 'form:not(.no-loader)', function () {
        var btn = $(this).find('button[type="submit"]');
        if (btn.length > 0 && !btn.prop('disabled')) {
            var originalText = btn.html();
            btn.data('original-text', originalText);
            btn.prop('disabled', true);
            btn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Lütfen Bekleyin...');
        }
    });

    // 6. Automatic Table Responsiveness
    // Wrap any table that isn't already in a table-responsive div
    $('table.table').each(function() {
        if (!$(this).parent().hasClass('table-responsive')) {
            $(this).wrap('<div class="table-responsive"></div>');
        }
    });
});
